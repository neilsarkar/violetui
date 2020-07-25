using UnityEngine;

/// <summary>
/// Static utility methods
/// </summary>
public static class Violet {
	public static void Log(string s) {
		Debug.Log($"{Color("VioletUI")} | {s}");
	}

	public static void LogWarning(string s) {
		Debug.LogWarning($"{Color("VioletUI")} | {s}");
	}

	public static void LogError(string s) {
		Debug.LogError($"{Color("VioletUI")} | {s}");
	}

	// #ceb2da
	// hsl(282, 35%, 78%)
	public static Color Hue = new Color(0.898f, 0.745f, 0.935f);

	// #b300ff
	// rgb(179, 0, 255)
	// hsl(282, 100%, 50%)
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