using System.Collections;
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
        static readonly Color Brown = new Color(0.25f, 0.15f, 0.02f);
        const float GridGap = 16f;

        // ------------------------------------------------------------------ Estructura común de ventana

        /// <summary>
        /// Ventana centrada con estructura vertical fija:
        ///   ENCABEZADO (icono + título + subtítulo, alto fijo) → CUERPO (flexible) → la sección de decisiones la agrega quien llama.
        /// Márgenes: ~6% a los lados; arriba se descuenta el borde transparente del sprite original.
        /// </summary>
        RectTransform Window(Transform layer, string title, string subtitle, Vector2 size, CraterEventType? illustration, Sprite image, out RectTransform body, out float contentWidth)
        {
            var overlay = UIFactory.Image(layer, "Ventana_" + title, new Color(0.02f, 0.04f, 0.12f, UIFactory.ArtPanel != null ? 0.4f : 0.65f), null, true).rectTransform;
            overlay.Stretch();
            var card = UIFactory.Panel(overlay, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, size);

            float padX = Mathf.Round(size.x * 0.06f);
            contentWidth = size.x - 2 * padX;
            var root = UIFactory.Rect(card.transform, "Contenido").Stretch(padX, 34, padX, 28 + UIFactory.TopInset);
            var v = UIFactory.VStack(root.gameObject, 14);
            v.childForceExpandHeight = false;

            // ENCABEZADO
            var header = UIFactory.Rect(root, "Encabezado");
            UIFactory.Size(header, 112, -1, -1, 112);
            var h = UIFactory.HStack(header.gameObject, 22);
            h.childForceExpandWidth = false;
            h.childAlignment = TextAnchor.MiddleLeft;
            if (HasIllustration(illustration, image))
            {
                var icon = UIFactory.Rect(header, "Ilustracion");
                UIFactory.Size(icon, 104, 104, 0);
                DrawArtIllustration(icon, illustration, image);
            }
            var titles = UIFactory.Rect(header, "Titulos");
            UIFactory.Size(titles, -1, -1, 1);
            var tv = UIFactory.VStack(titles.gameObject, 2);
            tv.childForceExpandHeight = false;
            tv.childAlignment = TextAnchor.MiddleLeft;
            var t = Fixed(titles, title, 50, 32, 58, Brown, FontStyles.Bold);
            t.textWrappingMode = TextWrappingModes.NoWrap;
            if (!string.IsNullOrEmpty(subtitle)) Fixed(titles, subtitle, 28, 20, 50, Theme.Ink, FontStyles.Bold);

            body = UIFactory.Rect(root, "Cuerpo");
            UIFactory.Size(body, -1, -1, -1).flexibleHeight = 1;
            var bv = UIFactory.VStack(body.gameObject, 12);
            bv.childForceExpandHeight = false;
            return overlay;
        }

        bool HasIllustration(CraterEventType? type, Sprite image)
            => image != null || (type == CraterEventType.EarthShadow && art.eclipseSun != null && art.eclipseEarth != null) || UIFactory.ArtPanel == null;

        /// <summary>Texto de alto fijo con tamaño automático acotado (no se sale ni se superpone).</summary>
        static TextMeshProUGUI Fixed(Transform parent, string text, float size, float min, float height, Color color, FontStyles style = FontStyles.Normal)
        {
            var t = UIFactory.Text(parent, "Texto", text, size, color, TextAlignmentOptions.MidlineLeft, style);
            t.enableAutoSizing = true;
            t.fontSizeMin = min; t.fontSizeMax = size;
            t.overflowMode = TextOverflowModes.Ellipsis;
            UIFactory.Size(t, height, -1, -1, height);
            return t;
        }

        /// <summary>Párrafo con alto según su contenido (sin tamaño automático).</summary>
        static TextMeshProUGUI Line(Transform parent, string text, float size = 24, Color? color = null, FontStyles style = FontStyles.Normal)
        {
            var t = UIFactory.Text(parent, "Linea", text, size, color ?? Theme.Ink, TextAlignmentOptions.Left, style);
            t.gameObject.AddComponent<LayoutElement>();
            return t;
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
            if (UIFactory.ArtPanel == null) DrawIllustration(box, type);
            return false;
        }

        /// <summary>Dibujo provisional (solo se usa si no hay arte original asignado).</summary>
        static void DrawIllustration(RectTransform art, CraterEventType? type)
        {
            var bg = UIFactory.Image(art, "Fondo", new Color(0.05f, 0.08f, 0.18f, 0.9f), SpriteFactory.Circle);
            bg.rectTransform.Stretch();
            var ex = UIFactory.Text(art, "!", type == null ? "!" : "", 52, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            ex.rectTransform.Stretch();
        }

        static Color ResourceColor(ResourceKind r) =>
            r == ResourceKind.Energy ? Theme.Energy : r == ResourceKind.Oxygen ? Theme.Oxygen : r == ResourceKind.Food ? Theme.Food : Theme.Health;

        static string ResourceLetter(ResourceKind r) =>
            r == ResourceKind.Energy ? "E" : r == ResourceKind.Oxygen ? "O2" : r == ResourceKind.Food ? "C" : "S";

        /// <summary>Espacio flexible que empuja la sección de decisiones al fondo de la ventana.</summary>
        static void Spacer(Transform parent)
        {
            var s = UIFactory.Rect(parent, "Espacio");
            UIFactory.Size(s, 0).flexibleHeight = 1;
        }

        // ------------------------------------------------------------------ Eventos

        // Presentación de un acontecimiento principal cuando OCURRE (EventPrompt.arrival):
        //   etapa 0: espera la transición del día y la pausa (no suena nada todavía)
        //   etapa 1: notificación inicial + empieza la música del evento
        //   etapa 2: aparece el fenómeno en la base + su efecto de sonido
        //   después: la ventana de decisiones. Cada ventana se presenta una sola vez (lastIntro).
        Coroutine introRoutine;
        EventPrompt introPrompt, lastIntro;
        int introStage = -1;

        /// <summary>Mientras se presenta un acontecimiento, el fenómeno visual espera a la etapa 2.</summary>
        bool HoldEventVisuals => introPrompt != null && introStage < 2;

        void ShowPrompt(EventPrompt p)
        {
            if (introRoutine != null) StopCoroutine(introRoutine);
            introRoutine = null;
            introPrompt = null;
            introStage = -1;
            if (eventOverlay != null) Destroy(eventOverlay.gameObject);
            eventOverlay = null;
            CloseBuildingCard(true);
            if (p != null && p.arrival && p.eventType.HasValue && p != lastIntro)
            {
                lastIntro = p;
                introPrompt = p;
                introStage = 0;
                RefreshGame();
                introRoutine = StartCoroutine(EventIntro(p));
                return;
            }
            RefreshGame();
            if (p == null) return;
            BuildPromptWindow(p);
        }

        IEnumerator EventIntro(EventPrompt p)
        {
            yield return null;                                     // deja empezar la transición del día
            while (G.turns.Busy || G.Paused) yield return null;
            if (G.events.Current != p) yield break;

            introStage = 1;
            Toast(p.title + ": " + p.subtitle, NoticeKind.Warning);
            UpdateMusic();                                         // la música del evento empieza con la notificación

            for (float t = 0; t < 1.6f; ) { if (!G.Paused) t += Time.unscaledDeltaTime; yield return null; }   // no avanza en pausa
            if (G.events.Current != p) yield break;

            introStage = 2;
            var type = p.eventType.Value;
            if (type != CraterEventType.EarthShadow) fx.Play(type, p.intensity);   // el eclipse se oscurece con RefreshGame
            RefreshGame();
            G.sound.PlayEventSfx(EventSfx(type));                  // el efecto acompaña al fenómeno visual

            for (float t = 0; t < 1.4f; ) { if (!G.Paused) t += Time.unscaledDeltaTime; yield return null; }   // no avanza en pausa
            if (G.events.Current != p) yield break;

            introRoutine = null;
            introPrompt = null;
            introStage = -1;
            RefreshGame();
            BuildPromptWindow(p);
        }

        AudioClip EventSfx(CraterEventType type) =>
            type == CraterEventType.EarthShadow ? art.sfxEclipse : type == CraterEventType.Micrometeorites ? art.sfxMeteors : art.sfxRadiation;

        void BuildPromptWindow(EventPrompt p)
        {
            bool eclipse = p.kind == PromptKind.EclipseAllocation;
            bool info = p.kind == PromptKind.Info;
            bool twoCols = p.options.Count >= 4;
            int rows = info ? 1 : Mathf.CeilToInt(p.options.Count / (twoCols ? 2f : 1f));
            float cellH = info ? 90 : twoCols ? 196 : 128;
            float optionsH = rows * cellH + (rows - 1) * GridGap;
            if (eclipse) optionsH = 206 * 2 + GridGap + 12 + 150;   // tarjetas 2 × 2 + pie con baterías y CONFIRMAR
            eventOverlay = Window(modalLayer, p.title, p.subtitle, new Vector2(1200, 600), p.eventType, p.image, out var body, out float width);

            // El alto de la ventana se calcula con el texto real: márgenes + encabezado + texto + decisiones.
            // Nunca supera 1000 px; si el texto no cabe, se reduce dentro de límites legibles.
            const float chrome = 85 + 112 + 14 + 36 + 20;
            float textBudget = 1000 - chrome - optionsH;
            float textH = 0;
            if (!string.IsNullOrEmpty(p.body))
            {
                var t = Line(body, p.body, 26, Theme.Ink);
                float need = t.GetPreferredValues(p.body, width, 0).y;
                if (need > textBudget - 30)
                {
                    t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = 26;
                    t.overflowMode = TextOverflowModes.Ellipsis;
                    need = textBudget - 30;
                    UIFactory.Size(t, need, -1, -1, need);
                }
                textH += need;
            }
            if (!string.IsNullOrEmpty(p.dataLabel)) { Line(body, p.dataLabel, 19, Theme.InkSoft, FontStyles.Italic); textH += 30; }
            Spacer(body);
            float height = Mathf.Min(1000, chrome + textH + optionsH);
            ((RectTransform)eventOverlay.GetChild(0)).sizeDelta = new Vector2(1200, height);

            if (eclipse) BuildEclipseAllocation(body, width);
            else BuildPromptOptions(body, p, width, twoCols, cellH, optionsH);
        }

        void BuildPromptOptions(RectTransform body, EventPrompt p, float width, bool twoCols, float cellH, float optionsH)
        {
            bool info = p.kind == PromptKind.Info;
            var grid = UIFactory.Rect(body, "Decisiones");
            UIFactory.Size(grid, optionsH, -1, -1, optionsH);
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            int cols = info ? 1 : twoCols ? 2 : 1;
            float cellW = info ? Mathf.Min(520, width) : (width - (cols - 1) * GridGap) / cols;
            g.cellSize = new Vector2(cellW, cellH);
            g.spacing = new Vector2(GridGap, GridGap);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = cols;
            g.childAlignment = TextAnchor.UpperCenter;

            foreach (var o in p.options)
            {
                var opt = o;
                var b = UIFactory.Button(grid, o.label, o.label, o.enabled ? (info ? Theme.Accent : Color.white) : Theme.Disabled, Brown, 32,
                    () => { if (!G.Paused) G.events.Choose(opt); });
                b.interactable = o.enabled;
                if (info) continue;
                var lbl = b.Label();
                lbl.alignment = TextAlignmentOptions.MidlineLeft;
                lbl.fontStyle = FontStyles.Normal;
                if (o.resource.HasValue)
                {
                    // Evento menor: icono del recurso + nombre + cantidad que se pierde.
                    var r = o.resource.Value;
                    var icon = UIFactory.Image(b.transform, "Icono", ResourceColor(r), SpriteFactory.Circle);
                    icon.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(24, -UIFactory.TopInset / 2f), new Vector2(76, 76), new Vector2(0, 0.5f));
                    var letter = UIFactory.Text(icon.transform, "Letra", ResourceLetter(r), 28, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
                    letter.rectTransform.Stretch();
                    lbl.Fit(120, 10, 28, 8, 20, 30);
                    lbl.text = "<b>" + o.label + "</b>\n" + UIFactory.Colored("<b>" + MinorEventRules.ResourceName(r) + " −" + o.cost + "</b>", Theme.Bad) +
                               (o.enabled ? "" : "\n" + UIFactory.Colored("<size=80%>No disponible: " + o.disabledReason + "</size>", Theme.Bad));
                    continue;
                }
                lbl.Fit(28, 12, 28, 10, 16, 24);
                lbl.alignment = TextAlignmentOptions.TopLeft;
                string text = "<size=125%><b>" + o.label + "</b></size>";
                if (!string.IsNullOrEmpty(o.details)) text += "\n<i>" + o.details + "</i>";
                if (!string.IsNullOrEmpty(o.costs)) text += "\n" + o.costs;
                if (!o.enabled && !string.IsNullOrEmpty(o.disabledReason)) text += "\n" + UIFactory.Colored("<b>No disponible: " + o.disabledReason + "</b>", Theme.Bad);
                lbl.text = text;
            }
        }

        /// <summary>
        /// Sombra de la Tierra: 4 tarjetas en cuadrícula 2 × 2 de igual tamaño, indicador de baterías y CONFIRMAR,
        /// todo dentro del panel. CONFIRMAR solo se habilita con una distribución válida y se ejecuta una sola vez.
        /// </summary>
        void BuildEclipseAllocation(RectTransform body, float width)
        {
            var powered = new bool[4];
            var c = G.Config;
            const float cellH = 206;
            float cellW = (width - GridGap) / 2f;

            var grid = UIFactory.Rect(body, "Secciones");
            float gridH = cellH * 2 + GridGap;
            UIFactory.Size(grid, gridH, -1, -1, gridH);
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(cellW, cellH);
            g.spacing = new Vector2(GridGap, GridGap);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 2;

            var footer = UIFactory.Rect(body, "Pie");
            UIFactory.Size(footer, 150, -1, -1, 150);
            var fv = UIFactory.VStack(footer.gameObject, 12);
            fv.childForceExpandHeight = false;
            fv.childAlignment = TextAnchor.UpperCenter;
            var status = Fixed(footer, "", 28, 20, 44, Theme.Ink, FontStyles.Bold);
            status.alignment = TextAlignmentOptions.Center;
            var row = UIFactory.Rect(footer, "Fila");
            UIFactory.Size(row, 90, -1, -1, 90);
            var rh = UIFactory.HStack(row.gameObject, 0);
            rh.childForceExpandWidth = false;
            rh.childAlignment = TextAnchor.MiddleCenter;
            bool confirmed = false;
            Button confirm = null;
            confirm = UIFactory.Button(row, "Confirmar", "CONFIRMAR", Theme.Accent, Brown, 34, () =>
            {
                if (confirmed || G.Paused || !EclipseRules.Affordable(powered, G.State.batteries, c)) return;
                confirmed = true;                  // evita el doble clic
                confirm.interactable = false;
                baseView.PreviewEclipse(null);
                G.events.ConfirmEclipse((bool[])powered.Clone());
            });
            UIFactory.Size(confirm, 90, 440, 0);

            var cards = new List<(Image bg, TextMeshProUGUI txt)>();
            void Refresh()
            {
                int used = EclipseRules.Cost(powered, c);
                int have = G.State.batteries;
                string pips = "";
                for (int i = 0; i < c.maxBatteries; i++)
                    pips += i < used ? UIFactory.Colored("■", Theme.Warn) : (i < have ? UIFactory.Colored("■", Theme.Good) : UIFactory.Colored("■", Theme.Disabled));
                status.text = "Baterías disponibles: " + have + "   " + pips + "   Usadas: " + used + " de " + have;
                confirm.interactable = !confirmed && used <= have;
                for (int i = 0; i < 4; i++)
                {
                    var sec = (EclipseSection)i;
                    int cost = c.eclipseSectionCost[i];
                    cards[i].bg.color = UIFactory.ArtPanel != null ? UIFactory.ArtTint(powered[i] ? Theme.Good : Theme.OffGray)
                                                                    : (powered[i] ? new Color(0.85f, 0.95f, 0.86f) : new Color(1f, 0.90f, 0.88f));
                    cards[i].txt.text = "<size=120%><b>" + EclipseRules.SectionName(sec) + "</b></size>\n" +
                                        "Costo: " + cost + " batería" + (cost > 1 ? "s" : "") + "\n" +
                                        (powered[i] ? UIFactory.Colored("<b>CON ENERGÍA</b>", Theme.Good) : UIFactory.Colored("<b>SIN ENERGÍA</b>", Theme.Bad)) + "\n" +
                                        EclipseRules.Consequence(sec, powered[i], c);
                }
                // La base muestra en vivo qué edificios quedarían encendidos (variantes "encendidos" del eclipse).
                baseView.PreviewEclipse((bool[])powered.Clone());
                baseView.SetState(G.State, G.turns.Projection());
            }

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var b = UIFactory.Button(grid, "Seccion" + i, "", Color.white, Brown, 22, null);
                var lbl = b.Label();
                lbl.Fit(26, 14, 26, 10, 16, 22);
                lbl.alignment = TextAlignmentOptions.TopLeft;
                lbl.fontStyle = FontStyles.Normal;
                cards.Add((b.GetComponent<Image>(), lbl));
                b.onClick.AddListener(() =>
                {
                    if (confirmed || G.Paused) return;
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
            box.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(-150, 40), new Vector2(1180, 230), new Vector2(0.5f, 0));
            var portrait = UIFactory.Image(box.transform, "Director", new Color(0.85f, 0.90f, 1f), SpriteFactory.Circle);
            portrait.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(34, -UIFactory.TopInset / 2f), new Vector2(140, 140), new Vector2(0, 0.5f));
            var helmet = UIFactory.Image(portrait.transform, "Casco", new Color(0.30f, 0.45f, 0.75f), SpriteFactory.Circle);
            helmet.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 8), new Vector2(90, 70));
            var who = UIFactory.Text(portrait.transform, "Nombre", "DIRECTOR", 16, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            who.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(140, 24));
            tutorialText = UIFactory.Text(box.transform, "Texto", "", 27, Brown).Fit(200, 86, 40, 14, 20, 28);
            tutorialNext = UIFactory.Button(box.transform, "Siguiente", "SIGUIENTE", Theme.Accent, Brown, 24, NextTutorial);
            ((RectTransform)tutorialNext.transform).Place(new Vector2(1, 0), new Vector2(-30, 16), new Vector2(240, 70), new Vector2(1, 0));
            var skip = UIFactory.Button(box.transform, "Omitir", "OMITIR TUTORIAL", Theme.PanelSoft, Theme.InkSoft, 20, () => { G.saves.ShowTutorial = false; CloseTutorial(); });
            ((RectTransform)skip.transform).Place(new Vector2(1, 0), new Vector2(-286, 16), new Vector2(260, 70), new Vector2(1, 0));
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
