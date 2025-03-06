using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models;

[Keyless]
public class CustomerFeedbackView
{
    public int Feedback_FeedbackId { get; set; }

    [Required(ErrorMessage = "請輸入姓名")]
    [Display(Name = "姓名")]
    public string? Feedback_Name { get; set; }

    [Required(ErrorMessage = "請選擇性別")]
    [Display(Name = "性別")]
    public string? Feedback_Gender { get; set; }

    [Required(ErrorMessage = "請選擇用餐時段")]
    [Display(Name = "用餐時段")]
    public DateTime? Feedback_DateTime { get; set; }

    [Required(ErrorMessage = "請選擇分店")]
    [Display(Name = "用餐門市")]
    public int Feedback_DiningLocationId { get; set; }

    [Display(Name = "用餐門市")]
    public string? Feedback_DiningLocation { get; set; }

    [Required(ErrorMessage = "請選擇用餐品項")]
    [Display(Name = "用餐品項")]
    public int? Feedback_MenuId { get; set; }

    [Display(Name = "用餐品項")]
    public string? Feedback_MenuName { get; set; }

    [Required(ErrorMessage = "請輸入手機號碼")]
    [RegularExpression(@"^09[0-9]{8}$", ErrorMessage = "手機號碼格式錯誤")]
    [Display(Name = "電話")]
    public string? Feedback_Phone { get; set; }

    [Required(ErrorMessage = "請輸入電子郵件")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [Display(Name = "email")]
    public string? Feedback_Email { get; set; }

    [Required(ErrorMessage = "請輸入您的意見")]
    [Display(Name = "問題建議")]
    public string? Feedback_Content { get; set; }

    [Display(Name = "填寫時間")]
    public DateTime? Feedback_Time { get; set; }

    public virtual MenuView? FeedbackMenu { get; set; }
}
