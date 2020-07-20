using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Reflection;

namespace VioletUI {

	public class TidyBehaviour : MonoBehaviour {
		protected virtual void OnDestroy() {
			ReleaseReferences();
		}

		[Conditional("UNITY_EDITOR")]
		void ReleaseReferences() {
			foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				Type fieldType = field.FieldType;

				if (typeof(IList).IsAssignableFrom(fieldType)) {
					IList list = field.GetValue(this) as IList;
					list?.Clear();
				} else if (typeof(IDictionary).IsAssignableFrom(fieldType)) {
					IDictionary dictionary = field.GetValue(this) as IDictionary;
					dictionary?.Clear();
				}

				if (!fieldType.IsPrimitive) {
					field.SetValue(this, null);
				}
			}
		}
	}
}