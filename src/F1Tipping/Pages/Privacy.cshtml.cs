using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages;

public class PrivacyModel : BasePageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(
        IConfiguration configuration,
        ILogger<PrivacyModel> logger
        ) : base(configuration)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}