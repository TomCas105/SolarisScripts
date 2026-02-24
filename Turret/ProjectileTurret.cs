using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class ProjectileTurret : Turret
{
    private Sprite projectileSprite;

    public float ProjectileVelocity { get; private set; }
    public float ProjectileTurnRate { get; private set; }
    public float MissileHitpoints { get; private set; }


    public override void InitializeTurret(TurretHardpoint _turretHardpoint)
    {
        base.InitializeTurret(_turretHardpoint);

        Sprite sprite;
        if (turretFaction != null)
        {
            sprite = AssetManager.Instance.GetSprite(turretData.projectileSpriteID, turretFaction.variants);
        }
        else
        {
            sprite = AssetManager.Instance.GetSprite(turretData.projectileSpriteID);
        }

        if (sprite != null)
        {
            projectileSprite = sprite;
        }
    }

    public override void UpdateStats()
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

        StatMod _projectileVelocity = StatMod.Identity;
        StatMod _projectileTurnRate = StatMod.Identity;
        StatMod _missileHitpoints = StatMod.Identity;


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

                    _projectileVelocity.Add(buff.Get("ProjectileVelocity", "Bonus"), buff.Get("ProjectileVelocity", "Multiplier"), buff.Get("ProjectileVelocity", "Factor"));
                    _projectileTurnRate.Add(buff.Get("ProjectileTurnRate", "Bonus"), buff.Get("ProjectileTurnRate", "Multiplier"), buff.Get("ProjectileTurnRate", "Factor"));
                    _missileHitpoints.Add(buff.Get("MissileHitpoints", "Bonus"), buff.Get("MissileHitpoints", "Multiplier"), buff.Get("MissileHitpoints", "Factor"));
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

        ProjectileVelocity = _projectileVelocity.Apply(turretData.projectileVelocity);
        ProjectileTurnRate = _projectileTurnRate.Apply(turretData.projectileTurnRate);
        MissileHitpoints = _missileHitpoints.Apply(turretData.missileHitpoints);
    }

    protected override void Reload()
    {
        if (currentAttackDelay > -Period)
        {
            currentAttackDelay -= Time.fixedDeltaTime;
        }
        else if (BurstCount > 1 && burstCountRemaining > 0)
        {
            burstCountRemaining = BurstCount;
        }
    }

    protected override void Attack(int _launchPointIndex)
    {
        int _attacks = turretData.projectilesPerAttack;

        Vector2 _launchPointOffset = turretData.launchPoints[_launchPointIndex];
        Vector2 _launchPoint = transform.up * _launchPointOffset.y + transform.right * _launchPointOffset.x + transform.position;
        float _angle = GetAngleToTarget(currentTarget);

        while (_attacks > 0)
        {
            _attacks--;

            var _projectile = new GameObject(turretData.id + "_projectile").AddComponent<Projectile>();
            _projectile.isMissile = turretData.isMissile;
            _projectile.damageData = TurretDamageData;
            _projectile.projectileVelocity = ProjectileVelocity;
            _projectile.projectileTurnRate = ProjectileTurnRate;
            _projectile.missileHitpoints = MissileHitpoints;
            _projectile.range = Range;
            _projectile.impactEffect = impactEffect;
            _projectile.impactSound = impactSound;
            _projectile.ownerShip = OwnerShip;

            if (turretData.projectileTurnRate > 0f)
            {
                _projectile.homingTarget = currentTarget;
            }

            _projectile.transform.rotation = Quaternion.Euler(0f, 0f, _angle + Random.Range(-Spread, Spread));
            _projectile.transform.position = _launchPoint;

            _projectile.InitializeProjectile(turretFaction, projectileSprite, turretData.trail ? turretData.trailData : null);
        }

        if (launchEffect != null)
        {
            var _launchEffect = Effect.InstantiateEffect(launchEffect);
            _launchEffect.transform.position = _launchPoint;
            _launchEffect.transform.rotation = transform.rotation;

            if (launchSound != null && launchSound.LoadedSounds != null && launchSound.LoadedSounds.Count > 0)
            {
                var _audioSource = _launchEffect.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.pitch = launchSound.pitch * Random.Range(0.8f, 1.2f);
                _audioSource.volume = launchSound.volume * Random.Range(0.8f, 1.2f);
                _audioSource.spatialBlend = launchSound.spatialBlend;
                _audioSource.maxDistance = launchSound.maxDistance * Random.Range(0.8f, 1.2f);
                _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                _audioSource.clip = launchSound.LoadedSounds[0];
                _audioSource.Play();
            }
        }
    }

    protected override bool CanFireAt(TargetFilter target)
    {
        //check range
        Vector2 _transformPos = transform.position;
        Vector2 _targetPos = target.transform.position;

        Vector2 _toTarget = _targetPos - _transformPos;
        float _distSqr = _toTarget.sqrMagnitude;

        float _rangeSqr = Range * Range;

        float _approxDist = Mathf.Sqrt(_distSqr); // iba raz
        float _timeToHit = _approxDist / ProjectileVelocity;

        Vector2 _predictedPos = _targetPos + target.GetVelocity() * _timeToHit;
        Vector2 _predictedDelta = _predictedPos - _transformPos;

        if (_predictedDelta.sqrMagnitude > _rangeSqr)
        {
            return false;
        }

        //check turret attack arc
        if (turningArc < 0f)
        {
            return true;
        }

        float _angleToTarget = Mathf.Atan2(_predictedDelta.y, _predictedDelta.x) * Mathf.Rad2Deg - 90f;

        float _defaultFacingAngle = transform.eulerAngles.z - transform.localEulerAngles.z + baseFacingAngle;

        float _deltaAngle = Mathf.DeltaAngle(_defaultFacingAngle, _angleToTarget);

        if (_deltaAngle > turningArc * 0.5f)
        {
            return false;
        }

        return true;
    }

    protected override float GetAngleToTarget(TargetFilter _target)
    {
        var _targetPosition = (Vector2)_target.transform.position;
        var _vel = _target.GetVelocity();
        float _dist = Vector2.Distance(_targetPosition + _vel, transform.position);
        _targetPosition += _vel * (_dist / ProjectileVelocity);
        return GetAngleTowards(_targetPosition);
    }
}

