using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

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
    }

    private Camera cam;
    private float targetZoom;
    private float zoomSpeed;
    private float lerpSpeed = 10.0f;
    private float minZoom = 10.0f; 
    private float maxZoom;
    
    private GameManager gameManager;
    private Config currentConfig;

    public void Start()
    {
        gameManager = GameManager.instance;

        cam = Camera.main;

        RefreshSetting();

        maxZoom = currentConfig.spawnSetting.spawnAreaHeight / 2.0f;

        if (maxZoom <= minZoom)
        {
            maxZoom = minZoom;
        }

        targetZoom = maxZoom;
        cam.orthographicSize = targetZoom;
    }

    public void HandleCameraZoom(float scrollValue)
    {
        zoomSpeed = currentConfig.cameraSetting.zoomSpeed;

        targetZoom -= scrollValue * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * lerpSpeed);

        ClampCameraPosition();
    }

    public void HandleCameraPanningMovement(float xValue, float yValue)
    {
        Vector3 newPos = transform.position + new Vector3(xValue, yValue, 0);

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float limitX = (currentConfig.spawnSetting.spawnAreaWidth / 2f) - camWidth;
        float limitY = (currentConfig.spawnSetting.spawnAreaHeight / 2f) - camHeight;

        limitX = Mathf.Max(limitX, 0);
        limitY = Mathf.Max(limitY, 0);

        newPos.x = Mathf.Clamp(newPos.x, -limitX, limitX);
        newPos.y = Mathf.Clamp(newPos.y, -limitY, limitY);

        transform.position = newPos;
        
        ClampCameraPosition();
    }

    public void ClampCameraPosition()
    {
        float halfAreaWidth = currentConfig.spawnSetting.spawnAreaWidth / 2f;
        float halfAreaHeight = currentConfig.spawnSetting.spawnAreaHeight / 2f;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float maxX = halfAreaWidth - camWidth;
        float maxY = halfAreaHeight - camHeight;

        maxX = Mathf.Max(maxX, 0);
        maxY = Mathf.Max(maxY, 0);

        float minX = -maxX;
        float minY = -maxY;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void RefreshSetting()
    {
        currentConfig = gameManager.configManager.currentConfig;

        maxZoom = currentConfig.spawnSetting.spawnAreaHeight / 2.0f;

        if (cam.orthographicSize > maxZoom)
        {
            cam.orthographicSize = maxZoom;
        }
        
        ClampCameraPosition();
    }
}
