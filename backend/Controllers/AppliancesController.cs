using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AppliancesController : ControllerBase
{
	private readonly AppDbContext _db;
	public AppliancesController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Appliance>>> GetAll()
	{
		var list = await _db.Appliances.AsNoTracking().ToListAsync();
		return Ok(list);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Appliance>> GetById(Guid id)
	{
		var entity = await _db.Appliances.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	public async Task<ActionResult<Appliance>> Create([FromBody] Appliance body)
	{
		body.Id = Guid.NewGuid();
		_db.Appliances.Add(body);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] Appliance body)
	{
		var entity = await _db.Appliances.FindAsync(id);
		if (entity is null) return NotFound();
		entity.Brand = body.Brand;
		entity.Model = body.Model;
		entity.SerialNumber = body.SerialNumber;
		entity.PurchaseDate = body.PurchaseDate;
		entity.WarrantyExpiryDate = body.WarrantyExpiryDate;
		entity.Notes = body.Notes;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Appliances.FindAsync(id);
		if (entity is null) return NotFound();
		_db.Appliances.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}