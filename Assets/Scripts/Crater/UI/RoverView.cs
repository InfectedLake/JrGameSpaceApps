using Crater.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Rover junto al tanque de agua (README). Es un único objeto que se conserva en todas las variantes.
    /// PENDIENTE DE ARTE: la carpeta Sprites no incluye un rover; mientras CraterArtSet.rover esté vacío se
    /// dibuja un marcador temporal en la misma posición. Al asignar rover / roverBroken se usa el arte original.
    /// </summary>
    public class RoverView : MonoBehaviour
    {
        Image sprite, smoke;
        Sprite normal, broken;
        GameObject placeholder;
        float t;
        bool isBroken, repairing;

        public static RoverView Create(RectTransform world, Sprite normal, Sprite broken)
        {
            var size = normal != null ? normal.rect.size : CraterLayout.RoverSize;
            var root = UIFactory.Rect(world, "Rover").TopLeft(CraterLayout.RoverCenter - size / 2f, size);
            var rv = root.gameObject.AddComponent<RoverView>();
            rv.normal = normal; rv.broken = broken;
            if (normal != null)
            {
                rv.sprite = UIFactory.Image(root, "Sprite", Color.white, normal);
                rv.sprite.rectTransform.Stretch();
            }
            else rv.placeholder = DrawPlaceholder(root);
            rv.smoke = UIFactory.Image(root, "Humo", new Color(0.35f, 0.35f, 0.38f, 0), SpriteFactory.Glow);
            rv.smoke.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 10), new Vector2(70, 70));
            return rv;
        }

        static GameObject DrawPlaceholder(RectTransform root)
        {
            var g = UIFactory.Rect(root, "MarcadorTemporal").Stretch();
            var outline = new Color(0.08f, 0.08f, 0.09f);
            for (int i = 0; i < 3; i++)
            {
                var w = UIFactory.Image(g, "Rueda", outline, SpriteFactory.Circle);
                w.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-36 + i * 36, -24), new Vector2(28, 28));
            }
            var o = UIFactory.Image(g, "Contorno", outline);
            o.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(104, 44));
            var b = UIFactory.Image(g, "Cuerpo", new Color(0.80f, 0.82f, 0.85f));
            b.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(96, 36));
            var s = UIFactory.Image(g, "Franja", new Color(0.35f, 0.75f, 0.72f));
            s.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(96, 8));
            var m = UIFactory.Image(g, "Mastil", outline);
            m.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(30, 32), new Vector2(8, 26));
            return g.gameObject;
        }

        public void SetState(GameState s)
        {
            if (s == null) return;
            isBroken = s.rovers.broken && !s.rovers.Repairing;
            repairing = s.rovers.Repairing;
            if (sprite != null) sprite.sprite = isBroken && broken != null ? broken : normal;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            bool showSmoke = (isBroken && broken == null) || repairing;
            smoke.color = new Color(0.35f, 0.35f, 0.38f, showSmoke ? 0.35f + 0.2f * Mathf.Sin(t * 4f) : 0f);
            smoke.rectTransform.anchoredPosition = new Vector2(0, 10 + Mathf.Repeat(t * 18f, 18f));
        }
    }
}
