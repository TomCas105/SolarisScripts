public enum StatType
{
    // Hull
    HullHP,
    HullRegen,
    HullDrain,
    HullWeight,

    // Armor
    ArmorHP,
    ArmorRegen,
    ArmorDrain,
    ArmorCount,
    ArmorThickness,
    ArmorQuality,
    ArmorWeight,

    // Shield
    ShieldHP,
    ShieldRegen,
    ShieldDrain,
    ShieldDelay,
    ShieldCount,
    ShieldDamageThreshold,
    ShieldEnergyConsumption,
    ShieldWeight,

    // Energy
    EnergyCapacity,
    EnergyRegen,
    EnergyDrain,

    // Heat
    HeatThreshold,
    HeatDissipation,
    HeatBuildup,

    // Crew
    CrewCapacity,
    CrewRegen,
    CrewDrain,
    CrewHullRepairSpeed,
    CrewBreachRepairSpeed,

    // Mobility
    MaxSpeed,
    TurnRate,
    MainThruster,
    ManeuverThruster,

    //Other
    WeightLimit,

    // Turret
    FireRate,
    Range,
    ProjectileSpeed,
    Accuracy,
    TotalDamage,
    HullDamage,
    ArmorDamage,
    ShieldDamage,
    HeatDamage,
    CrewDamage,
    Penetration,
    HeatLoad,
    EnergyDraw,
    MissileTurtRate,
    MissileHitpoints,
}

public struct StatMod
{
    public float Bonus;
    public float Mult;
    public float Fact;

    public static StatMod Identity => new StatMod { Bonus = 0f, Mult = 0f, Fact = 1f };

    public void Add(float bonus, float mult, float fact)
    {
        Bonus += bonus;
        Mult += mult;
        Fact *= fact;
    }

    public float Apply(float baseValue)
    {
        return (baseValue * (1 + Mult) + Bonus) * Fact;
    }

    public int ApplyInt(float baseValue)
    {
        return (int)Apply(baseValue);
    }
}