namespace HoliPics.Services.Interfaces
{
    public interface IEmailSenderService
    {
        public Task<string> SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
