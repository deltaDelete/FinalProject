using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace App.Models; 

public class Payment {
    [Key] public int Id { get; set; }
    [Required]
    public DateTimeOffset Date { get; set; }
    [Required]
    public TimeSpan Time { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [RegularExpression(@"^\d+(\.\d{1,2})?$")]
    public decimal Total { get; set; }

    [NotNull]
    [Required]
    public Customer? Customer { get; set; } = null;
    [NotNull]
    [Required]
    public Order? Order { get; set; } = null;
}