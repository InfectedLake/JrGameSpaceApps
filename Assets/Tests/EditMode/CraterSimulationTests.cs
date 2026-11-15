// Pruebas de la sección 20 del documento de diseño.
// Se ejecutan en Unity: Window > General > Test Runner > EditMode.
using System.Linq;
using Crater.Core;
using NUnit.Framework;

namespace Crater.Tests
{
    public class CraterSimulationTests
    {
        static BalanceConfig Cfg() => new BalanceConfig();
        static GameState NewState(BalanceConfig c) => GameState.CreateNew(c, 1234);

        // ---------------- Prueba 1: funcionamiento normal ----------------
        [Test]
        public void P1_EstadoInicial_ProduceValoresDelDocumento()
        {
            var c = Cfg(); var s = NewState(c);
            Assert.AreEqual(100, s.energy); Assert.AreEqual(70, s.oxygen); Assert.AreEqual(60, s.food);
            Assert.AreEqual(100, s.health); Assert.AreEqual(3, s.astronauts);
            Assert.IsTrue(s.IsOn(BuildingId.Solar)); Assert.IsTrue(s.IsOn(BuildingId.Greenhouse));
            Assert.IsTrue(s.IsOn(BuildingId.Recycler)); Assert.IsTrue(s.IsOn(BuildingId.Habitat));

            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(84, r.after.energy);           // rebalance [JUEGO]: todo encendido = −16
            Assert.AreEqual(76, r.after.oxygen);
            Assert.AreEqual(62, r.after.food);
            Assert.AreEqual(-16, r.energy.Delta);
            Assert.AreEqual(6, r.oxygen.Delta);
            Assert.AreEqual(2, r.food.Delta);
            Assert.AreEqual("100 + 23 − 22 − 12 − 5", r.energy.Formula());
            // La simulación no modifica el estado original.
            Assert.AreEqual(100, s.energy); Assert.AreEqual(1, s.day);
        }

        // ---------------- Prueba 2: edificios apagados ----------------
        [Test]
        public void P2_ApagarReciclador_ActualizaProyeccion()
        {
            var c = Cfg(); var s = NewState(c);
            s.buildingOn[(int)BuildingId.Recycler] = false;
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(-4, r.oxygen.Delta);          // +6 pasa a −4
            Assert.AreEqual(6, r.energy.GrossDelta);      // la energía pasa a +6
            Assert.AreEqual(100, r.after.energy);         // con límite 100 (se desperdician 6)
            Assert.AreEqual(6, r.energy.Wasted);
            Assert.AreEqual(0, r.Get(BuildingId.Recycler).energyUsed);
            Assert.AreEqual(BuildingVisual.Off, r.Get(BuildingId.Recycler).visual);
        }

        // ---------------- Prueba 3: falta de energía ----------------
        [Test]
        public void P3_SinEnergia_EdificioEncendidoNoFunciona()
        {
            var c = Cfg(); var s = NewState(c);
            s.energy = 0;
            var r = TurnSimulator.Simulate(s, c);
            // 0 + 23 = 23: reciclador (22) sí; invernadero (12 > 1) no; vivienda (5 > 1) no.
            Assert.AreEqual(BuildingVisual.Working, r.Get(BuildingId.Recycler).visual);
            Assert.AreEqual(BuildingVisual.NoPower, r.Get(BuildingId.Greenhouse).visual);
            Assert.AreEqual(BuildingVisual.NoPower, r.Get(BuildingId.Habitat).visual);
            Assert.AreEqual(1, r.after.energy);
            Assert.IsTrue(r.health.labels.Contains("vivienda apagada")); // vivienda sin energía: Salud −3
            Assert.IsTrue(r.Get(BuildingId.Greenhouse).on);
        }

        // ---------------- Prueba 4: falta de agua ----------------
        [Test]
        public void P4_SinAgua_RecicladorNoProduceOxigeno()
        {
            var c = Cfg(); var s = NewState(c);
            s.water = 0;
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(BuildingVisual.NoWater, r.Get(BuildingId.Recycler).visual);
            Assert.AreEqual(0, r.Get(BuildingId.Recycler).oxygenMade);
        }

