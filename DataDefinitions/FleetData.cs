using UnityEngine;

[System.Serializable]
public record FleetData
{
    public string factionID = "player"; //id of the fleet faction
    public FleetShipData[] fleetShips;
}
