using System;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    public enum PromptKind { Choice, Info, EclipseAllocation }

    /// <summary>
    /// Los textos de las ventanas son FUNCIONES que se evalúan al mostrarlas: si el jugador cambia de idioma con una
    /// ventana abierta, la interfaz la vuelve a dibujar y todos sus textos salen en el idioma nuevo, sin cerrar la decisión.
    /// </summary>
    public class PromptOption
    {
        public Func<string> label, details, costs, disabledReason;
        public bool enabled = true;
        public Action onChoose;
        /// <summary>Eventos menores: recurso que se pierde y cuánto (la interfaz muestra su icono).</summary>
        public ResourceKind? resource;
        public int cost;

        public string Label => label?.Invoke() ?? "";
        public string Details => details?.Invoke() ?? "";
        public string Costs => costs?.Invoke() ?? "";
        public string DisabledReason => disabledReason?.Invoke() ?? "";
    }

    /// <summary>
    /// Pantalla de acontecimiento que la interfaz debe mostrar. La lógica no conoce la interfaz:
    /// solo describe el texto y las opciones, y la interfaz llama al callback elegido.
    /// </summary>
    public class EventPrompt
    {
        public PromptKind kind;
        public CraterEventType? eventType;
        public int intensity;
        /// <summary>Ventana del momento en que el acontecimiento OCURRE (no un aviso previo ni un resultado).</summary>
        public bool arrival;
        public Func<string> title, subtitle, body, dataLabel;
        public DataKind dataKind = DataKind.Game;
        public Color color = Color.white;
        public Sprite image;
        public readonly List<PromptOption> options = new List<PromptOption>();

        /// <summary>Solo para PromptKind.EclipseAllocation.</summary>
        public Action<bool[]> onEclipseConfirm;

        public string Title => title?.Invoke() ?? "";
        public string Subtitle => subtitle?.Invoke() ?? "";
        public string Body => body?.Invoke() ?? "";
        public string DataLabel => dataLabel?.Invoke() ?? "";
    }
}
