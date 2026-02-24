using System.Collections.Generic;
using UnityEngine;

public class StarmapObject : MonoBehaviour
{
    public static class ObjectType
    {
        public const int STAR = 0;
        public const int PLANET = 1;
        public const int MOON = 2;
    }

    public static StarmapObject Create(string _name, StarmapSystem _system, Transform _parentObject, int _type, float _orbitDistance = 0f, bool _orbit = true)
    {
        var gameObject = new GameObject(_name);
        gameObject.transform.parent = _system.transform;

        var starmapObject = gameObject.AddComponent<StarmapObject>();
        starmapObject.Type = _type;
        starmapObject.system = _system;
        starmapObject.rotationAnchor = _parentObject;
        starmapObject.orbit = _orbit;
        starmapObject.orbitDistance = _orbitDistance;
        starmapObject.orbitAngle = Random.Range(0f, 360f);
        starmapObject.orbitPeriod = _orbitDistance * 25f;
        starmapObject.Size = _type switch
        {
            ObjectType.STAR => Random.Range(2.0f, 6.0f),
            ObjectType.PLANET => Random.Range(0.12f, 0.5f),
            ObjectType.MOON => Random.Range(0.006f, Mathf.Max(0.02f, _parentObject.transform.localScale.magnitude / 6f)),
            _ => 1f,
        };
        starmapObject.rotationPeriod = 3f + 100 * starmapObject.Size * Random.Range(0.5f, 1.5f);
        gameObject.transform.localScale *= starmapObject.Size;

        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        sr.sprite = _type switch
        {
            ObjectType.STAR => AssetManager.Instance.GetSprite("sprite_starmap_star"),
            ObjectType.PLANET => AssetManager.Instance.GetSprite("sprite_starmap_planet"),
            ObjectType.MOON => AssetManager.Instance.GetSprite("sprite_starmap_planet"),
            _ => null,
        };

        Color color;
        switch (_type)
        {
            case ObjectType.STAR:
                switch (Random.Range(0, 4))
                {
                    case 0:
                        color = Color.HSVToRGB(Random.Range(0.53f, 0.57f), Random.Range(0.35f, 0.55f), 1f);
                        break;
                    case 1:
                        color = Color.HSVToRGB(Random.Range(0.53f, 0.57f), Random.Range(0f, 0.25f), 1f);
                        break;
                    case 2:
                        color = Color.HSVToRGB(Random.Range(0.11f, 0.163f), Random.Range(0.4f, 0.6f), 1f);
                        break;
                    case 3:
                        color = Color.HSVToRGB(Random.Range(0.085f, 0.145f), Random.Range(0f, 0.15f), 1f);
                        break;
                    default:
                        color = Color.HSVToRGB(Random.Range(0.01f, 0.08f), Random.Range(0.75f, 0.8f), 1f);
                        break;
                }
                break;
            default:
                color = Color.white;
                break;
        }
        sr.color = color;

        starmapObject.SubObjects = new();

        return starmapObject;
    }

    private Transform rotationAnchor;
    private StarmapSystem system;
    private float orbitDistance;
    private float orbitAngle;
    private float orbitPeriod;
    private float rotationPeriod;
    private bool orbit;
    private LineRenderer orbitLineRenderer;

    public List<StarmapObject> SubObjects { get; private set; }
    public int Type { get; private set; }
    public float Size { get; private set; }

    void Start()
    {
        if (orbit && orbitPeriod >= 0.1f)
        {
            orbitAngle = (orbitAngle + 360f / orbitPeriod * Time.fixedDeltaTime) % 360f;
            transform.position = rotationAnchor.position + new Vector3(Mathf.Cos(orbitAngle) * orbitDistance, Mathf.Sin(orbitAngle) * orbitDistance);
        }
        else
        {
            transform.position = rotationAnchor.position;
        }
    }

    void FixedUpdate()
    {
        if (orbit && orbitPeriod >= 0.1f)
        {
            orbitAngle = (orbitAngle + 360f / orbitPeriod * Time.fixedDeltaTime) % 360f;
            transform.position = rotationAnchor.position + new Vector3(Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * orbitDistance, Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * orbitDistance);
        }
        transform.localRotation = Quaternion.Euler(0, 0, transform.localEulerAngles.z + Time.fixedDeltaTime * 360f / rotationPeriod);

        if (orbit)
        {
            ShowOrbitLine(true);
        }
    }

    public void ShowOrbitLine(bool _flag)
    {
        if (_flag)
        {
            if (orbitLineRenderer == null)
            {
                orbitLineRenderer = gameObject.AddComponent<LineRenderer>();
                orbitLineRenderer.material = AssetManager.Instance.GetMaterial("TrailMaterial");

                Color _c = new(1, 1, 1, 0.05f);
                orbitLineRenderer.startColor = _c;
                orbitLineRenderer.endColor = _c;
                float _width = Type switch
                {
                    ObjectType.STAR => 0.2f,
                    ObjectType.PLANET => 0.1f,
                    ObjectType.MOON => 0.05f,
                    _ => 0.1f
                };
                orbitLineRenderer.startWidth = _width;
                orbitLineRenderer.endWidth = _width;
                orbitLineRenderer.positionCount = 360;
                orbitLineRenderer.loop = true;
                for (int i = 0; i < 360; i++)
                {
                    orbitLineRenderer.SetPosition(i, rotationAnchor.position + new Vector3(Mathf.Cos(i * Mathf.Deg2Rad) * orbitDistance, Mathf.Sin(i * Mathf.Deg2Rad) * orbitDistance));
                }
            }
            else
            {
                for (int i = 0; i < 360; i++)
                {
                    orbitLineRenderer.SetPosition(i, rotationAnchor.position + new Vector3(Mathf.Cos(i * Mathf.Deg2Rad) * orbitDistance, Mathf.Sin(i * Mathf.Deg2Rad) * orbitDistance));
                }
            }
        }
        else if (orbitLineRenderer != null)
        {
            Destroy(orbitLineRenderer);
            orbitLineRenderer = null;
        }
    }

    public void IncreaseOrbitDistance(float _distance)
    {
        orbitDistance += _distance;
    }
}
