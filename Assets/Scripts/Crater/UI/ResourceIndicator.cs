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

        public static ResourceIndicator Create(RectTransform parent, string name, Sprite slotSprite, Sprite iconSprite, Vector2 slotPos, Vector2 iconCenter)
        {
            var size = slotSprite != null ? slotSprite.rect.size : new Vector2(291, 129);
            var slot = UIFactory.Image(parent, "Recurso_" + name, Color.white, slotSprite);
            slot.rectTransform.TopLeft(slotPos, size);
            var ri = slot.gameObject.AddComponent<ResourceIndicator>();
            ri.slot = slot;
            if (iconSprite != null)
            {
                var icon = UIFactory.Image(parent, "Icono_" + name, Color.white, iconSprite);
                icon.rectTransform.TopLeft(iconCenter - iconSprite.rect.size / 2f, iconSprite.rect.size);   // centrado: cualquier tamaño de icono
            }
            // La parte visible de la casilla va de y = 23 a 128 (sprite de 291 × 129); el icono ocupa la izquierda.
            // Nombre arriba, cantidad grande abajo y el cambio proyectado a su derecha, sin superponerse.
            var ink = new Color(0.25f, 0.15f, 0.02f);
            var label = UIFactory.Text(slot.transform, "Nombre", name.ToUpperInvariant(), 18, ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            label.rectTransform.TopLeft(new Vector2(108, 30), new Vector2(166, 24));
            label.enableAutoSizing = true; label.fontSizeMin = 14; label.fontSizeMax = 18;
            ri.valueText = UIFactory.Text(slot.transform, "Valor", "0", 46, ink, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            ri.valueText.rectTransform.TopLeft(new Vector2(108, 56), new Vector2(84, 60));
            ri.valueText.textWrappingMode = TextWrappingModes.NoWrap;
            ri.valueText.enableAutoSizing = true; ri.valueText.fontSizeMin = 32; ri.valueText.fontSizeMax = 46;
            ri.deltaText = UIFactory.Text(slot.transform, "Cambio", "", 24, Theme.Good, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            ri.deltaText.rectTransform.TopLeft(new Vector2(190, 62), new Vector2(84, 48));
            ri.deltaText.enableAutoSizing = true; ri.deltaText.fontSizeMin = 14; ri.deltaText.fontSizeMax = 26;
            return ri;
        }

        public void Show(int value, int max, ResourceChange change, int alertThreshold)
        {
            valueText.text = value.ToString();
            if (change != null)
            {
                int d;
                string txt;
                if (change.Wasted > 0) { d = change.GrossDelta; txt = Num.Signed(d) + "\n<size=60%>lleno</size>"; }
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
