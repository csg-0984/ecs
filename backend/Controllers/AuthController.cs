using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly IConfiguration _config;

	public AuthController(AppDbContext db, IConfiguration config)
	{
		_db = db;
		_config = config;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		if (await _db.Users.AnyAsync(u => u.Email == request.Email))
		{
			return Conflict(new { message = "Email already registered" });
		}

		var user = new AppUser
		{
			Id = Guid.NewGuid(),
			Email = request.Email.Trim().ToLowerInvariant(),
			Name = request.Name.Trim(),
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
		};

		_db.Users.Add(user);
		await _db.SaveChangesAsync();
		return Ok(new { message = "Registered" });
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
		if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
		{
			return Unauthorized(new { message = "Invalid credentials" });
		}

		var token = GenerateJwt(user);
		return Ok(new { token, user = new { user.Id, user.Email, user.Name, user.Role } });
	}

	[HttpGet("me")]
	public async Task<IActionResult> Me()
	{
		var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (sub is null) return Unauthorized();
		if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
		var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
		if (user is null) return Unauthorized();
		return Ok(new { user.Id, user.Email, user.Name, user.Role });
	}

	#if DEBUG
	[HttpPost("seed-demo")]
	public async Task<IActionResult> SeedDemo()
	{
		if (!await _db.Users.AnyAsync())
		{
			_db.Users.AddRange(
				new AppUser { Id = Guid.NewGuid(), Email = "admin@example.com", Name = "Admin", Role = "Admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123") },
				new AppUser { Id = Guid.NewGuid(), Email = "dispatcher@example.com", Name = "Dispatcher", Role = "Dispatcher", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123") },
				new AppUser { Id = Guid.NewGuid(), Email = "tech@example.com", Name = "Tech", Role = "Technician", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123") }
			);
			await _db.SaveChangesAsync();
		}
		return Ok(new { message = "Seeded" });
	}
	#endif

	private string GenerateJwt(AppUser user)
	{
		var jwtSection = _config.GetSection("Jwt");
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var issuer = jwtSection["Issuer"];
		var audience = jwtSection["Audience"];

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(JwtRegisteredClaimNames.Email, user.Email),
			new(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new(ClaimTypes.Name, user.Name),
			new(ClaimTypes.Role, user.Role)
		};

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			expires: DateTime.UtcNow.AddHours(8),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}

public sealed class RegisterRequest
{
	public string Email { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
	public RegisterRequestValidator()
	{
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
		RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
	}
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
	public LoginRequestValidator()
	{
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Password).NotEmpty();
	}
}