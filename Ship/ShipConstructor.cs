using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipConstructor : Ship
{
    public void MirrorColliderPoints()
    {
        List<Vector2> _tempPoints = new();

        for (int i = 0; i < shipType.colliderPoints.Length; i++)
        {
            if (shipType.colliderPoints[i].x <= 0f)
            {
                _tempPoints.Add(shipType.colliderPoints[i]);
            }
        }

        for (int i = shipType.colliderPoints.Length - 1; i >= 0; i--)
        {
            if (shipType.colliderPoints[i].x <= 0f)
            {
                _tempPoints.Add(new(-shipType.colliderPoints[i].x, shipType.colliderPoints[i].y));
            }
        }

        List<Vector2> _newPoints = new();
        for (int i = 0; i < _tempPoints.Count; i++)
        {
            if (i == 0 || i == _tempPoints.Count / 2 - 1 || _tempPoints[i].x != 0f || _tempPoints[_tempPoints.Count - 1 - i].x != 0f || _tempPoints[i].y != _tempPoints[_tempPoints.Count - 1 - i].y)
            {
                _newPoints.Add(_tempPoints[i]);
            }
        }

        shipType.colliderPoints = _newPoints.ToArray();
    }

    public void LoadColliderPoints()
    {
        if (shipCollider == null && GetComponent<PolygonCollider2D>() != null)
        {
            shipCollider = GetComponent<PolygonCollider2D>();
        }
        shipType.colliderPoints = shipCollider.points;
    }

    public void UpdateBaseData()
    {
        shipType.colliderPoints = shipCollider.points;

        foreach (var _turretHardpoint in TurretHardpoints)
        {
            _turretHardpoint.Angle = _turretHardpoint.Turret.transform.localEulerAngles.z;
            _turretHardpoint.Position = _turretHardpoint.Turret.transform.localPosition;
        }
    }
}
