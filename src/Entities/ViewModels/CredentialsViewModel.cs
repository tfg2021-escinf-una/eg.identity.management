using System.ComponentModel.DataAnnotations;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    public class CredentialsViewModel
    {
        [Required]
        public string EmailAddress { set; get; }

        [Required]
        public string Password { set; get; }
    }
}