using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Options;

public sealed class FeeServiceOptions
{
    public const string SectionName = "FeeService";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(1, 60)]
    public int TimeoutSeconds { get; set; } = 5;
}