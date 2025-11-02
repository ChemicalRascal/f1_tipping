using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages;

public class RulesModel : BasePageModel
{
    private readonly ILogger<RulesModel> _logger;

    public RulesModel(
        IConfiguration configuration,
        ILogger<RulesModel> logger
        ) : base(configuration)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}