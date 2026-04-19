using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fish : SpawnableObject
{
    private FishSetting fishSetting;
    
    [Header("Layer Mask")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask foodLayer;

    [Header("Hunger Meter")]
    [SerializeField] private Canvas hungerMeterCanvas;
    [SerializeField] private Slider hungerMeterSlider;

    private Vector3 targetPosition;
    private GameObject targetedFood;
    private float currentSpeed;
    private float currentHungerMeter;
    private bool isScared;
    private bool isAvoiding;
    private bool isSearchingFood;
    private bool isFoundFood;

    public void ApplySettings(FishSetting setting)
    {
        fishSetting = setting;

        SetCollider();
        
        // Debug.Log("Min Speed : " + fishSetting.minSpeed);
        // Debug.Log("Max Speed : " + fishSetting.maxSpeed);
        currentSpeed = fishSetting.minSpeed;

        if (!isInitialized)
        {
            PickRandomPosition();
            currentHungerMeter = 100;

            if (spriteRenderer != null)
            {
                // Max = titik tengah ditambah setengah dari tinggi
                float maxY = spriteRenderer.bounds.center.y + (spriteRenderer.bounds.size.y / 2f);

                Vector3 targetPosition = hungerMeterCanvas.transform.localPosition;
                targetPosition.y = (maxY - transform.position.y) + 0.2f;
                hungerMeterCanvas.transform.localPosition = targetPosition;
            }

            isInitialized = true;
        }
    }

    void Update()
    {
        if (fishSetting == null || !isInitialized) 
        {
            return;
        }

        CheckSurrondings();
        HandleMovement();
        HandleFacingDirection();
        HandleHunger();
    }

    public void ScareFish()
    {
        if (!isScared)
        {
            audioManager.PlaySFX("FishRunAway");
            StartCoroutine(Scare());
        }
    }

    private IEnumerator Scare()
    {
        isScared = true;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 fleeDirection = (transform.position - mousePos).normalized;

        targetPosition = transform.position + (fleeDirection * 10f);

        yield return new WaitForSeconds(2.0f);
        isScared = false;
        PickRandomPosition();
    }

    public void CheckSurrondings()
    {
        //Obstacle
        float angle = 30f; 
        float obstacleDetectionRadius = fishSetting.obstacleDetectionRadius;
        
        Vector2 moveDirection = (targetPosition - transform.position).normalized;
        Vector2 upperDirection = Quaternion.Euler(0, 0, angle) * moveDirection;
        Vector2 lowerDirection = Quaternion.Euler(0, 0, -angle) * moveDirection;

        RaycastHit2D center = Physics2D.Raycast(transform.position, moveDirection, obstacleDetectionRadius, obstacleLayer);
        RaycastHit2D upper = Physics2D.Raycast(transform.position, upperDirection, obstacleDetectionRadius, obstacleLayer);
        RaycastHit2D lower = Physics2D.Raycast(transform.position, lowerDirection, obstacleDetectionRadius, obstacleLayer);

        if (center.collider != null || upper.collider != null || lower.collider != null)
        {
            if (!isAvoiding)
            {
                StartCoroutine(SearchNewRandomPosition());
            }
        }

        //Food
        if (isSearchingFood)
        {
            if (targetedFood == null || !targetedFood.activeInHierarchy)
            {
                Collider2D foodCollider = Physics2D.OverlapCircle(transform.position, fishSetting.foodDetectionRadius, foodLayer);
                
                if (foodCollider != null && foodCollider.gameObject.activeInHierarchy)
                {
                    targetedFood = foodCollider.gameObject;
                    isFoundFood = true;
                }
                else if (isFoundFood) // Kalau tadi lagi nemu terus tiba-tiba makanannya hilang
                {
                    isFoundFood = false;
                    targetedFood = null;
                    PickRandomPosition();
                }
            }
            else
            {
                isFoundFood = true;
                targetPosition = targetedFood.transform.position;
            }
        }
    }

    IEnumerator SearchNewRandomPosition()
    {
        isAvoiding = true;
        PickRandomPosition();
    
        yield return new WaitForSeconds(1.0f); 
        
        isAvoiding = false;
    }

    public void HandleMovement()
    {
        float targetSpeed;

        if (isScared || (isSearchingFood && isFoundFood))
        {
            targetSpeed = fishSetting.maxSpeed;
        }
        else
        {
            targetSpeed = fishSetting.minSpeed;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            if (isScared) 
            {
                isScared = false;
            }

            PickRandomPosition();
        }
    }

    private void HandleFacingDirection()
    {
        float differences = targetPosition.x - transform.position.x;

        if (Mathf.Abs(differences) > 0.5f)
        {
            if (differences > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);  
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1); 
            }
        }  
        
        Vector3 canvasScale = hungerMeterCanvas.transform.localScale;
        canvasScale.x = Mathf.Abs(canvasScale.x); 
       
        if (transform.localScale.x < 0) 
        {
            canvasScale.x *= -1;
        }
        
        hungerMeterCanvas.transform.localScale = canvasScale;
    }

    public void HandleHunger()
    {
        if (!isSearchingFood)
        {
            currentHungerMeter -= (100.0f / fishSetting.hungerCooldown) * Time.deltaTime;

            if (currentHungerMeter <= 0)
            {
                currentHungerMeter = 0;
                isSearchingFood = true;
            }

            hungerMeterSlider.value = currentHungerMeter;
        }
    }

    public void FillHungerMeter(float amount)
    {
        currentHungerMeter += amount; 
        
        if (currentHungerMeter >= 100)
        {
            currentHungerMeter = 100;
        }

        isSearchingFood = false;
        isFoundFood = false;
        targetedFood = null;
    }

    public void PickRandomPosition()
    {
        float widthOffset = spriteRenderer.bounds.extents.x;
        float heightOffset = spriteRenderer.bounds.extents.y;

        float safeRangeX = (currentConfig.spawnSetting.spawnAreaWidth / 2f) - widthOffset;
        float safeRangeY = (currentConfig.spawnSetting.spawnAreaHeight / 2f) - heightOffset;

        Vector3 potentialTarget;
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            float randomX = Random.Range(-safeRangeX, safeRangeX);
            float randomY = Random.Range(-safeRangeY, safeRangeY);
            potentialTarget = new Vector3(randomX, randomY, 0);

            Collider2D hit = Physics2D.OverlapCircle(potentialTarget, widthOffset, obstacleLayer);
            
            if (hit == null) 
            {
                break;
            } 
            
            attempts++;
        } while (attempts < maxAttempts);

        targetPosition = potentialTarget;
    }

    public string GetFishName()
    {
        if (fishSetting != null)
        {
            return fishSetting.fishName;
        }

        return objName;
    }

    private void OnDrawGizmosSelected()
    {
        if (fishSetting == null) 
        {
            return;
        }

        //Obstacle
        Gizmos.color = Color.red;
        float angle = 30f;
        float range = fishSetting.obstacleDetectionRadius;
        
        Vector2 pos = transform.position;
        Vector2 moveDirection = (targetPosition - (Vector3)pos).normalized;
        
        if (moveDirection == Vector2.zero)
        {
            moveDirection = transform.right; 
        } 

        Vector2 upperDirection = Quaternion.Euler(0, 0, angle) * moveDirection;
        Vector2 lowerDirection = Quaternion.Euler(0, 0, -angle) * moveDirection;

        Gizmos.DrawLine(pos, pos + moveDirection * range);      
        Gizmos.DrawLine(pos, pos + upperDirection * range);      
        Gizmos.DrawLine(pos, pos + lowerDirection * range);

        //Food
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fishSetting.foodDetectionRadius);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Food") && isSearchingFood && isFoundFood)
        {
            Food food = collision.GetComponent<Food>();
            
            if (food != null && food.gameObject.activeInHierarchy) 
            {
                audioManager.PlaySFX("Eat");
                food.ReturnFoodToPool(food);
                FillHungerMeter(100);
                PickRandomPosition();
            }
        }
    }
}
