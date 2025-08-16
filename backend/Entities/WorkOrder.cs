namespace KitchenAfterSales.Api.Entities;

public enum WorkOrderStatus
{
	Created = 0,
	Assigned = 1,
	InProgress = 2,
	Completed = 3,
	Cancelled = 4
}

public enum WorkOrderPriority
{
	Low = 0,
	Medium = 1,
	High = 2
}

public sealed class WorkOrder
{
	public Guid Id { get; set; }
	public Guid CustomerId { get; set; }
	public Guid ApplianceId { get; set; }
	public Guid? AssignedTechnicianId { get; set; }

	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
	public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? ScheduledAt { get; set; }
	public DateTime? CompletedAt { get; set; }

	public Customer? Customer { get; set; }
	public Appliance? Appliance { get; set; }
	public Technician? AssignedTechnician { get; set; }
}