using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class Effect : MonoBehaviour
{
    private static Dictionary<string, GameObject> effectTypePrefabs;

    public static GameObject InstantiateEffect(EffectData _effectData)
    {
        var _type = _effectData.effectType;
        if (_type == "" || (_type != "explosion" && _type != "launch" && _type != "impact"))
        {
            return null;
        }

        if (effectTypePrefabs == null)
        {
            effectTypePrefabs = new();
        }

        if (!effectTypePrefabs.ContainsKey(_type))
        {
            var _path = _type switch
            {
                "launch" => "Effects/EffectLaunch",
                "impact" => "Effects/EffectImpact",
                _ => "Effects/EffectExplosion",
            };

            var _prefab = Resources.Load(_path, typeof(GameObject)) as GameObject;
            effectTypePrefabs.Add(_type, _prefab);
        }
        var _instance = Instantiate(effectTypePrefabs[_type]);
        _instance.GetComponent<Effect>().Initialize(_effectData);

        return _instance;
    }

    [SerializeField]
    private EffectData effectData;
    [SerializeField]
    private ParticleSystem particles;

    public void Initialize(EffectData _effectData = null)
    {
        if (_effectData != null)
        {
            effectData = _effectData;
        }

        if (effectData == null)
        {
            return;
        }

        particles = GetComponent<ParticleSystem>();

        var _main = particles.main;

        _main.startSpeed = new ParticleSystem.MinMaxCurve(effectData.speedMin, effectData.speedMax);

        _main.startLifetime = new ParticleSystem.MinMaxCurve(effectData.timeMin, effectData.timeMax);

        _main.startSize = effectData.particleSize;

        _main.simulationSpeed = effectData.timeScale;


        particles.emission.SetBurst(0, new ParticleSystem.Burst(0f, effectData.particles));

        var _color = particles.colorBySpeed;
        _color.range = new Vector2(effectData.speedMax / 2f, 0f);
        _color.color = new ParticleSystem.MinMaxGradient(effectData.effectColor);

        transform.localScale = Vector3.one * effectData.effectSize;
    }

    public EffectData GetEffectData() => effectData;
}
