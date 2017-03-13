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
            var time = DateTime.Now;
            using (Card card = new Card("CYBERGATE", BluetoothRadio.PrimaryRadio.LocalAddress.ToString(), false))
            {
                Console.WriteLine($"Card connection: {(DateTime.Now - time).TotalSeconds} sec");
                var cardTest = new CardTest(card);
                bool exit = false;

                while (!exit)
                {
                    try
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

                        card.BuildConnection();

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
                    finally
                    {
                        card.Disconnect();
                    }
                }
            }
        }
    }
}