using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ConfigManager configManager;
    public PlayerInput playerInput;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem clickParticlesEffect;
    
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

    public void PlayClickParticleEffect(Vector2 position)
    {
        clickParticlesEffect.Stop();
        clickParticlesEffect.transform.position = position;
        clickParticlesEffect.Play();
    }
}
