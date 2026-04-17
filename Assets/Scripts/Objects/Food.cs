using UnityEngine;

public class Food : SpawnableObject
{
    private FoodSetting foodSetting;
    private float currentSpeed;
    private float lowerBound;
    private bool hasLanded;

    public void ApplySettings()
    {
        foodSetting = currentConfig.foodSetting;

        currentSpeed = foodSetting.speed;
        lowerBound = -currentConfig.spawnSetting.spawnAreaHeight / 2.0f;
        
        if (!isInitialized)
        {
            SetHasLanded(false);
            isInitialized = true;
        }
    }

    void Update()
    {
        if (foodSetting == null || !isInitialized)
        {
            return;
        }

        if (!hasLanded)
        {
            transform.Translate(Vector3.down * currentSpeed * Time.deltaTime);

            float sway = Mathf.Sin(Time.time * foodSetting.swayFrequency) * foodSetting.swayAmplitude; 
            transform.position += new Vector3(sway * Time.deltaTime, 0, 0);

            if (transform.position.y < lowerBound + 0.4f)
            {
                SetHasLanded(true);
            }
        }
    }

    public bool GetHasLanded()
    {
        return hasLanded;
    }

    public void SetHasLanded(bool value)
    {
        hasLanded = value;
    }
}
