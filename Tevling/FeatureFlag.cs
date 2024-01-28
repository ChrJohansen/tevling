namespace Tevling;

/// <summary>
/// A collection of feature flags for the app.
/// </summary>

public sealed class FeatureFlag
{
    // NB! These names should match those in appsettings.json.
    public const string DevTools = "DevTools";

    private readonly string _featureFlag;

    private FeatureFlag(string featureFlag)
    {
        _featureFlag = featureFlag;
    }

    public override string ToString()
    {
        return _featureFlag;
    }

    public static implicit operator string(FeatureFlag f) => f.ToString();
    public static implicit operator FeatureFlag(string s) => new(s);
}
