using System.Collections;
using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Capas de pantalla de los eventos (encima de la base, debajo de la interfaz), con el arte original:
    ///  - Eclipse: "filtro de obsuresimiento" + "sol del eclipse" + "tierra del eclipse" arriba al centro.
    ///  - Micrometeoritos: "meteoritos de fondo" sobre la escena mientras dura la alerta.
    ///  - Radiación: destello breve (no hay sprite para este efecto).
    /// </summary>
    public class EffectsLayer : MonoBehaviour
    {
        Image flash, filter, sun, earth, meteors;
        CanvasGroup eclipseGroup;
        bool eclipse, meteorsOn;
        float eclipseAlpha, meteorsAlpha, meteorsT;

        public static EffectsLayer Create(Transform parent, CraterArtSet art)
        {
            var root = UIFactory.Rect(parent, "Efectos").Stretch();
            var fx = root.gameObject.AddComponent<EffectsLayer>();

            var eg = UIFactory.Rect(root, "Eclipse").Stretch();
            fx.eclipseGroup = eg.gameObject.AddComponent<CanvasGroup>();
            fx.eclipseGroup.blocksRaycasts = false;
            fx.eclipseGroup.alpha = 0;
            if (art.eclipseFilter != null)
            {
                // Se conserva la proporción del filtro (3003 × 1627) cubriendo toda la pantalla.
                var size = art.eclipseFilter.rect.size * (CraterLayout.ViewSize.y / art.eclipseFilter.rect.height);
                fx.filter = UIFactory.Image(eg, "Filtro", Color.white, art.eclipseFilter);
                fx.filter.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, size);
            }
            if (art.eclipseSun != null)
            {
                fx.sun = UIFactory.Image(eg, "SolEclipse", Color.white, art.eclipseSun);
                fx.sun.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 106 - art.eclipseSun.rect.height / 2f), art.eclipseSun.rect.size);
            }
            if (art.eclipseEarth != null)
            {
                fx.earth = UIFactory.Image(eg, "TierraEclipse", Color.white, art.eclipseEarth);
                fx.earth.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 106 - art.eclipseSun.rect.height / 2f), art.eclipseEarth.rect.size);
            }
            if (art.meteorsOverlay != null)
            {
                var size = art.meteorsOverlay.rect.size * (CraterLayout.ViewSize.x / art.meteorsOverlay.rect.width);
                fx.meteors = UIFactory.Image(root, "Meteoritos", new Color(1, 1, 1, 0), art.meteorsOverlay);
                fx.meteors.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -60), size, new Vector2(0.5f, 1));
            }
            fx.flash = UIFactory.Image(root, "Destello", new Color(1f, 0.95f, 0.6f, 0));
            fx.flash.rectTransform.Stretch();
            return fx;
        }

        public void SetEclipse(bool active) => eclipse = active;
        public void SetMeteors(bool active) => meteorsOn = active;

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            eclipseAlpha = Mathf.MoveTowards(eclipseAlpha, eclipse ? 1f : 0f, dt * 1.5f);
            eclipseGroup.alpha = eclipseAlpha;
            if (meteors != null)
            {
                meteorsAlpha = Mathf.MoveTowards(meteorsAlpha, meteorsOn ? 1f : 0f, dt * 2f);
                meteorsT += dt;
                meteors.color = new Color(1, 1, 1, meteorsAlpha);
                meteors.rectTransform.anchoredPosition = new Vector2(-6f * Mathf.Sin(meteorsT * 0.8f), -60 - 10f * Mathf.Repeat(meteorsT * 0.5f, 1f));
            }
        }

        public void Play(CraterEventType type, int intensity)
        {
            if (type == CraterEventType.SolarRadiation) StartCoroutine(Radiation(Mathf.Max(1, intensity)));
            if (type == CraterEventType.EarthShadow) eclipse = true;
            if (type == CraterEventType.Micrometeorites) StartCoroutine(MeteorShower());
        }

        IEnumerator Radiation(int intensity)
        {
            float t = 0;
            while (t < 1.2f)
            {
                t += Time.unscaledDeltaTime;
                flash.color = new Color(1f, 0.95f, 0.6f, Mathf.Clamp01(0.45f * intensity / 3f + 0.15f) * Mathf.Clamp01(1f - t / 1.2f) * Mathf.Abs(Mathf.Sin(t * 12f)));
                yield return null;
            }
            flash.color = new Color(1, 1, 1, 0);
        }

        IEnumerator MeteorShower()
        {
            meteorsOn = true;
            yield return new WaitForSecondsRealtime(2f);
        }
    }
}
