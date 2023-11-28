using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class Customer {
    [Key]
    public int Id { get; set; }
	[Required]
	[RegularExpression("^[\\p{L} \\.'\\-]+$")]
	public string FullName { get; set; } = string.Empty;

	[Required]
	public string Address { get; set; } = string.Empty;

	[Required]
	[RegularExpression("^(\\d+)$")]
	public long Phone { get; set; }
}
