using System;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Data
{
    /// <summary>Parámetros de un edificio. Assets > Create > CRATER > Edificio.</summary>
    [CreateAssetMenu(fileName = "Building", menuName = "CRATER/Edificio")]
    public class BuildingData : ScriptableObject
    {
        public BuildingStats stats = new BuildingStats();
        [TextArea(2, 4)] public string description;
        [TextArea(2, 6)] public string howItWorks;
        public string scienceId;
        public Sprite icon;
        public Color color = Color.white;
    }
}
