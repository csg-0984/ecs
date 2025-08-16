using KitchenAfterSales.Api.Data;
using KitchenAfterSales.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace KitchenAfterSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
	private readonly AppDbContext _db;
	public CustomersController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] bool requiresCounts = false, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = null)
	{
		var query = _db.Customers.AsNoTracking().AsQueryable();
		if (!string.IsNullOrWhiteSpace(search))
		{
			var term = search.Trim();
			query = query.Where(c => c.Name.Contains(term) || c.Phone.Contains(term) || c.Email.Contains(term));
		}
		if (!string.IsNullOrWhiteSpace(sortBy))
		{
			var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
			switch (sortBy)
			{
				case nameof(Customer.Name): query = desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name); break;
				case nameof(Customer.CreatedAt): query = desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt); break;
				default: query = query.OrderByDescending(c => c.CreatedAt); break;
			}
		}
		else
		{
			query = query.OrderByDescending(c => c.CreatedAt);
		}
		var total = await query.CountAsync();
		var effectiveSkip = skip ?? Math.Max(0, (page - 1) * pageSize);
		var effectiveTake = take ?? pageSize;
		var items = await query.Skip(effectiveSkip).Take(effectiveTake).ToListAsync();
		if (requiresCounts || Request.Query.ContainsKey("requiresCounts"))
		{
			return Ok(new { result = items, count = total });
		}
		return Ok(items);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Customer>> GetById(Guid id)
	{
		var entity = await _db.Customers.Include(c => c.Appliances).FirstOrDefaultAsync(c => c.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<ActionResult<Customer>> Create([FromBody] CustomerCreateDto body)
	{
		var entity = new Customer
		{
			Id = Guid.NewGuid(),
			Name = body.Name,
			Phone = body.Phone,
			Email = body.Email,
			Address = body.Address,
			City = body.City,
			Province = body.Province,
			PostalCode = body.PostalCode,
			CreatedAt = DateTime.UtcNow
		};
		_db.Customers.Add(entity);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateDto body)
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
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Customers.FindAsync(id);
		if (entity is null) return NotFound();
		entity.IsDeleted = true;
		await _db.SaveChangesAsync();
		return NoContent();
	}
	
	[HttpPost("{id}/warranty")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	[RequestSizeLimit(20_000_000)]
	public async Task<IActionResult> UploadWarranty(Guid id, IFormFile file)
	{
		var entity = await _db.Customers.FindAsync(id);
		if (entity is null) return NotFound();
		var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
		var root = Path.Combine(env.ContentRootPath, "uploads", "customer-warranty", id.ToString());
		Directory.CreateDirectory(root);
		var ext = Path.GetExtension(file.FileName);
		var filename = $"warranty{ext}";
		var path = Path.Combine(root, filename);
		using var stream = System.IO.File.Create(path);
		await file.CopyToAsync(stream);
		return Ok(new { url = $"/uploads/customer-warranty/{id}/{filename}" });
	}
}

public sealed class CustomerCreateDto
{
	public string Name { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string Province { get; set; } = string.Empty;
	public string PostalCode { get; set; } = string.Empty;
}

public sealed class CustomerUpdateDto : CustomerCreateDto {}

public sealed class CustomerCreateDtoValidator : FluentValidation.AbstractValidator<CustomerCreateDto>
{
	public CustomerCreateDtoValidator()
	{
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Phone).NotEmpty();
	}
}

public sealed class CustomerUpdateDtoValidator : FluentValidation.AbstractValidator<CustomerUpdateDto>
{
	public CustomerUpdateDtoValidator()
	{
		Include(new CustomerCreateDtoValidator());
	}
}