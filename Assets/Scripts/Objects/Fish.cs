using UnityEngine;

public class Fish : SpawnableObject<Fish>
{
    private FishSetting fishSetting;

    public void ApplySettings(FishSetting setting)
    {
        fishSetting = setting;

        Debug.Log("Min Speed : " + fishSetting.minSpeed);
        Debug.Log("Max Speed : " + fishSetting.maxSpeed);

        SetCollider();
    }

    public string GetFishName()
    {
        if (fishSetting != null)
        {
            return fishSetting.fishName;
        }

        return objName;
    }
}
