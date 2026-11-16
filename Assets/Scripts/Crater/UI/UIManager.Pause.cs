using System;
using System.Collections.Generic;
using Crater.Core;
using Crater.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    // Menú de pausa (botón de tres líneas), opciones de audio/pantalla, confirmaciones y música.
    public partial class UIManager
    {
        RectTransform pauseRoot, pauseMain, pauseOptions, confirmRoot;
        TextMeshProUGUI confirmTitle, confirmText;
        Button confirmYes;
        Action confirmAction;
        readonly System.Collections.Generic.List<Action> settingsSync = new System.Collections.Generic.List<Action>();

        /// <summary>Muestra en todos los paneles de opciones los valores reales (pausa y menú principal comparten AudioManager).</summary>
        void SyncSettings() { foreach (var a in settingsSync) a(); }

        // ------------------------------------------------------------------ Pausa

        /// <summary>Abre el menú de pausa sobre el gameplay (la escena sigue visible detrás).</summary>
        void OpenPause()
        {
            if (!G.InGame || pauseRoot == null) return;
            CloseBuildingCard(true);
            G.SetPaused(true);
            pauseRoot.gameObject.SetActive(true);
            pauseRoot.SetAsLastSibling();
            ShowPausePage(pauseMain);
        }

        /// <summary>Cierra el menú y reanuda. No toca las ventanas de eventos pendientes.</summary>
        void ClosePause()
        {
            if (pauseRoot != null) pauseRoot.gameObject.SetActive(false);
            if (confirmRoot != null) confirmRoot.gameObject.SetActive(false);
            G.SetPaused(false);
            G.sound.Save();
        }

        void ShowPausePage(RectTransform page)
        {
            pauseMain.gameObject.SetActive(page == pauseMain);
            pauseOptions.gameObject.SetActive(page == pauseOptions);
            if (page == pauseOptions) SyncSettings();
            confirmRoot.gameObject.SetActive(false);
        }

        void BuildPauseMenu()
        {
            // Encima de las ventanas de eventos: el fondo bloquea todos los clics del juego mientras está abierto.
            pauseRoot = UIFactory.Image(topLayer, "MenuPausa", new Color(0.0f, 0.03f, 0.10f, 0.55f), null, true).rectTransform;
            pauseRoot.Stretch();

            pauseMain = PauseCard("Principal", "PAUSE_TITLE", new Vector2(700, 660), out var body);
            PauseButton(body, "PAUSE_CONTINUE", Theme.Accent, ClosePause);
            PauseButton(body, "PAUSE_OPTIONS", Color.white, () => ShowPausePage(pauseOptions));
            PauseButton(body, "PAUSE_MAIN_MENU", Color.white, () => AskConfirm("CONFIRM_MENU_TITLE", "CONFIRM_MENU_TEXT", "CONFIRM_EXIT", ExitToMainMenu));
            PauseButton(body, "PAUSE_EXIT", Color.white, () => AskConfirm("CONFIRM_QUIT_TITLE", QuitNote(), "CONFIRM_EXIT", QuitGame));

            pauseOptions = PauseCard("Opciones", "OPTIONS_TITLE", new Vector2(900, 860), out var obody);
            BuildSettingsControls(obody, 900 - 2 * Mathf.Round(900 * 0.08f));
            var spacer = UIFactory.Rect(obody, "Espacio");
            UIFactory.Size(spacer, 0).flexibleHeight = 1;
            PauseButton(obody, "BACK", Theme.Accent, () => { G.sound.Save(); ShowPausePage(pauseMain); });

            BuildConfirm(pauseRoot);
            pauseRoot.gameObject.SetActive(false);
        }

        RectTransform PauseCard(string name, string titleKey, Vector2 size, out RectTransform body)
        {
            var card = UIFactory.Panel(pauseRoot, name, Color.white);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, size);
            float padX = Mathf.Round(size.x * 0.08f);
            body = UIFactory.Rect(card.transform, "Contenido").Stretch(padX, 40, padX, 30 + UIFactory.TopInset);
            var v = UIFactory.VStack(body.gameObject, 18);
            v.childForceExpandHeight = false;
            var t = UIFactory.Text(body, "Titulo", "", 60, Brown, TextAlignmentOptions.Center, FontStyles.Bold).L(titleKey);
            t.enableAutoSizing = true; t.fontSizeMin = 40; t.fontSizeMax = 60;
            UIFactory.Size(t, 80, -1, -1, 80);
            return (RectTransform)card.transform;
        }

        static Button PauseButton(Transform parent, string key, Color bg, UnityEngine.Events.UnityAction onClick)
        {
            var b = UIFactory.Button(parent, key, "", bg, Brown, 34, onClick).L(key);
            UIFactory.Size(b, 104, -1, -1, 104);
            return b;
        }

        /// <summary>Controles reales: volumen general, música, efectos y pantalla completa (se guardan al momento).</summary>
        void BuildSettingsControls(Transform parent, float width)
        {
            SliderRow(parent, "Master", "OPTIONS_MASTER", () => G.sound.Master, v => G.sound.Master = v);
            SliderRow(parent, "Music", "OPTIONS_MUSIC", () => G.sound.Music, v => G.sound.Music = v);
            SliderRow(parent, "Effects", "OPTIONS_SFX", () => G.sound.Effects, v => G.sound.Effects = v);

            Button full = null;
            bool fullscreen = G.sound.Fullscreen;
            full = UIFactory.Button(parent, "PantallaCompleta", "", Color.white, Brown, 30, () =>
            {
                // Screen.fullScreen se aplica en el siguiente frame: se muestra el valor pedido.
                fullscreen = !fullscreen;
                G.sound.Fullscreen = fullscreen;
                full.Label().text = FullscreenLabel(fullscreen);
            });
            UIFactory.Size(full, 96, -1, -1, 96);
            // La etiqueta se reescribe sola al cambiar el idioma (y al alternar, arriba).
            full.L(() => AudioManager.FullscreenSupported ? FullscreenLabel(fullscreen) : Loc.T("OPTIONS_FULLSCREEN_UNAVAILABLE"));
            settingsSync.Add(() => { fullscreen = G.sound.Fullscreen; if (AudioManager.FullscreenSupported) full.Label().text = FullscreenLabel(fullscreen); });
            if (!AudioManager.FullscreenSupported) full.interactable = false;
            LanguageRow(parent);
        }

        static string FullscreenLabel(bool on) => Loc.T("OPTIONS_FULLSCREEN", Loc.T(on ? "OPTIONS_ON" : "OPTIONS_OFF"));

        // ------------------------------------------------------------------ Idioma

        readonly List<(Button en, Button es)> languageButtons = new List<(Button en, Button es)>();

        /// <summary>
        /// Selector ENGLISH | ESPAÑOL (sin banderas: es un idioma, no un país). Cada opción se escribe en su propio
        /// idioma para que siempre se reconozca. El cambio es inmediato y se guarda como preferencia.
        /// </summary>
        RectTransform LanguageButtons(Transform parent, float height, float fontSize)
        {
            var pair = UIFactory.Rect(parent, "Idiomas");
            var h = UIFactory.HStack(pair.gameObject, 14);
            h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            var en = UIFactory.Button(pair, "English", "ENGLISH", Theme.PanelSoft, Theme.Ink, fontSize, () => LanguageSettings.Set(Language.English));
            var es = UIFactory.Button(pair, "Espanol", "ESPAÑOL", Theme.PanelSoft, Theme.Ink, fontSize, () => LanguageSettings.Set(Language.Spanish));
            languageButtons.Add((en, es));
            RefreshLanguageButtons();
            return pair;
        }

        void RefreshLanguageButtons()
        {
            foreach (var (en, es) in languageButtons)
            {
                if (en == null || es == null) continue;
                Select(en, !Loc.Spanish);
                Select(es, Loc.Spanish);
            }
        }

        /// <summary>Fila "Idioma" de Opciones (menú principal y pausa): etiqueta | ENGLISH | ESPAÑOL.</summary>
        void LanguageRow(Transform parent)
        {
            var row = UIFactory.Rect(parent, "LanguageRow");
            UIFactory.Size(row, 86, 0, -1, 86);
            var h = UIFactory.HStack(row.gameObject, 22);
            h.childForceExpandWidth = false; h.childForceExpandHeight = true;
            h.childAlignment = TextAnchor.MiddleLeft;
            var t = UIFactory.Text(row, "Label", "", 28, Brown, TextAlignmentOptions.MidlineLeft, FontStyles.Bold).L("OPTIONS_LANGUAGE");
            t.enableAutoSizing = true; t.fontSizeMin = 20; t.fontSizeMax = 28;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            var tl = UIFactory.Size(t, 86, 250, 0); tl.minWidth = 250;
            var pair = LanguageButtons(row, 86, 26);
            var pl = UIFactory.Size(pair, 86, 300, 1); pl.minWidth = 300;
        }

        void SliderRow(Transform parent, string name, string labelKey, Func<float> get, Action<float> onChange)
        {
            float value = get();
            // SoundRow → Label | SliderContainer (Background, FillArea/Fill, HandleSlideArea/Handle) | PercentageLabel.
            // Todos los anchos se fijan con LayoutElement (el texto no decide el reparto): la etiqueta y el
            // porcentaje tienen ancho fijo y el slider ocupa todo el espacio restante (mínimo 220 px).
            var row = UIFactory.Rect(parent, "SoundRow_" + name);
            UIFactory.Size(row, 70, 0, -1, 70);
            var h = UIFactory.HStack(row.gameObject, 22);
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = true;
            h.childAlignment = TextAnchor.MiddleLeft;
            var t = UIFactory.Text(row, "Label", "", 28, Brown, TextAlignmentOptions.MidlineLeft, FontStyles.Bold).L(labelKey);
            t.enableAutoSizing = true; t.fontSizeMin = 20; t.fontSizeMax = 28;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            var tl = UIFactory.Size(t, 70, 250, 0); tl.minWidth = 250;
            var pct = UIFactory.Text(row, "PercentageLabel", Mathf.RoundToInt(value * 100) + "%", 26, Brown, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
            pct.textWrappingMode = TextWrappingModes.NoWrap;
            var s = UIFactory.Slider(row, "SliderContainer", value, v => { onChange(v); pct.text = Mathf.RoundToInt(v * 100) + "%"; });
            var sl = UIFactory.Size(s, 70, 220, 1); sl.minWidth = 220;
            // Al reabrir Opciones se muestra el valor guardado SIN disparar OnValueChanged (no vuelve a guardar ni aplicar).
            settingsSync.Add(() => { float v = get(); s.SetValueWithoutNotify(v); pct.text = Mathf.RoundToInt(v * 100) + "%"; });
            pct.transform.SetAsLastSibling();
            var pl = UIFactory.Size(pct, 70, 86, 0); pl.minWidth = 86;
        }

        // ------------------------------------------------------------------ Confirmaciones

        void BuildConfirm(Transform parent)
        {
            confirmRoot = UIFactory.Image(parent, "Confirmacion", new Color(0, 0.02f, 0.08f, 0.45f), null, true).rectTransform;
            confirmRoot.Stretch();
            var card = UIFactory.Panel(confirmRoot, "Tarjeta", Color.white);
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 520));
            var body = UIFactory.Rect(card.transform, "Contenido").Stretch(70, 40, 70, 30 + UIFactory.TopInset);
            var v = UIFactory.VStack(body.gameObject, 16);
            v.childForceExpandHeight = false;
            confirmTitle = UIFactory.Text(body, "Titulo", "", 44, Brown, TextAlignmentOptions.Center, FontStyles.Bold);
            confirmTitle.enableAutoSizing = true; confirmTitle.fontSizeMin = 30; confirmTitle.fontSizeMax = 44;
            UIFactory.Size(confirmTitle, 70, -1, -1, 70);
            confirmText = UIFactory.Text(body, "Texto", "", 26, Theme.Ink, TextAlignmentOptions.Center);
            confirmText.enableAutoSizing = true; confirmText.fontSizeMin = 20; confirmText.fontSizeMax = 26;
            UIFactory.Size(confirmText, 150, -1, -1, 150);
            var spacer = UIFactory.Rect(body, "Espacio");
            UIFactory.Size(spacer, 0).flexibleHeight = 1;
            var row = UIFactory.Rect(body, "Botones");
            UIFactory.Size(row, 100, -1, -1, 100);
            UIFactory.HStack(row.gameObject, 24);
            UIFactory.Button(row, "Cancelar", "", Color.white, Brown, 32, () => confirmRoot.gameObject.SetActive(false)).L("CANCEL");
            confirmYes = UIFactory.Button(row, "Si", "", Theme.Bad, Brown, 32, () =>
            {
                confirmRoot.gameObject.SetActive(false);
                var a = confirmAction; confirmAction = null;
                a?.Invoke();
            });
            confirmRoot.gameObject.SetActive(false);
        }

        /// <summary>Confirmación con claves de localización (si cambia el idioma con la ventana abierta, se reescribe).</summary>
        void AskConfirm(string titleKey, string textKey, string yesKey, Action onYes)
        {
            confirmTitle.L(titleKey);
            confirmText.L(textKey);
            confirmYes.L(yesKey);
            confirmAction = onYes;
            confirmRoot.gameObject.SetActive(true);
            confirmRoot.SetAsLastSibling();
        }

        void ExitToMainMenu()
        {
            ClosePause();
            CloseAllModals();
            G.QuitToMenu();   // respeta el guardado automático existente y limpia eventos pendientes
            ShowMenu();
        }

        static string QuitNote()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return "CONFIRM_QUIT_TEXT_WEB";
