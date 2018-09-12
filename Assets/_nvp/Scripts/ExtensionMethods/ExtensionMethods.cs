using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExtensionMethods{
	public static class ExtensionMethods{
		public static SerializableVector3 MakeSerializable(this Vector3 v){
			return new SerializableVector3(v.x, v.y, v.z);
		}

		public static Vector3 ToVector3(this SerializableVector3 sv){
			return new Vector3(sv.x, sv.y, sv.z);
		}
	}
}