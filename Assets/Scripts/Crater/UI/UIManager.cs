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
    /// Interfaz de CRATER con el arte original del equipo (Unity UI + TextMeshPro).
    /// Separada de la simulación: solo lee el estado, pide la proyección y llama a los sistemas.
    ///
    /// Pantalla de juego (de atrás hacia adelante):
    ///   Vista (máscara) → Camara (zoom) → Mundo (fondo + edificios + rover) → Efectos de eventos → HUD → Ficha del edificio
    /// La "cámara" es el RectTransform Camara: al seleccionar un edificio se acerca (escala ×2 y desplazamiento)
    /// y recién al terminar aparece el panel; al cerrarlo vuelve a la vista general.
    /// GameObject: "UIManager" (hijo de "CRATER"). Inspector: asigna "art" (Assets/Data/Crater/ArtSet.asset).
    /// </summary>
    public partial class UIManager : MonoBehaviour
    {
        [Tooltip("Sprites originales (Assets/Data/Crater/ArtSet.asset).")]
        public CraterArtSet art;

        GameManager G => GameManager.Instance;

        RectTransform root;          // canvas
        RectTransform screensLayer;  // pantallas completas
        RectTransform modalLayer;    // ventanas de eventos
        RectTransform topLayer;      // tutorial, transición y avisos

        RectTransform menuScreen, setupScreen, gameScreen, scienceScreen, creditsScreen, optionsScreen, endScreen;

        // HUD
        readonly ResourceIndicator[] indicators = new ResourceIndicator[4]; // Comida, Energía, Oxígeno, Salud (orden de la referencia)
        TextMeshProUGUI dayText, endDayHint;
        RectTransform toastsRoot;
        Button endDayButton;
        BaseView baseView;
        EffectsLayer fx;
        RectTransform cameraRig;
        CanvasGroup transition;
        TextMeshProUGUI transitionText;

        RectTransform eventOverlay;
        bool built;

        // ------------------------------------------------------------------ Ciclo de vida

        void Start()
        {
            EnsureCameraAndEventSystem();
            if (art == null)
            {
                Debug.LogError("[CRATER] UIManager no tiene asignado el arte original (Assets/Data/Crater/ArtSet.asset). Usa CRATER > Asignar arte original.");
                art = ScriptableObject.CreateInstance<CraterArtSet>();
            }
            UIFactory.ArtPanel = art.panel;
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
            scaler.referenceResolution = new Vector2(1920, 1080);  // 1 píxel de arte = 1 píxel de referencia
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            root = (RectTransform)canvasGo.transform;

            var sky = UIFactory.Image(root, "Cielo", new Color(0.0f, 0.05f, 0.14f));
            sky.rectTransform.Stretch();
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

            toastsRoot = UIFactory.Rect(topLayer, "Avisos").Place(new Vector2(0.5f, 1), new Vector2(-150, -170), new Vector2(820, 300), new Vector2(0.5f, 1));
            var tv = UIFactory.VStack(toastsRoot.gameObject, 8);
            tv.childAlignment = TextAnchor.UpperCenter;

            var tr = UIFactory.Image(topLayer, "Transicion", new Color(0.0f, 0.03f, 0.10f, 0.85f));
            tr.rectTransform.Stretch();
            transition = tr.gameObject.AddComponent<CanvasGroup>();
            transition.alpha = 0; transition.blocksRaycasts = false;
            transitionText = UIFactory.Text(tr.transform, "Dia", "", 90, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            transitionText.rectTransform.Stretch();
        }

        /// <summary>Fondo de las pantallas de menú: el fondo original de la pantalla de inicio.</summary>
        void MenuBackdrop(RectTransform screen)
        {
            if (art.titleBackground == null) return;
            var bg = UIFactory.Image(screen, "FondoInicio", Color.white, art.titleBackground);
            bg.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, art.titleBackground.rect.size);
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
            CloseBuildingCard(true);
            CloseTutorial();
        }

        // ------------------------------------------------------------------ Pantalla de juego

        RectTransform BuildGameScreen()
        {
            var scr = UIFactory.Rect(screensLayer, "Juego").Stretch();

            // Vista con cámara: el mundo usa los píxeles del fondo original.
            var view = UIFactory.Rect(scr, "Vista").Stretch();
            view.gameObject.AddComponent<RectMask2D>();
            cameraRig = UIFactory.Rect(view, "Camara").TopLeft(Vector2.zero, CraterLayout.ViewSize);
            var world = UIFactory.Rect(cameraRig, "Mundo");
            world.anchorMin = world.anchorMax = new Vector2(0, 1);
            world.pivot = new Vector2(0, 1);
            world.anchoredPosition = new Vector2(-CraterLayout.ViewOrigin.x, CraterLayout.ViewOrigin.y);
            world.sizeDelta = CraterLayout.WorldSize;
            baseView = BaseView.Create(world, art);
            foreach (var b in baseView.buildings) b.Selected += OpenBuildingCard;
            fx = EffectsLayer.Create(scr, art);

            // Barra superior original: casillas Comida (agua y comida), Energía, Oxígeno y Salud (vivienda).
            var hud = UIFactory.Rect(scr, "HUD").Stretch();
            if (art.topBar != null) UIFactory.Image(hud, "FondoRecursos", Color.white, art.topBar).rectTransform.TopLeft(CraterLayout.TopBar, art.topBar.rect.size);
            string[] names = { "Comida", "Energía", "Oxígeno", "Salud" };
            Sprite[] slots = { art.slotFood, art.slotEnergy, art.slotOxygen, art.slotHealth };
            Sprite[] icons = { art.iconFood, art.iconEnergy, art.iconOxygen, art.iconHealth };
            for (int i = 0; i < 4; i++)
                indicators[i] = ResourceIndicator.Create(hud, names[i], slots[i], icons[i], CraterLayout.Slots[i], CraterLayout.Icons[i]);

            // Menú (hamburguesa): guarda y vuelve al menú principal.
            var menuBg = UIFactory.Image(hud, "FondoMenu", Color.white, art.menuBackground, true);
            menuBg.rectTransform.TopLeft(CraterLayout.MenuBackground, art.menuBackground != null ? art.menuBackground.rect.size : new Vector2(184, 129));
            var menuBtn = menuBg.gameObject.AddComponent<Button>();
            menuBtn.targetGraphic = menuBg;
            menuBtn.onClick.AddListener(() => { CloseBuildingCard(true); G.QuitToMenu(); ShowMenu(); });
            if (art.menuIcon != null) UIFactory.Image(hud, "IconoMenu", Color.white, art.menuIcon).rectTransform.TopLeft(CraterLayout.MenuIcon, art.menuIcon.rect.size);

            // Días.
            var days = UIFactory.Image(hud, "FondoDias", Color.white, art.daysBackground);
            days.rectTransform.TopLeft(CraterLayout.Days, art.daysBackground != null ? art.daysBackground.rect.size : new Vector2(201, 216));
            // El texto va en el recuadro claro superior del sprite (la parte derecha queda fuera de pantalla, como en la referencia).
            dayText = UIFactory.Text(days.transform, "Texto", "DÍA\n01/24", 30, new Color(0.25f, 0.15f, 0.02f), TextAlignmentOptions.Center, FontStyles.Bold);
            dayText.rectTransform.TopLeft(new Vector2(20, 18), new Vector2(138, 84));
            dayText.lineSpacing = -10;

            // Botón de siguiente turno (TERMINAR DÍA).
            var next = UIFactory.Image(hud, "SiguienteTurno", Color.white, art.nextTurn, true);
            next.rectTransform.TopLeft(CraterLayout.NextTurn, art.nextTurn != null ? art.nextTurn.rect.size : new Vector2(425, 182));
            endDayButton = next.gameObject.AddComponent<Button>();
            endDayButton.targetGraphic = next;
            var nc = endDayButton.colors; nc.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); endDayButton.colors = nc;
            endDayButton.onClick.AddListener(() => { CloseBuildingCard(true); G.turns.EndDay(); });
            var nextLabel = UIFactory.Text(next.transform, "Texto", "TERMINAR\nDÍA", 40, new Color(0.45f, 0.20f, 0.02f), TextAlignmentOptions.Center, FontStyles.Bold);
            nextLabel.rectTransform.TopLeft(new Vector2(175, 20), new Vector2(235, 142));
            endDayHint = UIFactory.Text(hud, "Pista", "", 22, Color.white, TextAlignmentOptions.Right, FontStyles.Bold);
            endDayHint.rectTransform.TopLeft(new Vector2(1000, 812), new Vector2(900, 34));

            BuildBuildingCard(scr);
            return scr;
        }

        void OnGameStarted()
        {
            CloseAllModals();
            ShowOnly(gameScreen);
            fx.SetEclipse(false);
            fx.SetMeteors(false);
            ResetCamera();
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

            indicators[0].Show(s.food, c.resourceMax, p.food, c.alertThreshold);
            indicators[1].Show(s.energy, c.resourceMax, p.energy, c.alertThreshold);
            indicators[2].Show(s.oxygen, c.resourceMax, p.oxygen, c.alertThreshold);
            indicators[3].Show(s.health, c.resourceMax, p.health, c.alertThreshold);
            dayText.text = "DÍA\n" + Mathf.Min(s.day, s.totalTurns).ToString("00") + "/" + s.totalTurns.ToString("00");

            // Eventos sobre la misma base: variantes y capas según el estado del día y la ventana abierta.
            var prompt = G.events.Current;
            bool eclipsePrompt = prompt != null && prompt.eventType == CraterEventType.EarthShadow;
            bool meteorPrompt = prompt != null && prompt.eventType == CraterEventType.Micrometeorites;
            if (prompt == null || prompt.kind != PromptKind.EclipseAllocation) baseView.PreviewEclipse(null);
            baseView.SetState(s, p);
            fx.SetEclipse(s.today.eclipse || eclipsePrompt);
            fx.SetMeteors(meteorPrompt || s.today.panelsStowed || s.today.roversStowed);

            bool can = G.turns.CanEndDay(out string why);
            endDayButton.interactable = can;
            endDayHint.text = can ? "" : why;
            RefreshBuildingCard();
        }

        // ------------------------------------------------------------------ Cámara

        bool zooming;
        Coroutine cameraMove;

        /// <summary>
        /// Posición de la cámara para que el centro del edificio quede en ZoomTarget, sin salir de la zona
        /// pintada del fondo (fuera de ella el sprite original tiene transparencias y trazos sueltos).
        /// </summary>
        Vector2 CameraPosFor(Vector2 focus, float zoom)
        {
            var o = CraterLayout.ViewOrigin;
            var tgt = CraterLayout.ZoomTarget;
            var min = CraterLayout.PaintedMin;
            var max = CraterLayout.PaintedMax;
            float left = Mathf.Clamp(focus.x - tgt.x / zoom, min.x, max.x - CraterLayout.ViewSize.x / zoom);
            float top = Mathf.Clamp(focus.y - tgt.y / zoom, min.y, max.y - CraterLayout.ViewSize.y / zoom);
            return new Vector2(-zoom * (left - o.x), zoom * (top - o.y));
        }

        void ResetCamera()
        {
            if (cameraMove != null) StopCoroutine(cameraMove);
            zooming = false;
            cameraRig.localScale = Vector3.one;
            cameraRig.anchoredPosition = Vector2.zero;
            baseView.SetSelectable(true);
        }

        IEnumerator MoveCamera(Vector2 pos, float zoom)
        {
            zooming = true;
            Vector2 p0 = cameraRig.anchoredPosition;
            float z0 = cameraRig.localScale.x, t = 0;
            while (t < CraterLayout.ZoomSeconds)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / CraterLayout.ZoomSeconds);
                k = k * k * (3f - 2f * k);
                cameraRig.anchoredPosition = Vector2.Lerp(p0, pos, k);
                cameraRig.localScale = Vector3.one * Mathf.Lerp(z0, zoom, k);
                yield return null;
            }
            cameraRig.anchoredPosition = pos;
            cameraRig.localScale = Vector3.one * zoom;
            zooming = false;
        }

        // ------------------------------------------------------------------ Ficha del edificio (después del acercamiento)

        RectTransform card;
        CanvasGroup cardGroup;
        TextMeshProUGUI cardName, cardStatus, cardInfo, cardNumbers;
        Button cardToggle;
        BuildingId? cardId;

        void BuildBuildingCard(RectTransform parent)
        {
            var img = UIFactory.Image(parent, "PanelEdificio", Color.white, art.panel, true);
            img.type = Image.Type.Sliced;
            card = img.rectTransform.TopLeft(CraterLayout.InfoPanelPos, CraterLayout.InfoPanelSize);
            cardGroup = card.gameObject.AddComponent<CanvasGroup>();
            var body = UIFactory.Rect(card, "Contenido").Stretch(46, 40, 46, 70);
            var v = UIFactory.VStack(body.gameObject, 18);
            v.childForceExpandHeight = false;
            Color ink = new Color(0.25f, 0.15f, 0.02f);
            cardName = UIFactory.Text(body, "Nombre", "", 56, ink, TextAlignmentOptions.Left, FontStyles.Bold);
            cardStatus = UIFactory.Text(body, "Estado", "", 32, Theme.Good, TextAlignmentOptions.Left, FontStyles.Bold);
            cardInfo = UIFactory.Text(body, "Funcion", "", 30, ink);
            cardNumbers = UIFactory.Text(body, "Numeros", "", 30, ink, TextAlignmentOptions.Left, FontStyles.Bold);
            foreach (var t in new[] { cardName, cardStatus, cardInfo, cardNumbers }) t.gameObject.AddComponent<LayoutElement>();
            var spacer = UIFactory.Rect(body, "Espacio");
            UIFactory.Size(spacer, 10).flexibleHeight = 1;
            cardToggle = UIFactory.Button(body, "Encender", "", Theme.Good, ink, 38, () => { if (cardId.HasValue) G.buildings.Toggle(cardId.Value); });
            UIFactory.Size(cardToggle, 120);
            var close = UIFactory.Button(body, "Cerrar", "CERRAR", Color.white, ink, 32, () => CloseBuildingCard(false));
            UIFactory.Size(close, 100);
            card.gameObject.SetActive(false);
        }

        /// <summary>1) selección → 2) acercamiento de cámara → 3) al terminar, aparece el panel.</summary>
        void OpenBuildingCard(BuildingId id)
        {
            if (zooming || cardId.HasValue || G.State == null || G.events.HasPending || G.turns.Busy) return;
            StartCoroutine(SelectRoutine(id));
        }

        IEnumerator SelectRoutine(BuildingId id)
        {
            baseView.SetSelectable(false);
            cardId = id;
            if (cameraMove != null) StopCoroutine(cameraMove);
            cameraMove = StartCoroutine(MoveCamera(CameraPosFor(CraterLayout.Focus(id), CraterLayout.ZoomFactor), CraterLayout.ZoomFactor));
            yield return cameraMove;
            if (cardId != id) yield break;
            card.gameObject.SetActive(true);
            card.SetAsLastSibling();
            RefreshBuildingCard();
            float t = 0;
            while (t < 0.15f) { t += Time.unscaledDeltaTime; cardGroup.alpha = t / 0.15f; yield return null; }
            cardGroup.alpha = 1;
        }

        /// <summary>Oculta el panel y devuelve la cámara a la vista general (inmediato si "instant").</summary>
        void CloseBuildingCard(bool instant)
        {
            bool wasOpen = cardId.HasValue || zooming;
            cardId = null;
            if (card != null) card.gameObject.SetActive(false);
            if (cameraRig == null) return;
            if (instant || !wasOpen) { if (wasOpen || cameraRig.localScale.x != 1f) ResetCamera(); return; }
            StartCoroutine(ZoomOutRoutine());
        }

        IEnumerator ZoomOutRoutine()
        {
            if (cameraMove != null) StopCoroutine(cameraMove);
            cameraMove = StartCoroutine(MoveCamera(Vector2.zero, 1f));
            yield return cameraMove;
            baseView.SetSelectable(true);
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
            cardStatus.color = r.visual == BuildingVisual.Working ? Theme.Good : r.visual == BuildingVisual.Off ? new Color(0.35f, 0.35f, 0.35f)
                             : r.visual == BuildingVisual.NoPower || r.visual == BuildingVisual.NoWater ? Theme.Bad : new Color(0.65f, 0.40f, 0.0f);
            cardInfo.text = data.description + " " + data.howItWorks;

            var parts = new List<string>();
            if (st.energy != 0) parts.Add("Energía " + Num.Signed(st.energy));
            if (st.oxygen != 0) parts.Add("Oxígeno " + Num.Signed(st.oxygen));
            if (st.food != 0) parts.Add("Comida " + Num.Signed(st.food));
            cardNumbers.text = string.Join("   ", parts) + " por día";

            bool on = G.State.IsOn(id);
            cardToggle.interactable = G.buildings.CanToggle(out _);
            cardToggle.GetComponent<Image>().color = UIFactory.ArtTint(on ? Theme.OffGray : Theme.Good);
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
            stripe.rectTransform.offsetMin = new Vector2(8, 8); stripe.rectTransform.offsetMax = new Vector2(20, -26);
            var t = UIFactory.Text(p.transform, "Texto", text, 22, Theme.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
            t.rectTransform.Stretch(32, 8, 16, 28);
            UIFactory.Size(p, Mathf.Max(80, t.GetPreferredValues(text, 760, 0).y + 40));
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
