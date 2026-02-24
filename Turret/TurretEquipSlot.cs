using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class TurretEquipSlot : MonoBehaviour
{
    [SerializeField]
    private LineRenderer radiusRenderer;
    [SerializeField]
    private LineRenderer arcRenderer;

    public TurretHardpoint Hardpoint { get; private set; }

    public bool Highlighted { get; set; } = false;

    [SerializeField]
    private Color radiusColorDefault;
    [SerializeField]
    private Color radiusColorHighlighted;
    [SerializeField]
    private Color arcColorDefault;
    [SerializeField]
    private Color arcColorHighlighted;

    void Start()
    {
        radiusRenderer.startColor = radiusColorDefault;
        radiusRenderer.endColor = radiusColorDefault;
        arcRenderer.startColor = arcColorDefault;
        arcRenderer.endColor = arcColorDefault;
    }

    void Update()
    {
        if (Highlighted)
        {
            Color _c = Color.Lerp(radiusRenderer.startColor, radiusColorHighlighted, 15f * Time.unscaledDeltaTime);
            radiusRenderer.startColor = _c;
            radiusRenderer.endColor = _c;
            _c = Color.Lerp(arcRenderer.startColor, arcColorHighlighted, 15f * Time.unscaledDeltaTime);
            arcRenderer.startColor = _c;
            arcRenderer.endColor = _c;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.3f, 1.3f), 15f * Time.unscaledDeltaTime);
            radiusRenderer.sortingOrder = 12;
            arcRenderer.sortingOrder = 11;
        }
        else
        {
            Color _c = Vector4.MoveTowards(radiusRenderer.startColor, radiusColorDefault, 10f * Time.unscaledDeltaTime);
            radiusRenderer.startColor = _c;
            radiusRenderer.endColor = _c;
            _c = Vector4.MoveTowards(arcRenderer.startColor, arcColorDefault, 10f * Time.unscaledDeltaTime);
            arcRenderer.startColor = _c;
            arcRenderer.endColor = _c;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 10f * Time.unscaledDeltaTime);
            radiusRenderer.sortingOrder = 10;
            arcRenderer.sortingOrder = 9;
        }
    }

    public void SetHardpoint(TurretHardpoint _turretHardpoint)
    {
        Hardpoint = _turretHardpoint;

        if (Hardpoint == null)
        {
            return;
        }

        float _radius = 0.15f * (Hardpoint.Size + 1) - 0.04f * Hardpoint.Size;
        GetComponent<CircleCollider2D>().radius = _radius * 1.05f + 0.08f;
        radiusRenderer.positionCount = 36;
        for (int i = 0; i < 36; i++)
        {
            radiusRenderer.SetPosition(i, _radius * new Vector3(Mathf.Cos(i * 10f * Mathf.Deg2Rad), Mathf.Sin(i * 10f * Mathf.Deg2Rad)));
        }
        transform.rotation = Quaternion.Euler(0, 0, Hardpoint.Angle + 90f);

        float _arc = Hardpoint.Arc;
        int _positions;
        float _pointDistance;
        float _parentAngle = transform.parent.eulerAngles.z;
        if (_arc < 0f)
        {
            _positions = 36;
            _pointDistance = 10f;
            arcRenderer.loop = true;
        }
        else
        {
            _positions = (int)(Hardpoint.Arc / 10f) + 1;
            _pointDistance = _arc / (_positions - 1);
            arcRenderer.loop = false;
        }
        arcRenderer.positionCount = _positions;
        for (int i = 0; i < _positions; i++)
        {
            arcRenderer.SetPosition(i, _radius * 3 * new Vector3(Mathf.Cos((i * _pointDistance - Hardpoint.Arc / 2 + _parentAngle) * Mathf.Deg2Rad), Mathf.Sin((i * _pointDistance - Hardpoint.Arc / 2 + _parentAngle) * Mathf.Deg2Rad)));
        }
    }
}
