using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GateKeeperSDK;
using GateKeeperSDKTest;

namespace ConsoleTest
{
    public class CardTest
    {
        public Card Card { get; set; }
        public Response Response { get; set; }

        public CardTest(Card card)
        {
            Card = card;
        }

        public object ExecuteMethod(string[] inputArray)
        {
            try
            {
                switch (inputArray[0].ToLower())
                {
                    case "mlst":
                        return Card.MLST(inputArray[1]);
                    case "list":
                        return Card.List(inputArray[1]);
                    case "changeworkingdirectory":
                        return Card.ChangeWorkingDirectory(inputArray[1]);
                    case "currentworkingdirectory":
                        return Card.CurrentWorkingDirectory();
                    case "put":
                        {
                            var bytes = File.ReadAllBytes(inputArray[2]);
                            return Card.Put(inputArray[1], bytes);
                        }
                    case "get":
                        {
                            var result = Card.Get(inputArray[1]);

                            if (inputArray.Length == 3)
                            {
                                if (result.DataFile != null)
                                {

                                    result.DataFile.Position = 0;
                                    using (var ms = new BinaryReader(result.DataFile))
                                    {
                                        File.WriteAllBytes(inputArray[2], ms.ReadBytes((int)result.DataFile.Length));
                                    }
                                }
                            }

                            return result;

                        }
                    case "freememory":
                        return Card.FreeMemory();
                    case "rename":
                        return Card.Rename(inputArray[1], inputArray[2]);
                    case "delete":
                        return Card.Delete(inputArray[1]);
                    case "createpath":
                        return Card.CreatePath(inputArray[1]);
                    case "deletepath":
                        return Card.DeletePath(inputArray[1]);
                    case "disconnect":
                        {
                            Card.Disconnect();
                            return "Card disconnected";
                        }
                    case "connect":
                        {
                            Card.BuildConnection();
                            return "Connection established";
                        }
                    default:
                        throw new Exception("There are no such method. \nType 'help' to see all commands.");
                }
            }
            catch (Exception e)
            {
                var message = e.InnerException?.Message ?? e.Message;
                return $"Command parameters are not valid: {message}";
            }
        }

        public string Help()
        {
            return
                @"
    Connect: Connects to the card
    Example: Connect

    Disconnect: Disconnects the card
    Example: Disconnect    

    List: Files and Directories
    Example: List /data

    ChangeWorkingDirectory: Set working directory
    Example: ChangeWorkingDirectory /data/test_folder

    CurrentWorkingDirectory: Get working directory
    Example: CurrentWorkingDirectory

    Put: Copy files to card
    Example: Put /data/test.txt C:/Users/User/GateKeeperSDKTest/test.txt

    Get: Download a file
    Example: Get /data/test.txt C:/Users/User/GateKeeperSDKTest/test.txt

    FreeMemory: Get free memory value on the card
    Example: FreeMemory

    Rename: Rename file
    Example: Rename /data/test.txt /data/temp123.txt

    Delete: Delete File
    Example: Delete /data/test.txt

    CreatePath: Make a directory
    Example: CreatePath /data/test_folder

    DeletePath: Delete Folder
    Example: DeletePath /data/test_folder";
        }

        public string MethodList()
        {
            var result = new List<string>();
            var methods = Card.GetType().GetMethods();
            var objectMethods = typeof(object).GetMethods().Select(x => x.Name).Concat(typeof(IDisposable).GetMethods().Select(x => x.Name));
            foreach (var methodInfo in methods.Where(x => x.IsPublic && !objectMethods.Contains(x.Name) && !x.Name.Contains("get_")))
            {
                string mI = $"{methodInfo.Name} (";
                var parameters = methodInfo.GetParameters().Aggregate("", (current, parameterInfo) => current + $"{parameterInfo.ParameterType.Name} {parameterInfo.Name}, ");
                mI += parameters;
                if (parameters.Length > 0) mI = mI.Remove(mI.Length - 2);
                mI += ")";
                result.Add(mI);
            }

            return result.Aggregate("", (x, y) => $"{x}\n{y}");
        }
    }
}
