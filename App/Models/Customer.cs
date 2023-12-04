using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class Customer {
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Заполните это поле")]
	[RegularExpression("^[\\p{L} \\.'\\-]+$")]
	public string FullName { get; set; } = string.Empty;

	[Required(ErrorMessage = "Заполните это поле")]
	public string Address { get; set; } = string.Empty;

	[Required(ErrorMessage = "Заполните это поле")]
	[RegularExpression("^(\\d+)$")]
	public long Phone { get; set; }

	[Required(ErrorMessage = "Заполните это поле")]
	[EmailAddress(ErrorMessage = "Неверный формат адреса электронной почты.")]
	public string Email { get; set; } = string.Empty;
}
