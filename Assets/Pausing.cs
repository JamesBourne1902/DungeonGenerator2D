using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pausing : MonoBehaviour
{
    public KeyCode pauseKey;
    public GameObject pauseMenu;

    private void Start()
    {
        Time.timeScale = 1.0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            PauseMenu();
        }
    }

    private void PauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Time.timeScale = (pauseMenu.activeSelf == true) ? 0f : 1f;
    }
}
