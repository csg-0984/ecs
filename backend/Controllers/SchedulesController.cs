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
public sealed class SchedulesController : ControllerBase
{
	private readonly AppDbContext _db;
	public SchedulesController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Schedule>>> GetAll([FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
	{
		var query = _db.Schedules
			.Include(s => s.Technician)
			.Include(s => s.WorkOrder)
			.AsNoTracking()
			.AsQueryable();
		if (start.HasValue) query = query.Where(s => s.EndTime >= start.Value);
		if (end.HasValue) query = query.Where(s => s.StartTime <= end.Value);
		var list = await query.OrderBy(s => s.StartTime).ToListAsync();
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
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<ActionResult<Schedule>> Create([FromBody] ScheduleCreateDto body)
	{
		var entity = new Schedule
		{
			Id = Guid.NewGuid(),
			TechnicianId = body.TechnicianId,
			WorkOrderId = body.WorkOrderId,
			StartTime = body.StartTime,
			EndTime = body.EndTime,
			Status = body.Status
		};
		_db.Schedules.Add(entity);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<IActionResult> Update(Guid id, [FromBody] ScheduleUpdateDto body)
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
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Schedules.FindAsync(id);
		if (entity is null) return NotFound();
		_db.Schedules.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}

public sealed class ScheduleCreateDto
{
	public Guid TechnicianId { get; set; }
	public Guid WorkOrderId { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;
}

public sealed class ScheduleUpdateDto : ScheduleCreateDto { }

public sealed class ScheduleCreateDtoValidator : AbstractValidator<ScheduleCreateDto>
{
	public ScheduleCreateDtoValidator()
	{
		RuleFor(x => x.TechnicianId).NotEmpty();
		RuleFor(x => x.WorkOrderId).NotEmpty();
		RuleFor(x => x.StartTime).NotEmpty();
		RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
	}
}

public sealed class ScheduleUpdateDtoValidator : AbstractValidator<ScheduleUpdateDto>
{
	public ScheduleUpdateDtoValidator()
	{
		Include(new ScheduleCreateDtoValidator());
	}
}