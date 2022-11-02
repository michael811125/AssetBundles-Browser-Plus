using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AssetBundleBrowser.AssetBundleDataSource;
using AssetBundleBrowser.Utilities;

namespace AssetBundleBrowser
{
    [System.Serializable]
    internal class AssetBundleBuildTab
    {
        const string k_BuildPrefPrefix = "ABBBuild:";

        private string m_streamingPath = "Assets/StreamingAssets";

        [SerializeField]
        private bool m_AdvancedSettings;

        [SerializeField]
        private Vector2 m_ScrollPosition;

        class ToggleData
        {
            internal ToggleData(
                bool s,
                string title,
                string tooltip,
                List<string> onToggles,
                BuildAssetBundleOptions opt = BuildAssetBundleOptions.None,
                ExtendBuildAssetBundleOptions extdOpt = ExtendBuildAssetBundleOptions.None)
            {
                if (onToggles.Contains(title))
                    state = true;
                else
                    state = s;
                content = new GUIContent(title, tooltip);
                option = opt;
                extdOption = extdOpt;
            }
            //internal string prefsKey
            //{ get { return k_BuildPrefPrefix + content.text; } }
            internal bool state;
            internal GUIContent content;
            internal BuildAssetBundleOptions option;
            internal ExtendBuildAssetBundleOptions extdOption;
        }

        private AssetBundleInspectTab m_InspectTab;

        [SerializeField]
        private BuildTabData m_UserData;

        List<ToggleData> m_ToggleData;
        ToggleData m_ForceRebuild;
        ToggleData m_CopyToStreaming;
        ToggleData m_RenameManifest;
        GUIContent m_TargetContent;
        GUIContent m_CompressionContent;
        GUIContent m_BundleNameContent;

        internal enum CompressOptions
        {
            Uncompressed = 0,
            StandardCompression,
            ChunkBasedCompression,
        }
        GUIContent[] m_CompressionOptions =
        {
            new GUIContent("No Compression"),
            new GUIContent("Standard Compression (LZMA)"),
            new GUIContent("Chunk Based Compression (LZ4)")
        };
        int[] m_CompressionValues = { 0, 1, 2 };

        internal enum BundleNameOptions
        {
            None,
            AppendHash,
            ReplaceByHash,
            Md5ForName
        }
        GUIContent[] m_BundleNameOptions = {
            new GUIContent("None"),
            new GUIContent("Append Hash To Bundle Name"),
            new GUIContent("Replace Bundle Name By Hash"),
            new GUIContent("Md5 For Bundle Name")
        };
        int[] m_BundleNameValues = { 0, 1, 2, 3 };

        internal AssetBundleBuildTab()
        {
            m_AdvancedSettings = false;
            m_UserData = new BuildTabData();
            m_UserData.m_OnToggles = new List<string>();
            m_UserData.m_UseDefaultPath = true;
        }

        internal void OnDisable()
        {
            var dataPath = System.IO.Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/AssetBundleBrowserBuild.dat";

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataPath);

