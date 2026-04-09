using UnityEngine;

public class Trash : SpawnableObject<Trash>
{
    private TrashSetting trashSetting;

    public void ApplySettings(TrashSetting setting)
    {
        trashSetting = setting;
    }
}
