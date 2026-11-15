using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Baterías de emergencia (0-4): carga, distribución durante el eclipse y uso de emergencia.
    /// No confundir con el recurso Energía. GameObject: "BatteryManager".
    /// </summary>
    public class BatteryManager : MonoBehaviour
    {
        GameManager G => GameManager.Instance;

        public int Charge => G.State.batteries;
        public int Max => G.Config.maxBatteries;

        public int SectionCost(EclipseSection s) => G.Config.eclipseSectionCost[(int)s];
        public int AllocationCost(bool[] powered) => EclipseRules.Cost(powered, G.Config);
        public bool CanAfford(bool[] powered) => EclipseRules.Affordable(powered, Charge, G.Config);

        public bool CanUseEmergency(out string reason) => BaseActions.CanUseBattery(G.State, G.Config, out reason);

        public void UseEmergency()
        {
            if (!CanUseEmergency(out _)) return;
            BaseActions.UseBattery(G.State, G.Config);
            G.Decision("Usó una batería de emergencia (+" + G.Config.emergencyBatteryEnergy + " energía)");
            G.Notify("Batería conectada: +" + G.Config.emergencyBatteryEnergy + " de energía.", NoticeKind.Good);
            G.NotifyChanged();
        }
    }
}
