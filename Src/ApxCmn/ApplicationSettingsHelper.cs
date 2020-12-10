using System.Diagnostics;
using Windows.Storage;

namespace ApxCmn
{
	/// <summary>
	/// Collection of string constants used in the entire solution. This file is shared for all projects
	/// </summary>
	public static class ApplicationSettingsHelper
	{
		public static object ReadResetSettingsValue(string key)
		{
			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
			{
				Debug.WriteLine($"Settings value for key '{key}' NOT found!!!");
				return null;
			}
			else
			{
				var value = ApplicationData.Current.LocalSettings.Values[key];
				ApplicationData.Current.LocalSettings.Values.Remove(key);
				Debug.WriteLine($"Settings value for key '{key}' found '{value}'");
				return value;
			}
		}

		public static void SaveSettingsValue(string key, object value)
		{
			Debug.WriteLine($"Storing settings key '{key}' : '{value}'");

			if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
			{
				ApplicationData.Current.LocalSettings.Values.Add(key, value);
			}
			else
			{
				ApplicationData.Current.LocalSettings.Values[key] = value;
			}
		}

	}
}
