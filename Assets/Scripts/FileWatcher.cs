using UnityEngine;
using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections;
using UnityEngine.Networking;

public class FileWatcher : MonoBehaviour
{
    private string spawnablesFolderPath;
    private string configFolderPath;
    private ConcurrentQueue<Action> actionsQueue;

    private FileSystemWatcher spawnablesFileSystemWatcher;
    private FileSystemWatcher configFileSystemWatcher;

    private SpawnManager spawnManager;
    private ConfigManager configManager;

    void Start() 
    {
        actionsQueue = new ConcurrentQueue<Action>();
        spawnablesFolderPath = Path.Combine(Application.dataPath, "../Spawnables");
        configFolderPath = Path.Combine(Application.dataPath, "..");

        spawnManager = SpawnManager.instance;
        configManager = GameManager.instance.configManager;

        LoadFiles();
        
        if (!Directory.Exists(spawnablesFolderPath))
        {
            Directory.CreateDirectory(spawnablesFolderPath);
        } 

        if (!Directory.Exists(configFolderPath))
        {
            Directory.CreateDirectory(configFolderPath);
        }
        
        //Spawnables
        spawnablesFileSystemWatcher = new FileSystemWatcher();
        spawnablesFileSystemWatcher.Path = spawnablesFolderPath;
        spawnablesFileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        spawnablesFileSystemWatcher.Filter = "*.png"; 
        spawnablesFileSystemWatcher.Created += OnSpawnablesFileCreated;
        spawnablesFileSystemWatcher.EnableRaisingEvents = true;

        //Config
        configFileSystemWatcher = new FileSystemWatcher();
        configFileSystemWatcher.Path = configFolderPath;
        configFileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        configFileSystemWatcher.Filter = "config.json";
        configFileSystemWatcher.Changed += OnConfigFileChanged;
        configFileSystemWatcher.EnableRaisingEvents = true;
    }

    void Update() 
    {
        if (actionsQueue.TryDequeue(out Action action)) 
        {
            action?.Invoke();
        }
    }

    void OnDisable()
    {
        if (spawnablesFileSystemWatcher != null)
        {
            spawnablesFileSystemWatcher.EnableRaisingEvents = false;
            spawnablesFileSystemWatcher.Dispose();
        }

        if (configFileSystemWatcher != null)
        {
            configFileSystemWatcher.EnableRaisingEvents = false;
            configFileSystemWatcher.Dispose();
        }
    }

    private void OnSpawnablesFileCreated(object sender, FileSystemEventArgs e)
    {
        actionsQueue.Enqueue(() =>
        {
            StartCoroutine(LoadTexture(e.FullPath)); 
        });
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        actionsQueue.Enqueue(() =>
        {
            configManager.LoadConfig();
            spawnManager.RefreshAllObjects();
        });
    }

    private void LoadFiles()
    {
        if (Directory.Exists(spawnablesFolderPath))
        {
            string[] files = Directory.GetFiles(spawnablesFolderPath, "*.png");

            foreach (string file in files)
            {
                StartCoroutine(LoadTexture(file));
            }
        }
    }

    IEnumerator LoadTexture(string path) 
    {
        yield return new WaitForSeconds(0.2f);
        using (UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture("file://" + path)) 
        {
            yield return unityWebRequest.SendWebRequest();
            
            if (unityWebRequest.result == UnityWebRequest.Result.Success) 
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(unityWebRequest);
                string fileName = Path.GetFileNameWithoutExtension(path);
                
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = fileName;

                string fileNameLower = fileName.ToLower();

                if (fileNameLower.Contains("fish")) 
                {
                   spawnManager.SpawnFish(sprite);
                }
                else if (fileNameLower.Contains("trash")) 
                {
                    spawnManager.SpawnTrash(sprite);
                }
            }
        }
    }

    public void OpenSpawnablesFolder()
    {
        if (!Directory.Exists(spawnablesFolderPath))
        {
            Directory.CreateDirectory(spawnablesFolderPath);
        }

        string winPath = spawnablesFolderPath.Replace("/", "\\");
        
        System.Diagnostics.Process.Start("explorer.exe", winPath);
    }

    public void OpenConfigInNotepad()
    {
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string configPath = Path.Combine(rootPath, "config.json");

        if (File.Exists(configPath))
        {
            System.Diagnostics.Process.Start("notepad.exe", configPath);
        }
    }
}
