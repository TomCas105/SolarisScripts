using UnityEngine;

[System.Serializable]
public record EffectData : DataDefinition
{
    public string effectType = "explosion"; //explosion, launch, impact, particle - determines the shape and other attributes of the effect
    public Gradient effectColor;
    public int particles = 30;
    public float effectSize = 1f;
    public float particleSize = 1f;
    public float timeScale = 1f;
    public float timeMin = 1f;
    public float timeMax = 5f;
    public float speedMin = 5f;
    public float speedMax = 15f;
}
