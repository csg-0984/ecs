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
public sealed class AppliancesController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly IWebHostEnvironment _env;
	public AppliancesController(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

	[HttpGet]
	public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] bool requiresCounts = false, [FromQuery] string? search = null)
	{
		var query = _db.Appliances.AsNoTracking().AsQueryable();
		if (!string.IsNullOrWhiteSpace(search))
		{
			var term = search.Trim();
			query = query.Where(a => a.Brand.Contains(term) || a.Model.Contains(term) || a.SerialNumber.Contains(term));
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
	public async Task<ActionResult<Appliance>> GetById(Guid id)
	{
		var entity = await _db.Appliances.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
		return entity is null ? NotFound() : Ok(entity);
	}

	[HttpPost]
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<ActionResult<Appliance>> Create([FromBody] ApplianceCreateDto body)
	{
		var entity = new Appliance
		{
			Id = Guid.NewGuid(),
			CustomerId = body.CustomerId,
			Brand = body.Brand,
			Model = body.Model,
			SerialNumber = body.SerialNumber,
			PurchaseDate = body.PurchaseDate,
			WarrantyExpiryDate = body.WarrantyExpiryDate,
			Notes = body.Notes
		};
		_db.Appliances.Add(entity);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<IActionResult> Update(Guid id, [FromBody] ApplianceUpdateDto body)
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
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.Appliances.FindAsync(id);
		if (entity is null) return NotFound();
		entity.IsDeleted = true;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPost("{id}/images")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	[RequestSizeLimit(20_000_000)]
	public async Task<IActionResult> UploadImages(Guid id, List<IFormFile> files)
	{
		var entity = await _db.Appliances.FindAsync(id);
		if (entity is null) return NotFound();
		var uploadRoot = Path.Combine(_env.ContentRootPath, "uploads", "appliances", id.ToString());
		Directory.CreateDirectory(uploadRoot);
		foreach (var file in files)
		{
			if (file.Length <= 0) continue;
			var ext = Path.GetExtension(file.FileName);
			var fileName = $"{Guid.NewGuid()}{ext}";
			var path = Path.Combine(uploadRoot, fileName);
			using var stream = System.IO.File.Create(path);
			await file.CopyToAsync(stream);
		}
		return Ok();
	}

	[HttpGet("{id}/images")]
	public IActionResult ListImages(Guid id)
	{
		var uploadRoot = Path.Combine(_env.ContentRootPath, "uploads", "appliances", id.ToString());
		if (!Directory.Exists(uploadRoot)) return Ok(Array.Empty<string>());
		var urls = Directory.GetFiles(uploadRoot)
			.Select(f => $"/uploads/appliances/{id}/{Path.GetFileName(f)}")
			.ToArray();
		return Ok(urls);
	}

	[HttpPost("{id}/warranty")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	[RequestSizeLimit(20_000_000)]
	public async Task<IActionResult> UploadWarranty(Guid id, IFormFile file)
	{
		var entity = await _db.Appliances.FindAsync(id);
		if (entity is null) return NotFound();
		var uploadRoot = Path.Combine(_env.ContentRootPath, "uploads", "warranty", id.ToString());
		Directory.CreateDirectory(uploadRoot);
		var ext = Path.GetExtension(file.FileName);
		var fileName = $"warranty{ext}";
		var path = Path.Combine(uploadRoot, fileName);
		using var stream = System.IO.File.Create(path);
		await file.CopyToAsync(stream);
		entity.WarrantyFileUrl = $"/uploads/warranty/{id}/{fileName}";
		await _db.SaveChangesAsync();
		return Ok(new { url = entity.WarrantyFileUrl });
	}
}

public sealed class ApplianceCreateDto
{
	public Guid CustomerId { get; set; }
	public string Brand { get; set; } = string.Empty;
	public string Model { get; set; } = string.Empty;
	public string SerialNumber { get; set; } = string.Empty;
	public DateTime? PurchaseDate { get; set; }
	public DateTime? WarrantyExpiryDate { get; set; }
	public string Notes { get; set; } = string.Empty;
}

public sealed class ApplianceUpdateDto : ApplianceCreateDto { }

public sealed class ApplianceCreateDtoValidator : FluentValidation.AbstractValidator<ApplianceCreateDto>
{
	public ApplianceCreateDtoValidator()
	{
		RuleFor(x => x.CustomerId).NotEmpty();
		RuleFor(x => x.Brand).NotEmpty();
		RuleFor(x => x.Model).NotEmpty();
		RuleFor(x => x.SerialNumber).NotEmpty();
	}
}

public sealed class ApplianceUpdateDtoValidator : FluentValidation.AbstractValidator<ApplianceUpdateDto>
{
	public ApplianceUpdateDtoValidator()
	{
		Include(new ApplianceCreateDtoValidator());
	}
}