namespace KitchenAfterSales.Api.Entities;

public sealed class Customer
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string Province { get; set; } = string.Empty;
	public string PostalCode { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
	public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}