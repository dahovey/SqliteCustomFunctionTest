using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SqliteCustomFunctionTest
{
    public class CustomFunction1 : IDisposable
    {
        public const string Name = "Fn_Custom1";

        private readonly IntPtr _handle;

        public CustomFunction1(SqliteConnection conn)
        {
            _handle = conn.Handle.ptr;

            RegisterFunction();
        }

        public void Dispose()
        {
        }

        private void FunctionCallback(IntPtr context, int valueCount, IntPtr values)
        {
            try
            {
                Console.WriteLine($"Parameters: {valueCount}");
                sqlite3_result_null(context);
            }
            catch (Exception e)
            {
                var msg = e.ToString();
                sqlite3_result_error(context, Encoding.UTF8.GetBytes(msg), msg.Length);
            }
        }
        
        private void RegisterFunction()
        {
            var functionName = Encoding.UTF8.GetBytes(Name);

            var status = sqlite3_create_function(_handle, functionName,
                 0, 4, IntPtr.Zero, FunctionCallback, IntPtr.Zero, IntPtr.Zero);

            if (status == 0)
                sqlite3_create_function(_handle, functionName,
                    0, 1, IntPtr.Zero, FunctionCallback, IntPtr.Zero, IntPtr.Zero);
        }
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UserFunctionCallback(IntPtr context, int nvalues, IntPtr values);

        private const string DllName = "e_sqlite3.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_int64(IntPtr context, long value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void sqlite3_result_null(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_create_function(IntPtr dbHandle, byte[] functionName, int numArgs, int textEncoding,
            IntPtr pApp, UserFunctionCallback xFunc, IntPtr xStep, IntPtr xFinal);
    }
}
