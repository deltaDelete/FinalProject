using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace App.Models; 

[JsonObject]
public class Payment {
    [Key]
    [JsonProperty]
    public int Id { get; set; }
    [Required]
    [JsonProperty]
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    [Required]
    [JsonProperty]
    public TimeSpan Time { get; set; } = DateTimeOffset.Now.TimeOfDay;

    [Required]
    [JsonProperty]
    [Precision(30, 2)]
    [Range(0, double.MaxValue)]
    [RegularExpression(@"^\d+((\.|,)\d{1,2})?$")]
    public decimal Total { get; set; }

    [NotNull]
    [Required]
    [JsonProperty]
    public Order? Order { get; set; } = null;
}