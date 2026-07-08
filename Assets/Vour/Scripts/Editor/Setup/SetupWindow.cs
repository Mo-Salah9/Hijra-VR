using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine.Rendering;
#if VOUR_OPENXR
using UnityEditor.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEngine.XR.Hands.OpenXR;
#endif

namespace CrizGames.Vour.Editor
{
    public class SetupWindow : EditorWindow
    {
        private enum FeatureState
        {
            NotSupported,
            NotInstalled,
            Installed,
        }
        
        private class EditorFeature
        {
            public bool IsEnabled
            {
                get => IsEnabledPredicate();
                set => SetIsEnabled(value);
            }
            public bool IsSupported => IsSupportedPredicate();
            public bool IsInstalled => IsInstalledPredicate();

            public string Name;
            
            public string NotSupportedReason;

            public Func<bool> IsEnabledPredicate;
            public Action<bool> SetIsEnabled;
            
            public Func<bool> IsSupportedPredicate;
            
            public Func<bool> IsInstalledPredicate;

            public Action InstallAction;
        }

        private class OptionalFeature
        {
            public string Name;
            public BuildTargetGroup BuildTargetGroup;
            public BuildTarget[] BuildTargets;
            public PackageIdUrl[] Packages;
            public (string package, string sampleDisplayName)[] Samples;
            public string XRLoader;
            public string XRLoaderSettingsPage;

            public Func<bool> CustomSetupFunc;

            public int GetSupportedBuildTarget()
            {
                for (var i = 0; i < BuildTargets.Length; i++)
                    if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup, BuildTargets[i]))
                        return i;
                return -1;
            }

            public bool IsFeatureInstalled()
            {
                return VourPackageManager.AllPackagesInstalled(Packages) 
                       && Samples.All(tuple =>  VourPackageManager.IsSampleImported(tuple.package, tuple.sampleDisplayName));
            }

            public void Install()
            {
                if (VourPackageManager.AllPackagesInstalled(Packages))
                    return;
                    
                var req = VourPackageManager.InstallPackages(Packages);

                EditorApplication.LockReloadAssemblies();
                EditorApplication.update += WaitForPackagesInstalled;
                return;
                
                void WaitForPackagesInstalled()
                {
                    if (!req.IsCompleted && req.Error == null && req.Status != StatusCode.Failure) 
                        return;
                    
                    EditorApplication.update -= WaitForPackagesInstalled;
                    
                    if (req.IsCompleted)
                        EditorApplication.update += ImportSamples;
                    else
                        EditorApplication.UnlockReloadAssemblies();
                }
            }

            private void ImportSamples()
            {
                EditorApplication.update -= ImportSamples;

                if (Samples.Length == 0)
                {
                    EditorApplication.UnlockReloadAssemblies();
                    return;
                }

                if (!VourPackageManager.AllPackagesInstalled(Packages))
                {
                    VourPackageManager.RebuildInstalledCache();
                    EditorApplication.update += ImportSamples;
                    return;
                }
                
                EditorUtility.DisplayDialog($"Install Feature: {Name}",
                    "A few sample assets from the installed packages will be imported. They are necessary for the Vour feature to function properly. Please do not delete them unless you know what you are doing.",
                    "Ok", null);

                if (VourPackageManager.TryImportSamples(Samples))
                    EditorApplication.UnlockReloadAssemblies();
                else
                    EditorApplication.update += ImportSamples;
            }

            public bool IsLoaderEnabled() => EditorTools.IsXRLoaderEnabled(XRLoader, BuildTargetGroup);

            public void EnableAndConfigureLoader()
            {
                EditorApplication.update -= EnableAndConfigureLoader;

#if VOUR_XRPLUGINMANAGEMENT
                // Make sure settings for this platform are available
                var xrSettings = EditorTools.GetOrCreateXRSettingsForBuildTarget();
                if (!xrSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup))
                    xrSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup);
