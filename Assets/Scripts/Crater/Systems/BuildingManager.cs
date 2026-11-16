using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Estado de los cuatro edificios: encendido/apagado, consumo, producción y averías.
    /// Encender o apagar NO toca los recursos: solo cambia el estado y pide una nueva proyección.
    /// GameObject: "BuildingManager".
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        GameManager G => GameManager.Instance;

        public BuildingData Data(BuildingId id) => G.GetBuilding(id);
        public bool IsOn(BuildingId id) => G.State != null && G.State.IsOn(id);

        public bool CanToggle(out string reason)
        {
            reason = "";
            if (!G.IsPlaying) { reason = Loc.T("REASON_MISSION_OVER"); return false; }
            if (G.events.HasPending) { reason = Loc.T("REASON_ANSWER_EVENT"); return false; }
            if (G.Paused) { reason = Loc.T("REASON_PAUSED"); return false; }
            return true;
        }

        public void Toggle(BuildingId id)
        {
            if (!CanToggle(out _)) return;
            G.State.buildingOn[(int)id] = !G.State.buildingOn[(int)id];
            G.NotifyChanged();
        }

        /// <summary>Resultado del edificio en la proyección de hoy (con energía, sin energía, etc.).</summary>
        public BuildingResult Result(BuildingId id) => G.turns.Projection()?.Get(id);

        /// <summary>Durante una emergencia se apagan las funciones no esenciales.</summary>
        public string ApplyEmergencyShutdown()
        {
            var names = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                var st = G.Config.Get((BuildingId)i);
                if (!st.essential && G.State.buildingOn[i])
                {
                    G.State.buildingOn[i] = false;
                    names.Add(Loc.Building((BuildingId)i));
                }
            }
            return string.Join(", ", names);
        }

        public static string StatusLabel(BuildingVisual v)
        {
            switch (v)
            {
                case BuildingVisual.Off: return Loc.T("STATE_OFF");
                case BuildingVisual.Working: return Loc.T("STATE_WORKING");
                case BuildingVisual.NoPower: return Loc.T("STATE_NO_POWER");
                case BuildingVisual.NoWater: return Loc.T("STATE_NO_WATER");
                case BuildingVisual.Paused: return Loc.T("STATE_PAUSED");
                case BuildingVisual.Broken: return Loc.T("STATE_BROKEN");
                default: return Loc.T("STATE_INACTIVE");
            }
        }
    }
}
