using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EdificioUI : MonoBehaviour {
    public TurnManager tm;
    public int indice;               // 0 panel, 1 invernadero, 2 reciclador, 3 escudo
    public Button boton;
    public Image imagen;
    public TMP_Text etiqueta;
    public Sprite spriteOn, spriteOff;   // opcionales: si están vacíos se usa solo color

    static readonly Color colOn   = Color.white;
    static readonly Color colOff  = new Color(0.45f, 0.45f, 0.45f);
    static readonly Color colFalla = new Color(1f, 0.45f, 0.45f);

    void OnEnable()  { tm.OnCambio += Refrescar; }
    void OnDisable() { tm.OnCambio -= Refrescar; }

    void Start() {
        boton.onClick.AddListener(() => tm.AlternarEdificio(indice));
        Refrescar();
    }

    void Refrescar() {
        var ed = tm.edificios[indice];
        bool activo = ed.activo;

        // ¿Estaría activo pero sin energía suficiente para funcionar este turno?
        var fallan = new List<Edificio>();
        tm.Simular(tm.estado, fallan);
        bool sinEnergia = activo && fallan.Contains(ed);

        if (spriteOn != null && spriteOff != null)
            imagen.sprite = activo ? spriteOn : spriteOff;
        imagen.color = !activo ? colOff : (sinEnergia ? colFalla : colOn);

        var d = ed.data;
        string estadoTxt = !activo ? "APAGADO" : (sinEnergia ? "SIN ENERGÍA" : "ACTIVO");
        etiqueta.text = $"{d.nombre}\n{estadoTxt}\n{Resumen(d)}";
    }

    static string Resumen(EdificioData d) {
        var partes = new List<string>();
        if (d.deltaEnergia != 0) partes.Add($"{Signo(d.deltaEnergia)} energía");
        if (d.deltaOxigeno != 0) partes.Add($"{Signo(d.deltaOxigeno)} oxígeno");
        if (d.deltaComida  != 0) partes.Add($"{Signo(d.deltaComida)} comida");
        return string.Join("  ", partes);
    }

    static string Signo(int v) { return v > 0 ? "+" + v : v.ToString(); }
}