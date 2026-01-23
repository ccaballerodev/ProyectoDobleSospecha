using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
public class StampEffect : MonoBehaviour
{
    [Header("Configuración del sello")]
    public float startScale = 0.1f;       // Escala inicial
    public float endScale = 1f;           // Escala final
    public float duration = 0.2f;         // Tiempo de animación
    public Ease ease = Ease.OutBack;       // Tipo de interpolación

    [Header("Opciones de retraso")]
    public float delay = 0f;              // Retraso antes de ejecutar

    [Header("Opcional: sonido al iniciar")]
    public AudioSource audioSource;       // Audio que se reproduce al iniciar

    private TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        PlayStamp();
    }

    // Llamar manualmente si se quiere reiniciar
    public void PlayStamp(float optionalDelay = -1f)
    {
        transform.localScale = Vector3.one * startScale;
        float effectiveDelay = optionalDelay >= 0 ? optionalDelay : delay;

        // Reproducir audio al inicio
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip, AudioListener.volume);
        }

        transform.DOScale(endScale, duration)
                 .SetEase(ease)
                 .SetDelay(effectiveDelay);
    }
}