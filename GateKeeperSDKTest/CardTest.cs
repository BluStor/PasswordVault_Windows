using System;
using System.Collections.Generic;
using System.Linq;
using GateKeeperSDK;

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

        private Response UploadFile(string fileName)
        {
            try
            {
                var buffer = new byte[256];
                new Random().NextBytes(buffer);
                Response = Card.Put(fileName, buffer);
            }
            catch (Exception e)
            {
                Response = new Response(550, e.Message);
            }

            return Response;
        }


        public object ExecuteMethod(string[] inputArray)
        {
            try
            {
                if (inputArray[0] == "Put")
                {
                    return UploadFile(inputArray[1]);
                }
                var method = Card.GetType().GetMethod(inputArray[0]);
                var parameters = inputArray.Select(x => (object) x).ToList();
                parameters.Remove(parameters[0]);
                var result = method.Invoke(Card, parameters.ToArray());
                return result;
            }
            catch (Exception e)
            {
                var message = e.InnerException?.Message ?? e.Message;
                return $"Command parameters are not valid: {message}";
            }
        }

        public string MethodList()
        {
            var result = new List<string>();
            var methods = Card.GetType().GetMethods();
            var objectMethods = typeof (object).GetMethods().Select(x => x.Name).Concat(typeof (IDisposable).GetMethods().Select(x => x.Name));
            foreach (var methodInfo in methods.Where(x => x.IsPublic && !objectMethods.Contains(x.Name) && !x.Name.Contains("get_")))
            {
                string mI = $"{methodInfo.Name} (";
                var parameters = methodInfo.GetParameters().Aggregate("", (current, parameterInfo) => current + $"{parameterInfo.ParameterType.Name} {parameterInfo.Name}, ");
                mI += parameters;
                if(parameters.Length > 0) mI = mI.Remove(mI.Length - 2);
                mI += ")";
                result.Add(mI);
            }

            return result.Aggregate("", (x, y) => $"{x}\n{y}");
        }
    }
}
