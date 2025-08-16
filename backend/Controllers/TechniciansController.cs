using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
	public async Task<ActionResult<Technician>> Create([FromBody] Technician body)
	{
		body.Id = Guid.NewGuid();
		_db.Technicians.Add(body);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] Technician body)
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
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Technicians.FindAsync(id);
		if (entity is null) return NotFound();
		_db.Technicians.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}