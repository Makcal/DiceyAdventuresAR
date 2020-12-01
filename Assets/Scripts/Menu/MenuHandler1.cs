using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler1 : MonoBehaviour
{
    public String gameSceneName;
    public void StartApplication()
    {
        SceneManager.LoadScene(gameSceneName);
    }  
    
    public void BackToMenu()
    {
        SceneManager.LoadScene("0");
    }

    public void Options()
    {
        SceneManager.LoadScene("2");
    }

    public void Objects()
    {
        SceneManager.LoadScene("3");
    }

    public void Gameplay()
    {
        SceneManager.LoadScene("4");
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
