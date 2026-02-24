[System.Serializable]
public record FleetShipData
{
    [System.Serializable]
    public record EquippedTurret
    {
        public string hardpointID;
        public string turretID;
    }

    public string shipTypeID = "ship"; //id of the ship type that is used

    //vitals
    public float currentHullHitpoints; //current hull hitpoints of the ship
    public float maxHullHitpoints; //max hull hitpoints of the ship
    public float hullRepairLeft; //remaining hitpoints that can be repaired by crew
    public float currentArmorHitpoints; //current armor hitpoints of the ship
    public float maxArmorHitpoints; //max armor hitpoints of the ship
    public float currentShieldHitpoints; //current ship hitpoints of the ship
    public float maxShieldHitpoints; //max ship hitpoints of the ship
    public float currentCrew; //current crew of the ship
    public float maxCrew; //max crew of the ship

    //equipment
    public string equippedArmorTypeID = "";
    public int equippedArmorCount = 0;
    public string equippedShieldTypeID = "";
    public int equippedShieldCount = 0;
    public EquippedTurret[] turrets;
    public string[] equippedEquipment;

    public Ship assignedShip;

    public FleetShipData(FleetShipData copy)
    {
        shipTypeID = copy.shipTypeID;
        assignedShip = copy.assignedShip;
        equippedArmorTypeID = copy.equippedArmorTypeID;
        equippedArmorCount = copy.equippedArmorCount;
        equippedShieldTypeID = copy.equippedShieldTypeID;
        equippedShieldCount = copy.equippedShieldCount;
        currentArmorHitpoints = copy.currentArmorHitpoints;
        maxHullHitpoints = copy.maxHullHitpoints;
        hullRepairLeft = copy.hullRepairLeft;
        currentArmorHitpoints = copy.currentArmorHitpoints;
        currentShieldHitpoints = copy.currentShieldHitpoints;
        maxArmorHitpoints = copy.maxArmorHitpoints;
        maxShieldHitpoints = copy.maxShieldHitpoints;
        currentCrew = copy.currentCrew;
        maxCrew = copy.maxCrew;

        turrets = new EquippedTurret[copy.turrets.Length];
        for (int i = 0; i < turrets.Length; i++)
        {
            turrets[i].turretID = copy.turrets[i].turretID;
            turrets[i].hardpointID = copy.turrets[i].hardpointID;
        }

        equippedEquipment = new string[copy.equippedEquipment.Length];
        for (int i = 0; i < equippedEquipment.Length; i++)
        {
            equippedEquipment[i] = copy.equippedEquipment[i];
        }
    }
}
