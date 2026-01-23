using UnityEngine;

[CreateAssetMenu(fileName = "NuevoCaso", menuName = "Juego/Caso")]
public class CasoData : ScriptableObject
{
    public string premisa;
    public Evidencia[] evidencias;
}

[System.Serializable]
public struct Evidencia
{
    public string nombreEvidencia;
    [TextArea] public string[] acusaciones; // Siempre 4
    [TextArea] public string[] defensas;    // Siempre 4
}