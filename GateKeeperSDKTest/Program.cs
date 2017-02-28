using System;
using ConsoleTest;
using GateKeeperSDK;
using InTheHand.Net.Bluetooth;

namespace GateKeeperSDKTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Response res;
            var time = DateTime.Now;
            using (Card card = new Card(Constants.Password, null, BluetoothRadio.PrimaryRadio.LocalAddress.ToString(), true))
            {
                Console.WriteLine($"Card connection: {(DateTime.Now - time).TotalSeconds} sec");
                var cardTest = new CardTest(card);
                bool exit = false;

                while (!exit)
                {
                    Console.Write("\nCommand: ");
                    var input = Console.ReadLine();
                    if (input == "help")
                    {
                        Console.WriteLine(cardTest.Help());
                        continue;
                    }

                    if (input == "exit")
                    {
                        exit = true;
                        continue; 
                    }

                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Command input is invalid");
                        continue;
                    }

                    var inputArray = input.Split(' ');

                    time = DateTime.Now;
                    var result = cardTest.ExecuteMethod(inputArray);
                    Console.WriteLine($"Method execution: {(DateTime.Now - time).TotalSeconds} sec");
                    if (result != null)
                    {
                        var response = result as Response;

                        Console.WriteLine("Response:");
                        Console.WriteLine(response?.ToString() ?? result.ToString());
                    }
                }
            }
        }
    }
}