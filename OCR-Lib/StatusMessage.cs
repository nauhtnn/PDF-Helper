using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public class StatusMessage
    {
        static StatusMessage instance;

        string message;

        public StatusMessage()
        {
            message = string.Empty;
        }

        public static StatusMessage GetInstance()
        {
            if (instance == null)
            {
                instance = new StatusMessage();
            }
            return instance;
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
