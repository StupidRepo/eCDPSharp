using eCDPSharp;

Console.WriteLine("Enter the MAC address of the device with no separators (e.g. 00AABBCCDDEE):");
var macAddress = Console.ReadLine();

Console.WriteLine("Enter any 6-digit McDonald's Store Number (can be made up):");
var storeNumber = Console.ReadLine();

Console.WriteLine("Enter the 6-digit McDonald's Store Management Number (can be made up):");
var storeManagementNumber = Console.ReadLine();

Console.WriteLine("Password: " + ECDP.GeneratePassword(macAddress, storeNumber, storeManagementNumber));