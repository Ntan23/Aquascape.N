using System.Collections;
using UnityEngine;

public class PlayerInput
{
    private float lastFoodTime;
    private Vector2 mousePos;
    ConfigManager configManager;
    SpawnManager spawnManager;

    public void Init(GameManager gameManager)
    {  
        configManager = gameManager.configManager;
        spawnManager = SpawnManager.instance;
    }

    public void DoUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

            float halfWidth = configManager.currentConfig.spawnSetting.spawnAreaWidth / 2f;
            float halfHeight = configManager.currentConfig.spawnSetting.spawnAreaHeight / 2f;

            bool isInsideX = mousePos.x >= -halfWidth && mousePos.x <= halfWidth;
            bool isInsideY = mousePos.y >= -halfHeight && mousePos.y <= halfHeight;

            if (isInsideX && isInsideY)
            {
                HandleInteraction(mousePos);
            }
            else
            {
                Debug.LogWarning("Klik di luar aquarium / spawn area , abaikan!");
            }
        }
    }

    void HandleInteraction(Vector2 mousePos)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Fish"))
            {
                Fish fish = hit.collider.GetComponent<Fish>();
                fish.ScareFish();
                return;
            }
            else if (hit.collider.CompareTag("Trash"))
            {
                Trash trash = hit.collider.GetComponent<Trash>();
                trash.FloatTrash();
                return;
            }
        }
        
        //Spawn Food
        float foodDelay = configManager.currentConfig.foodSetting.foodDelay;

        if (Time.time >= lastFoodTime + foodDelay)
        {
            spawnManager.SpawnFood(mousePos);
            lastFoodTime = Time.time;
        }
    }
}
