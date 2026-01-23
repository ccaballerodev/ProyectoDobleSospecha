using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("CanvasGroup para fade")]
    public CanvasGroup canvasGroup;

    [Header("Duración del fade")]
    public float fadeDuration = 0.8f;

    void Start()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[SceneTransition] No se ha asignado un CanvasGroup!");
            return;
        }

        // Mata cualquier tween activo en este CanvasGroup
        canvasGroup.DOKill();

        // Inicializa alpha en 1 (pantalla cubierta)
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Fade-in al inicio de la escena (1 -> 0)
        canvasGroup.DOFade(0f, fadeDuration).OnUpdate(() =>
        {
            canvasGroup.blocksRaycasts = canvasGroup.alpha > 0f;
        }).OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            Debug.Log("[SceneTransition] Fade-in completado");
        });
    }

    // Método público para botones
    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        if (canvasGroup == null)
            yield break;

        // Mata cualquier tween activo en este CanvasGroup
        DOTween.KillAll();

        // Fade-out (0 -> 1)
        canvasGroup.blocksRaycasts = true;
        yield return canvasGroup.DOFade(1f, fadeDuration).OnUpdate(() =>
        {
            canvasGroup.blocksRaycasts = canvasGroup.alpha > 0f;
        }).WaitForCompletion();

        Debug.Log($"[SceneTransition] Fade-out completado, cargando escena {sceneName}");

        // Cargar la nueva escena
        SceneManager.LoadScene(sceneName);
    }
}