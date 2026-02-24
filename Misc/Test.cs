using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<ModuleInfo> modInfos;

    public Ship controlledShip;

    public List<SpriteData> spriteVariants;
    public List<FactionData> factions;
    public List<TurretData> turretTypes;
    public List<ShipData> shipTypes;
    public List<SoundData> soundDatas;

    public List<TargetFilter> targets;

    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        var _am = AssetManager.Instance;

        _am.LoadModuleInfos();

        _am.LoadModule("core");

        var _spriteVariants = _am.spriteDatas.Values;
        foreach (var _spriteVariant in _spriteVariants)
        {
            spriteVariants.Add(_spriteVariant);
        }
        var _soundDatas = _am.soundDatas.Values;
        foreach (var _soundData in _soundDatas)
        {
            soundDatas.Add(_soundData);
        }
        var _factions = _am.factionDatas.Values;
        foreach (var _faction in _factions)
        {
            factions.Add(_faction);
        }
        var _turretTypes = _am.turretDatas.Values;
        foreach (var _type in _turretTypes)
        {
            turretTypes.Add(_type);
        }
        var _shipTypes = _am.shipDatas.Values;
        foreach (var _type in _shipTypes)
        {
            shipTypes.Add(_type);
        }
        modInfos = AssetManager.Instance.moduleInfos;
        targets = TargetFilter.targets;
    }

    void Update()
    {
    }

    public void SaveIntoJson(object _object, string _path)
    {
        string json = JsonUtility.ToJson(_object, true);
        System.IO.File.WriteAllText(Application.dataPath + "/" + _path, json);
        Debug.Log("exporting: " + Application.dataPath + "/" + _path);
    }
}
