[System.Serializable]
public record ShipLoadoutData : DataDefinition
{
    [System.Serializable]
    public record LoadoutTurret
    {
        public string turretID = "turret";
        public string hardpoint = "s0";
    }

    public string shipID = "ship";

    public string armorID = "armor";
    public int armorCount = 0;
    public string shieldID = "shield";
    public int shieldCount = 0;

    public string[] equipment = { };
    public LoadoutTurret[] turrets = { };
}
