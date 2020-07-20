using UnityEngine;

/// <summary>
/// Utility extensions for manipulating Rect values.
/// </summary>
public static class RectExtMethods {
	public static Rect VioletSetPosition(this Rect rect, Vector2 position) {
		return rect.VioletSetPosition(position.x, position.y);
	}

	public static Rect VioletSetPosition(this Rect rect, float x, float y) {
		return new Rect(x, y, rect.width, rect.height);
	}

	public static Rect VioletDisplaceX(this Rect rect, float x) {
		return new Rect(rect.x + x, rect.y, rect.width, rect.height);
	}

	public static Rect VioletDisplaceY(this Rect rect, float y) {
		return new Rect(rect.x, rect.y + y, rect.width, rect.height);
	}

	public static Rect VioletDisplace(this Rect rect, float x, float y) {
		return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
	}

	public static Rect VioletDisplace(this Rect rect, Vector2 displacement) {
		return new Rect(rect.position + displacement, rect.size);
	}

	public static Rect VioletSetWidth(this Rect rect, float width) {
		return new Rect(rect.x, rect.y, width, rect.height);
	}

	public static Rect VioletSetHeight(this Rect rect, float height) {
		return new Rect(rect.x, rect.y, rect.width, height);
	}

	public static Rect VioletTransform(this Rect rect, Matrix4x4 matrix) {
		return new Rect(matrix.MultiplyPoint3x4(rect.position), matrix.MultiplyVector(rect.size));
	}

	public static float ShortestSide(this Rect rect) {
		return Mathf.Min(rect.width, rect.height);
	}

	public static float longestSide(this Rect rect) {
		return Mathf.Max(rect.width, rect.height);
	}

	public static Rect VioletSetSize(this Rect rect, float size) {
		return rect.VioletSetSize(Vector2.one * size);
	}

	public static Rect VioletSetSize(this Rect rect, float width, float height) {
		return rect.VioletSetSize(new Vector2(width, height));
	}

	public static Rect VioletSetSize(this Rect rect, Vector2 size) {
		return new Rect(rect.position, size);
	}

	public static Rect VioletSetSizeCentered(this Rect rect, float size) {
		return rect.VioletSetSizeCentered(Vector2.one * size);
	}

	public static Rect VioletSetSizeCentered(this Rect rect, float width, float height) {
		return rect.VioletSetSizeCentered(new Vector2(width, height));
	}

	public static Rect VioletSetSizeCentered(this Rect rect, Vector2 size) {
		return new Rect(rect.center - size * 0.5f, size);
	}

	#region Transalation
	public static Rect VioletTranslate(this Rect rect, float x, float y) {
		return VioletTranslate(rect, new Vector2(x, y));
	}

	public static Rect VioletTranslate(this Rect rect, Vector2 translation) {
		return new Rect(rect.position + translation, rect.size);
	}
	#endregion

	#region Insetting
	/// <summary>
	/// Split a Rect by insetting from the right edge.
	/// </summary>
	/// <param name="rect">Rect to split.</param>
	/// <param name="inset">Inset width.</param>
	/// <param name="result">Inset Rect.</param>
	/// <returns></returns>
	public static Rect VioletSplitRight(this Rect rect, float inset, out Rect result) {
		result = new Rect(rect.x + rect.width - inset, rect.y, inset, rect.height);

		return rect.VioletPadRight(inset);
	}

	/// <summary>
	/// Split a Rect by insetting from the left edge.
	/// </summary>
	/// <param name="rect">Rect to split.</param>
	/// <param name="inset">Inset width.</param>
	/// <param name="result">Inset Rect.</param>
	/// <returns></returns>
	public static Rect VioletSplitLeft(this Rect rect, float inset, out Rect result) {
		result = new Rect(rect.x, rect.y, inset, rect.height);

		return rect.VioletPadLeft(inset);
	}

	/// <summary>
	/// Split a Rect by insetting from the bottom edge.
	/// </summary>
	/// <param name="rect">Rect to split.</param>
	/// <param name="inset">Inset height.</param>
	/// <param name="result">Inset Rect.</param>
	/// <returns></returns>
	public static Rect VioletSplitBottom(this Rect rect, float inset, out Rect result) {
		result = new Rect(rect.x, rect.y + rect.height - inset, rect.width, inset);

		return rect.VioletPadBottom(inset);
	}

	/// <summary>
	/// Split a Rect by insetting from the top edge.
	/// </summary>
	/// <param name="rect">Rect to split.</param>
	/// <param name="inset">Inset height.</param>
	/// <param name="result">Inset Rect.</param>
	/// <returns></returns>
	public static Rect VioletSplitTop(this Rect rect, float inset, out Rect result) {
		result = new Rect(rect.x, rect.y, rect.width, inset);

		return rect.VioletPadTop(inset);
	}
	#endregion

