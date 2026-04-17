using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ConfigManager configManager;
    public PlayerInput playerInput;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem bubbleParticles;
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
    }

    void Start()
    {
        playerInput = new PlayerInput();
        playerInput.Init(this);

        SetupBubbles();
    }

    void Update()
    {
        if (playerInput != null)
        {
            playerInput.DoUpdate();
        }
    }

    public void SetupBubbles()
    {
        float lowerBound = -configManager.currentConfig.spawnSetting.spawnAreaHeight / 2.0f;

        bubbleParticles.transform.position = new Vector3(0, lowerBound, 0);
        var particlesShape = bubbleParticles.shape;
        particlesShape.radius = configManager.currentConfig.spawnSetting.spawnAreaWidth / 2.0f;

        bubbleParticles.gameObject.SetActive(true);
    }

    public void PlayClickParticleEffect(Vector2 position)
    {
        clickParticlesEffect.Stop();
        clickParticlesEffect.transform.position = position;
        clickParticlesEffect.Play();
    }
}
