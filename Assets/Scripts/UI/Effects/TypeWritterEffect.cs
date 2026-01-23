using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Texto")]
    [TextArea] public string fullText;      // Texto completo a mostrar
    public TMP_Text targetText;             // TextMeshPro donde se mostrará

    [Header("Efecto")]
    public float charDuration = 0.05f;      // Duración por carácter
    public AudioSource typeSFX;             // Sonido que se reproduce por cada letra

    [Header("Opciones de números aleatorios")]
    public bool useRandomNumbers = false;   // Si se activa, reemplaza # por números aleatorios
    public int randomDigits = 3;            // Cantidad de números aleatorios

    private string processedText;           // Texto procesado con números aleatorios si corresponde
    private Tween typingTween;

    private int lastCharIndex = 0;          // Para saber cuándo aparece una nueva letra

    public bool IsTyping => typingTween != null && typingTween.IsActive() && typingTween.IsPlaying();

    /// <summary>
    /// Prepara el texto, reemplazando # por números aleatorios si corresponde
    /// </summary>
    public void PrepareText()
    {
        processedText = fullText;

        if (useRandomNumbers && randomDigits > 0)
        {
            System.Random rnd = new System.Random();
            string number = "";
            for (int i = 0; i < randomDigits; i++)
                number += rnd.Next(0, 10).ToString();

            processedText = processedText.Replace("#", number);
        }
    }

    /// <summary>
    /// Ejecuta el efecto de escritura
    /// </summary>
    public void PlayTypeEffect(Action onComplete = null)
    {
        if (targetText == null) return;

        PrepareText();            // Asegura que el texto está listo
        targetText.text = "";
        lastCharIndex = 0;
        typingTween?.Kill();

        int charCount = processedText.Length;

        typingTween = DOTween.To(() => 0, val =>
        {
            int currentLength = val;
            targetText.text = processedText.Substring(0, currentLength);

            // Reproducir el SFX solo cuando aparece una nueva letra
            if (typeSFX != null && currentLength > lastCharIndex)
            {
                typeSFX.PlayOneShot(typeSFX.clip, AudioListener.volume);
                lastCharIndex = currentLength;
            }
        }, charCount, charDuration * charCount)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            typingTween = null;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Salta la animación y muestra todo inmediatamente
    /// </summary>
    public void Skip()
    {
        if (typingTween != null && typingTween.IsActive())
        {
            typingTween.Kill();
            typingTween = null;
        }

        if (targetText != null)
            targetText.text = processedText;
    }
}