public class Projectile : MonoBehaviour
{
    public static Material trailMaterial;

    public DamageData damageData;
    public float projectileVelocity = 10f; //projectile velocity in 100s of meters per second
    public float projectileTurnRate = 0f; //determines the turning speed of the projectile and enables homing onto the target
    public bool isMissile = false; //whether the projectile is classified as missile and can be targeted by PD turrets
    public float missileHitpoints = 50f; //durability of the missile
    public float missileCollisionRadius = 0.1f; //collison radius of the missile
    public int projectilesPerAttack = 1; //number of projectiles launched, can be used to make shotgun-type weapons
    public float range = 10f;

    public Ship ownerShip;
    public TargetFilter thisTargetFilter;
    public TargetFilter homingTarget;
    public FactionData projectileFaction;
    private SpriteRenderer projectileSpriteRenderer;
    private CircleCollider2D missileCollider;

    private float traveledDistance = 0f;
    private float activeTime = 0f;
    private bool active = false;
    private float? surfaceImpactAngle = null;

    public SoundData impactSound;
    public EffectData impactEffect;

    void FixedUpdate()
    {
        if (!active)
        {
            return;
        }

        var _velocity = GetVelocity();
        RaycastHit2D[] _hits = Physics2D.RaycastAll(transform.position, _velocity, _velocity.magnitude * Time.fixedDeltaTime);
        transform.Translate(_velocity * Time.fixedDeltaTime, Space.World);

        if (_hits != null)
        {
            foreach (var _hit in _hits)
            {
                var _target = _hit.collider.GetComponent<TargetFilter>();

                if (_target == null || _target == thisTargetFilter || !CheckAlliance(_target))
                {
                    continue;
                }

                if (DamageTarget(_target, _hit.normal))
                {
                    transform.position = _hit.point;
                    DestroyProjectile();
                    break;
                }
            }
        }

        activeTime += Time.fixedDeltaTime;
        traveledDistance += _velocity.magnitude * Time.fixedDeltaTime;

        if (activeTime > 0.5f)
        {
            if (projectileTurnRate > 0f && homingTarget != null)
            {
                var _vector = homingTarget.transform.position - transform.position;
                var _angle = Mathf.Atan2(_vector.y, _vector.x) * Mathf.Rad2Deg;
                Quaternion _current = transform.rotation;
                transform.rotation = Quaternion.RotateTowards(_current, Quaternion.Euler(new(0, 0, _angle - 90f)), Time.fixedDeltaTime * projectileTurnRate);
            }
        }

        if (traveledDistance > range || activeTime > 2 * range / projectileVelocity)
        {
            DestroyProjectile();
        }

        if (isMissile && missileHitpoints < 0f)
        {
            DestroyProjectile();
        }
    }

