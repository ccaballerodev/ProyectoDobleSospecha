using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;          // Slider de la UI

    void Awake()
    {
        // Mantener este objeto entre escenas
        DontDestroyOnLoad(gameObject);

        // Inicializa el slider con el volumen actual
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value; // Ajusta volumen global
    }
}