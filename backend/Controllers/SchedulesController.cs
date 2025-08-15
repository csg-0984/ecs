using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SchedulesController : ControllerBase
{
	private readonly AppDbContext _db;
	public SchedulesController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Schedule>>> GetAll()
	{
		var list = await _db.Schedules
			.Include(s => s.Technician)
			.Include(s => s.WorkOrder)
			.AsNoTracking()
			.OrderBy(s => s.StartTime)
			.ToListAsync();
		return Ok(list);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Schedule>> GetById(Guid id)
	{
		var entity = await _db.Schedules
			.Include(s => s.Technician)
			.Include(s => s.WorkOrder)
			.AsNoTracking()
			.FirstOrDefaultAsync(s => s.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	public async Task<ActionResult<Schedule>> Create([FromBody] Schedule body)
	{
		body.Id = Guid.NewGuid();
		_db.Schedules.Add(body);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] Schedule body)
	{
		var entity = await _db.Schedules.FindAsync(id);
		if (entity is null) return NotFound();
		entity.TechnicianId = body.TechnicianId;
		entity.WorkOrderId = body.WorkOrderId;
		entity.StartTime = body.StartTime;
		entity.EndTime = body.EndTime;
		entity.Status = body.Status;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Schedules.FindAsync(id);
		if (entity is null) return NotFound();
		_db.Schedules.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}