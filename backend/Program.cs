using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular dev server
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
	options.AddPolicy("AppCors", policy =>
	{
		policy.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

// DbContext (SQLite)
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=./app.db";
builder.Services.AddDbContext<KitchenAfterSales.Api.Data.AppDbContext>(options =>
{
	options.UseSqlite(connectionString);
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Auth: JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? "dev-secret-change-me";
var issuer = jwtSection.GetValue<string>("Issuer") ?? "KitchenAfterSales";
var audience = jwtSection.GetValue<string>("Audience") ?? "KitchenAfterSalesFrontend";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
	.AddAuthentication(options =>
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
			IssuerSigningKey = signingKey,
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidIssuer = issuer,
			ValidAudience = audience,
			ClockSkew = TimeSpan.FromMinutes(2)
		};
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AppCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<KitchenAfterSales.Api.Data.AppDbContext>();
	db.Database.Migrate();
}

app.Run();
