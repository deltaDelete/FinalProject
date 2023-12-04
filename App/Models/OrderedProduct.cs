using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace App.Models;

[JsonObject]
public class OrderedProduct
{
    [Key]
    [JsonProperty] 
    public int Id { get; set; }

    [Required]
    [JsonIgnore] 
    public Order? Order { get; set; }
    [Required]
    [JsonProperty] 
    public Product? Product { get; set; }

    [JsonProperty]
    public int OrderId { get; set; }
    [JsonProperty]
    public int ProductId { get; set; }

    [Required] 
    [JsonProperty] 
    public int Quantity { get; set; }
}