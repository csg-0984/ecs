namespace KitchenAfterSales.Api.Entities;

public enum ScheduleStatus
{
	Scheduled = 0,
	Completed = 1,
	Cancelled = 2
}

public sealed class Schedule
{
	public Guid Id { get; set; }
	public Guid TechnicianId { get; set; }
	public Guid WorkOrderId { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;

	public Technician? Technician { get; set; }
	public WorkOrder? WorkOrder { get; set; }
}