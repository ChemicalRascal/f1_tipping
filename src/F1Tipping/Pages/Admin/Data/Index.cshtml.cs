using F1Tipping.Pages.PageModels;
using F1Tipping.Tipping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Tipping.Pages.Admin.Data
{
    public class IndexModel : AdminPageModel
    {
        private DataSeeder _dataSeeder;

        public IndexModel(DataSeeder dataSeeder)
        {
            _dataSeeder = dataSeeder;
        }

        public async Task OnGetAsync()
        {
        }

        public async Task OnGetSeedDataAsync()
        {
            await _dataSeeder.Seed2025SeasonAsync();
        }
    }
}
