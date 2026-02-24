using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public record TurretHardpoint
{
    [SerializeField]
    private string id = "s0";
    [SerializeField]
    private int size = 0; //max size of the turret that can be mounted on
    [SerializeField]
    private float angle = 0; //facing angle of the turret hardpoint
    [SerializeField]
    private float arc = -1f; //arc in degrees, negative means full rotation, should not be more than 360
    [SerializeField]
    private Vector2 position = Vector2.zero;

    public TurretHardpoint(TurretHardpoint _copy)
    {
        id = _copy.id;
        size = _copy.size;
        angle = _copy.angle;
        arc = _copy.arc;
        position = _copy.position;
    }

    public string Id { get => id; set => id = value; }
    public int Size { get => size; set => size = value; }
    public float Angle { get => angle; set => angle = value; }
    public float Arc { get => arc; set => arc = value; }
    public Vector2 Position { get => position; set => position = value; }
    public Turret Turret { get; set; }
}

[System.Serializable]
public abstract class Turret : MonoBehaviour
{
    public static Turret Instantiate(string _id, Transform _shipTurretTransform, TurretHardpoint _hardpoint)
    {
        var _turretType = AssetManager.Instance.GetTurretData(_id);

        if (_turretType == null)
        {
            return null;
        }

        var _turretGameObject = new GameObject(_turretType.id);
        var _turret = _turretType.weaponType switch
        {
            "ballistic" => _turretGameObject.AddComponent<ProjectileTurret>(),
            "beam" => _turretGameObject.AddComponent<BeamTurret>(),
            _ => _turretGameObject.AddComponent<Turret>(),
        };
        _turret.transform.parent = _shipTurretTransform;
        _turret.turretData = _turretType;
        _turret.InitializeTurret(_hardpoint);

        return _turret;
    }

    public static int TURRET_SMALL = 0;
    public static int TURRET_MEDIUM = 1;
    public static int TURRET_LARGE = 2;
    public static int TURRET_CAPITAL = 3;

    public static float ANGLE_TOLERANCE = 5f;

    [SerializeField]
    protected TurretData turretData;

    //need to be updated
    public DamageData TurretDamageData { get; protected set; }
    public float HeatLoad { get; protected set; }
    public float EnergyCost { get; protected set; }
    public float Range { get; protected set; }
    public float Period { get; protected set; }
    public int BurstCount { get; protected set; }
    public float BurstPeriod { get; protected set; }
    public float Spread { get; protected set; }
    public float TurnRate { get; protected set; }
    public float LaunchRecoil { get; protected set; }

    //dont need to be updated
    public float Weight { get; protected set; }
    public bool IsPointDefense { get => turretData.pointDefense; }

    //other
    public string ID { get => turretData.id; }
    public Ship OwnerShip { get; set; }
    public TargetFilter PrimaryTarget { get; set; }
    [SerializeField]
    protected TargetFilter currentTarget;
    protected FactionData turretFaction { get => OwnerShip.ShipFaction; }
    private SpriteRenderer turretSpriteRenderer;
    protected float baseFacingAngle = 0f;
    protected float turningArc = -1f;
    protected int launchPointSwitch = 0;
    protected float currentAttackDelay = 0f;
    protected int burstCountRemaining = 1;
    private float retargetDelay = 0f;
    public TurretHardpoint hardpoint;
    public Transform turretVisual;
    protected AudioSource launchAudioSource;
    protected SoundData launchSound;
    protected SoundData impactSound;
    protected EffectData launchEffect;
    protected EffectData impactEffect;

    public bool Active { get; private set; } = false;

    void FixedUpdate()
    {
        if (!Active || OwnerShip.OperationalCrew < 1 && OwnerShip.UsesCrew)
        {
            return;
        }

        RotateTurret();

        if (currentTarget != null)
        {
            if (PrimaryTarget != null && currentTarget.Type == 0 && PrimaryTarget != currentTarget && CanFireAt(PrimaryTarget))
            {
                currentTarget = PrimaryTarget;
            }
            EngageCurrentTarget();
        }
        else
        {
            Retarget();
        }

        Reload();
    }

