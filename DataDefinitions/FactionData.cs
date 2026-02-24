using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public record FactionData : DataDefinition
{
    public string name;
    public Color factionColor;
    public List<string> variants;
    public List<string> shipRooster;
    public List<string> alliedFactions;
    public List<string> hostileFactions;
    public int startingTerritorySize;
    public bool multipleStartingTerritories;
}
