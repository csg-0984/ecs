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
public sealed class WorkOrdersController : ControllerBase
{
	private readonly AppDbContext _db;
	public WorkOrdersController(AppDbContext db) { _db = db; }

	[HttpGet]
	public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] bool requiresCounts = false, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = null)
	{
		var query = _db.WorkOrders
			.Include(w => w.Customer)
			.Include(w => w.Appliance)
			.Include(w => w.AssignedTechnician)
			.AsNoTracking()
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(search))
		{
			var term = search.Trim();
			query = query.Where(w => w.Title.Contains(term) || w.Description.Contains(term));
		}

		// Support 'orderby' style like "CreatedAt desc"
		var orderby = Request.Query["orderby"].ToString();
		if (!string.IsNullOrEmpty(orderby))
		{
			var parts = orderby.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			sortBy = parts.ElementAtOrDefault(0) ?? sortBy;
			sortDir = parts.ElementAtOrDefault(1) ?? sortDir;
		}

		if (!string.IsNullOrWhiteSpace(sortBy))
		{
			var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
			switch (sortBy)
			{
				case nameof(WorkOrder.Title): query = desc ? query.OrderByDescending(w => w.Title) : query.OrderBy(w => w.Title); break;
				case nameof(WorkOrder.Priority): query = desc ? query.OrderByDescending(w => w.Priority) : query.OrderBy(w => w.Priority); break;
				case nameof(WorkOrder.Status): query = desc ? query.OrderByDescending(w => w.Status) : query.OrderBy(w => w.Status); break;
				case nameof(WorkOrder.CreatedAt): query = desc ? query.OrderByDescending(w => w.CreatedAt) : query.OrderBy(w => w.CreatedAt); break;
				default: query = query.OrderByDescending(w => w.CreatedAt); break;
			}
		}
		else
		{
			query = query.OrderByDescending(w => w.CreatedAt);
		}

		var total = await query.CountAsync();
		var effectiveSkip = skip ?? Math.Max(0, (page - 1) * pageSize);
		var effectiveTake = take ?? pageSize;
		var items = await query.Skip(effectiveSkip).Take(effectiveTake).ToListAsync();

		var wantsCounts = requiresCounts || Request.Query.ContainsKey("requiresCounts");
		if (wantsCounts)
		{
			return Ok(new { result = items, count = total });
		}
		return Ok(items);
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
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<ActionResult<WorkOrder>> Create([FromBody] WorkOrderCreateDto body)
	{
		var entity = new WorkOrder
		{
			Id = Guid.NewGuid(),
			CustomerId = body.CustomerId,
			ApplianceId = body.ApplianceId,
			AssignedTechnicianId = body.AssignedTechnicianId,
			Title = body.Title,
			Description = body.Description,
			Priority = body.Priority,
			ScheduledAt = body.ScheduledAt,
			CreatedAt = DateTime.UtcNow
		};
		_db.WorkOrders.Add(entity);
		await _db.SaveChangesAsync();
		return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	public async Task<IActionResult> Update(Guid id, [FromBody] WorkOrderUpdateDto body)
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
	[Authorize(Policy = "DispatcherOrAdmin")]
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
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var entity = await _db.WorkOrders.FindAsync(id);
		if (entity is null) return NotFound();
		entity.IsDeleted = true;
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPost("{id}/attachments")]
	[Authorize(Policy = "DispatcherOrAdmin")]
	[RequestSizeLimit(20_000_000)]
	public async Task<IActionResult> UploadAttachments(Guid id, List<IFormFile> files)
	{
		var entity = await _db.WorkOrders.FindAsync(id);
		if (entity is null) return NotFound();
		var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
		var uploadRoot = Path.Combine(env.ContentRootPath, "uploads", "workorders", id.ToString());
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

	[HttpGet("{id}/attachments")]
	public IActionResult ListAttachments(Guid id)
	{
		var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
		var uploadRoot = Path.Combine(env.ContentRootPath, "uploads", "workorders", id.ToString());
		if (!Directory.Exists(uploadRoot)) return Ok(Array.Empty<string>());
		var urls = Directory.GetFiles(uploadRoot).Select(f => $"/uploads/workorders/{id}/{Path.GetFileName(f)}").ToArray();
		return Ok(urls);
	}
}

public sealed class WorkOrderCreateDto
{
	public Guid CustomerId { get; set; }
	public Guid ApplianceId { get; set; }
	public Guid? AssignedTechnicianId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
	public DateTime? ScheduledAt { get; set; }
}

public sealed class WorkOrderUpdateDto
{
	public Guid? AssignedTechnicianId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
	public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
	public DateTime? ScheduledAt { get; set; }
	public DateTime? CompletedAt { get; set; }
}

public sealed class WorkOrderCreateDtoValidator : AbstractValidator<WorkOrderCreateDto>
{
	public WorkOrderCreateDtoValidator()
	{
		RuleFor(x => x.CustomerId).NotEmpty();
		RuleFor(x => x.ApplianceId).NotEmpty();
		RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Description).NotEmpty();
	}
}

public sealed class WorkOrderUpdateDtoValidator : AbstractValidator<WorkOrderUpdateDto>
{
	public WorkOrderUpdateDtoValidator()
	{
		RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Description).NotEmpty();
	}
}