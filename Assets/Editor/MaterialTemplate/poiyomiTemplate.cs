using UnityEditor;
using UnityEngine;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

	internal class poiyomiTemplate : MaterialTemplate {

		internal bool UpdatepoiyomiPropertys(Material TargetMaterial) {
			bool IsModified = false;
			if (IsModified) {
				EditorUtility.SetDirty(TargetMaterial);
				return true;
			}
			return false;
		}
	}
}
