[System.Serializable]
public record BuffData : DataDefinition
{
    //bonus is added to the base value before multipliers and factors, eg.: stat + bonus1 + bonus2
    //multipliers are added together and multiply base value, eg.: (stat + bonus1 + bonus2) + stat * (multiplier1 + multiplier2), multipliers should be only positive numbers to avoid negative stat values, but there are exceptions
    //factors are applied after multipliers, eg.: ((stat + bonus1 + bonus2) + stat * (multiplier1 + multiplier2)) * factor1 * factor2
    
    [System.Serializable]
    public record TurretBuffData
    {
        public string[] requiredTags; //turret must have all these tags for buff to apply
        public StatValue[] statValues;

        public float Get(string stat, string modifier)
        {
            stat = stat.ToLower();
            modifier = modifier.ToLower();

            foreach (var statValue in statValues)
            {
                if (statValue.stat.ToLower() == stat && statValue.modifier.ToLower() == modifier)
                {
                    return statValue.value;
                }
            }

            if (modifier == "bonus")
            {
                return 0;
            }
            else if (modifier == "multiplier")
            {
                return 0;
            }
            else if (modifier == "factor")
            {
                return 1;
            }

            return 0;
        }
    }

    [System.Serializable]
    public struct StatValue
    {
        public string stat;
        public string modifier;
        public float value;
    }

    //turrets
    public TurretBuffData[] turretBuffs;
    public StatValue[] statValues = { };

    public float Get(string stat, string modifier)
    {
        stat = stat.ToLower();
        modifier = modifier.ToLower();

        foreach (var statValue in statValues)
        {
            if (statValue.stat.ToLower() == stat && statValue.modifier.ToLower() == modifier)
            {
                return statValue.value;
            }
        }

        if (modifier == "bonus")
        {
            return 0;
        }
        else if (modifier == "multiplier")
        {
            return 0;
        }
        else if (modifier == "factor")
        {
            return 1;
        }

        return 0;
    }
}

/*
hullHitpointsBonus = 0;
hullHitpointsMultiplier = 0;
hullHitpointsFactor = 1;
hullRegenerationBonus = 0;
hullRegenerationMultiplier = 0;
hullRegenerationFactor = 1;
hullWeightMultiplier = 0;
hullWeightFactor = 1;
maxWeightBonus = 0;
maxWeightMultiplier = 0;
maxWeightFactor = 1;

armorCountBonus = 0;
armorCountMultiplier = 0;
armorCountFactor = 1;
armorHitpointsBonus = 0;
armorHitpointsMultiplier = 0;
armorHitpointsFactor = 1;
armorRegenerationBonus = 0;
armorRegenerationMultiplier = 0;
armorRegenerationFactor = 1;
armorQualityBonus = 0;
armorQualityMultiplier = 0;
armorQualityFactor = 1;
armorThicknessBonus = 0;
armorThicknessMultiplier = 0;
armorThicknessFactor = 1;
armorWeightMultiplier = 0;
armorWeightFactor = 1;

shieldCountBonus = 0;
shieldCountMultiplier = 0;
shieldCountFactor = 1;
shieldHitpointsBonus = 0;
shieldHitpointsMultiplier = 0;
shieldHitpointsFactor = 1;
shieldRegenerationBonus = 0;
shieldRegenerationMultiplier = 0;
shieldRegenerationFactor = 1;
shieldRegenerationDelayMultiplier = 0; //lower = better
shieldRegenerationDelayFactor = 1; //lower = better
shieldDamageThresholdBonus = 0;
shieldDamageThresholdMultiplier = 0;
shieldDamageThresholdFactor = 1;
shieldWeightMultiplier = 0;
shieldWeightFactor = 1;
shieldEnergyConsumptionMultiplier = 0;
shieldEnergyConsumptionFactor = 1;

energyCapacityBonus = 0;
energyCapacityMultiplier = 0;
energyCapacityFactor = 1;
energyRegenerationBonus = 0;
energyRegenerationMultiplier = 0;
energyRegenerationFactor = 1;

heatThresholdBonus = 0;
heatThresholdMultiplier = 0;
heatThresholdFactor = 1;
heatDissipationBonus = 0;
heatDissipationMultiplier = 0;
heatDissipationFactor = 1;
heatDamageMultiplier = 0;
heatDamageFactor = 1;

oxygenCapacityBonus = 0;
oxygenCapacityMultiplier = 0;
oxygenCapacityFactor = 1;
oxygenGenerationBonus = 0;
oxygenGenerationMultiplier = 0;
oxygenGenerationFactor = 1;

crewCapacityBonus = 0;
crewCapacityMultiplier = 0;
crewCapacityFactor = 1;
operationalCrewBonus = 0;
operationalCrewMultiplier = 0;
operationalCrewFactor = 1;
crewRegenerationBonus = 0;
crewRegenerationFactor = 1;
crewHullRepairSpeedMultiplier = 0;
crewHullRepairSpeedFactor = 1;
crewBreachRepairSpeedMultiplier = 0;
crewBreachRepairSpeedFactor = 1;
crewRepairCapacityBonus = 0;
crewRepairCapacityMultiplier = 0;
crewRepairCapacityFactor = 1;

maxSpeedBonus = 0;
maxSpeedMultiplier = 0;
maxSpeedFactor = 1;
turnRateBonus = 0;
turnRateMultiplier = 0;
turnRateFactor = 1;
mainThrusterPowerBonus = 0;
mainThrusterPowerMultiplier = 0;
mainThrusterPowerFactor = 1;
maneuverThrusterPowerBonus = 0;
maneuverThrusterPowerMultiplier = 0;
maneuverThrusterPowerFactor = 1;

fireRateMultiplier = 0; //higher = better
fireRateFactor = 1; //higher = better
rangeBonus = 0;
rangeMultiplier = 0;
rangeFactor = 1;
projectileSpeedBonus = 0;
projectileSpeedMultiplier = 0;
projectileSpeedFactor = 1;
projectileTurnRateBonus = 0;
projectileTurnRateMultiplier = 0;
projectileTurnRateFactor = 1;
missileHitpointsBonus = 0;
missileHitpointsMultiplier = 0;
missileHitpointsFactor = 1;
accuracyMultiplier = 0; //higher = better
accuracyFactor = 1; //higher = better
turnRateBonus = 0;
turnRateMultiplier = 0;
turnRateFactor = 1;
weightMultiplier = 0;
weightFactor = 1;
totalDamageMultiplier = 0;
totalDamageFactor = 1;
hullDamageMultiplier = 0;
hullDamageFactor = 1;
armorDamageMultiplier = 0;
armorDamageFactor = 1;
shieldDamageMultiplier = 0;
shieldDamageFactor = 1;
heatDamageMultiplier = 0;
heatDamageFactor = 1;
crewDamageMultiplier = 0;
crewDamageFactor = 1;
penetrationBonus = 0;
penetrationMultiplier = 0;
penetrationFactor = 1;
heatLoadMultiplier = 0; //lower = better
heatLoadFactor = 1; //lower = better
energyDrawMultiplier = 0; //lower = better
energyDrawFactor = 1; //lower = better
*/
