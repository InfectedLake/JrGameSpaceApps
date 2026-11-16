using System;
using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using Crater.Services;
using Crater.UI;
using UnityEngine;

namespace Crater.Systems
{
    public enum NoticeKind { Info, Good, Warning, Danger }

    /// <summary>Configuración elegida en la pantalla "Nueva partida".</summary>
    public class MissionSetup
    {
        public string baseName = "Base Shackleton";
        public List<string> astronautNames = new List<string>();
        public int cycles = 6;
        public int astronauts = 3;
        public bool tutorial = true;
    }

    /// <summary>
    /// Estado general de la partida: inicio, carga, fin, victoria y derrota.
    /// GameObject: "GameManager" (hijo de "CRATER"). Conecta en el Inspector los assets de datos
    /// (opcional) y los demás sistemas (si quedan vacíos se buscan automáticamente en la escena).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Datos (vacío = valores por defecto del documento)")]
        public GameBalanceData balance;
        public BuildingData[] buildingData = new BuildingData[4];
        public EventData[] eventData;

        [Header("Sistemas")]
        public TurnManager turns;
        public ResourceManager resources;
        public BuildingManager buildings;
        public EventManager events;
        public RadiationSystem radiation;
        public RoverManager rovers;
        public BatteryManager batteries;
        public NASADataService nasa;
        public ScienceInfoManager science;
        public SaveSystem saves;
        public AudioManager sound;
        public UIManager ui;

        public BalanceConfig Config { get; private set; }
        public GameState State { get; private set; }
        public GameRandom Rng { get; private set; }
        public bool InGame { get; private set; }
        public bool IsPlaying => InGame && State != null && State.outcome == Outcome.Playing;

        /// <summary>Cualquier cambio que la interfaz deba reflejar (también recalcula la proyección).</summary>
        public event Action StateChanged;
        public event Action GameStarted;
        public event Action<TurnReport> DayEnded;
        public event Action<Outcome> GameFinished;
        public event Action<string, NoticeKind> Noticed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ResolveSystems();
            for (int i = 0; i < 4; i++)
                if (buildingData == null || buildingData.Length < 4 || buildingData[i] == null) { buildingData = MergeDefaults(buildingData); break; }
            if (eventData == null || eventData.Length == 0) eventData = DefaultContent.Events();
            Config = (balance != null ? balance.config : new BalanceConfig()).Clone();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void ResolveSystems()
        {
            T Find<T>(T current) where T : Component
            {
                if (current != null) return current;
                var found = FindFirstObjectByType<T>();
                if (found != null) return found;
                var go = new GameObject(typeof(T).Name);
                go.transform.SetParent(transform.parent, false);
                return go.AddComponent<T>();
            }
            turns = Find(turns); resources = Find(resources); buildings = Find(buildings);
            events = Find(events); radiation = Find(radiation); rovers = Find(rovers);
            batteries = Find(batteries); nasa = Find(nasa); science = Find(science);
            saves = Find(saves); ui = Find(ui); sound = Find(sound);
        }

        static BuildingData[] MergeDefaults(BuildingData[] current)
        {
            var defaults = DefaultContent.Buildings();
            var result = new BuildingData[4];
            for (int i = 0; i < 4; i++)
                result[i] = current != null && i < current.Length && current[i] != null ? current[i] : defaults[i];
            return result;
        }

        public BuildingData GetBuilding(BuildingId id) => buildingData[(int)id];

        public EventData GetEvent(CraterEventType t)
        {
            foreach (var e in eventData) if (e != null && e.type == t) return e;
            foreach (var e in DefaultContent.Events()) if (e.type == t) return e;
            return null;
        }

        BalanceConfig BuildConfig(MissionSetup setup)
        {
            var c = (balance != null ? balance.config : new BalanceConfig()).Clone();
            c.cycles = Mathf.Max(2, setup.cycles);
            c.astronauts = Mathf.Clamp(setup.astronauts, 1, 6);
            for (int i = 0; i < 4; i++) c.buildings[i] = GetBuilding((BuildingId)i).stats.Clone();
            return c;
        }

        // ------------------------------------------------------------------ Partida

        public void NewGame(MissionSetup setup)
        {
            Config = BuildConfig(setup);
            State = GameState.CreateNew(Config, Environment.TickCount);
            State.baseName = string.IsNullOrWhiteSpace(setup.baseName) ? "Base Shackleton" : setup.baseName.Trim();
            State.astronautNames = new List<string>(setup.astronautNames);
            Rng = new GameRandom(State);
            State.schedule = EventScheduler.Generate(Config, Rng);
            State.minorSchedule = MinorEventRules.Generate(Config, Rng, State.schedule);
            State.history.Add(State.Snapshot());
            InGame = true;
            GameStarted?.Invoke();
            turns.BeginDay(setup.tutorial);
        }

        public bool ContinueGame()
        {
            var data = saves.Load();
            if (data == null || data.state == null || data.config == null) return false;
            Config = data.config;
            State = data.state;
            Rng = new GameRandom(State);
            InGame = true;
            GameStarted?.Invoke();
            turns.BeginDay(false);
            return true;
        }

        /// <summary>Reemplaza el estado por el resultado de un día simulado.</summary>
        internal void ReplaceState(GameState next)
        {
            Rng.Rebind(next);
            State = next;
        }

        internal void RaiseDayEnded(TurnReport r) => DayEnded?.Invoke(r);

        internal void FinishGame()
        {
            saves.Delete();
            GameFinished?.Invoke(State.outcome);
            NotifyChanged();
        }

        public void QuitToMenu()
        {
            if (IsPlaying) saves.Save();   // guardado automático existente: la misión se puede CONTINUAR
            SetPaused(false);
            events.Clear();
            InGame = false;
            State = null;
        }

        // ------------------------------------------------------------------ Pausa

        /// <summary>
        /// Pausa reconocida por todos los sistemas: con la partida en pausa no se puede terminar el día,
        /// encender/apagar edificios ni responder eventos. También detiene Time.timeScale.
        /// </summary>
        public bool Paused { get; private set; }
        public event Action<bool> PauseChanged;

        public void SetPaused(bool paused)
        {
            if (Paused == paused) return;
            Paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            PauseChanged?.Invoke(paused);
            NotifyChanged();
        }

        public void Decision(string text)
        {
            if (State == null) return;
            State.decisions.Add("Día " + State.day + ": " + text);
        }

        public void NotifyChanged() => StateChanged?.Invoke();

        public void Notify(string text, NoticeKind kind = NoticeKind.Info) => Noticed?.Invoke(text, kind);
    }
}
