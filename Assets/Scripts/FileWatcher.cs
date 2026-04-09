using UnityEngine;
using System.IO;
using System.Collections.Concurrent;
using System.Collections;
using UnityEngine.Networking;

public class FileWatcher : MonoBehaviour
{
    private string filePath;
    private ConcurrentQueue<string> fileQueue;

    private FileSystemWatcher fileSystemWatcher;
    private SpawnManager spawnManager;

    void Start() 
    {
        fileQueue = new ConcurrentQueue<string>();
        filePath = Path.Combine(Application.dataPath, "../Spawnables");

        spawnManager = SpawnManager.instance;

        LoadFiles();
        
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        } 
            
        fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = filePath;
        fileSystemWatcher.Filter = "*.png"; 
        fileSystemWatcher.Created += OnFileCreated;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    void Update() 
    {
        if (fileQueue.TryDequeue(out string path)) 
        {
            StartCoroutine(LoadTexture(path));
        }
    }

    void OnDisable()
    {
        if (fileSystemWatcher != null)
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Dispose();
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        fileQueue.Enqueue(e.FullPath);
    }

    private void LoadFiles()
    {
        if (Directory.Exists(filePath))
        {
            string[] files = Directory.GetFiles(filePath, "*.png");

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
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        string winPath = filePath.Replace("/", "\\");
        
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
