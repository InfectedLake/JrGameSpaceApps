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
            if (!G.IsPlaying) { reason = "La misión terminó"; return false; }
            if (G.events.HasPending) { reason = "Primero responde al evento"; return false; }
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
                    names.Add(st.name);
                }
            }
            return string.Join(", ", names);
        }

        public static string StatusLabel(BuildingVisual v)
        {
            switch (v)
            {
                case BuildingVisual.Off: return "APAGADO";
                case BuildingVisual.Working: return "FUNCIONANDO";
                case BuildingVisual.NoPower: return "SIN ENERGÍA";
                case BuildingVisual.NoWater: return "SIN AGUA";
                case BuildingVisual.Paused: return "EN PAUSA";
                case BuildingVisual.Broken: return "AVERIADO";
                default: return "INACTIVO";
            }
        }
    }
}
