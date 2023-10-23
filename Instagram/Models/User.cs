using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Instagram.Models
{
    public class User : IdentityUser <int>
    {
        [Required]
        public string? Login { get; set; }
        [Display(Name = "")]
        public string? Avatar { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? UserInfo { get; set; }
        [Required]
        public string? Sex { get; set; }
        [Display(Name = "Публикации")]
        public int? Publications { get; set; }
        [Display(Name = "Подписчики")]
        public int? Subscribers { get; set; }
        [Display(Name = "Подписки")]
        public int? Subscribes { get; set; }

        public User()
        {
            Publications = 0;
            Subscribers = 0;
            Subscribes = 0;
        }
    }
}
