using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public record TurretData : EquipmentBaseData
{
    [Header("Turret Info")]
    public string weaponClass = "Cannon"; //class of the weapon, used for displaying - cannon, machine gun, railgun, missile launcher...
    public string weaponType = "ballistic"; //type of the weapon, ballistic/beam
    public int size; //turret size, 0 small, 1 medium, 2 large, 3 capital

    [Header("Base Turret Stats")]
    public DamageData damageData; //damage data
    public float heatLoad = 1f; //heat generated after attack
    public float energyCost = 1f; //energy required and consumed to attack
    public float range = 50f; //range in 100s of meters
    public float period = 5f; //delay between attacks/bursts in seconds
    public int burstCount = 1; //number of attacks in one burst, must be 1 or more
    public float burstPeriod = 0.5f; //delay between attacks in burst in seconds
    public float turnRate = 10f; //degrees per second
    public float accuracy = 5f; //accuracy in degrees, lower is better, should be 0 for constant beam weapons
    public float launchRecoil = 1f; //force in MN that pushes the ship back after attack
    public bool pointDefense = false; //whether it can target missiles / fighters

    [Header("Core Data")]
    public string turretSpriteID = "turret"; //id for sprite variants
    public Vector2[] launchPoints = { }; //positions of attack launch points
    public Vector2 pivot = Vector2.zero; //location of the pivot around which the turret rotates
    public bool alternateFire = false; //true - switch launch points with each attack, false - shoot from all launch points
    public bool trail = false; //whether to create trail behind the projectile / whether beam weapons' beam should be attached to launch point or leave trail
    public TrailData trailData = new();
    public string launchEffectId = "effect_launch";
    public string impactEffectId = "effect_impact";
    public string launchSoundId = "sound_launch";
    public string impactSoundId = "sound_impact";

    [Header("Projectile Turret Stats")] //required for projectile type weapon
    public string projectileSpriteID = "projectile"; //sprite id for the projectile
    public float projectileVelocity = 10f; //projectile velocity in 100s of meters per second
    public float projectileTurnRate = 0f; //projectile turning speed towards the target
    public bool isMissile = false; //whether the projectile is classified as missile and can be targeted by PD turrets
    public float missileHitpoints = 50f; //durability of the missile
    public float missileCollisionRadius = 0.1f; //collision radius of the missile
    public int projectilesPerAttack = 1; //number of projectiles launched in each attack, must be 1 or more

    [Header("Beam Turret Stats")] //required for beam type weapons
    public string beamSpriteVariantsId = "beam"; //sprite id for the beam
}

[System.Serializable]
public record TrailData
{
    public Color startColor = Color.yellow; //color of the trail start
    public Color endColor = Color.yellow; //color of the trail end
    public float startWidth = 0.1f; //width of the trail start
    public float endWidth = 0.1f; //width of the trail end
    public float duration = 0.25f; //duration of trail/beam in seconds before disappearing
}
