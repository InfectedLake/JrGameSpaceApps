using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Resultado { Jugando, Victoria, Derrota }

[System.Serializable]
public class Edificio {
    public EdificioData data;
    public bool activo = true;
}

public class TurnManager : MonoBehaviour {
    public EstadoJuego estado = new EstadoJuego();
    public List<Edificio> edificios = new List<Edificio>();

    public int maximo = 100;
    public int diasObjetivo = 15;
    public int danoSaludPorFalta = 20;
    public Resultado resultado = Resultado.Jugando;

    public event System.Action OnTurnoTerminado;   // la UI se suscribe a esto

    [ContextMenu("Terminar dia")]
    public void SiguienteTurno() {
        if (resultado != Resultado.Jugando) {
            Debug.Log("La partida ya terminó: " + resultado);
            return;
        }

        // 1) Productores de energía primero, consumidores después
        var orden = edificios
            .Where(e => e.activo && e.data != null)
            .OrderByDescending(e => e.data.deltaEnergia)
            .ToList();

        foreach (var e in orden) {
            bool gastaEnergia = e.data.deltaEnergia < 0;
            if (gastaEnergia && estado.energia < -e.data.deltaEnergia) {
                Debug.Log($"  {e.data.nombre}: SIN ENERGIA, no funciona");
                continue;
            }
            estado.energia += e.data.deltaEnergia;
            estado.oxigeno += e.data.deltaOxigeno;
            estado.comida  += e.data.deltaComida;
        }

        // 2) Consumo de los astronautas
        estado.oxigeno -= estado.astronautas;
        estado.comida  -= estado.astronautas;

        // 3) Límites 0..maximo
        estado.energia = Mathf.Clamp(estado.energia, 0, maximo);
        estado.oxigeno = Mathf.Clamp(estado.oxigeno, 0, maximo);
        estado.comida  = Mathf.Clamp(estado.comida,  0, maximo);

        // 4) Daño por falta de oxígeno o comida
        if (estado.oxigeno == 0 || estado.comida == 0) {
            estado.salud -= danoSaludPorFalta;
            Debug.Log("  ¡Falta oxigeno o comida! Salud -" + danoSaludPorFalta);
        }
        estado.salud = Mathf.Max(estado.salud, 0);

        // 5) Avanzar el día y revisar el fin
        estado.sol++;
        if (estado.salud <= 0) resultado = Resultado.Derrota;
        else if (estado.sol > diasObjetivo) resultado = Resultado.Victoria;

        Debug.Log($"Dia {estado.sol} | E:{estado.energia} O:{estado.oxigeno} C:{estado.comida} S:{estado.salud} | {resultado}");
        OnTurnoTerminado?.Invoke();
    }

    [ContextMenu("Simular 5 turnos")]
    void Simular5() {
        for (int i = 0; i < 5; i++) SiguienteTurno();
    }

    [ContextMenu("Reiniciar")]
    public void Reiniciar() {
        estado = new EstadoJuego();
        resultado = Resultado.Jugando;
        OnTurnoTerminado?.Invoke();
    }
}