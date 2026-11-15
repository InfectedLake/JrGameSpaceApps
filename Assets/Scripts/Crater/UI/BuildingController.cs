using System;
using Crater.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Un edificio funcional de la base con su arte original. Es UN solo objeto lógico: las variantes
    /// (normal, encendida en el eclipse, rota por micrometeoritos) solo cambian el sprite de la misma Image,
    /// y las capas de la tormenta (agua / polvo) son hijos encima. La selección siempre llega al mismo edificio.
    /// </summary>
    public class BuildingController : MonoBehaviour
    {
        public BuildingId id;
        public event Action<BuildingId> Selected;
        public RectTransform Rect => (RectTransform)transform;

        Image main, water, dust;
        Sprite normal, lit, broken;
        Vector2 size, litOffset, litSize;
        BuildingVisual visual = BuildingVisual.Working;
        bool selectable = true;
        float t;

        public static BuildingController Create(RectTransform world, BuildingId id, Sprite normal, Sprite lit, Sprite broken, Sprite water, Sprite dust)
        {
            var pos = CraterLayout.BuildingPos(id);
            var size = normal != null ? normal.rect.size : new Vector2(200, 200);
            var root = UIFactory.Rect(world, "Edificio_" + id).TopLeft(pos, size);
            var bc = root.gameObject.AddComponent<BuildingController>();
            bc.id = id;
            bc.normal = normal; bc.lit = lit; bc.broken = broken; bc.size = size;
            bc.litOffset = CraterLayout.LitOffset(id);
            bc.litSize = lit != null ? lit.rect.size : size;
            bc.Build(water, dust);
            return bc;
        }

        void Build(Sprite waterSprite, Sprite dustSprite)
        {
            main = UIFactory.Image(transform, "Sprite", Color.white, normal, true);
            main.rectTransform.TopLeft(Vector2.zero, size);
            // Clic solo sobre píxeles opacos (la textura es legible por la configuración de importación).
            if (normal != null && normal.texture != null && normal.texture.isReadable) main.alphaHitTestMinimumThreshold = 0.5f;
            var area = CraterLayout.HitArea(id, size);
            if (area.size != size) main.gameObject.AddComponent<RectHitFilter>().area = area;
            var button = main.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => { if (selectable) Selected?.Invoke(id); });

            water = Overlay("CapaAgua", waterSprite, CraterLayout.WaterOffset(id));
            dust = Overlay("CapaPolvo", dustSprite, CraterLayout.DustOffset(id));
        }

        Image Overlay(string name, Sprite s, Vector2 offset)
        {
            if (s == null) return null;
            var img = UIFactory.Image(transform, name, Color.white, s);
            img.rectTransform.TopLeft(offset, s.rect.size);
            img.gameObject.SetActive(false);
            return img;
        }

        public void SetSelectable(bool on) => selectable = on;

        /// <param name="litOn">Eclipse: la sección de este edificio tiene baterías.</param>
        /// <param name="brokenOn">Micrometeoritos: equipo dañado (solo existe arte para la energía).</param>
        public void Refresh(BuildingResult r, bool litOn, bool brokenOn, bool waterOn, bool dustOn)
        {
            if (r != null) visual = r.visual;
            Sprite s = normal;
            Vector2 pos = Vector2.zero, sz = size;
            if (litOn && lit != null) { s = lit; pos = litOffset; sz = litSize; }
            else if (brokenOn && broken != null) s = broken;
            if (main.sprite != s) main.sprite = s;
            main.rectTransform.TopLeft(pos, sz);
            if (water != null) water.gameObject.SetActive(waterOn);
            if (dust != null) dust.gameObject.SetActive(dustOn && !waterOn);
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            Color c;
            switch (visual)
            {
                case BuildingVisual.Off: c = new Color(0.55f, 0.57f, 0.62f); break;
                case BuildingVisual.NoPower:
                case BuildingVisual.NoWater: c = Color.Lerp(Color.white, new Color(1f, 0.45f, 0.45f), 0.5f + 0.5f * Mathf.Sin(t * 5f)); break;
                case BuildingVisual.Paused:
                case BuildingVisual.Inactive: c = new Color(0.85f, 0.85f, 0.78f); break;
                default: c = Color.white; break;
            }
            main.color = c;
        }
    }

    /// <summary>Limita la zona clicable a un rectángulo del sprite (píxeles, origen arriba a la izquierda).</summary>
    public class RectHitFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        public Rect area;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            var rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, eventCamera, out var local)) return false;
            var r = rt.rect;
            var img = GetComponent<Image>();
            var spriteSize = img != null && img.sprite != null ? img.sprite.rect.size : r.size;
            float px = (local.x - r.xMin) / r.width * spriteSize.x;
            float py = (r.yMax - local.y) / r.height * spriteSize.y;
            return area.Contains(new Vector2(px, py));
        }
    }
}
