using Windows.Foundation.Metadata;
using Windows.UI.Composition;

namespace Bililive_dm_UWPViewer;

public class BuildInfo
{
    private static BuildInfo _buildInfo;

    private BuildInfo()
    {
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            Build = Build.FallCreators;
        else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            Build = Build.Creators;
        else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            Build = Build.Anniversary;
        else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2))
            Build = Build.Threshold2;
        else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 1))
            Build = Build.Threshold1;
        else
            Build = Build.Unknown;

        if (!BeforeCreatorsUpdate)
        {
            var capabilities = CompositionCapabilities.GetForCurrentView();
            capabilities.Changed += (s, e) => UpdateCapabilities(capabilities);
            UpdateCapabilities(capabilities);
        }

        void UpdateCapabilities(CompositionCapabilities capabilities)
        {
            AreEffectsSupported = capabilities.AreEffectsSupported();
            AreEffectsFast = capabilities.AreEffectsFast();
        }
    }

    public static Build Build { get; private set; }
    public static bool AreEffectsFast { get; private set; }
    public static bool AreEffectsSupported { get; private set; }
    public static bool BeforeCreatorsUpdate => Build < Build.Creators;

    public static BuildInfo RetrieveApiInfo()
    {
        return _buildInfo ?? (_buildInfo = new BuildInfo());
    }
}