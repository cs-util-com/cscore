# Steps to follow

1. Open ``PlainNetClassLib.csproj`` and set the new version number, compare with https://www.nuget.org/packages/com.csutil.cscore
2. Build the ``PlainNetClassLib`` which will also produce the new ``.nupkg`` file
3. Upload the new package file at https://www.nuget.org/packages/manage/upload
4. Use Unity to generate a new package version 
5. Upload the new package to https://publisher.assetstore.unity3d.com/package.html?id=417525