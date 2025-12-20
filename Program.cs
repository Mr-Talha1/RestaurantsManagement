//using BIPL_RAASTP2M.Data;
//using BIPL_RAASTP2M.Repositories;
//using BIPL_RAASTP2M.Security;
//using BIPL_RAASTP2M.Services;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;




//var builder = WebApplication.CreateBuilder(args);


//var secretKey = builder.Configuration["JwtIssuerOptions:SecretKey"];
//var _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Add database context
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Register your services and repositories
//builder.Services.AddScoped<ICoreRepository, CoreRepository>();
//builder.Services.AddScoped<ICoreService, CoreService>();


//// If you have a custom ServiceProvider, make sure it's registered with the correct interface
//builder.Services.AddScoped<IServiceProvider, ServiceProvider>();
//// Add services to the container.
//builder.Services.AddSingleton<TokenBlacklistService>();

//builder.Services.AddSingleton<IJwtFactory, JwtFactory>();

//// jwt wire up
//// Get options from app settings
//var jwtAppSettingOptions = builder.Configuration.GetSection(nameof(JwtIssuerOptions));

//builder.Services.Configure<JwtIssuerOptions>(options =>
//{
//    options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
//    options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
//    options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
//});
//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

using BIPL_RAASTP2M.Data;
using BIPL_RAASTP2M.Repositories;
using BIPL_RAASTP2M.Security;
using BIPL_RAASTP2M.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Secret Key for JWT Signing
var secretKey = builder.Configuration["JwtIssuerOptions:SecretKey"];
var _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger with JWT Authentication support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BIPL_RAASTP2M API", Version = "v1" });

    // Add security definition for JWT
    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securitySchema);

    // Add security requirement
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    };
    c.AddSecurityRequirement(securityRequirement);
});

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("KuickpayDB"), options =>
   {
       options.CommandTimeout(180); // 3 minutes
   }));

// Add database context

//builder.Services.AddDbContext<KuickpayNewDB>(options =>
//   options.UseSqlServer(builder.Configuration.GetConnectionString("KuickpayNewDB"), options =>
//   {
//       options.CommandTimeout(180); // 3 minutes
//   }));


// Register your services and repositories
builder.Services.AddScoped<ICoreRepository, CoreRepository>();
builder.Services.AddScoped<ICoreService, CoreService>();

// If you have a custom ServiceProvider, make sure it's registered with the correct interface
builder.Services.AddScoped<IServiceProvider, ServiceProvider>();
//Cloudinary
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Add singleton services
builder.Services.AddSingleton<TokenBlacklistService>();
builder.Services.AddSingleton<IJwtFactory, JwtFactory>();

// Configure JWT options
var jwtAppSettingOptions = builder.Configuration.GetSection(nameof(JwtIssuerOptions));
builder.Services.Configure<JwtIssuerOptions>(options =>
{
    options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
    options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
    options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
});

builder.Services.AddCors(o => {
    o.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
// Configure Authentication using JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

        ValidateAudience = true,
        ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = _signingKey,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});
builder.Services.AddHttpClient();
// Add HttpClient to the DI container
builder.Services.AddHttpClient<CoreService>();

//              var inMemorySettings = new Dictionary<string, string>
//             {
//                    { "CMHCredentials:AccessToken", "actualAccessToken" },
//                    { "CMHCredentials:CsrfToken", "actualCsrfToken" },
//                    { "CMHCredentials:csrfTokenForCookies", "csrfTokenForCookies" }

//                  };



//// Add existing configuration (e.g., appsettings.json)
//builder.Configuration.AddJsonFile("appsettings.json");

//// Add In-Memory Configuration
//builder.Configuration.AddInMemoryCollection(inMemorySettings);




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BIPL_RAASTP2M API V1");
    });
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
