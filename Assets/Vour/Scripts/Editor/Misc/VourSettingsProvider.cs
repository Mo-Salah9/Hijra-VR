#if VOUR_SETTINGS
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.XR.CoreUtils.Editor;

namespace CrizGames.Vour.Editor
{
    public class VourSettingsProvider : ScriptableSettingsProvider<VourSettings>
    {
        private const string SettingsPath = "Project/Vour";
        
        private UnityEditor.Editor _settingsEditor;
        
        [SettingsProvider]
        public static SettingsProvider CreateVourSettingsProvider()
        {
            var keywordsList = GetSearchKeywordsFromPath(AssetDatabase.GetAssetPath(VourSettings.Instance)).ToList();
            return new VourSettingsProvider { keywords = keywordsList };
        }

        private VourSettingsProvider(string path = SettingsPath, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
        }

        /// <inheritdoc />
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settingsEditor = UnityEditor.Editor.CreateEditor(VourSettings.Instance);
        }

        /// <inheritdoc />
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            
            if (_settingsEditor != null)
                Object.DestroyImmediate(_settingsEditor);
        }

        /// <inheritdoc />
        public override void OnGUI(string searchContext)
        {
            if (_settingsEditor != null)
                _settingsEditor.OnInspectorGUI();
        }
    }
}
#endif
