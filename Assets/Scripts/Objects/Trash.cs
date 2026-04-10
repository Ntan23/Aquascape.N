using UnityEngine;

public class Trash : SpawnableObject<Trash>
{
    private TrashSetting trashSetting;

    public void ApplySettings(TrashSetting setting)
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
