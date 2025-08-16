using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddFluentValidation(fv =>
{
	fv.RegisterValidatorsFromAssemblyContaining<Program>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

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

// Authorization policies by role
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("DispatcherOrAdmin", policy => policy.RequireRole("Dispatcher", "Admin"));
	options.AddPolicy("TechnicianOrAdmin", policy => policy.RequireRole("Technician", "Admin"));
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

// Serve static files for uploads (e.g., /uploads/*)
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "uploads")),
	RequestPath = "/uploads"
});

app.MapControllers();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<KitchenAfterSales.Api.Data.AppDbContext>();
	db.Database.Migrate();
}

app.Run();
