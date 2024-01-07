// This file is licensed under the Unity Companion License. 
// For full license terms, please see: https://unity3d.com/legal/licenses/Unity_Companion_License

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace StarterAssetsPackageChecker
{
    public static class PackageChecker
    {
        class PackageEntry
        {
            public string Name;
            public string Version;
        }

        [Serializable]
        class Settings
        {
            public string EditorFolderRoot = "Assets/StarterAssets/";

            public string[] PackagesToAdd = new string[]
            {
                "com.unity.cinemachine",
                "com.unity.inputsystem"
            };

            public string PackageCheckerScriptingDefine => "STARTER_ASSETS_PACKAGES_CHECKED";
        }

        static ListRequest _clientList;
        static SearchRequest _compatibleList;
        static List<PackageEntry> _packagesToAdd;

        static AddRequest[] _addRequests;
        static bool[] _installRequired;

        static Settings _settings;

        [InitializeOnLoadMethod]
        static void CheckPackage()
        {
            _settings = new Settings();
            var settingsFiles = Directory.GetFiles(Application.dataPath, "PackageCheckerSettings.json",
                SearchOption.AllDirectories);
            if (settingsFiles.Length > 0)
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(settingsFiles[0]), _settings);
            }

            // if we dont have the scripting define, it means the check has not been done
            if (!CheckScriptingDefine(_settings.PackageCheckerScriptingDefine))
            {
                _packagesToAdd = new List<PackageEntry>();
                _clientList = null;
                _compatibleList = null;

                _packagesToAdd = new List<PackageEntry>();

                foreach (string line in _settings.PackagesToAdd)
                {
                    string[] split = line.Split('@');

                    // if no version is given, return null
                    PackageEntry entry = new PackageEntry
                        { Name = split[0], Version = split.Length > 1 ? split[1] : null };

                    _packagesToAdd.Add(entry);
                }

                // Create a file in library that is queried to see if CheckPackage() has been run already
                SetScriptingDefine(_settings.PackageCheckerScriptingDefine);

                // create a list of compatible packages for current engine version
                _compatibleList = Client.SearchAll();

                while (!_compatibleList.IsCompleted)
                {
                    if (_compatibleList.Status == StatusCode.Failure || _compatibleList.Error != null)
                    {
                        if (_compatibleList.Error != null)
                        {
                            Debug.LogError(_compatibleList.Error.message);
                            break;
                        }
                    }
                }

                // create a list of packages found in the engine
                _clientList = Client.List();

                while (!_clientList.IsCompleted)
                {
                    if (_clientList.Status == StatusCode.Failure || _clientList.Error != null)
                    {
                        if (_clientList.Error != null)
                        {
                            Debug.LogError(_clientList.Error.message);
                            break;
                        }
                    }
                }

                _addRequests = new AddRequest[_packagesToAdd.Count];
                _installRequired = new bool[_packagesToAdd.Count];

                // default new packages to install = false. we will mark true after validating they're required
                for (int i = 0; i < _installRequired.Length; i++)
                {
                    _installRequired[i] = false;
                }

                // build data collections compatible packages for this project, and packages within the project
                List<PackageInfo> compatiblePackages =
                    new List<PackageInfo>();
                List<PackageInfo> clientPackages =
                    new List<PackageInfo>();

                foreach (var result in _compatibleList.Result)
                {
                    compatiblePackages.Add(result);
                }

                foreach (var result in _clientList.Result)
                {
                    clientPackages.Add(result);
                }

                // check for the latest verified package version for each package that is missing a version
                for (int i = 0; i < _packagesToAdd.Count; i++)
                {
                    // if a version number is not provided
                    if (_packagesToAdd[i].Version == null)
                    {
                        foreach (var package in compatiblePackages)
                        {
#if UNITY_2022_2_OR_NEWER
                            // if no latest verified version found, PackageChecker will just install latest release
                            if (_packagesToAdd[i].Name == package.name && package.versions.recommended != string.Empty)
                            {
                                // add latest verified version number to the packagetoadd list version
                                // so that we get the latest verified version only
                                _packagesToAdd[i].Version = package.versions.recommended;

                                // add to our install list
                                _installRequired[i] = true;

                                //Debug.Log(string.Format("Requested {0}. Latest verified compatible package found: {1}",
                                //    packagesToAdd[i].name, packagesToAdd[i].version));
                            }
                        }
#else
                            // if no latest verified version found, PackageChecker will just install latest release
                            if (_packagesToAdd[i].Name == package.name && package.versions.verified != string.Empty)
                            {
                                // add latest verified version number to the packagetoadd list version
                                // so that we get the latest verified version only
                                _packagesToAdd[i].Version = package.versions.verified;

                                // add to our install list
                                _installRequired[i] = true;

                                //Debug.Log(string.Format("Requested {0}. Latest verified compatible package found: {1}",
                                //    packagesToAdd[i].name, packagesToAdd[i].version));
                            }
                        }
#endif
                    }

                    // we don't need to catch packages that are not installed as their latest version has been collected
                    // from the campatiblelist result
                    foreach (var package in clientPackages)
                    {
                        if (_packagesToAdd[i].Name == package.name)
                        {
                            // see what version we have installed
#if UNITY_2022_2_OR_NEWER
                            switch (CompareVersion(_packagesToAdd[i].Version, package.version))
                            {
                                // latest verified is ahead of installed version
                                case 1:
                                    _installRequired[i] = EditorUtility.DisplayDialog("Confirm Package Upgrade",
                                        $"The version of \"{_packagesToAdd[i].Name}\" in this project is {package.version}. The latest verified " +
                                        $"version is {_packagesToAdd[i].Version}. Would you like to upgrade it to the latest version? (Recommended)",
                                        "Yes", "No");

                                    Debug.Log(
                                        $"<b>Package version behind</b>: {package.packageId} is behind latest verified " +
                                        $"version {package.versions.recommended}. prompting user install");
                                    break;

                                // latest verified matches installed version
                                case 0:
                                    _installRequired[i] = false;

                                    Debug.Log(
                                        $"<b>Package version match</b>: {package.packageId} matches latest verified version " +
                                        $"{package.versions.recommended}. Skipped install");
                                    break;

                                // latest verified is behind installed version
                                case -1:
                                    _installRequired[i] = EditorUtility.DisplayDialog("Confirm Package Downgrade",
                                        $"The version of \"{_packagesToAdd[i].Name}\" in this project is {package.version}. The latest verified version is {_packagesToAdd[i].Version}. " +
                                        $"{package.version} is unverified. Would you like to downgrade it to the latest verified version? " +
                                        "(Recommended)", "Yes", "No");

                                    Debug.Log(
                                        $"<b>Package version ahead</b>: {package.packageId} is newer than latest verified " +
                                        $"version {package.versions.recommended}, skipped install");
                                    break;
                            }
#else
                            switch (CompareVersion(_packagesToAdd[i].Version, package.version))
                            {
                                // latest verified is ahead of installed version
                                case 1:
                                    _installRequired[i] = EditorUtility.DisplayDialog("Confirm Package Upgrade",
                                        $"The version of \"{_packagesToAdd[i].Name}\" in this project is {package.version}. The latest verified " +
                                        $"version is {_packagesToAdd[i].Version}. Would you like to upgrade it to the latest version? (Recommended)",
                                        "Yes", "No");

                                    Debug.Log(
                                        $"<b>Package version behind</b>: {package.packageId} is behind latest verified " +
                                        $"version {package.versions.verified}. prompting user install");
                                    break;

                                // latest verified matches installed version
                                case 0:
                                    _installRequired[i] = false;

                                    Debug.Log(
                                        $"<b>Package version match</b>: {package.packageId} matches latest verified version " +
                                        $"{package.versions.verified}. Skipped install");
                                    break;

                                // latest verified is behind installed version
                                case -1:
                                    _installRequired[i] = EditorUtility.DisplayDialog("Confirm Package Downgrade",
                                        $"The version of \"{_packagesToAdd[i].Name}\" in this project is {package.version}. The latest verified version is {_packagesToAdd[i].Version}. " +
                                        $"{package.version} is unverified. Would you like to downgrade it to the latest verified version? " +
                                        "(Recommended)", "Yes", "No");

                                    Debug.Log(
                                        $"<b>Package version ahead</b>: {package.packageId} is newer than latest verified " +
                                        $"version {package.versions.verified}, skipped install");
                                    break;
                            }
#endif
                        }
                    }
                }

                // fixing bug with incompatiblity of cinemachine 2.6.14 and inputsystem 1.3.0
                string cinemachineVersion = "";
                string inputSystemVersion = "";
                int idxOfCinemachinePackage = -1;
                for (int i = 0; i < _packagesToAdd.Count; i++)
                {
                    if (_packagesToAdd[i].Name.Equals("com.unity.cinemachine"))
                    {
                        cinemachineVersion = _packagesToAdd[i].Version;
                        idxOfCinemachinePackage = i;
                    }
                    else if (_packagesToAdd[i].Name.Equals("com.unity.inputsystem"))
                    {
                        inputSystemVersion = _packagesToAdd[i].Version;
                    }
                }

                if (idxOfCinemachinePackage != -1 && cinemachineVersion == "2.6.14" && inputSystemVersion == "1.3.0")
                {
                    _packagesToAdd[idxOfCinemachinePackage].Version = "2.8.6";
                }

                // install our packages and versions
                for (int i = 0; i < _packagesToAdd.Count; i++)
                {
                    if (_installRequired[i])
                    {
                        _addRequests[i] = InstallSelectedPackage(_packagesToAdd[i].Name, _packagesToAdd[i].Version);
                    }
                }

                ReimportPackagesByKeyword();
            }
        }

        static AddRequest InstallSelectedPackage(string packageName, string packageVersion)
        {
            if (packageVersion != null)
            {
                packageName = packageName + "@" + packageVersion;
                Debug.Log($"<b>Adding package</b>: {packageName}");
            }

            AddRequest newPackage = Client.Add(packageName);

            while (!newPackage.IsCompleted)
            {
                if (newPackage.Status == StatusCode.Failure || newPackage.Error != null)
                {
                    if (newPackage.Error != null)
                    {
                        Debug.LogError(newPackage.Error.message);
                        return null;
                    }
                }
            }

            return newPackage;
        }

        static void ReimportPackagesByKeyword()
        {
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(_settings.EditorFolderRoot, ImportAssetOptions.ImportRecursive);
        }

        public static int CompareVersion(string latestVerifiedVersion, string projectVersion)
        {
            string[] latestVersionSplit = latestVerifiedVersion.Split('.');
            string[] projectVersionSplit = projectVersion.Split('.');
            int iteratorA = 0;
            int iteratorB = 0;

            while (iteratorA < latestVersionSplit.Length || iteratorB < projectVersionSplit.Length)
            {
                int latestVerified = 0;
                int installed = 0;

                if (iteratorA < latestVersionSplit.Length)
                {
                    latestVerified = Convert.ToInt32(latestVersionSplit[iteratorA]);
                }

                if (iteratorB < projectVersionSplit.Length)
                {
                    installed = Convert.ToInt32(projectVersionSplit[iteratorB]);
                }

                // latest verified is ahead of installed version
                if (latestVerified > installed) return 1;

                // latest verified is behind installed version
                if (latestVerified < installed) return -1;

                iteratorA++;
                iteratorB++;
            }

            // if the version is the same
            return 0;
        }


        static bool CheckScriptingDefine(string scriptingDefine)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Contains(scriptingDefine);
        }

        static void SetScriptingDefine(string scriptingDefine)
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!defines.Contains(scriptingDefine))
            {
                defines += $";{scriptingDefine}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            }
        }

        public static void RemovePackageCheckerScriptingDefine()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (defines.Contains(_settings.PackageCheckerScriptingDefine))
            {
                string newDefines = defines.Replace(_settings.PackageCheckerScriptingDefine, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
            }
        }
    }
}