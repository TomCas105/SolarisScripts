using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileTurret))]
public class TurretEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ProjectileTurret _turret = (ProjectileTurret)target;

        string _size = _turret.GetTurretType().size switch
        {
            0 => "Small/",
            1 => "Medium/",
            2 => "Large/",
            _ => "Capital/",
        };

        if (GUILayout.Button("Import Turret"))
        {
            var _path = EditorInputDialog.Show("Import Turret", "Enter file path:", "Data/Base/Core/Equipment/Turrets/" + _size + _turret.GetTurretType().id + ".json");

            var _file = File.OpenText(Application.dataPath + "/" + _path);
            if (_file != null && _path != "Data/Base/Core/Defs/Equipment/Turrets/" + _size)
            {
                var json = _file.ReadToEnd();
                TurretData _turretType = new();
                JsonUtility.FromJsonOverwrite(json, _turretType);
                _turret.ChangeTurretType(_turretType);
                _file.Close();
            }
            else
            {
                Debug.Log("File " + _path + " not found!");
            }
        }

        if (GUILayout.Button("Export Turret"))
        {
            var _path = EditorInputDialog.Show("Export Turret", "Enter file path:", "Data/Base/Core/Equipment/Turrets/" + _size + _turret.GetTurretType().id + ".json");

            if (_path != "Data/Base/Core/Defs/Equipment/Turrets/" + _size)
            {
                SaveIntoJson(_turret.GetTurretType(), _path);
            }
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Initialize Turret"))
        {
            var _hardpoint = new TurretHardpoint();
            _hardpoint.Arc = 180f;
            _hardpoint.Angle = 0f;
            _hardpoint.Id = "s0";
            _hardpoint.Size = 0;

            Ship _ownerShip = _turret.GetComponentInParent<Ship>();
            if (_ownerShip != null)
            {
                foreach (var _existingHardpoint in _ownerShip.GetShipType().turretHardpoints)
                {
                    if (_existingHardpoint.Id == _turret.hardpoint.Id)
                    {
                        _hardpoint.Arc = _existingHardpoint.Arc;
                        _hardpoint.Angle = _existingHardpoint.Angle;
                        _hardpoint.Id = _existingHardpoint.Id;
                        _hardpoint.Size = _existingHardpoint.Size;
                        _hardpoint.Position = _existingHardpoint.Position;
                        Debug.Log("Existing hardpoint found with id " + _hardpoint.Id + ", using existing hardpoint data.");
                        break;
                    }
                }
            }

            _turret.InitializeTurret(_hardpoint);
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Move Turret Hardpoint"))
        {
            _turret.hardpoint.Position = _turret.transform.localPosition;
        }
    }
    public void OnSceneGUI()
    {
        ProjectileTurret _turret = target as ProjectileTurret;

        if (_turret.GetTurretType().launchPoints != null && _turret.GetTurretType().launchPoints.Length > 0)
        {
            foreach (var _launchPoint in _turret.GetTurretType().launchPoints)
            {
                Vector3 _offset = RotatePointAroundPivot(_launchPoint, Vector3.zero, _turret.transform.eulerAngles);
                Vector3 _position = _offset + _turret.transform.position;
                Vector3 _sideOffset = RotatePointAroundPivot(new Vector2(_launchPoint.x, 0f), Vector3.zero, _turret.transform.eulerAngles);

                Handles.color = new Color(0, 0, 1, 0.85f);
                Handles.DrawLine(_turret.transform.position + _sideOffset, _position, 6f * (1 + _turret.GetTurretType().size));

                float _arc = _turret.Spread;
                Color _fill = new Color(1, 1, 0, 0.07f / _turret.GetTurretType().launchPoints.Length);
                Color _edge = new Color(1, 1, 0, 0.5f);
                Handles.color = _fill;
                Handles.DrawSolidArc(_position, Vector3.forward, _turret.transform.up, _arc, _turret.GetTurretType().range);
                Handles.DrawSolidArc(_position, Vector3.forward, _turret.transform.up, -_arc, _turret.GetTurretType().range);

                Handles.color = _edge;
                Handles.DrawWireArc(_position, Vector3.forward, _turret.transform.up, _arc, _turret.GetTurretType().range, 2f * (1 + _turret.GetTurretType().size));
                Handles.DrawWireArc(_position, Vector3.forward, _turret.transform.up, -_arc, _turret.GetTurretType().range, 2f * (1 + _turret.GetTurretType().size));
            }
        }

        if (_turret.GetTurretVisual() != null)
        {
            Vector3 _pivotOffset = RotatePointAroundPivot(_turret.GetTurretType().pivot + (Vector2)_turret.GetTurretVisual().localPosition, Vector3.zero, _turret.transform.eulerAngles);
            Handles.color = new Color(1, 0, 0, 0.1f);
            Handles.DrawSolidDisc(_turret.transform.position + _pivotOffset, Vector3.forward, 0.1f * (1 + _turret.GetTurretType().size));
            Handles.color = new Color(1, 0, 0, 0.75f);
            Handles.DrawSolidDisc(_turret.transform.position + _pivotOffset, Vector3.forward, 0.02f * (1 + _turret.GetTurretType().size));
            Handles.color = new Color(1, 0, 0, 0.75f);
            Handles.DrawWireDisc(_turret.transform.position + _pivotOffset, Vector3.forward, 0.1f * (1 + _turret.GetTurretType().size));
        }

        if (_turret.hardpoint != null)
        {
            Vector3 hardpointPos = _turret.hardpoint.Position + (Vector2)_turret.transform.parent.position;

            float size = 0.2f * (1 + _turret.GetTurretType().size);

            Handles.color = new Color(0f, 1f, 0f, 0.15f);
            Handles.DrawSolidDisc(hardpointPos, Vector3.forward, size);

            Handles.color = new Color(0f, 1f, 0f, 0.85f);
            Handles.DrawWireDisc(hardpointPos, Vector3.forward, size);
        }
    }

    public void SaveIntoJson(object _object, string _path)
    {
        string json = JsonUtility.ToJson(_object, true);
        File.WriteAllText(Application.dataPath + "/" + _path, json);
        Debug.Log("exporting: " + Application.dataPath + "/" + _path);
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
}