    private void Retarget()
    {
        if (retargetDelay <= 0f)
        {
            FindNewTarget();
            retargetDelay = 1f;
        }
        else
        {
            if (retargetDelay > 0f)
            {
                retargetDelay -= Time.fixedDeltaTime;
            }
        }
    }

    protected abstract void Reload();

    public void ChangeTurretType(TurretData _type)
    {
        turretData = _type;
        InitializeTurret(hardpoint);
    }

    public TurretData GetTurretType()
    {
        return turretData;
    }

    public virtual void InitializeTurret(TurretHardpoint _turretHardpoint)
    {
        hardpoint = _turretHardpoint;

        OwnerShip = GetComponentInParent<Ship>();

        turretVisual = transform.Find("Visual");
        if (turretVisual == null)
        {
            turretVisual = new GameObject("Visual").transform;
        }
        turretVisual.parent = transform;
        turretVisual.localPosition = -turretData.pivot;
        turretVisual.localRotation = new Quaternion();

        turretSpriteRenderer = turretVisual.GetComponent<SpriteRenderer>();
        if (turretSpriteRenderer == null)
        {
            turretSpriteRenderer = turretVisual.gameObject.AddComponent<SpriteRenderer>();
        }
        turretSpriteRenderer.sortingOrder = 1 + turretData.size;

        Sprite sprite;
        if (turretFaction != null)
        {
            sprite = AssetManager.Instance.GetSprite(turretData.turretSpriteID, turretFaction.variants);
        }
        else
        {
            sprite = AssetManager.Instance.GetSprite(turretData.turretSpriteID);
        }

        launchEffect = AssetManager.Instance.GetEffectData(turretData.launchEffectId);
        impactEffect = AssetManager.Instance.GetEffectData(turretData.impactEffectId);

        launchSound = AssetManager.Instance.GetSoundData(turretData.launchSoundId);
        launchSound = AssetManager.Instance.GetSoundData(turretData.launchSoundId);

        if (sprite != null)
        {
            turretSpriteRenderer.sprite = sprite;
        }

        baseFacingAngle = hardpoint.Angle;
        turningArc = hardpoint.Arc;
        transform.localRotation = Quaternion.Euler(0, 0, baseFacingAngle);
        transform.localPosition = hardpoint.Position;

        UpdateStats();

        burstCountRemaining = BurstCount;
        Active = true;
    }

    public void UpdateVisuals()
    {
        Sprite sprite;
        if (turretFaction != null)
        {
            sprite = AssetManager.Instance.GetSprite(turretData.turretSpriteID, turretFaction.variants);
        }
        else
        {
            sprite = AssetManager.Instance.GetSprite(turretData.turretSpriteID);
        }

        if (sprite != null)
        {
            turretSpriteRenderer.sprite = sprite;
        }
    }

