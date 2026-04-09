using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ConfigManager configManager;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        configManager = new ConfigManager();
        configManager.Init();
    }
}
