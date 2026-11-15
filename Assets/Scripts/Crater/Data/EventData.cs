using System;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Data
{
    [Serializable]
    public class EventOptionText
    {
        public string title;
        [TextArea(1, 3)] public string description;
        public DataTag tag = DataTag.Juego;
    }

    /// <summary>Textos y fuentes de un evento especial. Assets > Create > CRATER > Evento.</summary>
    [CreateAssetMenu(fileName = "Event", menuName = "CRATER/Evento")]
    public class EventData : ScriptableObject
    {
        public CraterEventType type;
        public string title;
        [TextArea(2, 4)] public string description;
        [TextArea(2, 4)] public string warningText;
        [TextArea(2, 4)] public string educationalMessage;
        public string scienceId;
        public string source;
        public Sprite image;
        public Color color = Color.white;
        public List<EventOptionText> options = new List<EventOptionText>();
    }
}
