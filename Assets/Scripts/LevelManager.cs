using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LevelOne()
    {
        SceneManager.LoadScene("Lvl1");
    }
    public void LevelTwo()
    {
        SceneManager.LoadScene("Lvl2");
    }
}
