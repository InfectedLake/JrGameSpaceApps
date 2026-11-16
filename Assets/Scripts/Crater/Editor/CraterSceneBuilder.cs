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
            camGo.AddComponent<AudioListener>();   // sin AudioListener Unity no reproduce ningún sonido

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
            gm.ui.art = BuildArtSet();

            Folder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            Selection.activeGameObject = gm.gameObject;
            Debug.Log("[CRATER] Escena creada: " + ScenePath + ". Presiona Play para jugar.");
        }

        // ------------------------------------------------------------------ Arte original

        const string ArtPath = DataRoot + "/ArtSet.asset";
        const string SpritesRoot = "Assets/Sprites/";

        /// <summary>
        /// Vuelve a enlazar ArtSet.asset con los sprites originales por su ruta y lo asigna al UIManager de la
        /// escena abierta. Úsalo si se reemplazan o renombran sprites. No modifica los archivos de Assets/Sprites.
        /// </summary>
        [MenuItem("CRATER/Asignar arte original", priority = 40)]
        public static void AssignArt()
        {
            var set = BuildArtSet();
            var ui = Object.FindFirstObjectByType<UIManager>();
            if (ui != null)
            {
                Undo.RecordObject(ui, "Asignar arte original");
                ui.art = set;
                EditorUtility.SetDirty(ui);
                EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            }
            Debug.Log("[CRATER] Arte original asignado" + (ui != null ? " al UIManager de la escena." : ". Abre Crater.unity para asignarlo al UIManager."));
        }

        static CraterArtSet BuildArtSet()
        {
            Folder(DataRoot);
            var set = AssetDatabase.LoadAssetAtPath<CraterArtSet>(ArtPath);
            if (set == null) { set = ScriptableObject.CreateInstance<CraterArtSet>(); AssetDatabase.CreateAsset(set, ArtPath); }
            Sprite S(string path)
            {
                EnsureWholeSprite(SpritesRoot + path);
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesRoot + path);
                if (s == null) Debug.LogWarning("[CRATER] No se encontró el sprite " + SpritesRoot + path);
                return s;
            }
            const string g = "gamplay principal/", e = "evento eclipse/", m = "evento micrometeoritos/", t = "evento tormeta solar/edificios tormenta solar_", i = "pantalla de inicio/";
            set.background = S(g + "fondo principal.png");
            set.energy = S(g + "edificio energia.png");
            set.food = S(g + "edificio agua y comida (añade un rover en el suelo alado del tanque de agua) 1.png");   // con el rover integrado
            set.central = S(g + "edificio central.png");
            set.oxygen = S(g + "edificio oxigeno.png");
            set.habitat = S(g + "edificio vivienda.png");
            set.energyBroken = S(m + "edificio energia roto.png");
            set.energyLit = S(e + "edificios encendidos_edificio energia.png");
            set.foodLit = S(e + "edificios encendidos_edificio agua y comida.png");
            set.oxygenLit = S(e + "edificios encendidos_edificio oxigeno.png");
            set.habitatLit = S(e + "edificios prendidos_edificio vivienda.png");
            set.waterEnergy = S(t + "Capa de agua de edificio de energia.png");
            set.waterFood = S(t + "Capa de agua de edificio de agua y comida.png");
            set.waterOxygen = S(t + "Capa de pintura 3.png");
            set.waterHabitat = S(t + "Capa de pintura 4.png");
            set.dustEnergy = S(t + "polvo edificio energia.png");
            set.dustFood = S(t + "polvo edificio agua y comida.png");
            set.dustOxygen = S(t + "polvo edificio oxigeno.png");
            set.dustHabitat = S(t + "polvo edificio vivienda.png");
            set.eclipseFilter = S(e + "filtro de obsuresimiento.png");
            set.eclipseSun = S(e + "sol del eclipse.png");
            set.eclipseEarth = S(e + "tierra del eclipse.png");
            set.meteorsOverlay = S(m + "meteoritos de fondo.png");
            set.topBar = S(g + "fondo de recursos.png");
            set.slotFood = S(g + "fondo de recurso agua y comida.png");
            set.slotEnergy = S(g + "fondo de recurso energia.png");
            set.slotOxygen = S(g + "fondo oxigeno.png");
            set.slotHealth = S(g + "fondo vivienda.png");
            set.iconFood = S(g + "recurso agua y comida.png");
            set.iconEnergy = S(g + "recurso energia.png");
            set.iconOxygen = S(g + "recurso oxigeno.png");
            set.iconHealth = S(g + "recurso vivienda 1.png");   // cruz de salud
            set.menuBackground = S(g + "fondo menu.png");
            set.menuIcon = S(g + "menu.png");
            set.daysBackground = S(g + "fondo dias pasados.png");
            set.nextTurn = S(g + "fondo y boton de siguente turno.png");
            set.panel = S(g + "fondo menu.png");
            set.infoPanel = S(g + "panel de informacion de edificio.png");
            set.titleBackground = S(i + "pantalla principal_fondo principal.png");
            set.titleBanner = S(i + "fondo de titulo del juego.png");
            set.buttonStart = S(i + "boton inicio.png");
            set.buttonOptions = S(i + "boton opciones.png");
            set.buttonCredits = S(i + "boton creditos.png");
            set.buttonExit = S(i + "boton salir.png");
            set.buttonSelected = S(i + "boton seleccionado universal.png");
            AudioClip A(string path)
            {
                var a = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/" + path);
                if (a == null) Debug.LogWarning("[CRATER] No se encontró el audio Assets/Audios/" + path);
                return a;
            }
            set.mixer = AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>("Assets/Data/Crater/CraterMixer.mixer");
            if (set.mixer == null) Debug.LogWarning("[CRATER] No se encontró Assets/Data/Crater/CraterMixer.mixer");
            set.musicMenu = A("Musica/menu.mp3");
            set.musicGame = A("Musica/main.mp3");
            set.musicEvent = A("Musica/MusicaEventoGrande.mp3");
            set.sfxClick = A("Efectos/botones.mp3");
            set.sfxMeteors = A("Efectos/real meteorits.mp3");
            set.sfxEclipse = A("Efectos/eclipse.mp3");
            set.sfxRadiation = A("Efectos/Radiacion.mp3");
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return set;
        }

        /// <summary>
        /// Las composiciones usan cada imagen completa. Si un PNG se reemplaza y Unity lo vuelve a importar en modo
        /// Multiple (con recortes automáticos), la referencia al sprite completo se pierde y la Image se ve blanca.
        /// Esto lo devuelve a Single y reaplica la configuración pixel art (sin tocar el archivo de imagen).
        /// </summary>
        static void EnsureWholeSprite(string assetPath)
        {
            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) return;
            bool changed = false;
            if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; changed = true; }
            if (imp.spriteImportMode != SpriteImportMode.Single)
            {
                Debug.LogWarning("[CRATER] " + assetPath + " estaba en modo " + imp.spriteImportMode + ": se vuelve a Single (la escena usa la imagen completa).");
                imp.spriteImportMode = SpriteImportMode.Single; changed = true;
            }
            if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; changed = true; }
            if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (imp.mipmapEnabled) { imp.mipmapEnabled = false; changed = true; }
            if (!imp.isReadable) { imp.isReadable = true; changed = true; }
            if (imp.maxTextureSize < 4096) { imp.maxTextureSize = 4096; changed = true; }
            if (changed) imp.SaveAndReimport();
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
