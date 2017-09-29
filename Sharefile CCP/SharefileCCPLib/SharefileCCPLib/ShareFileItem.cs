using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharefileCCPLib
{
    public class ShareFileItem
    {
        public ShareFileItem(string filename, string owner, string dateModified, string fileType)
        {
            Filename = filename;
            Owner = owner;
            DateModified = dateModified;
            FileType = fileType;
        }

        public string Filename { get; set; }
        public string Owner { get; set; }
        public string DateModified { get; set; }
        public string FileType { get; set; }
    }
}
