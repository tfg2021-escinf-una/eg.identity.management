using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class RoleViewModel
    {
        [Required]
        public string roleName { set; get; }
    }
}