using System;
using System.Collections;
using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using Crater.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Crater.UI
{
    /// <summary>
    /// Interfaz de CRATER (Unity UI + TextMeshPro, construida por código).
    /// Separada de la simulación: solo lee el estado, pide la proyección y llama a los sistemas.
    /// Durante la partida muestra solo: 4 recursos, día, la base, TERMINAR DÍA y MENÚ.
    /// GameObject: "UIManager" (hijo de "CRATER").
    /// </summary>
    public partial class UIManager : MonoBehaviour
    {
        [Tooltip("Escala de la base (la referencia mide 1425 × 738).")]
        public float baseScale = 1.12f;

        [Header("Arte definitivo (opcional)")]
        [Tooltip("Terreno + núcleo + módulo de agua en un solo sprite con la distribución de la referencia. " +
                 "Si está vacío se dibujan placeholders. El arte de cada edificio va en BuildingData.icon.")]
        public Sprite baseArt;

        GameManager G => GameManager.Instance;

        RectTransform root;          // canvas
        RectTransform screensLayer;  // pantallas completas
        RectTransform modalLayer;    // ventanas de eventos
        RectTransform topLayer;      // tutorial, transición y avisos

        RectTransform menuScreen, setupScreen, gameScreen, scienceScreen, creditsScreen, optionsScreen, endScreen;

        // HUD
        readonly ResourceIndicator[] indicators = new ResourceIndicator[4];
        TextMeshProUGUI dayText, endDayHint;
        RectTransform toastsRoot;
        Button endDayButton;
        BaseView baseView;
        EffectsLayer fx;
        Image sun, earth;
        CanvasGroup transition;
        TextMeshProUGUI transitionText;

        RectTransform eventOverlay;
        bool built;

        // ------------------------------------------------------------------ Ciclo de vida

        void Start()
        {
            EnsureCameraAndEventSystem();
            Build();
            G.StateChanged += RefreshGame;
            G.Noticed += Toast;
            G.GameStarted += OnGameStarted;
            G.GameFinished += ShowEnd;
            G.DayEnded += OnDayEnded;
            G.events.PromptChanged += ShowPrompt;
            G.events.EffectRequested += (type, intensity) => fx.Play(type, intensity);
            G.nasa.Initialize(G.saves.UseLiveData);
            ShowMenu();
        }

        void OnDestroy()
        {
            if (G == null) return;
            G.StateChanged -= RefreshGame;
            G.Noticed -= Toast;
            G.GameStarted -= OnGameStarted;
            G.GameFinished -= ShowEnd;
            G.DayEnded -= OnDayEnded;
        }

        void EnsureCameraAndEventSystem()
        {
            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Theme.Sky;
            }
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        void Build()
        {
            if (built) return;
            built = true;
            var canvasGo = new GameObject("CRATER_Canvas", typeof(RectTransform));
            canvasGo.layer = 5;
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            root = (RectTransform)canvasGo.transform;

            BuildSky(root);
            screensLayer = UIFactory.Rect(root, "Pantallas").Stretch();
            modalLayer = UIFactory.Rect(root, "Ventanas").Stretch();
            topLayer = UIFactory.Rect(root, "Superior").Stretch();

            menuScreen = BuildMainMenu();
            setupScreen = BuildSetup();
            gameScreen = BuildGameScreen();
            optionsScreen = BuildOptions();
            creditsScreen = BuildCredits();
            scienceScreen = UIFactory.Rect(screensLayer, "Ciencia").Stretch();
            endScreen = UIFactory.Rect(screensLayer, "Final").Stretch();

            toastsRoot = UIFactory.Rect(topLayer, "Avisos").Place(new Vector2(0.5f, 1), new Vector2(0, -135), new Vector2(820, 300), new Vector2(0.5f, 1));
            var tv = UIFactory.VStack(toastsRoot.gameObject, 8);
            tv.childAlignment = TextAnchor.UpperCenter;

            var tr = UIFactory.Image(topLayer, "Transicion", new Color(0.05f, 0.08f, 0.18f, 0.85f));
            tr.rectTransform.Stretch();
            transition = tr.gameObject.AddComponent<CanvasGroup>();
            transition.alpha = 0; transition.blocksRaycasts = false;
            transitionText = UIFactory.Text(tr.transform, "Dia", "", 90, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            transitionText.rectTransform.Stretch();
        }

        void BuildSky(RectTransform parent)
        {
            var sky = UIFactory.Image(parent, "Cielo", new Color(0.02f, 0.04f, 0.12f));
            sky.rectTransform.Stretch();
            var rnd = new System.Random(3);
            for (int i = 0; i < 140; i++)
            {
                bool warm = rnd.NextDouble() < 0.3;
                var s = UIFactory.Image(parent, "Estrella", warm ? new Color(1f, 0.75f, 0.4f, 0.9f) : new Color(1, 1, 1, (float)(0.4 + rnd.NextDouble() * 0.6)), SpriteFactory.Circle);
                float size = (float)(2 + rnd.NextDouble() * 4);
                s.rectTransform.Place(new Vector2((float)rnd.NextDouble(), (float)(0.55 + rnd.NextDouble() * 0.45)), Vector2.zero, new Vector2(size, size));
            }
            sun = UIFactory.Image(parent, "Sol", new Color(1f, 0.92f, 0.55f), SpriteFactory.Circle);
            sun.rectTransform.Place(new Vector2(1, 1), new Vector2(-120, -175), new Vector2(60, 60));
            var glow = UIFactory.Image(sun.transform, "Brillo", new Color(1f, 0.85f, 0.4f, 0.45f), SpriteFactory.Glow);
            glow.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 180));
            earth = UIFactory.Image(parent, "Tierra", new Color(0.18f, 0.35f, 0.70f), SpriteFactory.Circle);
            earth.rectTransform.Place(new Vector2(1, 1), new Vector2(-40, -165), new Vector2(70, 70));
        }

        void ShowOnly(RectTransform screen)
        {
            foreach (var s in new[] { menuScreen, setupScreen, gameScreen, scienceScreen, creditsScreen, optionsScreen, endScreen })
                if (s != null) s.gameObject.SetActive(s == screen);
        }

        void CloseAllModals()
        {
            if (eventOverlay != null) Destroy(eventOverlay.gameObject);
            eventOverlay = null;
            CloseBuildingCard();
            CloseTutorial();
        }

        // ------------------------------------------------------------------ Pantalla de juego

        RectTransform BuildGameScreen()
        {
            var scr = UIFactory.Rect(screensLayer, "Juego").Stretch();

            // Base en el centro, con las proporciones de la referencia
            var holder = UIFactory.Rect(scr, "ContenedorBase").Place(new Vector2(0.5f, 0.5f), new Vector2(0, -2), new Vector2(BaseView.RefWidth, BaseView.RefHeight));
            holder.localScale = Vector3.one * baseScale;
            baseView = BaseView.Create(holder, G.buildingData, baseArt);
            ((RectTransform)baseView.transform).Stretch();
            foreach (var b in baseView.buildings) b.Selected += OpenBuildingCard;
            fx = EffectsLayer.Create(scr);

            // Barra superior: solo los 4 recursos y el día
            var bar = UIFactory.Image(scr, "BarraSuperior", new Color(0.05f, 0.08f, 0.18f, 0.8f));
            bar.rectTransform.anchorMin = new Vector2(0, 1); bar.rectTransform.anchorMax = new Vector2(1, 1);
            bar.rectTransform.pivot = new Vector2(0.5f, 1);
            bar.rectTransform.offsetMin = new Vector2(0, -124); bar.rectTransform.offsetMax = Vector2.zero;
            string[] names = { "Energía", "Oxígeno", "Comida", "Salud" };
            string[] letters = { "E", "O2", "C", "S" };
            Color[] colors = { Theme.Energy, Theme.Oxygen, Theme.Food, Theme.Health };
            for (int i = 0; i < 4; i++)
            {
                indicators[i] = ResourceIndicator.Create(bar.transform, names[i], letters[i], colors[i]);
                ((RectTransform)indicators[i].transform).Place(new Vector2(0, 0.5f), new Vector2(24 + i * 362, 0), new Vector2(348, 104), new Vector2(0, 0.5f));
            }
            var dayCard = UIFactory.Panel(bar.transform, "Dia", Theme.Panel, false);
            dayCard.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(330, 104), new Vector2(1, 0.5f));
            dayText = UIFactory.Text(dayCard.transform, "Texto", "DÍA 01 / 24", 44, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            dayText.rectTransform.Stretch();

            // Botón principal
            endDayButton = UIFactory.Button(scr, "TerminarDia", "TERMINAR DÍA", Theme.Accent, Color.white, 40, () => { CloseBuildingCard(); G.turns.EndDay(); });
            ((RectTransform)endDayButton.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(460, 100), new Vector2(0.5f, 0));
            endDayHint = UIFactory.Text(scr, "Pista", "", 22, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            endDayHint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 126), new Vector2(900, 30), new Vector2(0.5f, 0));

            var menu = UIFactory.Button(scr, "Menu", "MENÚ", new Color(0.96f, 0.97f, 0.99f, 0.9f), Theme.Ink, 24, () => { G.QuitToMenu(); ShowMenu(); });
            ((RectTransform)menu.transform).Place(new Vector2(0, 0), new Vector2(24, 22), new Vector2(170, 70), Vector2.zero);

            BuildBuildingCard(scr);
            return scr;
        }

        void OnGameStarted()
        {
            CloseAllModals();
            ShowOnly(gameScreen);
            fx.SetEclipse(false);
            RefreshGame();
        }

        /// <summary>Lo llama TurnManager al empezar cada día.</summary>
        public void OnDayBegan(bool showTutorial)
        {
            if (showTutorial) StartTutorial();
        }

        void OnDayEnded(TurnReport r) => StartCoroutine(DayTransition(r));

        IEnumerator DayTransition(TurnReport r)
        {
            G.turns.Busy = true;
            RefreshGame();
            transitionText.text = r.after.outcome == Outcome.Playing ? "DÍA " + r.after.day.ToString("00") : "";
            float t = 0;
            while (t < 0.9f)
            {
                t += Time.unscaledDeltaTime;
                transition.alpha = t < 0.3f ? t / 0.3f : Mathf.Clamp01(1f - (t - 0.5f) / 0.4f);
                yield return null;
            }
            transition.alpha = 0;
            G.turns.Busy = false;
            RefreshGame();
        }

        void RefreshGame()
        {
            if (!built || G == null || G.State == null || !gameScreen.gameObject.activeSelf) return;
            var s = G.State;
            var c = G.Config;
            var p = G.turns.Projection();

            indicators[0].Show(s.energy, c.resourceMax, p.energy, c.alertThreshold);
            indicators[1].Show(s.oxygen, c.resourceMax, p.oxygen, c.alertThreshold);
            indicators[2].Show(s.food, c.resourceMax, p.food, c.alertThreshold);
            indicators[3].Show(s.health, c.resourceMax, p.health, c.alertThreshold);
            dayText.text = "DÍA " + Mathf.Min(s.day, s.totalTurns).ToString("00") + " / " + s.totalTurns.ToString("00");

            for (int i = 0; i < 4; i++) baseView.buildings[i].Refresh(p.buildings[i]);
            baseView.SetState(s, c);
            fx.SetEclipse(s.today.eclipse);
            sun.color = s.today.eclipse ? new Color(0.35f, 0.33f, 0.25f) : new Color(1f, 0.92f, 0.55f);
            earth.rectTransform.anchoredPosition = s.today.eclipse ? new Vector2(-120, -175) : new Vector2(-40, -165);

            bool can = G.turns.CanEndDay(out string why);
            endDayButton.interactable = can;
            endDayHint.text = can ? "" : why;
            RefreshBuildingCard();
        }

        // ------------------------------------------------------------------ Ficha contextual del edificio

        RectTransform card;
        TextMeshProUGUI cardName, cardStatus, cardInfo, cardNumbers;
        Button cardToggle;
        BuildingId? cardId;

        void BuildBuildingCard(RectTransform parent)
        {
            var img = UIFactory.Panel(parent, "FichaEdificio", Theme.Panel, true);
            card = img.rectTransform;
            card.sizeDelta = new Vector2(430, 300);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            cardName = UIFactory.Text(card, "Nombre", "", 30, Theme.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
            cardName.rectTransform.Place(new Vector2(0, 1), new Vector2(20, -12), new Vector2(330, 40), new Vector2(0, 1));
            var close = UIFactory.Button(card, "Cerrar", "X", Theme.PanelSoft, Theme.Ink, 24, CloseBuildingCard);
            ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(-12, -12), new Vector2(50, 50), new Vector2(1, 1));
            cardStatus = UIFactory.Text(card, "Estado", "", 22, Theme.Good, TextAlignmentOptions.Left, FontStyles.Bold);
            cardStatus.rectTransform.Place(new Vector2(0, 1), new Vector2(20, -54), new Vector2(390, 30), new Vector2(0, 1));
            cardInfo = UIFactory.Text(card, "Funcion", "", 19, Theme.Ink);
            cardInfo.rectTransform.Place(new Vector2(0, 1), new Vector2(20, -88), new Vector2(390, 76), new Vector2(0, 1));
            cardNumbers = UIFactory.Text(card, "Numeros", "", 20, Theme.InkSoft, TextAlignmentOptions.Left, FontStyles.Bold);
            cardNumbers.rectTransform.Place(new Vector2(0, 1), new Vector2(20, -168), new Vector2(390, 30), new Vector2(0, 1));
            cardToggle = UIFactory.Button(card, "Encender", "", Theme.Good, Color.white, 26, () => { if (cardId.HasValue) G.buildings.Toggle(cardId.Value); });
            ((RectTransform)cardToggle.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(390, 70), new Vector2(0.5f, 0));
            card.gameObject.SetActive(false);
        }

        void OpenBuildingCard(BuildingId id)
        {
            if (cardId == id && card.gameObject.activeSelf) { CloseBuildingCard(); return; }
            cardId = id;
            for (int i = 0; i < 4; i++) baseView.buildings[i].SetSelected(i == (int)id);

            // Junto al edificio: a la derecha si está a la izquierda de la pantalla y viceversa.
            var b = baseView.buildings[(int)id].Rect;
            var corners = new Vector3[4];
            b.GetWorldCorners(corners);
            var parent = (RectTransform)card.parent;
            Vector2 min = parent.InverseTransformPoint(corners[0]), max = parent.InverseTransformPoint(corners[2]);
            bool left = (min.x + max.x) / 2f < 0;
            float x = left ? max.x + 20 + card.sizeDelta.x / 2f : min.x - 20 - card.sizeDelta.x / 2f;
            float y = Mathf.Clamp((min.y + max.y) / 2f, -parent.rect.height / 2f + 190, parent.rect.height / 2f - 290);
            x = Mathf.Clamp(x, -parent.rect.width / 2f + 230, parent.rect.width / 2f - 230);
            card.anchoredPosition = new Vector2(x, y);
            card.gameObject.SetActive(true);
            card.SetAsLastSibling();
            RefreshBuildingCard();
        }

        void CloseBuildingCard()
        {
            cardId = null;
            if (card != null) card.gameObject.SetActive(false);
            if (baseView != null) foreach (var b in baseView.buildings) b.SetSelected(false);
        }

        void RefreshBuildingCard()
        {
            if (!cardId.HasValue || !card.gameObject.activeSelf || G.State == null) return;
            var id = cardId.Value;
            var data = G.GetBuilding(id);
            var st = G.Config.Get(id);
            var r = G.buildings.Result(id);
            cardName.text = st.name;
            cardStatus.text = BuildingManager.StatusLabel(r.visual) + (string.IsNullOrEmpty(r.reason) ? "" : ": " + r.reason);
            cardStatus.color = r.visual == BuildingVisual.Working ? Theme.Good : r.visual == BuildingVisual.Off ? Theme.OffGray
                             : r.visual == BuildingVisual.NoPower || r.visual == BuildingVisual.NoWater ? Theme.Bad : Theme.Warn;
            cardInfo.text = data.description + " " + data.howItWorks;

            var parts = new List<string>();
            if (st.energy > 0) parts.Add("Energía +" + st.energy);
            else if (st.energy < 0) parts.Add("Energía " + Num.Signed(st.energy));
            if (st.oxygen != 0) parts.Add("Oxígeno " + Num.Signed(st.oxygen));
            if (st.food != 0) parts.Add("Comida " + Num.Signed(st.food));
            cardNumbers.text = string.Join("   ", parts) + " por día";

            bool on = G.State.IsOn(id);
            bool can = G.buildings.CanToggle(out _);
            cardToggle.interactable = can;
            cardToggle.GetComponent<Image>().color = on ? Theme.OffGray : Theme.Good;
            cardToggle.Label().text = on ? "APAGAR" : "ENCENDER";
        }

        // ------------------------------------------------------------------ Avisos temporales

        void Toast(string text, NoticeKind kind)
        {
            if (!built) return;
            while (toastsRoot.childCount >= 3)
            {
                var old = toastsRoot.GetChild(0);
                old.SetParent(null);
                Destroy(old.gameObject);
            }
            var p = UIFactory.Panel(toastsRoot, "Aviso", Color.white, false);
            var stripe = UIFactory.Image(p.transform, "Color", Theme.Kind(kind), SpriteFactory.Rounded);
            stripe.rectTransform.anchorMin = Vector2.zero; stripe.rectTransform.anchorMax = new Vector2(0, 1);
            stripe.rectTransform.offsetMax = new Vector2(14, 0);
            var t = UIFactory.Text(p.transform, "Texto", text, 22, Theme.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
            t.rectTransform.Stretch(28, 8, 16, 8);
            UIFactory.Size(p, Mathf.Max(60, t.GetPreferredValues(text, 760, 0).y + 20));
            var cg = p.gameObject.AddComponent<CanvasGroup>();
            StartCoroutine(FadeToast(cg, kind == NoticeKind.Danger ? 6f : 4f));
        }

        IEnumerator FadeToast(CanvasGroup cg, float life)
        {
            float t = 0;
            while (t < life && cg != null)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = t > life - 0.6f ? (life - t) / 0.6f : 1f;
                yield return null;
            }
            if (cg != null) Destroy(cg.gameObject);
        }
    }
}
