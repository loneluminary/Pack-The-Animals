using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Utilities.Extensions
{
	public static class StringExtension
	{
		public static T ToEnum<T>(this string str) where T : struct, Enum
		{
			if (Enum.TryParse<T>(str, true, out var result)) return result;

			throw new ArgumentException($"Cannot convert '{str}' to enum {typeof(T).Name}");
		}

		public static string Truncate(this string str, int maxLength)
		{
			if (string.IsNullOrEmpty(str)) return str;

			return str.Length <= maxLength ? str : str[..maxLength];
		}

		public static string ToTitleCase(this string str)
		{
			return string.IsNullOrEmpty(str) ? str : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
		}

		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

		public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

		public static string Reverse(this string str)
		{
			if (string.IsNullOrEmpty(str)) return str;

			char[] charArray = str.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		public static string RemoveWhitespace(this string str) => new(str.Where(c => !char.IsWhiteSpace(c)).ToArray());

		public static string ToCamelCase(this string str)
		{
			if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
				return str;

			string camelCase = char.ToLower(str[0]).ToString();
			if (str.Length > 1)
				camelCase += str.Substring(1);
			return camelCase;
		}

		public static string SplitCamelCase(this string str)
		{
			return string.IsNullOrEmpty(str) ? str : Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
		}
	
		public static float TryParseFloat(this string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Replace(',', '.');
				string text2 = text;
				if (text2[0] == '$')
				{
					text2 = text2.Remove(0, 1);
				}
				int length = text2.Length;
				int num = 0;
				int num2 = 0;
				bool flag = false;
				for (int i = 0; i < length; i++)
				{
					if (text2[i] == '<')
					{
						flag = true;
						num = i;
					}
					if (text2[i] == '>')
					{
						num2 = i;
						break;
					}
				}
				if (flag)
				{
					text = text2.Remove(num, num2 - num + 1);
				}
				CultureInfo provider = new CultureInfo("en-US");
				const NumberStyles style = NumberStyles.Any;
				if (float.TryParse(text, style, provider, out var result))
				{
					return result;
				}
			}
		
			return 0f;
		}
		
		public static string ToCapital(this string input)
		{
			return input switch
			{
				null => throw new ArgumentNullException(nameof(input)),
				"" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
				_ => input[0].ToString().ToUpper() + input[1..]
			};
		}

		public static string Color(this string text, Color color)
		{
			return $"<color={color.ToHex()}>{text}</color>";
		}

		private static string ToHex(this Color color)
		{
			return $"#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}";
		}

		private static byte ToByte(float f)
		{
			f = Mathf.Clamp01(f);
			return (byte)(f * 255f);
		}
	}
}