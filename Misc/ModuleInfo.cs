using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        { "Ship", typeof(ShipData) },
        { "ShipLoadout", typeof(ShipLoadoutData) }
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
        { typeof(ShipLoadoutData), d => AssetManager.Instance.AddShipLoadoutData((ShipLoadoutData)d)   }
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
        var _baseData = JsonConvert.DeserializeObject<DataDefinition>(json);

        if (_baseData == null || string.IsNullOrEmpty(_baseData.dataType))
        {
            return null;
        }

        if (!typeMap.TryGetValue(_baseData.dataType, out var targetType))
        {
            return null;
        }

        return (DataDefinition)JsonConvert.DeserializeObject(json, targetType);
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
}
