using System;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace CloudCopyAndPasteShellExtension
{
    [Serializable]
    public class ClipboardData : ISerializable
    {
        private object data = null;
        public ClipboardData() { }
        public ClipboardData(object data)
            : base()
        {
            this.data = data;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data", data);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }
    }
}