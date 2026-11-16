using System;
using Crater.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Texto localizado: guarda de dónde sale su contenido (una clave o una función) y se reescribe solo cuando
    /// cambia el idioma (Loc.Changed). Si el objeto está oculto, se actualiza al volver a mostrarse.
    /// Uso: UIFactory.Text(...).L("MENU_START")  ·  boton.L("BACK")  ·  texto.L(() => Loc.T("HUD_CYCLE", c, t)).
    /// </summary>
    public class LocText : MonoBehaviour
    {
        TMP_Text target;
        Func<string> source;

        public static void Bind(TMP_Text t, Func<string> src)
        {
            if (t == null) return;
            var lt = t.GetComponent<LocText>();
            if (lt == null) lt = t.gameObject.AddComponent<LocText>();
            lt.target = t;
            lt.source = src;
            lt.Apply();
        }

        void OnEnable() { Loc.Changed += Apply; Apply(); }
        void OnDisable() { Loc.Changed -= Apply; }

        public void Apply()
        {
            if (target != null && source != null) target.text = source();
        }
    }

    public static class LocExtensions
    {
        public static T L<T>(this T t, string key) where T : TMP_Text { LocText.Bind(t, () => Loc.T(key)); return t; }
        public static T L<T>(this T t, Func<string> source) where T : TMP_Text { LocText.Bind(t, source); return t; }
        public static Button L(this Button b, string key) { b.Label().L(key); return b; }
        public static Button L(this Button b, Func<string> source) { b.Label().L(source); return b; }
    }
}
