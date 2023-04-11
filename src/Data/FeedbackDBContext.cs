using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SecureFeedbackAPI.Data
{
    public class FeedbackDbContext : DbContext
    {
        public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options)
            : base(options)
        {
        }

        public DbSet<SecureFeedback> Feedback { get; set; }
    }
}