using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    public String gameScene = "Main",
        mainMenuScene = "MainMenu",
        helpScene = "Help",
        objectsScene = "Objects",
        gameplayScene = "Gameplay";
    public void StartApplication()
    {
        SceneManager.LoadScene(gameScene);
    }
    
    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Help()
    {
        SceneManager.LoadScene(helpScene);
    }

    public void Objects()
    {
        SceneManager.LoadScene(objectsScene);
    }

    public void Gameplay()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
