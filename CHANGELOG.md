# Changelog (unofficial)

## [1.9.1] - 2022-11-02
- Fixed rename manifest doesn't process MainManifest.manifest rename issue.

## [1.9.0] - 2022-11-02
- Added Rename Manifest feature (optional).
- Modified BundleBuildMap.BuildAssetBundles() static method params (added renameManifest param).

## [1.8.2] - 2022-10-25
- Fixed replace by hash and md5 for bundle name has file name match issue, use (bundleName == manifestName) instead.

## [1.8.1] - 2022-10-17
- Fixed when choose replace by hash and md5 for bundle name options path issue ("\\" replace "/").

## [1.8.0] - 2022-10-15
- Added BundleUtility for common methods, can find BundleUtility.MakeMd5ForString method when using AssetBundleBrowser.Utilities.
- Added MD5 for bundle name option into ExtendBuildAssetBundleOptions (make a md5 by bundle name to replace it).
- Modified set AssetBundleBrowser.Utilities namespace for LevenshteinDistance class.

## [1.7.0] - 2022-10-12
- Added ExtendBuildAssetBundleOptions for Advenced Settings (most convenient).
- Modified BuildAssetBundles method in BundleBuildMap (changed extend options param).

## [1.6.2] - 2022-10-06
- Fixed replace by hash feature has load manifest file from local error issue.

## [1.6.1] - 2022-10-06
- Modified ReplaceByHash feature load manifest from build folder to replace, not use append hash to bundle name to replace.

## [1.6.0] - 2022-10-05
- Added WithoutManifest feature for build advenced settings.
- Added ReplaceByHash feature for build advenced settings.
- Added overload BuildAssetBundles() method with withoutManifest and replayceByHash params for custom build.

## [1.5.1] - 2022-08-29
- Fixed when enabled sync has refresh issue (do refresh all or refresh specific).

## [1.5.0] - 2022-08-29
- Added Sync feature for BuildMap. When drag a bundle to BuildMap can sync to specified BuildMap.
- Added namespace AssetBundleBrowser.AssetBundleDataSource for BundleBuildMap class.
- Added CustomBuildMap class for custom build.
- Modified custom build feature. You can set build name by yourself, also can decide build or not.

## [1.4.0] - 2022-06-18
- Added right-click to refresh a specific bundle. You can use right-click on bundle tree will show up refresh option can refresh a specific bundle or certain bundles (only typeof => BundleBuildMap will show up that option).

## [1.3.1] - 2022-06-14
- Fixed when move duplicates separate by asset name, but asset name has many extension name ex: myAsset.atlas.txt => will only get bundle name from first '.' = "myAsset".

## [1.3.0] - 2022-06-14
- Added Move duplicates separate by asset name into \<dependencies\> folder (right-click on a bundle in AssetBundle Browser Plus).
- Added Move duplicates by selected and separate by asset name into \<dependencies\> folder (right-click on multi-bundle in AssetBundle Browser Plus).

## [1.2.3] - 2022-06-14
- Optimized LevenshteinDistance algorithm.

## [1.2.2] - 2022-06-07
- Fixed When press build button will detect have any data source exist, if not will return and alert.

## [1.2.1] - 2022-06-07
- Added AutoRefresh toggle, but not recommend to checked (unless you don't have many assets).
- Modified methods and params access modifiers.

## [1.2.0] - 2022-06-06
- Optimized processing efficiency (only manual press refresh button will reload AssetDatabase by build map data source).
- Added custom build maps option for build.
- Added Open button in Build tab.
- Modified GetBuildMap method change private to public. You can cutom your BuildTool to gets AssetBundleBuild[] via BundleBuildMap(ScriptableObject).
#### 【Remark】When switch data source will not auto reload AssetDatabase, you will see Configure tab will appear exclamation mark messages but doesn't matter. If you want reload AssetDatabase by build map, you just manual press refresh button in left-top (If you have many assets will take a long time for reloading).

## [1.1.0] - 2022-06-01
- Optimized processing efficiency (does not refresh with every change). When open editor or manual press refresh button will refresh build map to reload data (only typeof => BundleBuildMap).
- Added save for data source index to make sure next open can get correct index to deploy bundles.
#### 【Remark】When editor window is opening but you have any asset name or asset path changed at this time, You have to manual press refresh button to reload build map data.

## [1.0.1] - 2022-06-01
- Optimized comment out when open editor to CreateDefaultDataSources().

## [1.0.0] - 2022-06-01
- Added BuildMap to group different version assets (right-click to Create/Asset Bundles Broswer Plus/BundleBuildMap).
- Modified AssetDatabaseABDataSource to hide ABDataSource.
- Removed Tests editor.

# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.8.0] - 2018-XX-XX
- Added more details to bundle detail view.
- fix for folder containing only non-asset items.

## [1.7.0] - 2018-07-23
- Sorting dependencies for easier identification.
- fix to windows file sharing violation error.
- minor bug fixes.

## [1.6.0] - 2018-04-25
- Stopped altering global styles.

## [1.5.0] - 2018-02-28
- Code static analysis cleanup.
- Documentation updates.
- fix to missing bundle-icon bug.

## [1.4.0] - 2018-02-23
- Added a search bar to the main tab.  Searches based on asset name.

## [1.3.0] - 2018-01-08
- serialization fix for inspect tab (was causing entire window to potentially be blank).
- documentation fix

## [1.2.0] - 2017-12-08
- Added asmdef to keep browser in its own assembly
- minor null check fixes

## [1.1.4] - 2017-11-09
- Initial submission for package distribution
