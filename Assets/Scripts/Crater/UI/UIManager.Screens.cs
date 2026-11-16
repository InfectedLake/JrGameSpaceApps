using System.Collections.Generic;
using System.Linq;
using Crater.Core;
using Crater.Data;
using Crater.Services;
using Crater.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    // Pantallas completas: menú, configuración, opciones, créditos, ciencia y final.
    public partial class UIManager
    {
        RectTransform startChoice;
        static readonly Color MenuInk = new Color(0.20f, 0.12f, 0.02f);

        // ------------------------------------------------------------------ Menú principal (arte original: pantalla de inicio)

        RectTransform BuildMainMenu()
        {
            var scr = UIFactory.Rect(screensLayer, "Menu").Stretch();
            MenuBackdrop(scr);
            var screen = UIFactory.Rect(scr, "Composicion").Place(new Vector2(0.5f, 0.5f), Vector2.zero, CraterLayout.ViewSize);

            var banner = UIFactory.Image(screen, "FondoTitulo", Color.white, art.titleBanner);
            banner.rectTransform.TopLeft(CraterLayout.TitleBanner, art.titleBanner != null ? art.titleBanner.rect.size : new Vector2(1020, 142));
            var title = UIFactory.Text(banner.transform, "Titulo", "CRATER", 104, MenuInk, TextAlignmentOptions.Center, FontStyles.Bold);
            title.rectTransform.Stretch(10, 6, 10, 6);
            title.characterSpacing = 24;

            MenuArtButton(screen, art.buttonStart, CraterLayout.MenuButtons[0], "MAIN_MENU_START", OpenStart);
            MenuArtButton(screen, art.buttonOptions, CraterLayout.MenuButtons[1], "MAIN_MENU_OPTIONS", ShowOptions);
            MenuArtButton(screen, art.buttonCredits, CraterLayout.MenuButtons[2], "MAIN_MENU_CREDITS", () => ShowOnly(creditsScreen));
            MenuArtButton(screen, art.buttonExit, CraterLayout.MenuButtons[3], "MAIN_MENU_EXIT", Quit);

            // Selector de idioma en la esquina inferior izquierda (zona libre del fondo lunar).
            var lang = UIFactory.Panel(screen, "Idioma", Color.white);
            lang.rectTransform.TopLeft(new Vector2(60, 880), new Vector2(560, 170));
            var langBody = UIFactory.Rect(lang.transform, "Contenido").Stretch(26, 14, 26, 14 + UIFactory.TopInset);
            var lv = UIFactory.VStack(langBody.gameObject, 8);
            lv.childForceExpandHeight = false;
            var langLabel = UIFactory.Text(langBody, "Etiqueta", "", 26, MenuInk, TextAlignmentOptions.Center, FontStyles.Bold).L("OPTIONS_LANGUAGE");
            UIFactory.Size(langLabel, 36, -1, -1, 36);
            var langPair = LanguageButtons(langBody, 70, 28);
            UIFactory.Size(langPair, 70, -1, -1, 70);

            // INICIO con partida guardada: elegir entre continuar o empezar una nueva.
            startChoice = UIFactory.Rect(scr, "Inicio").Stretch();
            var dim = startChoice.gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0.02f, 0.08f, 0.6f);
            var box = UIFactory.Panel(startChoice, "Caja", Color.white);
            box.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 420));
            var stack = UIFactory.Rect(box.transform, "Opciones").Stretch(50, 40, 50, 60);
            UIFactory.VStack(stack.gameObject, 20);
            UIFactory.Size(UIFactory.Button(stack, "Continuar", "", Theme.Accent, MenuInk, 36, () =>
            {
                startChoice.gameObject.SetActive(false);
                if (!G.ContinueGame()) Toast(Loc.T("NOTICE_NO_SAVE"), NoticeKind.Warning);
            }).L("MAIN_MENU_CONTINUE"), 96);
            UIFactory.Size(UIFactory.Button(stack, "Nueva", "", Color.white, MenuInk, 36, () => { startChoice.gameObject.SetActive(false); ShowSetup(); }).L("MAIN_MENU_NEW_GAME"), 96);
            UIFactory.Size(UIFactory.Button(stack, "Volver", "", Theme.OffGray, MenuInk, 28, () => startChoice.gameObject.SetActive(false)).L("BACK"), 76);
            startChoice.gameObject.SetActive(false);
            return scr;
        }

        /// <summary>Botón con su sprite original. El estado seleccionado se muestra aclarando el mismo botón
        /// (transición de color, como en la captura de referencia).</summary>
        void MenuArtButton(RectTransform parent, Sprite sprite, Vector2 pos, string key, UnityEngine.Events.UnityAction onClick)
        {
            var img = UIFactory.Image(parent, "Boton_" + key, Color.white, sprite, true);
            img.rectTransform.TopLeft(pos, sprite != null ? sprite.rect.size : new Vector2(401, 109));
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var colors = b.colors;
            colors.normalColor = new Color(0.88f, 0.88f, 0.88f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            b.colors = colors;
            b.onClick.AddListener(onClick);
            b.onClick.AddListener(() => UIFactory.ClickSound?.Invoke());   // sonido de clic (audio, no estilo)
            var t = UIFactory.Text(img.transform, "Texto", "", 44, MenuInk, TextAlignmentOptions.Center, FontStyles.Bold).L(key);
            t.rectTransform.Stretch(8, 4, 8, 4);
            t.enableAutoSizing = true; t.fontSizeMin = 30; t.fontSizeMax = 44;
            t.textWrappingMode = TextWrappingModes.NoWrap;
        }

        void OpenStart()
        {
            if (G.saves.HasSave) { startChoice.gameObject.SetActive(true); startChoice.SetAsLastSibling(); }
            else ShowSetup();
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ShowMenu()
        {
            CloseAllModals();
            ShowOnly(menuScreen);
            if (startChoice != null) startChoice.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Nueva partida

        TMP_InputField baseNameInput;
        readonly List<TMP_InputField> nameInputs = new List<TMP_InputField>();
        int setupAstronauts = 3, setupCycles = 6;
        bool setupTutorial = true;
        readonly List<Button> crewButtons = new List<Button>(), lengthButtons = new List<Button>();
        Button tutorialToggle;

        RectTransform BuildSetup()
        {
            var scr = UIFactory.Rect(screensLayer, "Configuracion").Stretch();
            MenuBackdrop(scr);
            var card = UIFactory.Panel(scr, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240, 960));
            var t = UIFactory.Text(card.transform, "Titulo", "", 52, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold).L("SETUP_TITLE");
            t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30 - UIFactory.TopInset), new Vector2(1100, 70), new Vector2(0.5f, 1));

            var body = UIFactory.Rect(card.transform, "Cuerpo").Stretch(84, 140, 84, 116 + UIFactory.TopInset);
            UIFactory.VStack(body.gameObject, 14);

            Label(body, "SETUP_BASE_NAME");
            baseNameInput = UIFactory.Input(body, "NombreBase", "Base Shackleton", "");
            ((TextMeshProUGUI)baseNameInput.placeholder).L("SETUP_BASE_PLACEHOLDER");
            UIFactory.Size(baseNameInput, 78, -1, -1, 78);

            Label(body, "SETUP_CREW_COUNT");
            var crewRow = UIFactory.Rect(body, "Tripulacion");
            UIFactory.HStack(crewRow.gameObject, 14);
            UIFactory.Size(crewRow, 64);
            foreach (int n in new[] { 2, 3, 4 })
            {
                int v = n;
                var cb = UIFactory.Button(crewRow, "N" + n, "", Theme.PanelSoft, Theme.Ink, 26, () => { setupAstronauts = v; RefreshSetup(); });
                if (n == 3) cb.L(() => Loc.T("SETUP_CREW_NORMAL", n)); else cb.Label().text = n.ToString();
                crewButtons.Add(cb);
            }

            Label(body, "SETUP_ASTRONAUT_NAMES");
            var namesRow = UIFactory.Rect(body, "Nombres");
            UIFactory.HStack(namesRow.gameObject, 14);
            UIFactory.Size(namesRow, 78, -1, -1, 78);
            for (int i = 0; i < 4; i++)
            {
                var input = UIFactory.Input(namesRow, "Astronauta" + i, DefaultContent.DefaultAstronauts[i], "");
                ((TextMeshProUGUI)input.placeholder).L("SETUP_NAME_PLACEHOLDER");
                nameInputs.Add(input);
            }

            Label(body, "SETUP_LENGTH");
            var lenRow = UIFactory.Rect(body, "Duracion");
            UIFactory.HStack(lenRow.gameObject, 14);
            UIFactory.Size(lenRow, 64);
            foreach (var (cycles, key) in new[] { (4, "SETUP_LENGTH_SHORT"), (6, "SETUP_LENGTH_NORMAL"), (8, "SETUP_LENGTH_LONG") })
            {
                int v = cycles;
                lengthButtons.Add(UIFactory.Button(lenRow, key, "", Theme.PanelSoft, Theme.Ink, 24, () => { setupCycles = v; RefreshSetup(); }).L(() => Loc.T(key, v)));
            }

            tutorialToggle = UIFactory.Button(body, "Tutorial", "", Theme.PanelSoft, Theme.Ink, 24, () => { setupTutorial = !setupTutorial; RefreshSetup(); });
            UIFactory.Size(tutorialToggle, 60);

            var note = UIFactory.Text(body, "Nota", "", 18, Theme.InkSoft).L("SETUP_NOTE");
            UIFactory.Size(note, 50);

            var back = UIFactory.Button(card.transform, "Volver", "", Theme.PanelSoft, Theme.Ink, 28, ShowMenu).L("BACK");
            ((RectTransform)back.transform).Place(new Vector2(0, 0), new Vector2(60, 30), new Vector2(260, 76), Vector2.zero);
            var start = UIFactory.Button(card.transform, "Empezar", "", Theme.Accent, Color.white, 32, StartMission).L("SETUP_START");
            ((RectTransform)start.transform).Place(new Vector2(1, 0), new Vector2(-60, 30), new Vector2(460, 76), new Vector2(1, 0));
            return scr;
        }

        static void Label(Transform parent, string key)
        {
            var l = UIFactory.Text(parent, "Etiqueta", "", 22, Theme.InkSoft, TextAlignmentOptions.Left, FontStyles.Bold).L(key);
            l.enableAutoSizing = true; l.fontSizeMin = 16; l.fontSizeMax = 22;
            UIFactory.Size(l, 30);
        }

        void ShowSetup()
        {
            setupTutorial = G.saves.ShowTutorial;
            ShowOnly(setupScreen);
            RefreshSetup();
        }

        void RefreshSetup()
        {
            int[] crew = { 2, 3, 4 };
            for (int i = 0; i < crewButtons.Count; i++) Select(crewButtons[i], crew[i] == setupAstronauts);
            int[] len = { 4, 6, 8 };
            for (int i = 0; i < lengthButtons.Count; i++) Select(lengthButtons[i], len[i] == setupCycles);
            for (int i = 0; i < nameInputs.Count; i++) nameInputs[i].gameObject.SetActive(i < setupAstronauts);
            tutorialToggle.Label().text = Loc.T(setupTutorial ? "SETUP_TUTORIAL_ON" : "SETUP_TUTORIAL_OFF");
        }

        static void Select(Button b, bool on)
        {
            b.GetComponent<Image>().color = UIFactory.ArtPanel != null ? UIFactory.ArtTint(on ? Theme.Accent : Theme.PanelSoft) : (on ? Theme.Accent : Theme.PanelSoft);
            b.Label().color = on ? Color.white : Theme.Ink;
        }

        void StartMission()
        {
            var setup = new MissionSetup
            {
                baseName = baseNameInput.text,
                astronauts = setupAstronauts,
                cycles = setupCycles,
                tutorial = setupTutorial,
            };
            for (int i = 0; i < setupAstronauts; i++)
                setup.astronautNames.Add(string.IsNullOrWhiteSpace(nameInputs[i].text) ? DefaultContent.DefaultAstronauts[i] : nameInputs[i].text.Trim());
            G.saves.ShowTutorial = setupTutorial;
            G.NewGame(setup);
        }

        // ------------------------------------------------------------------ Opciones

        TextMeshProUGUI optionsNasa;
        Button liveToggle;

        RectTransform BuildOptions()
        {
            var scr = UIFactory.Rect(screensLayer, "Opciones").Stretch();
            MenuBackdrop(scr);
            // La tarjeta se ancla a la pantalla con márgenes (6% / 7%): nunca sale de la vista en ninguna resolución.
            var card = UIFactory.Panel(scr, "Tarjeta", Theme.Panel);
            card.rectTransform.anchorMin = new Vector2(0.07f, 0.06f);
            card.rectTransform.anchorMax = new Vector2(0.93f, 0.94f);
            card.rectTransform.offsetMin = card.rectTransform.offsetMax = Vector2.zero;
            var t = UIFactory.Text(card.transform, "Titulo", "", 56, MenuInk, TextAlignmentOptions.Center, FontStyles.Bold).L("OPTIONS_TITLE");
            t.rectTransform.anchorMin = new Vector2(0, 1); t.rectTransform.anchorMax = new Vector2(1, 1);
            t.rectTransform.pivot = new Vector2(0.5f, 1);
            t.rectTransform.offsetMin = new Vector2(80, -(30 + UIFactory.TopInset + 76)); t.rectTransform.offsetMax = new Vector2(-80, -(30 + UIFactory.TopInset));

            // Cuerpo: área con desplazamiento vertical (máscara) cuyo contenido organiza AdaptiveColumns:
            // dos columnas del mismo ancho si caben, o una sola columna en pantallas estrechas.
            // Los anchos los asigna AdaptiveColumns: no dependen del largo de los textos.
            var view = UIFactory.Rect(card.transform, "Cuerpo").Stretch(90, 150, 90, 140 + UIFactory.TopInset);
            view.gameObject.AddComponent<RectMask2D>();
            var viewHit = view.gameObject.AddComponent<Image>();
            viewHit.color = new Color(1, 1, 1, 0.001f);
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            var cols = UIFactory.Rect(view, "Columnas");
            cols.anchorMin = new Vector2(0, 1); cols.anchorMax = new Vector2(1, 1);
            cols.pivot = new Vector2(0.5f, 1);
            cols.offsetMin = cols.offsetMax = Vector2.zero;
            scroll.viewport = view;
            scroll.content = cols;

            var left = Section(cols, "SonidoYPantalla", "OPTIONS_SOUND_DISPLAY");
            BuildSettingsControls(left, 0);
            var body = Section(cols, "DatosYPartida", "OPTIONS_DATA_GAME");
            var adaptive = cols.gameObject.AddComponent<AdaptiveColumns>();
            adaptive.left = left; adaptive.right = body;
            adaptive.gap = 80; adaptive.minColumnWidth = 640;   // menos de 640 px por columna (p. ej. 4:3) → una sola columna
            liveToggle = UIFactory.Button(body, "Vivo", "", Theme.PanelSoft, Theme.Ink, 26, () =>
            {
                G.saves.UseLiveData = !G.saves.UseLiveData;
                G.nasa.Initialize(G.saves.UseLiveData);
                RefreshOptions();
            });
            UIFactory.Size(liveToggle, 86, -1, -1, 86);
            optionsNasa = UIFactory.Text(body, "Estado", "", 22, Theme.InkSoft);
            optionsNasa.enableAutoSizing = true; optionsNasa.fontSizeMin = 16; optionsNasa.fontSizeMax = 22;
            optionsNasa.overflowMode = TextOverflowModes.Ellipsis;
            UIFactory.Size(optionsNasa, 120, -1, -1, 120);
            var retry = UIFactory.Button(body, "Reintentar", "", Theme.PanelSoft, Theme.Ink, 24, () => G.nasa.Initialize(G.saves.UseLiveData)).L("OPTIONS_RETRY_NASA");
            UIFactory.Size(retry, 86, -1, -1, 86);
            var tut = UIFactory.Button(body, "Tutorial", "", Theme.PanelSoft, Theme.Ink, 24, null);
            tut.onClick.AddListener(() => { G.saves.ShowTutorial = !G.saves.ShowTutorial; RefreshOptions(); });
            UIFactory.Size(tut, 86, -1, -1, 86);
            var del = UIFactory.Button(body, "Borrar", "", new Color(1f, 0.9f, 0.9f), Theme.Bad, 24, () =>
            {
                G.saves.Delete();
                Toast(Loc.T("NOTICE_SAVE_DELETED"), NoticeKind.Info);
            }).L("OPTIONS_DELETE_SAVE");
            UIFactory.Size(del, 86, -1, -1, 86);

            var back = UIFactory.Button(card.transform, "Volver", "", Theme.Accent, MenuInk, 30, () => { G.sound.Save(); ShowMenu(); }).L("BACK");
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(340, 96), new Vector2(0.5f, 0));
            G.nasa.StatusChanged += RefreshOptions;
            tutToggleRef = tut;
            return scr;
        }

        Button tutToggleRef;

        /// <summary>Sección (columna) de opciones: título + controles apilados. Su ancho lo fija AdaptiveColumns.</summary>
        RectTransform Section(RectTransform parent, string name, string titleKey)
        {
            var col = UIFactory.Rect(parent, name);
            var v = UIFactory.VStack(col.gameObject, 20);
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            var t = UIFactory.Text(col, "Titulo", "", 32, MenuInk, TextAlignmentOptions.MidlineLeft, FontStyles.Bold).L(titleKey);
            t.enableAutoSizing = true; t.fontSizeMin = 24; t.fontSizeMax = 32;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Ellipsis;
            var le = UIFactory.Size(t, 52, 0, -1, 52);
            le.minWidth = 0;
            return col;
        }

        void ShowOptions()
        {
            ShowOnly(optionsScreen);
            SyncSettings();
            RefreshOptions();
        }

        void RefreshOptions()
        {
            if (liveToggle == null) return;
            liveToggle.Label().text = Loc.T(G.saves.UseLiveData ? "OPTIONS_LIVE_ON" : "OPTIONS_LIVE_OFF");
            optionsNasa.text = G.nasa.StatusText + "\n" + Loc.T("OPTIONS_ENDPOINT", G.nasa.baseUrl);
            tutToggleRef.Label().text = Loc.T(G.saves.ShowTutorial ? "OPTIONS_TUTORIAL_ON" : "OPTIONS_TUTORIAL_OFF");
        }

        // ------------------------------------------------------------------ Créditos

        RectTransform BuildCredits()
        {
            var scr = UIFactory.Rect(screensLayer, "Creditos").Stretch();
            MenuBackdrop(scr);
            var card = UIFactory.Panel(scr, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 860));
            var t = UIFactory.Text(card.transform, "Titulo", "", 52, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold).L("CREDITS_TITLE");
            t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30 - UIFactory.TopInset), new Vector2(1000, 70), new Vector2(0.5f, 1));
            var text = UIFactory.Text(card.transform, "Texto", "", 26, Theme.Ink).L("CREDITS_TEXT");
            text.Fit(80, 130, 80, 112, 18, 26);
            var back = UIFactory.Button(card.transform, "Volver", "", Theme.Accent, Color.white, 28, ShowMenu).L("BACK");
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(-180, 30), new Vector2(300, 76), new Vector2(0.5f, 0));
            var science = UIFactory.Button(card.transform, "Ciencia", "", Color.white, Theme.Ink, 28, () => ShowScienceScreen(null)).L("SCIENCE_TITLE");
            ((RectTransform)science.transform).Place(new Vector2(0.5f, 0), new Vector2(180, 30), new Vector2(300, 76), new Vector2(0.5f, 0));
            return scr;
        }

        // ------------------------------------------------------------------ Ciencia real

        string scienceId;   // ficha abierta (para volver a dibujarla al cambiar de idioma)

        void ShowScienceScreen(string id)
        {
            scienceId = id;
            ShowOnly(scienceScreen);
            UIFactory.Clear(scienceScreen);
            MenuBackdrop(scienceScreen);
            var card = UIFactory.Panel(scienceScreen, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1700, 960));
            var t = UIFactory.Text(card.transform, "Titulo", Loc.T("SCIENCE_TITLE"), 48, Theme.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(60, -24 - UIFactory.TopInset), new Vector2(800, 64), new Vector2(0, 1));
            var area = UIFactory.Rect(card.transform, "Area").Stretch(60, 120, 60, 100 + UIFactory.TopInset);
            BuildScienceBrowser(area, id);
            var back = UIFactory.Button(card.transform, "Volver", Loc.T("BACK"), Theme.Accent, Color.white, 28, () => ShowOnly(creditsScreen));
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(300, 72), new Vector2(0.5f, 0));
        }

        /// <summary>Lista de fichas a la izquierda y detalle a la derecha. Se usa en pantalla completa y en ventana.</summary>
        void BuildScienceBrowser(RectTransform area, string selectId)
        {
            var list = UIFactory.Scroll(area, "Lista", 8, 6);
            var listRoot = (RectTransform)list.parent.parent;
            listRoot.anchorMin = new Vector2(0, 0); listRoot.anchorMax = new Vector2(0.3f, 1);
            listRoot.offsetMin = Vector2.zero; listRoot.offsetMax = new Vector2(-10, 0);

            var detail = UIFactory.Scroll(area, "Detalle", 24, 28);
            var detailRoot = (RectTransform)detail.parent.parent;
            detailRoot.anchorMin = new Vector2(0.3f, 0); detailRoot.anchorMax = new Vector2(1, 1);
            detailRoot.offsetMin = new Vector2(10, 0); detailRoot.offsetMax = Vector2.zero;

            void ShowEntry(ScienceData d)
            {
                UIFactory.Clear(detail);
                scienceId = d == null ? "valores" : d.id;
                if (d == null)
                {
                    Para(detail, "<b>" + Loc.T("SCIENCE_GAME_VALUES") + "</b>", 34, Theme.Ink);
                    Para(detail, Loc.T("SCIENCE_GAME_VALUES_INTRO"), 24, Theme.Ink);
                    TagLine(detail, DataTag.Real, Loc.T("SCIENCE_TAG_REAL"));
                    TagLine(detail, DataTag.Juego, Loc.T("SCIENCE_TAG_GAME"));
                    TagLine(detail, DataTag.Pendiente, Loc.T("SCIENCE_TAG_PENDING"));
                    Para(detail, ScienceInfoManager.GameValuesSummary(G.Config), 22, Theme.Ink);
                    Para(detail, "<b>" + Loc.T("SCIENCE_NASA_STATUS") + "</b>\n" + G.nasa.StatusText, 22, Theme.InkSoft);
                    return;
                }
                Para(detail, "<b>" + d.LTitle() + "</b>", 40, MenuInk);
                Para(detail, d.LExplanation(), 28, Theme.Ink);
                // BLOQUE 1: dato real + su fuente (dentro del mismo bloque, justo debajo del dato que respalda).
                string fact = d.LFact(), source = d.LSource(), note = d.LGameNote();
                if (!string.IsNullOrEmpty(fact))
                    Box(detail, d.factTag, Loc.T(d.factTag == DataTag.Real ? "SCIENCE_REAL_FACT" : "SCIENCE_PENDING_FACT"), fact,
                        string.IsNullOrEmpty(source) ? null : Loc.T("SCIENCE_SOURCE", source));
                // BLOQUE 2: valor o simplificación del juego.
                if (!string.IsNullOrEmpty(note)) Box(detail, DataTag.Juego, Loc.T("SCIENCE_GAME_VALUE"), note, null);
            }

            ScienceData first = null;
            foreach (var d in G.science.All)
            {
                var entry = d;
                var b = UIFactory.Button(list, d.id, d.LTitle(), Color.white, Theme.Ink, 22, () => ShowEntry(entry));
                b.Label().alignment = TextAlignmentOptions.Left;
                UIFactory.Size(b, 78, -1, -1, 78);
                if (d.id == selectId) first = d;
            }
            var gv = UIFactory.Button(list, "valores", Loc.T("SCIENCE_GAME_VALUES_BUTTON"), Theme.PanelSoft, Theme.Accent, 22, () => ShowEntry(null));
            gv.Label().alignment = TextAlignmentOptions.Left;
            UIFactory.Size(gv, 78, -1, -1, 78);
            ShowEntry(selectId == "valores" ? null : first ?? (string.IsNullOrEmpty(selectId) ? G.science.All.FirstOrDefault() : null));
        }

        static TextMeshProUGUI Para(Transform parent, string text, float size, Color color)
        {
            var t = UIFactory.Text(parent, "Parrafo", text, size, color);
            t.gameObject.AddComponent<LayoutElement>();
            return t;
        }

        /// <summary>
        /// Bloque de información: etiqueta ([REAL] / [JUEGO] / [PENDIENTE]) + texto completo + fuente opcional.
        /// Párrafos con alineación superior izquierda, ajuste de línea y márgenes internos; el alto crece con el
        /// contenido (la vista tiene desplazamiento), así nunca se recorta información científica.
        /// </summary>
        static void Box(Transform parent, DataTag tag, string heading, string text, string source)
        {
            var box = UIFactory.Panel(parent, "Bloque_" + tag, Color.white, false);
            var v = UIFactory.VStack(box.gameObject, 12);
            v.padding = new RectOffset(34, 34, 24 + (int)UIFactory.TopInset, 28);
            v.childForceExpandHeight = false;
            var head = UIFactory.Text(box.transform, "Etiqueta", UIFactory.Colored("<b>" + DefaultContent.TagLabel(tag) + "</b>", DefaultContent.TagColor(tag)) + "  <b>" + heading + "</b>",
                24, MenuInk, TextAlignmentOptions.TopLeft);
            head.gameObject.AddComponent<LayoutElement>();
            var t = UIFactory.Text(box.transform, "Texto", text, 26, Theme.Ink, TextAlignmentOptions.TopLeft);
            t.gameObject.AddComponent<LayoutElement>();
            if (source == null) return;
            var s = UIFactory.Text(box.transform, "Fuente", source, 22, Theme.InkSoft, TextAlignmentOptions.TopLeft, FontStyles.Italic);
            s.gameObject.AddComponent<LayoutElement>();
        }

        static void TagLine(Transform parent, DataTag tag, string text)
            => Para(parent, UIFactory.Colored("<b>" + DefaultContent.TagLabel(tag) + "</b>", DefaultContent.TagColor(tag)) + " " + text, 22, Theme.Ink);

        // ------------------------------------------------------------------ Pantalla final

        /// <summary>Final sencillo: resultado, días sobrevividos, estado de la tripulación y dos botones.</summary>
        void ShowEnd(Outcome outcome)
        {
            CloseAllModals();
            ShowOnly(endScreen);
            UIFactory.Clear(endScreen);
            MenuBackdrop(endScreen);
            var s = G.State;
            bool win = outcome == Outcome.Victory;
            int survived = win ? s.totalTurns : Mathf.Max(0, s.day - 1);

            var card = UIFactory.Panel(endScreen, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 760));
            var badge = UIFactory.Image(card.transform, "Medalla", win ? Theme.Good : Theme.Bad, SpriteFactory.Circle);
            badge.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(170, 170), new Vector2(0.5f, 1));
            var glow = UIFactory.Image(badge.transform, "Brillo", new Color(1, 1, 1, 0.25f), SpriteFactory.Ring);
            glow.rectTransform.Stretch(-20, -20, -20, -20);
            var mark = UIFactory.Text(badge.transform, "Marca", survived.ToString(), 90, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            mark.rectTransform.Stretch();

            var title = UIFactory.Text(card.transform, "Titulo", Loc.T(win ? "END_MISSION_COMPLETE" : "END_MISSION_FAILED"), 76, win ? Theme.Good : Theme.Bad, TextAlignmentOptions.Center, FontStyles.Bold);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -230), new Vector2(1000, 96), new Vector2(0.5f, 1));
            title.enableAutoSizing = true; title.fontSizeMin = 48; title.fontSizeMax = 76;

            // Los nombres que escribió el jugador no se traducen.
            string crew = s.astronautNames != null && s.astronautNames.Count > 0 ? string.Join(", ", s.astronautNames) : Loc.T("END_THE_CREW");
            string days = Loc.T("END_DAYS_SURVIVED", survived, s.totalTurns);
            string body = win
                ? days + "\n" + Loc.T(s.health >= 70 ? "END_WIN_HEALTHY" : s.health >= 35 ? "END_WIN_TIRED" : "END_WIN_CLOSE", crew)
                : (string.IsNullOrEmpty(s.defeatCause) ? Loc.T("DEFEAT_HEALTH") : Loc.Stored(s.defeatCause)) + "\n" + days;
            var text = UIFactory.Text(card.transform, "Texto", body, 36, Theme.Ink, TextAlignmentOptions.Center);
            text.enableAutoSizing = true; text.fontSizeMin = 24; text.fontSizeMax = 36; text.overflowMode = TextOverflowModes.Ellipsis;
            text.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -340), new Vector2(940, 200), new Vector2(0.5f, 1));

            var menu = UIFactory.Button(card.transform, "Menu", Loc.T("PAUSE_MAIN_MENU"), Theme.PanelSoft, Theme.Ink, 30, () => { G.QuitToMenu(); ShowMenu(); });
            ((RectTransform)menu.transform).Place(new Vector2(0.5f, 0), new Vector2(-240, 40), new Vector2(440, 90), new Vector2(0.5f, 0));
            var again = UIFactory.Button(card.transform, "Otra", Loc.T(win ? "END_PLAY_AGAIN" : "END_TRY_AGAIN"), Theme.Accent, Color.white, 30, () => { G.QuitToMenu(); ShowSetup(); });
            ((RectTransform)again.transform).Place(new Vector2(0.5f, 0), new Vector2(240, 40), new Vector2(440, 90), new Vector2(0.5f, 0));
        }
    }
}
