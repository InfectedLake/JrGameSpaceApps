using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using Crater.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    // Ventanas de eventos (alerta + ilustración + decisiones) y tutorial.
    public partial class UIManager
    {
        // ------------------------------------------------------------------ Utilidades

        /// <summary>Ventana centrada con fondo oscurecido y encabezado de color con ilustración.</summary>
        RectTransform Modal(string title, Color header, Vector2 size, out RectTransform content, Transform layer = null, CraterEventType? illustration = null, Sprite image = null)
        {
            bool artUI = UIFactory.ArtPanel != null;
            // Con el arte original el fondo se oscurece poco: la base muestra la variante del evento detrás.
            var overlay = UIFactory.Image(layer ?? modalLayer, "Ventana_" + title, new Color(0.02f, 0.04f, 0.12f, artUI ? 0.35f : 0.65f), null, true).rectTransform;
            overlay.Stretch();
            var cardImg = UIFactory.Panel(overlay, "Tarjeta", Theme.Panel);
            cardImg.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, artUI ? -60 : 0), size);
            if (artUI)
            {
                var artBox = UIFactory.Rect(cardImg.transform, "Ilustracion").Place(new Vector2(0, 1), new Vector2(36, -40), new Vector2(110, 110), new Vector2(0, 1));
                bool hasArt = DrawArtIllustration(artBox, illustration, image);
                var tt = UIFactory.Text(cardImg.transform, "Titulo", title, 52, new Color(0.25f, 0.15f, 0.02f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
                tt.rectTransform.anchorMin = new Vector2(0, 1); tt.rectTransform.anchorMax = new Vector2(1, 1);
                tt.rectTransform.pivot = new Vector2(0.5f, 1);
                tt.rectTransform.offsetMin = new Vector2(hasArt ? 166 : 40, -150); tt.rectTransform.offsetMax = new Vector2(-30, -36);
                content = UIFactory.Rect(cardImg.transform, "Contenido").Stretch(40, 30, 40, 160);
                return overlay;
            }
            var head = UIFactory.Panel(cardImg.transform, "Encabezado", header, false);
            head.rectTransform.anchorMin = new Vector2(0, 1); head.rectTransform.anchorMax = new Vector2(1, 1);
            head.rectTransform.pivot = new Vector2(0.5f, 1);
            head.rectTransform.offsetMin = new Vector2(0, -130); head.rectTransform.offsetMax = Vector2.zero;
            var art = UIFactory.Rect(head.transform, "Ilustracion").Place(new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(110, 110), new Vector2(0, 0.5f));
            DrawIllustration(art, illustration, image);
            var t = UIFactory.Text(head.transform, "Titulo", title, 48, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            t.rectTransform.Stretch(160, 4, 24, 4);
            content = UIFactory.Rect(cardImg.transform, "Contenido").Stretch(32, 24, 32, 148);
            return overlay;
        }

        /// <summary>Ilustración con arte original: EventData.image o, para el eclipse, el Sol y la Tierra del equipo.</summary>
        bool DrawArtIllustration(RectTransform box, CraterEventType? type, Sprite image)
        {
            if (image != null)
            {
                var img = UIFactory.Image(box, "Imagen", Color.white, image);
                img.preserveAspect = true;
                img.rectTransform.Stretch();
                return true;
            }
            if (type == CraterEventType.EarthShadow && art.eclipseSun != null && art.eclipseEarth != null)
            {
                var sunImg = UIFactory.Image(box, "Sol", Color.white, art.eclipseSun);
                sunImg.rectTransform.Stretch();
                var earthImg = UIFactory.Image(box, "Tierra", Color.white, art.eclipseEarth);
                earthImg.rectTransform.Stretch(6, 6, 6, 6);
                return true;
            }
            return false;
        }

        /// <summary>Ilustración del evento: el sprite de EventData.image o un dibujo simple de placeholder.</summary>
        static void DrawIllustration(RectTransform art, CraterEventType? type, Sprite image)
        {
            var bg = UIFactory.Image(art, "Fondo", new Color(0.05f, 0.08f, 0.18f, 0.9f), SpriteFactory.Circle);
            bg.rectTransform.Stretch();
            if (image != null)
            {
                var img = UIFactory.Image(art, "Imagen", Color.white, image);
                img.preserveAspect = true;
                img.rectTransform.Stretch(8, 8, 8, 8);
                return;
            }
            Image Dot(Color c, Sprite s, Vector2 pos, Vector2 size, float rot = 0)
            {
                var i = UIFactory.Image(art, "Parte", c, s);
                i.rectTransform.Place(new Vector2(0.5f, 0.5f), pos, size);
                i.rectTransform.localEulerAngles = new Vector3(0, 0, rot);
                return i;
            }
            switch (type)
            {
                case CraterEventType.SolarRadiation:
                    Dot(new Color(1f, 0.8f, 0.3f, 0.6f), SpriteFactory.Glow, Vector2.zero, new Vector2(110, 110));
                    for (int i = 0; i < 8; i++) Dot(new Color(1f, 0.85f, 0.3f), SpriteFactory.Rounded, Vector2.zero, new Vector2(8, 96), i * 22.5f);
                    Dot(new Color(1f, 0.9f, 0.4f), SpriteFactory.Circle, Vector2.zero, new Vector2(54, 54));
                    break;
                case CraterEventType.Micrometeorites:
                    foreach (var p in new[] { new Vector2(-22, 20), new Vector2(18, 4), new Vector2(-4, -26) })
                    {
                        Dot(new Color(1f, 0.6f, 0.3f, 0.6f), SpriteFactory.Rounded, p + new Vector2(14, 22), new Vector2(10, 44), 32);
                        Dot(new Color(0.65f, 0.55f, 0.5f), SpriteFactory.Circle, p, new Vector2(20, 18));
                    }
                    break;
                case CraterEventType.EarthShadow:
                    Dot(new Color(1f, 0.85f, 0.4f, 0.5f), SpriteFactory.Glow, new Vector2(10, 6), new Vector2(100, 100));
                    Dot(new Color(1f, 0.9f, 0.4f), SpriteFactory.Circle, new Vector2(10, 6), new Vector2(50, 50));
                    Dot(new Color(0.15f, 0.30f, 0.65f), SpriteFactory.Circle, new Vector2(-6, -2), new Vector2(58, 58));
                    break;
                default:
                    Dot(Theme.Bad, SpriteFactory.Circle, Vector2.zero, new Vector2(70, 70));
                    var ex = UIFactory.Text(art, "!", "!", 52, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
                    ex.rectTransform.Stretch();
                    break;
            }
        }

        static Color ResourceColor(ResourceKind r) =>
            r == ResourceKind.Energy ? Theme.Energy : r == ResourceKind.Oxygen ? Theme.Oxygen : r == ResourceKind.Food ? Theme.Food : Theme.Health;

        static string ResourceLetter(ResourceKind r) =>
            r == ResourceKind.Energy ? "E" : r == ResourceKind.Oxygen ? "O2" : r == ResourceKind.Food ? "C" : "S";

        static TextMeshProUGUI Line(Transform parent, string text, float size = 24, Color? color = null, FontStyles style = FontStyles.Normal)
        {
            var t = UIFactory.Text(parent, "Linea", text, size, color ?? Theme.Ink, TextAlignmentOptions.Left, style);
            t.gameObject.AddComponent<LayoutElement>();
            return t;
        }

        // ------------------------------------------------------------------ Eventos

        void ShowPrompt(EventPrompt p)
        {
            if (eventOverlay != null) Destroy(eventOverlay.gameObject);
            eventOverlay = null;
            CloseBuildingCard(true);
            RefreshGame();
            if (p == null) return;

            bool eclipse = p.kind == PromptKind.EclipseAllocation;
            bool four = p.options.Count >= 4;
            var size = new Vector2(1180, eclipse ? (UIFactory.ArtPanel != null ? 780 : 860) : four ? 800 : p.kind == PromptKind.Info ? 540 : 640);
            eventOverlay = Modal(p.title, p.color * 0.85f + Color.black * 0.15f, size, out var content, null, p.eventType, p.image);

            var v = UIFactory.VStack(content.gameObject, 12);
            v.childForceExpandHeight = false;
            if (!string.IsNullOrEmpty(p.subtitle)) Line(content, p.subtitle, 32, Theme.Ink, FontStyles.Bold);
            if (!string.IsNullOrEmpty(p.body)) Line(content, p.body, 26, Theme.Ink);
            if (!string.IsNullOrEmpty(p.dataLabel)) Line(content, p.dataLabel, 18, Theme.InkSoft, FontStyles.Italic);

            if (eclipse) BuildEclipseAllocation(content, p);
            else BuildPromptOptions(content, p);
        }

        void BuildPromptOptions(RectTransform content, EventPrompt p)
        {
            bool info = p.kind == PromptKind.Info;
            bool twoCols = p.options.Count >= 4;
            float w = twoCols ? 552 : 1116;
            float h = info ? 84 : twoCols ? 200 : 130;
            var grid = UIFactory.Rect(content, "Opciones");
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(w, h);
            g.spacing = new Vector2(12, 12);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = twoCols ? 2 : 1;
            int rows = Mathf.CeilToInt(p.options.Count / (twoCols ? 2f : 1f));
            UIFactory.Size(grid, rows * h + (rows - 1) * 12);

            foreach (var o in p.options)
            {
                var opt = o;
                var b = UIFactory.Button(grid, o.label, o.label, o.enabled ? (info ? Theme.Accent : Color.white) : Theme.Disabled, info ? Color.white : Theme.Ink, 30, () => G.events.Choose(opt));
                b.interactable = o.enabled;
                if (info) continue;
                var outline = UIFactory.Image(b.transform, "Borde", p.color, SpriteFactory.RoundedOutline);
                outline.enabled = UIFactory.ArtPanel == null;
                outline.rectTransform.Stretch();
                var lbl = b.Label();
                lbl.rectTransform.Stretch(22, 12, 22, 12);
                lbl.alignment = TextAlignmentOptions.TopLeft;
                lbl.fontStyle = FontStyles.Normal;
                if (o.resource.HasValue)
                {
                    // Evento menor: icono del recurso + nombre + cantidad que se pierde.
                    var r = o.resource.Value;
                    var icon = UIFactory.Image(b.transform, "Icono", ResourceColor(r), SpriteFactory.Circle);
                    icon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(84, 84), new Vector2(0, 0.5f));
                    var letter = UIFactory.Text(icon.transform, "Letra", ResourceLetter(r), 30, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
                    letter.rectTransform.Stretch();
                    lbl.rectTransform.Stretch(126, 12, 22, 12);
                    lbl.alignment = TextAlignmentOptions.MidlineLeft;
                    lbl.enableAutoSizing = false;
                    lbl.fontSize = 30;
                    lbl.text = "<b>" + o.label + "</b>\n" + UIFactory.Colored("<b>" + MinorEventRules.ResourceName(r) + " −" + o.cost + "</b>", Theme.Bad) +
                               (o.enabled ? "" : "\n" + UIFactory.Colored("<size=20>No disponible: " + o.disabledReason + "</size>", Theme.Bad));
                    continue;
                }
                string text = "<size=30><b>" + o.label + "</b></size>";
                if (!string.IsNullOrEmpty(o.details)) text += "\n<i>" + o.details + "</i>";
                if (!string.IsNullOrEmpty(o.costs)) text += "\n" + o.costs;
                if (!o.enabled && !string.IsNullOrEmpty(o.disabledReason)) text += "\n" + UIFactory.Colored("<b>No disponible: " + o.disabledReason + "</b>", Theme.Bad);
                lbl.text = text;
                lbl.enableAutoSizing = true;
                lbl.fontSizeMin = 14; lbl.fontSizeMax = 22;
            }
        }

        void BuildEclipseAllocation(RectTransform content, EventPrompt p)
        {
            var powered = new bool[4];
            var c = G.Config;
            var grid = UIFactory.Rect(content, "Secciones");
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(270, 300);
            g.spacing = new Vector2(12, 12);
            UIFactory.Size(grid, 300);

            var status = Line(content, "", 28, Theme.Ink, FontStyles.Bold);
            var confirm = UIFactory.Button(content, "Confirmar", "CONFIRMAR", Theme.Accent, Color.white, 30, () => G.events.ConfirmEclipse(powered));
            UIFactory.Size(confirm, 80);
            var cards = new List<(Image bg, TextMeshProUGUI txt)>();
            string[] icons = { "O2", "C", "E", "!" };

            void Refresh()
            {
                int used = EclipseRules.Cost(powered, c);
                int have = G.State.batteries;
                string pips = "";
                for (int i = 0; i < c.maxBatteries; i++)
                    pips += i < used ? UIFactory.Colored("■ ", Theme.Warn) : (i < have ? UIFactory.Colored("■ ", Theme.Good) : UIFactory.Colored("■ ", Theme.Disabled));
                status.text = "Baterías: " + pips + "  usadas " + used + " de " + have;
                confirm.interactable = used <= have;
                for (int i = 0; i < 4; i++)
                {
                    var sec = (EclipseSection)i;
                    cards[i].bg.color = powered[i] ? new Color(0.85f, 0.95f, 0.86f) : new Color(1f, 0.90f, 0.88f);
                    cards[i].txt.text = "<size=26><b>" + EclipseRules.SectionName(sec) + "</b></size>\n" +
                                        "Usa " + c.eclipseSectionCost[i] + " batería" + (c.eclipseSectionCost[i] > 1 ? "s" : "") + "\n\n" +
                                        (powered[i] ? UIFactory.Colored("<b>ENCENDIDO</b>", Theme.Good) + "\n" + EclipseRules.Consequence(sec, true, c)
                                                    : UIFactory.Colored("<b>APAGADO</b>", Theme.Bad) + "\n" + EclipseRules.Consequence(sec, false, c));
                    if (UIFactory.ArtPanel != null) cards[i].bg.color = UIFactory.ArtTint(powered[i] ? Theme.Good : Theme.OffGray);
                }
                // La base muestra en vivo qué edificios quedarían encendidos (variantes "encendidos" del eclipse).
                baseView.PreviewEclipse((bool[])powered.Clone());
                baseView.SetState(G.State, G.turns.Projection());
            }

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var b = UIFactory.Button(grid, "Seccion" + i, "", Color.white, Theme.Ink, 20, null);
                var lbl = b.Label();
                lbl.rectTransform.Stretch(16, 12, 16, 12);
                lbl.alignment = TextAlignmentOptions.TopLeft;
                lbl.fontStyle = FontStyles.Normal;
                lbl.enableAutoSizing = true; lbl.fontSizeMin = 14; lbl.fontSizeMax = 21;
                cards.Add((b.GetComponent<Image>(), lbl));
                b.onClick.AddListener(() =>
                {
                    powered[idx] = !powered[idx];
                    if (EclipseRules.Cost(powered, c) > G.State.batteries)
                    {
                        powered[idx] = false;
                        Toast("No alcanzan las baterías. Apaga otra sección primero.", NoticeKind.Warning);
                    }
                    Refresh();
                });
            }
            Refresh();
        }

        // ------------------------------------------------------------------ Tutorial

        RectTransform tutorialRoot, tutorialFrame;
        TextMeshProUGUI tutorialText;
        Button tutorialNext;
        int tutorialStep;

        void StartTutorial()
        {
            CloseTutorial();
            tutorialStep = 0;
            tutorialRoot = UIFactory.Rect(topLayer, "Tutorial").Stretch();
            tutorialFrame = UIFactory.Image(tutorialRoot, "Resaltado", Theme.Warn, SpriteFactory.RoundedOutline).rectTransform;
            tutorialFrame.anchorMin = tutorialFrame.anchorMax = new Vector2(0.5f, 0.5f);

            var box = UIFactory.Panel(tutorialRoot, "Dialogo", Theme.Panel, true);
            box.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 140), new Vector2(1100, 190), new Vector2(0.5f, 0));
            var portrait = UIFactory.Image(box.transform, "Director", new Color(0.85f, 0.90f, 1f), SpriteFactory.Circle);
            portrait.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(140, 140), new Vector2(0, 0.5f));
            var helmet = UIFactory.Image(portrait.transform, "Casco", new Color(0.30f, 0.45f, 0.75f), SpriteFactory.Circle);
            helmet.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 8), new Vector2(90, 70));
            var who = UIFactory.Text(portrait.transform, "Nombre", "DIRECTOR", 16, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            who.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(140, 24));
            tutorialText = UIFactory.Text(box.transform, "Texto", "", 27, Theme.Ink);
            tutorialText.rectTransform.Stretch(186, 70, 30, 16);
            tutorialNext = UIFactory.Button(box.transform, "Siguiente", "SIGUIENTE", Theme.Accent, Color.white, 24, NextTutorial);
            ((RectTransform)tutorialNext.transform).Place(new Vector2(1, 0), new Vector2(-20, 12), new Vector2(230, 56), new Vector2(1, 0));
            var skip = UIFactory.Button(box.transform, "Omitir", "OMITIR TUTORIAL", Theme.PanelSoft, Theme.InkSoft, 20, () => { G.saves.ShowTutorial = false; CloseTutorial(); });
            ((RectTransform)skip.transform).Place(new Vector2(1, 0), new Vector2(-266, 12), new Vector2(240, 56), new Vector2(1, 0));
            ShowTutorialStep();
        }

        void NextTutorial()
        {
            tutorialStep++;
            if (tutorialStep >= DefaultContent.TutorialSteps.Length) { CloseTutorial(); return; }
            ShowTutorialStep();
        }

        void ShowTutorialStep()
        {
            tutorialText.text = DefaultContent.TutorialSteps[tutorialStep];
            tutorialNext.Label().text = tutorialStep == DefaultContent.TutorialSteps.Length - 1 ? "¡A TRABAJAR!" : "SIGUIENTE";
        }

        RectTransform TutorialTarget()
        {
            switch (tutorialStep)
            {
                case 1: return (RectTransform)indicators[0].transform.parent;
                case 2: return baseView.buildings[(int)BuildingId.Solar].Rect;
                case 3: return baseView.buildings[(int)BuildingId.Recycler].Rect;
                case 4: return (RectTransform)endDayButton.transform;
                default: return null;
            }
        }

        void CloseTutorial()
        {
            if (tutorialRoot != null) Destroy(tutorialRoot.gameObject);
            tutorialRoot = null;
        }

        void Update()
        {
            if (tutorialRoot == null) return;
            var target = TutorialTarget();
            tutorialFrame.gameObject.SetActive(target != null);
            if (target == null) return;
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector2 min = tutorialRoot.InverseTransformPoint(corners[0]);
            Vector2 max = tutorialRoot.InverseTransformPoint(corners[2]);
            float pulse = 10f + 6f * Mathf.Sin(Time.unscaledTime * 5f);
            tutorialFrame.anchoredPosition = (min + max) / 2f;
            tutorialFrame.sizeDelta = (max - min) + Vector2.one * pulse * 2f;
        }
    }
}
