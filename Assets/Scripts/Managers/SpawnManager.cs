using UnityEngine;
using UnityEngine.Pool;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    [SerializeField] private BoxCollider2D spawnArea; 

    [Header("Prefabs")]
    [SerializeField] private Fish fishPrefab;
    [SerializeField] private Trash trashPrefab;

    private IObjectPool<Fish> fishPool;
    private IObjectPool<Trash> trashPool;

    private GameManager gameManager;
    private ConfigManager configManager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Setup Fish Pool
        fishPool = new ObjectPool<Fish>(
            createFunc: () => CreateFish(),
            actionOnGet: (fish) => OnGetFish(fish),
            actionOnRelease: (fish) => OnReturnFish(fish),
            actionOnDestroy: (fish) => OnDestroyFish(fish),
            collectionCheck: true, 
            defaultCapacity: 5, 
            maxSize: 50
        );

        trashPool = new ObjectPool<Trash>(
            createFunc: () => CreateTrash(),
            actionOnGet: (trash) => OnGetTrash(trash),
            actionOnRelease: (trash) => OnReturnTrash(trash),
            actionOnDestroy: (trash) => OnDestroyTrash(trash),
            collectionCheck: true, 
            defaultCapacity: 5, 
            maxSize: 50
        );
    }

    void Start()
    {
        gameManager = GameManager.instance;
        configManager = gameManager.configManager;
    }

    public void SpawnFish(Sprite sprite)
    {
        Fish newFish = fishPool.Get();
        newFish.SetSprite(sprite);
        newFish.transform.position = Vector3.zero;

        string fileName = sprite.name;
        string fishName = GetObjectName(fileName);

        FishSetting fishSetting = configManager.GetFishSettings(fishName);
    
        newFish.ApplySettings(fishSetting);
    }

    public void SpawnTrash(Sprite sprite)
    {
        Trash newTrash = trashPool.Get();
        newTrash.SetSprite(sprite);
        newTrash.transform.position = Vector3.zero;

        string fileName = sprite.name;
        string trashName = GetObjectName(fileName);

        TrashSetting trashSetting = configManager.GetTrashSettings(trashName);
    
        newTrash.ApplySettings(trashSetting);
    }

    private string GetObjectName(string fileName)
    {
        if (fileName.Contains("_"))
        {
            string[] parts = fileName.Split('_');
            if (parts.Length > 1)
            {
                string category = parts[0]; 
                string name = parts[1];
                
                Debug.Log($"Kategori: {category}, Nama: {name}");
                
                return name; 
            }
        }
        
        return fileName;
    }


    #region FishPoolFunction
    private Fish CreateFish()
    {
        Fish fish = Instantiate(fishPrefab, transform);
        fish.SetPool(fishPool);

        return fish;
    }

    private void OnGetFish(Fish fish)
    {
        fish.gameObject.SetActive(true);
    }

    private void OnReturnFish(Fish fish)
    {
        fish.gameObject.SetActive(false);
    }

    private void OnDestroyFish(Fish fish)
    {
        Destroy(fish.gameObject);
    }

    public void ReleaseFish(Fish fish) 
    {
        fishPool.Release(fish);
    }
    #endregion

    #region TrashPoolFunction
    private Trash CreateTrash()
    {
        Trash trash = Instantiate(trashPrefab, transform);
        trash.SetPool(trashPool);

        return trash;
    }

    private void OnGetTrash(Trash trash)
    {
        trash.gameObject.SetActive(true);
    }

    private void OnReturnTrash(Trash trash)
    {
        trash.gameObject.SetActive(false);
    }

    private void OnDestroyTrash(Trash trash)
    {
        Destroy(trash.gameObject);
    }

    public void ReleaseTrash(Trash trash) 
    {
        trashPool.Release(trash);
    }
    #endregion
}
