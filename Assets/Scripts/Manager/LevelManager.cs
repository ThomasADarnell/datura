using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static string lastLevelName = "Menu"; // Default to Main Menu scene

    void Start() { }
    void Update() { }

    public void LoadLevel(string levelName)
    {
        if (levelName != "Kill Screen")
        {
            lastLevelName = levelName;
        }

        SceneManager.LoadScene(levelName);
    }
    public void RetryLevel()
    {
        SceneManager.LoadScene(lastLevelName);
    }

    public void LevelOne()
    {
        LoadLevel("Lvl1");
    }

    public void LevelTwo()
    {
        LoadLevel("2D Level 2");
    }
}