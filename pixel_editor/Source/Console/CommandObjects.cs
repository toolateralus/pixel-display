﻿using pixel_renderer;
using System;

namespace pixel_editor
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

        internal static void Error(string cmdName, string errorInfo)
        {
            Runtime.Log($"{cmdName} was not called : {errorInfo}");
        }


        internal static void Error(string cmdName, CmdError error)
        {
            switch (error)
            {
                case CmdError.ArgumentNotFound:
                    Runtime.Log($"{cmdName}" + errorMessages[0]);
                        break;       
                    
                case CmdError.NullReference:  
                    Runtime.Log($"{cmdName}" + errorMessages[1]);
                        break;                 
                                               
                case CmdError.InvalidOperation:
                    Runtime.Log($"{cmdName}" + errorMessages[2]);
                        break;                 
                                               
                case CmdError.InvalidCast:    
                    Runtime.Log($"{cmdName}" + errorMessages[3]);
                        break;               
                                             
                default:                       
                    Runtime.Log($"{cmdName}" + errorMessages[4]);
                         break;
            };
        }
    }
}