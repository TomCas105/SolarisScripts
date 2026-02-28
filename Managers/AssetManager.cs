using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class AssetManager
{
    public static uint LOG_NORMAL = 0;
    public static uint LOG_ERROR = 1;
    public static uint LOG_WARNING = 2;

    private static readonly AssetManager instance = new AssetManager();
    public static AssetManager Instance => instance;

    public static void Log(string log, uint logType)
    {
        switch (logType)
        {
            case 0:
                Debug.Log(log);
                break;
            case 1:
                Debug.LogError(log);
                break;
            case 2:
                Debug.LogWarning(log);
                break;
            default:
                break;
        }

    }

    public static Texture2D LoadTexture(string _path)
    {
        Log("loading: " + _path.Replace("/", "\\"), LOG_NORMAL);
        byte[] _fileData;

        if (File.Exists(_path))
        {
            _fileData = File.ReadAllBytes(_path);
            Texture2D _texture = new Texture2D(2, 2);
            _texture.wrapMode = TextureWrapMode.Clamp;
            if (_texture.LoadImage(_fileData))
            {
                return _texture;
            }
        }
        Log("failed to load: " + _path.Replace("/", "\\"), LOG_ERROR);
        return null;
    }

    private Dictionary<string, Material> loadedMaterials;

    public List<ModuleInfo> moduleInfos;
    public Dictionary<string, SpriteData> spriteDataRegistry;
    public Dictionary<string, SoundData> soundDataRegistry;
    public Dictionary<string, EffectData> effectDataRegistry;
    public Dictionary<string, BuffData> buffDataRegistry;
    public Dictionary<string, FactionData> factionDataRegistry;
    public Dictionary<string, ShieldData> shieldDataRegistry;
    public Dictionary<string, ArmorData> armorDataRegistry;
    public Dictionary<string, TurretData> turretDataRegistry;
    public Dictionary<string, EquipmentData> equipmentDataRegistry;
    public Dictionary<string, ShipData> shipDataRegistry;
    public Dictionary<string, ShipLoadoutData> shipLoadoutDataRegistry;
    public Dictionary<string, FactionLoadoutSetData> factionLoadoutSetDataRegistry;

    private AssetManager()
    {
        moduleInfos = new();
        spriteDataRegistry = new();
        soundDataRegistry = new();
        effectDataRegistry = new();
        buffDataRegistry = new();
        factionDataRegistry = new();
        shieldDataRegistry = new();
        armorDataRegistry = new();
        turretDataRegistry = new();
        equipmentDataRegistry = new();
        shipDataRegistry = new();
        shipLoadoutDataRegistry = new();
        factionLoadoutSetDataRegistry = new();

        loadedMaterials = new();
    }

    public Material GetMaterial(string _material)
    {
        if (loadedMaterials.ContainsKey(_material))
        {
            return loadedMaterials[_material];
        }
        else
        {
            var _newMaterial = Resources.Load("TrailMaterial", typeof(Material)) as Material;
            loadedMaterials.Add(_material, _newMaterial);
            return _newMaterial;
        }
    }

    public void LoadModuleInfos()
    {
        //Base modules
        var _baseFolders = AssetDatabase.GetSubFolders("Assets/Data/Base");
        foreach (var _folder in _baseFolders)
        {
            DirectoryInfo _dir = new DirectoryInfo(_folder);
            var _files = _dir.GetFiles("*moduleInfo.json");
            foreach (var _file in _files)
            {
                if (_file.Name == "moduleInfo.json")
                {
                    Log("loading: " + _file.FullName, LOG_NORMAL);
                    var _moduleInfo = JsonUtility.FromJson<ModuleInfo>(_file.OpenText().ReadToEnd());

                    if (_moduleInfo != null)
                    {
                        _moduleInfo.ModulePathPrefix = _file.Directory.FullName;
                        _moduleInfo.BaseModule = true;
                        moduleInfos.Add(_moduleInfo);
                    }
                }
            }
        }

        //Mods
        var _modFolders = AssetDatabase.GetSubFolders("Assets/Data/Mods");
        foreach (var _folder in _modFolders)
        {
            DirectoryInfo _dir = new DirectoryInfo(_folder);
            var _files = _dir.GetFiles("*moduleInfo.json");
            foreach (var _file in _files)
            {
                if (_file.Name == "moduleInfo.json")
                {
                    var _moduleInfo = JsonUtility.FromJson<ModuleInfo>(_file.OpenText().ReadToEnd());

                    if (_moduleInfo != null)
                    {
                        _moduleInfo.ModulePathPrefix = _file.Directory.FullName;
                        moduleInfos.Add(_moduleInfo);
                    }
                }
            }
        }
    }

    public void LoadModule(string _moduleID)
    {
        foreach (var module in moduleInfos)
        {
            if (module.id == _moduleID)
            {
                module.LoadModule();
                return;
            }
        }
    }

    public void AddSpriteData(SpriteData _data)
    {
        if (spriteDataRegistry.ContainsKey(_data.id))
        {
            Log("merging sprites: " + _data.id, LOG_NORMAL);
            spriteDataRegistry[_data.id].Merge(_data);
        }
        else
        {
            spriteDataRegistry.Add(_data.id, _data);
        }
    }

    public void AddSoundData(SoundData _data)
    {
        if (soundDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing sound: " + _data.id, LOG_WARNING);
            soundDataRegistry.Remove(_data.id);
        }
        soundDataRegistry.Add(_data.id, _data);
    }

    public void AddEffectData(EffectData _data)
    {
        if (effectDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing effect: " + _data.id, LOG_WARNING);
            effectDataRegistry.Remove(_data.id);
        }
        effectDataRegistry.Add(_data.id, _data);
    }

    public void AddBuffData(BuffData _data)
    {
        if (buffDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing buff: " + _data.id, LOG_WARNING);
            buffDataRegistry.Remove(_data.id);
        }
        buffDataRegistry.Add(_data.id, _data);
    }

    public void AddFaction(FactionData _data)
    {
        if (factionDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing faction: " + _data.id, LOG_WARNING);
            factionDataRegistry.Remove(_data.id);
        }
        factionDataRegistry.Add(_data.id, _data);
    }

    public void AddShieldData(ShieldData _data)
    {
        if (shieldDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing shield: " + _data.id, LOG_WARNING);
            shieldDataRegistry.Remove(_data.id);
        }
        shieldDataRegistry.Add(_data.id, _data);
    }

    public void AddArmorData(ArmorData _data)
    {
        if (armorDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing armor: " + _data.id, LOG_WARNING);
            armorDataRegistry.Remove(_data.id);
        }
        armorDataRegistry.Add(_data.id, _data);
    }

    public void AddTurretData(TurretData _data)
    {
        if (turretDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing turret: " + _data.id, LOG_WARNING);
            turretDataRegistry.Remove(_data.id);
        }
        turretDataRegistry.Add(_data.id, _data);
    }

    public void AddEquipmentData(EquipmentData _data)
    {
        if (equipmentDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing equipment: " + _data.id, LOG_WARNING);
            equipmentDataRegistry.Remove(_data.id);
        }
        equipmentDataRegistry.Add(_data.id, _data);
    }

    public void AddShipData(ShipData _data)
    {
        if (shipDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing ship: " + _data.id, LOG_WARNING);
            shipDataRegistry.Remove(_data.id);
        }
        shipDataRegistry.Add(_data.id, _data);
    }

    public void AddShipLoadoutData(ShipLoadoutData _data)
    {
        if (shipLoadoutDataRegistry.ContainsKey(_data.id))
        {
            Log("replacing ship loadout: " + _data.id, LOG_WARNING);
            shipLoadoutDataRegistry.Remove(_data.id);
        }
        shipLoadoutDataRegistry.Add(_data.id, _data);
    }
    public void AddFactionLoadoutSetData(FactionLoadoutSetData _data)
    {
        if (shipLoadoutDataRegistry.ContainsKey(_data.id))
        {
            Log("merging faction loadout set: " + _data.id, LOG_NORMAL);
            factionLoadoutSetDataRegistry[_data.id].Merge(_data);
        }
        else
        {
            factionLoadoutSetDataRegistry.Add(_data.id, _data);
        }
    }

    public SoundData GetSoundData(string _id)
    {
        if (soundDataRegistry.ContainsKey(_id))
        {
            return soundDataRegistry[_id];
        }
        return null;
    }

    public EffectData GetEffectData(string _id)
    {
        if (effectDataRegistry.ContainsKey(_id))
        {
            return effectDataRegistry[_id];
        }
        return null;
    }
    public BuffData GetBuffData(string _id)
    {
        if (buffDataRegistry.ContainsKey(_id))
        {
            return buffDataRegistry[_id];
        }
        return null;
    }

    public FactionData GetFaction(string _id)
    {
        if (factionDataRegistry.ContainsKey(_id))
        {
            return factionDataRegistry[_id];
        }
        return null;
    }

    public ShieldData GetShieldData(string _id)
    {
        if (shieldDataRegistry.ContainsKey(_id))
        {
            return shieldDataRegistry[_id];
        }
        return null;
    }

    public ArmorData GetArmorData(string _id)
    {
        if (armorDataRegistry.ContainsKey(_id))
        {
            return armorDataRegistry[_id];
        }
        return null;
    }

    public TurretData GetTurretData(string _id)
    {
        if (turretDataRegistry.ContainsKey(_id))
        {
            return turretDataRegistry[_id];
        }
        return null;
    }

    public EquipmentData GetEquipmentData(string _id)
    {
        if (equipmentDataRegistry.ContainsKey(_id))
        {
            return equipmentDataRegistry[_id];
        }
        return null;
    }

    public ShipData GetShipData(string _id)
    {
        if (shipDataRegistry.ContainsKey(_id))
        {
            return shipDataRegistry[_id];
        }
        return null;
    }

    public ShipLoadoutData GetShipLoadoutData(string _id)
    {
        if (shipLoadoutDataRegistry.ContainsKey(_id))
        {
            return shipLoadoutDataRegistry[_id];
        }
        return null;
    }

    public FactionLoadoutSetData GetFactionLoadoutSetData(string _id)
    {
        if (factionLoadoutSetDataRegistry.ContainsKey(_id))
        {
            return factionLoadoutSetDataRegistry[_id];
        }
        return null;
    }

    public Sprite GetSprite(string _id, string _variant = "")
    {
        if (spriteDataRegistry.ContainsKey(_id))
        {
            return spriteDataRegistry[_id].GetSprite(_variant);
        }
        return null;
    }

    public Sprite GetSprite(string _id, List<string> _variants)
    {
        if (spriteDataRegistry != null && spriteDataRegistry.ContainsKey(_id))
        {
            var _sv = spriteDataRegistry[_id];

            if (_variants == null || _variants.Count == 0)
            {
                return _sv.GetDefaultSprite();
            }

            foreach (var _variant in _variants)
            {
                if (_sv.HasVariant(_variant))
                {
                    return _sv.GetSprite(_variant);
                }
            }

            return _sv.GetDefaultSprite(); ;
        }

        return null;
    }
}
