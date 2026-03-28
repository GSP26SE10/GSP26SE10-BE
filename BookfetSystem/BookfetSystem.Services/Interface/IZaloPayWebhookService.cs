using BookfetSystem.Services.Models.ZaloPay;

namespace BookfetSystem.Services.Interface
{
    public interface IZaloPayWebhookService
    {
        Task<(int ReturnCode, string ReturnMessage)> ProcessAsync(ZaloPayCallbackPayload payload);
    }
}
