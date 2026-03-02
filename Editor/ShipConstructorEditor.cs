using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ShipConstructor))]
public class ShipConstructorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ShipConstructor _ship = (ShipConstructor)target;


        if (GUILayout.Button("Import Ship"))
        {
            var _path = EditorInputDialog.Show("Import Ship", "Enter file path:", "Data/Base/Core/Ships/" + _ship.GetShipType().id + ".json");

            var _file = File.OpenText(Application.dataPath + "/" + _path);
            if (_file != null && _path != "Data/Base/Core/Defs/Ships/")
            {
                var json = _file.ReadToEnd();
                ShipData _shipData = new();
                JsonUtility.FromJsonOverwrite(json, _shipData);
                _ship.ChangeShipType(_shipData);
                _file.Close();
            }
            else
            {
                Debug.Log("File " + _path + " not found!");
            }
        }

        if (GUILayout.Button("Export Ship"))
        {
            var _path = EditorInputDialog.Show("Export Ship", "Enter file path:", "Data/Base/Core/Ships/" + _ship.GetShipType().id + ".json");

            if (_path != "Data/Base/Core/Defs/Ships/")
            {
                SaveIntoJson(_ship.GetShipType(), _path);
            }
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Initialize Ship"))
        {
            _ship.InitializeShip();
        }

        if (GUILayout.Button("Mirror Collider Points"))
        {
            _ship.MirrorColliderPoints();
            _ship.InitializeShip();
        }

        if (GUILayout.Button("Load Collider Points From Collider"))
        {
            _ship.LoadColliderPoints();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Set Faction"))
        {
            var _id = EditorInputDialog.Show("Set Faction", "Enter faction id:", _ship.ShipFactionID);
            _ship.ChangeFaction(_id);
        }
    }

    public void OnSceneGUI()
    {
        ShipConstructor _ship = target as ShipConstructor;
        if (_ship.TurretHardpoints == null || _ship.TurretHardpoints.Length < 1)
        {
            return;
        }

        foreach (var _turretHardpoint in _ship.GetShipType().turretHardpoints)
        {
            Vector3 _offset = RotatePointAroundPivot(_turretHardpoint.Position, Vector3.zero, _ship.transform.eulerAngles);
            Vector3 _position = _offset + _ship.transform.position;

            float _angle = (_ship.transform.eulerAngles.z + _turretHardpoint.Angle + 90) * Mathf.Deg2Rad;
            float _arc = 180f;
            Color _fill = new Color(0, 1, 0, 0.07f);
            Color _edge = new Color(0, 1, 0, 0.4f);
            if (_turretHardpoint.Arc >= 0f)
            {
                _arc = _turretHardpoint.Arc / 2;
                _fill = new Color(1, 1, 0, 0.07f);
                _edge = new Color(1, 1, 0, 0.4f);
            }
            Vector3 _facingVector = new(Mathf.Cos(_angle), Mathf.Sin(_angle));

            Handles.color = _fill;
            Handles.DrawSolidArc(_position, Vector3.forward, _facingVector, _arc, 0.2f * (1 + _turretHardpoint.Size));
            Handles.DrawSolidArc(_position, Vector3.forward, _facingVector, -_arc, 0.2f * (1 + _turretHardpoint.Size));

            Handles.color = _edge;
            Handles.DrawWireArc(_position, Vector3.forward, _facingVector, _arc, 0.2f * (1 + _turretHardpoint.Size), 2f);
            Handles.DrawWireArc(_position, Vector3.forward, _facingVector, -_arc, 0.2f * (1 + _turretHardpoint.Size), 2f);

            Handles.color = new Color(1, 0, 0, 0.15f);
            Handles.DrawSolidDisc(_position, Vector3.forward, 0.1f * (1 + _turretHardpoint.Size));
            Handles.color = new Color(1, 0, 0, 0.75f);
            Handles.DrawSolidDisc(_position, Vector3.forward, 0.03f * (1 + _turretHardpoint.Size));
            Handles.color = new Color(1, 0, 0, 1f);
            Handles.DrawWireDisc(_position, Vector3.forward, 0.1f * (1 + _turretHardpoint.Size), 3f);

            var _style = new GUIStyle();
            _style.alignment = TextAnchor.UpperLeft;
            _style.normal.textColor = new Color(1, 1, 0, 1f);
            _style.fontSize = 15;

            Handles.Label(_position, _turretHardpoint.Id, _style);
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
