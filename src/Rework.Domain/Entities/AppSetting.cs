namespace Rework.Domain.Entities;

public class AppSetting
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Category { get; private set; } = "general";
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private AppSetting() { } // EF Core

    public AppSetting(string key, string value, string category)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainRuleException("A setting key is required.");
        Key = key.Trim();
        Value = value;
        Category = string.IsNullOrWhiteSpace(category) ? "general" : category.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string value)
    {
        Value = value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
