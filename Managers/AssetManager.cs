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
    public Dictionary<string, SpriteData> spriteDatas;
    public Dictionary<string, SoundData> soundDatas;
    public Dictionary<string, EffectData> effectDatas;
    public Dictionary<string, BuffData> buffDatas;
    public Dictionary<string, FactionData> factionDatas;
    public Dictionary<string, ShieldData> shieldDatas;
    public Dictionary<string, ArmorData> armorDatas;
    public Dictionary<string, TurretData> turretDatas;
    public Dictionary<string, EquipmentData> equipmentDatas;
    public Dictionary<string, ShipData> shipDatas;

    private AssetManager()
    {
        moduleInfos = new();
        spriteDatas = new();
        soundDatas = new();
        effectDatas = new();
        buffDatas = new();
        factionDatas = new();
        shieldDatas = new();
        armorDatas = new();
        turretDatas = new();
        equipmentDatas = new();
        shipDatas = new();

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

    public void AddSpriteData(SpriteData _spriteData)
    {
        if (spriteDatas.ContainsKey(_spriteData.id))
        {
            Log("merging sprites: " + _spriteData.id, LOG_WARNING);
            spriteDatas[_spriteData.id].Merge(_spriteData);
        }
        else
        {
            spriteDatas.Add(_spriteData.id, _spriteData);
        }
    }

    public void AddSoundData(SoundData _soundData)
    {
        if (soundDatas.ContainsKey(_soundData.id))
        {
            Log("replacing sound: " + _soundData.id, LOG_WARNING);
            soundDatas.Remove(_soundData.id);
        }
        soundDatas.Add(_soundData.id, _soundData);
    }

    public void AddEffectData(EffectData _effectData)
    {
        if (effectDatas.ContainsKey(_effectData.id))
        {
            Log("replacing effect: " + _effectData.id, LOG_WARNING);
            effectDatas.Remove(_effectData.id);
        }
        effectDatas.Add(_effectData.id, _effectData);
    }

    public void AddBuffData(BuffData _buffData)
    {
        if (buffDatas.ContainsKey(_buffData.id))
        {
            Log("replacing buff: " + _buffData.id, LOG_WARNING);
            buffDatas.Remove(_buffData.id);
        }
        buffDatas.Add(_buffData.id, _buffData);
    }

    public void AddFaction(FactionData _faction)
    {
        if (factionDatas.ContainsKey(_faction.id))
        {
            Log("replacing faction: " + _faction.id, LOG_WARNING);
            factionDatas.Remove(_faction.id);
        }
        factionDatas.Add(_faction.id, _faction);
    }

    public void AddShieldData(ShieldData _shield)
    {
        if (shieldDatas.ContainsKey(_shield.id))
        {
            Log("replacing shield: " + _shield.id, LOG_WARNING);
            shieldDatas.Remove(_shield.id);
        }
        shieldDatas.Add(_shield.id, _shield);
    }

    public void AddArmorData(ArmorData _armor)
    {
        if (armorDatas.ContainsKey(_armor.id))
        {
            Log("replacing armor: " + _armor.id, LOG_WARNING);
            armorDatas.Remove(_armor.id);
        }
        armorDatas.Add(_armor.id, _armor);
    }

    public void AddTurretData(TurretData _turret)
    {
        if (turretDatas.ContainsKey(_turret.id))
        {
            Log("replacing turret: " + _turret.id, LOG_WARNING);
            turretDatas.Remove(_turret.id);
        }
        turretDatas.Add(_turret.id, _turret);
    }

    public void AddEquipmentData(EquipmentData _equipment)
    {
        if (equipmentDatas.ContainsKey(_equipment.id))
        {
            Log("replacing equipment: " + _equipment.id, LOG_WARNING);
            equipmentDatas.Remove(_equipment.id);
        }
        equipmentDatas.Add(_equipment.id, _equipment);
    }

    public void AddShipData(ShipData _ship)
    {
        if (shipDatas.ContainsKey(_ship.id))
        {
            Log("replacing ship: " + _ship.id, LOG_WARNING);
            shipDatas.Remove(_ship.id);
        }
        shipDatas.Add(_ship.id, _ship);
    }

    public SoundData GetSoundData(string _id)
    {
        if (soundDatas.ContainsKey(_id))
        {
            return soundDatas[_id];
        }
        return null;
    }

    public EffectData GetEffectData(string _id)
    {
        if (effectDatas.ContainsKey(_id))
        {
            return effectDatas[_id];
        }
        return null;
    }
    public BuffData GetBuffData(string _id)
    {
        if (buffDatas.ContainsKey(_id))
        {
            return buffDatas[_id];
        }
        return null;
    }

    public FactionData GetFaction(string _id)
    {
        if (factionDatas.ContainsKey(_id))
        {
            return factionDatas[_id];
        }
        return null;
    }

    public ShieldData GetShieldData(string _id)
    {
        if (shieldDatas.ContainsKey(_id))
        {
            return shieldDatas[_id];
        }
        return null;
    }

    public ArmorData GetArmorData(string _id)
    {
        if (armorDatas.ContainsKey(_id))
        {
            return armorDatas[_id];
        }
        return null;
    }

    public TurretData GetTurretData(string _id)
    {
        if (turretDatas.ContainsKey(_id))
        {
            return turretDatas[_id];
        }
        return null;
    }

    public EquipmentData GetEquipmentData(string _id)
    {
        if (equipmentDatas.ContainsKey(_id))
        {
            return equipmentDatas[_id];
        }
        return null;
    }

    public ShipData GetShipData(string _id)
    {
        if (shipDatas.ContainsKey(_id))
        {
            return shipDatas[_id];
        }
        return null;
    }

    public Sprite GetSprite(string _id, string _variant = "")
    {
        if (spriteDatas.ContainsKey(_id))
        {
            return spriteDatas[_id].GetSprite(_variant);
        }
        return null;
    }

    public Sprite GetSprite(string _id, List<string> _variants)
    {
        if (spriteDatas != null && spriteDatas.ContainsKey(_id))
        {
            var _sv = spriteDatas[_id];
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
