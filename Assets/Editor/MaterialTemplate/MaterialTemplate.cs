using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using VRSuya.Core;
using static VRSuya.Core.Translator;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

	public class MaterialTemplate : EditorWindow {

		public GameObject AvatarGameObject;
		public Material ReferenceMaterial;
		public Material[] TargetMaterials;

		public Color TargetShadow1Color = Color.white;
		public Color TargetShadow2Color = Color.white;
		public Color TargetShadow3Color = Color.white;
		public Color TargetShadowBorderColor = Color.white;
		public Color TargetRimShadeColor = Color.white;
		public Color TargetBacklightColor = Color.white;
		public Color TargetReflectionColor = Color.white;
		public Color TargetRimLightColor = Color.white;
		public Color TargetOutlineColor = Color.white;
		public Color TargetOutlineHighlightColor = Color.white;

		public bool UpdatelilToonBasic = true;
		public bool UpdatelilToonLighting = true;
		public bool UpdatelilToonShadow = true;
		public bool UpdatelilToonReceiveShadow = true;
		public bool UpdatelilToonBackfaceMask = true;
		public bool UpdatelilToonBacklight = true;
		public bool ForcelilToonShadow = false;
		public bool ForcelilToonRimShade = false;
		public bool ForcelilToonBacklight = false;
		public bool ForcelilToonReflection = false;
		public bool ForcelilToonRimLight = false;
		public bool UpdatelilToonShadowColor = false;
		public bool UpdatelilToonRimShadeColor = false;
		public bool UpdatelilToonBacklightColor = false;
		public bool UpdatelilToonReflectionColor = false;
		public bool UpdatelilToonRimLightColor = false;
		public bool UpdatelilToonOutlineColor = false;

		public bool UpdateUTSTextureShared = true;
		public bool UpdateUTSNormalMap = true;
		public bool UpdateUTSBasicShading = true;
		public bool UpdateUTSLightColor = true;
		public bool UpdateUTSEnvironmentalLightingPropertys = true;

		public bool UpdateRenderQueue = true;
		public bool UpdateGPUInstancing = true;
		public bool UpdateGlobalIllumination = true;

		const string UndoGroupName = "VRSuya MaterialTemplate";
		int UndoGroupIndex;

		enum ShaderType {
			Unknown,
			lilToon,
			poiyomi,
			UnityChanToonShader
		}

		SerializedObject SerializedMaterialTemplate;
		SerializedProperty SerializedTargetMaterials;

		bool FoldlilToon;
		bool Foldpoiyomi;
		bool FoldUnityChanToonShader;
		bool FoldGeneral;

		Vector2 ScrollPosition;
		const float BorderX = 30f;

		void OnEnable() {
			SerializedMaterialTemplate = new SerializedObject(this);
			SerializedTargetMaterials = SerializedMaterialTemplate.FindProperty("TargetMaterials");
		}

		[MenuItem("Tools/VRSuya/Utility/MaterialTemplate", priority = 1000)]
		static void CreateWindow() {
			MaterialTemplate AppWindow = GetWindowWithRect<MaterialTemplate>(new Rect(0, 0, 450, 665), true, "VRSuya MaterialTemplate");
			AppWindow.Initialize();
		}

		void Initialize() {
			AvatarGameObject = AvatarUtility.GetAvatarGameObject();
			TargetMaterials = new Material[0];
			FoldlilToon = true;
			Foldpoiyomi = true;
			FoldUnityChanToonShader = true;
			FoldGeneral = true;
		}

		void OnGUI() {
			if (SerializedMaterialTemplate == null || !SerializedMaterialTemplate.targetObject) {
				Initialize();
				if (SerializedMaterialTemplate == null) {
					Close();
					return;
				}
			}
			SerializedMaterialTemplate.Update();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			DrawHeaderSection();
			EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(400f));
			DrawlilToonSection();
			DrawGeneralSection();
			EditorGUILayout.EndScrollView();
			SerializedMaterialTemplate.ApplyModifiedProperties();
			EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			GUI.backgroundColor = Color.cyan;
			GUI.enabled = IsReadyToUpdate();
			if (GUILayout.Button(GetTranslatedString("String_Update"), GUILayout.Height(40f))) {
				UpdateMaterialPropertys();
				Repaint();
			}
			GUI.enabled = true;
			GUI.backgroundColor = Color.white;
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
		}

		void DrawHeaderSection() {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			EditorGUIUtility.labelWidth = 100f;
			LanguageIndex = EditorGUILayout.Popup(GetTranslatedString("String_Language"), LanguageIndex, LanguageOption);
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			AvatarGameObject = (GameObject)EditorGUILayout.ObjectField(GetTranslatedString("String_Avatar"), AvatarGameObject, typeof(GameObject), true);
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			Material NewReferenceMaterial = (Material)EditorGUILayout.ObjectField(GetTranslatedString("String_ReferenceMaterial"), ReferenceMaterial, typeof(Material), true);
			if (NewReferenceMaterial != ReferenceMaterial) {
				ReferenceMaterial = NewReferenceMaterial;
				UpdateMaterialColors();
			}
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			EditorGUILayout.PropertyField(SerializedTargetMaterials, new GUIContent("머테리얼"));
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			if (GUILayout.Button(GetTranslatedString("String_GetAvatarMaterials"))) {
				AddAvatarMaterials();
			}
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
		}

		void DrawlilToonSection() {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			FoldlilToon = EditorGUILayout.Foldout(FoldlilToon, "lilToon");
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			if (FoldlilToon) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(BorderX);
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					UpdatelilToonBasic = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonBasic"), UpdatelilToonBasic);
					UpdatelilToonLighting = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonLighting"), UpdatelilToonLighting);
					UpdatelilToonShadow = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonShadow"), UpdatelilToonShadow);
					UpdatelilToonReceiveShadow = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonReceiveShadow"), UpdatelilToonReceiveShadow);
					UpdatelilToonBackfaceMask = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonBackfaceMask"), UpdatelilToonBackfaceMask);
					UpdatelilToonBacklight = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonBacklight"), UpdatelilToonBacklight);
				}
				GUILayout.Space(BorderX);
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(BorderX);
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					ForcelilToonShadow = EditorGUILayout.ToggleLeft(GetTranslatedString("String_ForcelilToonShadow"), ForcelilToonShadow);
					ForcelilToonRimShade = EditorGUILayout.ToggleLeft(GetTranslatedString("String_ForcelilToonRimShade"), ForcelilToonRimShade);
					ForcelilToonBacklight = EditorGUILayout.ToggleLeft(GetTranslatedString("String_ForcelilToonBacklight"), ForcelilToonBacklight);
					ForcelilToonReflection = EditorGUILayout.ToggleLeft(GetTranslatedString("String_ForcelilToonReflection"), ForcelilToonReflection);
					ForcelilToonRimLight = EditorGUILayout.ToggleLeft(GetTranslatedString("String_ForcelilToonRimLight"), ForcelilToonRimLight);
				}
				GUILayout.Space(BorderX);
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(BorderX);
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					UpdatelilToonShadowColor = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonShadowColor"), UpdatelilToonShadowColor);
					TargetShadow1Color = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetShadow1Color")), TargetShadow1Color, showEyedropper: true, showAlpha: false, hdr: false);
					TargetShadow2Color = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetShadow2Color")), TargetShadow2Color, showEyedropper: true, showAlpha: true, hdr: false);
					TargetShadow3Color = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetShadow3Color")), TargetShadow3Color, showEyedropper: true, showAlpha: true, hdr: false);
					TargetShadowBorderColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetShadowBorderColor")), TargetShadowBorderColor, showEyedropper: true, showAlpha: true, hdr: false);
					UpdatelilToonRimShadeColor = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonRimShadeColor"), UpdatelilToonRimShadeColor);
					TargetRimShadeColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetRimShadeColor")), TargetRimShadeColor, showEyedropper: true, showAlpha: true, hdr: false);
					UpdatelilToonBacklightColor = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonBacklightColor"), UpdatelilToonBacklightColor);
					TargetBacklightColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetBacklightColor")), TargetBacklightColor, showEyedropper: true, showAlpha: true, hdr: true);
					UpdatelilToonReflectionColor = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonReflectionColor"), UpdatelilToonReflectionColor);
					TargetReflectionColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetReflectionColor")), TargetReflectionColor, showEyedropper: true, showAlpha: true, hdr: true);
					UpdatelilToonRimLightColor = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonRimLightColor"), UpdatelilToonRimLightColor);
					TargetRimLightColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetRimLightColor")), TargetRimLightColor, showEyedropper: true, showAlpha: true, hdr: true);
					UpdatelilToonOutlineColor = EditorGUILayout.ToggleLeft(GetTranslatedString("String_UpdatelilToonOutlineColor"), UpdatelilToonOutlineColor);
					TargetOutlineColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetOutlineColor")), TargetOutlineColor, showEyedropper: true, showAlpha: true, hdr: true);
					TargetOutlineHighlightColor = EditorGUILayout.ColorField(new GUIContent(GetTranslatedString("String_TargetOutlineHighlightColor")), TargetOutlineHighlightColor, showEyedropper: true, showAlpha: true, hdr: true);
				}
				GUILayout.Space(BorderX);
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
		}

		void DrawGeneralSection() {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(BorderX);
			FoldGeneral = EditorGUILayout.Foldout(FoldGeneral, GetTranslatedString("String_General"));
			GUILayout.Space(BorderX);
			EditorGUILayout.EndHorizontal();
			if (FoldGeneral) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(BorderX);
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					UpdateRenderQueue = EditorGUILayout.ToggleLeft(GetTranslatedString("String_RenderQueue"), UpdateRenderQueue);
					UpdateGPUInstancing = EditorGUILayout.ToggleLeft(GetTranslatedString("String_GPUInstancing"), UpdateGPUInstancing);
					UpdateGlobalIllumination = EditorGUILayout.ToggleLeft(GetTranslatedString("String_GlobalIllumination"), UpdateGlobalIllumination);
				}
				EditorGUI.indentLevel--;
				GUILayout.Space(BorderX);
				EditorGUILayout.EndHorizontal();
			}
		}

		bool IsReadyToUpdate() {
			return TargetMaterials.Length > 0;
		}

		public bool UpdateMaterialPropertys() {
			UndoGroupIndex = UnityUtility.InitializeUndoGroup(UndoGroupName);
			bool IsModified = false;
			lilToonTemplate lilToonTemplateInstance = new lilToonTemplate();
			poiyomiTemplate poiyomiTemplateInstance = new poiyomiTemplate();
			UTSTemplate UTSTemplateInstance = new UTSTemplate();
			GeneralTemplate GeneralTemplateInstance = new GeneralTemplate();
			foreach (Material TargetMaterial in TargetMaterials) {
				if (TargetMaterial) {
					Undo.RecordObject(TargetMaterial, UndoGroupName);
					switch (GetShaderType(TargetMaterial)) {
						case ShaderType.lilToon:
							if (lilToonTemplateInstance.UpdatelilToonPropertys(TargetMaterial)) {
								Undo.CollapseUndoOperations(UndoGroupIndex);
								IsModified = true;
							}
							break;
						case ShaderType.poiyomi:
							if (poiyomiTemplateInstance.UpdatepoiyomiPropertys(TargetMaterial)) {
								Undo.CollapseUndoOperations(UndoGroupIndex);
								IsModified = true;
							}
							break;
						case ShaderType.UnityChanToonShader:
							if (UTSTemplateInstance.UpdateUnityChanToonShaderPropertys(TargetMaterial)) {
								Undo.CollapseUndoOperations(UndoGroupIndex);
								IsModified = true;
							}
							break;
						default:
							Debug.LogError($"[VRSuya] {TargetMaterial.shader.name} 쉐이더는 지원하지 않습니다!");
							break;
					}
					if (GeneralTemplateInstance.UpdateGeneralPropertys(TargetMaterial)) {
						Undo.CollapseUndoOperations(UndoGroupIndex);
						IsModified = true;
					}
				}
			}
			if (IsModified) {
				AssetDatabase.SaveAssets();
				return true;
			}
			return false;
		}

		void AddAvatarMaterials() {
			Material[] AvatarMaterials = AvatarUtility.GetAvatarMaterials(AvatarGameObject);
			TargetMaterials = TargetMaterials.Concat(AvatarMaterials).Distinct().ToArray();
		}

		ShaderType GetShaderType(Material TargetMaterial) {
			string TargetShaderName = TargetMaterial.shader.name;
			if (TargetShaderName.Contains("lilToon", StringComparison.OrdinalIgnoreCase)) return ShaderType.lilToon;
			if (TargetShaderName.Contains("poiyomi", StringComparison.OrdinalIgnoreCase)) return ShaderType.poiyomi;
			if (TargetShaderName.Contains("UnityChanToonShader", StringComparison.OrdinalIgnoreCase)) return ShaderType.UnityChanToonShader;
			return ShaderType.Unknown;
		}

		Material[] GetRequestMaterials(ShaderType TargetShaderType) {
			List<Material> TargetMaterials = new List<Material>();
			string[] MaterialsGUID = AssetDatabase.FindAssets("glob:\"*.mat\"", new[] { "Assets" });
			foreach (string TargetMaterialGUID in MaterialsGUID) {
				Material TargetMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(TargetMaterialGUID));
				if (TargetMaterial) {
					if (GetShaderType(TargetMaterial) == TargetShaderType) {
						TargetMaterials.Add(TargetMaterial);
					}
				}
			}
			return TargetMaterials.Distinct().OrderBy(Item => Item.name).ToArray();
		}

		void UpdateMaterialColors() {
			if (ReferenceMaterial) {
				switch (GetShaderType(ReferenceMaterial)) {
					case ShaderType.lilToon:
						lilToonTemplate lilToonTemplateInstance = new lilToonTemplate();
						lilToonTemplateInstance.GetlilToonColors(ReferenceMaterial);
						break;
					case ShaderType.poiyomi:
						break;
					case ShaderType.UnityChanToonShader:
						break;
				}
			}
		}
	}
}