        [Test]
        public void P4_AguaMenorA20_ReduceCultivos30()
        {
            var c = Cfg(); var s = NewState(c);
            s.water = 19;
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(6, r.Get(BuildingId.Greenhouse).foodMade); // 8 × 0.7 = 5.6 → 6
            s.water = 20;
            Assert.AreEqual(8, TurnSimulator.Simulate(s, c).Get(BuildingId.Greenhouse).foodMade);
        }

        // ---------------- Prueba 5: tormenta solar ----------------
        static ScheduledEvent Storm(int intensity, bool warned, RadiationDefense choice, int choiceDay)
            => new ScheduledEvent { type = CraterEventType.SolarRadiation, day = 10, intensity = intensity, warned = warned, warningProcessed = true, choice = (int)choice, choiceDay = choiceDay };

        [Test]
        public void P5_ParedesDeAgua_ConAviso80_SinAviso20()
        {
            var c = Cfg();
            var s = NewState(c); s.day = 10;
            var o = RadiationRules.Resolve(s, Storm(3, true, RadiationDefense.WaterWalls, 8), RadiationDefense.WaterWalls, c);
            Assert.AreEqual(0.80f, o.block, 1e-4);
            Assert.AreEqual(7, o.damage);                 // 3 × 12 × 0.2 = 7.2
            Assert.AreEqual(20, s.water);                 // 50 − 30
            Assert.IsTrue(s.today.foodPaused);
            Assert.AreEqual(0.5f, s.today.oxygenFactor, 1e-4);
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(0, r.Get(BuildingId.Greenhouse).foodMade);
            Assert.AreEqual(5, r.Get(BuildingId.Recycler).oxygenMade);

            var s2 = NewState(c); s2.day = 10;
            var o2 = RadiationRules.Resolve(s2, Storm(3, false, RadiationDefense.None, -1), RadiationDefense.WaterWalls, c);
            Assert.AreEqual(0.20f, o2.block, 1e-4);
            Assert.AreEqual(29, o2.damage);               // 3 × 12 × 0.8 = 28.8
        }

        [Test]
        public void P5_Regolito_Terminado70_Incompleto_Aumenta15()
        {
            var c = Cfg();
            var s = NewState(c); s.day = 8;
            Assert.IsTrue(RadiationRules.CanChoose(RadiationDefense.RegolithIgloo, s, c, true, out _));
            RadiationRules.ApplyChoiceCosts(RadiationDefense.RegolithIgloo, s, c);
            Assert.AreEqual(75, s.energy);
            Assert.AreEqual(2, s.roverBusyTurns);
            var r1 = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(0, r1.iceCollected);          // rover ocupado: sin hielo
            var r2 = TurnSimulator.Simulate(r1.after, c);
            Assert.IsTrue(r2.regolithCompleted);
            Assert.IsTrue(r2.habitat.parts.Contains(10));  // Vivienda +10 por aislamiento
            var s3 = r2.after; s3.day = 10;
            var o = RadiationRules.Resolve(s3, Storm(2, true, RadiationDefense.RegolithIgloo, 8), RadiationDefense.RegolithIgloo, c);
            Assert.AreEqual(0.70f, o.block, 1e-4);
            Assert.AreEqual(7, o.damage);                 // 2 × 12 × 0.3 = 7.2

            var s4 = NewState(c); s4.day = 10;
            var thin = RadiationRules.Resolve(s4, Storm(2, false, RadiationDefense.None, -1), RadiationDefense.RegolithIgloo, c);
            Assert.AreEqual(-0.15f, thin.block, 1e-4);
            Assert.AreEqual(28, thin.damage);             // 2 × 12 × 1.15 = 27.6
        }

