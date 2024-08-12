#nullable disable
using System.Runtime.ExceptionServices;

namespace Guru.Internal
{
    public enum ELogLevel
    {
        NONE = 0,
        VERBOSE = 1,
        LOG = 2,
        WARNING = 3,
        ERROR = 4,
    }

    public static class LoggerUtils
    {
        private static int _logLevel = 0;
        private static StreamWriter _logFileWriter;

        public static void SetLogLevel(ELogLevel logLevel)
        {
            _logLevel = (int)logLevel;
        }

        public static void SetLogFileDir(string logFileDir)
        {
            if (!Directory.Exists(logFileDir))
                if (logFileDir != null)
                    Directory.CreateDirectory(logFileDir);
            string logFilePath = $"{logFileDir}/{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
            _logFileWriter = new StreamWriter(logFilePath);
        }

        public static void Verbose(string location, string operation, string log)
        {
            if (_logLevel > (int)ELogLevel.VERBOSE)
                return;

            string msg = GetLogMsg(ELogLevel.VERBOSE, location, operation, log);
            Console.WriteLine($"{msg}");
        }

        public static void Log(string location, string operation, string log)
        {
            if (_logLevel > (int)ELogLevel.LOG)
                return;

            string msg = GetLogMsg(ELogLevel.LOG, location, operation, log);
            Console.WriteLine($"{msg}");
        }

        public static void LogWarning(string location, string operation, string warning)
        {
            if (_logLevel > (int)ELogLevel.WARNING)
                return;

            string msg = GetLogMsg(ELogLevel.WARNING, location, operation, warning);

            Console.WriteLine($"{msg}");
        }

        public static void LogError(string location, string operation, string error)
        {
            string msg = GetLogMsg(ELogLevel.ERROR, location, operation, error);
            Console.Error.WriteLine($"{msg}");
        }

        public static void LogException(System.Exception source, string message = null)
        {
            if (source == null)
            {
                return;
            }

            string exceptionMessage;
            if (!string.IsNullOrEmpty(message))
            {
                exceptionMessage = $"\n{message}\n{source.Message}";
            }
            else
            {
                exceptionMessage = source.Message;
            }

            Exception customException = new Exception(exceptionMessage, source);
            var info = ExceptionDispatchInfo.Capture(customException);

            Console.Error.WriteLine($"[异常] {exceptionMessage}");
        }

        private static string GetLogMsg(ELogLevel level, string location, string operation, string log)
        {
            string msg = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{Enum.GetName(typeof(ELogLevel), level)}] [{location}].[{operation}] : {log}";
            return msg;
        }

        private static void Log2File(string message)
        {
            if (_logFileWriter is null)
            {
                return;
            }


        }
    }
}