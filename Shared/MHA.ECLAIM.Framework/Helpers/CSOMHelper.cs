using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using MHA.Framework.Core.SP;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using File = Microsoft.SharePoint.Client.File;

using Microsoft.IdentityModel.Tokens;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class CSOMHelper
    {
        public static File GetFileByServerRelativePath(string fileUrl, ClientContext clientContext)
        {
            fileUrl = Uri.UnescapeDataString(fileUrl);
            ResourcePath resourcePath = ResourcePath.FromDecodedUrl(fileUrl);
            File targetFile = clientContext.Web.GetFileByServerRelativePath(resourcePath);

            return targetFile;
        }
    }
}
