using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI evidenceTitleText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] buttonTexts;

    [Header("UI Velocímetro")]
    [SerializeField] private RectTransform agujaTransform;
    [SerializeField] private float suavidadRotacion = 5f;

    [Header("Data")]
    [SerializeField] private CasoData casoActual;

    private int puntosAcusacionActual = 0;
    private bool estaTipeando = false;

    private NetworkVariable<int> evidenciaIndice = new NetworkVariable<int>(0);
    private NetworkVariable<bool> esFaseAcusacion = new NetworkVariable<bool>(true);
    private NetworkVariable<float> inclinacionCulpabilidad = new NetworkVariable<float>(0f);

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerCountChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerCountChanged;

        ActualizarInterfazLocal();

        // Suscripciones
        evidenciaIndice.OnValueChanged += (oldVal, newVal) => ActualizarInterfazLocal();
        esFaseAcusacion.OnValueChanged += (oldVal, newVal) => ActualizarInterfazLocal();

        inclinacionCulpabilidad.OnValueChanged += (oldVal, newVal) => {
            StopCoroutine("MoverAguja");
            StartCoroutine(MoverAguja(newVal));
        };
    }

    private void OnPlayerCountChanged(ulong id) => ActualizarInterfazLocal();

    void ActualizarInterfazLocal()
    {
        bool faltanJugadores = NetworkManager.Singleton.ConnectedClients.Count < 2;

        if (faltanJugadores)
        {
            BloquearPorFaltaDeJugadores();
            return;
        }

        // --- ESTA ES LA CLAVE ---
        // Si estamos tipeando, NO permitimos que esta función habilite botones
        if (estaTipeando)
        {
            foreach (var btn in optionButtons) btn.interactable = false;
            return;
        }

        int evIdx = evidenciaIndice.Value;
        if (evIdx >= casoActual.evidencias.Length)
        {
            evidenceTitleText.text = "Veredicto Final";
            return;
        }

        evidenceTitleText.text = "Evidencia: " + casoActual.evidencias[evIdx].nombreEvidencia;

        bool esAcusador = (NetworkManager.Singleton.LocalClientId == 0);
        bool miTurno = (esFaseAcusacion.Value && esAcusador) || (!esFaseAcusacion.Value && !esAcusador);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            // Solo se habilitan si es mi turno Y no se está tipeando
            optionButtons[i].interactable = miTurno;

            if (miTurno)
            {
                if (esFaseAcusacion.Value)
                    buttonTexts[i].text = casoActual.evidencias[evIdx].acusaciones[i].texto;
                else
                    buttonTexts[i].text = casoActual.evidencias[evIdx].defensas[i].texto;
            }
            else
            {
                buttonTexts[i].text = "Esperando respuesta...";
            }
        }
    }

    public void OnOptionSelected(int index)
    {
        // Bloqueo preventivo inmediato en el cliente que presiona
        foreach (var btn in optionButtons) btn.interactable = false;
        SubmitSelectionServerRpc(index);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitSelectionServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;

        int evIdx = evidenciaIndice.Value;
        string textoElegido = "";

        if (esFaseAcusacion.Value)
        {
            puntosAcusacionActual = casoActual.evidencias[evIdx].acusaciones[index].puntos;
            textoElegido = "ACUSADOR: " + casoActual.evidencias[evIdx].acusaciones[index].texto;
            esFaseAcusacion.Value = false;
        }
        else
        {
            int puntosDefensa = casoActual.evidencias[evIdx].defensas[index].puntos;
            textoElegido = "DEFENSA: " + casoActual.evidencias[evIdx].defensas[index].texto;

            float balanceRonda = (float)(puntosAcusacionActual - puntosDefensa) / 100f;
            inclinacionCulpabilidad.Value = Mathf.Clamp(inclinacionCulpabilidad.Value + balanceRonda, -1f, 1f);

            esFaseAcusacion.Value = true;
            evidenciaIndice.Value++;
        }

        ShowDialogueClientRpc(textoElegido);
    }

    [ClientRpc]
    void ShowDialogueClientRpc(string fullText)
    {
        // Forzamos detener cualquier tipeo anterior antes de empezar el nuevo
        StopCoroutine("TypeSentence");
        StartCoroutine(TypeSentence(fullText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        estaTipeando = true;

        // Mientras la corrutina corre, nos aseguramos que los botones estén apagados
        foreach (var btn in optionButtons) btn.interactable = false;

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }

        // Ya terminó de escribir
        estaTipeando = false;

        // Ahora sí llamamos a la lógica de turnos para que habilite al que sigue
        ActualizarInterfazLocal();
    }

    private void BloquearPorFaltaDeJugadores()
    {
        evidenceTitleText.text = "Sala de espera";
        dialogueText.text = "Buscando otro sospechoso...";
        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
            btn.GetComponentInChildren<TextMeshProUGUI>().text = "Esperando jugador...";
        }
    }

    IEnumerator MoverAguja(float objetivo)
    {
        float anguloZ = objetivo * -90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, anguloZ);
        while (Quaternion.Angle(agujaTransform.localRotation, targetRotation) > 0.1f)
        {
            agujaTransform.localRotation = Quaternion.Lerp(agujaTransform.localRotation, targetRotation, Time.deltaTime * suavidadRotacion);
            yield return null;
        }
        agujaTransform.localRotation = targetRotation;
    }
}