    public virtual void UpdateStats()
    {
        StatMod _totalDamage = StatMod.Identity;
        StatMod _hullDamage = StatMod.Identity;
        StatMod _armorDamage = StatMod.Identity;
        StatMod _shieldDamage = StatMod.Identity;
        StatMod _heatDamage = StatMod.Identity;
        StatMod _crewDamage = StatMod.Identity;
        StatMod _penetration = StatMod.Identity;

        StatMod _fireRate = StatMod.Identity;
        StatMod _range = StatMod.Identity;
        StatMod _turnRate = StatMod.Identity;
        StatMod _spread = StatMod.Identity;
        StatMod _heatLoad = StatMod.Identity;
        StatMod _energyDraw = StatMod.Identity;
        StatMod _weight = StatMod.Identity;

        if (OwnerShip?.Buffs != null)
        {
            var _turretBuffs = SelectSuitableBuffs(OwnerShip.Buffs.ToArray());

            if (_turretBuffs != null)
            {
                foreach (var buff in _turretBuffs)
                {
                    _totalDamage.Add(buff.Get("TotalDamage", "Bonus"), buff.Get("TotalDamage", "Multiplier"), buff.Get("TotalDamage", "Factor"));
                    _hullDamage.Add(buff.Get("HullDamage", "Bonus"), buff.Get("HullDamage", "Multiplier"), buff.Get("HullDamage", "Factor"));
                    _armorDamage.Add(buff.Get("ArmorDamage", "Bonus"), buff.Get("ArmorDamage", "Multiplier"), buff.Get("ArmorDamage", "Factor"));
                    _shieldDamage.Add(buff.Get("ShieldDamage", "Bonus"), buff.Get("ShieldDamage", "Multiplier"), buff.Get("ShieldDamage", "Factor"));
                    _heatDamage.Add(buff.Get("HeatDamage", "Bonus"), buff.Get("HeatDamage", "Multiplier"), buff.Get("HeatDamage", "Factor"));
                    _crewDamage.Add(buff.Get("CrewDamage", "Bonus"), buff.Get("CrewDamage", "Multiplier"), buff.Get("CrewDamage", "Factor"));
                    _penetration.Add(buff.Get("Penetration", "Bonus"), buff.Get("Penetration", "Multiplier"), buff.Get("Penetration", "Factor"));

                    _fireRate.Add(0f, buff.Get("FireRate", "Multiplier"), buff.Get("FireRate", "Factor"));
                    _range.Add(buff.Get("Range", "Bonus"), buff.Get("Range", "Multiplier"), buff.Get("Range", "Factor"));
                    _turnRate.Add(buff.Get("TurnRate", "Bonus"), buff.Get("TurnRate", "Multiplier"), buff.Get("TurnRate", "Factor"));
                    _spread.Add(0f, buff.Get("Spread", "Multiplier"), buff.Get("Spread", "Factor"));
                    _heatLoad.Add(0f, buff.Get("HeatLoad", "Multiplier"), buff.Get("HeatLoad", "Factor"));
                    _energyDraw.Add(0f, buff.Get("EnergyDraw", "Multiplier"), buff.Get("EnergyDraw", "Factor"));
                    _weight.Add(0f, buff.Get("Weight", "Multiplier"), buff.Get("Weight", "Factor"));
                }
            }
        }

        TurretDamageData = new();
        var _baseDamage = turretData.damageData;

        _hullDamage.Add(_totalDamage.Bonus, _totalDamage.Mult, _totalDamage.Fact);
        _armorDamage.Add(_totalDamage.Bonus, _totalDamage.Mult, _totalDamage.Fact);
        _shieldDamage.Add(_totalDamage.Bonus, _totalDamage.Mult, _totalDamage.Fact);
        _heatDamage.Add(_totalDamage.Bonus, _totalDamage.Mult, _totalDamage.Fact);
        _crewDamage.Add(_totalDamage.Bonus, _totalDamage.Mult, _totalDamage.Fact);

        TurretDamageData.hullDamage = _hullDamage.Apply(_baseDamage.hullDamage);
        TurretDamageData.armorDamage = _armorDamage.Apply(_baseDamage.armorDamage);
        TurretDamageData.shieldDamage = _shieldDamage.Apply(_baseDamage.shieldDamage);
        TurretDamageData.heatDamage = _heatDamage.Apply(_baseDamage.heatDamage);
        TurretDamageData.crewDamage = _crewDamage.Apply(_baseDamage.crewDamage);
        TurretDamageData.penetration = _penetration.Apply(_baseDamage.penetration);

        TurretDamageData.spallingDamage = _baseDamage.spallingDamage;
        TurretDamageData.damageLossFromDistance = _baseDamage.damageLossFromDistance;
        TurretDamageData.impactForce = _baseDamage.impactForce;
        TurretDamageData.ignoresArmor = _baseDamage.ignoresArmor;
        TurretDamageData.overpenetration = _baseDamage.overpenetration;
        TurretDamageData.partialPenetration = _baseDamage.partialPenetration;
        TurretDamageData.causesHullBreach = _baseDamage.causesHullBreach;
        TurretDamageData.damageRandomness = _baseDamage.damageRandomness;
        TurretDamageData.penetrationRandomness = _baseDamage.penetrationRandomness;

        Range = _range.Apply(turretData.range);
        TurnRate = _turnRate.Apply(turretData.turnRate);
        Spread = _spread.Apply(turretData.spread);
        HeatLoad = _heatLoad.Apply(turretData.heatLoad);
        EnergyCost = _energyDraw.Apply(turretData.energyCost);
        Weight = _weight.Apply(turretData.weight);

        float fireRateMultiplier = _fireRate.Apply(1f);
        Period = turretData.period;
        BurstPeriod = turretData.burstPeriod;

        if (fireRateMultiplier > 0f)
        {
            Period /= fireRateMultiplier;
            BurstPeriod /= fireRateMultiplier;
        }

        BurstCount = turretData.burstCount;
        LaunchRecoil = turretData.launchRecoil;
    }

