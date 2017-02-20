using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateKeeperSDK
{
    public class Response : IDisposable
    {
        /**
         * The numeric status code received at the conclusion of the action.
         */
        protected int mStatus;

        /**
         * The {@code String} device address of the paired CyberGate card
         */
        protected String mDeviceAddress;

        /**
         * The {@code String} message received at the conclusion of the action.
         */
        protected String mMessage;

        /**
         * The {@code File} representing the location that any data is stored from a response
         */
        protected Stream mDataFile;

        /**
         * Create a {@code Response} with the basic attributes of the given {@code Response}.
         *
         * @param response the {@code Response} object to copy
         * @since 0.16.0
         */
        public Response(Response response)
        {
            mStatus = response.getStatus();
            mMessage = response.getMessage();
            mDataFile = response.getDataFile();
        }

        /**
         * Create a {@code Response} with the given status code and message.
         *
         * @param status  the numeric status code to classify this response
         * @param message the {@code String} message to describe this response
         * @since 0.5.0
         */
        public Response(int status, String message)
        {
            mStatus = status;
            mMessage = message;
        }

        /**
         * Create a {@code Response} with the given status code, message, and dataFile.
         *
         * @param status   the numeric status code to classify this response
         * @param message  the {@code String} message to describe this response
         * @param dataFile the {@code File} that holds body data for this response
         * @since 0.16.0
         */
        public Response(int status, String message, Stream dataFile)
        {
            mStatus = status;
            mMessage = message;
            mDataFile = dataFile;
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
            String responseString = Encoding.Default.GetString(commandData);
            String[] split = responseString.Split(new[] { ' ', '\n', '\r' }, 2);
            mStatus = Convert.ToInt32(split[0]);
            if (split.Length > 1)
            {
                mMessage = split[1];
            }
            mDataFile = bodyDataFile;
        }

        /**
         * Retrieve the status code of the Response.
         *
         * @return the numeric status code
         * @since 0.5.0
         */
        public int getStatus()
        {
            return mStatus;
        }

        /**
         * Retrieve the message of the Response.
         *
         * @return the {@code String} message
         * @since 0.5.0
         */
        public String getMessage()
        {
            return mMessage;
        }

        /**
         * Retrieve the status message of the Response.
         *
         * @return the space-separated status code and message
         * @since 0.5.0
         */
        public String getStatusMessage()
        {
            return mStatus + " " + mMessage;
        }

        /**
         * Retrieve the body data of the Response.
         *
         * @return the {@code File} that hold the response data
         * @since 0.16.0
         */
        public Stream getDataFile()
        {
            return mDataFile;
        }

        /**
         * Assign the file that holds the Response data.
         *
         * @param dataFile the {@code File} that holds response data
         * @since 0.16.0
         */
        public void setDataFile(Stream dataFile)
        {
            mDataFile = dataFile;
        }

        /**
         * Read the data file to a String. DO NOT use for large amounts of data.
         *
         * @return the String contents of the file, or an empty String if the file is not present or cannot be read
         * @since 0.17.0
         */
        public String readDataFile()
        {
            try
            {
                if (mDataFile != null)
                {
                    mDataFile.Position = 0;
                    using (var streamReader = new StreamReader(mDataFile))
                    {
                        return streamReader.ReadToEnd();
                    }
                    //return GKFileUtils.readFile(mDataFile);
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
            mDataFile.Dispose();
        }
    }
}
