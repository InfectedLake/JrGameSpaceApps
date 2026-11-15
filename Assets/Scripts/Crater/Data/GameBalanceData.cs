using System;
using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Data
{
    /// <summary>Valores de balance [JUEGO]. Assets > Create > CRATER > Balance.</summary>
    [CreateAssetMenu(fileName = "GameBalance", menuName = "CRATER/Balance")]
    public class GameBalanceData : ScriptableObject
    {
        public BalanceConfig config = new BalanceConfig();
    }
}
