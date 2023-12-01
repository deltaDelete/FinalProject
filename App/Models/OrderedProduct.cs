using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class OrderedProduct
{
    [Key] public int Id { get; set; }

    [Required] public Order? Order { get; set; }
    [Required] public Product? Product { get; set; }

    [Required] public int Quantity { get; set; }
}