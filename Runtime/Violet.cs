using UnityEngine;

/// <summary>
/// Static utility methods
/// </summary>
public static class Violet {
	/// <summary>
	/// Outputs a debug log tagged with a colored VioletUI
	/// </summary>
	/// <param name="s">String to log</param>
	public static void Log(string s) {
		Debug.Log($"{Color("VioletUI | ")}{s}");
	}

	/// <summary>
	/// Outputs a warning log tagged with a colored VioletUI
	/// </summary>
	/// <param name="s">String to log</param>
	public static void LogWarning(string s) {
		Debug.LogWarning($"{Color("VioletUI | ")}{s}");
	}

	/// <summary>
	/// Outputs an error log tagged with a colored VioletUI
	/// </summary>
	/// <param name="s">String to log</param>
	public static void LogError(string s) {
		Debug.LogError($"{Color("VioletUI | ")}{s}");
	}

	/// <summary>
	/// Logs if VIOLET_VERBOSE define symbol is set
	/// </summary>
	/// <param name="s"></param>
	public static void LogVerbose(string s) {
#if VIOLET_VERBOSE
		Log($"{Color("verbose | ")}{s}");
#endif
	}

	// #ceb2da
	// hsl(282, 35%, 78%)
	/// <summary>
	/// Violet color used for inspector buttons
	/// </summary>
	/// <returns></returns>
	public static Color Hue = new Color(r, g, b);

	public const float r = 0.898f;
	public const float g = 0.745f;
	public const float b = 0.935f;

	// #b300ff
	// rgb(179, 0, 255)
	// hsl(282, 100%, 50%)
	/// <summary>
	/// More saturated color used for log text
	/// </summary>
	/// <returns></returns>
	public static Color BrightHue = new Color(179/255, 0, 1);
	/// <summary>
	/// Wraps `s` in <color> tags for printing to unity console
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static string Color(string s, string hex = "b300ff") {
		return $"<color=#{hex}>{s}</color>";
	}

}