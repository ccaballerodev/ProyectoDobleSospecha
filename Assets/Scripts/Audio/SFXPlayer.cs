using UnityEngine;
using UnityEngine.UI;

public class SFXPlayer : MonoBehaviour
{
    private Button button;
    private AudioSource audioSource;

    void Awake()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();

        // Asegurarse de que no se reproduzca automáticamente
        audioSource.playOnAwake = false;

        // Conectar el evento OnClick
        button.onClick.AddListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip, AudioListener.volume);
        }
    }
}