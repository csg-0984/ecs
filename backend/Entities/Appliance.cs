namespace KitchenAfterSales.Api.Entities;

public sealed class Appliance
{
	public Guid Id { get; set; }
	public Guid CustomerId { get; set; }
	public string Brand { get; set; } = string.Empty;
	public string Model { get; set; } = string.Empty;
	public string SerialNumber { get; set; } = string.Empty;
	public DateTime? PurchaseDate { get; set; }
	public DateTime? WarrantyExpiryDate { get; set; }
	public string Notes { get; set; } = string.Empty;

	public Customer? Customer { get; set; }
}