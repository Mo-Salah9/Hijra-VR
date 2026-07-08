using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CrizGames.Vour.Editor
{
    public struct PackageIdUrl
    {
        public string ID;
        public string VersionOrURL;

        public static implicit operator PackageIdUrl(string p)
        {
            var split = p.Split(':');
            if (split.Length == 1)
            {
                Debug.LogError($"Package {split[0]} has no version specified!");
                return new PackageIdUrl
                {
                    ID = split[0].Trim(),
                    VersionOrURL = "1.0.0"
                };
            } 
            else
            {
                return new PackageIdUrl
                {
                    ID = split[0].Trim(),
                    VersionOrURL = split[1].Trim()
                };
            }
        }
    }
    
    /// <summary>
    /// Install and uninstall packages
    /// Based on XRPackageMetadataStore from XR Plugin Management
    /// </summary>
    [InitializeOnLoad]
    public class VourPackageManager
    {
        private const string k_RebuildCache = "Vour Setup Rebuilding Cache";
        private const string k_CachedMDStoreKey = "Vour Setup Metadata Store";

        private static float k_TimeOutDelta = 30f;

        private static readonly string ManifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        [Serializable]
        private struct CachedPackageInfo
        {
            public PackageInfo[] installedPackages;
        }

        private static CachedPackageInfo _cachedPackageInfo = new CachedPackageInfo()
        {
            installedPackages = {}
        };

        private static void LoadCachedMDStoreInformation()
        {
            string data = SessionState.GetString(k_CachedMDStoreKey, "{}");
            _cachedPackageInfo = JsonUtility.FromJson<CachedPackageInfo>(data);
        }

        private static void StoreCachedMDStoreInformation()
        {
            SessionState.EraseString(k_CachedMDStoreKey);
            string data = JsonUtility.ToJson(_cachedPackageInfo, true);
            SessionState.SetString(k_CachedMDStoreKey, data);
        }

        private enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        [Serializable]
        private struct PackageRequest
        {
            [SerializeField]
            public string packageId;
            [SerializeField]
            public ListRequest packageListRequest;
            [SerializeField]
            public float timeOut;
            [SerializeField]
            public string logMessage;
            [SerializeField]
            public LogLevel logLevel;
        }

        [Serializable]
        private struct PackageRequests
        {
            [SerializeField]
            public List<PackageRequest> activeRequests;
        }

        private const string k_DefaultSessionStateString = "DEFAULTSESSION";

        private static bool SessionStateHasStoredData(string queueName)
        {
            return SessionState.GetString(queueName, k_DefaultSessionStateString) != k_DefaultSessionStateString;
        }

        public static bool IsRebuildingCache => SessionStateHasStoredData(k_RebuildCache);

        public static bool IsPackageInstalled(PackageIdUrl package)
        {
            return _cachedPackageInfo.installedPackages?.Any(x => x.name == package.ID) ?? false;
        }
        
        public static bool AllPackagesInstalled(params PackageIdUrl[] packages)
        {
            return packages.All(IsPackageInstalled);
        }

        public static AddAndRemoveRequest InstallPackages(params PackageIdUrl[] packages)
        {
            var request = Client.AddAndRemove(packagesToAdd: packages.Select(x => $"{x.ID}@{x.VersionOrURL}").ToArray());
            return ProcessAddAndRemoveRequest(request, "Installing packages...", "Error trying to install packages");
        }
        
        public static AddAndRemoveRequest UninstallPackages(params PackageIdUrl[] packages)
        {
            var request = Client.AddAndRemove(packagesToRemove: packages.Select(x => $"{x.ID}").ToArray());
            return ProcessAddAndRemoveRequest(request, "Uninstalling packages...", "Error trying to uninstall packages");
        }

        private static AddAndRemoveRequest ProcessAddAndRemoveRequest(AddAndRemoveRequest request, string infoTitle, string errorBeginning)
        {
            EditorUtility.DisplayProgressBar("Vour Setup", infoTitle, 0);
            EditorApplication.update += ShowProgress;
            return request;

            void ShowProgress()
            {
                if (!request.IsCompleted && request.Error == null && request.Status != StatusCode.Failure) 
                    return;
                
                if (request.Error != null)
                    Debug.LogError($"{errorBeginning}: {request.Error.errorCode} - {request.Error.message}");
                
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= ShowProgress;
            }
        }
        
        public static bool TryImportSamples((string package, string sampleDisplayName)[] samples)
        {
            var samplesInstalled = true;
            foreach (var (package, sampleDisplayName) in samples)
            {
                if (!TryImportSample(package, sampleDisplayName))
                    samplesInstalled = false;
            }

            return samplesInstalled;
        }

        public static bool TryImportSample(string package, string sampleDisplayName)
        {
            var packageInfo = GetPackageInfo(package);
            if (packageInfo == null)
                return false;

            try
            {
                var packageSamples = Sample.FindByPackage(package, packageInfo.version);

                var sample = packageSamples.First(x => x.displayName == sampleDisplayName);
                sample.Import(Sample.ImportOptions.OverridePreviousImports | Sample.ImportOptions.HideImportWindow);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Couldn't find samples of the {package} {packageInfo.version} package. Exception: {e}");
                return false;
            }
        }

        public static bool IsSampleImported(string package, string sampleDisplayName)
        {
            var packageInfo = GetPackageInfo(package);
            if (packageInfo == null)
                return false;
            
            var sampleDestinationPath = Path.Combine(Application.dataPath, "Samples", packageInfo.displayName, packageInfo.version, sampleDisplayName);

            return Directory.Exists(sampleDestinationPath);
        }

        private static PackageInfo GetPackageInfo(string package)
        {
            return _cachedPackageInfo.installedPackages?.FirstOrDefault(x => x.name == package);
        }

        public static string GetCurrentStatusDisplayText()
        {
            if (IsRebuildingCache)
                return "Querying for installed packages...";

            return "";
        }

        static VourPackageManager()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;

            if (IsEditorInPlayMode())
                return;

            AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
        }

        private static void Refresh()
        {
            if (!IsRebuildingCache)
                AfterAssemblyReload();
        }

        private static void AfterAssemblyReload()
        {
            LoadCachedMDStoreInformation();

            if (!IsEditorInPlayMode())
            {
                RebuildInstalledCache();
                StartAllQueues();
            }
        }

        private static bool IsEditorInPlayMode()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isPlaying ||
                EditorApplication.isPaused;
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    StopAllQueues();
                    StoreCachedMDStoreInformation();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    LoadCachedMDStoreInformation();
                    StartAllQueues();
                    break;
            }
        }

        private static void StopAllQueues()
        {
            EditorApplication.update -= RebuildCache;
        }

        private static void StartAllQueues()
        {
            EditorApplication.update += RebuildCache;
        }

        private static void AddRequestToQueue(PackageRequest request, string queueName)
        {
            PackageRequests reqs;

            if (SessionStateHasStoredData(queueName))
            {
                string fromJson = SessionState.GetString(queueName, k_DefaultSessionStateString);
                reqs = JsonUtility.FromJson<PackageRequests>(fromJson);
            }
            else
            {
                reqs = new PackageRequests();
                reqs.activeRequests = new List<PackageRequest>();
            }

            reqs.activeRequests.Add(request);
            string json = JsonUtility.ToJson(reqs);
            SessionState.SetString(queueName, json);
        }

        private static void SetRequestsInQueue(PackageRequests reqs, string queueName)
        {
            string json = JsonUtility.ToJson(reqs);
            SessionState.SetString(queueName, json);
        }

        private static PackageRequests GetAllRequestsInQueue(string queueName)
        {
            var reqs = new PackageRequests();
            reqs.activeRequests = new List<PackageRequest>();

            if (SessionStateHasStoredData(queueName))
            {
                string fromJson = SessionState.GetString(queueName, k_DefaultSessionStateString);
                reqs = JsonUtility.FromJson<PackageRequests>(fromJson);
                SessionState.EraseString(queueName);
            }

            return reqs;
        }

        public static void RebuildInstalledCache()
        {
            if (IsRebuildingCache)
                return;

            var req = new PackageRequest();
            req.packageListRequest = Client.List(true, true);
            req.timeOut = Time.realtimeSinceStartup + k_TimeOutDelta;
            AddRequestToQueue(req, k_RebuildCache);
            EditorApplication.update += RebuildCache;
        }

        private static void RebuildCache()
        {
            EditorApplication.update -= RebuildCache;

            if (IsEditorInPlayMode())
                return; // Use the cached data that should have been passed in the play state change.

            PackageRequests reqs = GetAllRequestsInQueue(k_RebuildCache);

            if (reqs.activeRequests == null || reqs.activeRequests.Count == 0)
                return;

            var req = reqs.activeRequests[0];
            reqs.activeRequests.Remove(req);

            if (req.timeOut < Time.realtimeSinceStartup)
            {
                req.logMessage = $"Timeout trying to get package list after {k_TimeOutDelta}s.";
                req.logLevel = LogLevel.Warning;
                Log(req);
                Refresh();
            }
            else if (req.packageListRequest.IsCompleted)
            {
                if (req.packageListRequest.Status == StatusCode.Success)
                {
                    var installedPackages = new List<PackageInfo>();

                    foreach (var packageInfo in req.packageListRequest.Result)
                    {
                        installedPackages.Add(packageInfo);
                        //Debug.Log(JsonUtility.ToJson(packageInfo, true));
                    }
                    
                    _cachedPackageInfo.installedPackages = installedPackages.ToArray();
                }

                StoreCachedMDStoreInformation();
            }
            else if (!req.packageListRequest.IsCompleted)
            {
                AddRequestToQueue(req, k_RebuildCache);
                EditorApplication.update += RebuildCache;
            }
            else
            {
                req.logMessage = "Unable to rebuild installed package cache. Some state may be missing or incorrect.";
                req.logLevel = LogLevel.Warning;
                Log(req);
            }

            if (reqs.activeRequests.Count > 0)
            {
                SetRequestsInQueue(reqs, k_RebuildCache);
                EditorApplication.update += RebuildCache;
            }
        }

        private static void Log(PackageRequest req)
        {
            /*const string header = "Vour";
            switch(req.logLevel)
            {
                case LogLevel.Info:
                    Debug.Log($"{header}: {req.logMessage}");
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning($"{header} Warning: {req.logMessage}");
                    break;

                case LogLevel.Error:
                    Debug.LogError($"{header} error. Failure reason: {req.logMessage}.\n Check if there are any other errors in the console and make sure they are corrected before trying again.");
                    break;
            }*/
        }
    }
}