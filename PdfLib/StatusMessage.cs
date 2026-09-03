using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrLib
{
    public class StatusMessage
    {
        static StatusMessage _instance;

        string message;

        public StatusMessage()
        {
            message = string.Empty;
        }

        public static StatusMessage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StatusMessage();
                }
                return _instance;
            }
        }

        public void AddMessage(string newMessage)
        {
            message += DateTime.Now.ToString("[dd/MM/yyyy HH:mm:ss] ") + newMessage + "\n";
        }
        
        public string ConsumeMessage()
        {
            string temp = message;
            message = string.Empty;
            return temp;
        }
    }
}
