using Microsoft.OpenApi.Models;
using Saga.DomainShared.Interfaces;
using Saga.Persistence;
using Saga.WebApi.Infrastructures.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

//builder.Services.AddControllers(); 

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "WEB API SAGA", 
        Version = "v1",
        Description = "API untuk aplikasi Mobile SAGA", 
        License = new OpenApiLicense
        {
            Name = "SAGA WEB",
            Url = new Uri("http://portal.sidoagunggroup.com/")
        }
    });
});


builder.Services.AddPersistence(
    builder.Configuration
);

builder.Services.AddTransient<ICurrentUser, CurrentUserService>();
builder.Services.AddTransient<IRepositoryService, RepositoryService>();

builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Middleware
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

//var webSocketOptions = new WebSocketOptions
//{
//    KeepAliveInterval = TimeSpan.FromMinutes(30)
//};
//webSocketOptions.AllowedOrigins.Add("https://mitra.sidoagunggroup.com");

//app.UseWebSockets();

app.MapControllers();

app.Run();
