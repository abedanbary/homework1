
using System.ComponentModel.DataAnnotations;

namespace homework2.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

       
        public string Role { get; set; }

        
        public string? FirstName { get; set; }

      
        public string ?LastName { get; set; }

   
        public string ?NationalId { get; set; }

        
        [RegularExpression(@"^\d{4} \d{4} \d{4} \d{4}$", ErrorMessage = "Invalid credit card format")]
        public string ?CreditCardNumber { get; set; }

       
        [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Invalid date format (MM/YY)")]
        public string ?ValidDate { get; set; }

       
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVC must be 3 digits")]
        public string ?CVC { get; set; }
    }
}
