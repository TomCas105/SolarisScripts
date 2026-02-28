using System.Collections.Generic;

[System.Serializable]
public record FactionLoadoutSetData : DataDefinition
{
    public string factionID = "faction";
    public List<string> loadoutIDs;

    public FactionLoadoutSetData()
    {
        loadoutIDs = new();
    }

    public void Merge(FactionLoadoutSetData other)
    {
        foreach (var _loadoutID in other.loadoutIDs)
        {
            if (!loadoutIDs.Contains(_loadoutID))
            {
                loadoutIDs.Add(_loadoutID);
            }
        }
    }
}