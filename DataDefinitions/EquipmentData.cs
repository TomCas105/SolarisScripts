using UnityEngine;

[System.Serializable]
public record EquipmentBaseData : DataDefinition
{
    public string name = "Equipment";
    public int cost = 50; //market cost
    public float weight = 25f; // weight in 10s of tonnes
    public string[] tags = { }; //used for applying bonuses and for filtering in shop - Energy, Kinetic, Railgun, Armor, Shield, Hull, Crew...
    public TagRequirement[] requirements = { }; //equipment can only be installed on ships that have one of these requirements fulfilled

    public string iconSpriteID = "";
    public float iconScale = 1f; //scaling of the UI icon
    public Vector2 iconOffset = Vector2.zero; //offset of the UI icon
}

[System.Serializable]
public record EquipmentData : EquipmentBaseData
{
    public string equipmentType = "module"; //refit or module, module usually provides stat bonuses and have weight, refits usually provide stat multipliers or stat factors and have 0 weight
    public string linkedBuff;
}
