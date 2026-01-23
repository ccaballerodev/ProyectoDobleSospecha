using UnityEngine;
using DG.Tweening;
using System;

public class GenericSlidingPanel : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup canvasGroup;   // Para fade y bloqueo de inputs
    public RectTransform panel;       // Panel a mover

    [Header("Animación")]
    public Vector2 slideDirection = Vector2.right; // Dirección del slide
    public float slideDistance = 800f;             // Distancia del slide
    public float moveDuration = 0.5f;              // Duración de la animación
    public Ease easeIn = Ease.OutSine;
    public Ease easeOut = Ease.InSine;

    [Header("Opciones")]
    public bool mirror = false;       // Si true, sale en la dirección opuesta a la entrada

    private Vector2 originalPos;

    void Awake()
    {
        if (panel == null)
            panel = GetComponent<RectTransform>();

        originalPos = panel.anchoredPosition;
    }

    // Entrada animada
    public void SlideIn()
    {
        if (panel == null || canvasGroup == null) return;

        // Estado inicial fuera de la pantalla
        panel.anchoredPosition = originalPos + slideDirection.normalized * slideDistance;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        // Mata tweens anteriores
        DOTween.Kill(panel);
        DOTween.Kill(canvasGroup);

        // Animación
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, moveDuration));
        seq.Join(panel.DOAnchorPos(originalPos, moveDuration).SetEase(easeIn));
    }

    // Salida animada
    public void SlideOut(Action onComplete = null)
    {
        if (panel == null || canvasGroup == null) return;

        DOTween.Kill(panel);
        DOTween.Kill(canvasGroup);

        // Determinar dirección de salida
        Vector2 exitDirection = mirror ? -slideDirection.normalized : slideDirection.normalized;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, moveDuration));
        seq.Join(panel.DOAnchorPos(originalPos + exitDirection * slideDistance, moveDuration).SetEase(easeOut));
        seq.OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            onComplete?.Invoke();
        });
    }
}