using GreenSwampApp.Pages;
using GreenSwampApp.Pages;

namespace GreenSwampApp.Services
{
    public interface ICsvExportService
    {
        Task SaveContactMessageAsync(ContactInputModel message, CancellationToken cancellationToken = default);
        Task<List<ContactInputModel>> GetAllMessagesAsync(CancellationToken cancellationToken = default);
    }
}