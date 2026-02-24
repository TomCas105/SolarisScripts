using System.Collections.Generic;
using UnityEngine;

public class Starmap : MonoBehaviour
{
    private static Starmap instance;

    public static Starmap GetStarmap()
    {
        if (instance == null)
        {
            new Starmap();
        }
        return instance;
    }

    private List<StarmapSystem> systems;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var _am = AssetManager.Instance;

        _am.LoadModuleInfos();

        _am.LoadModule("core");

        GenerateSystems(500);
    }

    void FixedUpdate()
    {
        for (int i = 0; i < systems.Count; i++)
        {
            for (int j = 0; j < systems[i].ConnectedSystems.Count; j++)
            {
                Debug.DrawLine(systems[i].transform.position, systems[i].ConnectedSystems[j].transform.position, Color.red);
            }
        }
    }

    public void GenerateSystems(int count)
    {
        systems = new();
        for (int i = 0; i < count; i++)
        {
            Vector2 _point = Vector2.zero;
            bool _valid = false;
            int iters = 0;

            while (!_valid && iters < 100)
            {
                _point = new Vector2(Random.Range(-5 * count, 5 * count), Random.Range(-5 * count, 5 * count));

                _valid = Physics2D.CircleCastAll(_point, 100f, Vector3.back).Length < 1;

                iters++;
            }

            if (iters < 100)
            {
                systems.Add(StarmapSystem.Create($"{i}", _point, Random.Range(0, 10)));
            }
        }

        for (int a = 0; a < count; a++)
        {
            for (int b = 0; b < count; b++)
            {
                if (a != b)
                {
                    float _dist = Vector2.Distance(systems[a].transform.position, systems[b].transform.position);
                    bool _valid = true;

                    for (int c = 0; c < count; c++)
                    {
                        if (c == a || c == b)
                        {
                            continue;
                        }

                        if (Vector2.Distance(systems[c].transform.position, systems[a].transform.position) < _dist
                        && Vector2.Distance(systems[c].transform.position, systems[b].transform.position) < _dist)
                        {
                            _valid = false;
                            break;
                        }
                    }

                    if (_valid)
                    {
                        if (!systems[a].ConnectedSystems.Contains(systems[b]))
                        {
                            systems[a].ConnectedSystems.Add(systems[b]);
                        }

                        if (!systems[b].ConnectedSystems.Contains(systems[a]))
                        {
                            systems[b].ConnectedSystems.Add(systems[a]);
                        }
                    }
                }
            }
        }

        List<StarmapSystem> _freeSystems = new(systems);
        foreach (var _faction in AssetManager.Instance.factionDatas.Values)
        {
            var _system = _freeSystems[Random.Range(0, _freeSystems.Count)];
            var _takenSystems = InitFaction(_system, _faction);
            foreach (var _takenSystem in _takenSystems)
            {
                _freeSystems.Remove(_takenSystem);
            }
        }
    }

    public List<StarmapSystem> InitFaction(StarmapSystem _system, FactionData _faction)
    {
        List<StarmapSystem> _systems = new();
        _systems.Add(_system);

        Color c = Color.HSVToRGB(Random.Range(0f, 1f), 1, 1);

        foreach (var _system1 in _system.ConnectedSystems)
        {
            if (!(_system1.owningFaction == "" || _system1.owningFaction == null))
            {
                continue;
            }

            if (!_systems.Contains(_system1))
            {
                _systems.Add(_system1);
            }

            foreach (var _system2 in _system1.ConnectedSystems)
            {
                if (!(_system2.owningFaction == "" || _system2.owningFaction == null))
                {
                    continue;
                }

                if (!_systems.Contains(_system2))
                {
                    _systems.Add(_system2);
                }

                foreach (var _system3 in _system2.ConnectedSystems)
                {
                    if (!(_system3.owningFaction == "" || _system3.owningFaction == null))
                    {
                        continue;
                    }

                    if (!_systems.Contains(_system3))
                    {
                        _systems.Add(_system3);
                    }
                }
            }
        }

        foreach (var _factionSystem in _systems)
        {
            _factionSystem.owningFaction = _faction.id;
            _factionSystem.SystemSpriteRenderer.color = c;
        }

        return _systems;
    }
}
