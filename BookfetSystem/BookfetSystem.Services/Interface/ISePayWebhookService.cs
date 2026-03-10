using BookfetSystem.Services.Models.SePay;

namespace BookfetSystem.Services.Interface
{
    public interface ISePayWebhookService
    {
        Task<bool> ProcessAsync(SePayWebhookPayload payload);
    }
}
