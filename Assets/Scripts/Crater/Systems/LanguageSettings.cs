using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Preferencia de idioma (PlayerPrefs "crater.lang"): inglés si nunca se eligió otro, y la última elección del
    /// jugador en las siguientes sesiones. Se carga una sola vez al arrancar (GameManager.Awake) y solo se escribe
    /// cuando el jugador cambia el idioma: cambiar de escena o de pantalla nunca la sobrescribe.
    /// </summary>
    public static class LanguageSettings
    {
        const string Key = "crater.lang";
        static bool loaded;
        static readonly HashSet<string> reported = new HashSet<string>();

        public static void Load()
        {
            if (loaded) return;
            loaded = true;
            var lang = Language.English;
            if (PlayerPrefs.HasKey(Key) && PlayerPrefs.GetInt(Key) == (int)Language.Spanish) lang = Language.Spanish;
            Loc.Set(lang);
            Loc.Missing += key => { if (reported.Add(key ?? "")) Debug.LogWarning("[CRATER] Falta la clave de localización: " + key); };
        }

        /// <summary>Elección del jugador: guarda la preferencia y actualiza toda la interfaz al instante.</summary>
        public static void Set(Language language)
        {
            PlayerPrefs.SetInt(Key, (int)language);
            PlayerPrefs.Save();
            Loc.Set(language);
        }
    }
}
