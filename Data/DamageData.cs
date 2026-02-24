[System.Serializable]
public record DamageData
{
    public static int TOTAL = 0;
    public static int KINETIC = 1;
    public static int EXPLOSIVE = 2;
    public static int THERMAL = 3;
    public static int ENERGY = 4;

    public int type = 1; //1 kinetic, 2 explosive, 3 thermal, 4 energy
    public float hullDamage = 10;
    public float armorDamage = 1;
    public float shieldDamage = 10;
    public float damageRandomness = 0.25f; //+- hull, armor, shield damage in %, should be between 0 and 1, 0 for constant beam weapons
    public float crewDamage = 1;
    public float heatDamage = 1;
    public float penetration = 10; //armor penetration in centimeters
    public float penetrationRandomness = 0.1f; //+-penetration in %, should be between 0 and 1, 0 for constant beam weapons
    public float spallingDamage = 0f; //% of damage dealt when armor is not penetrated
    public float damageLossFromDistance = 0f; //% of hull, armor, shield damage lost at max range
    public float penetrationLossFromDistance = 0f; //% of penetration lost at max range
    public float impactForce = 1f; //force in MN that pushes the ship back when hit
    public bool ignoresArmor = false; //can deal damage through armor
    public bool overpenetration = false; //can overpenetrate (penetration > 250% armor thickness => 75% damage)
    public bool partialPenetration = false; //can partially penetrate (penetration < 110% armor thickness => 50% damage)
    public bool causesHullBreach = true;
}
