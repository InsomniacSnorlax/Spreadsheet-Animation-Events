using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Snorlax
{
    [CustomEditor(typeof(EventCSV))]
    public class EventCSVEditor : Editor
    {
        EventCSV eventCSV;
        string from = string.Empty;
        string to;

        public List<Object> ObjectList = new List<Object>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            eventCSV = (EventCSV)target;

            EditorGUILayout.BeginHorizontal();
            //"Header: ",
            GUILayout.Label("Header: ", GUILayout.Width(60));
            from = GUILayout.TextField(from);
            GUILayout.Label("->", GUILayout.Width(20));
            
            to = GUILayout.TextField(to);

            GUILayout.Space(20);

            if (GUILayout.Button("Convert Header name", GUILayout.Width(150))) ConvertHeader();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load From CSV")) ReadCSV();

            if (GUILayout.Button("Write To CSV")) WriteCSV();

            if (GUILayout.Button("Add FBXs to Scriptable Object")) GetEvents();

            if (GUILayout.Button("Clear Object List")) ObjectList.Clear();

            EditorGUILayout.EndHorizontal();


            DropArea();

            if (GUILayout.Button("Apply CSV to FBX events")) ApplyEvents();
        }
        
        void ConvertHeader()
        {
            eventCSV.Headers[eventCSV.Headers.FindIndex(e => e == from)] = to;

            foreach(var anim in eventCSV.AnimationData)
            {
                anim.animationEventDatas.Where(e => e.MethodName == from).ToList().ForEach(e => e.MethodName = to);
            }
        }

        void ReadCSV()
        {
            eventCSV.AnimationData.Clear();

            string[] data = File.ReadAllLines(AssetDatabase.GetAssetPath(eventCSV.textAssetData));

            eventCSV.Headers = data.First().Split(",").ToList();

            for (int row = 1; row < data.Length; row++)
            {
                var AnimationData = new AnimationData();
                var columnsData = data[row].Split(",");
                AnimationData.AnimationName = columnsData.First();
                for (int column = 1; column < columnsData.Length; column++)
                {
                    var split = columnsData[column].Split("-").ToList();

                    split.ForEach(e =>
                    {
                        AnimationData.animationEventDatas.Add(new AnimationEventData(eventCSV.Headers[column], e));
                    });
                }

                eventCSV.AnimationData.Add(AnimationData);
            }
        }
        void WriteCSV()
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(AssetDatabase.GetAssetPath(eventCSV.textAssetData)))
                {
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < eventCSV.Headers.Count; i++)
                    {
                        string name = i == eventCSV.Headers.Count - 1 ? eventCSV.Headers[i] : eventCSV.Headers[i] + ",";
                        sb.Append(name);
                    }

                    sb.Append("\r\n");

                    for (int i = 0; i < eventCSV.AnimationData.Count; i++)
                    {
                        var data = eventCSV.AnimationData[i];

                        sb.Append(data.AnimationName + ",");
                        for (int index = 1; index < eventCSV.Headers.Count; index++)
                        {
                            var animEvent = data.animationEventDatas.Where(e => e.MethodName == eventCSV.Headers[index]).ToList();
                            Debug.Log(animEvent.Count);
                            string name = string.Empty;

                            for (int frame = 0; frame < animEvent.Count; frame++)
                            {

                                if (animEvent != null && animEvent[frame].Frame != -1) name += animEvent[frame].Frame + "";
                                if (animEvent.Count > 1 && frame != animEvent.Count - 1) name += "-";
                            }
                          
                            if (index != eventCSV.Headers.Count - 1) name += ",";
                            sb.Append(name);
                        }

                        sb.Append("\r\n");
                    }

                    file.Write(sb.ToString());
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        void GetEvents()
        {
            for (int i = 0; i < ObjectList.Count; i++)
            {
              
                var import = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ObjectList[i]));
                var clips = GetClips(ObjectList[i]);
                for (int x = 0; x < import.clipAnimations.Length; x++)
                {
                    var data = eventCSV.AnimationData.Find(e => import.clipAnimations[x].name == e.AnimationName);
                    var targetAnimationClip = clips.ToList().Find(e => e.name == import.clipAnimations[x].name);
                    if (targetAnimationClip == null) continue;

                    if (data == null) { data = new AnimationData(); eventCSV.AnimationData.Add(data); }
                    
                    data.animationEventDatas.Clear();
                    data.AnimationName = targetAnimationClip.name;
                    var events = targetAnimationClip.events;
                    for (int y =0; y < events.Length; y++ )
                    {
                        data.animationEventDatas.Add(new AnimationEventData(events[y].functionName, $"{Mathf.Floor(events[y].time * targetAnimationClip.frameRate)}"));
                    }
                }
            }
        }

        void ApplyEvents()
        {
            for (int i = 0; i < ObjectList.Count; i++)
            {
                var import = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ObjectList[i]));
                var data = eventCSV.AnimationData.Find(e => ObjectList[i].name == e.AnimationName);
                if (import == null || data == null) continue;

                SerializedObject so = new SerializedObject(import);
                SerializedProperty m_clips = so.FindProperty("m_ClipAnimations");
                var clips = GetClips(ObjectList[i]);

                for (int x = 0; x < import.clipAnimations.Length; x++)
                {
                    var sp = m_clips.GetArrayElementAtIndex(x);
                    var targetAnimationClip = clips.ToList().Find(e => e.name == import.clipAnimations[x].name);
                    SetEvents(sp, targetAnimationClip, data);
                }

                so.ApplyModifiedProperties();
                import.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DropArea()
        {
            Rect drop_area = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "");

            GUI.Box(drop_area, $"Drag & Drop GameObjects and or Objects here \n\r Current Objects: {ObjectList.Count}", EditorStyles.centeredGreyMiniLabel);

            var e = Event.current;

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(e.mousePosition)) return;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach(var refObj in DragAndDrop.objectReferences)
                        {
                            var import = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(refObj));
                            if (import == null || ObjectList.Contains(refObj)) continue;
                            ObjectList.Add(refObj);
                        }
                    }
                    break;
            }
        }

       
        public List<AnimationClip> GetClips(Object FBX)
        {
            List<AnimationClip> tempList = new List<AnimationClip>();

            var Items = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(FBX));

            foreach (var item in Items)
            {
                if (item is AnimationClip clip) tempList.Add(clip);
            }

            return tempList;
        }

        private void SetEvents(SerializedProperty sp, AnimationClip animationClip, AnimationData animationData)
        {
            SerializedProperty serializedProperty = sp.FindPropertyRelative("events");
            serializedProperty.ClearArray();
            if (serializedProperty != null && serializedProperty.isArray)
            {
                for (int i = 0; i < animationData.animationEventDatas.Count; i++)
                {
                    var data = animationData.animationEventDatas[i];

                    if (data.Frame == -1 || string.IsNullOrEmpty(data.MethodName)) continue;

                    serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);
                    SerializedProperty eventProperty = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
                    eventProperty.FindPropertyRelative("functionName").stringValue = data.MethodName;
                    eventProperty.FindPropertyRelative("time").floatValue = decimal.ToSingle(new decimal(data.Frame) / new decimal(animationClip.frameRate * animationClip.length));
                }
            }
        }

        
    }

    [System.Serializable]
    public class AnimationData
    {
        public string AnimationName;
        public List<AnimationEventData> animationEventDatas = new List<AnimationEventData>();
    }
    [System.Serializable]
    public class AnimationEventData
    {
        public AnimationEventData(string MethodName, string Frame)
        {
            this.MethodName = MethodName;
            if (!string.IsNullOrEmpty(Frame))
                this.Frame = int.Parse(Frame);
        }

        public string MethodName;
        public int Frame = -1;
    }
}