            bf.Serialize(file, m_UserData);
            file.Close();

        }
        internal void OnEnable(EditorWindow parent)
        {
            m_InspectTab = (parent as AssetBundleBrowserMain).m_InspectTab;

            //LoadData...
            var dataPath = System.IO.Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/AssetBundleBrowserBuild.dat";

            if (File.Exists(dataPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataPath, FileMode.Open);
                var data = bf.Deserialize(file) as BuildTabData;
                if (data != null)
                    m_UserData = data;
                file.Close();
            }

            m_ToggleData = new List<ToggleData>();
            m_ToggleData.Add(new ToggleData(
                false,
                "Exclude Type Information",
                "Do not include type information within the asset bundle (don't write type tree).",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.DisableWriteTypeTree));
            m_ToggleData.Add(new ToggleData(
                false,
                "Force Rebuild",
                "Force rebuild the asset bundles",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.ForceRebuildAssetBundle));
            m_ToggleData.Add(new ToggleData(
                false,
                "Ignore Type Tree Changes",
                "Ignore the type tree changes when doing the incremental build check.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.IgnoreTypeTreeChanges));
            //m_ToggleData.Add(new ToggleData(
            //    false,
            //    "Append Hash",
            //    "Append the hash to the assetBundle name.",
            //    m_UserData.m_OnToggles,
            //    BuildAssetBundleOptions.AppendHashToAssetBundleName));
            m_ToggleData.Add(new ToggleData(
                false,
                "Strict Mode",
                "Do not allow the build to succeed if any errors are reporting during it.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.StrictMode));
            m_ToggleData.Add(new ToggleData(
                false,
                "Dry Run Build",
                "Do a dry run build.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.DryRunBuild));
            m_ToggleData.Add(new ToggleData(
                false,
                "Without Manifest",
                "When build finished do remove menifest files.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.None,
                ExtendBuildAssetBundleOptions.WithoutManifest));

            m_ForceRebuild = new ToggleData(
                false,
                "Clear Folders",
                "Will wipe out all contents of build directory as well as StreamingAssets/AssetBundles if you are choosing to copy build there.",
                m_UserData.m_OnToggles);
            m_CopyToStreaming = new ToggleData(
                false,
                "Copy to StreamingAssets",
                "After build completes, will copy all build content to " + m_streamingPath + " for use in stand-alone player.",
                m_UserData.m_OnToggles);
            m_RenameManifest = new ToggleData(
                false,
                "Rename Manifest File",
                "After build completes, will rename main manifest file",
                m_UserData.m_OnToggles);


            m_TargetContent = new GUIContent("Build Target", "Choose target platform to build for.");
            m_CompressionContent = new GUIContent("Compression", "Choose no compress, standard (LZMA), or chunk based (LZ4)");
            m_BundleNameContent = new GUIContent("Bundle Name", "Choose none, append hash, replace by hash, or md5 for bundle name");

            if (m_UserData.m_UseDefaultPath)
            {
                ResetPathToDefault();
            }
        }

        internal void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            bool newState = false;
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Example build setup"), centeredStyle);
            //basic options
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            // build target
            using (new EditorGUI.DisabledScope(!AssetBundleModel.Model.DataSource.CanSpecifyBuildTarget))
            {
                ValidBuildTarget tgt = (ValidBuildTarget)EditorGUILayout.EnumPopup(m_TargetContent, m_UserData.m_BuildTarget);
                if (tgt != m_UserData.m_BuildTarget)
                {
                    m_UserData.m_BuildTarget = tgt;
                    if (m_UserData.m_UseDefaultPath)
                    {
                        m_UserData.m_OutputPath = "AssetBundles/";
                        m_UserData.m_OutputPath += m_UserData.m_BuildTarget.ToString();
                        //EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
                    }
                }
            }

            //output path
            using (new EditorGUI.DisabledScope(!AssetBundleModel.Model.DataSource.CanSpecifyBuildOutputDirectory))
            {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                var newPath = EditorGUILayout.TextField("Output Path", m_UserData.m_OutputPath);
                if (!System.String.IsNullOrEmpty(newPath) && newPath != m_UserData.m_OutputPath)
                {
                    m_UserData.m_UseDefaultPath = false;
                    m_UserData.m_OutputPath = newPath;
                    //EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                Color bc = GUI.backgroundColor;
                GUI.backgroundColor = new Color32(66, 231, 255, 255);
                if (GUILayout.Button("Open", GUILayout.MaxWidth(75f)))
                    this._OpenFolder();
                GUI.backgroundColor = bc;
                GUI.backgroundColor = new Color32(255, 242, 0, 255);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
                    BrowseForFolder();
                GUI.backgroundColor = bc;
                GUI.backgroundColor = new Color32(255, 66, 116, 255);
                if (GUILayout.Button("Reset", GUILayout.MaxWidth(75f)))
                    ResetPathToDefault();
                GUI.backgroundColor = bc;
                //if (string.IsNullOrEmpty(m_OutputPath))
                //    m_OutputPath = EditorUserBuildSettings.GetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath");
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                newState = GUILayout.Toggle(
                    m_ForceRebuild.state,
                    m_ForceRebuild.content);
                if (newState != m_ForceRebuild.state)
                {
                    if (newState)
                        m_UserData.m_OnToggles.Add(m_ForceRebuild.content.text);
                    else
                        m_UserData.m_OnToggles.Remove(m_ForceRebuild.content.text);
                    m_ForceRebuild.state = newState;
                }
                newState = GUILayout.Toggle(
                    m_CopyToStreaming.state,
                    m_CopyToStreaming.content);
                if (newState != m_CopyToStreaming.state)
                {
                    if (newState)
                        m_UserData.m_OnToggles.Add(m_CopyToStreaming.content.text);
                    else
                        m_UserData.m_OnToggles.Remove(m_CopyToStreaming.content.text);
                    m_CopyToStreaming.state = newState;
                }
                EditorGUILayout.BeginHorizontal();
                newState = GUILayout.Toggle(
                  m_RenameManifest.state,
                  m_RenameManifest.content);
                if (newState != m_RenameManifest.state)
                {
                    if (newState)
                    {
                        m_UserData.m_OnToggles.Add(m_RenameManifest.content.text);

                    }
                    else
                        m_UserData.m_OnToggles.Remove(m_RenameManifest.content.text);
                    m_RenameManifest.state = newState;
                }
                EditorGUILayout.Space();
                if (newState)
                {
                    var newManifestName = EditorGUILayout.TextField("", m_UserData.m_ManifestName);
                    if (!System.String.IsNullOrEmpty(newManifestName) && newManifestName != m_UserData.m_ManifestName)
                    {
                        m_UserData.m_ManifestName = newManifestName;
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

            }

            // advanced options
            using (new EditorGUI.DisabledScope(!AssetBundleModel.Model.DataSource.CanSpecifyBuildOptions))
            {
                EditorGUILayout.Space();
                m_AdvancedSettings = EditorGUILayout.Foldout(m_AdvancedSettings, "Advanced Settings");
                if (m_AdvancedSettings)
                {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    CompressOptions cmp = (CompressOptions)EditorGUILayout.IntPopup(
                        m_CompressionContent,
                        (int)m_UserData.m_Compression,
                        m_CompressionOptions,
                        m_CompressionValues);

                    if (cmp != m_UserData.m_Compression)
                    {
                        m_UserData.m_Compression = cmp;
                    }

                    BundleNameOptions bundleNameOption = (BundleNameOptions)EditorGUILayout.IntPopup(
                        m_BundleNameContent,
                        (int)m_UserData.m_BundleNameOption,
                        m_BundleNameOptions,
                        m_BundleNameValues);

                    if (bundleNameOption != m_UserData.m_BundleNameOption)
                    {
                        m_UserData.m_BundleNameOption = bundleNameOption;
                    }

                    foreach (var tog in m_ToggleData)
                    {
                        newState = EditorGUILayout.ToggleLeft(
                            tog.content,
                            tog.state);
                        if (newState != tog.state)
                        {
                            if (newState)
                                m_UserData.m_OnToggles.Add(tog.content.text);
                            else
                                m_UserData.m_OnToggles.Remove(tog.content.text);
                            tog.state = newState;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = indent;
                }
            }

            // build.
            EditorGUILayout.Space();
            if (GUILayout.Button("Build"))
            {
                EditorApplication.delayCall += ExecuteBuild;
            }
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void ExecuteBuild()
        {
            if (AssetBundleBrowserMain.instance.DataSourceCount == 0)
            {
                EditorUtility.DisplayDialog("Build Message", "You don't have any data source!!! Please create a data source first.", "OK");
                return;
            }

            string renameManifest = string.Empty;
            if (AssetBundleModel.Model.DataSource.CanSpecifyBuildOutputDirectory)
            {
                if (string.IsNullOrEmpty(m_UserData.m_OutputPath))
                    BrowseForFolder();

                if (string.IsNullOrEmpty(m_UserData.m_OutputPath)) //in case they hit "cancel" on the open browser
                {
                    Debug.LogError("AssetBundle Build: No valid output path for build.");
                    return;
                }

                if (m_ForceRebuild.state)
                {
                    string message = "Do you want to delete all files in the directory " + m_UserData.m_OutputPath;
                    if (m_CopyToStreaming.state)
                        message += " and " + m_streamingPath;
                    message += "?";
                    if (EditorUtility.DisplayDialog("File delete confirmation", message, "Yes", "No"))
                    {
                        try
                        {
                            if (Directory.Exists(m_UserData.m_OutputPath))
                                Directory.Delete(m_UserData.m_OutputPath, true);

                            if (m_CopyToStreaming.state)
                                if (Directory.Exists(m_streamingPath))
                                    Directory.Delete(m_streamingPath, true);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
                if (!Directory.Exists(m_UserData.m_OutputPath))
                    Directory.CreateDirectory(m_UserData.m_OutputPath);

                if (m_RenameManifest.state)
                {
                    renameManifest = m_UserData.m_ManifestName;
                }
            }

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            ExtendBuildAssetBundleOptions extdOpt = ExtendBuildAssetBundleOptions.None;

            if (AssetBundleModel.Model.DataSource.CanSpecifyBuildOptions)
            {
                // compression
                if (m_UserData.m_Compression == CompressOptions.Uncompressed)
                    opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
                else if (m_UserData.m_Compression == CompressOptions.ChunkBasedCompression)
                    opt |= BuildAssetBundleOptions.ChunkBasedCompression;

                // bundle name
                if (m_UserData.m_BundleNameOption == BundleNameOptions.AppendHash)
                    opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
                else if (m_UserData.m_BundleNameOption == BundleNameOptions.ReplaceByHash)
                    extdOpt |= ExtendBuildAssetBundleOptions.ReplaceByHash;
                else if (m_UserData.m_BundleNameOption == BundleNameOptions.Md5ForName)
                    extdOpt |= ExtendBuildAssetBundleOptions.Md5ForBundleName;

                // toggle options
                foreach (var tog in m_ToggleData)
                {
                    // for extend options
                    if (tog.extdOption != ExtendBuildAssetBundleOptions.None && tog.state) extdOpt |= tog.extdOption;
                    // for options
                    else if (tog.state) opt |= tog.option;
                }
            }

            ABBuildInfo buildInfo = new ABBuildInfo();

            buildInfo.outputDirectory = m_UserData.m_OutputPath;
            buildInfo.renameManifest = renameManifest;
            buildInfo.options = opt;
            buildInfo.extdOptions = extdOpt;
            buildInfo.buildTarget = (BuildTarget)m_UserData.m_BuildTarget;
            buildInfo.onBuild = (assetBundleName) =>
            {
                if (m_InspectTab == null)
                    return;
                m_InspectTab.AddBundleFolder(buildInfo.outputDirectory);
                m_InspectTab.RefreshBundles();
            };

            AssetBundleModel.Model.DataSource.BuildAssetBundles(buildInfo);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (m_CopyToStreaming.state) DirectoryCopy(m_UserData.m_OutputPath, m_streamingPath);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                File.Copy(filePath, newFilePath, true);
            }
        }

        /// <summary>
        /// Extension method for open folder
        /// </summary>
        private void _OpenFolder()
        {
            string dir = $"{Application.dataPath}/../{m_UserData.m_OutputPath}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            System.Diagnostics.Process.Start(dir);
        }

        private void BrowseForFolder()
        {
            m_UserData.m_UseDefaultPath = false;
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", m_UserData.m_OutputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                m_UserData.m_OutputPath = newPath;
                //EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
            }
        }
        private void ResetPathToDefault()
        {
            m_UserData.m_UseDefaultPath = true;
            m_UserData.m_OutputPath = "AssetBundles/";
            m_UserData.m_OutputPath += m_UserData.m_BuildTarget.ToString();
            //EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
        }

        /// <summary>
        /// Remove manifest file from build folder
        /// </summary>
        /// <param name="outputDirectory"></param>
        internal static bool WithoutManifestFile(string outputDirectory)
        {
            try
            {
                // filter only extension is manifest
                string[] files = Directory.GetFiles(outputDirectory, "*.manifest", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (File.Exists(file)) File.Delete(file);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Replace bundle name by hash (read hash from manifest to replace)
        /// </summary>
        /// <param name="outputDirectory"></param>
        internal static bool ReplaceBundleNameByHash(string outputDirectory)
        {
            // outputDirectory last path name = manifestName
            outputDirectory = outputDirectory.Replace("\\", "/");
            string[] pathArgs = outputDirectory.Split('/');
            string manifestName = pathArgs[pathArgs.Length - 1];

            string manifestFullPath = string.Empty;

            // get all files
            string[] files = Directory.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories);
            // search from all file to find menifest
            foreach (var file in files)
            {
                // get manifest whole path (manifest will build in outputDirectory)
                if (file.IndexOf(manifestName) != -1)
                {
                    manifestFullPath = Path.GetFullPath(file);
                    break;
                }
            }

            if (string.IsNullOrEmpty(manifestFullPath)) return false;

            try
            {
                // file stream to read manifest
                var fs = new FileStream(manifestFullPath, FileMode.Open, FileAccess.Read, FileShare.None);
                var bundle = AssetBundle.LoadFromStream(fs);
                fs.Dispose();

                // load manifest asset
                AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                // replace all bundle file name by hash
                foreach (var file in files)
                {
                    string bundleName = file.Replace(outputDirectory, string.Empty);
                    bundleName = bundleName.Substring(1, bundleName.Length - 1);
                    // skip process manifest & .manifest extension
                    if (bundleName == manifestName || file.IndexOf(".manifest") != -1) continue;

                    // file name (without extension)
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // get hash from manifest
                    string hash = manifest.GetAssetBundleHash(bundleName).ToString();
                    // replace it be new file
                    string newFile = file.Replace(fileName, hash);
                    // copy override
                    if (File.Exists(file)) File.Move(file, newFile);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make md5 by bundle file name and to replace it
        /// </summary>
        /// <returns></returns>
        internal static bool Md5ForBundleName(string outputDirectory)
        {
            // outputDirectory last path name = manifestName
            outputDirectory = outputDirectory.Replace("\\", "/");
            string[] pathArgs = outputDirectory.Split('/');
            string manifestName = pathArgs[pathArgs.Length - 1];

            try
            {
                // get all files
                string[] files = Directory.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories);
                // replace all bundle file name by hash
                foreach (var file in files)
                {
                    string bundleName = file.Replace(outputDirectory, string.Empty);
                    bundleName = bundleName.Substring(1, bundleName.Length - 1);
                    // skip process manifest & .manifest extension
                    if (bundleName == manifestName || file.IndexOf(".manifest") != -1) continue;

                    // file name (without extension)
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // make a md5 by file name
                    string md5 = BundleUtility.MakeMd5ForString(fileName);
                    // replace it be new file
                    string newFile = file.Replace(fileName, md5);
                    // copy override
                    if (File.Exists(file)) File.Move(file, newFile);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rename main manifest file
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        internal static bool RenameManifest(string outputDirectory, string newName)
        {
            // outputDirectory last path name = manifestName
            outputDirectory = outputDirectory.Replace("\\", "/");
            string[] pathArgs = outputDirectory.Split('/');
            string manifestName = pathArgs[pathArgs.Length - 1];

            try
            {
                // get all files
                string[] files = Directory.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories);
                // replace all bundle file name by hash
                foreach (var file in files)
                {
                    string bundleName = file.Replace(outputDirectory, string.Empty);
                    bundleName = bundleName.Substring(1, bundleName.Length - 1);

                    // only process main manifest file
                    if (bundleName == manifestName || bundleName == $"{manifestName}.manifest")
                    {
                        // file name (without extension)
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        // replace it be new file (only replace last)
                        int startIndex = file.LastIndexOf(bundleName);
                        string newFile = file.Remove(startIndex, bundleName.Length).Insert(startIndex, newName);
                        // specific process
                        if (bundleName == $"{manifestName}.manifest") newFile = $@"{newFile}.manifest";
                        // copy override
                        if (File.Exists(file)) File.Move(file, newFile);
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        //Note: this is the provided BuildTarget enum with some entries removed as they are invalid in the dropdown
        internal enum ValidBuildTarget
        {
            //NoTarget = -2,        --doesn't make sense
            //iPhone = -1,          --deprecated
            //BB10 = -1,            --deprecated
            //MetroPlayer = -1,     --deprecated
            StandaloneOSXUniversal = 2,
            StandaloneOSXIntel = 4,
            StandaloneWindows = 5,
            WebPlayer = 6,
            WebPlayerStreamed = 7,
            iOS = 9,
            PS3 = 10,
            XBOX360 = 11,
            Android = 13,
            StandaloneLinux = 17,
            StandaloneWindows64 = 19,
            WebGL = 20,
            WSAPlayer = 21,
            StandaloneLinux64 = 24,
            StandaloneLinuxUniversal = 25,
            WP8Player = 26,
            StandaloneOSXIntel64 = 27,
            BlackBerry = 28,
            Tizen = 29,
            PSP2 = 30,
            PS4 = 31,
            PSM = 32,
            XboxOne = 33,
            SamsungTV = 34,
            N3DS = 35,
            WiiU = 36,
            tvOS = 37,
            Switch = 38
        }

        [System.Serializable]
        internal class BuildTabData
        {
            internal List<string> m_OnToggles;
            internal ValidBuildTarget m_BuildTarget = ValidBuildTarget.StandaloneWindows;
            internal CompressOptions m_Compression = CompressOptions.StandardCompression;
            internal BundleNameOptions m_BundleNameOption = BundleNameOptions.None;
            internal string m_OutputPath = string.Empty;
            internal bool m_UseDefaultPath = true;
            internal string m_ManifestName = string.Empty;
        }
    }

}