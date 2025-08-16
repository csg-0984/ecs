using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class WorkOrdersController : ControllerBase
{
	private readonly AppDbContext _db;
	public WorkOrdersController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<WorkOrder>>> GetAll()
	{
		var list = await _db.WorkOrders
			.Include(w => w.Customer)
			.Include(w => w.Appliance)
			.Include(w => w.AssignedTechnician)
			.AsNoTracking()
			.OrderByDescending(w => w.CreatedAt)
			.ToListAsync();
		return Ok(list);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<WorkOrder>> GetById(Guid id)
	{
		var entity = await _db.WorkOrders
			.Include(w => w.Customer)
			.Include(w => w.Appliance)
			.Include(w => w.AssignedTechnician)
			.FirstOrDefaultAsync(w => w.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	public async Task<ActionResult<WorkOrder>> Create([FromBody] WorkOrder body)
	{
		body.Id = Guid.NewGuid();
		body.CreatedAt = DateTime.UtcNow;
		_db.WorkOrders.Add(body);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] WorkOrder body)
	{
		var entity = await _db.WorkOrders.FindAsync(id);
		if (entity is null) return NotFound();
		entity.Title = body.Title;
		entity.Description = body.Description;
		entity.Status = body.Status;
		entity.Priority = body.Priority;
		entity.ScheduledAt = body.ScheduledAt;
		entity.CompletedAt = body.CompletedAt;
		entity.AssignedTechnicianId = body.AssignedTechnicianId;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPost("{id}/assign/{technicianId}")]
	public async Task<IActionResult> Assign(Guid id, Guid technicianId)
	{
		var entity = await _db.WorkOrders.FindAsync(id);
		var tech = await _db.Technicians.FindAsync(technicianId);
		if (entity is null || tech is null) return NotFound();
		entity.AssignedTechnicianId = technicianId;
		entity.Status = WorkOrderStatus.Assigned;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.WorkOrders.FindAsync(id);
		if (entity is null) return NotFound();
		_db.WorkOrders.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}