#endif
                var loaderSet = EditorTools.SetXRLoader(XRLoader, BuildTargetGroup, true);

                var customSetupResult = CustomSetupFunc?.Invoke() ?? true;

                if (!loaderSet || !customSetupResult)
                    EditorApplication.update += EnableAndConfigureLoader;
            }

            public void DisableLoader()
            {
                EditorTools.SetXRLoader(XRLoader, BuildTargetGroup, false);
            }
        }

        private const string PackageXRManagement = "com.unity.xr.management:4.5.0";
        private const string PackageXRInteractionToolkit = "com.unity.xr.interaction.toolkit:3.1.1";
        private const string PackageOpenXR = "com.unity.xr.openxr:1.14.1";
        private const string PackageXRHands = "com.unity.xr.hands:1.5.0";

        public static readonly PackageIdUrl[] RequiredPackages =
        {
            "com.unity.inputsystem:1.11.2",
            "com.unity.xr.core-utils:2.5.1",
        };

        private static readonly OptionalFeature[] OptionalFeatures =
        {
            new OptionalFeature
            {
                Name = "Desktop VR",
                BuildTargetGroup = BuildTargetGroup.Standalone,
                BuildTargets = new[]
                {
                    BuildTarget.StandaloneWindows64, 
                    BuildTarget.StandaloneWindows, 
                    BuildTarget.StandaloneOSX,
                    BuildTarget.StandaloneLinux64
                },
                Packages = new PackageIdUrl[]
                {
                    PackageXRManagement,
                    PackageOpenXR,
                    PackageXRInteractionToolkit,
                    PackageXRHands,
                },
                Samples = new[]
                {
                    ("com.unity.xr.hands", "HandVisualizer"),
                    ("com.unity.xr.interaction.toolkit", "Starter Assets"),
                    ("com.unity.xr.interaction.toolkit", "Hands Interaction Demo"),
                },
                XRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader",
                XRLoaderSettingsPage = "Project/XR Plug-in Management/OpenXR",
                CustomSetupFunc = () =>
                {
#if VOUR_OPENXR
                    // Minimal setup that the user can expand later manually
                    var features = new[]
                    {
                        typeof(KHRSimpleControllerProfile),
                    };

                    return EnableFeatures(BuildTargetGroup.Standalone, features) 
                           && FixAllAutomaticValidationErrors(BuildTargetGroup.Standalone);
#else
                    return false;
#endif
                }
            },
            new OptionalFeature
            {
                Name = "Meta Quest",
                BuildTargetGroup = BuildTargetGroup.Android,
                BuildTargets = new[] { BuildTarget.Android },
                Packages = new PackageIdUrl[]
                {
                    PackageXRManagement,
                    PackageOpenXR,
                    PackageXRInteractionToolkit,
                    PackageXRHands,
                },
                Samples = new[]
                {
                    ("com.unity.xr.hands", "HandVisualizer"),
                    ("com.unity.xr.interaction.toolkit", "Starter Assets"),
                    ("com.unity.xr.interaction.toolkit", "Hands Interaction Demo"),
                },
                XRLoader = "UnityEngine.XR.OpenXR.OpenXRLoader",
                XRLoaderSettingsPage = "Project/XR Plug-in Management/OpenXR",
                CustomSetupFunc = () =>
                {
#if VOUR_OPENXR
                    // Use Vulkan only, as there have been memory leaks by unloading videos with OpenGLES3 in the past
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
                    
                    var features = new[]
                    {
                        typeof(MetaQuestFeature),
                        typeof(HandTracking),
                        typeof(MetaHandTrackingAim),
                        typeof(HandInteractionProfile),
                        typeof(OculusTouchControllerProfile),
                        typeof(MetaQuestTouchPlusControllerProfile)
                    };

                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                    if (settings == null || !EnableFeatures(settings, features))
                        return false;

#if VOUR_URP
                    // Set single-pass instanced (multi-view) rendering for URP
                    settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
#else
                    // Set multi-pass rendering for BRP.
                    // Shader Graph doesn't support single-pass instanced rendering on BRP,
                    // which cause XRI hand rendering issues
                    // https://docs.unity3d.com/6000.0/Documentation/Manual/SinglePassStereoRendering.html
                    settings.renderMode = OpenXRSettings.RenderMode.MultiPass;
#endif
                    return FixAllAutomaticValidationErrors(BuildTargetGroup.Android);
#else
                    return false;
#endif
                }
            },
            new OptionalFeature
            {
                Name = "Web XR",
                BuildTargetGroup = BuildTargetGroup.WebGL,
                BuildTargets = new[] { BuildTarget.WebGL },
                Packages = new PackageIdUrl[]
                {
                    PackageXRManagement,
                    PackageXRInteractionToolkit,
                    PackageXRHands,
                    "com.unity.burst:1.8.18",
                    "com.unity.render-pipelines.universal:17.0.3",
                    new() { ID = "com.de-panther.webxr", VersionOrURL = "https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr#webxr/0.22.1" },
                    new() { ID = "com.de-panther.webxr-interactions", VersionOrURL = "https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr-interactions#webxr-interactions/0.22.0" },
                    new() { ID = "com.de-panther.webxr-input-profiles-loader", VersionOrURL = "https://github.com/De-Panther/webxr-input-profiles-loader.git?path=/Packages/webxr-input-profiles-loader#0.6.2" },
                },
                Samples = new[]
                {
                    ("com.unity.xr.hands", "HandVisualizer"),
                    ("com.unity.xr.interaction.toolkit", "Starter Assets"),
                    ("com.unity.xr.interaction.toolkit", "Hands Interaction Demo"),
                    ("com.de-panther.webxr-interactions", "XR Interaction Toolkit Sample")
                },
                XRLoader = "WebXR.WebXRLoader",
                XRLoaderSettingsPage = "Project/XR Plug-in Management/WebXR",
                CustomSetupFunc = () =>
                {
                    var urpAssetGuids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
                    var urpAssetPath = urpAssetGuids
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == "VourWebXRPipelineAsset");

                    if (string.IsNullOrEmpty(urpAssetPath))
                    {
                        Debug.LogError("Unable to find VourWebXRPipelineAsset. Please assign the pipeline asset it in the graphics settings manually.");
                        return false;
                    }

                    if (GraphicsSettings.defaultRenderPipeline == null)
                    {
                        if (!EditorUtility.DisplayDialog("Enable Feature: WebXR",
                                "For WebXR to work correctly, the project will switch to the Universal Render Pipeline.",
                                "Ok", "Cancel"))
                            return false;
                    }

                    // Set URP asset
                    var renderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(urpAssetPath);
                    GraphicsSettings.defaultRenderPipeline = renderPipeline;
                    
                    // Set render pipeline for quality level of WebGL in case there was some other URP asset defined,
                    // which can happen when you use the Unity URP project template.
                    const string buildTargetGroupName = nameof(BuildTargetGroup.WebGL);
                    for (int i = 0; i < 16; i++)
                    {
                        if (!QualitySettings.IsPlatformIncluded(buildTargetGroupName, i)) 
                            continue;
                        
                        QualitySettings.SetQualityLevel(i, false);
                        QualitySettings.renderPipeline = renderPipeline;
                        break;
                    }

                    // Set required color space
                    // https://github.com/De-Panther/unity-webxr-export/blob/master/Packages/webxr/Editor/WebXRBuildProcessor.cs#L101
#if UNITY_6000_0_OR_NEWER
                    PlayerSettings.colorSpace = ColorSpace.Linear;
#else
                    PlayerSettings.colorSpace = ColorSpace.Gamma;
#endif
                    
                    // Open XR plugin settings
                    EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.WebGL;
                    SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Project Validation");
                    return true;
                }
            }
        };

        private static readonly EditorFeature[] EditorFeatures =
        {
            new EditorFeature
            {
                Name = "Video Preview",
                IsSupportedPredicate = () => FFmpegInstaller.IsSupported,
                NotSupportedReason = "Only Windows is currently supported.",
#if VOUR_SETTINGS
                IsEnabledPredicate = () => VourSettings.Instance.enableVideoPreview,
                SetIsEnabled = value => VourSettings.Instance.enableVideoPreview = value,
#else
                IsEnabledPredicate = () => false,
                SetIsEnabled = _ => {},
#endif
                IsInstalledPredicate = () => FFmpegInstaller.IsFFmpegInstalled,
                InstallAction = () =>
                {
                    if (EditorUtility.DisplayDialog("Enable Feature: Video Preview",
                            "For the video preview to work, FFmpeg will be downloaded locally for the project. Do you want to continue?",
                            "Yes", "No"))
                    {
                        FFmpegInstaller.InstallFFmpeg();
                    }
                }
            }
        };

        private const string WindowTitle = "Vour Setup Window";
        private const string Title = "Vour Setup";

        private const float WindowWidth = 300f;
        private const float HeaderSizeX = 150;

        private const int WindowPadding = 20;

        private static GUIStyle HeaderLabelStyle => new GUIStyle(GUI.skin.label)
            { fontStyle = FontStyle.Bold, fontSize = 24, alignment = TextAnchor.MiddleCenter };

        private bool _setupRequired;

        private Texture2D _headerImg;
        private Vector2 _headerSize;

        private ReorderableList _runtimeFeatureList;
        private ReorderableList _editorFeatureList;

        public static void ShowWindow()
        {
            var window = GetWindow<SetupWindow>(true, WindowTitle, true);
            window.ShowUtility();
        }

        private void Init()
        {
            if (_headerImg == null)
            {
                _headerImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Vour/Textures/Editor/VourEditorLogo.png");
                _headerSize = new Vector2(HeaderSizeX, _headerImg.height * (HeaderSizeX / _headerImg.width));
            }

            CreateRuntimeFeatureList();
            CreateEditorFeatureList();

            _setupRequired = !VourPackageManager.AllPackagesInstalled(RequiredPackages);
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnGUI()
        {
            if (_setupRequired)
                _setupRequired = !VourPackageManager.AllPackagesInstalled(RequiredPackages);

            var headerRect = new Rect(position.width / 2 - _headerSize.x / 2, 30, _headerSize.x, _headerSize.y);
            GUI.DrawTexture(headerRect, _headerImg);
            GUILayout.Space(_headerSize.y + 40);
            GUILayout.Label(Title, HeaderLabelStyle);
            GUILayout.Space(WindowPadding);

            GUILayout.BeginHorizontal();
            GUILayout.Space(WindowPadding);
            GUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(EditorTools.IsEditorInPlayMode());
            DrawContent();
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            GUILayout.Space(WindowPadding);
            GUILayout.EndHorizontal();

            SetWindowSize();
        }

        private void DrawContent()
        {
            var windowWidth = position.width;
            var windowWidthPadded = windowWidth - WindowPadding * 2;

            if (_setupRequired)
            {
                var rect = GUILayoutUtility.GetRect(windowWidthPadded, 24);
                if (GUI.Button(rect, "Install required dependencies"))
                    VourPackageManager.InstallPackages(RequiredPackages);
                GUILayout.Space(10);
            }

            _runtimeFeatureList!.DoLayoutList();
            _editorFeatureList!.DoLayoutList();
        }

        private void CreateRuntimeFeatureList()
        {
            var title = new GUIContent("Optional Features");
            _runtimeFeatureList = CreateFeatureListBase(OptionalFeatures, title, DrawFeatureCallback);
            return;

            void DrawFeatureCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                var feature = OptionalFeatures[index];

                var isFeatureInstalled = feature.IsFeatureInstalled();

                var availableTargetIdx = feature.GetSupportedBuildTarget();
                var isGroupAvailable = availableTargetIdx >= 0;
                
                Action<bool> onToggle = value =>
                {
                    if (value)
                    {
                        feature.EnableAndConfigureLoader();
                    }
                    else
                    {
                        // Uninstall
                        // var choice = EditorUtility.DisplayDialog("Uninstall Packages", "Are you sure you want to uninstall the packages for this feature?", "Yes", "Cancel");
                        // if (choice)
                        //     VourPackageManager.UninstallPackages(FilterOutSharedPackages(li).Select(p => p.id));

                        // Disable loader
                        feature.DisableLoader();
                    }
                };

                Action onInstall = () => feature.Install();
                
                Action onHelpClick = () =>
                {
                    // Open BuildPlayerWindow with buildTargetGroup selected
                    BuildPlayerWindow.ShowBuildPlayerWindow();
                    EditorUserBuildSettings.selectedBuildTargetGroup = feature.BuildTargetGroup;
                };
                
                var settingsTooltip = "XR Loader Settings";
                Action onSettingsClick = () =>
                {
                    EditorUserBuildSettings.selectedBuildTargetGroup = feature.BuildTargetGroup;
                    SettingsService.OpenProjectSettings(feature.XRLoaderSettingsPage);
                };

                var state = FeatureState.Installed;
                if (!isGroupAvailable)
                    state = FeatureState.NotSupported;
                else if (!isFeatureInstalled)
                    state = FeatureState.NotInstalled;
                
                var disableAll = _setupRequired || VourPackageManager.IsRebuildingCache;
                var enabled = isFeatureInstalled && feature.IsLoaderEnabled();
                const string notSupportedReason = "The required platform module is not installed.";
                
                DrawFeature(rect, disableAll, feature.Name, state, enabled, notSupportedReason, onToggle, onInstall, onHelpClick, onSettingsClick, settingsTooltip);
            }
        }

        private void CreateEditorFeatureList()
        {
            var title = new GUIContent("Optional Editor Features");
            _editorFeatureList = CreateFeatureListBase(EditorFeatures, title, DrawFeatureCallback);

            // Status is for runtime features but I want it to be placed at the bottom of the window so it is here
            _editorFeatureList.drawFooterCallback = rect =>
            {
                var status = VourPackageManager.GetCurrentStatusDisplayText();
                GUI.Label(rect, status, EditorStyles.label);
            };
            return;

            void DrawFeatureCallback(Rect rect, int index, bool isActive, bool isFocused)
            {
                var feature = EditorFeatures[index];

                var state = FeatureState.Installed;
                if (!feature.IsSupported)
                    state = FeatureState.NotSupported;
                else if (!feature.IsInstalled)
                    state = FeatureState.NotInstalled;
                
                var disableAll = _setupRequired || VourPackageManager.IsRebuildingCache || !feature.IsSupported;
                DrawFeature(rect, disableAll, feature.Name, state, feature.IsEnabled, feature.NotSupportedReason, feature.SetIsEnabled, feature.InstallAction);
            }
        }
        
        private void DrawFeature(
            Rect rect, 
            bool grayOut, 
            string name, 
            FeatureState state, 
            bool enabled, 
            string notSupportedReason, 
            Action<bool> onToggle, 
            Action installAction, 
            Action helpClick = null, 
            Action settingsClick = null, 
            string settingsTooltip = null)
        {
            // Disable if setup required or rebuilding cache
            EditorGUI.BeginDisabledGroup(grayOut);
            
            if (state == FeatureState.NotSupported)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.Label(rect, name);
                EditorGUI.EndDisabledGroup();
                
                var labelSize = EditorStyles.label.CalcSize(new GUIContent(name));
                
                // Info icon
                var infoButtonRect = new Rect(rect.x + labelSize.x + 4, rect.y, rect.width - labelSize.x, rect.height);
                var content = new GUIContent(EditorGUIUtility.IconContent("_Help@2x").image, notSupportedReason);
                if(GUI.Button(infoButtonRect, content, EditorStyles.label))
                    helpClick?.Invoke();
            }
            else if (state == FeatureState.NotInstalled)
            {
                GUI.Label(rect, name);
                
                // Install button
                var buttonSize = EditorStyles.miniButton.CalcSize(new GUIContent("Install"));
                var buttonRect = new Rect(rect.x + rect.width - buttonSize.x, rect.y + 2, buttonSize.x, buttonSize.y);
                if (GUI.Button(buttonRect, "Install"))
                    installAction();
            }
            else
            {
                // Settings button
                if (enabled && settingsClick != null)
                {
                    var content = new GUIContent(EditorGUIUtility.IconContent("_Popup@2x").image, settingsTooltip);
                    var buttonSize = EditorStyles.iconButton.CalcSize(content);
                    var buttonRect = new Rect(rect.x + rect.width - buttonSize.x, rect.y + 2, buttonSize.x, buttonSize.y);
                    if(GUI.Button(buttonRect, content, EditorStyles.label))
                        settingsClick();

                    rect.width -= buttonSize.x;
                }
                
                // Toggle
                if (EditorGUI.ToggleLeft(rect, name, enabled) != enabled)
                    onToggle(!enabled);
            }

            EditorGUI.EndDisabledGroup();
        }

        private ReorderableList CreateFeatureListBase<T>(T[] elements, GUIContent title, ReorderableList.ElementCallbackDelegate drawElementCallback)
        {
            var list = new ReorderableList(elements, typeof(T), false, true, false, false);
            list.drawHeaderCallback = rect =>
            {
                var labelSize = EditorStyles.label.CalcSize(title);
                var labelRect = new Rect(rect);
                labelRect.width = labelSize.x;

                EditorGUI.LabelField(labelRect, title, EditorStyles.label);
            };
            list.drawElementCallback = drawElementCallback;
            list.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                var tex = GUI.skin.label.normal.background;
                if (tex == null && GUI.skin.label.normal.scaledBackgrounds.Length > 0)
                    tex = GUI.skin.label.normal.scaledBackgrounds[0];
                if (tex == null) return;

                GUI.DrawTexture(rect, GUI.skin.label.normal.background);
            };
            list.elementHeightCallback = i => list.elementHeight;
            return list;
        }

        private void SetWindowSize()
        {
            var windowHeight = (30 + _headerSize.y + 40) + WindowPadding * 2 
                                   + (_runtimeFeatureList.elementHeight * _runtimeFeatureList.count + 36) 
                                   + (_editorFeatureList.elementHeight * _editorFeatureList.count + 48);
            if (_setupRequired) windowHeight += 34;
            if (!string.IsNullOrEmpty(VourPackageManager.GetCurrentStatusDisplayText())) windowHeight += 16;

            minSize = maxSize = new Vector2(WindowWidth, windowHeight);
        }

