using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class OrderStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public OrderStatus()
    {
        
    }

    public OrderStatus(string name)
    {
        Name = name;
    }

    public OrderStatus(int id, string name)
    {
        Id = id;
        Name = name;
    }
}