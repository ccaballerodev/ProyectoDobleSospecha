using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class SceneTransition : MonoBehaviour
{
    [Header("CanvasGroup para fade")]
    public CanvasGroup canvasGroup;

    [Header("Duración del fade")]
    public float fadeDuration = 0.8f;

    [Header("Referencias de Red")]
    [SerializeField] private TMP_InputField inputFieldCodigo;
    [SerializeField] private RelayManager relayManager;

    void Start()
    {
        if (canvasGroup == null) return;

        canvasGroup.DOKill();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Fade-in al abrir el menú (Pantalla negra -> Menú visible)
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
        });
    }

    // --- MÉTODOS PARA LOS BOTONES ---

    public void BotonHost()
    {
        // Hacemos el fade out y luego llamamos al Host
        StartCoroutine(ProcesoTransicion(() => {
            relayManager.Host();
        }));
    }

    public void BotonJoin()
    {
        string codigo = inputFieldCodigo.text;
        if (!string.IsNullOrEmpty(codigo))
        {
            relayManager.Client(codigo);
            // Hacemos el fade out y luego llamamos al Client
            StartCoroutine(ProcesoTransicion(() => {
                
            }));
        }
        else
        {
            Debug.LogWarning("¡Debes ingresar un código!");
        }
    }

    // --- LA CORRUTINA DE TRANSICIÓN ---
    // Esta función oscurece la pantalla y ejecuta la acción que le pidamos
    IEnumerator ProcesoTransicion(System.Action accionAlOscurecer)
    {
        canvasGroup.blocksRaycasts = true;

        // Esperamos a que el fade termine (Pantalla se vuelve negra)
        yield return canvasGroup.DOFade(1f, fadeDuration).WaitForCompletion();

        // Ejecutamos la conexión (Host o Join)
        accionAlOscurecer?.Invoke();
    }
}