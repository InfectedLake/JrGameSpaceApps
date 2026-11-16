using Crater.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Estado visual del rover. El rover YA está dibujado en el sprite original "edificio agua y comida … 1"
    /// (junto al tanque de agua), así que aquí no se dibuja otro rover: solo se muestra humo encima cuando
    /// está averiado o en reparación (no hay sprite de rover roto en la carpeta original).
    /// </summary>
    public class RoverView : MonoBehaviour
    {
        Image smoke;
        float t;
        bool damaged;

        public static RoverView Create(RectTransform world)
        {
            var root = UIFactory.Rect(world, "RoverEstado").TopLeft(CraterLayout.RoverCenter - new Vector2(60, 60), new Vector2(120, 120));
            var rv = root.gameObject.AddComponent<RoverView>();
            rv.smoke = UIFactory.Image(root, "Humo", new Color(0.35f, 0.35f, 0.38f, 0), SpriteFactory.Glow);
            rv.smoke.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80));
            return rv;
        }

        public void SetState(GameState s)
        {
            if (s == null) return;
            damaged = s.rovers.broken || s.rovers.Repairing;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            smoke.color = new Color(0.35f, 0.35f, 0.38f, damaged ? 0.35f + 0.2f * Mathf.Sin(t * 4f) : 0f);
            smoke.rectTransform.anchoredPosition = new Vector2(0, 10 + Mathf.Repeat(t * 18f, 30f));
        }
    }
}
