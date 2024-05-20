using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void StartButtonPressed()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
    }

    public void BackToMaimenuButtonPressed()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
