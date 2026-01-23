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
        StopCoroutine("TypeSentence");

        // Determinamos quién está hablando basándonos en el texto
        // Si el texto empieza con "ACUSADOR", el ID es 0, si no, es 1
        bool esTurnoAcusador = fullText.StartsWith("ACUSADOR");
        ulong hablandoId = esTurnoAcusador ? (ulong)0 : (ulong)1;

        StartCoroutine(TypeSentence(fullText, hablandoId));
    }

    IEnumerator TypeSentence(string sentence, ulong autorId)
    {
        estaTipeando = true;
        foreach (var btn in optionButtons) btn.interactable = false;

        // --- ACTIVAR ANIMACIÓN DE HABLAR ---
        ControlarAnimacionJugador(autorId, "Hablar");

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }

        // --- VOLVER A ESPERAR ---
        ControlarAnimacionJugador(autorId, "Esperar");

        estaTipeando = false;
        ActualizarInterfazLocal();
    }

    private void ControlarAnimacionJugador(ulong id, string trigger)
    {
        // 1. Buscamos al cliente por su ID (0 para Host, 1 para Cliente)
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var networkClient))
        {
            var playerObject = networkClient.PlayerObject;
            if (playerObject != null)
            {
                // 2. Buscamos TODOS los animadores que hay dentro del jugador (en sus hijos)
                Animator[] anims = playerObject.GetComponentsInChildren<Animator>(true);

                foreach (Animator anim in anims)
                {
                    // 3. SOLO activamos el trigger en el modelo que está encendido (Active)
                    // Esto es importante porque el otro modelo está oculto
                    if (anim.gameObject.activeInHierarchy)
                    {
                        anim.SetTrigger(trigger);
                    }
                }
            }
        }
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