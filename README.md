# Unity Asset Bundle Browser Plus tool

This tool enables the user to view and edit the configuration of asset bundles for their Unity project.  It will block editing that would create invalid bundles, and inform you of any issues with existing bundles.  It also provides basic build functionality.

This tool is intended to replace the current workflow of selecting assets and setting their asset bundle manually in the inspector.  It can be dropped into any Unity project with a version of 5.6 or greater.  It will create a new menu item in <font color=#0050FF>*Window->AssetBundle Browser Plus*</font>.  

# Unofficial Extension
Added BuildMap to group different version assets <font color=#0050FF>(right-click to Create/AssetBundle Broswer Plus/Bundle Build Map Asset)</font>.

### Create BuildMap

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_1.png)

### Allow Custom Build

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_2.png)

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_3.png)

### Switch AssetDatabase

When switch data source will not auto reload AssetDatabase, you will see Configure tab will appear exclamation mark messages but doesn't matter. If you want reload AssetDatabase by build map, you just manual press refresh button in left-top <font color=#FF0000>(If you have many assets will take a long time for reloading)</font>.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_4.png)

### Auto Refresh <font color=#FF0000>(Not Recommend)</font>

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_5.png)

### Refresh Specific Bundle <font color=#FF0000>(Recommend)</font>

When switch data source if has exclamation mark messages you can right-click on bundle tree to refresh a specific bundle more efficiency (don't need to refresh all), and you have changed asset path, rename asset name, delete asset from project you can also right-click to refresh.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_6.png)

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_7.png)

### Sync Bundle To Specified BuildMap

When enabled sync feature can sync asset data to other BuildMap <font color=#FF0000>(recommend refresh specific a bundle instead of refresh all)</font>.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_8.gif)

### Custom Your BuildTool

```
    // your BundleBuildMap assetPath.
    string assetPath = "Assets/BuildMaps/MyBundleMap.asset"
    // load BundleBuildMap
    var bundleBuildMap = AssetDatabase.LoadAssetAtPath<BundleBuildMap>(assetPath);
    if (bundleBuildMap == null)
    {
        Debug.Log($"Error in LoadAssetAtPath: {assetPath}");
        return;
    }
	
    // output path
    string outputDirectory = Path.Combine(Application.dataPath, $"../AssetBundles/MyBundle");
	
    // platform
    BuildTarget target = BuildTarget.StandaloneWindows;
	
    // LZ4
    BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
    
    // including extend options
    ExtendBuildAssetBundleOptions extdOptions = ExtendBuildAssetBundleOptions.None;
    extdOptions |= ExtendBuildAssetBundleOptions.WithoutManifest;
    // choose one [ReplaceByHash] or [Md5ForBundleName]
    // extdOptions |= ExtendBuildAssetBundleOptions.ReplaceByHash;
    // extdOptions |= ExtendBuildAssetBundleOptions.Md5ForBundleName
	
    // rename manifest (if set it empty or null will not to do)
    string renameManifest = "myManifest";
	
    // execute regular build
    BundleBuildMap.BuildAssetBundles(outputDirectory, bundleBuildMap.GetBuildMap(), options, target);
	
    // execute extend build
    BundleBuildMap.BuildAssetBundles(outputDirectory, bundleBuildMap.GetBuildMap(), options, target, extdOptions, renameManifest);
```

### Build Extension

- Rename Manifest : after build completes, will rename main manifest file.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_10.png)

### Advenced Settings Extension

- WithoutManifest : after build remove manifest file from build folder.

- ReplaceByHash : after build load manifest from build folder to read hash to replace.

- Md5ForBundleName : after build get file name to make a md5 and to replace it.

**※Remark** : If use custom build tool for extend option **ReplaceByHash** or **Md5ForBundleName** choose one.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/desc_img_9.png)

# Installation
### Install via git URL
Add https://github.com/michael811125/AssetBundles-Browser-Plus.git to Package Manager.

### Install via OpenUPM
The package is available on the openupm registry. It's recommended to install it via openupm-cli.

https://openupm.com/packages/com.michaelo.assetbundlebrowser.plus/?subPage=readme

```
openupm add com.michaelo.assetbundlebrowser.plus
```

## Full Documentation
#### Official Released Features
See [the official manual page](https://docs.unity3d.com/Manual/AssetBundles-Browser.html) or view the included [project manual page](Documentation/com.unity.assetbundlebrowser.md)