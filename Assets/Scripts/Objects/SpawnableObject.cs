using UnityEngine;
using UnityEngine.Pool;

public abstract class SpawnableObject<T> : MonoBehaviour where T : class
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CapsuleCollider2D objCollider;
    private IObjectPool<T> pool;
    protected string objName;

    protected GameManager gameManager;
    protected Config currentConfig;

    public void Init()
    {
        gameManager = GameManager.instance;
        currentConfig = gameManager.configManager.currentConfig;
    }

    public void SetPool(IObjectPool<T> pool)
    {
        this.pool = pool;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
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

    public void ReturnToPool()
    {
        pool.Release(this as T);
    }

    public void SetName(string name)
    {
        objName = name;
    }
}