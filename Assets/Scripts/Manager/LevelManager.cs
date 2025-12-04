using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static string lastLevelName = "Menu"; // Default to Main Menu scene

    [Header("Level 2 Win Screen Images")]
    public GameObject level2WinImage1;
    public GameObject level2WinImage2;

    void Start() 
    {
        // Check if we're on the Win Screen after completing Level 2
        if (SceneManager.GetActiveScene().name == "Win Screen" && lastLevelName == "2D Level 2")
        {
            EnableLevel2WinImages();
        }
        else
        {
            DisableLevel2WinImages();
        }
    }
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

    public void LevelEvil()
    {
        LoadLevel("Lvl2");
    }

    public void MainMenu()
    {
        LoadLevel("Menu");
    }

    public void NextLevel()
    {
        // After Level 2, go back to main menu
        if (lastLevelName == "2D Level 2")
        {
            MainMenu();
        }
        // After Level 1, go to Level 2
        else if (lastLevelName == "Lvl1" || lastLevelName == "Lvl1-3d")
        {
            LevelTwo();
        }
        // Default: go to Level 1
        else
        {
            LevelOne();
        }
    }

    private void EnableLevel2WinImages()
    {
        if (level2WinImage1 != null)
            level2WinImage1.SetActive(true);
        if (level2WinImage2 != null)
            level2WinImage2.SetActive(true);
    }

    private void DisableLevel2WinImages()
    {
        if (level2WinImage1 != null)
            level2WinImage1.SetActive(false);
        if (level2WinImage2 != null)
            level2WinImage2.SetActive(false);
    }
}