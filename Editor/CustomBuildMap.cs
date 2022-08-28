using System;

namespace AssetBundleBrowser.AssetBundleDataSource
{
    [Serializable]
    public class CustomBuildMap
    {
        public bool build;
        public string buildName;
        public BundleBuildMap bundleBuildMap;
    }
}
