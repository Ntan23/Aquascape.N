using System;
using UnityEngine;

public class Trash : SpawnableObject
{
    private TrashSetting trashSetting;
    private float upperBound;
    private float lowerBound;
    private float currentSpeed;
    private float currentSwayFrequency;
    private float currentSwayAmplitude;
    private bool hasLanded;
    private bool isFloatingUp;
    private bool isInitialized;


    public override void Init(Config config, TrashSetting setting)
    {
        trashSetting = setting;
        currentConfig = config;

        SetCollider();

        // Debug.Log("Min Speed : " + trashSetting.minSpeed);
        // Debug.Log("Max Speed : " + trashSetting.maxSpeed);

        currentSpeed = trashSetting.minSpeed;
        currentSwayFrequency = trashSetting.minSwayFrequency;
        currentSwayAmplitude = trashSetting.minSwayAmplitude;

        upperBound = currentConfig.spawnSetting.spawnAreaHeight / 2.0f;
        lowerBound = -currentConfig.spawnSetting.spawnAreaHeight / 2.0f;

        isFloatingUp = false;
        hasLanded = false;

        if (!isInitialized)
        {
            objCollider.isTrigger = false;
            isInitialized = true;
        }
    }

    void Update()
    {
        if (trashSetting == null || !isInitialized) 
        {
            return;
        }

        //Sway Frequency => Kecepatan (Tempo Swaynya), Sway Amplitude => Jarak (Lebar Swaynya)
        float targetSpeed = isFloatingUp ? trashSetting.maxSpeed : trashSetting.minSpeed;
        float targetSwayFrequency = isFloatingUp ? trashSetting.maxSwayFrequency : trashSetting.minSwayFrequency;
        float targetSwayAmplitude = isFloatingUp ? trashSetting.maxSwayAmplitude : trashSetting.minSwayAmplitude;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);
        currentSwayFrequency = Mathf.Lerp(currentSwayFrequency, targetSwayFrequency, Time.deltaTime * 5f);
        currentSwayAmplitude = Mathf.Lerp(currentSwayAmplitude, targetSwayAmplitude, Time.deltaTime * 5f);

        float sway = Mathf.Sin(Time.time * currentSwayFrequency) * currentSwayAmplitude; 
        transform.position += new Vector3(sway * Time.deltaTime, 0, 0);

        if (!hasLanded && !isFloatingUp)
        {
            transform.Translate(Vector3.down * currentSpeed * Time.deltaTime);

            if (transform.position.y < lowerBound + 0.5f)
            {
                hasLanded = true;
            }
        }

        if (isFloatingUp)
        {
            transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
            
            if (transform.position.y >= upperBound + 0.5f)
            {
                isInitialized = false;
                ReturnTrashToPool(this);
            }
        }
    }

    void OnMouseDown()
    {
        if(!isFloatingUp) 
        {
            isFloatingUp = true;
            objCollider.isTrigger = true;
        }
    }

    public string GetTrashName()
    {
        if (trashSetting != null)
        {
            return trashSetting.trashName;
        }

        return objName;
    }
}
