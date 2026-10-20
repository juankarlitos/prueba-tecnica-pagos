namespace PaymentApi.Domain;

public static class PaymentChannels
{
    public const string Web = "WEB";
    public const string Mobile = "MOBILE";
    public const string Branch = "BRANCH";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { Web, Mobile, Branch };

    public static bool IsValid(string? channel) =>
        channel is not null && All.Contains(channel);
}