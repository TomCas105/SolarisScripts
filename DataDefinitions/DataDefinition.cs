[System.Serializable]
public record DataDefinition
{
    public string id = "null";
    public string dataType = "none";

    public virtual void OnPostLoad(string moduleRootPath) { }
}

[System.Serializable]
public struct TagRequirement
{
    public string[] requiredTags;
}
