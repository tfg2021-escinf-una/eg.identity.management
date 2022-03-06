using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class TokensViewModel
    {
        [Required]
        public string jwtToken { set; get; }

        [Required]
        public string refreshToken { set; get; }
    }
}