using F1Tipping.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Tipping.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private ModelDbContext _modelDb;

        public IndexModel(ILogger<IndexModel> logger, ModelDbContext modelDb)
        {
            _logger = logger;
            _modelDb = modelDb;
        }

        public void OnGet()
        {
            //if (User is not null)
            //{
            //    Redirect("Player/Init");
            //}
        }
    }
}
