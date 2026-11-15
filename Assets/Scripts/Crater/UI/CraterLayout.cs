using Crater.Core;
using UnityEngine;

namespace Crater.UI
{
    /// <summary>
    /// Posiciones del arte original, medidas sobre las capturas de referencia de Assets/Sprites
    /// (comparación de píxeles; la captura "ejemplo de como se deve de ver" es la composición a escala 0.5).
    ///
    /// MUNDO: coordenadas en píxeles del sprite "fondo principal" (2146 × 2065), origen arriba a la izquierda.
    /// La vista del juego es la ventana de 1920 × 1080 que empieza en (184, 636) → 1 píxel de arte = 1 píxel
    /// de UI a la resolución de referencia (1920 × 1080).
    ///
    /// PANTALLA: coordenadas de la interfaz a 1920 × 1080, origen arriba a la izquierda.
    ///
    /// Orden de capas (de atrás hacia adelante, verificado contra la referencia):
    ///   fondo → energía → agua y comida (con el tanque) → rover → núcleo → oxígeno → vivienda
    /// Cada edificio lleva encima sus propias capas de evento (agua / polvo).
    /// </summary>
    public static class CraterLayout
    {
        public static readonly Vector2 WorldSize = new Vector2(2146, 2065);
        public static readonly Vector2 ViewOrigin = new Vector2(184, 636);
        public static readonly Vector2 ViewSize = new Vector2(1920, 1080);
        /// <summary>Zona pintada del fondo (la cámara no muestra nada fuera de ella).</summary>
        public static readonly Vector2 PaintedMin = new Vector2(190, 640), PaintedMax = new Vector2(2100, 1712);

        // Esquina superior izquierda de cada sprite en el mundo.
        public static readonly Vector2 Central = new Vector2(894, 908);
        public static Vector2 BuildingPos(BuildingId id)
        {
            switch (id)
            {
                case BuildingId.Solar: return new Vector2(552, 852);
                case BuildingId.Greenhouse: return new Vector2(1340, 780);
                case BuildingId.Recycler: return new Vector2(744, 1176);
                default: return new Vector2(1266, 1088);
            }
        }

        // Desplazamiento de las capas respecto del edificio (alineación de siluetas).
        public static Vector2 WaterOffset(BuildingId id)
        {
            switch (id)
            {
                case BuildingId.Solar: return new Vector2(132, 26);
                case BuildingId.Greenhouse: return new Vector2(-4, 2);
                case BuildingId.Recycler: return new Vector2(-2, 22);
                default: return new Vector2(-7, -2);
            }
        }

        public static Vector2 DustOffset(BuildingId id)
        {
            switch (id)
            {
                case BuildingId.Solar: return new Vector2(133, 36);
                case BuildingId.Greenhouse: return new Vector2(6, 2);
                case BuildingId.Recycler: return new Vector2(1, 28);
                default: return new Vector2(0, 6);
            }
        }

        /// <summary>Variante "encendida" del eclipse: tiene margen para el brillo.</summary>
        public static Vector2 LitOffset(BuildingId id)
        {
            switch (id)
            {
                case BuildingId.Greenhouse: return new Vector2(-4, 0);
                case BuildingId.Habitat: return new Vector2(-46, 0);
                default: return Vector2.zero;
            }
        }

        /// <summary>
        /// Zona clicable dentro del sprite (píxeles del sprite). El sprite de agua y comida incluye la explanada
        /// y el tanque de agua, que no son parte del invernadero.
        /// </summary>
        public static Rect HitArea(BuildingId id, Vector2 spriteSize)
            => id == BuildingId.Greenhouse ? new Rect(0, 0, 300, 330) : new Rect(Vector2.zero, spriteSize);

        /// <summary>Centro visual del edificio para el acercamiento de la cámara (mundo).</summary>
        public static Vector2 Focus(BuildingId id)
        {
            switch (id)
            {
                case BuildingId.Solar: return new Vector2(907, 1014);
                case BuildingId.Greenhouse: return new Vector2(1490, 945);
                case BuildingId.Recycler: return new Vector2(891, 1294);
                default: return new Vector2(1380, 1252);
            }
        }

        /// <summary>Rover en el suelo, al lado del tanque de agua (README).</summary>
        public static readonly Vector2 RoverCenter = new Vector2(1620, 1222);
        public static readonly Vector2 RoverSize = new Vector2(120, 80);

        // Cámara: en la captura de selección el edificio se ve al doble y queda a la derecha del panel.
        public const float ZoomFactor = 2f;
        public static readonly Vector2 ZoomTarget = new Vector2(1459, 497);
        public const float ZoomSeconds = 0.45f;

        // Interfaz de juego (pantalla).
        public static readonly Vector2 TopBar = new Vector2(132, 8);
        public static readonly Vector2[] Slots = { new Vector2(172, 0), new Vector2(490, 0), new Vector2(812, 0), new Vector2(1130, 2) };
        public static readonly Vector2[] Icons = { new Vector2(194, 34), new Vector2(512, 38), new Vector2(832, 34), new Vector2(1142, 34) };
        public static readonly Vector2 MenuBackground = new Vector2(1584, 0);
        public static readonly Vector2 MenuIcon = new Vector2(1626, 54);
        public static readonly Vector2 Days = new Vector2(1760, 204);
        public static readonly Vector2 NextTurn = new Vector2(1476, 852);
        public static readonly Vector2 InfoPanelPos = new Vector2(63, 77);
        public static readonly Vector2 InfoPanelSize = new Vector2(851, 924);

        // Pantalla de inicio.
        public static readonly Vector2 TitleBanner = new Vector2(830, 82);
        public static readonly Vector2[] MenuButtons = { new Vector2(1442, 316), new Vector2(1446, 500), new Vector2(1446, 662), new Vector2(1446, 832) };

        /// <summary>Coloca un RectTransform por su esquina superior izquierda (y hacia abajo).</summary>
        public static RectTransform TopLeft(this RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(pos.x, -pos.y);
            rt.sizeDelta = size;
            return rt;
        }
    }
}
