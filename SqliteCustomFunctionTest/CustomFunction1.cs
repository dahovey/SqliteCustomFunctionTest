using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SqliteCustomFunctionTest
{
    public delegate void CallbackDelegate(IntPtr handle, int parameterCount, IntPtr parametersHandle);

    public class CustomFunction1 : IDisposable
    {
        private FunctionHook _hook;

        private static callback_scalar_function _bridge = new callback_scalar_function(CustomFunctionCallback);
        public const string Name = "Fn_Custom1";

        private readonly SqliteConnection _handle;

        public CustomFunction1(SqliteConnection conn)
        {
            _handle = conn;

            RegisterFunction();
        }

        public void Dispose()
        {
            _hook.free();

            sqlite3_create_function_v2(_handle.Handle, GetBytes(Name),
                0, 1, IntPtr.Zero, null, null, null, null);
        }

        private void FunctionCallback(IntPtr context, int num_args, IntPtr arguments)
        {
            try
            {
                Console.WriteLine($"Parameters: {num_args}");

                sqlite3_result_null(context);
            }
            catch (Exception e)
            {
                var msg = e.ToString();
                sqlite3_result_error(context, GetBytes(msg), msg.Length);
            }
        }

        private static void CustomFunctionCallback(IntPtr context, int parameterCount, IntPtr parametersHandle)
        {
            FunctionHook.from_ptr(sqlite3_user_data(context)).call(context, parameterCount, parametersHandle);
        }

        private byte[] GetBytes(string value)
        {
            byte[] bytes = new byte[Encoding.UTF8.GetByteCount(value) + 1];
            bytes[Encoding.UTF8.GetBytes(value, 0, value.Length, bytes, 0)] = (byte)0;
            return bytes;
        }

        private void RegisterFunction()
        {
            _hook = new FunctionHook(FunctionCallback, null);

            sqlite3_create_function_v2(_handle.Handle, GetBytes(Name),
                    0, 1, _hook.ptr, _bridge, null, null, null);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UserFunctionCallback(IntPtr context, int nvalues, IntPtr values);

        private const string DllName = "sqlite3.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void sqlite3_result_int64(IntPtr context, long value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void sqlite3_result_null(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sqlite3_user_data(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int sqlite3_create_function(IntPtr dbHandle, byte[] functionName, int numArgs, int textEncoding,
            IntPtr pApp, UserFunctionCallback xFunc, IntPtr xStep, IntPtr xFinal);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void callback_agg_function_final(IntPtr context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void callback_scalar_function(IntPtr context, int nArgs, IntPtr argsptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void callback_agg_function_step(IntPtr context, int nArgs, IntPtr argsptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void callback_destroy(IntPtr p);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_create_function_v2(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, callback_scalar_function func, callback_agg_function_step fstep, callback_agg_function_final ffinal, callback_destroy fdestroy);

    }
}
