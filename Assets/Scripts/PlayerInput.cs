using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput
{
    private float lastFoodTime;
    private Vector2 mousePos;
    GameManager gameManager;
    AudioManager audioManager;
    ConfigManager configManager;
    SpawnManager spawnManager;
    CameraManager cameraManager;

    public void Init(GameManager gameManager)
    {  
        this.gameManager = gameManager;
        spawnManager = SpawnManager.instance;
        cameraManager = CameraManager.instance;
        audioManager = AudioManager.instance;
        
        configManager = gameManager.configManager;
    }

    public void DoUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

            float halfWidth = configManager.currentConfig.spawnSetting.spawnAreaWidth / 2f;
            float halfHeight = configManager.currentConfig.spawnSetting.spawnAreaHeight / 2f;

            bool isInsideX = mousePos.x >= -halfWidth && mousePos.x <= halfWidth;
            bool isInsideY = mousePos.y >= -halfHeight && mousePos.y <= halfHeight;

            if (isInsideX && isInsideY)
            {
                gameManager.PlayClickParticleEffect(mousePos);
                audioManager.PlaySFX("BubblePop");
                HandleInteraction(mousePos);
            }
            else
            {
                Debug.LogWarning("Klik di luar aquarium / spawn area , abaikan!");
            }
        }

        ///Camera
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        if (scrollValue != 0)
        { 
            cameraManager.HandleCameraZoom(scrollValue); 
        }

        if (Input.GetMouseButton(2)) 
        {
            float camPanningSpeed = configManager.currentConfig.cameraSetting.panningSpeed;

            float moveX = Input.GetAxis("Mouse X") * camPanningSpeed * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * camPanningSpeed * Time.deltaTime;

            cameraManager.HandleCameraPanningMovement(-moveX, -moveY);
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
