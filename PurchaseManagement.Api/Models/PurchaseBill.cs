using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseManagement.Api.Models;

[Table("Purchase_Bills")]
public class PurchaseBill
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("Item")]
    public string Item { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("Batch")]
    public string Batch { get; set; } = string.Empty;

    [Column("Standard_Cost")]
    public decimal StandardCost { get; set; }

    [Column("Standard_Price")]
    public decimal StandardPrice { get; set; }

    [Column("Quantity")]
    public int Quantity { get; set; }

    [Column("Discount")]
    public decimal Discount { get; set; }

    [Column("Total_Cost")]
    public decimal TotalCost { get; set; }

    [Column("Total_Selling")]
    public decimal TotalSelling { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
