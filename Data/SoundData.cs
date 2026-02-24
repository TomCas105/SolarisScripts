using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public record SoundData : DataDefinition
{
    public float volume = 1f;
    public float pitch = 1f;
    public bool loop = false;
    public float spatialBlend = 1f; //0 ~ 2D, 1~3D
    public float maxDistance = 75f;
    public List<string> soundFiles;

    public List<AudioClip> LoadedSounds { get; private set; }

    public SoundData()
    {
        LoadedSounds = new List<AudioClip>();
    }


    public void LoadSounds(string _modulePath)
    {
        foreach (var _sound in soundFiles)
        {
            LoadSoundFile(_modulePath + "\\Sounds\\" + _sound);
        }
    }

    private async void LoadSoundFile(string _filePath)
    {
        if (!File.Exists(_filePath))
        {
            AssetManager.Log("File not found: " + _filePath, AssetManager.LOG_ERROR);
            return;
        }

        AssetManager.Log("Loading: " + _filePath, AssetManager.LOG_NORMAL);

        string uri = "file://" + _filePath;

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN);

        if (request.downloadHandler is DownloadHandlerAudioClip dh)
        {
            dh.streamAudio = false;
            dh.compressed = false;
        }

        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            AssetManager.Log($"Loading failed: {_filePath} | {request.error}", AssetManager.LOG_ERROR);
            return;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            AssetManager.Log("AudioClip is null: " + _filePath, AssetManager.LOG_ERROR);
            return;
        }

        clip.name = id;

        LoadedSounds.Add(clip);
    }
}
