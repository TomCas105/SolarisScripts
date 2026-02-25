using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class Ship : MonoBehaviour
{
    private const float CREW_HULL_REPAIR_SPEED = 0.003f;
    private const float CREW_BREACH_REPAIR_SPEED = 0.01f;
    private const float HEAT_DAMAGE_PER_SECOND = 0.005f;
    private const float PERCENT_CREW_LOSS_FROM_OXYGEN = 0.01f;

    public enum TurretRangeType

    {
        NORMAL,
        DEFENSIVE,
        SNIPER,
        AGRESSIVE
    }

    public static Ship Instantiate(string _shipID, Vector2 _position, string _factionID = "pirate")
    {
        var _shipType = AssetManager.Instance.GetShipData(_shipID);


        if (_shipType == null)
        {
            return null;
        }

        var _ship = new GameObject(_shipType.id).AddComponent<Ship>();
        _ship.shipType = _shipType;
        _ship.ShipFactionID = _factionID;
        _ship.transform.position = _position;

        return _ship;
    }

    public static Ship Instantiate(ShipLoadoutData shipLoadoutData, Vector2 _position, string _factionID = "pirate")
    {
        var _ship = Instantiate(shipLoadoutData.shipID, _position, _factionID);

        if (_ship == null)
        {
            return null;
        }

        _ship.Armor = AssetManager.Instance.GetArmorData(shipLoadoutData.armorID);
        _ship.CurrentArmorCount = shipLoadoutData.armorCount;
        _ship.Shield = AssetManager.Instance.GetShieldData(shipLoadoutData.shieldID);
        _ship.CurrentShieldCount = shipLoadoutData.shieldCount;

        foreach (var _module in shipLoadoutData.equipment)
        {
            _ship.AddEquipment(_module, false);
        }

        _ship.InitializeShip();

        foreach (var _turret in shipLoadoutData.turrets)
        {
            _ship.SetTurret(_turret.turretID, _turret.hardpoint);
        }

        _ship.InitializeStats();

        return _ship;
    }

    private record HullBreach
    {
        public float size;
        public float duration;

        public HullBreach(float size, float duration)
        {
            this.size = size;
            this.duration = duration;
        }
    }

    [SerializeField]
    protected ShipData shipType;

    //hull
    public float MaxHullHitpoints { get; private set; }
    public float CurrentHullHitpoints { get; private set; }
    public float HullRegeneration { get; private set; }
    public float HullDrain { get; private set; }
    //armor
    public float MaxArmorHitpoints { get; private set; }
    public float CurrentArmorHitpoints { get; private set; }
    public float ArmorThickness { get; private set; }
    public float ArmorRegeneration { get; private set; }
    public float ArmorQuality { get; private set; }
    public float ArmorDrain { get; private set; }
    //shield
    public float MaxShieldHitpoints { get; private set; }
    public float CurrentShieldHitpoints { get; private set; }
    public float ShieldRegeneration { get; private set; }
    public float ShieldDrain { get; private set; }
    public float ShieldRegenerationDelay { get; private set; }
    public float ShieldDamageThreshold { get; private set; }
    public float ShieldEnergyConsumption { get; private set; }
    //energy
    public float MaxEnergyPoints { get; private set; }
    public float CurrentEnergyPoints { get; private set; }
    public float EnergyRegeneration { get; private set; }
    public float EnergyDrain { get; private set; }
    //heat
    public float MaxHeatThreshold { get; private set; }
    public float CurrentHeat { get; private set; }
    public float HeatDissipation { get; private set; }
    public float HeatBuildup { get; private set; }
    //crew
    public int CrewCapacity { get; private set; }
    public int OperationalCrew { get; private set; }
    public float CurrentCrewAmount { get; private set; }
    public float CrewRegeneration { get; private set; }
    public float CrewDrain { get; private set; }
    public float HullRepairSpeedPerCrew { get; private set; }
    public float BreachRepairSpeedPerCrew { get; private set; }
    //oxygen
    public float OxygenCapacity { get; private set; }
    public float CurrentOxygenAmount { get; private set; }
    public float OxygenRegenereration { get; private set; }
    public float OxygenDrain { get; private set; }
    //mobility
    public float ForwardThrusterForce { get; private set; }
    public float ManeuverThrusterForce { get; private set; }
    public float MaxSpeed { get; private set; }
    public float TurningRate { get; private set; }
    public float Weight { get; private set; }

    //equipment
    public ArmorData Armor { get; set; }
    public int MaxArmorCount { get; private set; }
    public int CurrentArmorCount { get; private set; }
    public ShieldData Shield { get; set; }
    public int MaxShieldCount { get; private set; }
    public int CurrentShieldCount { get; private set; }
    public List<EquipmentData> EquippedRefits { get; private set; }
    public List<EquipmentData> EquippedModules { get; private set; }

    //other
    private List<HullBreach> hullBreaches;
    private float shieldLastHit = 0f; //time from last hit
    private bool shieldDisabled = false; //disabled shield if exhausted
    private Rigidbody2D shipRigidbody;
    private SpriteRenderer shipSpriteRenderer;
    private SpriteRenderer shieldSpriteRenderer;
    protected PolygonCollider2D shipCollider;
    protected CircleCollider2D shieldCollider;
    protected AudioClip explosionSound;
    protected EffectData explosionEffect;
    private Light2D heatLight;

    public string ID { get => shipType.id; }
    public string ShipClass { get => shipType.shipClass; }
    public string ShipClassSuffix { get => shipType.shipClassSuffix; }
    public string ShipType { get => shipType.shipType; }
    public float OperationalCrewPercentage { get => UsesCrew ? OperationalCrew > 0 ? Mathf.Clamp01(Mathf.Round(CurrentCrewAmount) / OperationalCrew) : 1f : 0f; }
    public int HullBreachesCount { get => hullBreaches == null ? 0 : hullBreaches.Count; }
    public List<BuffData> Buffs { get; private set; }
    public bool IsAlive { get; private set; }
    public bool UsesCrew { get => shipType.useCrew; }
    public string ShipFactionID { get; private set; } = "player";
    public FactionData ShipFaction { get; private set; }
    public bool OverheatAttack { get; private set; } = true;
    public TargetFilter ShipTargetFilter { get; private set; }
    public Transform TurretsTransform { get; private set; }
    public Transform ShieldTransform { get; private set; }
    public TurretHardpoint[] TurretHardpoints { get; private set; }
    public Sprite WireframeSprite { get; private set; }
    public float ShipRadius { get; private set; }
    public IShipInputProvider ShipInputProvider { get; set; }
    public BuffData[] buffs;

    void Awake()
    {
        EquippedRefits = new();
        EquippedModules = new();
        hullBreaches = new();
        Buffs = new();
    }

    void Start()
    {
        if (!IsAlive)
        {
            InitializeShip();
            InitializeStats();
        }
    }

    void FixedUpdate()
    {
        if (IsAlive)
        {
            ApplyShipRegenerations(Time.fixedDeltaTime);

            if (ShipInputProvider != null)
            {
                var _input = ShipInputProvider.GetInput();

                float _verticalInput = _input.Vertical;
                float _verticalLimit = _input.VerticalLimit;
                float _horizontalInput = _input.Horizontal;
                float _horizontalLimit = _input.HorizontalLimit;
                float _turnInput = _input.Turn;

                ApplyMovement(_verticalInput, _horizontalInput, _verticalLimit, _horizontalLimit);
                ApplyTurn(_turnInput);
            }

            if (CurrentHullHitpoints <= 0f)
            {
                DestroyShip();
            }
        }
    }

    public void InitializeShip()
    {
        gameObject.name = shipType.shipClass + "-class " + shipType.shipType;
        TurretHardpoints = new TurretHardpoint[shipType.turretHardpoints.Length];
        for (int i = 0; i < TurretHardpoints.Length; i++)
        {
            TurretHardpoints[i] = new TurretHardpoint(shipType.turretHardpoints[i]);
        }

        ShipFaction = AssetManager.Instance.GetFaction(ShipFactionID);

        shipSpriteRenderer = GetComponent<SpriteRenderer>();
        if (shipSpriteRenderer == null)
        {
            shipSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        shipRigidbody = GetComponent<Rigidbody2D>();
        if (shipRigidbody == null)
        {
            shipRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        shipCollider = GetComponent<PolygonCollider2D>();
        if (shipCollider == null)
        {
            shipCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

        shieldCollider = GetComponent<CircleCollider2D>();
        if (shieldCollider == null)
        {
            shieldCollider = gameObject.AddComponent<CircleCollider2D>();
            shieldCollider.isTrigger = true;
        }

        heatLight = GetComponent<Light2D>();
        if (heatLight == null)
        {
            heatLight = gameObject.AddComponent<Light2D>();
        }

        ShipTargetFilter = GetComponent<TargetFilter>();
        if (ShipTargetFilter == null)
        {
            ShipTargetFilter = gameObject.AddComponent<TargetFilter>();
        }

        TurretsTransform = transform.Find("Turrets");
        if (TurretsTransform == null)
        {
            TurretsTransform = new GameObject("Turrets").transform;
        }

        ShieldTransform = transform.Find("Shield");
        if (ShieldTransform == null)
        {
            ShieldTransform = new GameObject("Shield").transform;
        }

        shieldSpriteRenderer = ShieldTransform.GetComponent<SpriteRenderer>();
        if (shieldSpriteRenderer == null)
        {
            shieldSpriteRenderer = ShieldTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (GetComponent<SortingGroup>() == null)
        {
            gameObject.AddComponent<SortingGroup>();
        }

        Sprite sprite;
        Sprite shieldSprite;
        if (ShipFaction != null)
        {
            sprite = AssetManager.Instance.GetSprite(shipType.shipSpriteID, ShipFaction.variants);
            shieldSprite = AssetManager.Instance.GetSprite(shipType.shieldSpriteID, ShipFaction.variants);
        }
        else
        {
            sprite = AssetManager.Instance.GetSprite(shipType.shipSpriteID);
            shieldSprite = AssetManager.Instance.GetSprite(shipType.shieldSpriteID);
        }

        if (sprite == null)
        {
            AssetManager.Log($"Failed to load sprite for ship: {shipType.id}", AssetManager.LOG_ERROR);
        }

        WireframeSprite = AssetManager.Instance.GetSprite(shipType.wireframeSpriteID);

        if (sprite != null)
        {
            shipSpriteRenderer.sprite = sprite;
        }
        if (shieldSprite != null)
        {
            shieldSpriteRenderer.sprite = shieldSprite;
        }

        shieldSpriteRenderer.sortingOrder = 6;

        explosionEffect = AssetManager.Instance.GetEffectData(shipType.explosionEffectId);

        shipCollider.points = shipType.colliderPoints;
        shieldCollider.radius = shipType.shieldRadius;
        var _lm = shieldCollider.excludeLayers;
        _lm.value = 1;

        heatLight.lightType = Light2D.LightType.Point;
        heatLight.color = new Color(1f, 0.1f, 0f);
        heatLight.falloffIntensity = 0.75f;
        heatLight.pointLightInnerRadius = 0f;
        heatLight.pointLightOuterRadius = shipType.shieldRadius;
        heatLight.intensity = 0f;
        heatLight.enabled = false;

        ShipTargetFilter.TargetFaction = ShipFaction;

        TurretsTransform.parent = transform;
        TurretsTransform.localPosition = Vector3.zero;
        TurretsTransform.localRotation = new Quaternion();

        ShieldTransform.parent = transform;
        ShieldTransform.localPosition = Vector3.zero;
        ShieldTransform.localRotation = new Quaternion();
        ShieldTransform.localScale = Vector3.one * shipType.shieldRadius * 2;
        ShieldTransform.gameObject.layer = 3;

        shipRigidbody.linearDamping = 0.01f;
        shipRigidbody.angularDamping = 0.25f;

        InitializeBuffs();

        buffs = Buffs.ToArray();

        IsAlive = true;
    }

    public void InitializeBuffs()
    {
        Buffs = new();

        if (shipType.baseBuffs != null)
        {
            foreach (var buffID in shipType.baseBuffs)
            {
                var buff = AssetManager.Instance.GetBuffData(buffID);
                if (buff != null)
                {
                    Buffs.Add(buff);
                }
            }
        }

        if (EquippedRefits != null)
        {
            foreach (var equipment in EquippedRefits)
            {
                var buff = AssetManager.Instance.GetBuffData(equipment.linkedBuff);
                if (buff != null)
                {
                    Buffs.Add(buff);
                }
            }
        }

        if (EquippedModules != null)
        {
            foreach (var equipment in EquippedModules)
            {
                var buff = AssetManager.Instance.GetBuffData(equipment.linkedBuff);
                if (buff != null)
                {
                    Buffs.Add(buff);
                }
            }
        }
    }

    public void InitializeStats()
    {
        UpdateStats();

        CurrentHullHitpoints = MaxHullHitpoints;
        CurrentArmorHitpoints = MaxArmorHitpoints;
        CurrentShieldHitpoints = MaxShieldHitpoints;
        CurrentEnergyPoints = MaxEnergyPoints;
        CurrentHeat = 0f;
        CurrentCrewAmount = CrewCapacity;
        CurrentOxygenAmount = OxygenCapacity;
    }

    public void UpdateStats()
    {
        float _equipmentWeight = 0f;

        if (TurretHardpoints != null)
        {
            foreach (var turret in TurretHardpoints)
            {
                if (turret.Turret != null)
                {
                    turret.Turret.UpdateStats();
                    _equipmentWeight += turret.Turret.Weight;
                }
            }
        }

        StatMod _hullHP = StatMod.Identity;
        StatMod _hullRegen = StatMod.Identity;
        StatMod _hullDrain = StatMod.Identity;
        StatMod _hullWeight = StatMod.Identity;

        StatMod _armorHP = StatMod.Identity;
        StatMod _armorCount = StatMod.Identity;
        StatMod _armorQuality = StatMod.Identity;
        StatMod _armorThickness = StatMod.Identity;
        StatMod _armorRegen = StatMod.Identity;
        StatMod _armorDrain = StatMod.Identity;
        StatMod _armorWeight = StatMod.Identity;

        StatMod _shieldHP = StatMod.Identity;
        StatMod _shieldCount = StatMod.Identity;
        StatMod _shieldThreshold = StatMod.Identity;
        StatMod _shieldRegen = StatMod.Identity;
        StatMod _shieldDrain = StatMod.Identity;
        StatMod _shieldDelay = StatMod.Identity;
        StatMod _shieldWeight = StatMod.Identity;
        StatMod _shieldEnergyConsumption = StatMod.Identity;

        StatMod _energy = StatMod.Identity;
        StatMod _energyRegen = StatMod.Identity;
        StatMod _energyDrain = StatMod.Identity;

        StatMod _heat = StatMod.Identity;
        StatMod _heatDissipation = StatMod.Identity;
        StatMod _heatBuildup = StatMod.Identity;

        StatMod _crew = StatMod.Identity;
        StatMod _crewOperational = StatMod.Identity;
        StatMod _crewRegen = StatMod.Identity;
        StatMod _crewDrain = StatMod.Identity;

        StatMod _oxygen = StatMod.Identity;
        StatMod _oxygenGeneration = StatMod.Identity;
        StatMod _oxygenDrain = StatMod.Identity;

        StatMod _maxSpeed = StatMod.Identity;
        StatMod _turnRate = StatMod.Identity;
        StatMod _mainThruster = StatMod.Identity;
        StatMod _maneuverThruster = StatMod.Identity;

        float _shieldDelayMult = 1f;
        float _shieldDelayFact = 1f;

        float _crewHullRepairSpeedMult = 1f;
        float _crewHullRepairSpeedFact = 1f;
        float _crewBreachRepairSpeedMult = 1f;
        float _crewBreachRepairSpeedFact = 1f;

        if (Buffs != null)
        {
            foreach (var buff in Buffs)
            {
                // --- Hull ---
                _hullHP.Add(buff.Get("HullHP", "Bonus"), buff.Get("HullHP", "Multiplier"), buff.Get("HullHP", "Factor"));
                _hullRegen.Add(buff.Get("HullRegen", "Bonus"), buff.Get("HullRegen", "Multiplier"), buff.Get("HullRegen", "Factor"));
                _hullDrain.Add(buff.Get("HullDrain", "Bonus"), buff.Get("HullDrain", "Multiplier"), buff.Get("HullDrain", "Factor"));
                _hullWeight.Add(buff.Get("HullWeight", "Bonus"), buff.Get("HullWeight", "Multiplier"), buff.Get("HullWeight", "Factor"));

                // --- Armor ---
                _armorHP.Add(buff.Get("ArmorHP", "Bonus"), buff.Get("ArmorHP", "Multiplier"), buff.Get("ArmorHP", "Factor"));
                _armorCount.Add(buff.Get("ArmorCount", "Bonus"), buff.Get("ArmorCount", "Multiplier"), buff.Get("ArmorCount", "Factor"));
                _armorQuality.Add(buff.Get("ArmorQuality", "Bonus"), buff.Get("ArmorQuality", "Multiplier"), buff.Get("ArmorQuality", "Factor"));
                _armorThickness.Add(buff.Get("ArmorThickness", "Bonus"), buff.Get("ArmorThickness", "Multiplier"), buff.Get("ArmorThickness", "Factor"));
                _armorRegen.Add(buff.Get("ArmorRegen", "Bonus"), buff.Get("ArmorRegen", "Multiplier"), buff.Get("ArmorRegen", "Factor"));
                _armorDrain.Add(buff.Get("ArmorDrain", "Bonus"), buff.Get("ArmorDrain", "Multiplier"), buff.Get("ArmorDrain", "Factor"));
                _armorWeight.Add(buff.Get("ArmorWeight", "Bonus"), buff.Get("ArmorWeight", "Multiplier"), buff.Get("ArmorWeight", "Factor"));

                // --- Shield ---
                _shieldHP.Add(buff.Get("ShieldHP", "Bonus"), buff.Get("ShieldHP", "Multiplier"), buff.Get("ShieldHP", "Factor"));
                _shieldCount.Add(buff.Get("ShieldCount", "Bonus"), buff.Get("ShieldCount", "Multiplier"), buff.Get("ShieldCount", "Factor"));
                _shieldThreshold.Add(buff.Get("ShieldDamageThreshold", "Bonus"), buff.Get("ShieldDamageThreshold", "Multiplier"), buff.Get("ShieldDamageThreshold", "Factor"));
                _shieldRegen.Add(buff.Get("ShieldRegen", "Bonus"), buff.Get("ShieldRegen", "Multiplier"), buff.Get("ShieldRegen", "Factor"));
                _shieldDrain.Add(buff.Get("ShieldDrain", "Bonus"), buff.Get("ShieldDrain", "Multiplier"), buff.Get("ShieldDrain", "Factor"));
                _shieldDelayMult += buff.Get("ShieldDelay", "Multiplier");
                _shieldDelayFact *= buff.Get("ShieldDelay", "Factor");
                _shieldWeight.Add(buff.Get("ShieldWeight", "Bonus"), buff.Get("ShieldWeight", "Multiplier"), buff.Get("ShieldWeight", "Factor"));
                _shieldEnergyConsumption.Add(buff.Get("ShieldEnergyConsumption", "Bonus"), buff.Get("ShieldEnergyConsumption", "Multiplier"), buff.Get("ShieldEnergyConsumption", "Factor"));

                // --- Energy ---
                _energy.Add(buff.Get("EnergyCapacity", "Bonus"), buff.Get("EnergyCapacity", "Multiplier"), buff.Get("EnergyCapacity", "Factor"));
                _energyRegen.Add(buff.Get("EnergyRegen", "Bonus"), buff.Get("EnergyRegen", "Multiplier"), buff.Get("EnergyRegen", "Factor"));
                _energyDrain.Add(buff.Get("EnergyDrain", "Bonus"), buff.Get("EnergyDrain", "Multiplier"), buff.Get("EnergyDrain", "Factor"));

                // --- Heat ---
                _heat.Add(buff.Get("HeatThreshold", "Bonus"), buff.Get("HeatThreshold", "Multiplier"), buff.Get("HeatThreshold", "Factor"));
                _heatDissipation.Add(buff.Get("HeatDissipation", "Bonus"), buff.Get("HeatDissipation", "Multiplier"), buff.Get("HeatDissipation", "Factor"));
                _heatBuildup.Add(buff.Get("HeatBuildup", "Bonus"), buff.Get("HeatBuildup", "Multiplier"), buff.Get("HeatBuildup", "Factor"));

                // --- Crew ---
                _crew.Add(buff.Get("CrewCapacity", "Bonus"), buff.Get("CrewCapacity", "Multiplier"), buff.Get("CrewCapacity", "Factor"));
                _crewOperational.Add(buff.Get("OperationalCrew", "Bonus"), buff.Get("OperationalCrew", "Multiplier"), buff.Get("OperationalCrew", "Factor"));
                _crewHullRepairSpeedMult += buff.Get("CrewHullRepairSpeed", "Multiplier");
                _crewHullRepairSpeedFact *= buff.Get("CrewHullRepairSpeed", "Factor");
                _crewBreachRepairSpeedMult += buff.Get("CrewBreachRepairSpeed", "Multiplier");
                _crewBreachRepairSpeedFact *= buff.Get("CrewBreachRepairSpeed", "Factor");
                _crewRegen.Add(buff.Get("CrewRegen", "Bonus"), 0f, buff.Get("CrewRegen", "Factor"));
                _crewDrain.Add(buff.Get("CrewDrain", "Bonus"), 0f, buff.Get("CrewDrain", "Factor"));

                // --- Oxygen ---
                _oxygen.Add(buff.Get("OxygenCapacity", "Bonus"), buff.Get("OxygenCapacity", "Multiplier"), buff.Get("OxygenCapacity", "Factor"));
                _oxygenGeneration.Add(buff.Get("OxygenGeneration", "Bonus"), buff.Get("OxygenGeneration", "Multiplier"), buff.Get("OxygenGeneration", "Factor"));
                _oxygenDrain.Add(buff.Get("OxygenDrain", "Bonus"), buff.Get("OxygenDrain", "Multiplier"), buff.Get("OxygenDrain", "Factor"));

                // --- Mobility ---
                _maxSpeed.Add(buff.Get("MaxSpeed", "Bonus"), buff.Get("MaxSpeed", "Multiplier"), buff.Get("MaxSpeed", "Factor"));
                _turnRate.Add(buff.Get("TurnRate", "Bonus"), buff.Get("TurnRate", "Multiplier"), buff.Get("TurnRate", "Factor"));
                _mainThruster.Add(buff.Get("MainThruster", "Bonus"), buff.Get("MainThruster", "Multiplier"), buff.Get("MainThruster", "Factor"));
                _maneuverThruster.Add(buff.Get("ManeuverThruster", "Bonus"), buff.Get("ManeuverThruster", "Multiplier"), buff.Get("ManeuverThruster", "Factor"));
            }
        }

        MaxHullHitpoints = _hullHP.Apply(shipType.hullHitpoints);
        HullRegeneration = _hullRegen.Apply(shipType.hullRegeneration);
        HullDrain = _hullDrain.Apply(0f);
        Weight = _hullWeight.Apply(shipType.weight) + _equipmentWeight;

        MaxArmorCount = _armorCount.ApplyInt(shipType.maxArmor);
        if (Armor != null)
        {
            MaxArmorHitpoints = _armorHP.Apply(Armor.armorHitpoints) * CurrentArmorCount;
            ArmorThickness = _armorThickness.Apply(Armor.armorThickness) * CurrentArmorCount;
            ArmorQuality = _armorQuality.Apply(Armor.armorQuality);
            ArmorRegeneration = _armorRegen.Apply(Armor.armorRegeneration) * CurrentArmorCount;
            ArmorDrain = _armorDrain.Apply(0);
            Weight += _armorWeight.Apply(CurrentArmorCount * Armor.weight);
        }

        MaxShieldCount = _shieldCount.ApplyInt(shipType.maxShield);
        if (Shield != null)
        {
            MaxShieldHitpoints = _shieldHP.Apply(Shield.shieldHitpoints) * CurrentShieldCount;
            ShieldDamageThreshold = _shieldThreshold.Apply(Shield.shieldDamageThreshold) * CurrentShieldCount;
            ShieldRegenerationDelay = Shield.shieldRegenerationDelay * _shieldDelayMult * _shieldDelayFact;
            ShieldRegeneration = _shieldRegen.Apply(Shield.shieldRegeneration) * CurrentShieldCount;
            ShieldDrain = _shieldDrain.Apply(0);
            ShieldEnergyConsumption = _shieldEnergyConsumption.Apply(Shield.energyConsumption) * CurrentShieldCount;
            Weight += _shieldWeight.Apply(CurrentShieldCount * Shield.weight);
        }

        if (shipType.useEnergy)
        {
            MaxEnergyPoints = _energy.Apply(shipType.energyPoints);
            EnergyRegeneration = _energyRegen.Apply(shipType.energyRegeneration);
            EnergyDrain = _energyDrain.Apply(0f);
        }

        if (shipType.useHeat)
        {
            MaxHeatThreshold = _heat.Apply(shipType.heatThreshold);
            HeatDissipation = _heatDissipation.Apply(shipType.heatDissipation);
            HeatBuildup = _heatBuildup.Apply(0);
        }

        if (shipType.useCrew)
        {
            CrewCapacity = _crew.ApplyInt(shipType.maxCrew);
            OperationalCrew = Mathf.Min(CrewCapacity, _crewOperational.ApplyInt(shipType.operationalCrew));
            HullRepairSpeedPerCrew = CREW_HULL_REPAIR_SPEED * _crewHullRepairSpeedMult * _crewHullRepairSpeedFact;
            BreachRepairSpeedPerCrew = _crewBreachRepairSpeedMult * _crewBreachRepairSpeedFact;
            CrewRegeneration = _crewRegen.Apply(0f);
            CrewDrain = _crewDrain.Apply(0f);
        }

        if (shipType.useOxygen)
        {
            OxygenCapacity = _oxygen.Apply(shipType.oxygenCapacity);
            OxygenRegenereration = _oxygenGeneration.Apply(shipType.oxygenRegeneration);
            OxygenDrain = _oxygenDrain.Apply(0f);
        }

        // --- Mobility ---
        ForwardThrusterForce = _mainThruster.Apply(shipType.forwardThrusterForce);
        ManeuverThrusterForce = _maneuverThruster.Apply(shipType.maneuverThrusterForce);
        MaxSpeed = _maxSpeed.Apply(shipType.maxSpeed);
        TurningRate = _turnRate.Apply(shipType.turningRate);

        shipRigidbody.mass = Weight;
    }

    public void SetTurret(string _turretID, string _hardpointID)
    {
        TurretHardpoint _targetHardpoint = null;
        foreach (var _hardpoint in TurretHardpoints)
        {
            if (_hardpoint.Id == _hardpointID)
            {
                _targetHardpoint = _hardpoint;
                break;
            }
        }

        if (_targetHardpoint == null)
        {
            return;
        }

        if (_targetHardpoint.Turret != null)
        {
            Destroy(_targetHardpoint.Turret.gameObject);
            _targetHardpoint.Turret = null;
        }

        if (_turretID != null && _turretID != "")
        {
            _targetHardpoint.Turret = Turret.Instantiate(_turretID, TurretsTransform, _targetHardpoint);
        }
    }

    public bool TakeDamage(DamageData _damage, Ship _attacker = null, float? _distance = null, float? _maxRange = null, float _hitAngle = 90f)
    {
        var _damageFactorFromDistance = 1f;
        var _penetrationFactorFromDistance = 1f;
        if (_distance != null && _maxRange != null)
        {
            _damageFactorFromDistance = 1f - _damage.damageLossFromDistance * ((float)_distance / (float)_maxRange);
            _penetrationFactorFromDistance = 1f - _damage.penetrationLossFromDistance * ((float)_distance / (float)_maxRange);
        }

        //shield damage
        var _damageType = _damage.type;
        if (CurrentShieldHitpoints > 0 && !shieldDisabled && Shield != null)
        {
            var shieldDamage = _damage.shieldDamage * _damageFactorFromDistance * Random.Range(1 - _damage.damageRandomness, 1 + _damage.damageRandomness);
            shieldDamage *= Shield.shieldDamageMultiplier[0];
            if (_damageType != 0)
            {
                shieldDamage *= Shield.shieldDamageMultiplier[_damageType];
            }

            CurrentShieldHitpoints -= shieldDamage;

            if (shieldDamage > ShieldDamageThreshold || CurrentShieldHitpoints <= 0)
            {
                shieldLastHit = 0f;
            }

            UpdateShield();

            if (CurrentShieldHitpoints > 0)
            {
                return true;
            }
            else
            {
                shieldDisabled = true;
                return false;
            }
        }

        CurrentShieldHitpoints = Mathf.Max(0, CurrentShieldHitpoints);

        var _angleFactor = Mathf.Sin(Mathf.Clamp(_hitAngle, 1f, 179f) * Mathf.Deg2Rad);
        //armor damage
        if (CurrentArmorHitpoints > 0 && Armor != null)
        {
            var _armorDamage = _damage.armorDamage * _damageFactorFromDistance * _angleFactor * Random.Range(1 - _damage.damageRandomness, 1 + _damage.damageRandomness);
            _armorDamage *= Armor.armorDamageMultiplier[0];
            if (_damageType != 0)
            {
                _armorDamage *= Armor.armorDamageMultiplier[_damageType];
            }

            CurrentArmorHitpoints -= _armorDamage;

        }
        CurrentArmorHitpoints = Mathf.Max(0, CurrentArmorHitpoints);

        //hull damage
        var _hullDamage = _damage.hullDamage * _damageFactorFromDistance * Random.Range(1 - _damage.damageRandomness, 1 + _damage.damageRandomness);
        var _penetration = _damage.penetration * _penetrationFactorFromDistance * Random.Range(1 - _damage.penetrationRandomness, 1 + _damage.penetrationRandomness);
        var _armorThickness = GetArmorThickness() / _angleFactor;
        bool _armorPenetrated = false;

        if (_penetration > _armorThickness)
        {
            _armorPenetrated = true;
            if (_damage.partialPenetration && _penetration <= _armorThickness * 1.1f)
            {
                _hullDamage *= 0.5f;
            }
            else if (_damage.overpenetration && _penetration >= _armorThickness * 2.5f)
            {
                _hullDamage *= 0.75f;
            }
        }
        else if (Armor == null || (_damage.ignoresArmor && !Armor.cannotBeIgnored[_damageType]))
        {
            _armorPenetrated = true;
        }
        else if (_damage.spallingDamage > 0f)
        {
            _hullDamage *= _damage.spallingDamage * _penetration / _armorThickness;
        }
        else //no penetration or spalling damage
        {
            _hullDamage = 0f;
        }

        if (Armor != null)
        {
            _hullDamage *= Armor.hullDamageMultiplier[0];
            if (_damageType != 0)
            {
                _hullDamage *= Armor.hullDamageMultiplier[_damageType];
            }
        }

        CurrentHullHitpoints -= _hullDamage;
        CurrentHullHitpoints = Mathf.Max(0, CurrentHullHitpoints);

        //crew damage
        if (shipType.useCrew)
        {
            CurrentCrewAmount -= _damage.crewDamage * (Armor == null ? 1 : Armor.crewDamageMultiplier) * _damageFactorFromDistance * Random.Range(0, 2f) * (1f - CurrentHullHitpoints / MaxHullHitpoints);
        }
        CurrentCrewAmount = Mathf.Max(0, CurrentCrewAmount);

        //thermal damage
        if (shipType.useHeat)
        {
            CurrentHeat += _damage.heatDamage * (Armor == null ? 1 : Armor.heatDamageMultiplier) * _damageFactorFromDistance;
        }

        //hull breach
        if (_damage.causesHullBreach && _armorPenetrated && shipType.useOxygen)
        {
            var _breachChance = _penetration / (_armorThickness + _penetration) * 0.2f;
            _breachChance *= (1f - Mathf.Clamp01(CurrentHullHitpoints / MaxHullHitpoints)) * 0.8f + 0.2f;

            if (Random.Range(0f, 1f) <= _breachChance)
            {
                var _breachSize = _damage.hullDamage * 0.03f * _damageFactorFromDistance * Random.Range(0.5f, 1.5f);
                var _duration = Mathf.Max(1f, _breachSize * 6f);

                hullBreaches.Add(new HullBreach(_breachSize, _duration));
            }
        }

        UpdateStats();

        if (CurrentHullHitpoints <= 0f)
        {
            DestroyShip();
        }

        return true;
    }

    public void AddEquipment(string equipmentId, bool update = true)
    {
        EquipmentData equipment = AssetManager.Instance.GetEquipmentData(equipmentId);

        if (equipment == null)
        {
            return;
        }

        if (equipment.equipmentType == "refit" && EquippedRefits.Count < shipType.refitSlots)
        {
            EquippedRefits.Add(equipment);
            if (update)
            {
                AddBuff(equipment.linkedBuff);
            }
        }
        else if (equipment.equipmentType == "module" && EquippedModules.Count < shipType.moduleSlots)
        {
            EquippedModules.Add(equipment);
            if (update)
            {
                AddBuff(equipment.linkedBuff);
            }
        }
    }

    public void AddBuff(string buffId, bool update = true)
    {
        BuffData buff = AssetManager.Instance.GetBuffData(buffId);

        if (buff == null)
        {
            return;
        }

        Buffs.Add(buff);

        if (update)
        {
            UpdateStats();
            UpdateShield();
        }
    }

    public void RemoveBuff(string buffId)
    {
        if (Buffs == null || Buffs.Count == 0)
        {
            return;
        }

        BuffData buffFound = null;

        foreach (var buff in Buffs)
        {
            if (buff.id == buffId)
            {
                buffFound = buff;
            }
        }

        RemoveBuff(buffFound);
    }

    public void RemoveBuff(BuffData buff)
    {
        if (Buffs == null || Buffs.Count == 0 || !Buffs.Contains(buff))
        {
            return;
        }

        Buffs.Remove(buff);

        UpdateStats();
        UpdateShield();
    }

    public void ChangeShipType(ShipData _type)
    {
        shipType = _type;
        InitializeShip();
        InitializeStats();
    }

    public ShipData GetShipType()
    {
        return shipType;
    }

    public void ChangeFaction(string _factionID)
    {
        FactionData _faction = AssetManager.Instance.GetFaction(_factionID);
        if (_faction != null)
        {
            ShipFactionID = _factionID;
            ShipFaction = _faction;

            var _sprite = AssetManager.Instance.GetSprite(shipType.shipSpriteID, ShipFaction.variants);

            if (_sprite != null)
            {
                shipSpriteRenderer.sprite = _sprite;
            }

            foreach (var _hardpoint in TurretHardpoints)
            {
                if (_hardpoint.Turret != null)
                {
                    _hardpoint.Turret.UpdateVisuals();
                }
            }

            var _shieldSprite = AssetManager.Instance.GetSprite(shipType.shieldSpriteID, ShipFaction.variants);

            if (_shieldSprite != null)
            {
                shieldSpriteRenderer.sprite = _shieldSprite;
            }

            UpdateShield();
        }
    }

    public bool CanDrainEnergy(float _energy)
    {
        if (!shipType.useEnergy)
        {
            return true;
        }

        return CurrentEnergyPoints >= _energy;
    }

    public bool CanGenerateHeat(float _heat)
    {
        if (!shipType.useHeat)
        {
            return true;
        }

        if (_heat < 0)
        {
            return CurrentHeat >= -_heat;
        }

        return CurrentHeat < MaxHeatThreshold - _heat || OverheatAttack;
    }

    public void DrainEnergy(float _energy)
    {
        if (!shipType.useEnergy)
        {
            return;
        }

        CurrentEnergyPoints -= _energy;
    }

    public void GenerateHeat(float _heat)
    {
        if (!shipType.useHeat)
        {
            return;
        }

        CurrentHeat += _heat;
    }

    private void ApplyShipRegenerations(float _time)
    {
        float _heatDamage = shipType.useHeat ? Mathf.Max(0, (CurrentHeat / MaxHeatThreshold - 1f) * HEAT_DAMAGE_PER_SECOND) : 0f;
        float _hullDelta = (HullRegeneration + HullRepairSpeedPerCrew * CurrentCrewAmount - HullDrain - _heatDamage) * _time;
        CurrentHullHitpoints = Mathf.Clamp(CurrentHullHitpoints + _hullDelta, 0f, MaxHullHitpoints);

        float _armorDelta = (ArmorRegeneration - ArmorDrain) * _time;
        CurrentArmorHitpoints = Mathf.Clamp(CurrentArmorHitpoints + _armorDelta, 0f, MaxArmorHitpoints);


        if (shieldLastHit < ShieldRegenerationDelay)
        {
            shieldLastHit = Mathf.Min(shieldLastHit + _time, ShieldRegenerationDelay);
        }

        float _shieldRegenMult = 1f;

        if (shieldLastHit < ShieldRegenerationDelay)
        {
            _shieldRegenMult = Mathf.Clamp01(shieldLastHit / ShieldRegenerationDelay);
        }

        float _shieldDelta = (ShieldRegeneration * _shieldRegenMult - ShieldDrain) * _time;

        CurrentShieldHitpoints = Mathf.Clamp(CurrentShieldHitpoints + _shieldDelta, 0f, MaxShieldHitpoints);

        if (shieldDisabled && CurrentShieldHitpoints > MaxShieldHitpoints * 0.1f)
        {
            shieldDisabled = false;
        }

        if (shipType.useEnergy)
        {
            float _energyDelta = (EnergyRegeneration - EnergyDrain) * _time;
            CurrentEnergyPoints = Mathf.Clamp(CurrentEnergyPoints + _energyDelta, 0f, MaxEnergyPoints);
        }

        if (shipType.useHeat)
        {
            float _heatDelta = (HeatBuildup - HeatDissipation) * _time;
            CurrentHeat = Mathf.Clamp(CurrentHeat + _heatDelta, 0f, 3 * MaxHeatThreshold);

            float heatStart = MaxHeatThreshold * 0.25f;
            float heatStop = MaxHeatThreshold * 1.5f;

            if (CurrentHeat > heatStart)
            {
                float heatColorFactor = Mathf.Clamp01((CurrentHeat - heatStart) / (heatStop - heatStart));
                heatLight.intensity = 10f * heatColorFactor;
                heatLight.enabled = true;
            }
            else
            {
                heatLight.intensity = 0f;
                heatLight.enabled = false;
            }
        }

        if (shipType.useCrew)
        {
            float _crewLoss = 0f;

            if (CurrentOxygenAmount <= 0f)
            {
                _crewLoss += Mathf.Max(1f, CurrentCrewAmount * PERCENT_CREW_LOSS_FROM_OXYGEN);
            }

            float _crewDelta = (CrewRegeneration - CrewDrain - _crewLoss) * _time;
            CurrentCrewAmount = Mathf.Clamp(CurrentCrewAmount + _crewDelta, 0f, CrewCapacity);
        }

        if (shipType.useOxygen)
        {
            float _breachOxygenLoss = 0f;

            for (int i = 0; i < hullBreaches.Count; i++)
            {
                _breachOxygenLoss += hullBreaches[i].size;
            }

            float _oxygenDelta = (OxygenRegenereration - OxygenDrain - _breachOxygenLoss) * _time;
            CurrentOxygenAmount = Mathf.Clamp(CurrentOxygenAmount + _oxygenDelta, 0f, OxygenCapacity);

            int _breachesRepairedSimultaneously = 1 + ((int)CurrentCrewAmount / 100);
            var _repairedBreaches = new List<HullBreach>();
            int _freeCrew = (int)CurrentCrewAmount;

            for (int i = 0; i < Mathf.Min(hullBreaches.Count, _breachesRepairedSimultaneously); i++)
            {
                int _crewAssigned = Mathf.Min(_freeCrew, 100);
                var breach = hullBreaches[i];
                breach.duration -= _time * CREW_BREACH_REPAIR_SPEED * BreachRepairSpeedPerCrew * _crewAssigned;
                if (breach.duration <= 0f)
                {
                    _repairedBreaches.Add(breach);
                }
            }

            for (int i = 0; i < _repairedBreaches.Count; i++)
            {
                hullBreaches.Remove(_repairedBreaches[i]);
            }
        }

        UpdateShield();
    }

    private void ApplyMovement(float vertical, float horizontal, float verticalLimit, float horizontalLimit)
    {
        float _forwardThrusterPower = ForwardThrusterForce * OperationalCrewPercentage;
        float _maneuverThrusterPower = ManeuverThrusterForce * OperationalCrewPercentage;

        Vector2 _forward = transform.up;
        Vector2 _right = transform.right;

        Vector2 _velocity = shipRigidbody.linearVelocity;

        float _forwardSpeed = Vector2.Dot(_velocity, _forward);
        float _lateralSpeed = Vector2.Dot(_velocity, _right);

        float _maxForwardSpeed = MaxSpeed * verticalLimit;
        float _maxLateralSpeed = MaxSpeed * horizontalLimit;

        if (Mathf.Abs(vertical) > 0.01f)
        {
            bool acceleratingSameDirection = Mathf.Sign(vertical) == Mathf.Sign(_forwardSpeed);

            if (!acceleratingSameDirection || Mathf.Abs(_forwardSpeed) < _maxForwardSpeed)
            {
                shipRigidbody.AddForce(_forward * vertical * _maneuverThrusterPower);
            }
        }

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            bool acceleratingSameDirection = Mathf.Sign(horizontal) == Mathf.Sign(_lateralSpeed);

            if (!acceleratingSameDirection || Mathf.Abs(_lateralSpeed) < _maxLateralSpeed)
            {
                shipRigidbody.AddForce(_right * horizontal * _maneuverThrusterPower);
            }
        }

        if (vertical > 0f && _forwardSpeed < _maxForwardSpeed)
        {
            shipRigidbody.AddForce(_forward * _forwardThrusterPower * vertical);
        }

        if (Mathf.Abs(horizontal) < 0.01f || Mathf.Abs(_lateralSpeed) > _maxLateralSpeed)
        {
            float _brake = Mathf.Clamp(_lateralSpeed, -1f, 1f);
            shipRigidbody.AddForce(-_right * _brake * _maneuverThrusterPower);
        }

        if (Mathf.Abs(vertical) < 0.01f || Mathf.Abs(_forwardSpeed) > _maxForwardSpeed)
        {
            float _brake = Mathf.Clamp(_forwardSpeed, -1f, 1f);
            shipRigidbody.AddForce(-_forward * _brake * _maneuverThrusterPower);
        }
    }

    private void ApplyTurn(float turnInput)
    {
        float _maneuverThrusterPower = ManeuverThrusterForce * OperationalCrewPercentage / 2f;

        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float _torque = -turnInput * _maneuverThrusterPower;
            shipRigidbody.AddTorque(_torque);

            if (Mathf.Abs(shipRigidbody.angularVelocity) > TurningRate)
            {
                shipRigidbody.angularVelocity = Mathf.Sign(shipRigidbody.angularVelocity) * TurningRate;
            }
        }
        else
        {

            if (Mathf.Abs(shipRigidbody.angularVelocity) < 0.1f)
            {
                shipRigidbody.angularVelocity = 0f;
            }
            else
            {
                float _torque = -Mathf.Sign(shipRigidbody.angularVelocity) * _maneuverThrusterPower;
                shipRigidbody.AddTorque(_torque);
            }
        }
    }

    private void UpdateShipRadius()
    {
        if (shipType.colliderPoints == null || shipType.colliderPoints.Length < 1)
        {
            ShipRadius = 0.5f;
            return;
        }

        float _max = 0.5f;
        foreach (var _point in shipType.colliderPoints)
        {
            float _dist = _point.magnitude;
            if (_dist > _max)
            {
                _max = _dist;
            }
        }

        ShipRadius = _max;
    }

    private void UpdateShield()
    {
        if (shieldSpriteRenderer == null)
        {
            return;
        }

        Color _c = shieldSpriteRenderer.color;
        var _alpha = shipType.shieldAlphaFull - shipType.shieldAlphaMinimum;
        var _shieldPercentage = Mathf.Clamp01(CurrentShieldHitpoints / MaxShieldHitpoints);
        _c.a = shipType.shieldAlphaMinimum + _alpha * _shieldPercentage;
        shieldSpriteRenderer.color = _c;

        if (CurrentShieldHitpoints <= 0 || Shield == null || shieldDisabled)
        {
            if (shieldCollider.enabled)
            {
                shieldCollider.enabled = false;
                shieldSpriteRenderer.enabled = false;
            }
        }
        else if (!shieldCollider.enabled)
        {
            shieldCollider.enabled = true;
            shieldSpriteRenderer.enabled = true;
        }
    }

    private float GetArmorThickness()
    {
        if (Armor == null || CurrentArmorHitpoints <= 0f || MaxArmorHitpoints < 1 || CurrentArmorCount == 0)
        {
            return 0f;
        }

        float _armorPercentage = CurrentArmorHitpoints / (MaxArmorHitpoints * (1 - ArmorQuality));

        return ArmorThickness * Mathf.Clamp01(_armorPercentage);
    }

    private bool IsAtPosition(Vector2 _targetPosition)
    {
        return Vector2.Distance(transform.position, _targetPosition) < ShipRadius / 2f;
    }

    private bool IsFacingAngle(float _angle)
    {
        return Mathf.DeltaAngle(_angle, transform.eulerAngles.z) < 1f;
    }

    private bool IsFacingPoint(Vector2 _targetPosition)
    {
        var _vector = _targetPosition - (Vector2)transform.position;
        return IsFacingAngle(Mathf.Atan2(_vector.y, _vector.x) * Mathf.Rad2Deg);
    }

    private float GetTurretsRange(TurretRangeType _rangeType)
    {
        float _max = 0f;
        float _min = float.MaxValue;
        float _avg = 0f;

        int _turretCount = 0;

        foreach (var _turretHardpoint in TurretHardpoints)
        {
            var _turret = _turretHardpoint.Turret;
            if (_turret != null)
            {
                _turretCount++;
                _avg += _turret.Range;

                if (_turret.Range < _min)
                {
                    _min = _turret.Range;
                }

                if (_turret.Range > _max)
                {
                    _max = _turret.Range;
                }
            }
        }

        if (_turretCount == 0)
        {
            _min = 50f;
            _max = 200f;
            _avg = 100f;
        }
        else
        {
            _avg /= _turretCount;
        }

        return _rangeType switch
        {
            TurretRangeType.NORMAL => _min,
            TurretRangeType.DEFENSIVE => _avg,
            TurretRangeType.SNIPER => _max,
            _ => Mathf.Min(_min, 5f),
        };
    }

    private void DestroyShip()
    {
        IsAlive = false;

        if (explosionEffect != null)
        {
            var _explosionEffect = Effect.InstantiateEffect(explosionEffect);
            _explosionEffect.transform.position = transform.position;

            var _audioSource = _explosionEffect.AddComponent<AudioSource>();
            //TODO
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (shipType == null)
        {
            return;
        }

        string _armor = Armor != null ? "\n" + Armor.id + " x" + CurrentArmorCount : "";
        string _shield = Shield != null ? "\n" + Shield.id + " x" + CurrentShieldCount : "";
        string _speed = shipRigidbody != null ? $"\nSpeed: {shipRigidbody.linearVelocity.magnitude:f2}" : "";

        var _style = new GUIStyle();
        _style.alignment = TextAnchor.UpperLeft;
        _style.normal.textColor = new Color(0, 1, 1, 0.5f);

        float _shieldRegenerationMultiplier = 1f;

        if (shieldLastHit < ShieldRegenerationDelay)
        {
            _shieldRegenerationMultiplier = Mathf.Clamp01(shieldLastHit / ShieldRegenerationDelay);
        }

        string _vitals =
            $"\nHull: {CurrentHullHitpoints:f0}({HullRepairSpeedPerCrew * CurrentCrewAmount:f0})" +
            $"\nArmor: {CurrentArmorHitpoints:f0}({GetArmorThickness():f1}) (+{ArmorRegeneration:f2}/s)" +
            $"\nShield: {CurrentShieldHitpoints:f0} (+{ShieldRegeneration * _shieldRegenerationMultiplier:f2}/s)" +
            $"\nEnergy: {CurrentEnergyPoints:f0} (+{EnergyRegeneration:f2}/s)" +
            $"\nOxygen: {CurrentOxygenAmount:f0}({HullBreachesCount})" +
            $"\nCrew: {CurrentCrewAmount:f0}/{OperationalCrew:f0}" +
            $"\nHeat: {CurrentHeat:f0}";

        string _breaches = "\n";
        if (hullBreaches != null && hullBreaches.Count > 0)
        {
            for (int i = 0; i < hullBreaches.Count; i++)
            {
                _breaches += $"\nBreach {i + 1}: Size {hullBreaches[i].size:f1}, Duration {hullBreaches[i].duration:f1}s";
            }
        }

        Handles.Label(transform.position + new Vector3(2, 4), ShipClass + "-class" + _speed + _vitals + _armor + _shield + _breaches, _style);
        Handles.color = new Color(0, 1, 1, 0.3f);
        Handles.DrawDottedLine(transform.position, transform.position + new Vector3(2, 3.9f), 6f);

        Handles.color = new Color(1, 0.5f, 0, 0.05f);
        Handles.DrawSolidDisc(transform.position, Vector3.forward, ShipRadius);
    }
}
