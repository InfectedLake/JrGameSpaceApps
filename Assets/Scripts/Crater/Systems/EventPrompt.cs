using System;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    public enum PromptKind { Choice, Info, EclipseAllocation }

    public class PromptOption
    {
        public string label;
        public string details;
        public string costs;
        public bool enabled = true;
        public string disabledReason;
        public Action onChoose;
        /// <summary>Eventos menores: recurso que se pierde y cuánto (la interfaz muestra su icono).</summary>
        public ResourceKind? resource;
        public int cost;
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
        public string title = "";
        public string subtitle = "";
        public string body = "";
        public string dataLabel = "";
        public DataKind dataKind = DataKind.Game;
        public Color color = Color.white;
        public Sprite image;
        public readonly List<PromptOption> options = new List<PromptOption>();

        /// <summary>Solo para PromptKind.EclipseAllocation.</summary>
        public Action<bool[]> onEclipseConfirm;
    }
}
