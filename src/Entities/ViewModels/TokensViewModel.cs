using System.ComponentModel.DataAnnotations;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    public class TokensViewModel
    {
        [Required]
        public string jwtToken { set; get; }

        [Required]
        public string refreshToken { set; get; }
    }
}