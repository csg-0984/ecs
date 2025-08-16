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
	}
}