using UnityEngine;
using UnityEngine.Pool;

public abstract class SpawnableObject : MonoBehaviour 
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected CapsuleCollider2D objCollider;
    private IObjectPool<Fish> fishPool;
    private IObjectPool<Trash> trashPool;
    protected string objName;

    protected Config currentConfig;

    public virtual void Init(Config config,  FishSetting fishSetting)
    {
        currentConfig = config;
        SetCollider();
    }

    public virtual void Init(Config config, TrashSetting fishSetting)
    {
        currentConfig = config;
        SetCollider();
    }

    public void SetPool(IObjectPool<Fish> pool)
    {
        fishPool = pool;
    }

    public void SetPool(IObjectPool<Trash> pool)
    {
        trashPool = pool;
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
        fishPool.Release(fish);
    }

    public void ReturnTrashToPool(Trash trash)
    {
        trashPool.Release(trash);
    }

    public void SetName(string name)
    {
        objName = name;
    }
}