using System;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Data
{
    /// <summary>Ficha de divulgación científica con su etiqueta REAL / JUEGO / PENDIENTE.</summary>
    [CreateAssetMenu(fileName = "Science", menuName = "CRATER/Ciencia")]
    public class ScienceData : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea(2, 6)] public string explanation;
        [TextArea(2, 6)] public string fact;
        public DataTag factTag = DataTag.Real;
        public string source;
        [TextArea(2, 6)] public string gameNote;
        public Sprite illustration;
    }
}