        [Test]
        public void P5_Bunker50_YSinDefensa()
        {
            var c = Cfg();
            var s = NewState(c); s.day = 10;
            var o = RadiationRules.Resolve(s, Storm(1, false, RadiationDefense.None, -1), RadiationDefense.Bunker, c);
            Assert.AreEqual(0.5f, o.block, 1e-4);
            Assert.AreEqual(6, o.damage);
            Assert.AreEqual(70 - 15 - 6, s.habitat);
            Assert.AreEqual(62, s.oxygen); Assert.AreEqual(42, s.water); Assert.AreEqual(52, s.food);
            Assert.IsTrue(s.today.productionPaused);

            var s2 = NewState(c); s2.day = 10;
            var none = RadiationRules.Resolve(s2, Storm(3, false, RadiationDefense.None, -1), RadiationDefense.None, c);
            Assert.AreEqual(36, none.damage);
            Assert.AreEqual(34, s2.habitat);
            Assert.IsTrue(none.electronicsDamaged);
            Assert.AreEqual(50, s2.panels.capacity);
            Assert.IsTrue(s2.stormDose > 0);
        }

        // ---------------- Prueba 6: micrometeoritos ----------------
        [Test]
        public void P6_GuardarPaneles_RoversExpuestos30()
        {
            var c = Cfg();
            var s = NewState(c);
            var ev = new ScheduledEvent { type = CraterEventType.Micrometeorites, warned = true };
            var o = MeteorRules.Resolve(s, ev, MeteorProtect.Panels, new FixedRandom(0.10), c); // 0.10 < 0.30 → avería
            Assert.AreEqual(80, s.energy);
            Assert.IsTrue(s.today.panelsStowed);
            Assert.IsTrue(o.roversBroke); Assert.IsFalse(o.panelsBroke);
            Assert.AreEqual(50, s.rovers.capacity);
            Assert.AreEqual(0, TurnSimulator.Simulate(s, c).solarProduced);
            Assert.AreEqual(3, TurnSimulator.Simulate(s, c).iceCollected); // 6 × 50%

            var s2 = NewState(c);
            var o2 = MeteorRules.Resolve(s2, ev, MeteorProtect.Rovers, new FixedRandom(0.50), c); // 0.50 ≥ 0.30 → resiste
            Assert.IsFalse(o2.panelsBroke);
            Assert.AreEqual(35, s2.water); Assert.AreEqual(60, s2.oxygen);
            Assert.AreEqual(0, TurnSimulator.Simulate(s2, c).iceCollected);
        }

        [Test]
        public void P6_Reparaciones_InmediataYConTiempo()
        {
            var c = Cfg();
            var s = NewState(c);
            s.panels.capacity = 50; s.panels.broken = true;
            Assert.AreEqual(12, TurnSimulator.Simulate(s, c).solarProduced); // 23 × 50% = 11.5 → 12

            Assert.IsTrue(BaseActions.CanRepairNow(s.panels, s, c, out _));
            BaseActions.RepairNow(s.panels, s, c);
            Assert.AreEqual(100, s.panels.capacity); Assert.AreEqual(80, s.energy); Assert.AreEqual(40, s.water);

            var t = NewState(c);
            t.rovers.capacity = 50; t.rovers.broken = true;
            BaseActions.RepairSlow(t.rovers, c);
            var d1 = TurnSimulator.Simulate(t, c);
            Assert.AreEqual(0, d1.iceCollected);
            var d2 = TurnSimulator.Simulate(d1.after, c);
            Assert.AreEqual(0, d2.iceCollected);
            Assert.AreEqual(100, d2.after.rovers.capacity);
            Assert.AreEqual(6, TurnSimulator.Simulate(d2.after, c).iceCollected);
        }

