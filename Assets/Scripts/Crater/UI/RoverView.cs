using Crater.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Rover animado: va y viene entre el hangar y el depósito de hielo cuando recolecta,
    /// construye el iglú de regolito, o queda guardado / en reparación.
    /// </summary>
    public class RoverView : MonoBehaviour
    {
        public enum Mode { Collecting, Building, Parked, Repairing }

        public Vector2 garage, ice, igloo;
        public float size = 1f;
        RectTransform rt, drill;
        Image body, smoke;
        Mode mode = Mode.Collecting;
        float speedFactor = 1f;
        float t;

        public static RoverView Create(Transform parent, Vector2 garage, Vector2 ice, Vector2 igloo)
        {
            var root = UIFactory.Rect(parent, "Rover").Place(new Vector2(0.5f, 0.5f), garage, new Vector2(90, 60));
            var rv = root.gameObject.AddComponent<RoverView>();
            rv.rt = root; rv.garage = garage; rv.ice = ice; rv.igloo = igloo;
            for (int i = 0; i < 3; i++)
            {
                var w = UIFactory.Image(root, "Rueda", new Color(0.22f, 0.24f, 0.28f), SpriteFactory.Circle);
                w.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-30 + i * 30, -22), new Vector2(26, 26));
            }
            rv.body = UIFactory.Image(root, "Cuerpo", new Color(0.97f, 0.97f, 1f), SpriteFactory.Rounded);
            rv.body.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(84, 40));
            var stripe = UIFactory.Image(root, "Franja", Theme.Accent, SpriteFactory.Rounded);
            stripe.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(84, 10));
            var cam = UIFactory.Image(root, "Camara", new Color(0.3f, 0.35f, 0.45f), SpriteFactory.Rounded);
            cam.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(22, 30), new Vector2(18, 26));
            rv.drill = UIFactory.Image(root, "Taladro", new Color(0.55f, 0.58f, 0.65f), SpriteFactory.Rounded).rectTransform;
            rv.drill.Place(new Vector2(0.5f, 0.5f), new Vector2(48, -6), new Vector2(26, 12));
            rv.smoke = UIFactory.Image(root, "Humo", new Color(0.4f, 0.4f, 0.45f, 0), SpriteFactory.Glow);
            rv.smoke.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-10, 40), new Vector2(60, 60));
            return rv;
        }

        public void SetState(GameState s)
        {
            if (s == null) return;
            if (s.rovers.Repairing) mode = Mode.Repairing;
            else if (s.roverBusyTurns > 0 && s.roverTask == RoverTask.Regolith) mode = Mode.Building;
            else if (s.today.roversStowed || s.rovers.Effective <= 0) mode = Mode.Parked;
            else mode = Mode.Collecting;
            speedFactor = s.rovers.broken ? 0.45f : 1f;
            body.color = s.rovers.broken ? new Color(1f, 0.85f, 0.7f) : new Color(0.97f, 0.97f, 1f);
        }

        void Update()
        {
            t += Time.unscaledDeltaTime * speedFactor;
            Vector2 target;
            switch (mode)
            {
                case Mode.Collecting:
                    float k = Mathf.PingPong(t * 0.12f, 1f);
                    float e = k * k * (3 - 2 * k);
                    target = Vector2.Lerp(garage, ice, e);
                    rt.localScale = new Vector3(Mathf.PingPong(t * 0.12f, 2f) < 1f ? size : -size, size, 1);
                    drill.localEulerAngles = new Vector3(0, 0, k > 0.95f ? Mathf.Sin(t * 40f) * 12f : 0);
                    break;
                case Mode.Building:
                    target = igloo + new Vector2(Mathf.Sin(t * 1.5f) * 40f * size, 0);
                    rt.localScale = new Vector3(Mathf.Cos(t * 1.5f) > 0 ? size : -size, size, 1);
                    drill.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * 20f) * 10f);
                    break;
                default:
                    target = garage;
                    rt.localScale = new Vector3(size, size, 1);
                    drill.localEulerAngles = Vector3.zero;
                    break;
            }
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, target, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 6f));
            float sa = mode == Mode.Repairing || speedFactor < 1f ? 0.35f + 0.25f * Mathf.Sin(t * 4f) : 0f;
            smoke.color = new Color(0.4f, 0.4f, 0.45f, sa);
            smoke.rectTransform.anchoredPosition = new Vector2(-10, 40 + Mathf.Repeat(t * 20f, 20f));
        }
    }
}