    protected List<BuffData.TurretBuffData> SelectSuitableBuffs(BuffData[] buffs)
    {
        List<BuffData.TurretBuffData> turretBuffs = new();

        //select buffs that pass requirements
        foreach (var buff in buffs)
        {
            if (buff.turretBuffs == null || buff.turretBuffs.Length == 0)
            {
                continue;
            }

            foreach (var turretBuff in buff.turretBuffs)
            {
                if (turretBuff.requiredTags == null || turretBuff.requiredTags.Length == 0)
                {
                    turretBuffs.Add(turretBuff);
                    continue;
                }

                int k = 0;
                for (int i = 0; i < turretBuff.requiredTags.Length; i++)
                {
                    for (int j = 0; j < turretData.tags.Length; j++)
                    {
                        if (turretData.tags[j] == turretBuff.requiredTags[i])
                        {
                            k++;
                        }
                    }
                }

                if (k == turretBuff.requiredTags.Length)
                {
                    turretBuffs.Add(turretBuff);
                }
            }
        }

        return turretBuffs;
    }

    protected void FindNewTarget()
    {
        TargetFilter _newTarget = currentTarget;
        float _max = float.MaxValue;

        if (currentTarget == null && _newTarget == null)
        {
            var _targets = TargetFilter.GetHostileTargets(turretFaction, 0);
            if (_targets != null && _targets.Count > 0)
            {
                foreach (var _target in _targets)
                {
                    if (_target != OwnerShip.ShipTargetFilter && CanFireAt(_target))
                    {
                        float _rangeSqr = (_target.transform.position - transform.position).sqrMagnitude;
                        if (_rangeSqr < _max * _max)
                        {
                            _max = _rangeSqr;
                            _newTarget = _target;
                        }
                    }
                }
            }
        }

        if (turretData.pointDefense)
        {
            if (_newTarget != null && _newTarget.Type != 2)
            {
                var _targets = TargetFilter.GetHostileTargets(turretFaction, 2);
                if (_targets != null && _targets.Count > 0)
                {
                    foreach (var _target in _targets)
                    {
                        if (CanFireAt(_target))
                        {
                            float _rangeSqr = (_target.transform.position - transform.position).sqrMagnitude;
                            if (_rangeSqr < _max * _max)
                            {
                                _max = _rangeSqr;
                                _newTarget = _target;
                            }
                        }
                    }
                }
            }

            if (_newTarget == null)
            {
                var _targets = TargetFilter.GetHostileTargets(turretFaction, 1);
                if (_targets != null && _targets.Count > 0)
                {
                    foreach (var _target in _targets)
                    {
                        if (CanFireAt(_target))
                        {
                            float _rangeSqr = (_target.transform.position - transform.position).sqrMagnitude;
                            if (_rangeSqr < _max * _max)
                            {
                                _max = _rangeSqr;
                                _newTarget = _target;
                            }
                        }
                    }
                }
            }
        }

        currentTarget = _newTarget;
    }

    private void EngageCurrentTarget()
    {
        if (CanFireAt(currentTarget))
        {
            if (turretData.launchPoints == null || turretData.launchPoints.Length < 1)
            {
                return;
            }

            if (GetAttackReady())
            {
                DrainShipVitals();
                if (turretData.alternateFire)
                {
                    Attack(launchPointSwitch);
                    SwitchLaunchPoints();
                }
                else
                {
                    for (int i = 0; i < turretData.launchPoints.Length; i++)
                    {
                        Attack(i);
                    }
                }

                if (BurstCount > 1)
                {
                    burstCountRemaining--;
                }

                if (BurstCount > 1 && burstCountRemaining > 0)
                {
                    currentAttackDelay = BurstPeriod;
                }
                else
                {
                    currentAttackDelay = Period * Random.Range(0.9f, 1.1f) / OwnerShip.OperationalCrewPercentage;
                    if (BurstCount > 1)
                    {
                        burstCountRemaining = BurstCount;
                    }
                }
            }
        }
        else
        {
            currentTarget = null;
            FindNewTarget();
            retargetDelay = Mathf.Min(1f, Period);
        }
    }

