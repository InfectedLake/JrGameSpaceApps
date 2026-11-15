using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crater.Core;
using Crater.Data;
using Crater.Services;
using Crater.Systems;
using Crater.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Crater.EditorTools
{
    /// <summary>
    /// Menú "CRATER" del editor:
    ///  - Crear assets de datos: genera los ScriptableObjects (balance, edificios, eventos y ciencia)
    ///    en Assets/Data/Crater con los valores del documento de diseño.
    ///  - Construir escena del juego: crea Assets/Scenes/Crater.unity con todos los sistemas ya conectados
    ///    y la agrega como primera escena de Build Settings.
    /// </summary>
    public static class CraterSceneBuilder
    {
        const string DataRoot = "Assets/Data/Crater";
        const string ScenePath = "Assets/Scenes/Crater.unity";

        [MenuItem("CRATER/Construir escena y datos (todo)", priority = 0)]
        public static void BuildAll()
        {
            CreateDataAssets();
            BuildScene();
        }

        [MenuItem("CRATER/1. Crear assets de datos", priority = 20)]
        public static void CreateDataAssets()
        {
            Folder(DataRoot);
            Folder(DataRoot + "/Edificios");
            Folder(DataRoot + "/Eventos");
            Folder(DataRoot + "/Ciencia");

            var balancePath = DataRoot + "/GameBalance.asset";
            if (AssetDatabase.LoadAssetAtPath<GameBalanceData>(balancePath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameBalanceData>(), balancePath);

            string[] buildingFiles = { "PanelSolar", "Invernadero", "RecicladorAire", "Vivienda" };
            var buildings = DefaultContent.Buildings();
            for (int i = 0; i < 4; i++) CreateIfMissing(buildings[i], DataRoot + "/Edificios/" + buildingFiles[i] + ".asset");

            string[] eventFiles = { "RadiacionSolar", "Micrometeoritos", "SombraTierra" };
            var events = DefaultContent.Events();
            for (int i = 0; i < events.Length; i++) CreateIfMissing(events[i], DataRoot + "/Eventos/" + eventFiles[i] + ".asset");

            foreach (var s in DefaultContent.Science()) CreateIfMissing(s, DataRoot + "/Ciencia/" + s.id + ".asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CRATER] Assets de datos listos en " + DataRoot);
        }

        [MenuItem("CRATER/2. Construir escena del juego", priority = 21)]
        public static void BuildScene()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("CRATER", "La escena " + ScenePath + " ya existe. ¿Reemplazarla?", "Reemplazar", "Cancelar"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.Sky;
            camGo.transform.position = new Vector3(0, 0, -10);

            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif

            var root = new GameObject("CRATER");
            T Sys<T>(string name) where T : Component
            {
                var go = new GameObject(name);
                go.transform.SetParent(root.transform, false);
                return go.AddComponent<T>();
            }

            var gm = Sys<GameManager>("GameManager");
            gm.turns = Sys<TurnManager>("TurnManager");
            gm.resources = Sys<ResourceManager>("ResourceManager");
            gm.buildings = Sys<BuildingManager>("BuildingManager");
            gm.events = Sys<EventManager>("EventManager");
            gm.radiation = Sys<RadiationSystem>("RadiationSystem");
            gm.rovers = Sys<RoverManager>("RoverManager");
            gm.batteries = Sys<BatteryManager>("BatteryManager");
            gm.nasa = Sys<NASADataService>("NASADataService");
            gm.science = Sys<ScienceInfoManager>("ScienceInfoManager");
            gm.saves = Sys<SaveSystem>("SaveSystem");
            gm.ui = Sys<UIManager>("UIManager");

            // Datos
            gm.balance = AssetDatabase.LoadAssetAtPath<GameBalanceData>(DataRoot + "/GameBalance.asset");
            var b = Load<BuildingData>(DataRoot + "/Edificios").OrderBy(x => (int)x.stats.id).ToArray();
            if (b.Length == 4) gm.buildingData = b;
            var e = Load<EventData>(DataRoot + "/Eventos").OrderBy(x => (int)x.type).ToArray();
            if (e.Length > 0) gm.eventData = e;
            var sci = Load<ScienceData>(DataRoot + "/Ciencia");
            var order = DefaultContent.Science().Select(x => x.id).ToList();
            gm.science.entries = sci.OrderBy(x => { int i = order.IndexOf(x.id); return i < 0 ? 999 : i; }).ToList();

            Folder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            Selection.activeGameObject = gm.gameObject;
            Debug.Log("[CRATER] Escena creada: " + ScenePath + ". Presiona Play para jugar.");
        }

        static List<T> Load<T>(string folder) where T : Object
            => AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(x => x != null).ToList();

        static void CreateIfMissing(Object asset, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) return;
            AssetDatabase.CreateAsset(asset, path);
        }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
