using UnityEngine;
using UnityEngine.Pool;

public abstract class SpawnableObject<T> : MonoBehaviour where T : class
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private IObjectPool<T> pool;
    protected string objName;

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

    public void ReturnToPool()
    {
        pool.Release(this as T);
    }

    public void SetName(string name)
    {
        objName = name;
    }
}