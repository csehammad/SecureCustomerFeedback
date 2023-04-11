using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureFeedbackAPI.Data;
using SecureFeedbackAPI.DTO;
using SecureFeedbackAPI.Helpers;
using System.Text;

namespace SecureFeedbackAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackDbContext _context;

        public FeedbackController(FeedbackDbContext context)
        {
            _context = context;
        }

        private SecureFeedback EncryptCustomerFeedback(CustomerFeedback feedback)
        {
            byte[] encryptedDek = SecurityHelper.Instance.GenerateEncryptedDek(feedback.Email);

            byte[] encryptedEmail = SecurityHelper.Instance.Encrypt(feedback.Email, encryptedDek);
            byte[] encryptedContactNumber = SecurityHelper.Instance.Encrypt(feedback.ContactNumber, encryptedDek);
            byte[] encryptedFeedback = SecurityHelper.Instance.Encrypt(feedback.Feedback, encryptedDek);

            return new SecureFeedback
            {
                CustomerEmail = encryptedEmail,
                ContactNumber = encryptedContactNumber,
                EncryptedFeedback = encryptedFeedback,
                Dek = encryptedDek,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // POST api/feedback
        [HttpPost]
        public async Task<ActionResult<SecureFeedback>> PostFeedback(CustomerFeedback feedback)
        {
            SecureFeedback secureFeedback = EncryptCustomerFeedback(feedback);
            _context.Feedback.Add(secureFeedback);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFeedback), new { id = secureFeedback.Id }, secureFeedback);
        }

        // GET api/feedback/5
        [HttpGet]
        public async Task<ActionResult<CustomerFeedback>> GetFeedback(long id)
        {
            var secureFeedback = await _context.Feedback.FindAsync(id);

            if (secureFeedback == null)
            {
                return NotFound();
            }

            var feedback = DecryptSecureFeedback(secureFeedback);
            return feedback;
        }

        private CustomerFeedback DecryptSecureFeedback(SecureFeedback secureFeedback)
        {
            return new CustomerFeedback
            {
                Email = SecurityHelper.Instance.Decrypt(secureFeedback.CustomerEmail, secureFeedback.Dek),
                ContactNumber = SecurityHelper.Instance.Decrypt(secureFeedback.ContactNumber, secureFeedback.Dek),
                Feedback = SecurityHelper.Instance.Decrypt(secureFeedback.EncryptedFeedback, secureFeedback.Dek)
            };
        }
    }
}