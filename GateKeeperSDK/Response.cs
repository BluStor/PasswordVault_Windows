using System;
using System.IO;
using System.Text;

namespace GateKeeperSDK
{
    public class Response : IDisposable
    {
        /**
         * The numeric status code received at the conclusion of the action.
         */
        protected int MStatus;

        /**
         * The {@code String} device address of the paired CyberGate card
         */
        protected string MDeviceAddress;

        /**
         * The {@code String} message received at the conclusion of the action.
         */
        protected string MMessage;

        /**
         * The {@code File} representing the location that any data is stored from a response
         */
        protected Stream MDataFile;

        /**
         * Create a {@code Response} with the basic attributes of the given {@code Response}.
         *
         * @param response the {@code Response} object to copy
         * @since 0.16.0
         */
        public Response(Response response)
        {
            MStatus = response.Status;
            MMessage = response.Message;
            MDataFile = response.DataFile;
        }

        /**
         * Create a {@code Response} with the given status code and message.
         *
         * @param status  the numeric status code to classify this response
         * @param message the {@code String} message to describe this response
         * @since 0.5.0
         */
        public Response(int status, string message)
        {
            MStatus = status;
            MMessage = message;
        }

        /**
         * Create a {@code Response} with the given status code, message, and dataFile.
         *
         * @param status   the numeric status code to classify this response
         * @param message  the {@code String} message to describe this response
         * @param dataFile the {@code File} that holds body data for this response
         * @since 0.16.0
         */
        public Response(int status, string message, Stream dataFile)
        {
            MStatus = status;
            MMessage = message;
            MDataFile = dataFile;
        }

        /**
         * Create a {@code Response} with the given command data.
         *
         * @param commandData the data containing the status code and message
         * @since 0.5.0
         */

        /**
         * Create a {@code Response} with the given command and body data.
         *
         * @param commandData  the data containing the status code and message
         * @param bodyDataFile the {@code File} containing body data
         * @since 0.16.0
         */
        public Response(byte[] commandData, Stream bodyDataFile = null)
        {
            string responseString = Encoding.Default.GetString(commandData);
            var split = responseString.Split(new[] { ' ', '\n', '\r' }, 2);
            MStatus = Convert.ToInt32(split[0]);
            if (split.Length > 1)
            {
                MMessage = split[1];
            }
            MDataFile = bodyDataFile;
        }

        /**
         * Retrieve the status code of the Response.
         *
         * @return the numeric status code
         * @since 0.5.0
         */
        public int Status => MStatus;

        /**
         * Retrieve the message of the Response.
         *
         * @return the {@code String} message
         * @since 0.5.0
         */
        public string Message => MMessage;

        /**
         * Retrieve the status message of the Response.
         *
         * @return the space-separated status code and message
         * @since 0.5.0
         */
        public string StatusMessage => MStatus + " " + MMessage;

        /**
         * Retrieve the body data of the Response.
         *
         * @return the {@code File} that hold the response data
         * @since 0.16.0
         */
        public Stream DataFile => MDataFile;

        /**
         * Read the data file to a String. DO NOT use for large amounts of data.
         *
         * @return the String contents of the file, or an empty String if the file is not present or cannot be read
         * @since 0.17.0
         */
        public string ReadDataFile()
        {
            try
            {
                if (MDataFile != null)
                {
                    MDataFile.Position = 0;
                    using (var streamReader = new StreamReader(MDataFile))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (IOException e)
            {
                //Log.e(Response.class.getCanonicalName(), "Error reading data file", e);
            }

            return "";
        }

        public void Dispose()
        {
            MDataFile.Dispose();
        }

        public override string ToString()
        {
            long length = 0;
            string file = string.Empty;
            try
            {
                length = DataFile?.Length ?? 0;
                file = ReadDataFile();
            }
            catch (Exception){}
            return $"Status: {Status}\nMessage: {Message}\nData file: {length} bytes\n{file}";
        }
    }
}
