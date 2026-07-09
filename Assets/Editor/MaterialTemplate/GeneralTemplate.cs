using UnityEditor;
using UnityEngine;

/*
 * VRSuya Utility
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Utility {

	internal class GeneralTemplate : MaterialTemplate {

		internal bool UpdateGeneralPropertys(Material TargetMaterial) {
			bool IsModified = false;
			if (UpdateRenderQueue) {
				if (UpdateRenderQueuePropertys(TargetMaterial)) IsModified = true;
			}
			if (UpdateGPUInstancing) {
				if (UpdateGPUInstancingPropertys(TargetMaterial)) IsModified = true;
			}
			if (UpdateGlobalIllumination) {
				if (UpdateGlobalIlluminationPropertys(TargetMaterial)) IsModified = true;
			}
			if (IsModified) {
				EditorUtility.SetDirty(TargetMaterial);
				return true;
			}
			return false;
		}

		bool UpdateRenderQueuePropertys(Material TargetMaterial) {
			bool IsDrity = false;
			bool IsTransparent = TargetMaterial.shader.name.Contains("Transparent");
			int RenderQueue = (!IsTransparent) ? -1 : 3000;
			if (TargetMaterial.renderQueue != RenderQueue) { TargetMaterial.renderQueue = RenderQueue; IsDrity = true; }
			return IsDrity;
		}

		bool UpdateGPUInstancingPropertys(Material TargetMaterial) {
			bool IsDrity = false;
			bool EnableInstancingVariants = true;
			if (TargetMaterial.enableInstancing != EnableInstancingVariants) { TargetMaterial.enableInstancing = EnableInstancingVariants; IsDrity = true; }
			return IsDrity;
		}

		bool UpdateGlobalIlluminationPropertys(Material TargetMaterial) {
			bool IsDrity = false;
			MaterialGlobalIlluminationFlags GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
			bool DoubleSidedGI = true;
			if (TargetMaterial.globalIlluminationFlags != GlobalIlluminationFlags) { TargetMaterial.globalIlluminationFlags = GlobalIlluminationFlags; IsDrity = true; }
			if (TargetMaterial.doubleSidedGI != DoubleSidedGI) { TargetMaterial.doubleSidedGI = DoubleSidedGI; IsDrity = true; }
			return IsDrity;
		}
	}
}
