using AssetBundleBrowser.AssetBundleDataSource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBundleBuildMap", menuName = "Asset Bundles Browser Plus/BundleBuildMap", order = 1)]
public class BundleBuildMap : ScriptableObject, ABDataSource
{
    [Serializable]
    public class BuildBundleInfo
    {
        /* Bundle Info Data */
        public string assetPath;          // assetName
        public string assetBundleName;    // bundleName
        public string assetBundleVariant; // variantName
        private string _fullBundleName;   // fullBundelName = {bundleName}.{bariantName} => split by . [extension]
        public string fullBundleName { get { return _fullBundleName; } }

        /// <summary>
        /// Auto check to set bundleName and variantName
        /// </summary>
        public void AutoSetFullBundleName()
        {
            if (!string.IsNullOrEmpty(this.assetBundleVariant)) this._fullBundleName = $"{this.assetBundleName}.{this.assetBundleVariant}";
            else this._fullBundleName = this.assetBundleName;
        }
    }

    public string sourceName = string.Empty;
    public string providerName = string.Empty;

    /// <summary>
    /// (Implement ABDataSource interface) Name
    /// </summary>
    public string Name
    {
        get
        {
            return this.sourceName;
        }
    }

    /// <summary>
    /// (Implement ABDataSource interface) ProviderName
    /// </summary>
    public string ProviderName
    {
        get
        {
            return this.providerName;
        }
    }

    [SerializeField]
    protected GenericDictionary<string, BuildBundleInfo> _dictBuildBundleInfo = new GenericDictionary<string, BuildBundleInfo>();

    public const string KEY_DATASOURCES_FOLDER_PATH = "KEY_DATASOURCES_FOLDER_PATH";
    public static void OpenDataSources()
    {
        string folderPath = EditorPrefs.GetString(KEY_DATASOURCES_FOLDER_PATH, Application.dataPath);
        string openFolderPath = EditorUtility.OpenFolderPanel("Open Data Sources", folderPath, string.Empty);
        if (!string.IsNullOrEmpty(openFolderPath)) EditorPrefs.SetString(KEY_DATASOURCES_FOLDER_PATH, openFolderPath);
    }

    /// <summary>
    /// (Implement ABDataSource interface) CreateDataSources
    /// </summary>
    /// <returns></returns>
    public static List<ABDataSource> CreateDataSources()
    {
        var retList = new List<ABDataSource>();

        //retList.AddRange(CreateDefaultDataSources());

        string folderPath = EditorPrefs.GetString(KEY_DATASOURCES_FOLDER_PATH, string.Empty);
        if (string.IsNullOrEmpty(folderPath)) return retList;

        try
        {
            var files = Directory.GetFiles(folderPath, "*.asset", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var stripPath = file.Replace(Application.dataPath, "Assets");
                retList.Add(AssetDatabase.LoadAssetAtPath<BundleBuildMap>(stripPath));
            }
        }
        catch
        {
            EditorPrefs.DeleteKey(KEY_DATASOURCES_FOLDER_PATH);
        }

        return retList;
    }

    /// <summary>
    /// Create default BundleBuildMap DataSource
    /// </summary>
    /// <returns></returns>
    internal static List<ABDataSource> CreateDefaultDataSources()
    {
        var retList = new List<ABDataSource>();

        string scriptName = $"{typeof(BundleBuildMap).Name}";
        string[] guids = AssetDatabase.FindAssets(scriptName);
        foreach (string guid in guids)
        {
            if (AssetDatabase.GUIDToAssetPath(guid).Contains($"/{scriptName}.cs"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                path = path.Replace($@"Assets/", $"{Application.dataPath}/");
                path = path.Replace($@"/{scriptName}.cs", string.Empty);
                string defaultFolder = @$"{path}/../Default/";
                try
                {
                    var files = new DirectoryInfo(defaultFolder).GetFiles("*.asset", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var stripPath = file.FullName.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
                        retList.Add(AssetDatabase.LoadAssetAtPath<BundleBuildMap>(stripPath));
                    }
                }
                catch
                {
                    return retList;
                }
            }
        }

        return retList;
    }

    /// <summary>
    /// Save data
    /// </summary>
    private void _Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssetIfDirty(this);
        AssetDatabase.Refresh();
    }

    private bool _HasBuildBundleInfoByAssetPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return false;

