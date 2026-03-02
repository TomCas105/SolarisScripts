using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public List<ArmorData> armorDatas;
    public List<ShieldData> shieldDatas;
    public List<BuffData> buffDatas;
    public List<EffectData> effectDatas;
    public List<EquipmentData> equipmentDatas;
    public List<ShipLoadoutData> shipLoadoutDatas;

    public List<TargetFilter> targets;

    public Camera mainCamera;
    private int spawnIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        var _am = AssetManager.Instance;

        _am.LoadModuleInfos();

        _am.LoadModule("core");

        spriteVariants = new List<SpriteData>();
        factions = new List<FactionData>();
        turretTypes = new List<TurretData>();
        shipTypes = new List<ShipData>();
        soundDatas = new List<SoundData>();
        armorDatas = new List<ArmorData>();
        shieldDatas = new List<ShieldData>();
        buffDatas = new List<BuffData>();
        effectDatas = new List<EffectData>();
        equipmentDatas = new List<EquipmentData>();
        shipLoadoutDatas = new List<ShipLoadoutData>();

        spriteVariants.AddRange(_am.spriteDataRegistry.Values);
        factions.AddRange(_am.factionDataRegistry.Values);
        turretTypes.AddRange(_am.turretDataRegistry.Values);
        shipTypes.AddRange(_am.shipDataRegistry.Values);
        soundDatas.AddRange(_am.soundDataRegistry.Values);
        armorDatas.AddRange(_am.armorDataRegistry.Values);
        shieldDatas.AddRange(_am.shieldDataRegistry.Values);
        buffDatas.AddRange(_am.buffDataRegistry.Values);
        effectDatas.AddRange(_am.effectDataRegistry.Values);
        equipmentDatas.AddRange(_am.equipmentDataRegistry.Values);
        shipLoadoutDatas.AddRange(_am.shipLoadoutDataRegistry.Values);

        modInfos = AssetManager.Instance.moduleInfos;
        targets = TargetFilter.targets;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 _mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            var _ship = Ship.Instantiate(shipLoadoutDatas[spawnIndex], _mousePos, "pirate");
            _ship.AddComponent<PrimitiveShipAI>();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Vector2 _mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var _ship = Ship.Instantiate(shipLoadoutDatas[spawnIndex], _mousePos, "empire");
            _ship.AddComponent<PrimitiveShipAI>();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            Vector2 _mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var _ship = Ship.Instantiate(shipLoadoutDatas[spawnIndex], _mousePos, "player");
            _ship.AddComponent<PlayerShipInput>();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            spawnIndex = (spawnIndex + 1) % shipLoadoutDatas.Count;
            Debug.Log("Selected loadout: " + shipLoadoutDatas[spawnIndex].id);
        }
    }
}
