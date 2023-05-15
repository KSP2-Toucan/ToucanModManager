using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToucanServices.Services.Data.Debugging
{
    public static class ServiceDebugger
    {
        public static ServiceResult<T> Error<T>(string ErrorMessage)
        {
            return new ServiceResult<T> 
            {
                IsSuccess = false,
                Result = default(T),
                Level = LogLevel.ERROR,
                Message = ErrorMessage
            };
        }

        public static ServiceResult<T> Warning<T>(string WarningMessage)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Result = default(T),
                Level = LogLevel.WARNING,
                Message = WarningMessage
            };
        }

        public static ServiceResult<T> Success<T>(T Result)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Result = Result,
                Level = LogLevel.SUCCESS,
                Message = ""
            };
        }
    }
}
