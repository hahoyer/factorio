using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Net2Assembly
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymInitialize
        (
            IntPtr hProcess,
            string UserSearchPath,
            [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymCleanup(IntPtr hProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ulong SymLoadModuleEx
        (
            IntPtr hProcess,
            IntPtr hFile,
            string ImageName,
            string ModuleName,
            long BaseOfDll,
            int DllSize,
            IntPtr Data,
            int Flags);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymEnumerateSymbols64
        (
            IntPtr hProcess,
            ulong BaseOfDll,
            SymEnumerateSymbolsProc64 EnumSymbolsCallback,
            IntPtr UserContext);

        public delegate bool SymEnumerateSymbolsProc64(string SymbolName,
            ulong SymbolAddress,
            uint SymbolSize,
            IntPtr UserContext);

        public static string[] GetSymbols(this string assemblyPath)
        {
            var intPtr = LoadLibrary(assemblyPath);
            var result = GetSymbols(intPtr);
            FreeLibrary(intPtr);
            return result;
        }

        internal static string[] GetSymbols(this IntPtr hCurrentProcess)
        {
            var status = SymInitialize(hCurrentProcess, null, false);

            if(status == false)
                throw new Exception("Failed to initialize sym.");

            try
            {
                // Load dll.
                var baseOfDll = SymLoadModuleEx
                (
                    hCurrentProcess,
                    IntPtr.Zero,
                    "c:\\windows\\system32\\user32.dll",
                    null,
                    0,
                    0,
                    IntPtr.Zero,
                    0);

                if(baseOfDll == 0)
                    throw new Exception("Failed to load module.");

                var result = new List<string>();

                if(SymEnumerateSymbols64
                    (
                        hCurrentProcess,
                        baseOfDll,
                        (name, address, size, context) =>
                        {
                            result.Add(name);
                            return true;
                        },
                        IntPtr.Zero
                    )
                )
                    return result.ToArray();

                throw new Exception("Failed to enum symbols.");
            }
            finally
            {
                SymCleanup(hCurrentProcess);
            }
        }
    }
}