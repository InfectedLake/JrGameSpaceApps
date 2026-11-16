namespace Crater.UI
{
    // Cambio de idioma en vivo.
    public partial class UIManager
    {
        /// <summary>
        /// Se llama cuando el jugador cambia de idioma (Loc.Changed). Los textos fijos se actualizan solos (LocText);
        /// aquí se vuelven a escribir los textos dinámicos y las pantallas que se arman al mostrarse.
        /// No toca el estado: no reinicia turnos, música, eventos ni recursos, no cierra decisiones pendientes y
        /// conserva los nombres que escribió el jugador.
        /// </summary>
        void OnLanguageChanged()
        {
            RefreshLanguageButtons();
            if (!built || G == null) return;

            // Los avisos temporales (4-6 s) se generaron con el idioma anterior: se retiran para no mezclar idiomas.
            for (int i = toastsRoot.childCount - 1; i >= 0; i--) Destroy(toastsRoot.GetChild(i).gameObject);

            RefreshGame();      // HUD (ciclo, pista de TERMINAR DÍA), ficha del edificio abierta
            RefreshOptions();   // botones de datos de NASA y tutorial
            if (setupScreen.gameObject.activeSelf) RefreshSetup();
            if (scienceScreen.gameObject.activeSelf) ShowScienceScreen(scienceId);
            if (endScreen.gameObject.activeSelf && G.State != null) ShowEnd(G.State.outcome);

            // Ventana de un acontecimiento abierta: se vuelve a dibujar con el MISMO EventPrompt (la decisión sigue
            // pendiente y la distribución del eclipse se conserva). Si se está presentando (notificación/fenómeno),
            // la ventana aún no existe y saldrá directamente en el idioma nuevo.
            if (eventOverlay != null && introPrompt == null && G.events.Current != null)
            {
                Destroy(eventOverlay.gameObject);
                eventOverlay = null;
                BuildPromptWindow(G.events.Current);
            }
        }
    }
}