#else
            return "CONFIRM_QUIT_TEXT";
#endif
        }

        void QuitGame()
        {
            if (G.IsPlaying) G.saves.Save();
            G.sound.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;   // Application.Quit no cierra el Editor
#elif UNITY_WEBGL
            ClosePause();
            G.QuitToMenu();
            ShowMenu();
            Toast(Loc.T("NOTICE_CLOSE_TAB"), NoticeKind.Info);
#else
            Application.Quit();
#endif
        }

        // ------------------------------------------------------------------ Música

        /// <summary>
        /// Música según la pantalla y la FASE del acontecimiento (EventScheduler.MusicEvent), no según las ventanas
        /// abiertas. Se llama en cada refresco, pero las solicitudes al AudioManager son idempotentes: abrir paneles,
        /// seleccionar edificios, cerrar notificaciones o pasar de día nunca reinicia ni detiene una pista.
        ///  - Fuera de la partida (menú, opciones, ciencia, final): música del menú. Se detiene el efecto de evento.
        ///  - Un acontecimiento en Alerta, Preparación, Activo o Resolviendo: su música (BeginEventMusic con su Uid).
        ///    Si llegó SIN aviso, empieza con la notificación inicial (EventIntro, etapa 1).
        ///  - Ninguno (eventos menores y emergencias incluidos): música de gameplay (EndEventMusic, fundido largo).
        /// </summary>
        void UpdateMusic()
        {
            if (G == null || G.sound == null) return;
            if (!G.InGame || G.State == null || !gameScreen.gameObject.activeSelf)
            {
                G.sound.StopEventSfx();
                G.sound.SetMusic(MusicTrack.Menu);
                return;
            }
            var ev = EventScheduler.MusicEvent(G.State, G.Config);
            if (ev == null) { G.sound.EndEventMusic(); return; }
            // Llegada sin aviso: la música espera a la notificación inicial (si ya sonaba por la alerta, sigue).
            bool waitNotice = introPrompt != null && introStage < 1 && G.sound.ActiveEventMusic == null;
            if (waitNotice) { G.sound.SetMusic(MusicTrack.Game); return; }
            G.sound.BeginEventMusic(ev.Uid);
        }
    }
}
