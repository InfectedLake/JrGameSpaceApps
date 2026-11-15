using System.Collections;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Animaciones breves de eventos: radiación (destello y partículas), micrometeoritos
    /// (estelas que caen) y eclipse (oscurecimiento del escenario mientras dura).
    /// </summary>
    public class EffectsLayer : MonoBehaviour
    {
        Image flash, eclipseShade;
        RectTransform particles;
        bool eclipse;
        float eclipseAlpha;

        public static EffectsLayer Create(Transform parent)
        {
            var root = UIFactory.Rect(parent, "Efectos").Stretch();
            var fx = root.gameObject.AddComponent<EffectsLayer>();
            fx.eclipseShade = UIFactory.Image(root, "Eclipse", new Color(0.03f, 0.05f, 0.18f, 0));
            fx.eclipseShade.rectTransform.Stretch();
            fx.particles = UIFactory.Rect(root, "Particulas").Stretch();
            fx.flash = UIFactory.Image(root, "Destello", new Color(1f, 0.95f, 0.6f, 0));
            fx.flash.rectTransform.Stretch();
            return fx;
        }

        public void SetEclipse(bool active) => eclipse = active;

        void Update()
        {
            eclipseAlpha = Mathf.MoveTowards(eclipseAlpha, eclipse ? 0.55f : 0f, Time.unscaledDeltaTime * 0.6f);
            eclipseShade.color = new Color(0.03f, 0.05f, 0.18f, eclipseAlpha);
        }

        public void Play(CraterEventType type, int intensity)
        {
            switch (type)
            {
                case CraterEventType.SolarRadiation: StartCoroutine(Radiation(Mathf.Max(1, intensity))); break;
                case CraterEventType.Micrometeorites: StartCoroutine(Meteors()); break;
                case CraterEventType.EarthShadow: eclipse = true; break;
            }
        }

        IEnumerator Radiation(int intensity)
        {
            var list = new List<(RectTransform rt, Vector2 v)>();
            var size = ((RectTransform)transform).rect.size;
            for (int i = 0; i < 30 * intensity; i++)
            {
                var p = UIFactory.Image(particles, "P", new Color(1f, 0.85f, 0.3f, 0.9f), SpriteFactory.Glow);
                p.rectTransform.Place(new Vector2(1, 1), new Vector2(Random.Range(-300f, 0f), Random.Range(-200f, 100f)), Vector2.one * Random.Range(12f, 26f));
                list.Add((p.rectTransform, new Vector2(Random.Range(-900f, -500f), Random.Range(-500f, -250f))));
            }
            float t = 0;
            while (t < 1.4f)
            {
                t += Time.unscaledDeltaTime;
                flash.color = new Color(1f, 0.95f, 0.6f, Mathf.Clamp01(0.55f * (1f - t / 0.9f)) * Mathf.Abs(Mathf.Sin(t * 14f)));
                foreach (var (rt, v) in list) rt.anchoredPosition += v * Time.unscaledDeltaTime;
                yield return null;
            }
            flash.color = new Color(1, 1, 1, 0);
            foreach (var (rt, _) in list) Destroy(rt.gameObject);
        }

        IEnumerator Meteors()
        {
            var list = new List<(RectTransform rt, Vector2 v)>();
            for (int i = 0; i < 26; i++)
            {
                var m = UIFactory.Image(particles, "M", new Color(1f, 0.75f, 0.5f, 0.95f), SpriteFactory.Rounded);
                m.rectTransform.Place(new Vector2(Random.value, 1), new Vector2(0, Random.Range(50f, 600f)), new Vector2(10, 46));
                m.rectTransform.localEulerAngles = new Vector3(0, 0, 25);
                list.Add((m.rectTransform, new Vector2(-420f, -900f) * Random.Range(0.8f, 1.3f)));
            }
            float t = 0;
            while (t < 2f)
            {
                t += Time.unscaledDeltaTime;
                foreach (var (rt, v) in list) rt.anchoredPosition += v * Time.unscaledDeltaTime;
                flash.color = new Color(1f, 0.8f, 0.6f, t > 0.8f && t < 1.2f ? 0.15f : 0f);
                yield return null;
            }
            flash.color = new Color(1, 1, 1, 0);
            foreach (var (rt, _) in list) Destroy(rt.gameObject);
        }
    }
}
