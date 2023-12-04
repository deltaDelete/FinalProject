using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Microsoft.EntityFrameworkCore;

namespace App.Models;

public class Product {
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    [Precision(30, 2)]
    [RegularExpression(@"^\d+((\.|,)\d{1,2})?$")]
    public decimal Price { get; set; }
    
    public AvaloniaList<OrderedProduct>? Orders { get; set; }
}
