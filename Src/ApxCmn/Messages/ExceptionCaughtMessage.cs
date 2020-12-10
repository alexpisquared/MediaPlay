using System.Runtime.Serialization;

namespace ApxCmn.Messages
{
    [DataContract]
    public class ExceptionCaughtMessage
    {
        public ExceptionCaughtMessage()
        {
        }

        public ExceptionCaughtMessage(string exceptionMsg)
        {
            this.ExceptionMsg = exceptionMsg;
        }

        [DataMember]
        public string ExceptionMsg;
    }
}
