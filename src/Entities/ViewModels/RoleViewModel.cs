using System.ComponentModel.DataAnnotations;

namespace EG.IdentityManagement.Microservice.Entities.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        public string roleName { set; get; }
    }
}