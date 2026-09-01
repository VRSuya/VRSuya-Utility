using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using static VRSuya.Core.Translator;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

    [CustomEditor(typeof(BlendshapeController))]
    public class BlendshapeControllerEditor : Editor {

		SerializedProperty SerializedTargetSkinnedMeshRenderer;
		SerializedProperty SerializedTargetAnimator;

		List<string> ExceedLimitBlendshape = new List<string>();

		const string UndoGroupName = "VRSuya BlendshapeController";

		void OnEnable() {
			SerializedTargetSkinnedMeshRenderer = serializedObject.FindProperty("TargetSkinnedMeshRenderer");
			SerializedTargetAnimator = serializedObject.FindProperty("TargetAnimator");
		}

        public override void OnInspectorGUI() {
			serializedObject.Update();
			BlendshapeController TargetInstance = (BlendshapeController)target;
			EditorGUILayout.PropertyField(SerializedTargetSkinnedMeshRenderer, new GUIContent(GetTranslatedString("String_SkinnedMeshRenderer")));
			EditorGUILayout.PropertyField(SerializedTargetAnimator, new GUIContent(GetTranslatedString("String_Animator")));
			if (TargetInstance.BlendShapeList.Count > 0) {
				EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
				for (int Index = 0; Index < TargetInstance.BlendShapeList.Count; Index++) {
					string BlendShapeName = TargetInstance.BlendShapeList.Keys.ElementAt(Index);
					float CurrentValue = TargetInstance.TargetSkinnedMeshRenderer.GetBlendShapeWeight(TargetInstance.BlendShapeList.Values.ElementAt(Index));
					if (CurrentValue < 0.0f || CurrentValue > 100.0f) {
						if (!ExceedLimitBlendshape.Exists(Item => Item == BlendShapeName)) {
							ExceedLimitBlendshape.Add(BlendShapeName);
						}
					}
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(BlendShapeName);
					EditorGUI.BeginChangeCheck();
					float NewValue = EditorGUILayout.Slider(CurrentValue, 0, 100);
					EditorGUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck()) {
						Undo.RecordObject(TargetInstance.TargetSkinnedMeshRenderer, UndoGroupName);
						TargetInstance.TargetSkinnedMeshRenderer.SetBlendShapeWeight(TargetInstance.BlendShapeList.Values.ElementAt(Index), NewValue);
						EditorUtility.SetDirty(TargetInstance.TargetSkinnedMeshRenderer);
					}
				}
			}
			if (ExceedLimitBlendshape.Count > 0) {
				EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
				EditorGUILayout.LabelField(GetTranslatedString("String_OutofRangeBlendshape"));
				EditorGUI.indentLevel++;
				foreach (string ExceedBlendshape in ExceedLimitBlendshape) {
					EditorGUILayout.LabelField("▶ " + ExceedBlendshape);
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
			serializedObject.ApplyModifiedProperties();
			if (GUILayout.Button(GetTranslatedString("String_Update"))) {
				(target as BlendshapeController).UpdateBlendshapeList();
				ExceedLimitBlendshape = new List<string>();
			}
		}
    }
}