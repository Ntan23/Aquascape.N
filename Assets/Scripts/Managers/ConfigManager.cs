using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Config
{
    public List<FishSetting> fishSettings;
    public List<TrashSetting> trashSettings;
    public int fishSpawnCount;
    public int trashSpawnCount;

    public Config()
    {
        fishSettings = new List<FishSetting>();
        trashSettings = new List<TrashSetting>();
        fishSpawnCount = 1;
        trashSpawnCount = 1;
    }
}

[Serializable]
public class FishSetting
{
    public string fishName;
    public float minSpeed;
    public float maxSpeed;
    public float detectionRadius;
    public float hungerCooldown;
    public float hungerMeter;

    public FishSetting()
    {
        fishName = string.Empty;
        minSpeed = 2.0f;
        maxSpeed = 5.0f;
        detectionRadius = 5.0f;
        hungerCooldown = 2.0f;
        hungerMeter = 50.0f;
    }
}

[Serializable]
public class TrashSetting
{
    public string trashName;
    public float minSpeed;
    public float maxSpeed;

    public TrashSetting()
    {
        trashName = string.Empty;
        minSpeed = 1.0f;
        maxSpeed = 3.0f;
    }
}

public class ConfigManager
{
    public Config currentConfig;

    private Dictionary<string, FishSetting> fishSettingsDictionary;
    private Dictionary<string, TrashSetting> trashSettingsDictioanry;

    public void Init()
    {
        fishSettingsDictionary = new Dictionary<string, FishSetting>();
        trashSettingsDictioanry = new Dictionary<string, TrashSetting>();

        LoadConfig();
    }

    public void LoadConfig()
    {
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string jsonPath = Path.Combine(rootPath, "config.json");

        Debug.Log("Path Config: " + jsonPath);

        if (File.Exists(jsonPath))
        {
            string jsonString = File.ReadAllText(jsonPath);
            currentConfig = JsonUtility.FromJson<Config>(jsonString);
            
            Debug.Log("Config Berhasil Di-Update");
        }
        else
        {
            Debug.LogWarning("Config file tidak ditemukan! Membuat config default...");

            currentConfig = new Config();
            
            string defaultJson = JsonUtility.ToJson(currentConfig, true);
            string directoryPath = Path.GetDirectoryName(jsonPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(jsonPath, defaultJson);
            
            Debug.Log("Config default berhasil dibuat di: " + jsonPath);
        }

        PrepareFishSettings();
        PrepareTrashSettings();
    }

    private void PrepareFishSettings()
    {
        fishSettingsDictionary.Clear();

        List<FishSetting> currentFishSettings = currentConfig.fishSettings;
        
        foreach (FishSetting fishSettings in currentFishSettings) 
        {
            string fishName = fishSettings.fishName.ToUpper();
            fishSettingsDictionary[fishName] = fishSettings;
        }
    }

    private void PrepareTrashSettings()
    {
        trashSettingsDictioanry.Clear();

        List<TrashSetting> currentTrashSettings = currentConfig.trashSettings;

        foreach (TrashSetting trashSettings in currentTrashSettings)
        {
            string trashName = trashSettings.trashName.ToUpper();
            trashSettingsDictioanry[trashName] = trashSettings;
        }
    }

    public FishSetting GetFishSettings(string name)
    {
        string fishName = name.ToUpper();

        if (fishSettingsDictionary.TryGetValue(fishName, out FishSetting fishSetting))
        {
            return fishSetting;
        }
        
        Debug.LogWarning($"Data untuk {name} tidak ketemu di config!");
        return null; 
    }

    public TrashSetting GetTrashSettings(string name)
    {
        string trashName = name.ToUpper();

        if (trashSettingsDictioanry.TryGetValue(trashName, out TrashSetting trashSetting))
        {
            return trashSetting;
        }

        Debug.LogWarning($"Data untuk {name} tidak ketemu di config!");
        return null; 
    }
}


