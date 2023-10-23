using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Instagram.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Логин")]
        public string? Login { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Аватар")]
        public string? Avatar { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Имя")]
        public string? Name { get; set; }
        [Required]
        [Display(Name = "Краткая информация")]
        public string? UserInfo { get; set; }
        [Required]
        [Display(Name = "Ваш номер")]
        public string? PhoneNumber { get; set; }
        [Required]
        [Display(Name = "Ваш пол")]
        public string? Sex { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords is not the same")]
        [DataType(DataType.Password)]
        [Display(Name = "Повторите пароль")]
        public string PasswordConfirm { get; set; }

    }
}
