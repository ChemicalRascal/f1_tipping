﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.AspNetCore.Authorization;
using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages.Admin.Data.Drivers
{
    public class IndexModel : AdminPageModel
    {
        private readonly ModelDbContext _context;

        public IndexModel(ModelDbContext context)
        {
            _context = context;
        }

        public IList<Driver> Driver { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Driver = (await _context.Drivers.ToListAsync())
                .OrderBy(d => d.Team.ListOrder)
                .ThenBy(d => d.ListOrder)
                .ToList();
        }
    }
}
