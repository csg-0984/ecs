using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
	private readonly AppDbContext _db;
	public CustomersController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
	{
		var list = await _db.Customers.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
		return Ok(list);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Customer>> GetById(Guid id)
	{
		var entity = await _db.Customers.Include(c => c.Appliances).FirstOrDefaultAsync(c => c.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	public async Task<ActionResult<Customer>> Create([FromBody] Customer body)
	{
		body.Id = Guid.NewGuid();
		body.CreatedAt = DateTime.UtcNow;
		_db.Customers.Add(body);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = body.Id }, body);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] Customer body)
	{
		var entity = await _db.Customers.FindAsync(id);
		if (entity is null) return NotFound();
		entity.Name = body.Name;
		entity.Phone = body.Phone;
		entity.Email = body.Email;
		entity.Address = body.Address;
		entity.City = body.City;
		entity.Province = body.Province;
		entity.PostalCode = body.PostalCode;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Customers.FindAsync(id);
		if (entity is null) return NotFound();
		_db.Customers.Remove(entity);
		await _db.SaveChangesAsync();
		return NoContent();
	}
}