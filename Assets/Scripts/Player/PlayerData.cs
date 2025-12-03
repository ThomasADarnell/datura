using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;
    public int maxHealth = 4;
    public int currentHealth = 4;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }
}
