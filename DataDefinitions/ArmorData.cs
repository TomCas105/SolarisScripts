[System.Serializable]
public record ArmorData : EquipmentBaseData
{
    public float armorHitpoints = 25f;
    public float armorThickness = 3f; //armor thickness in centimeters
    public float armorRegeneration = 0f; //armor regeneration per second
    public float armorQuality = 0.05f; //% of armor hit points missing, before it starts losing thickness

    public float[] penetrationMultiplier = { 1f, 1f, 1f, 1f, 1f };
    public float[] hullDamageMultiplier = { 1f, 1f, 1f, 1f, 1f };
    public float[] armorDamageMultiplier = { 1f, 1f, 1f, 1f, 1f };
    public float crewDamageMultiplier = 1f;
    public float heatDamageMultiplier = 1f;
    public bool[] cannotBeIgnored = { false, false, false, false, false};
}
