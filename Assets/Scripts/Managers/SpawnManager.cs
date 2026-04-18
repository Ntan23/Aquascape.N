using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

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
    private List<Food> activeFoods;

    private List<Sprite> availableSprites;

    [Header("Button")]
    [SerializeField] private Button spawnRandomButton;

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
        activeFoods = new List<Food>();

        availableSprites = new List<Sprite>();
        
        gameManager = GameManager.instance;
        configManager = gameManager.configManager;

        currentConfig = configManager.currentConfig;

        spawnRandomButton.onClick.RemoveAllListeners();
        spawnRandomButton.onClick.AddListener(OnSpawnRandomButtonClicked);

        RefreshSpawnArea();
    }

    public void SpawnFish(Sprite sprite)
    {
        Fish newFish = fishPool.Get();
        newFish.SetSprite(sprite);

        if (!availableSprites.Contains(sprite))
        {
            availableSprites.Add(sprite);
        }

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

        if (!availableSprites.Contains(sprite))
        {
            availableSprites.Add(sprite);
        }

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

        if (!activeFoods.Contains(newFood)) 
        {
            activeFoods.Add(newFood);
        }
    }

    public void SpawnRandomObject()
    {
        int availableSpritesCount = availableSprites.Count;
        
        if (availableSpritesCount > 0)
        {
            int randomIndex = Random.Range(0, availableSpritesCount);
     
            Sprite randomSprite = availableSprites[randomIndex];
            string spriteName = randomSprite.name.ToLower();

            if (spriteName.StartsWith("fish_"))
            {
                SpawnFish(randomSprite);
            }
            else if (spriteName.StartsWith("trash_"))
            {
                SpawnTrash(randomSprite);
            }
        }
    }

    private IEnumerator SpawnRandomCooldownRoutine()
    {
        spawnRandomButton.interactable = false;
        yield return new WaitForSeconds(1.0f);
        spawnRandomButton.interactable = true;
    }

    private void OnSpawnRandomButtonClicked()
    {
        if (!IsAquariumFull())
        {
            SpawnRandomObject();
            StartCoroutine(SpawnRandomCooldownRoutine());
        }
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
        
        int fishCount = activeFishes.Values.Sum(list => list.Count);
        int trashCount = activeTrashes.Values.Sum(list => list.Count);

        int totalCount = fishCount + trashCount;

        if (totalCount >= maxObjectToSpawn)
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
        float halfWidth = currentConfig.spawnSetting.spawnAreaWidth / 2.0f;
        float halfHeight = currentConfig.spawnSetting.spawnAreaHeight / 2.0f;

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

                    SpriteRenderer fishSpriteRenderer = fish.GetSpriteRenderer();

                    float currentFishXAbsPos = Mathf.Abs(fish.transform.position.x);
                    float currentFishYAbsPos = Mathf.Abs(fish.transform.position.y);

                    float widthOffset = fishSpriteRenderer.bounds.extents.x;
                    float heightOffset = fishSpriteRenderer.bounds.extents.y;

                    float safeRangeX = halfWidth - widthOffset;
                    float safeRangeY = halfHeight - heightOffset;

                    if (currentFishXAbsPos >= safeRangeX || currentFishYAbsPos >= safeRangeY)
                    {
                        fish.transform.position = GetSafeSpawnPosition(fishSpriteRenderer);

                        fish.PickRandomPosition();
                    }
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

                    SpriteRenderer trashSpriteRenderer = trash.GetSpriteRenderer();

                    float currentTrashXAbsPos = Mathf.Abs(trash.transform.position.x);
                    float currentTrashYAbsPos = Mathf.Abs(trash.transform.position.y);

                    float widthOffset = trashSpriteRenderer.bounds.extents.x;
                    float heightOffset = trashSpriteRenderer.bounds.extents.y;

                    float safeRangeX = halfWidth - widthOffset;
                    float safeRangeY = halfHeight - heightOffset;

                    if (currentTrashXAbsPos >= safeRangeX || currentTrashYAbsPos >= safeRangeY)
                    {
                        trash.transform.position = ClampPosition(trash.transform.position, safeRangeX, safeRangeY);
                    }
                }
            }
        }

        for (int i = activeFoods.Count - 1; i >= 0; i--)
        {
            Food food = activeFoods[i]; 
            food.Init(this, currentConfig);
            food.ApplySettings();

            float currentFoodXAbsPos = Mathf.Abs(food.transform.position.x);
            float currentFoodYAbsPos = Mathf.Abs(food.transform.position.y);

            if (currentFoodXAbsPos >= halfWidth || currentFoodYAbsPos >= halfHeight)
            {
                ReleaseFood(food);
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

    private Vector3 ClampPosition(Vector3 pos, float width, float height)
    {
        return new Vector3(
            Mathf.Clamp(pos.x, -width, width),
            Mathf.Clamp(pos.y, -height, height),
            pos.z
        );
    }

    public void DeleteSprites(string spriteName)
    {
        Sprite sprite = availableSprites.FirstOrDefault(sprite => sprite.name == spriteName);

        if (sprite != null)
        {
            availableSprites.Remove(sprite);
        }
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
        activeFoods.Remove(food);  
        food.SetHasLanded(false);

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
