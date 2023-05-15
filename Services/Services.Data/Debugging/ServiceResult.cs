using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToucanServices.Services.Data.Debugging
{
    public enum LogLevel
    {
        SUCCESS,
        WARNING,
        ERROR
    }

    public struct ServiceResult<T>
    {
        /// <summary>
        /// Check this if there was any errors, warnings, etc.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The value returned from a function.
        /// </summary>
        public T? Result { get; set; }


        /// <summary>
        /// Warning, Error, etc. Use this to check what type of error.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// The message of the causation of an unsuccessful attempt
        /// </summary>
        public string Message { get; set; }
    }
}
