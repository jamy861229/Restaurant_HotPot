using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "請輸入 Email")]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        public string Email { get; set; }
    }
}
