using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Radiación: intensidad, cálculo de protección, daño y dosis acumulada. GameObject: "RadiationSystem".
    /// </summary>
    public class RadiationSystem : MonoBehaviour
    {
        GameManager G => GameManager.Instance;

        public float PreviewBlock(RadiationDefense d, ScheduledEvent ev, out string why)
            => RadiationRules.Block(d, G.State, ev, G.Config, out why);

        public int PreviewDamage(RadiationDefense d, ScheduledEvent ev)
            => RadiationRules.Damage(ev.intensity, PreviewBlock(d, ev, out _), G.Config);

        public RadiationOutcome Resolve(ScheduledEvent ev, RadiationDefense d)
        {
            var o = RadiationRules.Resolve(G.State, ev, d, G.Config);
            G.NotifyChanged();
            return o;
        }

        public static string IntensityName(int i) => i >= 3 ? "Alta" : (i == 2 ? "Media" : "Baja");

        public static string DefenseName(RadiationDefense d)
        {
            switch (d)
            {
                case RadiationDefense.WaterWalls: return "Paredes de agua";
                case RadiationDefense.RegolithIgloo: return "Refugio de regolito";
                case RadiationDefense.Bunker: return "Refugio interior";
                default: return "Sin protección";
            }
        }
    }
}