        // ---------------- Prueba 7: eclipse ----------------
        [Test]
        public void P7_Eclipse_CuatroBaterias_CincoNecesarias()
        {
            var c = Cfg();
            var s = NewState(c);
            Assert.AreEqual(4, s.batteries);
            Assert.AreEqual(5, EclipseRules.TotalCost(c));
            Assert.IsFalse(EclipseRules.Affordable(new[] { true, true, true, true }, s.batteries, c));

            var sel = new[] { true, true, false, true }; // sin centro de energía
            Assert.IsTrue(EclipseRules.Affordable(sel, s.batteries, c));
            EclipseRules.Apply(s, sel, c);
            Assert.AreEqual(0, s.batteries);
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(0, r.solarProduced);
            Assert.AreEqual(26, r.Get(BuildingId.Recycler).energyUsed); // 22 × 1.2 = 26.4
            Assert.AreEqual(1, r.after.energyOverheadTurns);

            var s2 = NewState(c);
            EclipseRules.Apply(s2, new[] { false, true, true, true }, c); // sin oxígeno
            var r2 = TurnSimulator.Simulate(s2, c);
            Assert.AreEqual(0, r2.Get(BuildingId.Recycler).oxygenMade);
            Assert.AreEqual(70 - 25 - 6, r2.after.oxygen);
            Assert.AreEqual(1, s2.batteries);

            var s3 = NewState(c);
            EclipseRules.Apply(s3, new[] { true, true, true, false }, c); // sin monitoreo
            Assert.IsTrue(s3.nextEventUnwarned);
        }

        // ---------------- Prueba 8: victoria y derrota ----------------
        [Test]
        public void P8_Victoria_AlCompletarLaMision()
        {
            var c = Cfg(); c.cycles = 2;
            var s = NewState(c);
            for (int i = 0; i < c.TotalTurns; i++) s = TurnSimulator.Simulate(s, c).after;
            Assert.AreEqual(Outcome.Victory, s.outcome);
        }

        [Test]
        public void P8_Derrota_SaludCero()
        {
            var c = Cfg();
            var s = NewState(c);
            s.oxygen = 0; s.health = 20;
            for (int i = 0; i < 4; i++) s.buildingOn[i] = false;
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(0, r.after.health);
            Assert.AreEqual(Outcome.Defeat, r.after.outcome);
        }

        // ---------------- Prueba 9: proyección ----------------
        [Test]
        public void P9_ProyeccionIgualAlCalculoDefinitivo()
        {
            var c = Cfg();
            var s = NewState(c);
            var rng = new GameRandom(s);
            for (int day = 0; day < 20; day++)
            {
                int toggle = rng.Range(0, 4);
                s.buildingOn[toggle] = !s.buildingOn[toggle];
                var projected = TurnSimulator.Simulate(s, c).after;
                var final = TurnSimulator.Simulate(s, c).after;
                Assert.AreEqual(projected.energy, final.energy);
                Assert.AreEqual(projected.oxygen, final.oxygen);
                Assert.AreEqual(projected.food, final.food);
                Assert.AreEqual(projected.health, final.health);
                Assert.AreEqual(projected.water, final.water);
                s = final;
            }
        }

        // ---------------- Calendario ----------------
        [Test]
        public void Calendario_TresTiposYMaximoDosEclipses()
        {
            var c = Cfg();
            for (int seed = 0; seed < 200; seed++)
            {
                var s = GameState.CreateNew(c, seed);
                var list = EventScheduler.Generate(c, new GameRandom(s));
                Assert.AreEqual(5, list.Count);
                Assert.IsTrue(list.Any(e => e.type == CraterEventType.SolarRadiation));
                Assert.IsTrue(list.Any(e => e.type == CraterEventType.Micrometeorites));
                Assert.IsTrue(list.Any(e => e.type == CraterEventType.EarthShadow));
                Assert.IsTrue(list.Count(e => e.type == CraterEventType.EarthShadow) <= 2);
                Assert.IsTrue(list.All(e => e.day > c.turnsPerCycle && e.day <= c.TotalTurns)); // ciclo 1 sin eventos
            }
        }

