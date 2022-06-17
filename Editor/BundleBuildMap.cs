using AssetBundleBrowser.AssetBundleDataSource;
using AssetBundleBrowser.AssetBundleModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBundleBuildMap", menuName = "AssetBundle Browser Plus/Bundle Build Map Asset", order = 1)]
public class BundleBuildMap : ScriptableObject, ABDataSource
{
    [Serializable]
    public class BuildBundleInfo
    {
        /* Bundle Info Data */
        public string assetPath;          // assetName
        public string assetBundleName;    // bundleName
        public string assetBundleVariant; // variantName
        public string fullBundleName      // fullBundelName = {bundleName}.{bariantName} => split by . [extension]
        {
            get
            {
                if (!string.IsNullOrEmpty(this.assetBundleVariant)) return $"{this.assetBundleName}.{this.assetBundleVariant}";
                else return this.assetBundleName;
            }
        }
    }

    public bool allowCustomBuild = false;
    public string sourceName = string.Empty;
    public string providerName = string.Empty;

    [SerializeField]
    internal List<BundleBuildMap> customBuildMaps;

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
                var data = AssetDatabase.LoadAssetAtPath<BundleBuildMap>(stripPath);
                if (AssetDatabase.IsNativeAsset(data)) retList.Add(data);
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
    public void Save()
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
        Dictionary<decimal, BuildBundleInfo> samples = new Dictionary<decimal, BuildBundleInfo>();
        foreach (var buildBundleInfo in BuildBundleInfoArray)
        {
            decimal pct = LevenshteinDistance.LevenshteinDistanceDecimal(buildBundleInfo.assetPath, tAssetPath);
            if (!samples.ContainsKey(pct)) samples.Add(pct, buildBundleInfo);
        }

        if (samples.Count > 0)
        {
            decimal max = samples.Keys.Max();
            Debug.Log($"<color=#FFE733>BundleName: {bundleName},</color> <color=#FF9933>Most Similar ({(max * 100).ToString("f2")}%) => Replace</color> <color=#33D6FF>old AssetPath: {samples[max].assetPath}</color> <color=#FF9933>to</color> <color=#A4FF33>new AssetPath:{tAssetPath}</color>");
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
            if (bundleInfo.fullBundleName == bundleName) listBuildBundleInfo.Add(bundleInfo); ;
        }

