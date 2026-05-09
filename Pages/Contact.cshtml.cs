using GreenSwampApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GreenSwampApp.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ICsvExportService _csvExportService;

        public ContactModel(ICsvExportService csvExportService)
        {
            _csvExportService = csvExportService;
        }

        [BindProperty]
        public ContactInputModel Input { get; set; } = new();

        public void OnGet()
        {
            // Инициализация при GET запросе
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Input.CreatedAt = DateTime.Now;

                await _csvExportService.SaveContactMessageAsync(Input, cancellationToken);

                TempData["Success"] = true;
                TempData["ShowSuccessModal"] = true;
                Input = new ContactInputModel();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Произошла ошибка при сохранении сообщения. Пожалуйста, попробуйте позже.";
                return Page();
            }
        }
    }

    public class ContactInputModel
    {
        [Required(ErrorMessage = "Поле 'Ваше имя' обязательно для заполнения")]
        [Display(Name = "Your Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле 'Email' обязательно для заполнения")]
        [EmailAddress(ErrorMessage = "Введите корректный email адрес")]
        [Display(Name = "Your Email")]
        [RegularExpression(@".+\.edu$", ErrorMessage = "Допустимы только email-адреса в домене .edu")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле 'Тема сообщения' обязательно для заполнения")]
        [Display(Name = "Message Topic")]
        public string Topic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле 'Сообщение' обязательно для заполнения")]
        [MinLength(10, ErrorMessage = "Сообщение должно содержать минимум 10 символов")]
        [Display(Name = "Your Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }
    }
}