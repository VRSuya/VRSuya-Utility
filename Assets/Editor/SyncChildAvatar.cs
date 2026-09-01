using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

using VRSuya.Core;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from curlune/VRCUtil ( https://github.com/curlune/VRCUtil )
 * Thanks to Dalgona.
 */

namespace VRSuya.Utility {

	public class SyncChildAvatar : EditorWindow {

		[MenuItem("Tools/VRSuya/Utility/Sync All Child Avatar Bone", priority = 1000)]
		static void SyncAllChildAvatar() {
			foreach (GameObject TargetAvatarGameObject in AvatarUtility.GetAvatarGameObjects()) {
                Animator ParentAvatarAnimator = TargetAvatarGameObject.GetComponent<Animator>();
				if (!ParentAvatarAnimator) continue;
                Animator[] ChildAvatarAnimator = TargetAvatarGameObject.GetComponentsInChildren<Animator>(true);
				foreach (Animator TargetAnimator in ChildAvatarAnimator) {
					CreateConstraintComponents(ParentAvatarAnimator, TargetAnimator);
				}
			}
			Debug.Log($"[VRSuya] Synced All Child Avatars");
		}

		static void CreateConstraintComponents(Animator ParentAnimator, Animator ChildAnimator) {
			if (ParentAnimator == ChildAnimator) return;
			foreach (HumanBodyBones TargetBone in UnityUtility.GetHumanBoneList()) {
				if (TargetBone == HumanBodyBones.LastBone) continue;
                Transform ParentBoneTransform = ParentAnimator.GetBoneTransform(TargetBone);
                Transform ChildBoneTransform = ChildAnimator.GetBoneTransform(TargetBone);
				if (!ParentBoneTransform || !ChildBoneTransform) continue;
                RotationConstraint TargetRotationConstraint = UnityUtility.GetOrCreateComponent<RotationConstraint>(ChildBoneTransform.gameObject);
				TargetRotationConstraint.AddSource(new ConstraintSource() { sourceTransform = ParentBoneTransform, weight = 1.0f });
				TargetRotationConstraint.constraintActive = true;
				if (TargetBone == HumanBodyBones.Hips) {
                    PositionConstraint TargetPositionConstraint = UnityUtility.GetOrCreateComponent<PositionConstraint>(ChildBoneTransform.gameObject);
					TargetPositionConstraint.AddSource(new ConstraintSource() { sourceTransform = ParentBoneTransform, weight = 1.0f });
					TargetPositionConstraint.constraintActive = true;
				}
			}
		}
	}
}