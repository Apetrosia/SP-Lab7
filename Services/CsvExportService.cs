using CsvHelper;
using CsvHelper.Configuration;
using GreenSwampApp.Services;
using GreenSwampApp.Pages;
using System.Globalization;

namespace GreenSwampApp.Services
{
    public class CsvExportService : ICsvExportService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _csvFilePath;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CsvExportService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _csvFilePath = Path.Combine(_environment.ContentRootPath, "App_Data", "contacts.csv");
        }

        public async Task SaveContactMessageAsync(ContactInputModel message, CancellationToken cancellationToken = default)
        {
            var directory = Path.GetDirectoryName(_csvFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            var records = new List<ContactInputModel> { message };

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var fileExists = File.Exists(_csvFilePath);

                using (var stream = new FileStream(_csvFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = !fileExists,
                    Delimiter = ";",
                }))
                {
                    if (!fileExists)
                    {
                        csv.WriteHeader<ContactInputModel>();
                        await csv.NextRecordAsync();
                    }

                    csv.WriteRecord(message);
                    await csv.NextRecordAsync();
                    await writer.FlushAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<ContactInputModel>> GetAllMessagesAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_csvFilePath))
            {
                return new List<ContactInputModel>();
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                using (var reader = new StreamReader(_csvFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                }))
                {
                    var records = csv.GetRecords<ContactInputModel>().ToList();
                    return records;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}