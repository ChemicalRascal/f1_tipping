// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Tipping.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordConfirmation : BasePageModel
{
    public ForgotPasswordConfirmation(IConfiguration configuration) : base(configuration)
    { }

    public void OnGet()
    {
    }
}
