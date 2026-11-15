using Crater.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Casilla de recurso de la barra superior con el arte original ("fondo de recurso …" + "recurso …").
    /// Muestra la cantidad, el cambio proyectado con signo (+ / −, comprensible sin color) y una alerta
    /// sencilla (la casilla parpadea en rojo) cuando el valor es 25 o menos.
    /// </summary>
    public class ResourceIndicator : MonoBehaviour
    {
        Image slot;
        TextMeshProUGUI valueText, deltaText;
        bool alert;
        float t;

        public static ResourceIndicator Create(RectTransform parent, string name, Sprite slotSprite, Sprite iconSprite, Vector2 slotPos, Vector2 iconPos)
        {
            var size = slotSprite != null ? slotSprite.rect.size : new Vector2(291, 129);
            var slot = UIFactory.Image(parent, "Recurso_" + name, Color.white, slotSprite);
            slot.rectTransform.TopLeft(slotPos, size);
            var ri = slot.gameObject.AddComponent<ResourceIndicator>();
            ri.slot = slot;
            if (iconSprite != null)
            {
                var icon = UIFactory.Image(parent, "Icono_" + name, Color.white, iconSprite);
                icon.rectTransform.TopLeft(iconPos, iconSprite.rect.size);
            }
            // La parte opaca de la casilla empieza 23 px abajo; el texto va a la derecha del icono.
            ri.valueText = UIFactory.Text(slot.transform, "Valor", "0", 44, Theme.Ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            ri.valueText.rectTransform.TopLeft(new Vector2(118, 30), new Vector2(110, 56));
            ri.deltaText = UIFactory.Text(slot.transform, "Cambio", "", 24, Theme.Good, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            ri.deltaText.rectTransform.TopLeft(new Vector2(118, 82), new Vector2(165, 34));
            var label = UIFactory.Text(slot.transform, "Nombre", name.ToUpperInvariant(), 17, Theme.InkSoft, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            label.rectTransform.TopLeft(new Vector2(150, 32), new Vector2(128, 24));
            return ri;
        }

        public void Show(int value, int max, ResourceChange change, int alertThreshold)
        {
            valueText.text = value.ToString();
            if (change != null)
            {
                int d;
                string txt;
                if (change.Wasted > 0) { d = change.GrossDelta; txt = Num.Signed(d) + " (lleno)"; }
                else { d = change.Delta; txt = Num.Signed(d); }
                deltaText.text = txt;
                deltaText.color = d > 0 ? Theme.Good : (d < 0 ? Theme.Bad : Theme.InkSoft);
            }
            else deltaText.text = "";
            alert = value <= alertThreshold;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            slot.color = alert ? Color.Lerp(Color.white, new Color(1f, 0.55f, 0.55f), 0.5f + 0.5f * Mathf.Sin(t * 5f)) : Color.white;
        }
    }
}
