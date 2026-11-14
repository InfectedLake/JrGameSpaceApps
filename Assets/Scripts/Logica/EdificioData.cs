using UnityEngine;

[CreateAssetMenu(fileName = "NuevoEdificio", menuName = "Outpost/Edificio")]
public class EdificioData : ScriptableObject {
    public string nombre;
    // Positivo = produce, negativo = consume (por turno, si está activo)
    public int deltaEnergia;
    public int deltaOxigeno;
    public int deltaComida;
}