#if VOUR_OPENXR
        private static bool EnableFeatures(BuildTargetGroup targetGroup, Type[] features)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
            return settings != null && EnableFeatures(settings, features);
        }

        private static bool EnableFeatures(OpenXRSettings settings, Type[] features)
        {
            // Enable features
            var enabledAllFeatures = true;
            foreach (var featureType in features)
            {
                var feature = settings.GetFeature(featureType);
                if (feature != null)
                    feature.enabled = true;
                else
                    enabledAllFeatures = false;
            }
            
            return enabledAllFeatures;
        }
        
        
        private static bool FixAllAutomaticValidationErrors(BuildTargetGroup targetGroup)
        {
            var xrErrors = GetValidationErrors(targetGroup);
            FixValidationIssues(xrErrors);
            return xrErrors.Count == 0;
        }
        
        private static void FixValidationIssues(List<OpenXRFeature.ValidationRule> issues)
        {
            foreach (var issue in issues)
            {
                if (issue.fixItAutomatic)
                    issue.fixIt();
            }
        }
        
        private static List<OpenXRFeature.ValidationRule> GetValidationErrors(BuildTargetGroup targetGroup)
        {
            return GetValidationIssues(targetGroup).Where(x => x.error).ToList();
        }
        
        private static List<OpenXRFeature.ValidationRule> GetValidationIssues(BuildTargetGroup targetGroup)
        {
            var issues = new List<OpenXRFeature.ValidationRule>();
            OpenXRProjectValidation.GetCurrentValidationIssues(issues, targetGroup);
            return issues;
        }
#endif

        /// <summary>
        /// Filter out packages shared with other installed features
        /// </summary>
        private PackageIdUrl[] FilterOutSharedPackages(OptionalFeature feature)
        {
            var installedFeatures = OptionalFeatures.Where(f => f != feature && f.IsFeatureInstalled());
            var packages = installedFeatures.Select(f => f.Packages).SelectMany(x => x).Distinct();
            return feature.Packages.Where(p => !packages.Contains(p)).ToArray();
        }
    }
}