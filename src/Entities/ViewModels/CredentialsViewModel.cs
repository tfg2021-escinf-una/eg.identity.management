using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    [ExcludeFromCodeCoverage]
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