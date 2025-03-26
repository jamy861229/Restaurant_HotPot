using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System;

namespace Restaurant.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 寄送純文字郵件
        /// </summary>
        /// <param name="toEmail">收件者 Email</param>
        /// <param name="subject">郵件主旨</param>
        /// <param name="body">郵件內容（純文字）</param>
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // 從 appsettings.json 讀取 SMTP 設定
            string host = _configuration["Smtp:Host"];
            int port = int.Parse(_configuration["Smtp:Port"]);
            bool enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]);
            string userName = _configuration["Smtp:UserName"];
            string password = _configuration["Smtp:Password"];

            // 建立 SmtpClient
            using (var smtpClient = new SmtpClient(host, port))
            {
                // 不使用預設憑證
                smtpClient.UseDefaultCredentials = false;
                // 設定自己的憑證 (帳號/密碼)
                smtpClient.Credentials = new NetworkCredential(userName, password);
                // 啟用或停用 SSL / TLS
                smtpClient.EnableSsl = enableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(userName), // 寄件者
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // 若要寄送 HTML，可改成 true
                };

                mailMessage.To.Add(toEmail);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    // 這裡可做錯誤處理或記錄 Log
                    throw new Exception("寄信失敗：" + ex.Message, ex);
                }
            }
        }
    }
}
