using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class ReplyFeedbackController : Controller
    {
        private readonly MyDbContext _context;

        public ReplyFeedbackController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index_ReplyFeedback()
        {
            var feebacks = await _context.CustomerFeedbacks
                               .Select(m => new CustomerFeedbackView
                               {
                                   FeedbackId = m.FeedbackId,
                                   FeedbackName = m.FeedbackName,
                                   FeedbackGender = m.FeedbackGender,
                                   FeedbackEmail = m.FeedbackEmail,
                                   FeedbackPhone = m.FeedbackPhone,
                                   FeedbackContent = m.FeedbackContent,
                                   FeedbackDateTime = m.FeedbackDateTime,
                                   FeedbackDiningLocation = m.FeedbackDiningLocation,
                                   FeedbackTime = m.FeedbackTime,
                                   FeedbackMenuName = m.FeedbackMenuName,
                                   FeedbackReply=m.FeedbackReply,
                               })
                               .ToListAsync();
            return View(feebacks);
        }

        public async Task<IActionResult> Edit_ReplyFeedback(int? feedbackId)
        {
            if (feedbackId == null)
            {
                return NotFound();
            }

            var feedback = await _context.CustomerFeedbacks.FindAsync(feedbackId);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_ReplyFeedback(int? feedbackId, CustomerFeedbackView model)
        {

            // 更新資料
            var customerFeedback = await _context.CustomerFeedbacks.FindAsync(feedbackId);

            customerFeedback.FeedbackReply = model.FeedbackReply;
            
            try
            {
                _context.CustomerFeedbacks.Update(customerFeedback);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "更新資料時發生錯誤：" + ex.Message);
                return View(model);
            }

            return RedirectToAction("Index_ReplyFeedback");
        }
    }
}
