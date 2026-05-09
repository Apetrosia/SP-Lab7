using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenSwampApp.Pages
{
    public class Model404 : PageModel
    {
        private readonly ILogger<Model404> _logger;

        public Model404(ILogger<Model404> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
