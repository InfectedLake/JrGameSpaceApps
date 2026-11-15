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
        Button continueButton;

        // ------------------------------------------------------------------ Menú principal

        RectTransform BuildMainMenu()
        {
            var scr = UIFactory.Rect(screensLayer, "Menu").Stretch();
            var moon = UIFactory.Image(scr, "Luna", Theme.Ground, SpriteFactory.Circle);
            moon.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, -900), new Vector2(2600, 1500));
            var r = new System.Random(11);
            for (int i = 0; i < 12; i++)
            {
                var c = UIFactory.Image(moon.transform, "Crater", Theme.GroundDark, SpriteFactory.Circle);
                float s = 60 + (float)r.NextDouble() * 160;
                c.rectTransform.Place(new Vector2((float)(0.25 + r.NextDouble() * 0.5), (float)(0.78 + r.NextDouble() * 0.2)), Vector2.zero, new Vector2(s, s * 0.6f));
            }
            var miniBase = UIFactory.Image(scr, "MiniBase", new Color(0.95f, 0.96f, 0.99f), SpriteFactory.Circle);
            miniBase.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(380, 520), new Vector2(150, 90));
            var miniRing = UIFactory.Image(miniBase.transform, "Anillo", new Color(0.55f, 0.70f, 0.95f), SpriteFactory.Ring);
            miniRing.rectTransform.Stretch();

            var title = UIFactory.Text(scr, "Titulo", "CRATER", 170, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(1200, 200), new Vector2(0.5f, 1));
            title.characterSpacing = 18;
            var sub = UIFactory.Text(scr, "Subtitulo", "Comanda una base en el polo sur de la Luna", 34, new Color(0.85f, 0.90f, 1f), TextAlignmentOptions.Center);
            sub.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -290), new Vector2(1200, 50), new Vector2(0.5f, 1));

            var col = UIFactory.Rect(scr, "Botones").Place(new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(520, 560));
            UIFactory.VStack(col.gameObject, 16);
            MenuButton(col, "NUEVA PARTIDA", Theme.Accent, Color.white, ShowSetup);
            continueButton = MenuButton(col, "CONTINUAR", Theme.Panel, Theme.Ink, () => { if (!G.ContinueGame()) Toast("No hay una partida guardada válida.", NoticeKind.Warning); });
            MenuButton(col, "CIENCIA REAL", Theme.Panel, Theme.Ink, () => ShowScienceScreen(null));
            MenuButton(col, "OPCIONES", Theme.Panel, Theme.Ink, ShowOptions);
            MenuButton(col, "CRÉDITOS", Theme.Panel, Theme.Ink, () => ShowOnly(creditsScreen));
            return scr;
        }

        Button MenuButton(Transform parent, string label, Color bg, Color fg, UnityEngine.Events.UnityAction onClick)
        {
            var b = UIFactory.Button(parent, label, label, bg, fg, 32, onClick);
            UIFactory.Size(b, 88);
            return b;
        }

        public void ShowMenu()
        {
            CloseAllModals();
            ShowOnly(menuScreen);
            continueButton.interactable = G.saves.HasSave;
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
            var card = UIFactory.Panel(scr, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240, 900));
            var t = UIFactory.Text(card.transform, "Titulo", "NUEVA MISIÓN", 52, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1100, 70), new Vector2(0.5f, 1));

            var body = UIFactory.Rect(card.transform, "Cuerpo").Stretch(60, 140, 60, 120);
            UIFactory.VStack(body.gameObject, 14);

            Label(body, "Nombre de la base");
            baseNameInput = UIFactory.Input(body, "NombreBase", "Base Shackleton", "Escribe un nombre...");
            UIFactory.Size(baseNameInput, 64);

            Label(body, "Número de astronautas (cada uno consume 1 oxígeno y 1 comida por día)");
            var crewRow = UIFactory.Rect(body, "Tripulacion");
            UIFactory.HStack(crewRow.gameObject, 14);
            UIFactory.Size(crewRow, 64);
            foreach (int n in new[] { 2, 3, 4 })
            {
                int v = n;
                crewButtons.Add(UIFactory.Button(crewRow, "N" + n, n + (n == 3 ? " (normal)" : ""), Theme.PanelSoft, Theme.Ink, 26, () => { setupAstronauts = v; RefreshSetup(); }));
            }

            Label(body, "Nombres de los astronautas");
            var namesRow = UIFactory.Rect(body, "Nombres");
            UIFactory.HStack(namesRow.gameObject, 14);
            UIFactory.Size(namesRow, 64);
            for (int i = 0; i < 4; i++) nameInputs.Add(UIFactory.Input(namesRow, "Astronauta" + i, DefaultContent.DefaultAstronauts[i], "Nombre"));

            Label(body, "Duración de la misión (un ciclo = 4 días de gestión)");
            var lenRow = UIFactory.Rect(body, "Duracion");
            UIFactory.HStack(lenRow.gameObject, 14);
            UIFactory.Size(lenRow, 64);
            foreach (var (cycles, label) in new[] { (4, "Corta (fácil): 4 ciclos"), (6, "Normal: 6 ciclos"), (8, "Larga: 8 ciclos") })
            {
                int v = cycles;
                lengthButtons.Add(UIFactory.Button(lenRow, label, label, Theme.PanelSoft, Theme.Ink, 24, () => { setupCycles = v; RefreshSetup(); }));
            }

            tutorialToggle = UIFactory.Button(body, "Tutorial", "", Theme.PanelSoft, Theme.Ink, 24, () => { setupTutorial = !setupTutorial; RefreshSetup(); });
            UIFactory.Size(tutorialToggle, 60);

            var note = UIFactory.Text(body, "Nota", "Valores iniciales del documento: Energía 100, Oxígeno 70, Comida 60, Salud 100. Agua 50 y Vivienda 70 [JUEGO].", 18, Theme.InkSoft);
            UIFactory.Size(note, 50);

            var back = UIFactory.Button(card.transform, "Volver", "VOLVER", Theme.PanelSoft, Theme.Ink, 28, ShowMenu);
            ((RectTransform)back.transform).Place(new Vector2(0, 0), new Vector2(60, 30), new Vector2(260, 76), Vector2.zero);
            var start = UIFactory.Button(card.transform, "Empezar", "¡EMPEZAR MISIÓN!", Theme.Accent, Color.white, 32, StartMission);
            ((RectTransform)start.transform).Place(new Vector2(1, 0), new Vector2(-60, 30), new Vector2(460, 76), new Vector2(1, 0));
            return scr;
        }

        static void Label(Transform parent, string text)
        {
            var l = UIFactory.Text(parent, "Etiqueta", text, 22, Theme.InkSoft, TextAlignmentOptions.Left, FontStyles.Bold);
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
            tutorialToggle.Label().text = setupTutorial ? "Tutorial del director: SÍ (toca para omitirlo)" : "Tutorial del director: NO (toca para activarlo)";
        }

        static void Select(Button b, bool on)
        {
            b.GetComponent<Image>().color = on ? Theme.Accent : Theme.PanelSoft;
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
            var card = UIFactory.Panel(scr, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 760));
            var t = UIFactory.Text(card.transform, "Titulo", "OPCIONES", 52, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1000, 70), new Vector2(0.5f, 1));
            var body = UIFactory.Rect(card.transform, "Cuerpo").Stretch(60, 140, 60, 120);
            UIFactory.VStack(body.gameObject, 18);

            liveToggle = UIFactory.Button(body, "Vivo", "", Theme.PanelSoft, Theme.Ink, 26, () =>
            {
                G.saves.UseLiveData = !G.saves.UseLiveData;
                G.nasa.Initialize(G.saves.UseLiveData);
                RefreshOptions();
            });
            UIFactory.Size(liveToggle, 70);
            optionsNasa = UIFactory.Text(body, "Estado", "", 22, Theme.InkSoft);
            UIFactory.Size(optionsNasa, 110);
            var retry = UIFactory.Button(body, "Reintentar", "Volver a consultar NASA DONKI", Theme.PanelSoft, Theme.Ink, 24, () => G.nasa.Initialize(G.saves.UseLiveData));
            UIFactory.Size(retry, 64);
            var tut = UIFactory.Button(body, "Tutorial", "", Theme.PanelSoft, Theme.Ink, 24, null);
            tut.onClick.AddListener(() => { G.saves.ShowTutorial = !G.saves.ShowTutorial; RefreshOptions(); });
            UIFactory.Size(tut, 64);
            var del = UIFactory.Button(body, "Borrar", "Borrar la partida guardada", new Color(1f, 0.9f, 0.9f), Theme.Bad, 24, () =>
            {
                G.saves.Delete();
                Toast("Partida guardada borrada.", NoticeKind.Info);
            });
            UIFactory.Size(del, 64);

            var back = UIFactory.Button(card.transform, "Volver", "VOLVER", Theme.Accent, Color.white, 28, ShowMenu);
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(300, 76), new Vector2(0.5f, 0));
            G.nasa.StatusChanged += RefreshOptions;
            tutToggleRef = tut;
            return scr;
        }

        Button tutToggleRef;

        void ShowOptions()
        {
            ShowOnly(optionsScreen);
            RefreshOptions();
        }

        void RefreshOptions()
        {
            if (liveToggle == null) return;
            liveToggle.Label().text = G.saves.UseLiveData ? "Datos de NASA en vivo: ACTIVADOS" : "Datos de NASA en vivo: DESACTIVADOS (usar archivo)";
            optionsNasa.text = G.nasa.StatusText + "\nEndpoint: " + G.nasa.baseUrl + "FLR (ventanas de hasta 30 días, sin clave).";
            tutToggleRef.Label().text = G.saves.ShowTutorial ? "Tutorial al empezar: SÍ" : "Tutorial al empezar: NO";
        }

        // ------------------------------------------------------------------ Créditos

        RectTransform BuildCredits()
        {
            var scr = UIFactory.Rect(screensLayer, "Creditos").Stretch();
            var card = UIFactory.Panel(scr, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 860));
            var t = UIFactory.Text(card.transform, "Titulo", "CRÉDITOS", 52, Theme.Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1000, 70), new Vector2(0.5f, 1));
            var text = UIFactory.Text(card.transform, "Texto",
                "<b>CRATER</b> - juego educativo sobre la administración de una base lunar.\n" +
                "Proyecto para NASA Space Apps Challenge 2026.\n\n" +
                "<b>Datos y fuentes científicas</b>\n" +
                "NASA DONKI - Community Coordinated Modeling Center (CCMC): actividad solar.\n" +
                "NASA RadLab: mediciones de radiación (LND en la superficie lunar, CRaTER en órbita).\n" +
                "Estudios de NASA sobre radiación y blindaje (agua, regolito, concepto ARMOR).\n" +
                "Estudios de NASA sobre habitabilidad y salud de astronautas: análogo HERA y misiones de la ISS.\n" +
                "NASA Lunar Reconnaissance Orbiter (eclipse de marzo de 2025) y NASA Moon Trek (terreno lunar).\n" +
                "Canadian Space Agency: Deep Space Food Challenge y sistema CANGrow.\n\n" +
                "NASA y la CSA no respaldan este juego. Más detalles en CIENCIA REAL (menú principal).\n\n" +
                "<b>Tecnología</b>\nUnity, TextMeshPro. Fuente Liberation Sans (licencia SIL OFL).",
                26, Theme.Ink);
            text.rectTransform.Stretch(70, 130, 70, 120);
            var back = UIFactory.Button(card.transform, "Volver", "VOLVER", Theme.Accent, Color.white, 28, ShowMenu);
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(300, 76), new Vector2(0.5f, 0));
            return scr;
        }

        // ------------------------------------------------------------------ Ciencia real

        void ShowScienceScreen(string id)
        {
            ShowOnly(scienceScreen);
            UIFactory.Clear(scienceScreen);
            var card = UIFactory.Panel(scienceScreen, "Tarjeta", Theme.Panel);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1700, 960));
            var t = UIFactory.Text(card.transform, "Titulo", "CIENCIA REAL", 48, Theme.Ink, TextAlignmentOptions.Left, FontStyles.Bold);
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(50, -24), new Vector2(800, 64), new Vector2(0, 1));
            var area = UIFactory.Rect(card.transform, "Area").Stretch(40, 120, 40, 100);
            BuildScienceBrowser(area, id);
            var back = UIFactory.Button(card.transform, "Volver", "VOLVER", Theme.Accent, Color.white, 28, () =>
            {
                ShowMenu();
            });
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(300, 72), new Vector2(0.5f, 0));
        }

        /// <summary>Lista de fichas a la izquierda y detalle a la derecha. Se usa en pantalla completa y en ventana.</summary>
        void BuildScienceBrowser(RectTransform area, string selectId)
        {
            var list = UIFactory.Scroll(area, "Lista", 8, 6);
            var listRoot = (RectTransform)list.parent.parent;
            listRoot.anchorMin = new Vector2(0, 0); listRoot.anchorMax = new Vector2(0.3f, 1);
            listRoot.offsetMin = Vector2.zero; listRoot.offsetMax = new Vector2(-10, 0);

            var detail = UIFactory.Scroll(area, "Detalle", 16, 16);
            var detailRoot = (RectTransform)detail.parent.parent;
            detailRoot.anchorMin = new Vector2(0.3f, 0); detailRoot.anchorMax = new Vector2(1, 1);
            detailRoot.offsetMin = new Vector2(10, 0); detailRoot.offsetMax = Vector2.zero;

            void ShowEntry(ScienceData d)
            {
                UIFactory.Clear(detail);
                if (d == null)
                {
                    Para(detail, "<b>Valores del juego</b>", 34, Theme.Ink);
                    Para(detail, "Para que el juego sea divertido, algunas cifras las inventamos nosotros. Siempre las marcamos así:", 24, Theme.Ink);
                    TagLine(detail, DataTag.Real, "dato respaldado por NASA, la CSA o una publicación científica citada.");
                    TagLine(detail, DataTag.Juego, "cifra creada para equilibrar la experiencia.");
                    TagLine(detail, DataTag.Pendiente, "dato que todavía requiere verificación.");
                    Para(detail, ScienceInfoManager.GameValuesSummary(G.Config), 22, Theme.Ink);
                    Para(detail, "<b>Estado de los datos de NASA</b>\n" + G.nasa.StatusText, 22, Theme.InkSoft);
                    return;
                }
                Para(detail, "<b>" + d.title + "</b>", 36, Theme.Ink);
                Para(detail, d.explanation, 26, Theme.Ink);
                if (!string.IsNullOrEmpty(d.fact)) Box(detail, DefaultContent.TagLabel(d.factTag) + " " + d.fact, DefaultContent.TagColor(d.factTag));
                if (!string.IsNullOrEmpty(d.source)) Para(detail, "<b>Fuente:</b> " + d.source, 20, Theme.InkSoft);
                if (!string.IsNullOrEmpty(d.gameNote)) Box(detail, DefaultContent.TagLabel(DataTag.Juego) + " " + d.gameNote, DefaultContent.TagColor(DataTag.Juego));
            }

            ScienceData first = null;
            foreach (var d in G.science.All)
            {
                var entry = d;
                var b = UIFactory.Button(list, d.id, d.title, Color.white, Theme.Ink, 22, () => ShowEntry(entry));
                b.Label().alignment = TextAlignmentOptions.Left;
                UIFactory.Size(b, 64);
                if (d.id == selectId) first = d;
            }
            var gv = UIFactory.Button(list, "valores", "Valores del juego y etiquetas", Theme.PanelSoft, Theme.Accent, 22, () => ShowEntry(null));
            gv.Label().alignment = TextAlignmentOptions.Left;
            UIFactory.Size(gv, 64);
            ShowEntry(first ?? (string.IsNullOrEmpty(selectId) ? G.science.All.FirstOrDefault() : null));
        }

        static TextMeshProUGUI Para(Transform parent, string text, float size, Color color)
        {
            var t = UIFactory.Text(parent, "Parrafo", text, size, color);
            t.gameObject.AddComponent<LayoutElement>();
            return t;
        }

        static void Box(Transform parent, string text, Color tagColor)
        {
            var box = UIFactory.Panel(parent, "Caja", new Color(tagColor.r, tagColor.g, tagColor.b, 0.14f), false);
            UIFactory.VStack(box.gameObject, 0, 16);
            var t = UIFactory.Text(box.transform, "Texto", text, 22, Theme.Ink);
            t.gameObject.AddComponent<LayoutElement>();
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

            var title = UIFactory.Text(card.transform, "Titulo", win ? "¡MISIÓN COMPLETADA!" : "MISIÓN FALLIDA", 76, win ? Theme.Good : Theme.Bad, TextAlignmentOptions.Center, FontStyles.Bold);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -230), new Vector2(1000, 96), new Vector2(0.5f, 1));

            string crew = s.astronautNames != null && s.astronautNames.Count > 0 ? string.Join(", ", s.astronautNames) : "La tripulación";
            string body = win
                ? "Días sobrevividos: " + survived + " de " + s.totalTurns + "\n" + crew + (s.health >= 70 ? " volvieron sanos y felices." : s.health >= 35 ? " volvieron cansados, pero a salvo." : " volvieron a salvo... ¡por muy poco!")
                : (string.IsNullOrEmpty(s.defeatCause) ? "La salud de la tripulación llegó a cero." : s.defeatCause) + "\nDías sobrevividos: " + survived + " de " + s.totalTurns;
            var text = UIFactory.Text(card.transform, "Texto", body, 36, Theme.Ink, TextAlignmentOptions.Center);
            text.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -340), new Vector2(980, 200), new Vector2(0.5f, 1));

            var menu = UIFactory.Button(card.transform, "Menu", "MENÚ PRINCIPAL", Theme.PanelSoft, Theme.Ink, 30, () => { G.QuitToMenu(); ShowMenu(); });
            ((RectTransform)menu.transform).Place(new Vector2(0.5f, 0), new Vector2(-240, 40), new Vector2(440, 90), new Vector2(0.5f, 0));
            var again = UIFactory.Button(card.transform, "Otra", win ? "JUGAR OTRA VEZ" : "INTENTAR DE NUEVO", Theme.Accent, Color.white, 30, () => { G.QuitToMenu(); ShowSetup(); });
            ((RectTransform)again.transform).Place(new Vector2(0.5f, 0), new Vector2(240, 40), new Vector2(440, 90), new Vector2(0.5f, 0));
        }
    }
}
