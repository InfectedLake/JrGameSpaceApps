using System;
using System.Collections.Generic;
using System.Globalization;

namespace Crater.Core
{
    /// <summary>Idiomas del juego. El predeterminado es inglés.</summary>
    public enum Language { English = 0, Spanish = 1 }

    /// <summary>
    /// Localización centralizada (C# puro, sin dependencias de Unity, para que la usen también la simulación y las reglas).
    ///  - Cada texto visible tiene una clave estable (p. ej. "MENU_START") con su versión en inglés y en español.
    ///  - Las tablas están en LocTables.*.cs, agrupadas por área.
    ///  - Plantillas con parámetros: T("ALERT_DAYS", 2) → "will arrive in 2 days" / "llegará en 2 días".
    ///  - Plurales: Days(n) → "1 day" / "2 days" · "1 día" / "2 días".
    ///  - Changed avisa a la interfaz para que se vuelva a escribir sin reiniciar nada.
    /// La preferencia del jugador la guarda LanguageSettings (Systems) en PlayerPrefs.
    /// </summary>
    public static partial class Loc
    {
        public static Language Current { get; private set; } = Language.English;
        public static bool Spanish => Current == Language.Spanish;

        /// <summary>Se dispara después de cambiar el idioma.</summary>
        public static event Action Changed;

        /// <summary>Se dispara con cada clave que no existe en la tabla (la interfaz lo registra en la consola).</summary>
        public static event Action<string> Missing;

        static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>(StringComparer.Ordinal);

        static Loc()
        {
            AddUI();
            AddGameplay();
            AddEvents();
            AddMinorEvents();
            AddContent();
            AddScience();
        }

        public static void Set(Language language)
        {
            if (language == Current) return;
            Current = language;
            Changed?.Invoke();
        }

        static void Add(string key, string en, string es) => table[key] = new[] { en, es };

        public static bool Has(string key) => key != null && table.ContainsKey(key);

        /// <summary>Texto de la clave en el idioma actual. Si falta, devuelve la clave (visible para detectarla).</summary>
        public static string T(string key)
        {
            if (key != null && table.TryGetValue(key, out var v)) return v[(int)Current];
            Missing?.Invoke(key);
            return key ?? "";
        }

        /// <summary>Plantilla con parámetros {0}, {1}…</summary>
        public static string T(string key, params object[] args) => string.Format(CultureInfo.InvariantCulture, T(key), args);

        /// <summary>Texto de la clave, o "fallback" si la clave no existe (contenido editable desde assets).</summary>
        public static string TOr(string key, string fallback) => key != null && table.TryGetValue(key, out var v) ? v[(int)Current] : fallback;

        /// <summary>Texto guardado en la partida que puede ser una clave (causas de derrota, motivos de emergencia).</summary>
        public static string Stored(string keyOrText) => Has(keyOrText) ? T(keyOrText) : keyOrText ?? "";

        /// <summary>Texto en un idioma concreto (pruebas).</summary>
        public static string In(Language language, string key) => table.TryGetValue(key, out var v) ? v[(int)language] : null;

        public static IEnumerable<KeyValuePair<string, string[]>> All => table;

        // ------------------------------------------------------------------ Plurales y nombres frecuentes

        public static string Days(int n) => n == 1 ? T("UNIT_DAY_ONE") : T("UNIT_DAYS", n);

        public static string Resource(ResourceKind r)
        {
            switch (r)
            {
                case ResourceKind.Energy: return T("RESOURCE_ENERGY");
                case ResourceKind.Oxygen: return T("RESOURCE_OXYGEN");
                case ResourceKind.Food: return T("RESOURCE_FOOD");
                default: return T("RESOURCE_HEALTH");
            }
        }

        public static string Building(BuildingId id)
        {
            switch (id)
            {
                case BuildingId.Solar: return T("BUILDING_SOLAR");
                case BuildingId.Greenhouse: return T("BUILDING_GREENHOUSE");
                case BuildingId.Recycler: return T("BUILDING_OXYGEN_RECYCLER");
                default: return T("BUILDING_HABITAT");
            }
        }

        public static string BuildingKey(BuildingId id) =>
            id == BuildingId.Solar ? "SOLAR" : id == BuildingId.Greenhouse ? "GREENHOUSE" : id == BuildingId.Recycler ? "RECYCLER" : "HABITAT";

        public static string EventKey(CraterEventType t) =>
            t == CraterEventType.SolarRadiation ? "SOLAR" : t == CraterEventType.Micrometeorites ? "METEORS" : "SHADOW";
    }
}