        return this._dictBuildBundleInfo.ContainsKey(assetPath);
    }

    /// <summary>
    /// Get bundle by [assetPath = (assetName)]
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private BuildBundleInfo _GetBuildBundleInfoByAssetPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;

        this._dictBuildBundleInfo.TryGetValue(assetPath, out var buildBundleInfo);
        return buildBundleInfo;
    }

    /// <summary>
    /// Get by bundleName to find similar assetPath from bundle
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="tAssetPath"></param>
    /// <returns></returns>
    private BuildBundleInfo _GetBuildBundleInfoByBundleNameWithSimilarAssetPath(string bundleName, string tAssetPath)
    {
        var BuildBundleInfoArray = this._GetBuildBundleInfoArrayByBundleName(bundleName);
        Dictionary<int, BuildBundleInfo> samples = new Dictionary<int, BuildBundleInfo>();
        foreach (var buildBundleInfo in BuildBundleInfoArray)
        {
            samples.Add(LevenshteinDistance(buildBundleInfo.assetPath, tAssetPath), buildBundleInfo);
        }

        if (samples.Count > 0)
        {
            int max = samples.Keys.Max();
            return samples[max];
        }

        return null;
    }

    /// <summary>
    /// Get bundles of same bundleName 
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private BuildBundleInfo[] _GetBuildBundleInfoArrayByBundleName(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return new BuildBundleInfo[] { };

        List<BuildBundleInfo> listBuildBundleInfo = new List<BuildBundleInfo>();
        foreach (var bundleInfo in this._dictBuildBundleInfo.Values.ToArray())
        {
            if (bundleInfo.fullBundleName == bundleName) listBuildBundleInfo.Add(bundleInfo);
        }

        return listBuildBundleInfo.ToArray();
    }

    private void _Add(string assetPath, BuildBundleInfo buildBundleInfo)
    {
        this._dictBuildBundleInfo.Add(assetPath, buildBundleInfo);
        this._Save();
    }

    private void _Remove(string assetPath)
    {
        this._dictBuildBundleInfo.Remove(assetPath);
        this._Save();
    }

    private void _AddBundle(string assetPath, string bundleName, string variantName)
    {
        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, variantName);

            // If it doesn't exist
            if (!this._HasBuildBundleInfoByAssetPath(assetPath))
            {
                var buildBundleInfo = new BuildBundleInfo();
                buildBundleInfo.assetPath = assetPath;
                buildBundleInfo.assetBundleName = bundleName;
                buildBundleInfo.assetBundleVariant = variantName;
                buildBundleInfo.AutoSetFullBundleName();
                this._Add(assetPath, buildBundleInfo);
            }
            else
            {
                // if bundleName set to empty or null will remove from buildMap
                if (string.IsNullOrEmpty(bundleName)) this._Remove(assetPath);
                // update bundleMap data
                else
                {
                    var buildBundleInfo = this._GetBuildBundleInfoByAssetPath(assetPath);
                    buildBundleInfo.assetPath = assetPath;
                    buildBundleInfo.assetBundleName = bundleName;
                    buildBundleInfo.assetBundleVariant = variantName;
                    buildBundleInfo.AutoSetFullBundleName();
                    this._Save();
                }
            }
        }
    }

    private AssetBundleBuild[] _GetBuildMap()
    {
        // group by assetBundleName
        var groups = this._dictBuildBundleInfo.Values.GroupBy(x => x.assetBundleName);

        // after filter start to collect assetNames to AssetBundleBuild list
        List<AssetBundleBuild> abBuilds = new List<AssetBundleBuild>();
        foreach (var group in groups)
        {
            var abBuild = new AssetBundleBuild();
            string assetBundleName = group.Key;
            List<string> collectAssetNames = new List<string>();
            string assetBundleVariantName = string.Empty;

            abBuild.assetBundleName = assetBundleName;
            foreach (var b in group)
            {
                collectAssetNames.Add(b.assetPath);
                if (!string.IsNullOrEmpty(b.assetBundleVariant)) assetBundleVariantName = b.assetBundleVariant;
            }
            abBuild.assetNames = collectAssetNames.ToArray();
            abBuild.assetBundleVariant = assetBundleVariantName;
            abBuilds.Add(abBuild);
        }

        return abBuilds.ToArray();
    }

    private string[] _GetAllAssetBundleNames()
    {
        // refresh all bundle by buildMap
        this._RefreshAllAssetBundle();

        List<string> bundleNames = new List<string>();
        AssetBundleBuild[] abBuilds = this._GetBuildMap();
        for (int i = 0; i < abBuilds.Length; i++)
        {
            // to chect combine bundleName and variantName for fullBundleName
            string fullBundleName = (!string.IsNullOrEmpty(abBuilds[i].assetBundleVariant)) ? $"{abBuilds[i].assetBundleName}.{abBuilds[i].assetBundleVariant}" : abBuilds[i].assetBundleName;
            bundleNames.Add(fullBundleName);
        }

        return bundleNames.ToArray();
    }

    private void _RefreshAllAssetBundle()
    {
        // clear all asset bundle and compare with buildMap
        foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            foreach (var assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
            {
                // to check variant
                string variant = (bundleName.Split('.').Length > 1) ? bundleName.Split('.')[1] : string.Empty;

                var buildBundleInfo = this._GetBuildBundleInfoByAssetPath(assetPath);
                if (buildBundleInfo != null)
                {
                    if (buildBundleInfo.assetPath != assetPath) buildBundleInfo.assetPath = assetPath;
                    if (buildBundleInfo.assetBundleVariant != variant) buildBundleInfo.assetBundleVariant = variant;
                }
                else
                {
                    buildBundleInfo = this._GetBuildBundleInfoByBundleNameWithSimilarAssetPath(bundleName, assetPath);
                    if (buildBundleInfo != null)
                    {
                        string newAssetPath = (buildBundleInfo.assetPath != assetPath) ? assetPath : buildBundleInfo.assetPath;
                        string newBundleName = buildBundleInfo.assetBundleName;
                        string newBundleVariant = (buildBundleInfo.assetBundleVariant != variant) ? variant : buildBundleInfo.assetBundleVariant;
                        this._AddBundle(newAssetPath, newBundleName, newBundleVariant);
                    }
                }

                var assetImporter = AssetImporter.GetAtPath(assetPath);
                if (assetImporter != null) assetImporter.SetAssetBundleNameAndVariant(null, null);
            }
        }

        foreach (var abBuild in this._GetBuildMap())
        {
            foreach (string assetName in abBuild.assetNames)
            {
                var assetImporter = AssetImporter.GetAtPath(assetName);
                if (assetImporter != null) assetImporter.SetAssetBundleNameAndVariant(abBuild.assetBundleName, abBuild.assetBundleVariant);
                else this._Remove(assetName);
            }
        }
    }

    /// <summary>
    /// String similarity (levenshtein distance algorithm)
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    internal static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        if (n == 0)
        {
            return m;
        }
        if (m == 0)
        {
            return n;
        }
        for (int i = 0; i <= n; d[i, 0] = i++)
            ;
        for (int j = 0; j <= m; d[0, j] = j++)
            ;
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    #region Implement ABDataSource interfaces
    public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
    {
        //Debug.Log("<color=#FF9DD5>GetAssetPathsFromAssetBundle => </color>" + assetBundleName);
        return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
    }

    public string GetAssetBundleName(string assetPath)
    {
        //Debug.Log("<color=#FFB872>GetAssetBundleName => </color>" + assetPath);
        var importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
        {
            return string.Empty;
        }
        var bundleName = importer.assetBundleName;
        if (importer.assetBundleVariant.Length > 0)
        {
            bundleName = bundleName + "." + importer.assetBundleVariant;
        }
        return bundleName;
    }

    public string GetImplicitAssetBundleName(string assetPath)
    {
        //Debug.Log("<color=#FFE641>GetImplicitAssetBundleName => </color>" + assetPath);
        return AssetDatabase.GetImplicitAssetBundleName(assetPath);
    }

    public string[] GetAllAssetBundleNames()
    {
        return this._GetAllAssetBundleNames();
    }

    public bool IsReadOnly()
    {
        return false;
    }

    public void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variantName)
    {
        //Debug.Log("<color=#A0FF41>SetAssetBundleNameAndVariant assetPath => </color>" + assetPath);
        //Debug.Log("<color=#A0FF41>SetAssetBundleNameAndVariant bundleName => </color>" + bundleName);
        //Debug.Log("<color=#A0FF41>SetAssetBundleNameAndVariant variantName => </color>" + variantName);
        this._AddBundle(assetPath, bundleName, variantName);
    }

    public void RemoveUnusedAssetBundleNames()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }

    public bool CanSpecifyBuildTarget
    {
        get { return true; }
    }
    public bool CanSpecifyBuildOutputDirectory
    {
        get { return true; }
    }

    public bool CanSpecifyBuildOptions
    {
        get { return true; }
    }

    public bool BuildAssetBundles(ABBuildInfo info)
    {
        if (info == null)
        {
            Debug.Log("Error in build");
            return false;
        }

        // BuildAssetBundles with buildMap
        var buildManifest = BuildPipeline.BuildAssetBundles(info.outputDirectory, this._GetBuildMap(), info.options, info.buildTarget);
        if (buildManifest == null)
        {
            Debug.Log("Error in build");
            return false;
        }

        foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
        {
            if (info.onBuild != null)
            {
                info.onBuild(assetBundleName);
            }
        }

        return true;
    }
    #endregion
}
