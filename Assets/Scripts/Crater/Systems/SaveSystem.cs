using System;
using System.IO;
using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    [Serializable]
    public class SaveData
    {
        public int version = 2;
        public string savedAt;
        public BalanceConfig config;
        public GameState state;
    }

    /// <summary>
    /// Guardado y carga de la partida (JSON en Application.persistentDataPath) y de las opciones
    /// (PlayerPrefs). Se guarda automáticamente al empezar cada día y tras cada decisión. GameObject: "SaveSystem".
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public string fileName = "crater_save.json";
        GameManager G => GameManager.Instance;

        public string PathToFile => Path.Combine(Application.persistentDataPath, fileName);
        public bool HasSave => File.Exists(PathToFile);

        public void Save()
        {
            if (G == null || !G.IsPlaying) return;
            try
            {
                var data = new SaveData { savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"), config = G.Config, state = G.State };
                File.WriteAllText(PathToFile, JsonUtility.ToJson(data));
            }
            catch (Exception e) { Debug.LogWarning("[CRATER] No se pudo guardar: " + e.Message); }
        }

        public SaveData Load()
        {
            try
            {
                if (!HasSave) return null;
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathToFile));
                if (data == null || data.state == null || data.state.buildingOn == null || data.state.buildingOn.Length != 4) return null;
                if (data.state.outcome != Outcome.Playing || data.version < 2) return null; // v1 tenía el antiguo escudo
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CRATER] Partida guardada dañada: " + e.Message);
                return null;
            }
        }

        public void Delete()
        {
            try { if (HasSave) File.Delete(PathToFile); } catch (Exception) { }
        }

        // ---------- Opciones ----------
        public bool UseLiveData
        {
            get => PlayerPrefs.GetInt("crater.liveData", 1) == 1;
            set { PlayerPrefs.SetInt("crater.liveData", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public bool ShowTutorial
        {
            get => PlayerPrefs.GetInt("crater.tutorial", 1) == 1;
            set { PlayerPrefs.SetInt("crater.tutorial", value ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}
