using TMPro;
using Unity.Netcode;
using UnityEngine;

public enum PlayerTurn
{
    Player1,
    Player2
}

public class PlayerTurnController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private NetworkVariable<PlayerTurn> currentTurn = new NetworkVariable<PlayerTurn>(
        PlayerTurn.Player1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            playerText.text = "Host (P1)";
        else
            playerText.text = "Cliente (P2)";

        currentTurn.OnValueChanged += OnTurnChanged;
        ActualizarMensajeTurno(currentTurn.Value);
    }

    private void OnTurnChanged(PlayerTurn oldTurn, PlayerTurn newTurn)
    {
        ActualizarMensajeTurno(newTurn);
    }

    private void ActualizarMensajeTurno(PlayerTurn turnoActual)
    {
        dialogueText.text = $"Turno de: {turnoActual}";
    }
    public void OnButtonPressed()
    {
        SendButtonPressedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)] 
    private void SendButtonPressedServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        bool esTurnoP1 = (senderId == 0 && currentTurn.Value == PlayerTurn.Player1);
        bool esTurnoP2 = (senderId != 0 && currentTurn.Value == PlayerTurn.Player2);

        if (esTurnoP1 || esTurnoP2)
        {
            string message = $"Jugador {(senderId == 0 ? "1" : "2")} presionó el botón";

            // Cambiar el turno
            currentTurn.Value = (currentTurn.Value == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;

            // Notificar a todos
            ShowTextClientRpc(message);
        }
        else
        {
            Debug.Log($"Jugador {senderId} intentó presionar pero no es su turno.");
        }
    }

    [ClientRpc]
    private void ShowTextClientRpc(string message)
    {
        dialogueText.text = message;
    }
}