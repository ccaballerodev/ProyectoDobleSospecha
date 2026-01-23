using UnityEngine;
using UnityEngine.InputSystem; // Nuevo Input System

public class PauseMenuController : MonoBehaviour
{
    [Header("Panel de pausa global")]
    public GenericSlidingPanel pausePanel;  // Este es el panel que cubre toda la pantalla

    [Header("MenuManager de la escena")]
    public MenuManager menuManager;        // Referencia al MenuManager
    public GameObject pauseMenuInManager;  // El subpanel de pausa dentro del MenuManager

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeMenu();
            else ShowMenu();
        }
    }

    /// <summary>
    /// Muestra el menú de pausa
    /// </summary>
    void ShowMenu()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning("[PauseMenuController] pausePanel no asignado");
            return;
        }

        isPaused = true;

        // 1?? Activar el panel global y animarlo
        pausePanel.gameObject.SetActive(true);
        pausePanel.SlideIn();

        // 2?? Avisar al MenuManager cuál submenú está activo
        if (menuManager != null && pauseMenuInManager != null)
        {
            menuManager.SetCurrentMenu(pauseMenuInManager);
        }
    }

    /// <summary>
    /// Oculta el menú de pausa
    /// </summary>
    public void ResumeMenu()
    {
        if (pausePanel == null)
            return;

        isPaused = false;

        // Slide out y desactivar al completar
        pausePanel.SlideOut(() =>
        {
            pausePanel.gameObject.SetActive(false);
        });
    }
}