	#region Padding
	/// <summary>
	/// Pad a Rect from all sides.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPad(this Rect rect, float padding) {
		return VioletPad(rect, padding, padding, padding, padding);
	}

	/// <summary>
	/// Pad a Rect from its left edge.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPadLeft(this Rect rect, float padding) {
		return VioletPad(rect, padding, 0.0f, 0.0f, 0.0f);
	}

	/// <summary>
	/// Pad a Rect from its right edge.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPadRight(this Rect rect, float padding) {
		return VioletPad(rect, 0.0f, padding, 0.0f, 0.0f);
	}

	/// <summary>
	/// Pad a Rect from its top edge.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPadTop(this Rect rect, float padding) {
		return VioletPad(rect, 0.0f, 0.0f, padding, 0.0f);
	}

	/// <summary>
	/// Pad a Rect from its bottom edge.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPadBottom(this Rect rect, float padding) {
		return VioletPad(rect, 0.0f, 0.0f, 0.0f, padding);
	}

	/// <summary>
	/// Pad a Rect from its left and right edges.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPadHorizontal(this Rect rect, float padding) {
		return VioletPad(rect, padding, padding, 0.0f, 0.0f);
	}

	/// <summary>
	/// Pad a Rect from its top and bottom edges.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="padding">Amount to pad.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPadVertical(this Rect rect, float padding) {
		return VioletPad(rect, 0.0f, 0.0f, padding, padding);
	}

	/// <summary>
	/// Pad a Rect from its left, right, top, and bottom edges.
	/// </summary>
	/// <param name="rect">Rect to pad.</param>
	/// <param name="left">Amount to pad left edge.</param>
	/// <param name="right">Amount to pad right edge.</param>
	/// <param name="top">Amount to pad top edge.</param>
	/// <param name="bottom">Amount to pad left edge.</param>
	/// <returns>Padded Rect.</returns>
	public static Rect VioletPad(this Rect rect, float left, float right, float top, float bottom) {
		return new Rect(rect.x + left, rect.y + top, rect.width - left - right, rect.height - top - bottom);
	}
	#endregion

	#region Sequencing
	public static Rect VioletStepDown(this Rect rect, float padding = 0.0f) {
		return new Rect(rect.x, rect.y + rect.height + padding, rect.width, rect.height);
	}

	public static Rect VioletStepUp(this Rect rect, float padding = 0.0f) {
		return new Rect(rect.x, rect.y - rect.height - padding, rect.width, rect.height);
	}

	public static Rect VioletStepLeft(this Rect rect, float padding = 0.0f) {
		return new Rect(rect.x - rect.width - padding, rect.y, rect.width, rect.height);
	}

	public static Rect VioletStepRight(this Rect rect, float padding = 0.0f) {
		return new Rect(rect.x + rect.width + padding, rect.y, rect.width, rect.height);
	}
	#endregion

	#region Extrusions
	public static Rect VioletExtrudeLeft(this Rect rect, float width) {
		return new Rect(rect.x - width, rect.y, width, rect.height);
	}

	public static Rect VioletExtrudeRight(this Rect rect, float width) {
		return new Rect(rect.x + rect.width, rect.y, width, rect.height);
	}

	public static Rect VioletExtrudeTop(this Rect rect, float height) {
		return new Rect(rect.x, rect.y - height, rect.width, height);
	}

	public static Rect VioletExtrudeBottom(this Rect rect, float height) {
		return new Rect(rect.x, rect.y + rect.height, rect.width, height);
	}
	#endregion


	#region Backfills
	public static Rect VioletBackfillLeft(this Rect rect, float width) {
		return rect.VioletPadRight(rect.width - width);
	}

	public static Rect VioletBackfillRight(this Rect rect, float width) {
		return rect.VioletPadLeft(rect.width - width);
	}

	public static Rect VioletBackfillTop(this Rect rect, float height) {
		return rect.VioletPadBottom(rect.height - height);
	}

	public static Rect VioletBackfillBottom(this Rect rect, float height) {
		return rect.VioletPadTop(rect.height - height);
	}
	#endregion

	#region Events
	public static bool WasContextClicked(this Rect rect, int contextButton = 1, bool useEvent = true) {
		if (Event.current.IsContextClick(contextButton) && rect.Contains(Event.current.mousePosition)) {
			if (useEvent) {
				Event.current.Use();
			}

			return true;
		}

		return false;
	}

	public static bool WasLeftClicked(this Rect rect, bool useEvent = true) {
		if (Event.current.IsLeftClick() && rect.Contains(Event.current.mousePosition)) {
			if (useEvent) {
				Event.current.Use();
			}

			return true;
		}

		return false;
	}
	public static bool WasDragged(this Rect rect, int dragButton = 0, bool useEvent = true) {
		if (Event.current.IsDrag(dragButton) && rect.Contains(Event.current.mousePosition)) {
			if (useEvent) {
				Event.current.Use();
			}

			return true;
		}

		return false;
	}
	#endregion
}