namespace HoliPics.Services.Interfaces
{
    public interface IEmailSenderService
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
