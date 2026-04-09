using UnityEngine;

public class Fish : SpawnableObject<Fish>
{
    private FishSetting fishSetting;

    public void ApplySettings(FishSetting setting)
    {
        fishSetting = setting;
    }
}
