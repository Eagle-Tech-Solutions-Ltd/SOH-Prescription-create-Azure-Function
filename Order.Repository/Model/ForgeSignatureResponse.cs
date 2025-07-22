using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Model
{
    public class ForgeSignatureResponse
    {
        public ForgeSignatureResponse()
        {
            data = new ForgeSignatureDataResponse();
            status = new ForgeSignatureStatusResponse();
        }

        public ForgeSignatureDataResponse data { get; set; }
        public ForgeSignatureStatusResponse status { get; set; }
    }

    public class ForgeSignatureDataResponse
    {
        public ForgeSignatureDataResponse()
        {
            download_url = string.Empty;
        }

        public string download_url { get; set; }
    }

    public class ForgeSignatureStatusResponse
    {
        public ForgeSignatureStatusResponse()
        {
            success = false;
            status_code = 0;
        }

        public bool success { get; set; }
        public int status_code { get; set; }
    }
}
