using KitchenAfterSales.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace KitchenAfterSales.Api.Data;

public sealed class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<AppUser> Users => Set<AppUser>();
	public DbSet<Customer> Customers => Set<Customer>();
	public DbSet<Appliance> Appliances => Set<Appliance>();
	public DbSet<Technician> Technicians => Set<Technician>();
	public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
	public DbSet<Schedule> Schedules => Set<Schedule>();
	public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AppUser>()
			.HasIndex(u => u.Email)
			.IsUnique();

		modelBuilder.Entity<Customer>()
			.Property(c => c.Name)
			.HasMaxLength(200);

		modelBuilder.Entity<Appliance>()
			.HasOne(a => a.Customer)
			.WithMany(c => c.Appliances)
			.HasForeignKey(a => a.CustomerId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<WorkOrder>()
			.HasOne(w => w.Customer)
			.WithMany(c => c.WorkOrders)
			.HasForeignKey(w => w.CustomerId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<WorkOrder>()
			.HasOne(w => w.Appliance)
			.WithMany()
			.HasForeignKey(w => w.ApplianceId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<WorkOrder>()
			.HasOne(w => w.AssignedTechnician)
			.WithMany(t => t.WorkOrders)
			.HasForeignKey(w => w.AssignedTechnicianId)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Schedule>()
			.HasOne(s => s.Technician)
			.WithMany(t => t.Schedules)
			.HasForeignKey(s => s.TechnicianId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<Schedule>()
			.HasOne(s => s.WorkOrder)
			.WithMany()
			.HasForeignKey(s => s.WorkOrderId)
			.OnDelete(DeleteBehavior.Cascade);

		// Global query filters for soft delete
		modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
		modelBuilder.Entity<Appliance>().HasQueryFilter(e => !e.IsDeleted);
		modelBuilder.Entity<WorkOrder>().HasQueryFilter(e => !e.IsDeleted);
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var entries = ChangeTracker.Entries();
		var utcNow = DateTime.UtcNow;
		foreach (var entry in entries)
		{
			if (entry.Entity is Customer c)
			{
				if (entry.State == EntityState.Modified) c.UpdatedAt = utcNow;
			}
			else if (entry.Entity is Appliance a)
			{
				if (entry.State == EntityState.Modified) a.UpdatedAt = utcNow;
			}
			else if (entry.Entity is WorkOrder w)
			{
				if (entry.State == EntityState.Modified) w.UpdatedAt = utcNow;
			}
		}

		// Create audit logs for important entities
		foreach (var entry in entries)
		{
			string? entityName = entry.Entity.GetType().Name;
			if (entityName is nameof(Customer) or nameof(Appliance) or nameof(WorkOrder) or nameof(Schedule))
			{
				string action = entry.State switch
				{
					EntityState.Added => "Create",
					EntityState.Modified => "Update",
					EntityState.Deleted => "Delete",
					_ => string.Empty
				};
				if (!string.IsNullOrEmpty(action))
				{
					AuditLogs.Add(new AuditLog
					{
						Id = Guid.NewGuid(),
						EntityName = entityName,
						EntityId = (Guid)(entry.CurrentValues["Id"] ?? Guid.Empty),
						Action = action,
						OccurredAt = utcNow
					});
				}
			}
		}

		return base.SaveChangesAsync(cancellationToken);
	}
}

public sealed class AuditLog
{
	public Guid Id { get; set; }
	public string EntityName { get; set; } = string.Empty;
	public Guid EntityId { get; set; }
	public string Action { get; set; } = string.Empty;
	public DateTime OccurredAt { get; set; }
}