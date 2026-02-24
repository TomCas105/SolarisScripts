public record ShipLoadoutData : DataDefinition
{
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

    public string[] modules = { };
    public string[] refits = { };
    public LoadoutTurret[] turrets = { };
}
