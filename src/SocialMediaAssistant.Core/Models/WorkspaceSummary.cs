namespace SocialMediaAssistant.Core.Models;

// Bu bir entity değil, sadece veri taşımak için bir model.
public class WorkspaceSummary
{
    public Guid Id { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int AccountCount { get; set; }
}