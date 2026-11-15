using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FilaRecurso : MonoBehaviour {
    public Image icono;
    public TMP_Text cantidad;
    public TMP_Text delta;
    public Slider barra;
    public Image relleno;
    public int umbralBajo = 25;

    public void Mostrar(int actual, int proyectado, int maximo) {
        cantidad.text = actual.ToString();
        barra.maxValue = maximo;
        barra.value = actual;

        int d = proyectado - actual;
        if (d > 0)      { delta.text = "+" + d; delta.color = new Color(0.3f, 0.85f, 0.4f); }
        else if (d < 0) { delta.text = d.ToString(); delta.color = new Color(0.95f, 0.3f, 0.3f); }
        else            { delta.text = "0"; delta.color = Color.gray; }

        if (relleno != null)
            relleno.color = actual <= umbralBajo ? new Color(0.95f, 0.3f, 0.3f) : new Color(0.3f, 0.7f, 1f);
    }
}