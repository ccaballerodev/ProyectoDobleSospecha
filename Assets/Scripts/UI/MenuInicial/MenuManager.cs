using UnityEngine;
using System;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias a los menús")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject playMenu;
    public GameObject pauseMenu; // opcional, si tienes menú de pausa

    private GameObject currentMenu;

    void Start()
    {
        if (mainMenu != null) ShowMenu(mainMenu);
        else currentMenu = pauseMenu;
    }

    // ---------------- Ocultar todos los menús ----------------
    void HideAll()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        playMenu.SetActive(false);
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    // ---------------- Mostrar un menú ----------------
    public void ShowMenu(GameObject nextMenu)
    {
        if (currentMenu == nextMenu) return;

        // Animación de salida del menú actual
        if (currentMenu != null)
        {
            var tween = currentMenu.GetComponent<GenericSlidingPanel>();
            if (tween != null)
            {
                tween.SlideOut(() =>
                {
                    currentMenu.SetActive(false);
                    ActivateMenu(nextMenu);
                });
                return; // Sale del método, el siguiente menú se activa en el callback
            }

            // Si no tiene GenericSlidingPanel, simplemente oculta
            currentMenu.SetActive(false);
        }

        ActivateMenu(nextMenu);
    }

    // ---------------- Activar menú ----------------
    void ActivateMenu(GameObject menu)
    {
        menu.SetActive(true);
        currentMenu = menu;

        // Llama a la animación de entrada si existe GenericSlidingPanel
        var tween = menu.GetComponent<GenericSlidingPanel>();
        tween?.SlideIn();
    }

    // ---------------- Métodos públicos para botones ----------------
    public void ShowMainMenu() => ShowMenu(mainMenu);
    public void ShowOptionsMenu() => ShowMenu(optionsMenu);
    public void ShowPlayMenu() => ShowMenu(playMenu);
    public void ShowPauseMenu() => ShowMenu(pauseMenu);

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }

    public void SetCurrentMenu(GameObject menu)
    {
        currentMenu = menu;
    }
}