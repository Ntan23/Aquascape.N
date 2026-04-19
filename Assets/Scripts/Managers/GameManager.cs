using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ConfigManager configManager;
    public PlayerInput playerInput;
    private AudioManager audioManager;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem clickParticlesEffect;

    [Header("UI")]
    [SerializeField] private Button mainMenuButton;
    
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

    void Start()
    {
        audioManager = AudioManager.instance;

        audioManager.PlaySFX("BubblesInWater");

        mainMenuButton.onClick.AddListener(OnClickMainMenu);
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

    private void OnClickMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
