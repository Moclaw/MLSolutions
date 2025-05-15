namespace Services.External.SmsService
{
    public interface ISmsServices
    {
        /// <summary>
        /// Sends an SMS message to a single recipient.
        /// </summary>
        /// <param name="to">The recipient's phone number.</param>
        /// <param name="message">The content of the SMS message.</param>
        void SendSms(string to, string message);
        /// <summary>
        /// Sends an SMS message to multiple recipients.
        /// </summary>
        /// <param name="to">A list of recipient phone numbers.</param>
        /// <param name="message">The content of the SMS message.</param>
        void SendSms(List<string> to, string message);
    }

}
