namespace KitchenAfterSales.Api.Entities;

public sealed class AppUser
{
	public Guid Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public string Role { get; set; } = "User";
}