using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizzApp.Pages
{
    public class QuizzModel : PageModel
    {
        private readonly ILogger<QuizzModel> _logger;

        public QuizzModel(ILogger<QuizzModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public void Index()
        {

        }
        
    }
}
