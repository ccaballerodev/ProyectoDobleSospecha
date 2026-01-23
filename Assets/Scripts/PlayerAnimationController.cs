using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimationController : NetworkBehaviour
{
    // Referencias a los animadores de cada modelo hijo
    [SerializeField] private Animator animatorP1;
    [SerializeField] private Animator animatorP2;

    // Usamos el NetworkAnimator para sincronizar los Triggers por la red
    private NetworkAnimator networkAnimator;

    private void Awake()
    {
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    // Esta función se llama cuando alguien presiona un botón de acción
    public void EjecutarAnimacionAccion()
    {
        // Solo el dueño del objeto puede pedir que se anime
        if (!IsOwner) return;

        // Disparamos un Trigger llamado "Accion" que debe existir en ambos Animators
        if (OwnerClientId == 0)
        {
            // Si soy el Host, animo mi modelo de Acusador
            animatorP1.SetTrigger("Hablar");
        }
        else
        {
            // Si soy el Cliente, animo mi modelo de Defensor
            animatorP2.SetTrigger("Defender");
        }

        // Para que el OTRO jugador lo vea, enviamos la señal al servidor
        // Si usas NetworkAnimator, esto suele sincronizarse automáticamente 
        // si el componente está configurado correctamente.
    }
}