using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ModuleInfo
{
    private static readonly Dictionary<string, Type> typeMap = new()
    {
        { "Sprite", typeof(SpriteData) },
        { "Sound", typeof(SoundData) },
        { "Effect", typeof(EffectData) },
        { "Buff", typeof(BuffData) },
        { "Faction", typeof(FactionData) },
        { "Shield", typeof(ShieldData) },
        { "Armor", typeof(ArmorData) },
        { "Turret", typeof(TurretData) },
        { "Equipment", typeof(EquipmentData) },
        { "Ship", typeof(ShipData) }
    };

    private static readonly Dictionary<Type, Action<DataDefinition>> registryMap = new()
    {
        { typeof(SpriteData), d => AssetManager.Instance.AddSpriteData((SpriteData)d) },
        { typeof(SoundData), d => AssetManager.Instance.AddSoundData((SoundData)d) },
        { typeof(EffectData), d => AssetManager.Instance.AddEffectData((EffectData)d) },
        { typeof(BuffData), d => AssetManager.Instance.AddBuffData((BuffData)d) },
        { typeof(FactionData), d => AssetManager.Instance.AddFaction((FactionData)d) },
        { typeof(ShieldData), d => AssetManager.Instance.AddShieldData((ShieldData)d) },
        { typeof(ArmorData), d => AssetManager.Instance.AddArmorData((ArmorData)d) },
        { typeof(TurretData), d => AssetManager.Instance.AddTurretData((TurretData)d) },
        { typeof(EquipmentData), d => AssetManager.Instance.AddEquipmentData((EquipmentData)d) },
        { typeof(ShipData), d => AssetManager.Instance.AddShipData((ShipData)d) },
    };

    public string id = "id";
    public string name = "module";
    public string author = "author";
    public string description = "";
    public string version = "";

    public bool BaseModule { get; set; } = false;
    public bool Enabled { get; set; } = true;
    public string ModulePathPrefix;

    public void LoadModule()
    {
        string _path = "Assets" + ModulePathPrefix.Substring(Application.dataPath.Length) + "/";

        LoadData(_path + "Defs");

        /*
        LoadData<SpriteData>(_path + "Sprites", AssetManager.Instance.AddSpriteData);
        LoadData<SoundData>(_path + "Sounds", AssetManager.Instance.AddSoundData);
        LoadData<EffectData>(_path + "Effects", AssetManager.Instance.AddEffectData);
        LoadData<BuffData>(_path + "Buffs", AssetManager.Instance.AddBuffData);
        LoadData<FactionData>(_path + "Factions", AssetManager.Instance.AddFaction);
        LoadData<ShieldData>(_path + "Equipment/Shields", AssetManager.Instance.AddShieldData);
        LoadData<ArmorData>(_path + "Equipment/Armors", AssetManager.Instance.AddArmorData);
        LoadData<TurretData>(_path + "Equipment/Turrets", AssetManager.Instance.AddTurretData);
        LoadData<EquipmentData>(_path + "Equipment/Equipment", AssetManager.Instance.AddEquipmentData);
        LoadData<ShipData>(_path + "Ships", AssetManager.Instance.AddShipData);
        */
    }

    private FileInfo[] GetJSONFiles(string _directory)
    {
        DirectoryInfo _dir = new DirectoryInfo(_directory);
        return _dir.GetFiles("*.json");
    }

    private string[] GetAllSubfolders(string _directory)
    {
        List<string> _allFolders = new();

        var _folders = AssetDatabase.GetSubFolders(_directory);
        foreach (var _folder in _folders)
        {
            _allFolders.AddRange(GetAllSubfolders(_folder));
        }

        _allFolders.Add(RelativePathToFullPath((_directory + "\\").Replace("/", "\\")));

        return _allFolders.ToArray();
    }

    private string RelativePathToFullPath(string _s)
    {
        var _path = Application.dataPath + (_s).Substring(("Assets").Length);
        return _path;
    }

    private void LoadData(string folderPath)
    {
        foreach (var _folder in GetAllSubfolders(folderPath))
        {
            var _jsons = GetJSONFiles(_folder);

            foreach (var _json in _jsons)
            {
                AssetManager.Log("loading: " + _json.FullName, AssetManager.LOG_NORMAL);

                try
                {
                    string json = File.ReadAllText(_json.FullName);

                    DataDefinition data = DeserializeJSON(json);

                    if (data == null)
                    {
                        AssetManager.Log("failed: " + _json.FullName, AssetManager.LOG_ERROR);
                        continue;
                    }

                    Register(data);
                }
                catch (Exception e)
                {
                    AssetManager.Log(
                        $"exception while loading {_json.FullName}: {e.Message}",
                        AssetManager.LOG_ERROR
                    );
                }
            }
        }
    }

    private DataDefinition DeserializeJSON(string json)
    {
        DataDefinition baseData = JsonUtility.FromJson<DataDefinition>(json);

        if (baseData == null || string.IsNullOrEmpty(baseData.dataType))
        {
            Debug.LogError("Invalid or missing dataType.");
            return null;
        }

        if (!typeMap.TryGetValue(baseData.dataType, out Type targetType))
        {
            Debug.LogError($"Unknown dataType: {baseData.dataType}");
            return null;
        }

        DataDefinition finalData = (DataDefinition)JsonUtility.FromJson(json, targetType);

        return finalData;
    }

    private void Register(DataDefinition data)
    {
        if (registryMap.TryGetValue(data.GetType(), out var action))
        {
            action.Invoke(data);
        }
        else
        {
            AssetManager.Log($"Missing registry for type {data.GetType().Name}", AssetManager.LOG_ERROR);
        }
    }

    private void LoadData<T>(string folderPath, Action<T> registerCallback)
    {
        foreach (var folder in GetAllSubfolders(folderPath))
        {
            var jsons = GetJSONFiles(folder);

            foreach (var json in jsons)
            {
                AssetManager.Log("loading: " + json.FullName, AssetManager.LOG_NORMAL);

                try
                {
                    using var sr = json.OpenText();
                    var data = JsonUtility.FromJson<T>(sr.ReadToEnd());

                    if (data == null)
                    {
                        AssetManager.Log("loading failed: " + json.FullName, AssetManager.LOG_ERROR);
                        continue;
                    }

                    registerCallback.Invoke(data);
                }
                catch
                {
                    AssetManager.Log("loading failed: " + json.FullName, AssetManager.LOG_ERROR);
                }
            }
        }
    }
}
