// 랩 씬 생성기 도우미: private [SerializeField] 필드를 채우는 확장 메서드와 기본 도형 생성 (Sandbox 폴더 안에서만 사용).
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.Sandbox.EditorTools
{
    internal static class LabEditorExtensions
    {
        public static T SetRef<T>(this T target, string field, Object value) where T : Object
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T SetRefList<T>(this T target, string field, IList<Object> values) where T : Object
        {
            var so = new SerializedObject(target);
            var list = so.FindProperty(field);
            list.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
    }
}
