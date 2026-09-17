using System.ComponentModel.DataAnnotations;

namespace PurchaseManagement.Api.DTOs;

public class PurchaseBillRequestDto
{
    [Required(ErrorMessage = "Item is required")]
    public string Item { get; set; } = string.Empty;

    [Required(ErrorMessage = "Batch is required")]
    public string Batch { get; set; } = string.Empty;

    [Required(ErrorMessage = "Standard Cost is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Standard Cost must be 0 or greater")]
    public decimal StandardCost { get; set; }

    [Required(ErrorMessage = "Standard Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Standard Price must be 0 or greater")]
    public decimal StandardPrice { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Discount is required")]
    [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
    public decimal Discount { get; set; }
}
