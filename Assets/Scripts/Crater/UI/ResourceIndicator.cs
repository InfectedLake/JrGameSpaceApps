using Crater.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Indicador de la barra superior: icono, nombre, cantidad, barra, cambio proyectado con signo
    /// (+ verde / − rojo, comprensible sin color) y estado de alerta (≤ 25).
    /// </summary>
    public class ResourceIndicator : MonoBehaviour
    {
        Image background, fill, icon;
        TextMeshProUGUI nameText, valueText, deltaText, alertText;
        RectTransform fillRect;
        Color color;
        bool alert;
        float shown = -1, target;
        float t;

        public static ResourceIndicator Create(Transform parent, string name, string letter, Color color)
        {
            var bg = UIFactory.Panel(parent, "Recurso_" + name, Theme.Panel, false);
            var ri = bg.gameObject.AddComponent<ResourceIndicator>();
            ri.background = bg;
            ri.color = color;

            ri.icon = UIFactory.Image(bg.transform, "Icono", color, SpriteFactory.Circle);
            ri.icon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(70, 70), new Vector2(0, 0.5f));
            var l = UIFactory.Text(ri.icon.transform, "Letra", letter, 28, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            l.rectTransform.Stretch();

            ri.nameText = UIFactory.Text(bg.transform, "Nombre", name.ToUpperInvariant(), 20, Theme.InkSoft, TextAlignmentOptions.Left, FontStyles.Bold);
            ri.nameText.rectTransform.Place(new Vector2(0, 1), new Vector2(96, -8), new Vector2(150, 26), new Vector2(0, 1));
            ri.valueText = UIFactory.Text(bg.transform, "Valor", "0", 40, Theme.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
            ri.valueText.rectTransform.Place(new Vector2(0, 1), new Vector2(96, -30), new Vector2(110, 46), new Vector2(0, 1));
            ri.deltaText = UIFactory.Text(bg.transform, "Cambio", "+0", 28, Theme.Good, TextAlignmentOptions.Right, FontStyles.Bold);
            ri.deltaText.rectTransform.Place(new Vector2(1, 1), new Vector2(-14, -30), new Vector2(150, 46), new Vector2(1, 1));
            ri.alertText = UIFactory.Text(bg.transform, "Alerta", "¡BAJO!", 18, Theme.Bad, TextAlignmentOptions.Right, FontStyles.Bold);
            ri.alertText.rectTransform.Place(new Vector2(1, 1), new Vector2(-14, -8), new Vector2(120, 24), new Vector2(1, 1));

            var track = UIFactory.Image(bg.transform, "Barra", new Color(0.82f, 0.86f, 0.92f), SpriteFactory.Rounded);
            track.rectTransform.Place(new Vector2(0, 0), new Vector2(96, 12), new Vector2(0, 14), new Vector2(0, 0));
            track.rectTransform.anchorMax = new Vector2(1, 0);
            track.rectTransform.offsetMax = new Vector2(-14, 26);
            ri.fill = UIFactory.Image(track.transform, "Relleno", color, SpriteFactory.Rounded);
            ri.fillRect = ri.fill.rectTransform;
            ri.fillRect.anchorMin = Vector2.zero; ri.fillRect.anchorMax = new Vector2(1, 1);
            ri.fillRect.offsetMin = ri.fillRect.offsetMax = Vector2.zero;
            return ri;
        }

        /// <param name="change">Cambio proyectado de la simulación del día.</param>
        public void Show(int value, int max, ResourceChange change, int alertThreshold)
        {
            target = Mathf.Clamp01(value / (float)Mathf.Max(1, max));
            if (shown < 0) shown = target;
            valueText.text = value.ToString();

            if (change != null)
            {
                string txt;
                int d;
                if (change.Wasted > 0) { d = change.GrossDelta; txt = Num.Signed(d) + " (lleno)"; }
                else { d = change.Delta; txt = Num.Signed(d); }
                deltaText.text = txt;
                deltaText.color = d > 0 ? Theme.Good : (d < 0 ? Theme.Bad : Theme.InkSoft);
            }
            else deltaText.text = "";

            alert = value <= alertThreshold;
            alertText.gameObject.SetActive(alert);
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 0.8f);
            fillRect.anchorMax = new Vector2(Mathf.Max(0.001f, shown), 1);
            fill.color = alert ? Theme.Bad : color;
            background.color = alert ? Color.Lerp(Theme.Panel, new Color(1f, 0.82f, 0.82f), 0.5f + 0.5f * Mathf.Sin(t * 4f)) : Theme.Panel;
        }
    }
}
