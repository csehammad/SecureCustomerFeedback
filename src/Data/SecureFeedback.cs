namespace SecureFeedbackAPI
{
    public class SecureFeedback
    {
        public long Id { get; set; }
        public byte[] CustomerEmail { get; set; }
        public byte[] ContactNumber { get; set; }
        public byte[] EncryptedFeedback { get; set; }
        public byte[] Dek { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}