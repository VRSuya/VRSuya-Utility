#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using VRC.SDKBase;

using VRSuya.Core;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

	[ExecuteInEditMode]
	[AddComponentMenu("VRSuya/VRSuya TextureReplacer")]
	[HelpURL("https://vrsuya.booth.pm/")]
	public class TextureReplacer : MonoBehaviour, IEditorOnly {

		[Serializable]
		public struct TextureExpression {
			public bool ShowDetails;
			public Texture2D BeforeTexture;
			public Texture2D AfterTexture;
			public MaterialDetail[] OriginMaterial;

			public TextureExpression(bool ShowDetail, Texture2D ExistTexture, Texture2D NewTexture, MaterialDetail[] ExistMaterials) {
				ShowDetails = ShowDetail;
				BeforeTexture = ExistTexture;
				AfterTexture = NewTexture;
				OriginMaterial = ExistMaterials;
			}
		};

		[Serializable]
		public struct MaterialDetail {
			public Material OriginMaterial;
			public string[] PropertyName;

			public MaterialDetail(Material ExistMaterial, string[] ExsitPropertyName) {
				OriginMaterial = ExistMaterial;
				PropertyName = ExsitPropertyName;
			}
		}

		[SerializeField]
		public List<TextureExpression> AvatarTextures = new List<TextureExpression>();
		List<TextureExpression> TargetTextures = new List<TextureExpression>();

		public GameObject AvatarGameObject = null;
		public Material[] AvatarMaterials = new Material[0];

		const string UndoGroupName = "VRSuya TextureReplacer";
		int UndoGroupIndex;

		void Reset() {
			RequestGetAvatarMaterials();
		}

		public void RequestUpdateAvatarMaterials() {
			UndoGroupIndex = UnityUtility.InitializeUndoGroup(UndoGroupName);
			TargetTextures = AvatarTextures.Where(Item => Item.BeforeTexture != Item.AfterTexture).ToList();
			if (AvatarMaterials.Length > 0 && TargetTextures.Count > 0) ChangeTexture2Ds();
		}

		public void RequestGetAvatarMaterials() {
			AvatarGameObject = AvatarUtility.GetAvatarGameObject(this.gameObject);
			AvatarMaterials = AvatarUtility.GetAvatarMaterials(AvatarGameObject);
			AvatarTextures = GetAvatarTextures(AvatarGameObject);
		}

		List<TextureExpression> GetAvatarTextures(GameObject TargetGameObject) {
			TextureExpression[] AvatarTextureExpressions = AddAvatarTextureDetails(TargetGameObject);
			List<TextureExpression> NewAvatarTextureExpressions = new List<TextureExpression>();
			Texture2D[] ExistTexture = AvatarTextureExpressions.Select(Item => Item.BeforeTexture).Distinct().ToArray();
			for (int TextureIndex = 0; TextureIndex < ExistTexture.Length; TextureIndex++) {
				MaterialDetail[] TextureMaterials = AvatarTextureExpressions
					.Where(Item => Item.BeforeTexture == ExistTexture[TextureIndex])
					.SelectMany(Item => Item.OriginMaterial).ToArray();
				for (int MaterialIndex = 0; MaterialIndex < TextureMaterials.Length; MaterialIndex++) {
					if (NewAvatarTextureExpressions.Exists(Item => Item.BeforeTexture == ExistTexture[TextureIndex])) {
						TextureExpression OldAvatarTextureExpression = NewAvatarTextureExpressions.Find(Item => Item.BeforeTexture == ExistTexture[TextureIndex]);
						List<MaterialDetail> NewMaterialDetail = OldAvatarTextureExpression.OriginMaterial.Concat(new MaterialDetail[] { TextureMaterials[MaterialIndex] }).ToList();
						NewMaterialDetail.Sort((a, b) => string.Compare(a.OriginMaterial.name, b.OriginMaterial.name, StringComparison.Ordinal));
						TextureExpression NewAvatarTextureExpression = new TextureExpression() {
							ShowDetails = OldAvatarTextureExpression.ShowDetails,
							BeforeTexture = OldAvatarTextureExpression.BeforeTexture,
							AfterTexture = OldAvatarTextureExpression.AfterTexture,
							OriginMaterial = NewMaterialDetail.ToArray()
						};
						NewAvatarTextureExpressions.Remove(OldAvatarTextureExpression);
						NewAvatarTextureExpressions.Add(NewAvatarTextureExpression);
					} else {
						NewAvatarTextureExpressions.Add(new TextureExpression(false, ExistTexture[TextureIndex], ExistTexture[TextureIndex], new MaterialDetail[] { TextureMaterials[MaterialIndex] }));
					}
				}
			}
			NewAvatarTextureExpressions.Sort((a, b) => string.Compare(a.BeforeTexture.name, b.BeforeTexture.name, StringComparison.Ordinal));
			return NewAvatarTextureExpressions;
		}

		void ChangeTexture2Ds() {
			int ModifiedCount = 0;
			Texture2D[] TargetTexture2Ds = TargetTextures.Select(TargetTexture => TargetTexture.BeforeTexture).ToArray();
			foreach (Material TargetMaterial in AvatarMaterials) {
				if (TargetMaterial) {
					Shader TargetShader = TargetMaterial.shader;
					int PropertyCount = ShaderUtil.GetPropertyCount(TargetShader);
					for (int Index = 0; Index < PropertyCount; Index++) {
						if (ShaderUtil.GetPropertyType(TargetShader, Index) == ShaderUtil.ShaderPropertyType.TexEnv) {
							string PropertyName = ShaderUtil.GetPropertyName(TargetShader, Index);
							Texture ExistMaterialTexture = TargetMaterial.GetTexture(PropertyName);
							if (ExistMaterialTexture is Texture2D) {
								if (Array.Exists(TargetTexture2Ds, TargetTexture => ExistMaterialTexture == TargetTexture)) {
									Undo.RecordObject(TargetMaterial, UndoGroupName);
									Texture2D newTexture2D = TargetTextures
										.Where(TargetTextureExpression => ExistMaterialTexture == TargetTextureExpression.BeforeTexture)
										.Select(TargetTextureExpression => TargetTextureExpression.AfterTexture).ToArray()[0];
									TargetMaterial.SetTexture(PropertyName, newTexture2D);
									EditorUtility.SetDirty(TargetMaterial);
									Undo.CollapseUndoOperations(UndoGroupIndex);
									ModifiedCount++;
								}
							}
						}
					}
				}
			}
			Debug.Log($"[VRSuya] {ModifiedCount} textures have been replaced");
		}

		TextureExpression[] AddAvatarTextureDetails(GameObject TargetGameObject) {
			List<TextureExpression> NewTextureExpressions = new List<TextureExpression>();
			Material[] AvatarMaterials = AvatarUtility.GetAvatarMaterials(TargetGameObject);
			if (AvatarMaterials.Length > 0) {
				NewTextureExpressions = AvatarMaterials
					.SelectMany(Item => GetMaterialTextureDetails(Item))
					.Distinct()
					.OrderBy(Item => Item.BeforeTexture.name)
					.ToList();
			}
			return NewTextureExpressions.ToArray();
		}

		TextureExpression[] GetMaterialTextureDetails(Material TargetMaterial) {
			TextureExpression[] MaterialTextureExpressions = new TextureExpression[0];
			if (TargetMaterial) {
				Shader TargetShader = TargetMaterial.shader;
				int PropertyCount = ShaderUtil.GetPropertyCount(TargetShader);
				for (int Index = 0; Index < PropertyCount; Index++) {
					if (ShaderUtil.GetPropertyType(TargetShader, Index) == ShaderUtil.ShaderPropertyType.TexEnv) {
						string PropertyName = ShaderUtil.GetPropertyName(TargetShader, Index);
						Texture MaterialTexture = TargetMaterial.GetTexture(PropertyName);
						if (MaterialTexture is Texture2D) {
							MaterialDetail newMaterialDetail = new MaterialDetail() {
								OriginMaterial = TargetMaterial,
								PropertyName = new string[] { PropertyName }
							};
							TextureExpression newTextureExpression = new TextureExpression() {
								ShowDetails = false,
								BeforeTexture = (Texture2D)MaterialTexture,
								AfterTexture = (Texture2D)MaterialTexture,
								OriginMaterial = new MaterialDetail[] { newMaterialDetail }
							};
							MaterialTextureExpressions = MaterialTextureExpressions.Concat(new TextureExpression[] { newTextureExpression }).ToArray();
						}
					}
				}
			}
			return MaterialTextureExpressions;
		}
	}
}
#endif