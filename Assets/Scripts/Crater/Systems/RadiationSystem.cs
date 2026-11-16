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


        public static string DefenseName(RadiationDefense d)
        {
            switch (d)
            {
                case RadiationDefense.WaterWalls: return Loc.T("EVENT_SOLAR_OPT0_TITLE");
                case RadiationDefense.RegolithIgloo: return Loc.T("EVENT_SOLAR_OPT1_TITLE");
                case RadiationDefense.Bunker: return Loc.T("EVENT_SOLAR_OPT2_TITLE");
                default: return Loc.T("EVENT_SOLAR_OPT3_TITLE");
            }
        }
    }
}
