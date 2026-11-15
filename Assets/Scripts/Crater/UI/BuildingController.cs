using System;
using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using Crater.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Un edificio funcional de la base: zona seleccionable, estado visual (encendido, apagado,
    /// sin energía, averiado) y animación sencilla. El dibujo lo hace BaseView con placeholders;
    /// si BuildingData.icon tiene un sprite, se usa ese arte en su lugar.
    /// </summary>
    public class BuildingController : MonoBehaviour
    {
        public BuildingId id;
        public event Action<BuildingId> Selected;

        /// <summary>Contenedor del arte del edificio (placeholder o sprite definitivo).</summary>
        public RectTransform Art { get; private set; }
        public RectTransform Rect => (RectTransform)transform;

        CanvasGroup artGroup;
        Image alarm, badge, selection;
        TextMeshProUGUI badgeText, nameTag;
        readonly List<Image> lights = new List<Image>();
        readonly List<Image> cells = new List<Image>();
        readonly List<Color> lightBase = new List<Color>();
        BuildingVisual visual = BuildingVisual.Working;
        bool selected;
        float t;

        public static BuildingController Create(Transform parent, BuildingData data, Vector2 pos, Vector2 size)
        {
            var root = UIFactory.Rect(parent, "Edificio_" + data.stats.id).Place(new Vector2(0.5f, 0.5f), pos, size);
            var bc = root.gameObject.AddComponent<BuildingController>();
            bc.id = data.stats.id;
            bc.Build(data, size);
            return bc;
        }

        void Build(BuildingData data, Vector2 size)
        {
            alarm = UIFactory.Image(transform, "Alarma", new Color(1, 0.2f, 0.2f, 0), SpriteFactory.Glow);
            alarm.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.35f);
            selection = UIFactory.Image(transform, "Seleccion", new Color(1f, 0.85f, 0.3f, 0), SpriteFactory.RoundedOutline);
            selection.rectTransform.Stretch(-10, -10, -10, -10);

            Art = UIFactory.Rect(transform, "Arte").Stretch();
            artGroup = Art.gameObject.AddComponent<CanvasGroup>();
            artGroup.blocksRaycasts = false;
            if (data.icon != null)
            {
                var icon = UIFactory.Image(Art, "Sprite", Color.white, data.icon);
                icon.preserveAspect = true;
                icon.rectTransform.Stretch();
            }

            // Zona interactiva (todo el edificio)
            var hit = UIFactory.Image(transform, "Seleccionar", new Color(1, 1, 1, 0.001f), null, true);
            hit.rectTransform.Stretch();
            var button = hit.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => Selected?.Invoke(id));

            badge = UIFactory.Image(transform, "Estado", Theme.Good, SpriteFactory.Circle);
            badge.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0, 18), new Vector2(46, 46));
            badgeText = UIFactory.Text(badge.transform, "T", "ON", 16, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            badgeText.rectTransform.Stretch();

            var tag = UIFactory.Panel(transform, "Nombre", new Color(0.08f, 0.12f, 0.22f, 0.75f), false);
            tag.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0, -14), new Vector2(190, 32));
            nameTag = UIFactory.Text(tag.transform, "T", data.stats.name, 18, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            nameTag.rectTransform.Stretch();
        }

        public void AddLight(Image img) { lights.Add(img); lightBase.Add(img.color); }
        public void AddCell(Image img) => cells.Add(img);

        public void SetSelected(bool on) => selected = on;

        public void Refresh(BuildingResult r)
        {
            if (r == null) return;
            visual = r.visual;
            artGroup.alpha = visual == BuildingVisual.Off ? 0.5f : 1f;

            switch (visual)
            {
                case BuildingVisual.Working: badge.color = Theme.Good; badgeText.text = "ON"; break;
                case BuildingVisual.Off: badge.color = Theme.OffGray; badgeText.text = "OFF"; break;
                case BuildingVisual.NoPower:
                case BuildingVisual.NoWater: badge.color = Theme.Bad; badgeText.text = "!"; break;
                default: badge.color = Theme.Warn; badgeText.text = visual == BuildingVisual.Broken ? "!" : "II"; break;
            }
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            bool anim = visual == BuildingVisual.Working || visual == BuildingVisual.Broken;
            float speed = visual == BuildingVisual.Broken ? 0.45f : 1f;

            alarm.color = new Color(1f, 0.2f, 0.2f, visual == BuildingVisual.NoPower || visual == BuildingVisual.NoWater ? 0.3f + 0.25f * Mathf.Sin(t * 5f) : 0f);
            selection.color = new Color(1f, 0.85f, 0.3f, selected ? 0.7f + 0.3f * Mathf.Sin(t * 4f) : 0f);
            bool alert = visual != BuildingVisual.Working && visual != BuildingVisual.Off;
            badge.rectTransform.localScale = Vector3.one * (alert ? 1f + 0.12f * Mathf.Sin(t * 6f) : 1f);

            for (int i = 0; i < lights.Count; i++)
            {
                var b = lightBase[i];
                float k = anim ? 0.75f + 0.25f * Mathf.Sin(t * 2.5f * speed + i) : 0.45f;
                lights[i].color = new Color(b.r * k, b.g * k, b.b * k, b.a);
            }
            for (int i = 0; i < cells.Count; i++)
            {
                float w = anim ? 0.5f + 0.5f * Mathf.Sin(t * 3f * speed - i * 0.8f) : 0f;
                cells[i].color = Color.Lerp(new Color(0.12f, 0.25f, 0.55f), new Color(0.35f, 0.55f, 0.95f), w);
            }
        }
    }
}
