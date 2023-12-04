using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace App.Models;

[JsonObject]
public class Product {
    [Key]
    [JsonProperty]
    public int Id { get; set; }
    [Required]
    [JsonProperty]
    public string Name { get; set; } = string.Empty;

    [Required]
    [JsonProperty]
    [Range(0, double.MaxValue)]
    [Precision(30, 2)]
    [RegularExpression(@"^\d+((\.|,)\d{1,2})?$")]
    public decimal Price { get; set; }
    
    [JsonIgnore]
    public AvaloniaList<OrderedProduct>? Orders { get; set; }
}
