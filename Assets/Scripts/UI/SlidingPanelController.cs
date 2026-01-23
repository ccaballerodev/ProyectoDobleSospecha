using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SlidingPanelController : MonoBehaviour, IPointerClickHandler
{
    [Header("Panel a controlar")]
    public GenericSlidingPanel panel;

    [Header("Typewriter Effects")]
    public List<TypewriterEffect> typewritersInOrder;

    [Header("Sello")]
    public StampEffect stampEffect; // Efecto de sello a ejecutar al final

    [Header("Delay antes de aparecer")]
    public float startDelay = 1f;

    [Header("Delay entre Typewriters")]
    public float typewriterDelay = 1f; // Delay de 1 segundo por Typewriter

    [Header("Cambio de escena")]
    public string sceneToLoad; // Escena a cargar al finalizar el sello
    public float postStampDelay = 3f; // Delay después del sello antes de cambiar de escena

    private bool isOpen = false;
    private int currentTypewriterIndex = 0;
    private TypewriterEffect currentTypewriter = null;

    void Start()
    {
        if (panel == null)
        {
            Debug.LogWarning("[SlidingPanelController] No se ha asignado panel.");
            return;
        }

        StartCoroutine(OpenPanelWithDelay());
    }

    private IEnumerator OpenPanelWithDelay()
    {
        yield return new WaitForSeconds(startDelay);

        panel.gameObject.SetActive(true);
        panel.SlideIn();
        isOpen = true;

        if (typewritersInOrder != null && typewritersInOrder.Count > 0)
        {
            currentTypewriterIndex = 0;
            StartCoroutine(PlayNextTypewriterWithDelay());
        }
        else
        {
            // Si no hay Typewriters, activa y ejecuta el sello
            if (stampEffect != null)
            {
                stampEffect.gameObject.SetActive(true);
                stampEffect.PlayStamp();
                StartCoroutine(PostStampDelay());
            }
        }
    }

    private IEnumerator PlayNextTypewriterWithDelay()
    {
        if (currentTypewriterIndex >= typewritersInOrder.Count)
        {
            currentTypewriter = null;

            // Todos los Typewriters terminaron, activa y ejecuta el sello
            if (stampEffect != null)
            {
                stampEffect.gameObject.SetActive(true);
                stampEffect.PlayStamp();
                StartCoroutine(PostStampDelay());
            }

            yield break;
        }

        currentTypewriter = typewritersInOrder[currentTypewriterIndex];
        if (currentTypewriter == null)
        {
            currentTypewriterIndex++;
            yield return StartCoroutine(PlayNextTypewriterWithDelay());
            yield break;
        }

        // Espera el delay antes de iniciar este Typewriter
        yield return new WaitForSeconds(typewriterDelay);

        currentTypewriter.PrepareText();
        bool done = false;

        currentTypewriter.PlayTypeEffect(() => done = true);

        // Espera hasta que termine la animación
        while (!done)
            yield return null;

        currentTypewriterIndex++;
        StartCoroutine(PlayNextTypewriterWithDelay());
    }

    private IEnumerator PostStampDelay()
    {
        yield return new WaitForSeconds(postStampDelay);

        // Mata todos los tweens activos antes de cambiar de escena
        DOTween.KillAll();

        ChangeScene();
    }

    private void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("[SlidingPanelController] No se ha definido ninguna escena para cargar.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOpen || panel == null) return;

        if (currentTypewriter != null && currentTypewriter.IsTyping)
        {
            currentTypewriter.Skip();
        }
        else
        {
            panel.SlideOut(() =>
            {
                panel.gameObject.SetActive(false);
                isOpen = false;
            });
        }
    }
}