using System.ComponentModel.DataAnnotations;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    public class CredentialsViewModel
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { set; get; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { set; get; }
    }
}