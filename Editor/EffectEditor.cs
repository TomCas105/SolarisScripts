using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Effect))]
public class EffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Effect _effect = (Effect)target;


        if (GUILayout.Button("Import Effect"))
        {
            var _path = EditorInputDialog.Show("Import Turret", "Enter file path:", "Data/Base/Core/Effects/" + _effect.GetEffectData().id + ".json");

            var _file = File.OpenText(Application.dataPath + "/" + _path);
            if (_file != null && _path != "Data/Base/Core/Defs/Effects/")
            {
                var json = _file.ReadToEnd();
                EffectData _effectData = new();
                JsonUtility.FromJsonOverwrite(json, _effectData);
                _effect.Initialize(_effectData);
            }
            else
            {
                Debug.Log("File " + _path + " not found!");
            }
        }

        if (GUILayout.Button("Export Effect"))
        {
            var _path = EditorInputDialog.Show("Export Turret", "Enter file path:", "Data/Base/Core/Effects/" + _effect.GetEffectData().id + ".json");

            if (_path != "Data/Base/Core/Defs/Effects/")
            {
                SaveIntoJson(_effect.GetEffectData(), _path);
            }
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Initialize Effect"))
        {
            _effect.Initialize(_effect.GetEffectData());
        }
    }

    public void SaveIntoJson(object _object, string _path)
    {
        string json = JsonUtility.ToJson(_object, true);
        File.WriteAllText(Application.dataPath + "/" + _path, json);
        Debug.Log("exporting: " + Application.dataPath + "/" + _path);
    }
}
