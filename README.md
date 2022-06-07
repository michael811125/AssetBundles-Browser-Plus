# Unity Asset Bundle Browser Plus tool

This tool enables the user to view and edit the configuration of asset bundles for their Unity project.  It will block editing that would create invalid bundles, and inform you of any issues with existing bundles.  It also provides basic build functionality.

This tool is intended to replace the current workflow of selecting assets and setting their asset bundle manually in the inspector.  It can be dropped into any Unity project with a version of 5.6 or greater.  It will create a new menu item in <font color=#0050FF>*Window->AssetBundle Browser Plus*</font>.  

# Unofficial Extension
Added BuildMap to group different version assets <font color=#0050FF>(right-click to Create/Asset Bundles Broswer Plus/Bundle Build Map Asset)</font>.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/img_01.png)

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/img_02.png)

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/img_03.PNG)

When switch data source will not auto reload AssetDatabase, you will see Configure tab will appear exclamation mark messages but doesn't matter. If you want reload AssetDatabase by build map, you just manual press refresh button in left-top <font color=#FF0000>(If you have many assets will take a long time for reloading)</font>.

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/img_04.png)

Auto Refresh <font color=#FF0000>(Not Recommend)</font>

![](https://github.com/michael811125/AssetBundles-Browser-Plus/blob/master/Documentation/images/img_05.png)

# Installation
### Install via git URL
Add https://github.com/michael811125/AssetBundles-Browser-Plus.git to Package Manager.

## Full Documentation
#### Official Released Features
See [the official manual page](https://docs.unity3d.com/Manual/AssetBundles-Browser.html) or view the included [project manual page](Documentation/com.unity.assetbundlebrowser.md)