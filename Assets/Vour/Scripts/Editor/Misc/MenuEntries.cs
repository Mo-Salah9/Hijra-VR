using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    public static class MenuEntries
    {
        [MenuItem("Vour/Setup", false, 100)]
        public static void ShowSetupWindow()
        {
            SetupWindow.ShowWindow();
        }

        [MenuItem("Vour/Center Editor Camera &c", false, 200)]
        public static void CenterEditorCam()
        {
            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                var target = view.camera.transform;
                target.position = Vector3.zero;
                target.rotation = Quaternion.identity;
                view.AlignViewToObject(target);
            }
        }

        [MenuItem("Vour/Open Online Documentation", false, 300)]
        public static void OpenOnlineDocs()
        {
            Application.OpenURL("https://crizgames.gitbook.io/vour/");
        }

        [MenuItem("GameObject/Vour/Location/Empty Location", false, 11)]
        public static void AddLocationEmpty() => AddLocation(LocationType.Empty);

        [MenuItem("GameObject/Vour/Location/Image Location", false, 12)]
        public static void AddLocationImage() => AddLocation(LocationType.Image);

        [MenuItem("GameObject/Vour/Location/Video Location", false, 13)]
        public static void AddLocationVideo() => AddLocation(LocationType.Video);

        [MenuItem("GameObject/Vour/Location/Scene Location", false, 14)]
        public static void AddLocationScene() => AddLocation(LocationType.Scene);

        private static void AddLocation(LocationType type)
        {
            // If it doesn't exist yet, add one
            if (LocationManager.GetManager() == null)
                AddLocationManager();

            // Add location to scene
            var go = (GameObject) PrefabUtility.InstantiatePrefab(VourSettings.Instance.locationPrefab);
            go.name = $"{type} Location";
            go.GetComponent<Location>().locationType = type;
            
            // Move to last place in hierarchy
            go.transform.SetAsLastSibling();
            
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            
            // Select the newly created object
            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/Vour/Teleport Point", false, 31)]
        public static void AddTeleportPoint(MenuCommand menuCommand) => AddPlaceable(menuCommand, VourSettings.Instance.defaultTeleportPoint);

        [MenuItem("GameObject/Vour/Info Point", false, 32)]
        public static void AddInfoPoint(MenuCommand menuCommand) => AddPlaceable(menuCommand, VourSettings.Instance.defaultInfoPoint);

        [MenuItem("GameObject/Vour/Info Panel", false, 33)]
        public static void AddInfoPanel(MenuCommand menuCommand) => AddPlaceable(menuCommand, VourSettings.Instance.defaultInfoPanel);

        [MenuItem("GameObject/Vour/Video Point", false, 34)]
        public static void AddVideoPoint(MenuCommand menuCommand) => AddPlaceable(menuCommand, VourSettings.Instance.defaultVideoPoint);

        [MenuItem("GameObject/Vour/Video Panel", false, 35)]
        public static void AddVideoPanel(MenuCommand menuCommand) => AddPlaceable(menuCommand, VourSettings.Instance.defaultVideoPanel);

        private static void AddPlaceable(MenuCommand menuCommand, GameObject prefab)
        {
            var selectedGo = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (selectedGo == null || !selectedGo.TryGetComponent<Location>(out _))
            {
                EditorUtility.DisplayDialog($"Cannot Add {prefab.name}", $"You must have a location selected to parent this {prefab.name} to!", "Okay");
                return;
            }
            
            // Create object
            var go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, Selection.activeTransform);
            
            // Reparent to object of context click
            GameObjectUtility.SetParentAndAlign(go, selectedGo);
            
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            
            // Select the newly created object
            Selection.activeGameObject = go;
        }
        
        [MenuItem("GameObject/Vour/Location Manager", false, 51)]
        public static void AddLocationManager()
        {
            var managerInScene = LocationManager.GetManager();
            if (managerInScene != null)
            {
                EditorUtility.DisplayDialog("Info", "A Location Manager object is already in the scene!", "Okay");
                Selection.activeObject = managerInScene.gameObject;
                return;
            }

            // Add location manager to scene
            var go = (GameObject) PrefabUtility.InstantiatePrefab(VourSettings.Instance.locationManagerPrefab);
            
            // Move to first place in hierarchy
            go.transform.SetAsFirstSibling();
            
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            
            // Select the newly created object
            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/Vour/Player", false, 52)]
        public static void AddPlayer()
        {
            var playerInScene = Object.FindFirstObjectByType<Player>();
            if (playerInScene != null)
            {
                EditorUtility.DisplayDialog("Info", "A Player object is already in the scene!", "Okay");
                Selection.activeObject = playerInScene.gameObject;
                return;
            }

            var player = (GameObject)PrefabUtility.InstantiatePrefab(VourSettings.Instance.playerPrefab);
            
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(player, "Create " + player.name);
            
            Selection.activeGameObject = player;
        }
    }
}