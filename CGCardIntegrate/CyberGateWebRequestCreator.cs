using System;
using System.Net;

namespace CGCardIntegrate
{
    public class CyberGateWebRequestCreator : IWebRequestCreate
    {

        public void Register()
        {
            WebRequest.RegisterPrefix("cybergate", this);
        }

        public WebRequest Create(Uri uri)
        {
            return new CyberGateWebRequest(uri);
        }
    }
}
