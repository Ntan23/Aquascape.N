using UnityEngine;
using UnityEngine.Pool;

public abstract class SpawnableObject : MonoBehaviour 
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected CapsuleCollider2D objCollider;
    protected string objName;
    protected bool isInitialized;

    protected Config currentConfig;
    protected SpawnManager spawnManager;

    public virtual void Init(SpawnManager spawnManager, Config config)
    {
        if (this.spawnManager == null)
        {
            this.spawnManager = spawnManager;
        }
        
        currentConfig = config;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public void SetCollider()
    {
        objCollider.size = spriteRenderer.sprite.bounds.size * currentConfig.objColliderSizeMultiplier;

        if (objCollider.size.x > objCollider.size.y) 
        {
            objCollider.direction = CapsuleDirection2D.Horizontal;
        } 
        else 
        {
            objCollider.direction = CapsuleDirection2D.Vertical;
        }
    }

    public void ReturnFishToPool(Fish fish)
    {
        spawnManager.ReleaseFish(fish);
    }

    public void ReturnTrashToPool(Trash trash)
    {
        spawnManager.ReleaseTrash(trash);
    }

    public void ReturnFoodToPool(Food food)
    {
        spawnManager.ReleaseFood(food);
    }

    public void SetName(string name)
    {
        objName = name;
    }
}