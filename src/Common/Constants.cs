using System;
using hw.Helper;


namespace Common
{
    public static class Constants
    {
        public const string TempExtension = ".temp";
        public const string RequestExtension = ".request";
        public const string ResponseExtension = ".response";

        public static readonly string RootPath = Environment
            .GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            .PathCombine("hw.FileCommunicator");
    }
}