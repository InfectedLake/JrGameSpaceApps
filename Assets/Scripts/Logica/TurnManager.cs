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

    public event System.Action OnCambio;           // cualquier cambio que la UI deba reflejar
    public event System.Action OnTurnoTerminado;   // solo al terminar el día (sonidos, animaciones)

    // Calcula cómo quedaría el estado tras un turno. NO modifica nada.
    public EstadoJuego Simular(EstadoJuego origen, List<Edificio> sinEnergia = null) {
        var e = origen.Copiar();

        var orden = edificios
            .Where(x => x.activo && x.data != null)
            .OrderByDescending(x => x.data.deltaEnergia)
            .ToList();

        foreach (var ed in orden) {
            bool gastaEnergia = ed.data.deltaEnergia < 0;
            if (gastaEnergia && e.energia < -ed.data.deltaEnergia) {
                sinEnergia?.Add(ed);
                continue;
            }
            e.energia += ed.data.deltaEnergia;
            e.oxigeno += ed.data.deltaOxigeno;
            e.comida  += ed.data.deltaComida;
        }

        e.oxigeno -= e.astronautas;
        e.comida  -= e.astronautas;

        e.energia = Mathf.Clamp(e.energia, 0, maximo);
        e.oxigeno = Mathf.Clamp(e.oxigeno, 0, maximo);
        e.comida  = Mathf.Clamp(e.comida,  0, maximo);

        if (e.oxigeno == 0 || e.comida == 0) e.salud -= danoSaludPorFalta;
        e.salud = Mathf.Max(e.salud, 0);

        e.sol++;
        return e;
    }

    // Lo que la UI muestra en vivo
    public EstadoJuego Proyectar() { return Simular(estado); }

    // Para que el clic en un edificio use esto (y la UI se entere)
    public void AlternarEdificio(int indice) {
        if (resultado != Resultado.Jugando) return;
        edificios[indice].activo = !edificios[indice].activo;
        OnCambio?.Invoke();
    }

    [ContextMenu("Terminar dia")]
    public void SiguienteTurno() {
        if (resultado != Resultado.Jugando) {
            Debug.Log("La partida ya terminó: " + resultado);
            return;
        }

        var fallidos = new List<Edificio>();
        estado = Simular(estado, fallidos);
        foreach (var f in fallidos) Debug.Log($"  {f.data.nombre}: SIN ENERGIA, no funciona");

        if (estado.salud <= 0) resultado = Resultado.Derrota;
        else if (estado.sol > diasObjetivo) resultado = Resultado.Victoria;

        Debug.Log($"Dia {estado.sol} | E:{estado.energia} O:{estado.oxigeno} C:{estado.comida} S:{estado.salud} | {resultado}");
        OnTurnoTerminado?.Invoke();
        OnCambio?.Invoke();
    }

    [ContextMenu("Simular 5 turnos")]
    void Simular5() {
        for (int i = 0; i < 5; i++) SiguienteTurno();
    }

    [ContextMenu("Reiniciar")]
    public void Reiniciar() {
        estado = new EstadoJuego();
        resultado = Resultado.Jugando;
        OnCambio?.Invoke();
    }

    // Si marcas/desmarcas "Activo" en el Inspector durante Play, la UI también se actualiza
    void OnValidate() {
        if (Application.isPlaying) OnCambio?.Invoke();
    }
}