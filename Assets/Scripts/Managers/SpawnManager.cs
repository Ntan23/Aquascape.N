using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [Header("Prefabs")]
    [SerializeField] private Fish fishPrefab;
    [SerializeField] private Trash trashPrefab;
    [SerializeField] private Food foodPrefab;

    [Header("Spawn Area / Aquarium")]
    [SerializeField] private SpriteRenderer spawnAreaSpriteRenderer;
    private float spawnAreaWidth;
    private float spawnAreaHeight;

    private IObjectPool<Fish> fishPool;
    private IObjectPool<Trash> trashPool;
    private IObjectPool<Food> foodPool;

    private Dictionary<string, List<Fish>> activeFishes;
    private Dictionary<string, List<Trash>> activeTrashes;

    private Config currentConfig;
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

        foodPool = new ObjectPool<Food>(
            createFunc: () => CreateFood(),
            actionOnGet: (food) => OnGetFood(food),
            actionOnRelease: (food) => OnReturnFood(food),
            actionOnDestroy: (food) => OnDestroyFood(food),
            collectionCheck: true, 
            defaultCapacity: 5, 
            maxSize: 50
        );
    }

    void Start()
    {
        activeFishes = new Dictionary<string, List<Fish>>();
        activeTrashes = new Dictionary<string, List<Trash>>();
        
        gameManager = GameManager.instance;
        configManager = gameManager.configManager;

        currentConfig = configManager.currentConfig;

        RefreshSpawnArea();
    }

    public void SpawnFish(Sprite sprite)
    {
        Fish newFish = fishPool.Get();
        newFish.SetSprite(sprite);

        SpriteRenderer newFishSpriteRenderer = newFish.GetSpriteRenderer();
        newFish.transform.position = GetSafeSpawnPosition(newFishSpriteRenderer);

        string fileName = sprite.name;
        string fishName = GetObjectName(fileName);

        newFish.SetName(fishName);

        if (!activeFishes.ContainsKey(fishName))
        {
            activeFishes[fishName] = new List<Fish>();
        }
    
        activeFishes[fishName].Add(newFish);

        FishSetting fishSetting = configManager.GetFishSettings(fishName);
        newFish.ApplySettings(fishSetting);
    }

    public void SpawnTrash(Sprite sprite)
    {
        Trash newTrash = trashPool.Get();
        newTrash.SetSprite(sprite);

        SpriteRenderer newTrashSpriteRenderer = newTrash.GetSpriteRenderer();
        newTrash.transform.position = GetSafeSpawnPosition(newTrashSpriteRenderer);

        string fileName = sprite.name;
        string trashName = GetObjectName(fileName);

        newTrash.SetName(trashName);

        if (!activeTrashes.ContainsKey(trashName))
        {
            activeTrashes[trashName] = new List<Trash>();
        }

        activeTrashes[trashName].Add(newTrash);

        TrashSetting trashSetting = configManager.GetTrashSettings(trashName);
        newTrash.ApplySettings(trashSetting);
    }

    public void SpawnFood(Vector2 pos)
    {
        Food newFood = foodPool.Get();
        newFood.ApplySettings();

        newFood.transform.position = pos;
    }

    public Vector3 GetSafeSpawnPosition(SpriteRenderer spriteRenderer)
    {
        float widthOffset = spriteRenderer.bounds.extents.x;
        float heightOffset = spriteRenderer.bounds.extents.y;

        float safeRangeX = (currentConfig.spawnSetting.spawnAreaWidth / 2f) - widthOffset;
        float safeRangeY = (currentConfig.spawnSetting.spawnAreaHeight / 2f) - heightOffset;

        Vector2 detectionSize = spriteRenderer.bounds.size * currentConfig.spawnSetting.spawnSpacing;

        int maxAttempts = 15; 
        
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(-safeRangeX, safeRangeX);
            float randomY = Random.Range(-safeRangeY, safeRangeY);
            Vector3 randomPos = new Vector3(randomX, randomY, 0);

            Collider2D hit = Physics2D.OverlapBox(randomPos, detectionSize, 0f);

            if (hit == null)
            {
                return randomPos;
            }
        }

        Debug.LogWarning("Gagal nemu posisi aman, area mungkin terlalu penuh!");
        return Vector3.zero;
    }

    private string GetObjectName(string fileName)
    {
        if (fileName.Contains("_"))
        {
            string[] parts = fileName.Split('_');
            if (parts.Length > 1)
            {
                string name = parts[1];
                
                return name; 
            }
        }
        
        return fileName;
    }

    public bool IsAquariumFull()
    {
        int maxObjectToSpawn = currentConfig.spawnSetting.maxObjectToSpawn;
        int currentObjCount = activeFishes.Count + activeTrashes.Count;

        if (currentObjCount > maxObjectToSpawn)
        {
            return true;
        }

        return false;
    }

    public void RefreshConfig()
    {
        currentConfig = configManager.currentConfig;
    }

    public void RefreshAllObjects()
    {
        foreach (var list in activeFishes.Values)
        {
            foreach (Fish fish in list)
            {
                string fishName = fish.GetFishName();
                FishSetting fishSetting = configManager.GetFishSettings(fishName);
                
                if (fishSetting != null)
                {
                    fish.Init(this, currentConfig);
                    fish.ApplySettings(fishSetting);
                }
            }
        }

        foreach (var list in activeTrashes.Values)
        {
            foreach (Trash trash in list)
            {
                string trashName = trash.GetTrashName();
                TrashSetting trashSetting = configManager.GetTrashSettings(trashName);

                if (trashSetting != null)
                {
                    trash.Init(this, currentConfig);
                    trash.ApplySettings(trashSetting);
                }
            }
        }
    }

    public void RefreshSpawnArea()
    {
        spawnAreaWidth = currentConfig.spawnSetting.spawnAreaWidth;
        spawnAreaHeight = currentConfig.spawnSetting.spawnAreaHeight;

        Vector2 spawnAreaSize = new Vector2(spawnAreaWidth, spawnAreaHeight);

        spawnAreaSpriteRenderer.size = spawnAreaSize;
    }

    #region FishPoolFunction
    private Fish CreateFish()
    {
        Fish fish = Instantiate(fishPrefab, transform);
        fish.Init(this, currentConfig);

        return fish;
    }

    private void OnGetFish(Fish fish)
    {
        fish.gameObject.SetActive(true);
    }

    private void OnReturnFish(Fish fish)
    {
        fish.gameObject.SetActive(false);

        string fishName = fish.GetFishName();
        
        if (activeFishes.ContainsKey(fishName))
        {
            activeFishes[fishName].Remove(fish);
        }
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
        trash.Init(this, currentConfig);

        return trash;
    }

    private void OnGetTrash(Trash trash)
    {
        trash.gameObject.SetActive(true);
    }

    private void OnReturnTrash(Trash trash)
    {
        trash.gameObject.SetActive(false);

        string trashName = trash.GetTrashName();
        
        if (activeTrashes.ContainsKey(trashName))
        {
            activeTrashes[trashName].Remove(trash);
        }
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
    
    #region FoodPoolFunction
    private Food CreateFood()
    {
        Food food = Instantiate(foodPrefab, transform);
        food.Init(this, currentConfig);

        return food;
    }

    private void OnGetFood(Food food)
    {
        food.gameObject.SetActive(true);
    }

    private void OnReturnFood(Food food)
    {
        food.gameObject.SetActive(false);
    }

    private void OnDestroyFood(Food food)
    {
        Destroy(food.gameObject);
    }

    public void ReleaseFood(Food food) 
    {
        foodPool.Release(food);
    }
    #endregion
}
