using UnityEngine;

public class Trash : SpawnableObject
{
    private TrashSetting trashSetting;

    public override void Init(Config currentConfig, TrashSetting setting)
    {
        trashSetting = setting;

        Debug.Log("Min Speed : " + trashSetting.minSpeed);
        Debug.Log("Max Speed : " + trashSetting.maxSpeed);
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
