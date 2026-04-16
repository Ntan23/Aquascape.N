using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ConfigManager configManager;
    public PlayerInput playerInput;
    
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

        playerInput = new PlayerInput();
        playerInput.Init(this);
    }

    void Update()
    {
        if (playerInput != null)
        {
            playerInput.DoUpdate();
        }
    }
}
