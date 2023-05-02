using Pixel;
using System;

namespace Pixel.Types
{
    public enum CmdError { ArgumentNotFound, NullReference, InvalidOperation, InvalidCast };
    public partial class Command
    {
        private static readonly string[] errorMessages = 
        { 
            " was not called: Argument provided was either invalid or null",
            " was not called: There was a null reference. check the command object's error object for more info.",
            " was not called: An invalid operation was attempted.",
            " was not called: An invalid cast operation was attempted."
        };
        public static void Error(string cmdName, string errorInfo)
        {
            Interop.Log($"{cmdName} was not called : {errorInfo}");
        }
        public static void Error(string cmdName, CmdError error)
        {
            switch (error)
            {
                case CmdError.ArgumentNotFound:
                    Interop.Log($"{cmdName}" + errorMessages[0]);
                        break;       
                    
                case CmdError.NullReference:
                    Interop.Log($"{cmdName}" + errorMessages[1]);
                        break;                 
                                               
                case CmdError.InvalidOperation:
                    Interop.Log($"{cmdName}" + errorMessages[2]);
                        break;                 
                                               
                case CmdError.InvalidCast:
                    Interop.Log($"{cmdName}" + errorMessages[3]);
                        break;               
                                             
                default:
                    Interop.Log($"{cmdName}" + errorMessages[4]);
                         break;
            };
        }
    }
}