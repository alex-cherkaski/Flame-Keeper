using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.PostProcessing;

public enum PauseOptionType
{
    ReturnToGame,
    RestartLevel,
    ReturnToMenu,
    Quit
}

public class PauseMenuController : MonoBehaviour
{
    public PostProcessingProfile postProcessingProfile;
    public GameObject pauseMenu;
    public List<PauseOptionElement> options;
    public float moveCursorDelay = 0.1f; // How long we wait before accepting another input

    private int selectedIndex = 0;
    private bool isPaused = false;
    private float lastCursorMoveTime = 0.0f;

    private void Start()
    {
        isPaused = false;
        postProcessingProfile.depthOfField.enabled = false;
        Time.timeScale = 1.0f;
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (isPaused && Input.GetButtonDown(StringConstants.Input.PauseConfirm))
        {
            DoAction(options[selectedIndex].type);
        }

        if (isPaused && Input.GetButtonDown(StringConstants.Input.PauseBack))
        {
            SetPaused(false);
        }

        if (isPaused && Mathf.Abs(Input.GetAxis(StringConstants.Input.VerticalMovement)) > 0.5f && Time.unscaledTime - lastCursorMoveTime > moveCursorDelay)
        {
            selectedIndex -= Mathf.RoundToInt(Input.GetAxis(StringConstants.Input.VerticalMovement)); // Input is always from -1 to 1
            selectedIndex = selectedIndex % options.Count; // Loop from bottom to top
            if (selectedIndex < 0) selectedIndex = options.Count - 1; // Loop from top to bottom

            // Update states
            for (int i = 0; i < options.Count; i++)
            {
                options[i].SetState(i == selectedIndex);
            }

            lastCursorMoveTime = Time.unscaledTime;
        }

        if (Input.GetButtonDown(StringConstants.Input.Start))
        {
            SetPaused(!isPaused);
        }
    }

    private void DoAction(PauseOptionType action)
    {
        switch (action)
        {
            case PauseOptionType.ReturnToMenu:
                SetPaused(false);
                FlameKeeper.Get().ResetGame();
                break;
            case PauseOptionType.RestartLevel:
                SetPaused(false);
                FlameKeeper.Get().RestartLevel();
                break;
            case PauseOptionType.Quit:
                FlameKeeper.Get().CloseApplication();
                break;
            default:
            case PauseOptionType.ReturnToGame:
                SetPaused(false);
                break;
        }
    }

    private void SetPaused(bool paused)
    {
        if (isPaused == paused)
            return;

        isPaused = paused;
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        pauseMenu.SetActive(isPaused);
        selectedIndex = 0;
        postProcessingProfile.depthOfField.enabled = isPaused;

        for (int i = 0; i < options.Count; i++)
        {
            options[i].SetState(i == selectedIndex);
        }

        if (isPaused)
        {
            FlameKeeper.Get().levelController.GetPlayer().DisableInput();
        }
        else
        {
            FlameKeeper.Get().levelController.GetPlayer().EnableInput();
        }
    }

    private void OnDestroy()
    {
        // Just so it doesnt mess up your editor
        postProcessingProfile.depthOfField.enabled = false;
    }
}