    protected abstract void Attack(int _launchPoint);

    protected virtual void SwitchLaunchPoints()
    {
        //for constant beam weapons, switch after beam duration
        launchPointSwitch = (launchPointSwitch + 1) % turretData.launchPoints.Length;
    }

    protected virtual bool CanFireAt(TargetFilter target)
    {
        Vector2 _transformPos = transform.position;
        Vector2 _targetPos = target.transform.position;

        Vector2 _delta = _targetPos - _transformPos;
        float _distSqr = _delta.sqrMagnitude;

        float _rangeSqr = Range * Range;

        if (_distSqr > _rangeSqr)
        {
            return false;
        }

        if (turningArc < 0f)
        {
            return true;
        }

        float _angleToTarget = Mathf.Atan2(_delta.y, _delta.x) * Mathf.Rad2Deg - 90f;

        float _defaultFacingAngle =
            transform.eulerAngles.z - transform.localEulerAngles.z + baseFacingAngle;

        float _deltaAngle = Mathf.DeltaAngle(_defaultFacingAngle, _angleToTarget);

        if (_deltaAngle > turningArc * 0.5f)
        {
            return false;
        }

        return true;
    }

    protected virtual bool GetAttackReady()
    {
        if (!CanDrainVitals())
        {
            return false;
        }

        if (currentAttackDelay > 0)
        {
            return false;
        }

        if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, GetAngleToTarget(currentTarget))) > ANGLE_TOLERANCE)
        {
            return false;
        }

        return true;
    }

    protected virtual float GetAngleToTarget(TargetFilter _target)
    {
        return GetAngleTowards(_target.transform.position);
    }

    protected float GetAngleTowards(Vector2 _targetVector)
    {
        Vector2 _deltaVector = _targetVector - (Vector2)transform.position;
        var _angle = Mathf.Atan2(_deltaVector.y, _deltaVector.x) * Mathf.Rad2Deg;
        _angle = (_angle + 360f) % 360f;
        if (_angle > 180f)
        {
            _angle -= 360f;
        }
        return _angle - 90f;
    }

    private void TurnTowards(float _angle, float _delta)
    {
        if (turningArc < 0f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, _angle), _delta);
        }
        else
        {
            var _defaultFacingAngle = (transform.eulerAngles.z - transform.localEulerAngles.z + baseFacingAngle + 360f) % 360f;
            if (_defaultFacingAngle > 180f)
            {
                _defaultFacingAngle -= 360f;
            }

            var _currentAngle = (transform.rotation.eulerAngles.z + 360f) % 360f;
            if (_currentAngle > 180f + _defaultFacingAngle)
            {
                _currentAngle -= 360f;
            }

            _angle = (_angle + 360f) % 360f;
            if (_angle > 180f + _defaultFacingAngle)
            {
                _angle -= 360f;
            }
            _angle = Mathf.Clamp(_angle, _defaultFacingAngle - turningArc / 2f, _defaultFacingAngle + turningArc / 2f);

            transform.rotation = Quaternion.Euler(0, 0, Mathf.MoveTowards(_currentAngle, _angle, _delta));
        }
    }

    private void RotateTurret()
    {
        float _targetAngle = currentTarget != null
            ? GetAngleToTarget(currentTarget)
            : transform.eulerAngles.z - transform.localEulerAngles.z + baseFacingAngle;
        TurnTowards(_targetAngle, TurnRate * Time.fixedDeltaTime * OwnerShip.OperationalCrewPercentage);
    }

    public Transform GetTurretVisual()
    {
        return turretVisual;
    }

    protected virtual bool CanDrainVitals()
    {
        if (!OwnerShip.CanDrainEnergy(EnergyCost))
        {
            return false;
        }

        if (!OwnerShip.CanGenerateHeat(HeatLoad))
        {
            return false;
        }

        return true;
    }

    protected virtual void DrainShipVitals()
    {
        OwnerShip.DrainEnergy(EnergyCost);
        OwnerShip.GenerateHeat(HeatLoad);
    }
}
