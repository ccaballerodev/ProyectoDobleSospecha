using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class BattleManager : NetworkBehaviour
{
    [Header("UI Presentación Evidencia")]
    [SerializeField] private CanvasGroup panelPresentacion;
    [SerializeField] private Image imagenPresentacion;
    [SerializeField] private TextMeshProUGUI textoPresentacion;

    [Header("UI General")]
    [SerializeField] private TextMeshProUGUI evidenceTitleText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] buttonTexts;

    [Header("UI Velocímetro")]
    [SerializeField] private RectTransform agujaTransform;
    [SerializeField] private float suavidadRotacion = 5f;

    [Header("Data")]
    [SerializeField] private CasoData casoActual;

    [SerializeField] private TextMeshProUGUI joinCodeText;

    private bool juegoIniciado = false;
    private bool estaTipeando = false;
    private int puntosAcusacionActual = 0;

    private NetworkVariable<int> evidenciaIndice = new NetworkVariable<int>(0);
    private NetworkVariable<bool> esFaseAcusacion = new NetworkVariable<bool>(true);
    private NetworkVariable<float> inclinacionCulpabilidad = new NetworkVariable<float>(0f);

    private void Update()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            
            if (joinCodeText != null)
            {
                joinCodeText.gameObject.SetActive(false);
                Debug.Log("Jugador conectado: Ocultando código de unión");
            }
        }
        else
        {
            joinCodeText.gameObject.SetActive(true);
            joinCodeText.text = "Código de unión: " + RelayManager.joinCodeGame;
        }
        
    }

    public override void OnNetworkSpawn()
    {
        // Forzar estado invisible al inicio para todos
        panelPresentacion.alpha = 0;
        panelPresentacion.gameObject.SetActive(false);
        foreach (var btn in optionButtons) btn.interactable = false;

        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerCountChanged;

        // Escuchar cambio de evidencia
        evidenciaIndice.OnValueChanged += (oldVal, newVal) => {
            if (IsClient) StartCoroutine(SecuenciaCambioEvidencia());
        };

        esFaseAcusacion.OnValueChanged += (oldVal, newVal) => ActualizarInterfazLocal();

        inclinacionCulpabilidad.OnValueChanged += (oldVal, newVal) => {
            StopCoroutine("MoverAguja");
            StartCoroutine(MoverAguja(newVal));
        };

        ActualizarInterfazLocal();
    }

    private void OnPlayerCountChanged(ulong id)
    {
        // Solo el Host decide cuándo arrancar oficialmente
        if (IsServer && !juegoIniciado && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            juegoIniciado = true;
            // Llamamos a un ClientRpc para que todos sepan que el juego inició
            IniciarJuegoClientRpc();
        }
        ActualizarInterfazLocal();
    }

    [ClientRpc]
    void IniciarJuegoClientRpc()
    {
        juegoIniciado = true;
        StartCoroutine(SecuenciaCambioEvidencia());
    }

    IEnumerator SecuenciaCambioEvidencia()
    {
        // Bloqueo estricto
        foreach (var btn in optionButtons) btn.interactable = false;

        int evIdx = evidenciaIndice.Value;
        if (evIdx >= casoActual.evidencias.Length)
        {
            evidenceTitleText.text = "Veredicto Final";
            yield break;
        }

        // Configurar UI antes del Fade
        var evidencia = casoActual.evidencias[evIdx];
        imagenPresentacion.sprite = evidencia.imagenEvidencia;
        textoPresentacion.text = evidencia.nombreEvidencia;
        evidenceTitleText.text = "Evidencia: " + evidencia.nombreEvidencia;

        // Asegurar que el panel esté listo para el Fade In
        panelPresentacion.gameObject.SetActive(true);
        panelPresentacion.alpha = 0;
        panelPresentacion.transform.localScale = Vector3.one * 0.8f;

        // Animación Fade In
        panelPresentacion.DOFade(1f, 0.8f);
        panelPresentacion.transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(3f);

        // Animación Fade Out
        panelPresentacion.DOFade(0f, 0.5f).OnComplete(() => panelPresentacion.gameObject.SetActive(false));
        yield return new WaitForSeconds(0.6f);

        ActualizarInterfazLocal();
    }

    void ActualizarInterfazLocal()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count < 2)
        {
            BloquearPorFaltaDeJugadores();
            return;
        }

        // No habilitar nada si estamos en medio de procesos visuales
        if (!juegoIniciado || estaTipeando || panelPresentacion.gameObject.activeInHierarchy) return;

        int evIdx = evidenciaIndice.Value;
        bool esAcusador = (NetworkManager.Singleton.LocalClientId == 0);
        bool miTurno = (esFaseAcusacion.Value && esAcusador) || (!esFaseAcusacion.Value && !esAcusador);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = miTurno;
            if (miTurno)
            {
                buttonTexts[i].text = esFaseAcusacion.Value ?
                    casoActual.evidencias[evIdx].acusaciones[i].texto :
                    casoActual.evidencias[evIdx].defensas[i].texto;
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
        int evIdx = evidenciaIndice.Value;
        string textoElegido = "";

        if (esFaseAcusacion.Value)
        {
            puntosAcusacionActual = casoActual.evidencias[evIdx].acusaciones[index].puntos;
            textoElegido = "ACUSADOR: " + casoActual.evidencias[evIdx].acusaciones[index].texto;
            esFaseAcusacion.Value = false;
            ShowDialogueClientRpc(textoElegido, false); // false = no cambiar de evidencia aún
        }
        else
        {
            int puntosDefensa = casoActual.evidencias[evIdx].defensas[index].puntos;
            textoElegido = "DEFENSA: " + casoActual.evidencias[evIdx].defensas[index].texto;

            float balanceRonda = (float)(puntosAcusacionActual - puntosDefensa) / 100f;
            inclinacionCulpabilidad.Value = Mathf.Clamp(inclinacionCulpabilidad.Value + balanceRonda, -1f, 1f);

            esFaseAcusacion.Value = true;
            // Indicamos que después de este diálogo DEBE cambiar la evidencia
            ShowDialogueClientRpc(textoElegido, true);
        }
    }

    [ClientRpc]
    void ShowDialogueClientRpc(string fullText, bool debeCambiarEvidencia)
    {
        StopCoroutine("TypeSentence");
        bool esTurnoAcusador = fullText.StartsWith("ACUSADOR");
        ulong hablandoId = esTurnoAcusador ? (ulong)0 : (ulong)1;
        StartCoroutine(TypeSentence(fullText, hablandoId, debeCambiarEvidencia));
    }

    IEnumerator TypeSentence(string sentence, ulong autorId, bool cambiarEvidenciaAlFinal)
    {
        estaTipeando = true;
        foreach (var btn in optionButtons) btn.interactable = false;

        ControlarAnimacionJugador(autorId, "Hablar");

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }

        ControlarAnimacionJugador(autorId, "Esperar");
        estaTipeando = false;

        if (cambiarEvidenciaAlFinal)
        {
            yield return new WaitForSeconds(1f); // Pausa dramática antes de la nueva evidencia
            if (IsServer) evidenciaIndice.Value++;
        }
        else
        {
            ActualizarInterfazLocal();
        }
    }

    // --- MÉTODOS DE APOYO (Animación y Aguja) ---

    private void ControlarAnimacionJugador(ulong id, string trigger)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var networkClient))
        {
            var anim = networkClient.PlayerObject.GetComponentInChildren<Animator>();
            if (anim != null && anim.gameObject.activeInHierarchy)
            {
                anim.SetTrigger(trigger);
            }
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

    private void BloquearPorFaltaDeJugadores()
    {
        evidenceTitleText.text = "Sala de espera";
        dialogueText.text = "Buscando otro sospechoso...";
        foreach (var btn in optionButtons)
        {
            btn.interactable = false;
            buttonTexts[btn.transform.GetSiblingIndex()].text = "Esperando jugador...";
        }
    }
}