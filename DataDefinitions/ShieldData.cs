[System.Serializable]
public record ShieldData : EquipmentBaseData
{
    public float shieldHitpoints = 75f;
    public float shieldRegeneration = 0.5f; //shield regeneration per second
    public float shieldRegenerationDelay = 10f; //delay before regeneration in seconds
    public float shieldDamageThreshold = 0.5f;
    public float energyConsumption = 0.1f;

    public float[] shieldDamageMultiplier = { 1f, 1f, 1f, 1f, 1f };
}
