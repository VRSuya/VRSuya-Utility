using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using VRC.SDKBase;

using VRSuya.Core;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

	public class PortraitCamera : Editor {

		enum ColorType {
			Levin, Macchiato
		};

		static Dictionary<ColorType, string> ColorList = new Dictionary<ColorType, string>() {
			{ ColorType.Levin, "#26BFBB" },
			{ ColorType.Macchiato, "#BF0000" }
		};

		[MenuItem("Tools/VRSuya/Utility/PortraitCamera/Macchiato", priority = 1000)]
		static void AddMacchaitoCamera() {
			AddNewCamera(ColorList[ColorType.Macchiato]);
		}

		[MenuItem("Tools/VRSuya/Utility/PortraitCamera/Levin", priority = 1000)]
		static void AddLevinCamera() {
			AddNewCamera(ColorList[ColorType.Levin]);
		}

		static Camera AddNewCamera(string HEXColorCode) {
			VRC_AvatarDescriptor TargetAvatarDescriptor = AvatarUtility.GetAvatarDescriptor();
			if (TargetAvatarDescriptor) {
				GameObject NewCameraGameObject = new GameObject("PortraitCamera");
				Camera NewCameraComponent = NewCameraGameObject.AddComponent<Camera>();
				NewCameraComponent.clearFlags = CameraClearFlags.SolidColor;
				NewCameraComponent.backgroundColor = UnityUtility.HexToColor(HEXColorCode);
				NewCameraComponent.fieldOfView = 1.0f;
				NewCameraComponent.nearClipPlane = 0.01f;
				NewCameraComponent.renderingPath = RenderingPath.Forward;
				NewCameraGameObject.transform.position = GetCameraPosition(TargetAvatarDescriptor);
				NewCameraGameObject.transform.rotation = GetCameraRotation(TargetAvatarDescriptor);
				Undo.RegisterCreatedObjectUndo(NewCameraGameObject, "VRSuya PortraitCamera");
				EditorUtility.SetDirty(NewCameraComponent);
				SceneView.RepaintAll();
				Selection.objects = new Object[] { NewCameraGameObject };
				Selection.activeGameObject = NewCameraGameObject;
				return NewCameraComponent;
			} else {
				return null;
			}
		}

		static Vector3 GetCameraPosition(VRC_AvatarDescriptor AvatarDescriptor) {
			Vector3 NewCameraPosition = new Vector3(0.0f, 1.2f, 13.5f);
			Vector3 Offset = new Vector3(0.0f, -0.02f, 14.0f);
			Transform AvatarTransform = AvatarDescriptor.gameObject.transform;
			Vector3 AvatarViewPosition = AvatarTransform.position + (AvatarTransform.rotation * AvatarDescriptor.ViewPosition);
			NewCameraPosition = AvatarViewPosition + (AvatarTransform.rotation * Offset);
			return NewCameraPosition;
		}

		static Quaternion GetCameraRotation(VRC_AvatarDescriptor AvatarDescriptor) {
			Quaternion AvatarRotation = AvatarDescriptor.gameObject.transform.rotation;
			Quaternion ReferenceRotation = Quaternion.Euler(0, 180, 0);
			Quaternion RotationDifference = AvatarRotation * Quaternion.Inverse(ReferenceRotation);
			return RotationDifference;
		}
	}
}