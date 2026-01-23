using Unity.Netcode;
using UnityEngine;

public class PlayerVisuals : NetworkBehaviour
{
    [SerializeField] private GameObject modeloP1;
    [SerializeField] private GameObject modeloP2;

    public override void OnNetworkSpawn()
    {
        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        // OwnerClientId 0 siempre es el Host (Acusador)
        // OwnerClientId 1 (o más) es el Cliente (Defensor)

        if (OwnerClientId == 0)
        {
            modeloP1.SetActive(true);
            modeloP2.SetActive(false);
            Debug.Log($"Jugador {OwnerClientId}: Activando visual de Acusador");
        }
        else
        {
            modeloP1.SetActive(false);
            modeloP2.SetActive(true);
            Debug.Log($"Jugador {OwnerClientId}: Activando visual de Defensor");
        }
    }
}