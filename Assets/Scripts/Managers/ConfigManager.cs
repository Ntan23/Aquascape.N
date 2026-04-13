using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Config
{
    public List<FishSetting> fishSettings;
    public List<TrashSetting> trashSettings;
    public SpawnSetting spawnSetting;
    public float objColliderSizeMultiplier;

    public Config()
    {
        fishSettings = new List<FishSetting>();
        trashSettings = new List<TrashSetting>();
        spawnSetting = new SpawnSetting();
        objColliderSizeMultiplier = 0.7f;
    }
}

[Serializable]
public class FishSetting
{
    public string fishName;
    public float minSpeed;
    public float maxSpeed;
    public float foodDetectionRadius;
    public float obstacleDetectionRadius;
    public float hungerCooldown;

    public FishSetting()
    {
        fishName = string.Empty;
        minSpeed = 2.0f;
        maxSpeed = 5.0f;
        foodDetectionRadius = 10.0f;
        obstacleDetectionRadius = 4.0f;
        hungerCooldown = 10.0f;
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

[Serializable]
public class SpawnSetting
{
    public float spawnAreaWidth;
    public float spawnAreaHeight;
    public int maxObjectToSpawn;
    public float spawnSpacing;

    public SpawnSetting()
    {
        spawnAreaWidth = 30.0f;
        spawnAreaHeight = 20.0f;
        maxObjectToSpawn = 10;
        spawnSpacing = 1.3f;
    }
}

public class ConfigManager
{
    public Config currentConfig;

    private Dictionary<string, FishSetting> fishSettingsDictionary;
    private Dictionary<string, TrashSetting> trashSettingsDictionary;

    public void Init()
    {
        fishSettingsDictionary = new Dictionary<string, FishSetting>();
        trashSettingsDictionary = new Dictionary<string, TrashSetting>();

        LoadConfig();
    }

    public void LoadConfig()
    {
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string configPath = Path.Combine(rootPath, "config.json");

        if (File.Exists(configPath))
        {
            string jsonString = File.ReadAllText(configPath);
            currentConfig = JsonUtility.FromJson<Config>(jsonString);
            
            Debug.Log("Config Berhasil Di-Load");
        }
        else
        {
            Debug.LogWarning("Config file tidak ditemukan! Membuat config default...");

            currentConfig = new Config();
            
            string defaultJson = JsonUtility.ToJson(currentConfig, true);
            string directoryPath = Path.GetDirectoryName(configPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(configPath, defaultJson);
            
            Debug.Log("Config default berhasil dibuat di: " + configPath);
        }

        PrepareFishSettings();
        PrepareTrashSettings();
    }

    public void SaveConfig()
    {
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string configPath = Path.Combine(rootPath, "config.json");

        string jsonString = JsonUtility.ToJson(currentConfig, true);
        File.WriteAllText(configPath, jsonString);
        
        Debug.Log("<color=green>Config JSON berhasil di-update!</color>");
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
        trashSettingsDictionary.Clear();

        List<TrashSetting> currentTrashSettings = currentConfig.trashSettings;

        foreach (TrashSetting trashSettings in currentTrashSettings)
        {
            string trashName = trashSettings.trashName.ToUpper();
            trashSettingsDictionary[trashName] = trashSettings;
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

        FishSetting newSetting = new FishSetting();
        newSetting.fishName = fishName;
        currentConfig.fishSettings.Add(newSetting);
        fishSettingsDictionary.Add(fishName, newSetting);

        SaveConfig();

        return newSetting;
    }

    public TrashSetting GetTrashSettings(string name)
    {
        string trashName = name.ToUpper();

        if (trashSettingsDictionary.TryGetValue(trashName, out TrashSetting trashSetting))
        {
            return trashSetting;
        }

        Debug.LogWarning($"Data untuk {name} tidak ketemu di config!");

        TrashSetting newSetting = new TrashSetting();
        newSetting.trashName = trashName;
        currentConfig.trashSettings.Add(newSetting);
        trashSettingsDictionary.Add(trashName, newSetting);

        SaveConfig();

        return newSetting; 
    }
}


