#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using VRC.SDKBase;

using VRSuya.Core;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

	[AddComponentMenu("VRSuya/VRSuya BlendshapeViewer")]
	[HelpURL("https://vrsuya.booth.pm/")]
	public class BlendshapeController : MonoBehaviour, IEditorOnly {

		public SkinnedMeshRenderer TargetSkinnedMeshRenderer = null;
		public Animator TargetAnimator = null;

		List<string> TargetBlendShapeNames = new List<string>();
		public Dictionary<string, int> BlendShapeList = new Dictionary<string, int>();

		void Reset() {
			SetVariable();
			UpdateBlendshapeList();
		}

		void SetVariable() {
			GameObject AvatarGameObject = AvatarUtility.GetAvatarGameObject(this.gameObject);
			if (AvatarGameObject) {
				GameObject HeadGameObject = AvatarUtility.GetHeadGameObject(AvatarGameObject);
				if (HeadGameObject) TargetSkinnedMeshRenderer = HeadGameObject.GetComponent<SkinnedMeshRenderer>();
				TargetAnimator = AvatarGameObject.GetComponent<Animator>();
			}
		}

		public void UpdateBlendshapeList() {
			if (TargetSkinnedMeshRenderer && TargetAnimator) {
				TargetBlendShapeNames = GetAnimationBlendshapeName();
				if (TargetBlendShapeNames.Count > 0) {
					BlendShapeList = CreateBlendshapeList();
				}
			}
		}

		Dictionary<string, int> CreateBlendshapeList() {
			Dictionary<string, int> NewBlendShapeList = new Dictionary<string, int>();
			Mesh TargetMesh = TargetSkinnedMeshRenderer.sharedMesh;
			int BlendShapeCount = TargetMesh.blendShapeCount;
			for (int Index = 0; Index < BlendShapeCount; Index++) {
				if (TargetBlendShapeNames.Exists(Item => TargetMesh.GetBlendShapeName(Index) == Item)) {
					NewBlendShapeList.Add(TargetMesh.GetBlendShapeName(Index), Index);
				}
			}
			return NewBlendShapeList;
		}

		List<string> GetAnimationBlendshapeName() {
			if (!TargetAnimator || !TargetAnimator.runtimeAnimatorController) {
				return new List<string>();
			}
			List<string> NewBlendshapeNameList = AnimatorHelper.GetAllAnimationClips(TargetAnimator.runtimeAnimatorController as AnimatorController)
				.Where(Item => Item != null)
				.SelectMany(Item => AnimationUtility.GetCurveBindings(Item))
				.Where(Item => Item.type == typeof(SkinnedMeshRenderer) && AvatarUtility.HeadGameObjectNames.Contains(Item.path))
				.Select(Item => Item.propertyName.Substring(11))
				.Distinct()
				.OrderBy(Item => Item, StringComparer.Ordinal)
				.ToList();
			return NewBlendshapeNameList;
		}
	}
}
#endif