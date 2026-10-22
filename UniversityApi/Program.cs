using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.Mapping;
using UniversityApi.Middleware;
using UniversityApi.Models;
using UniversityApi.Repositories.Implementations;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Implementations;
using UniversityApi.Services.Interfaces;
using UniversityApi.Swagger;

const string CorsPolicyName = "ClientApps";

var builder = WebApplication.CreateBuilder(args);

// ---------------- Database ----------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile));

// ---------------- JWT настройкалары (кілт — user-secrets / environment, Git-те емес) ----------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is not configured (min 32 bytes). Set it outside the source code: " +
        "dotnet user-secrets set \"Jwt:Key\" \"<random string, 32+ chars>\"");
}
builder.Services.Configure<JwtOptions>(jwtSection);

// ---------------- ASP.NET Core Identity ----------------
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // пароль күрделілігі
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // бірегей username және email
        options.User.RequireUniqueEmail = true;

        // бірнеше сәтсіз кіру әрекетінен кейін бұғаттау
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

// ---------------- Authentication (JWT Bearer) ----------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // claim атаулары токендегідей қалады: sub, name, role
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.Zero, // мерзімі өткен токен бірден қабылданбайды
            NameClaimType = AppClaims.Name,
            RoleClaimType = AppClaims.Role
        };

        options.Events = new JwtBearerEvents
        {
            // 401: токен жоқ / жарамсыз / мерзімі өткен -> бірыңғай ReturnResult форматы
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var (code, message) = context.AuthenticateFailure switch
                {
                    null => ("UNAUTHORIZED", "Authentication is required"),
                    SecurityTokenExpiredException => ("TOKEN_EXPIRED", "Access token has expired"),
                    _ => ("INVALID_TOKEN", "Access token is invalid")
                };
                await ApiResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, code, message);
            },
            // 403: аутентификациядан өткен, бірақ құқығы жоқ
            OnForbidden = context => ApiResponseWriter.WriteAsync(context.HttpContext,
                StatusCodes.Status403Forbidden, "FORBIDDEN", "You do not have permission to perform this action")
        };
    });

// ---------------- Authorization (рөлдер және саясаттар) ----------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.AdminOnly, p => p.RequireAuthenticatedUser().RequireRole(AppRoles.Admin));
    options.AddPolicy(AppPolicies.ManageGrades, p => p.RequireAuthenticatedUser().RequireRole(AppRoles.Admin, AppRoles.Teacher));
    options.AddPolicy(AppPolicies.StudentOrAdmin, p => p.RequireAuthenticatedUser().RequireRole(AppRoles.Admin, AppRoles.Student));
});

// ---------------- CORS (аталған саясат, нақты екі клиент мекенжайы) ----------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy => policy
        .WithOrigins(allowedOrigins)
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders("Authorization", "Content-Type", "Accept"));
});

// ---------------- API Versioning ----------------
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true; // /api/courses = v1 (1-тапсырма URL-дері жұмыс істейді)
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// ---------------- Repositories, Services (Scoped) ----------------
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// ---------------- Controllers + валидация қатесін ReturnResult форматына келтіру ----------------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors
                        .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid value" : x.ErrorMessage)
                        .ToArray());

            var result = ReturnResult<object>.Failure(
                StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR",
                "One or more validation errors occurred",
                context.HttpContext.TraceIdentifier,
                errors);

            return new BadRequestObjectResult(result);
        };
    });

// ---------------- Swagger / OpenAPI (v1 + v2, JWT Bearer) ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(c =>
{
    // Әр нұсқа өз құжатына түседі; 1-тапсырманың нұсқасыз ескі URL-дері (api/students ...) құжатта қайталанбайды
    c.DocInclusionPredicate((docName, api) =>
    {
        var path = api.RelativePath ?? string.Empty;
        var isLegacy = System.Text.RegularExpressions.Regex.IsMatch(
            path, @"^api/(students|teachers|courses|enrollments)(/|$)");
        return !isLegacy && (api.GroupName is null || api.GroupName == docName);
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "POST /api/auth/login арқылы алынған JWT-ті мұнда енгізіңіз ('Bearer' сөзінсіз)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------- DB: миграциялар + тест деректері + рөлдер/пайдаланушылар ----------------
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();

    if (!db.Database.GetMigrations().Any())
    {
        throw new InvalidOperationException(
            "No EF Core migrations found. Create the initial migration first: " +
            "dotnet ef migrations add InitialCreate");
    }

    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
    await IdentitySeeder.SeedAsync(sp, app.Configuration,
        sp.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder"));
}

// ---------------- Middleware pipeline (реті маңызды) ----------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

var versionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    foreach (var description in versionProvider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            $"University API {description.GroupName.ToUpperInvariant()}");
    }
});

app.UseCors(CorsPolicyName);   // 1) CORS
app.UseAuthentication();       // 2) кім екенін анықтау (JWT -> ClaimsPrincipal)
app.UseAuthorization();        // 3) не істей алатынын тексеру ([Authorize], policy)

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
