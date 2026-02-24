using UnityEngine;

[System.Serializable]
public record ShipData : DataDefinition
{
    [Header("Ship Info")]
    public string shipClass = "Class"; //name of the ship class
    public string shipClassSuffix = "class"; //ship class suffix
    public string shipType = "Frigate"; //frigate, line destroyer, fast cruiser...
    public string[] tags = { "frigate" }; //used for searching in shop (Escort, Cruiser, Capital, Empire, Battlecruiser, Light Cruiser, Destroyer...)
    public string[] baseBuffs = { }; //base ships buffs (eg.: line destroyers get extra turret range)
    public int cost = 500;

    [Header("Base Ship Stats")]
    public float hullHitpoints = 1000f; //maximum hull hitpoints
    public float hullRegeneration = 0f; //hull regeneration speed per second
    public int maxArmor = 10; //max number of armor platings
    public int maxShield = 10; //max number of shield generators
    public bool useEnergy = true; //whether the ship requires energy to function
    public float energyPoints = 1000f; //maximum energy
    public float energyRegeneration = 50f; //energy regeneration per second
    public bool useHeat = true; //whether the ship generates heat or takes heat damage
    public float heatThreshold = 1000f; //maximum allowed heat, before the ship starts taking damage, weapons will not shoot if ship is overheated
    public float heatDissipation = 10f; //heat dissipation per second
    public bool useCrew = true; //whether the ship uses crew
    public int maxCrew = 500; //max number of crew members
    public int operationalCrew = 400; //required number of crew members for 100% efficiency, affects turret turn rate, fire rate and ship hull regeneration
    public bool useOxygen = true; //whether the ship uses oxygen
    public float oxygenCapacity = 1000f; //maximum oxygen
    public float oxygenRegeneration = 2f; //oxygen generated per second

    [Header("Base Mobility Stats")]
    public float forwardThrusterForce = 4000f; //forward force in meganewtons, acceleration = force/current mass
    public float maneuverThrusterForce = 2000f; //lateral and rotational force in meganewtons
    public float maxSpeed = 3f; //max speed in 100s of meters per second
    public float turningRate = 5f; //max rotation speed in degrees per second
    public float weight = 2000f; //base hull weight 10s of in tonnes
    public float maxWeight = 4000f; //max ship weight 10s of in tonnes

    [Header("Equipment")]
    public TurretHardpoint[] turretHardpoints = { };
    public int moduleSlots = 6; //should scale with the ship size
    public int refitSlots = 2; //should be around 1-5, refits significantly affect ship's combat performance

    [Header("Core Data")]
    public string shipSpriteID = "sprite_ship"; //sprite reference for the ship
    public string wireframeSpriteID = "sprite_wireframe_ship"; //sprite reference for the ship wireframe
    public string shieldSpriteID = "sprite_shield"; //id reference to the sprites for the shield bubble
    public Vector2[] colliderPoints = { }; //points for constructing the collider, assigned counter-clockwise from center-top point of the ship
    public float shieldRadius = 3f;

    [Header("Misc")]
    public float shieldAlphaFull = 0.75f; //shield alpha when full
    public float shieldAlphaMinimum = 0.25f; //shield alpha when almost depleted
    public string explosionEffectId = "effect_explosion";
    public string explosionSoundId = "sound_explosion";
}
