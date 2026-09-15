using Microsoft.AspNetCore.Identity;
namespace restaurant.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        // add profile fields like PhoneNumber, Address, etc. as needed
    }
}
