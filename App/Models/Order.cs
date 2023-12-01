using System;
using System.ComponentModel.DataAnnotations;
using Avalonia.Collections;

namespace App.Models;

public class Order {
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    
    [Required]
    public TimeSpan Time { get; set; } = DateTimeOffset.Now.TimeOfDay;

    [Required] public OrderStatus Status { get; set; }

    public AvaloniaList<OrderedProduct>? Products { get; set; }
}
