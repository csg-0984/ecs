namespace KitchenAfterSales.Api.Entities;

public sealed class Technician
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Skills { get; set; } = string.Empty;
	public bool IsActive { get; set; } = true;

	public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
	public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}