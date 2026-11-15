using Crater.Core;
using Crater.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Vista 2.5D de la base, con la distribución DEFINITIVA de la imagen de referencia
    /// (Assets/Reference/Screenshot 2026-11-15 011911.png, 1425 × 738 px):
    ///
    ///   Energía: superior izquierda    Comida: superior derecha
    ///   Oxígeno: inferior izquierda    Vivienda: inferior derecha
    ///   Núcleo con antena: centro      Agua: extremo derecho
    ///
    /// Todas las posiciones están en píxeles de esa imagen (origen arriba a la izquierda), así que
    /// la composición coincide con la referencia. Los dibujos son placeholders temporales:
    ///  - Arte final de un edificio → BuildingData.icon (reemplaza su placeholder).
    ///  - Arte final del terreno, núcleo y módulo de agua → UIManager.baseArt (reemplaza esos placeholders).
    /// </summary>
    public class BaseView : MonoBehaviour
    {
        public const float RefWidth = 1425f, RefHeight = 738f;

        public readonly BuildingController[] buildings = new BuildingController[4];
        public RoverView rover;
        Image waterDrop, igloo;
        float t;

        /// <summary>Zona de cada edificio en la referencia: (x mín, y mín, x máx, y máx).</summary>
        public static readonly Rect[] Areas =
        {
            Rect.MinMaxRect(250, 140, 650, 370), // Energía
            Rect.MinMaxRect(830, 90, 1060, 330), // Comida
            Rect.MinMaxRect(395, 385, 640, 560), // Oxígeno
            Rect.MinMaxRect(780, 320, 950, 560), // Vivienda
        };

        static Vector2 P(float x, float y) => new Vector2(x - RefWidth / 2f, RefHeight / 2f - y);

        public static BaseView Create(Transform parent, BuildingData[] data, Sprite baseArt)
        {
            var root = UIFactory.Rect(parent, "Base");
            var bv = root.gameObject.AddComponent<BaseView>();
            bv.Build(root, data, baseArt);
            return bv;
        }

        // ---------------------------------------------------------------- utilidades de dibujo

        static readonly Color Outline = new Color(0.08f, 0.08f, 0.09f);
        static readonly Color Roof = new Color(0.90f, 0.91f, 0.93f);
        static readonly Color Wall = new Color(0.80f, 0.82f, 0.85f);
        static readonly Color Side = new Color(0.62f, 0.65f, 0.70f);
        static readonly Color Teal = new Color(0.35f, 0.75f, 0.72f);
        static readonly Color Metal = new Color(0.35f, 0.37f, 0.40f);

        /// <summary>Imagen en coordenadas de la referencia, relativa al origen (en referencia) del padre.</summary>
        static Image Img(Transform parent, Vector2 origin, string name, Color c, float x, float y, float w, float h, float rot = 0, Sprite s = null)
        {
            var img = UIFactory.Image(parent, name, c, s);
            img.rectTransform.Place(new Vector2(0.5f, 0.5f), P(x, y) - P(origin.x, origin.y), new Vector2(w, h));
            img.rectTransform.localEulerAngles = new Vector3(0, 0, rot);
            return img;
        }

        /// <summary>Bloque 2.5D: techo claro arriba, fachada y lateral sombreado, con contorno negro.</summary>
        static void Block(Transform p, Vector2 o, float cx, float cy, float w, float h, float roof)
        {
            float top = cy - (h + roof) / 2f;
            Img(p, o, "Contorno", Outline, cx, cy, w + 10, h + roof + 10);
            Img(p, o, "Techo", Roof, cx, top + roof / 2f, w, roof);
            Img(p, o, "Fachada", Wall, cx, top + roof + h / 2f, w, h);
            Img(p, o, "Lateral", Side, cx + w / 2f - 8, top + roof + h / 2f, 16, h);
            Img(p, o, "Borde", Outline, cx, top + roof, w, 3);
        }

        static void Corridor(Transform p, Vector2 o, Vector2 a, Vector2 b, float thick)
        {
            var mid = (a + b) / 2f;
            float len = Vector2.Distance(a, b);
            float ang = -Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            Img(p, o, "PasilloContorno", Outline, mid.x, mid.y, len + 8, thick + 8, ang);
            Img(p, o, "Pasillo", new Color(0.85f, 0.86f, 0.89f), mid.x, mid.y, len, thick, ang);
            Img(p, o, "PasilloSombra", new Color(0.70f, 0.72f, 0.76f), mid.x, mid.y + thick * 0.25f, len, thick * 0.35f, ang);
        }

        // ---------------------------------------------------------------- construcción

        void Build(RectTransform root, BuildingData[] data, Sprite baseArt)
        {
            var o = new Vector2(RefWidth / 2f, RefHeight / 2f); // origen = centro de la referencia
            var scenery = UIFactory.Rect(root, "Escenario").Stretch();

            if (baseArt != null)
            {
                var art = UIFactory.Image(scenery, "ArteBase", Color.white, baseArt);
                art.preserveAspect = true;
                art.rectTransform.Stretch();
            }
            else
            {
                // Terreno lunar
                Img(scenery, o, "Suelo", new Color(0.48f, 0.49f, 0.51f), 712, 450, 1700, 600);
                var rnd = new System.Random(5);
                for (int i = 0; i < 16; i++)
                    Img(scenery, o, "Loma", new Color(0.48f, 0.49f, 0.51f), i * 100f, 160 + (float)rnd.NextDouble() * 20, 180, 70, 0, SpriteFactory.Circle);
                Img(scenery, o, "Meseta", new Color(0.66f, 0.66f, 0.67f), 715, 410, 1010, 520, 0, SpriteFactory.Circle);
                for (int i = 0; i < 40; i++)
                {
                    float x = (float)rnd.NextDouble() * 1425, y = 170 + (float)rnd.NextDouble() * 560;
                    float dx = (x - 715) / 505f, dy = (y - 410) / 260f;
                    if (dx * dx + dy * dy < 1.05f) continue;
                    float s = 14 + (float)rnd.NextDouble() * 22;
                    Img(scenery, o, "Crater", new Color(0.38f, 0.39f, 0.41f), x, y, s, s * 0.5f, 0, SpriteFactory.Circle);
                }
                Img(scenery, o, "HieloLunar", new Color(0.80f, 0.90f, 0.98f, 0.8f), 1330, 590, 110, 40, 0, SpriteFactory.Circle);

                // Pasillos (conectan cada edificio con el núcleo)
                Corridor(scenery, o, new Vector2(540, 325), new Vector2(650, 375), 46);
                Corridor(scenery, o, new Vector2(760, 345), new Vector2(940, 280), 46);
                Corridor(scenery, o, new Vector2(645, 425), new Vector2(585, 455), 40);
                Corridor(scenery, o, new Vector2(760, 425), new Vector2(805, 440), 40);

                // Explanada y módulo de agua (extremo derecho, separado)
                Img(scenery, o, "Explanada", new Color(0.84f, 0.84f, 0.86f), 1030, 380, 200, 120, -8);
                Img(scenery, o, "AguaContorno", Outline, 1140, 385, 120, 80);
                Img(scenery, o, "Agua", new Color(0.22f, 0.24f, 0.27f), 1140, 385, 110, 70);
                Img(scenery, o, "AguaTecho", new Color(0.32f, 0.34f, 0.38f), 1140, 358, 110, 16);
                Img(scenery, o, "AguaTubo", Metal, 1092, 418, 10, 34);
                waterDrop = Img(scenery, o, "Gota", new Color(0.45f, 0.80f, 0.95f), 1158, 390, 22, 26, 0, SpriteFactory.Circle);
                var lbl = UIFactory.Text(scenery, "EtiquetaAgua", "Agua", 18, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
                lbl.rectTransform.Place(new Vector2(0.5f, 0.5f), P(1140, 440), new Vector2(120, 26));

                // Núcleo central con antena
                Img(scenery, o, "Mastil", Metal, 700, 245, 8, 110);
                Img(scenery, o, "Antena1", Metal, 700, 205, 30, 6);
                Img(scenery, o, "Antena2", Metal, 700, 225, 24, 6);
                Img(scenery, o, "CajaAntena", new Color(0.25f, 0.26f, 0.28f), 700, 300, 30, 20);
                Block(scenery, o, 700, 385, 150, 110, 55);
                foreach (var w in new[] { new Vector4(665, 365, 44, 24), new Vector4(735, 365, 44, 24), new Vector4(660, 410, 28, 30), new Vector4(738, 405, 44, 22) })
                    Img(scenery, o, "Ventana", Teal, w.x, w.y, w.z, w.w);
            }

            igloo = Img(scenery, o, "IgluRegolito", new Color(0.58f, 0.56f, 0.53f), 1000, 560, 120, 70, 0, SpriteFactory.Circle);

            // Edificios funcionales
            for (int i = 0; i < 4; i++)
            {
                var a = Areas[i];
                var b = BuildingController.Create(root, data[i], P(a.center.x, a.center.y), a.size);
                buildings[i] = b;
                if (data[i].icon == null) DrawPlaceholder((BuildingId)i, b, a.center);
            }

            rover = RoverView.Create(root, P(1090, 450), P(1320, 575), P(1000, 545));
            rover.size = 0.6f;
        }

        void DrawPlaceholder(BuildingId id, BuildingController b, Vector2 o)
        {
            var p = b.Art;
            switch (id)
            {
                case BuildingId.Solar:
                    // Paneles exteriores a la izquierda
                    Img(p, o, "PataPanel", Metal, 300, 330, 6, 40);
                    Img(p, o, "PanelExtContorno", Outline, 315, 300, 130, 64, -22);
                    b.AddCell(Img(p, o, "PanelExt", new Color(0.15f, 0.30f, 0.65f), 315, 300, 120, 54, -22));
                    Block(p, o, 410, 240, 110, 60, 32);
                    Block(p, o, 510, 300, 230, 82, 46);
                    Img(p, o, "TechoPanel1Contorno", Outline, 455, 190, 160, 52, -12);
                    b.AddCell(Img(p, o, "TechoPanel1", new Color(0.15f, 0.30f, 0.65f), 455, 190, 150, 44, -12));
                    Img(p, o, "TechoPanel2Contorno", Outline, 560, 222, 120, 46, -12);
                    b.AddCell(Img(p, o, "TechoPanel2", new Color(0.15f, 0.30f, 0.65f), 560, 222, 110, 38, -12));
                    b.AddLight(Img(p, o, "Ventana", Teal, 385, 240, 44, 24));
                    b.AddLight(Img(p, o, "Ventana", Teal, 565, 300, 34, 18));
                    b.AddLight(Img(p, o, "Rayo", new Color(1f, 0.85f, 0.2f), 425, 312, 30, 38));
                    Img(p, o, "CajaElectrica", new Color(0.55f, 0.57f, 0.60f), 640, 255, 52, 42);
                    Img(p, o, "Cable", new Color(0.85f, 0.2f, 0.2f), 600, 260, 70, 3, -10);
                    break;

                case BuildingId.Greenhouse:
                    Block(p, o, 970, 272, 140, 60, 40);
                    Block(p, o, 895, 185, 110, 112, 36);
                    Img(p, o, "Maquina", new Color(0.82f, 0.83f, 0.86f), 895, 112, 54, 32);
                    foreach (var x in new[] { 860f, 882f, 904f }) b.AddLight(Img(p, o, "Franja", Teal, x, 200, 14, 64));
                    b.AddLight(Img(p, o, "VentanaTecho", Teal, 925, 150, 42, 14));
                    b.AddLight(Img(p, o, "Planta", new Color(0.30f, 0.75f, 0.35f), 930, 195, 22, 30));
                    b.AddLight(Img(p, o, "Ventana", Teal, 1000, 285, 50, 18));
                    Img(p, o, "Caja", new Color(0.82f, 0.83f, 0.86f), 1042, 282, 30, 46);
                    break;

                case BuildingId.Recycler:
                    Img(p, o, "CajaContorno", Outline, 430, 482, 66, 56);
                    Img(p, o, "Caja", Roof, 430, 482, 58, 48);
                    Block(p, o, 530, 465, 130, 100, 40);
                    Img(p, o, "Maquina", new Color(0.30f, 0.31f, 0.34f), 530, 400, 52, 26);
                    b.AddLight(Img(p, o, "Ventana", Teal, 490, 468, 18, 40));
                    b.AddLight(Img(p, o, "O2", new Color(0.40f, 0.85f, 0.70f), 515, 468, 22, 34, 0, SpriteFactory.Circle));
                    Img(p, o, "Puerta", Roof, 497, 532, 22, 36);
                    foreach (var tk in new[] { new Vector3(568, 500, 92), new Vector3(588, 495, 98), new Vector3(608, 488, 100) })
                    {
                        Img(p, o, "TanqueContorno", Outline, tk.x, tk.y, 22, tk.z + 4);
                        b.AddLight(Img(p, o, "Tanque", new Color(0.45f, 0.47f, 0.50f), tk.x, tk.y, 16, tk.z));
                    }
                    break;

                default: // Vivienda
                    Block(p, o, 860, 362, 62, 46, 22);
                    Block(p, o, 860, 470, 150, 122, 40);
                    b.AddLight(Img(p, o, "Franja1", Teal, 840, 488, 64, 20));
                    b.AddLight(Img(p, o, "Franja2", Teal, 905, 455, 40, 30));
                    b.AddLight(Img(p, o, "Casa", new Color(1f, 0.85f, 0.25f), 810, 465, 28, 32));
                    Img(p, o, "AnexoContorno", Outline, 915, 530, 66, 46);
                    Img(p, o, "Anexo", Roof, 915, 530, 58, 38);
                    b.AddLight(Img(p, o, "AnexoVentana", Teal, 922, 532, 34, 12));
                    break;
            }
        }

        // ---------------------------------------------------------------- estado

        public void SetState(GameState s, BalanceConfig c)
        {
            bool building = s.roverBusyTurns > 0 && s.roverTask == RoverTask.Regolith;
            igloo.gameObject.SetActive(s.regolithReady || building);
            igloo.rectTransform.localScale = Vector3.one * (s.regolithReady ? 1f : 0.55f);
            // El módulo de agua no muestra números: la gota se apaga cuando queda poca agua.
            if (waterDrop != null) waterDrop.color = s.water < c.lowWaterThreshold ? new Color(0.55f, 0.35f, 0.30f) : new Color(0.45f, 0.80f, 0.95f);
            rover.SetState(s);
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            if (waterDrop != null) waterDrop.rectTransform.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(t * 2f));
        }
    }
}
