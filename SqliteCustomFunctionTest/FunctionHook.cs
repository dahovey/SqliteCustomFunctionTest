using System;
using System.Runtime.InteropServices;

namespace SqliteCustomFunctionTest
{
    internal class FunctionHook
    {
        private CallbackDelegate _func;
        private object _user_data;
        private GCHandle _h;

        internal FunctionHook(CallbackDelegate func, object v)
        {
            this._func = func;
            this._user_data = v;
            this._h = GCHandle.Alloc((object)this);
        }

        internal IntPtr ptr
        {
            get
            {
                return (IntPtr)this._h;
            }
        }

        internal static FunctionHook from_ptr(IntPtr p)
        {
            return ((GCHandle)p).Target as FunctionHook;
        }

        internal void call(IntPtr context, int num_args, IntPtr argsptr)
        {
            //var scalarSqlite3Context = new scalar_sqlite3_context(context, this._user_data);
            //sqlite3_value[] args = new sqlite3_value[num_args];
            //int num = Marshal.SizeOf(typeof(IntPtr));
            //for (int index = 0; index < num_args; ++index)
            //{
            //    IntPtr p = Marshal.ReadIntPtr(argsptr, index * num);
            //    args[index] = new sqlite3_value(p);
            //}
            this._func(context, num_args, argsptr);
        }

        internal void free()
        {
            this._func = (CallbackDelegate)null;
            this._user_data = (object)null;
            this._h.Free();
        }
    }
}
