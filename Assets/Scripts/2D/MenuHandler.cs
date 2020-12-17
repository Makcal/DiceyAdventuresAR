using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    public String gameSceneDev = "Main",
	    gameSceneAR = "AR",
        mainMenuScene = "MainMenu",
        helpScene = "Help",
        objectsScene = "Objects",
        gameplayScene = "Gameplay";
    public void StartApplication()
    {
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			SceneManager.LoadScene(gameSceneDev);
		else
			SceneManager.LoadScene(gameSceneAR);
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
