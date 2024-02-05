using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace WebApplication3.Model {
    public class ApplicationUser : IdentityUser {

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        public string Gender { get; set; }

        public string NRIC { get; set; }
        //email & password inherited

        [DisplayName("Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime BirthDate { get; set; } //DateOnly only avaliable in ef core 8+

        public string Resume { get; set; }

        [DisplayName("Who Am I")]
        public string WhoAmI { get; set; }

        public string? SessionID = Guid.NewGuid().ToString();

        public string setSessionID() {
            SessionID = Guid.NewGuid().ToString();
            return SessionID;
        }
    }
}
