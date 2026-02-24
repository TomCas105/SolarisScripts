using System.Collections.Generic;
using UnityEngine;

public class StarmapSystem : MonoBehaviour
{
    public static StarmapSystem Create(string name, Vector2 position, int planets)
    {
        GameObject _go = new GameObject(name);
        _go.transform.position = position;
        _go.isStatic = true;
        var _system = _go.AddComponent<StarmapSystem>();
        _system.ConnectedSystems = new();
        _system.SystemSpriteRenderer = _go.AddComponent<SpriteRenderer>();
        _system.SystemSpriteRenderer.sprite = AssetManager.Instance.GetSprite("sprite_starmap_system");
        _system.SystemSpriteRenderer.sortingOrder = 10;
        _system.SystemCollider = _go.AddComponent<CircleCollider2D>();
        _system.SystemCollider.radius = 50f;
        return _system;

        //stars
        _system.Stars = new();
        _system.Stars.Add(StarmapObject.Create(name, _system, _go.transform, StarmapObject.ObjectType.STAR, _orbit: false));

        //planets and moons
        _system.Planets = new();
        float _orbitDistance = Random.Range(0.75f, 1.25f) * _system.Stars[0].Size + _system.Stars[0].Size;
        for (int i = 0; i < planets; i++)
        {
            var _planetName = name + $"-{(char)(i + 'A')}";
            var _newPlanet = StarmapObject.Create(_planetName, _system, _system.transform, StarmapObject.ObjectType.PLANET, _orbitDistance);
            _system.Planets.Add(_newPlanet);

            int _moons = Random.Range(0f, 1f) < _newPlanet.Size * 2f ? (int)Random.Range(1f, 20f * _newPlanet.Size) : 0;
            float _moonOrbitDistance = Random.Range(0.45f, 0.6f) * _newPlanet.Size + _newPlanet.Size;
            for (int j = 0; j < _moons; j++)
            {
                _newPlanet.SubObjects.Add(StarmapObject.Create(_planetName + $"{j + 1}", _system, _newPlanet.transform, StarmapObject.ObjectType.MOON, _moonOrbitDistance));
                _moonOrbitDistance += Random.Range(0.45f, 0.6f) * _newPlanet.Size;
            }
            _newPlanet.IncreaseOrbitDistance(_moonOrbitDistance + _newPlanet.Size);
            _orbitDistance += _newPlanet.Size + 2 * _moonOrbitDistance + Random.Range(0.75f, 1.25f) * _system.Stars[0].Size;
        }

        _system.Size = _orbitDistance * 1.25f;
    }

    public string Name { get; private set; }
    public List<StarmapObject> Stars { get; private set; }
    public List<StarmapObject> Planets { get; private set; }
    public float Size { get; private set; }
    public SpriteRenderer SystemSpriteRenderer { get; private set; }
    public CircleCollider2D SystemCollider { get; private set; }
    public List<StarmapSystem> ConnectedSystems;
    public string owningFaction;

    void Start()
    {

    }

    void FixedUpdate()
    {

    }
}
