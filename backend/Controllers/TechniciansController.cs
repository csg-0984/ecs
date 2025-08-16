using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TechniciansController : ControllerBase
{
	private readonly AppDbContext _db;
	public TechniciansController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Technician>>> GetAll()
	{
		var list = await _db.Technicians.AsNoTracking().ToListAsync();
		return Ok(list);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Technician>> GetById(Guid id)
	{
		var entity = await _db.Technicians.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	[Authorize(Policy = "AdminOnly")]
	public async Task<ActionResult<Technician>> Create([FromBody] TechnicianCreateDto body)
	{
		var entity = new Technician
		{
			Id = Guid.NewGuid(),
			Name = body.Name,
			Phone = body.Phone,
			Email = body.Email,
			Skills = body.Skills,
			IsActive = body.IsActive
		};
		_db.Technicians.Add(entity);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Update(Guid id, [FromBody] TechnicianUpdateDto body)
	{
		var entity = await _db.Technicians.FindAsync(id);
		if (entity is null) return NotFound();
		entity.Name = body.Name;
		entity.Phone = body.Phone;
		entity.Email = body.Email;
		entity.Skills = body.Skills;
		entity.IsActive = body.IsActive;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Technicians.FindAsync(id);
		if (entity is null) return NotFound();
		_db.Technicians.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}

public sealed class TechnicianCreateDto
{
	public string Name { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Skills { get; set; } = string.Empty;
	public bool IsActive { get; set; } = true;
}

public sealed class TechnicianUpdateDto : TechnicianCreateDto {}

public sealed class TechnicianCreateDtoValidator : AbstractValidator<TechnicianCreateDto>
{
	public TechnicianCreateDtoValidator()
	{
		RuleFor(x => x.Name).NotEmpty();
		RuleFor(x => x.Phone).NotEmpty();
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
	}
}

public sealed class TechnicianUpdateDtoValidator : AbstractValidator<TechnicianUpdateDto>
{
	public TechnicianUpdateDtoValidator()
	{
		Include(new TechnicianCreateDtoValidator());
	}
}