        [Test]
        public void AnimoCiclo4_ViviendaApagada_Vivienda10()
        {
            var c = Cfg();
            var s = NewState(c); s.day = 16; // último día del ciclo 4
            Assert.IsFalse(TurnSimulator.Simulate(s, c).moraleHit);   // vivienda encendida todo el ciclo
            s.buildingOn[(int)BuildingId.Habitat] = false;
            Assert.IsTrue(TurnSimulator.Simulate(s, c).moraleHit);    // apagada: fatiga y estrés
        }

        [Test]
        public void Emergencia_NoEsDerrotaInmediata()
        {
            var c = Cfg();
            var s = NewState(c);
            s.habitat = 0;
            s.buildingOn[(int)BuildingId.Habitat] = false;
            var r = TurnSimulator.Simulate(s, c);
            Assert.IsTrue(r.after.emergency);
            Assert.AreEqual(Outcome.Playing, r.after.outcome);
        }

        // ---------------- Rebalance ----------------
        [Test]
        public void Rebalance_ApagarReciclador_EnergiaMas6_OxigenoMenos4()
        {
            var c = Cfg(); var s = NewState(c);
            s.energy = 50;
            s.buildingOn[(int)BuildingId.Recycler] = false;
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(6, r.energy.Delta);
            Assert.AreEqual(-4, r.oxygen.Delta);
            Assert.AreEqual(2, r.food.Delta);
        }

        [Test]
        public void Vivienda_Apagada_Salud3()
        {
            var c = Cfg(); var s = NewState(c);
            s.buildingOn[(int)BuildingId.Habitat] = false;
            var r = TurnSimulator.Simulate(s, c);
            Assert.AreEqual(-3, r.health.Delta);
        }

        // ---------------- Eventos menores ----------------
        [Test]
        public void Menores_SeisEventos_DosOpciones_RecursosDistintos_SinGratis()
        {
            var c = Cfg();
            Assert.AreEqual(6, c.minorEvents.Count);
            foreach (var e in c.minorEvents)
            {
                Assert.IsTrue(e.optionA.resource != e.optionB.resource);
                Assert.IsTrue(e.optionA.cost > 0 && e.optionB.cost > 0);
            }
        }

        [Test]
        public void Menores_NoCoincidenConPrincipalesNiSeRepitenAntesDeAgotarse()
        {
            var c = Cfg();
            for (int seed = 0; seed < 200; seed++)
            {
                var s = GameState.CreateNew(c, seed);
                var rng = new GameRandom(s);
                var mains = EventScheduler.Generate(c, rng);
                var minors = MinorEventRules.Generate(c, rng, mains);
                Assert.IsTrue(minors.Count >= 4);
                foreach (var m in minors)
                {
                    Assert.IsTrue(m.day > c.turnsPerCycle);                       // nada durante el tutorial
                    Assert.IsFalse(mains.Any(e => e.day == m.day || e.day - EventScheduler.WarningLead(e.type, c) == m.day));
                }
                for (int i = 1; i < minors.Count; i++) Assert.IsTrue(minors[i].day - minors[i - 1].day >= c.minorEventMinGap);
                var first = minors.Take(6).Select(m => m.id).ToList();
                Assert.AreEqual(first.Count, first.Distinct().Count());
            }
        }

        [Test]
        public void Menores_OpcionSinRecursoSeDeshabilita_YSeCobraUnaVez()
        {
            var c = Cfg(); var s = NewState(c);
            var frio = c.minorEvents.First(e => e.id == "frio");
            s.energy = 5;
            Assert.IsFalse(MinorEventRules.CanPay(s, frio.optionA));  // Energía −8 con solo 5
            Assert.IsTrue(MinorEventRules.CanPay(s, frio.optionB));   // Salud −5
            MinorEventRules.Apply(s, frio.optionB, c);
            Assert.AreEqual(95, s.health);
            Assert.AreEqual(5, s.energy);
            s.energy = 0; s.health = 3;
            Assert.IsFalse(MinorEventRules.CanResolve(s, frio));       // ninguna opción válida: se pospone
        }
    }
}
