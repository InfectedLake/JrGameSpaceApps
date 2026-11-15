using System;

namespace Crater.Core
{
    public interface IRandom
    {
        /// <summary>Número en [0, 1).</summary>
        double NextDouble();
        /// <summary>Entero en [min, maxExclusive).</summary>
        int Range(int min, int maxExclusive);
    }

    /// <summary>
    /// Generador reproducible: guarda la semilla y cuántos números se sacaron,
    /// así una partida cargada continúa con la misma secuencia.
    /// </summary>
    public class GameRandom : IRandom
    {
        GameState state;
        Random rng;

        /// <summary>Apunta el contador al nuevo estado (cada día el estado se reemplaza por una copia).</summary>
        public void Rebind(GameState newState)
        {
            newState.rngDraws = state.rngDraws;
            state = newState;
        }

        public GameRandom(GameState state)
        {
            this.state = state;
            rng = new Random(state.rngSeed);
            for (int i = 0; i < state.rngDraws; i++) rng.NextDouble();
        }

        public double NextDouble()
        {
            state.rngDraws++;
            return rng.NextDouble();
        }

        public int Range(int min, int maxExclusive)
        {
            if (maxExclusive <= min) return min;
            return min + (int)Math.Floor(NextDouble() * (maxExclusive - min));
        }
    }

    /// <summary>Generador con resultados fijos, útil para pruebas.</summary>
    public class FixedRandom : IRandom
    {
        readonly double[] values;
        int i;
        public FixedRandom(params double[] values) { this.values = values; }
        public double NextDouble() => values.Length == 0 ? 0.5 : values[i++ % values.Length];
        public int Range(int min, int maxExclusive) => maxExclusive <= min ? min : min + (int)Math.Floor(NextDouble() * (maxExclusive - min));
    }
}
