using UnityEngine;

[CreateAssetMenu(fileName = "NuevoCaso", menuName = "Juego/Caso")]
public class CasoData : ScriptableObject
{
    public string premisa;
    public EvidenciaConPuntos[] evidencias;
}

[System.Serializable]
public struct FraseConValor
{
    [TextArea] public string texto;
    public int puntos; // El "valor oculto"
}

[System.Serializable]
public struct EvidenciaConPuntos
{
    public string nombreEvidencia;
    public FraseConValor[] acusaciones; // 4 opciones
    public FraseConValor[] defensas;    // 4 opciones
}