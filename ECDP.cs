namespace eCDPSharp;

using System;
using System.Text;
using System.Text.RegularExpressions;

public partial class ECDP
{
	private static readonly byte[][] SHUFFLE_LUT =
	[
		[
			0x01, 0x0A, 0x16, 0x04, 0x07, 0x18, 0x0C, 0x10, 0x05, 0x17, 0x09, 0x03, 0x12, 0x08, 0x15, 0x13, 0x0B, 0x02,
			0x0F, 0x0D, 0x11, 0x0E, 0x06, 0x14
		],
		[
			0x07, 0x0C, 0x0E, 0x11, 0x09, 0x16, 0x10, 0x06, 0x14, 0x0D, 0x01, 0x02, 0x12, 0x08, 0x13, 0x0B, 0x0F, 0x0A,
			0x18, 0x15, 0x04, 0x05, 0x03, 0x17
		],
		[
			0x0F, 0x04, 0x09, 0x03, 0x06, 0x07, 0x11, 0x12, 0x15, 0x16, 0x02, 0x08, 0x05, 0x17, 0x0C, 0x0D, 0x01, 0x18,
			0x0B, 0x14, 0x0E, 0x10, 0x13, 0x0A
		],
		[
			0x02, 0x0A, 0x0E, 0x12, 0x0B, 0x03, 0x0C, 0x06, 0x13, 0x07, 0x11, 0x09, 0x15, 0x18, 0x10, 0x17, 0x14, 0x0F,
			0x04, 0x01, 0x05, 0x08, 0x16, 0x0D
		],
		[
			0x0B, 0x02, 0x09, 0x16, 0x14, 0x01, 0x12, 0x11, 0x15, 0x06, 0x0F, 0x17, 0x07, 0x10, 0x0C, 0x0E, 0x08, 0x18,
			0x13, 0x03, 0x0A, 0x0D, 0x04, 0x05
		],
		[
			0x09, 0x0F, 0x05, 0x0D, 0x16, 0x15, 0x12, 0x11, 0x03, 0x0A, 0x04, 0x10, 0x0E, 0x14, 0x02, 0x01, 0x13, 0x0C,
			0x06, 0x0B, 0x17, 0x18, 0x07, 0x08
		],
		[
			0x12, 0x02, 0x0C, 0x09, 0x0D, 0x0E, 0x04, 0x07, 0x16, 0x14, 0x17, 0x01, 0x11, 0x03, 0x10, 0x15, 0x08, 0x0A,
			0x05, 0x13, 0x0B, 0x18, 0x0F, 0x06
		]
	];

	private const string PASSWORD_ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

	public static string GeneratePassword(string? macAddress, string? storeNumber, string? storeManagementNumber)
	{
		if (macAddress == null || !HexRegex().IsMatch(macAddress.ToUpper()))
			throw new ArgumentException("MAC Address must be 12 hexadecimal characters in length, no separators!");

		if (storeNumber == null || !PINRegex().IsMatch(storeNumber))
			throw new ArgumentException("McDonald's Store Number must be a decimal number of exactly 6 digits!");

		if (storeManagementNumber == null || !PINRegex().IsMatch(storeManagementNumber))
			throw new ArgumentException(
				"McDonald's Store Management Number of DS Card must be a decimal number of exactly 6 digits!");

		var moddedMac = macAddress.ToUpper();
		var aggregate = moddedMac + storeNumber + storeManagementNumber;

		Console.WriteLine("- MAC used in generation: " + moddedMac);
		Console.WriteLine("- McDonald's Store Number: " + storeNumber);
		Console.WriteLine("- McDonald's Store Management Number: " + storeManagementNumber);
		Console.WriteLine("- Aggregated result for calculation: " + aggregate);

		var lutIndex = DetermineLUT(aggregate);
		return GetPassword(ShuffleInput(aggregate, lutIndex));
	}

	private static int DetermineLUT(string added)
	{
		var lutIndex = 0;
		var addString = new StringBuilder();
		foreach (var c in added)
		{
			addString.Append(int.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber));
			addString.Append('+');
			lutIndex += int.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
		}

		Console.WriteLine("- Determine LUT from addition: " + addString.ToString().TrimEnd('+'));
		if (lutIndex < 7)
		{
			Console.WriteLine("- Result LUT index = 0 (addition result is less than 7)");
			return 0;
		}

		Console.WriteLine($"- Result LUT index = {lutIndex % 7} ({lutIndex} mod 7)");
		return lutIndex % 7;
	}

	private static string ShuffleInput(string input, int lutIndex)
	{
		var result = new StringBuilder();
		Console.WriteLine($"- Shuffling with LUT table {lutIndex + 1} (LUT index {lutIndex} + 1)");

		var table = string.Join(" ", SHUFFLE_LUT[lutIndex]);
		Console.WriteLine($"Table (decimal) = {table}");

		for (var i = 0; i < input.Length; i++)
		{
			var c = input[SHUFFLE_LUT[lutIndex][i] - 1];
			Console.WriteLine($"Input character {SHUFFLE_LUT[lutIndex][i]:D2} (one-indexed): {c}");
			result.Append(c);
		}

		Console.WriteLine($"Result = {result}");
		return result.ToString();
	}

	private static string GetPassword(string shuffledResult)
	{
		var result = new StringBuilder();
		Console.WriteLine("- Generating password...");

		for (var i = 0; i < shuffledResult.Length / 4; i++)
		{
			var digit = int.Parse(shuffledResult.Substring(i * 4, 4), System.Globalization.NumberStyles.HexNumber);
			Console.WriteLine($"Calculate password char {i + 1} -> {digit:X4} modulo 33: {digit % 33}");

			digit %= 33;
			result.Append(PASSWORD_ALPHABET[digit]);
		}

		return result.ToString();
	}

	[GeneratedRegex(@"[0123456789ABCDEF]{12}")]
	private static partial Regex HexRegex();

	[GeneratedRegex(@"[0-9]{6}")]
	private static partial Regex PINRegex();
}