    public void InitializeProjectile(FactionData _faction, Sprite _sprite, TrailData _trailData = null)
    {
        projectileFaction = _faction;

        if (isMissile)
        {
            if (thisTargetFilter == null)
            {
                thisTargetFilter = gameObject.AddComponent<TargetFilter>();
            }
            thisTargetFilter.TargetFaction = projectileFaction;

            if (missileCollider == null)
            {
                missileCollider = gameObject.AddComponent<CircleCollider2D>();
            }
            missileCollider.isTrigger = true;
            missileCollider.radius = missileCollisionRadius;
        }

        if (_sprite != null)
        {
            projectileSpriteRenderer = GetComponent<SpriteRenderer>();
            if (projectileSpriteRenderer == null)
            {
                projectileSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            projectileSpriteRenderer.sprite = _sprite;
            projectileSpriteRenderer.sortingOrder = 3;
        }

        if (_trailData != null)
        {
            var _trailRenderer = GetComponent<TrailRenderer>();
            if (_trailRenderer == null)
            {
                _trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            if (trailMaterial == null)
            {
                trailMaterial = AssetManager.Instance.GetMaterial("TrailMaterial");
            }

            _trailRenderer.startWidth = _trailData.startWidth;
            _trailRenderer.endWidth = _trailData.endWidth;
            _trailRenderer.startColor = _trailData.startColor;
            _trailRenderer.endColor = _trailData.endColor;
            _trailRenderer.time = _trailData.duration;
            _trailRenderer.sortingOrder = 2;
            _trailRenderer.materials = new Material[] { trailMaterial };
        }

        traveledDistance = 0f;
        activeTime = 0f;
        active = true;
    }

    public bool TakeDamage(float _amount)
    {
        if (active && isMissile)
        {
            missileHitpoints -= _amount;
            return true;
        }
        return false;
    }

    public Vector2 GetVelocity()
    {
        return transform.up * projectileVelocity;
    }

    private bool CheckAlliance(TargetFilter _target)
    {
        if (_target == null || projectileFaction == null || _target.TargetFaction == null)
        {
            return false;
        }

        if (projectileFaction.hostileFactions.Contains(_target.TargetFaction.id))
        {
            return true;
        }

        return false;
    }

    private bool DamageTarget(TargetFilter _target, Vector2 _hitNormal)
    {
        float _surfaceAngle = Mathf.Atan2(_hitNormal.y, _hitNormal.x) * Mathf.Rad2Deg;
        var _projectileDirection = -transform.up;
        float _projectileAngle = Mathf.Atan2(_projectileDirection.y, _projectileDirection.x) * Mathf.Rad2Deg;
        float _impactAngle = 90f - Mathf.Abs(Mathf.DeltaAngle(_surfaceAngle, _projectileAngle));
        surfaceImpactAngle = _surfaceAngle - 90f;

        return _target.TakeDamage(damageData, ownerShip, traveledDistance, range, _impactAngle);
    }

    private void DestroyProjectile()
    {
        if (impactEffect != null)
        {
            var _impactEffect = Effect.InstantiateEffect(impactEffect);
            _impactEffect.transform.position = transform.position;
            if (surfaceImpactAngle == null)
            {
                _impactEffect.transform.rotation = Quaternion.Euler(0, 0, -transform.eulerAngles.z);
            }
            else
            {
                _impactEffect.transform.rotation = Quaternion.Euler(0, 0, (float)surfaceImpactAngle);
            }

            if (impactSound != null && impactSound.LoadedSounds != null && impactSound.LoadedSounds.Count > 0)
            {
                var _audioSource = _impactEffect.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.pitch = impactSound.pitch;
                _audioSource.volume = impactSound.volume;
                _audioSource.spatialBlend = impactSound.spatialBlend;
                _audioSource.maxDistance = impactSound.maxDistance;
                _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                _audioSource.clip = impactSound.LoadedSounds[0];
                _audioSource.Play();
            }
        }

        var _trailRenderer = GetComponent<TrailRenderer>();
        active = false;

        if (projectileSpriteRenderer != null)
        {
            projectileSpriteRenderer.enabled = false;
        }

        if (missileCollider != null)
        {
            missileCollider.enabled = false;
        }

        if (_trailRenderer == null)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(FadeOut(_trailRenderer));
        Destroy(GetComponent<TargetFilter>());
        Destroy(gameObject, _trailRenderer.time);
    }

    private IEnumerator FadeOut(TrailRenderer _trailRenderer)
    {
        float _r = _trailRenderer.startColor.r;
        float _g = _trailRenderer.startColor.g;
        float _b = _trailRenderer.startColor.b;
        float _a = _trailRenderer.startColor.a;
        float _time = _trailRenderer.time;

        float newAlpha = 1f;
        int j = (int)(_time / 0.05f);

        for (int i = 0; i < j; i++)
        {
            newAlpha -= _a / j;
            Color newColor = new(_r, _g, _b, newAlpha);
            _trailRenderer.startColor = newColor;
            yield return new WaitForSeconds(1 / (float)j);
        }
    }
}
