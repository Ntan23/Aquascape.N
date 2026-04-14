using System.Collections;
using UnityEngine;

public class Fish : SpawnableObject
{
    private FishSetting fishSetting;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask foodLayer;

    private Vector3 targetPosition;
    private float currentSpeed;
    private float currentHungerMeter;
    private bool isScared;
    private bool isAvoiding;
    private bool isSearchingFood;
    private bool isFoundFood;
    private bool isInitialized;

    public override void Init(Config config, FishSetting setting)
    {
        fishSetting = setting;
        currentConfig = config;

        SetCollider();
        
        Debug.Log("Min Speed : " + fishSetting.minSpeed);
        Debug.Log("Max Speed : " + fishSetting.maxSpeed);

        if (!isInitialized)
        {
            PickRandomPosition();
            currentSpeed = fishSetting.minSpeed;
            currentHungerMeter = 100;
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

    void OnMouseDown()
    {
        if (!isScared)
        {
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
        Vector2 moveDirection = (targetPosition - transform.position).normalized;
        RaycastHit2D obstacleRaycastHit = Physics2D.Raycast(transform.position, moveDirection, fishSetting.obstacleDetectionRadius, obstacleLayer);

        if (obstacleRaycastHit.collider != null)
        {
            if (!isAvoiding)
            {
                StartCoroutine(SearchNewRandomPosition());
            }
            return;
        }

        if (isSearchingFood)
        {
            Collider2D foodCollider = Physics2D.OverlapCircle(transform.position, fishSetting.foodDetectionRadius, foodLayer);

            if (foodCollider != null)
            {
                targetPosition = foodCollider.gameObject.transform.position;
                isFoundFood = true;
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

        if (Mathf.Abs(differences) > 0.2f)
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
        }
    }

    public void FillHungerMeter(float amount)
    {
        currentHungerMeter += amount; 
        
        if (currentHungerMeter >= 100)
        {
            currentHungerMeter = 100;
            isSearchingFood = false;
            isFoundFood = false;
            PickRandomPosition();
        }
    }

    public void PickRandomPosition()
    {
        float widthOffset = spriteRenderer.bounds.extents.x;
        float heightOffset = spriteRenderer.bounds.extents.y;

        float safeRangeX = (currentConfig.spawnSetting.spawnAreaWidth / 2f) - widthOffset;
        float safeRangeY = (currentConfig.spawnSetting.spawnAreaHeight / 2f) - heightOffset;

        float randomX = Random.Range(-safeRangeX, safeRangeX);
        float randomY = Random.Range(-safeRangeY,safeRangeY);

        targetPosition = new Vector3(randomX, randomY, 0);
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
        Gizmos.DrawWireSphere(transform.position, fishSetting.obstacleDetectionRadius);

        //Food
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fishSetting.foodDetectionRadius);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Food") && isSearchingFood)
        {
            FillHungerMeter(100);
            PickRandomPosition();

            //Perlu buat return to pool (food)
        }
    }
}
