using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels {
    public class Register {
        public const string passwordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$";

        [Display(Name = "First Name")]
        [Required]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string NRIC { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Key]   //Must be unique
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(12)] //Min 12 chars, Use combination of lower-case, upper-case, Numbers and special characters)
        [RegularExpression(passwordRegex, ErrorMessage = "Weak Password")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Date of Birth")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile Resume { get; set; }

        [Required]
        [FileExtensions(Extensions = "docx,pdf")]  //(.docx or .pdf file)
        public string FileName => Resume.FileName;

        [Display(Name = "Who Am I")]
        [Required]
        [DataType(DataType.MultilineText)]
        public string WhoAmI { get; set; }
    }
}
