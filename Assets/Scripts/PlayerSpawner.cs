using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    private void Update()
    {
        if (IsOwner)
        {
            PosicionarJugador();
        }
    }

    public override void OnNetworkSpawn()
    {
        // Solo el dueño del objeto se posiciona a sí mismo
        if (IsOwner)
        {
            PosicionarJugador();
        }
    }

    private void PosicionarJugador()
    {
        // Buscamos los puntos de spawn en la escena
        // Nota: Asegúrate de ponerles estos tags a tus puntos de spawn en Unity
        GameObject spawnHost = GameObject.FindWithTag("HostSpawn");
        GameObject spawnClient = GameObject.FindWithTag("ClientSpawn");

        if (OwnerClientId == 0) // El Host (Acusador)
        {
            if (spawnHost != null)
            {
                transform.position = spawnHost.transform.position;
                transform.rotation = spawnHost.transform.rotation;
            }
        }
        else // El Cliente (Defensor)
        {
            if (spawnClient != null)
            {
                transform.position = spawnClient.transform.position;
                transform.rotation = spawnClient.transform.rotation;
            }
        }
    }
}