        return listBuildBundleInfo.ToArray();
    }

    private void _Add(string assetPath, BuildBundleInfo buildBundleInfo)
    {
        this._dictBuildBundleInfo.Add(assetPath, buildBundleInfo);
        this.Save();
    }

    private void _Remove(string assetPath)
    {
        this._dictBuildBundleInfo.Remove(assetPath);
        this.Save();
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
                    this.Save();
                }
            }
        }
    }

    public AssetBundleBuild[] GetBuildMap(string[] bundleNames = null)
    {
        // group by assetBundleName
        var groups = this._dictBuildBundleInfo.Values.GroupBy(x => x.assetBundleName);

        // after filter start to collect assetNames to AssetBundleBuild dict
        Dictionary<string, AssetBundleBuild> dictAssetBundleBuild = new Dictionary<string, AssetBundleBuild>();
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
            dictAssetBundleBuild.Add(assetBundleName, abBuild);
        }

        // only return AssetBundleBuild list by selected
        if (bundleNames != null)
        {
            List<AssetBundleBuild> listAssetBundleBuild = new List<AssetBundleBuild>();
            foreach (var bundleName in bundleNames)
            {
                if (dictAssetBundleBuild.ContainsKey(bundleName)) listAssetBundleBuild.Add(dictAssetBundleBuild[bundleName]);
            }
            return listAssetBundleBuild.ToArray();
        }

        return dictAssetBundleBuild.Values.ToArray();
    }

    private string[] _GetAllAssetBundleNames()
    {
        List<string> bundleNames = new List<string>();
        AssetBundleBuild[] abBuilds = this.GetBuildMap();
        for (int i = 0; i < abBuilds.Length; i++)
        {
            // to chect combine bundleName and variantName for fullBundleName
            string fullBundleName = (!string.IsNullOrEmpty(abBuilds[i].assetBundleVariant)) ? $"{abBuilds[i].assetBundleName}.{abBuilds[i].assetBundleVariant}" : abBuilds[i].assetBundleName;
            bundleNames.Add(fullBundleName);
        }

        return bundleNames.ToArray();
    }

    internal void RefreshAllAssetBundle(string[] bundleNames = null)
    {
        bool bySelected = false;
        if (bundleNames != null) bySelected = true;
        else bundleNames = AssetDatabase.GetAllAssetBundleNames();

        // clear all asset bundle and compare with buildMap
        foreach (var bundleName in bundleNames)
        {
            // search current AssetDatabase and compare with current BuildMap whether asset path is different or not
            foreach (var assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(bundleName))
            {
                // to check variant
                string variant = (bundleName.Split('.').Length > 1) ? bundleName.Split('.')[1] : string.Empty;

                // if it's same with current BuildMap asset path key, but asset path is different will set again correct asset path from AssetDatabase
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

        AssetBundleBuild[] abBuilds = bySelected ? this.GetBuildMap(bundleNames) : this.GetBuildMap();
        foreach (var abBuild in abBuilds)
        {
            foreach (string assetName in abBuild.assetNames)
            {
                var assetImporter = AssetImporter.GetAtPath(assetName);
                if (assetImporter != null) assetImporter.SetAssetBundleNameAndVariant(abBuild.assetBundleName, abBuild.assetBundleVariant);
                else this._Remove(assetName);
            }
        }
    }

    internal void RefreshAllAssetBundle(IEnumerable<BundleInfo> bundles)
    {
        HashSet<string> bundleNames = new HashSet<string>();

        foreach (var bundle in bundles)
        {
            // folder
            if (typeof(BundleFolderInfo).IsInstanceOfType(bundle))
            {
                foreach (var subBundle in (bundle as BundleFolderInfo).GetChildList())
                {
                    string bundleName = subBundle.m_Name.bundleName;
                    if (!bundleNames.Contains(bundleName)) bundleNames.Add(bundleName);
                }
            }
            else if (typeof(BundleInfo).IsInstanceOfType(bundle))
            {
                string bundleName = bundle.m_Name.bundleName;
                if (!bundleNames.Contains(bundleName)) bundleNames.Add(bundleName);
            }
        }

        if (bundleNames.Count > 0) this.RefreshAllAssetBundle(bundleNames.ToArray());
    }

    public BundleBuildMap[] GetCustomBuildMaps()
    {
        if (this.customBuildMaps == null) return new BundleBuildMap[] { };
        return this.customBuildMaps.ToArray();
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
            // combine bundle name with variant name (full bundle name)
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
        if (this.allowCustomBuild)
        {
            if (this.customBuildMaps != null && this.customBuildMaps.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Custom Build Maps Progress Notification:\n");

                for (int i = 0; i < this.customBuildMaps.Count; i++)
                {
                    string outputDirectory = $"{info.outputDirectory}/{this.customBuildMaps[i].sourceName}";

                    if (BuildAssetBundles(outputDirectory, this.customBuildMaps[i].GetBuildMap(), info.options, info.buildTarget, null))
                    {
                        stringBuilder.Append($"<color=#9DFF42>[{this.customBuildMaps[i].sourceName}] Build Result => Success</color>\n");
                    }
                    else
                    {
                        stringBuilder.Append($"<color=#FF4243>[{this.customBuildMaps[i].sourceName}] Build Result => Failure</color>\n");
                    }
                }

                Debug.Log(stringBuilder.ToString());
            }
        }
        else
        {
            return BuildAssetBundles(info.outputDirectory, this.GetBuildMap(), info.options, info.buildTarget, info.onBuild);
        }

        return false;
    }

    public static bool BuildAssetBundles(string outputDirectory, AssetBundleBuild[] buildMap, BuildAssetBundleOptions options, BuildTarget buildTarget, Action<string> onBuild)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        var buildManifest = BuildPipeline.BuildAssetBundles(outputDirectory, buildMap, options, buildTarget);
        if (buildManifest == null)
        {
            Debug.Log("Error in build");
            return false;
        }

        if (onBuild != null)
        {
            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                onBuild(assetBundleName);
            }
        }

        return true;
    }
    #endregion
}
