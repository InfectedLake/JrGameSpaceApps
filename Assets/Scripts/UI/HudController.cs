using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour {
    public TurnManager tm;
    public FilaRecurso energia, oxigeno, comida, salud;
    public TMP_Text textoDia;
    public Button botonTerminar;

    void OnEnable()  { tm.OnCambio += Refrescar; }
    void OnDisable() { tm.OnCambio -= Refrescar; }

    void Start() {
        botonTerminar.onClick.AddListener(tm.SiguienteTurno);
        Refrescar();
    }

    void Refrescar() {
        var e = tm.estado;
        var p = tm.Proyectar();          // cómo quedaría si terminas el día ahora
        int m = tm.maximo;

        energia.Mostrar(e.energia, p.energia, m);
        oxigeno.Mostrar(e.oxigeno, p.oxigeno, m);
        comida.Mostrar(e.comida, p.comida, m);
        salud.Mostrar(e.salud, p.salud, m);

        textoDia.text = $"Día {Mathf.Min(e.sol, tm.diasObjetivo)} / {tm.diasObjetivo}";
        botonTerminar.interactable = tm.resultado == Resultado.Jugando;
    }
}