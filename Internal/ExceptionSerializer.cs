//using AkengMauiCrashReporter.Models;

//namespace AkengMauiCrashReporter.Internal
//{
//    internal static class ExceptionSerializer
//    {
//        public static CrashExceptionInfo? Serialize(Exception? exception)
//        {
//            if (exception is null)
//                return null;

//            return new CrashExceptionInfo
//            {
//                Type = exception.GetType().FullName ?? exception.GetType().Name,

//                Message = exception.Message,

//                StackTrace = exception.StackTrace,

//                InnerException = Serialize(exception.InnerException)
//            };
//        }
//    }
//}
