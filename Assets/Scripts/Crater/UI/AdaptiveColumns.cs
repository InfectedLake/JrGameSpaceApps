using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Coloca dos secciones (columnas con VerticalLayoutGroup) dentro del contenido de un ScrollRect:
    ///  - Con ancho suficiente: dos columnas del MISMO ancho, lado a lado.
    ///  - En pantallas estrechas: una sola columna, una sección debajo de la otra (se desplaza verticalmente).
    /// El ancho de cada columna se asigna aquí, así que nunca depende del ancho preferido del texto
    /// (en TextMeshPro ese valor es el largo del texto sin saltos de línea y puede medir miles de píxeles).
    /// </summary>
    [ExecuteAlways]
    public class AdaptiveColumns : MonoBehaviour
    {
        public RectTransform left, right;
        public float gap = 80f;
        [Tooltip("Ancho mínimo de cada columna para usar dos columnas.")]
        public float minColumnWidth = 560f;

        RectTransform content;
        float lastWidth = -1f;

        public bool TwoColumns { get; private set; }

        void OnEnable() { content = (RectTransform)transform; lastWidth = -1f; }

        void LateUpdate() => Arrange(false);

        public void Arrange(bool force)
        {
            if (left == null || right == null) return;
            if (content == null) content = (RectTransform)transform;
            float width = content.rect.width;
            if (width <= 0) return;

            TwoColumns = (width - gap) / 2f >= minColumnWidth;
            float colW = TwoColumns ? (width - gap) / 2f : width;
            Size(left, colW);
            Size(right, colW);
            float hl = LayoutUtility.GetPreferredHeight(left), hr = LayoutUtility.GetPreferredHeight(right);
            Place(left, 0, 0, colW, hl);
            if (TwoColumns) Place(right, colW + gap, 0, colW, hr);
            else Place(right, 0, hl + gap * 0.6f, colW, hr);
            float total = TwoColumns ? Mathf.Max(hl, hr) : hl + gap * 0.6f + hr;
            if (force || !Mathf.Approximately(content.sizeDelta.y, total)) content.sizeDelta = new Vector2(content.sizeDelta.x, total);
            lastWidth = width;
        }

        static void Size(RectTransform rt, float w)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            if (!Mathf.Approximately(rt.sizeDelta.x, w))
            {
                rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);   // las filas se recalculan con el ancho nuevo
            }
        }

        static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
