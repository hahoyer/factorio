using System;
using System.Runtime.InteropServices;

namespace Lua
{
    public enum LuaTypes
    {
        None = -1,
        Nil = 0,
        Boolean = 1,
        LightUserdata = 2,
        Number = 3,
        String = 4,
        Table = 5,
        Function = 6,
        UserData = 7,
        Thread = 8
    }

    static class NativeMethods
    {
        public const int LUA_REGISTRYINDEX	= (-10000);
        public const int LUA_ENVIRONINDEX	= (-10001);
        public const int LUA_GLOBALSINDEX	= (-10002);
        public static int LuaUpValueIndex(int i)	{return LUA_GLOBALSINDEX-i;}

        const string Libname = "lua52";

		static NativeMethods ()
		{
			DynamicLibraryPath.RegisterPathForDll (Libname);
		}

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gc")]
        internal static extern int LuaGC(this IntPtr luaState, int what, int data);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_typename")]
        internal static extern IntPtr LuaTypeName(this IntPtr luaState, int type);

        [DllImport
            (Libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "luaL_error")]
        internal static extern void LuaLError(this IntPtr luaState, string message);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_where")]
        internal static extern void LuaLWhere(this IntPtr luaState, int level);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_newstate")]
        internal static extern IntPtr LuaLNewState();

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_close")]
        internal static extern void LuaClose(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_openlibs")]
        internal static extern void LuaLOpenLibs(this IntPtr luaState);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luaL_loadstring")]
        internal static extern int LuaLLoadString(this IntPtr luaState, string chunk);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_loadstring")]
        internal static extern int LuaLLoadString(this IntPtr luaState, byte[] chunk);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_createtable")]
        internal static extern void LuaCreateTable(this IntPtr luaState, int narr, int nrec);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gettable")]
        internal static extern void LuaGetTable(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_settop")]
        internal static extern void LuaSetTop(this IntPtr luaState, int newTop);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_insert")]
        internal static extern void LuaInsert(this IntPtr luaState, int newTop);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_remove")]
        internal static extern void LuaRemove(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawget")]
        internal static extern void LuaRawGet(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_settable")]
        internal static extern void LuaSetTable(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawset")]
        internal static extern void LuaRawSet(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_setmetatable")]
        internal static extern void LuaSetMetatable(this IntPtr luaState, int objIndex);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_getmetatable")]
        internal static extern int LuaGetMetatable(this IntPtr luaState, int objIndex);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_equal")]
        internal static extern int LuaNetEqual(this IntPtr luaState, int index1, int index2);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushvalue")]
        internal static extern void LuaPushValue(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_replace")]
        internal static extern void LuaReplace(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gettop")]
        internal static extern int LuaGetTop(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_type")]
        internal static extern int LuaType(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_ref")]
        internal static extern int LuaLRef(this IntPtr luaState, int registryIndex);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawgeti")]
        internal static extern void LuaRawGetI(this IntPtr luaState, int tableIndex, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_rawseti")]
        internal static extern void LuaRawSetI(this IntPtr luaState, int tableIndex, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_newuserdata")]
        internal static extern IntPtr LuaNewUserData(this IntPtr luaState, uint size);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_touserdata")]
        internal static extern IntPtr LuaToUserData(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_unref")]
        internal static extern void LuaLUnref(this IntPtr luaState, int registryIndex, int reference);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_isstring")]
        internal static extern int LuaIsString(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_isstring_strict")]
        internal static extern int LuaNetIsStringStrict(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_iscfunction")]
        internal static extern int LuaIsCFunction(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushnil")]
        internal static extern void LuaPushNil(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_pcall")]
        internal static extern int LuaNetPCall(this IntPtr luaState, int nArgs, int nResults, int errfunc);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_tocfunction")]
        internal static extern IntPtr LuaToCFunction(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_tonumber")]
        internal static extern double LuaNetToNumber(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_toboolean")]
        internal static extern int LuaToBoolean(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_atpanic")]
        internal static extern void LuaAtPanic(this IntPtr luaState, IntPtr panicf);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushstdcallcfunction")]
        internal static extern void LuaPushStdCallCFunction(this IntPtr luaState, IntPtr function);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushnumber")]
        internal static extern void LuaPushNumber(this IntPtr luaState, double number);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushboolean")]
        internal static extern void LuaPushBoolean(this IntPtr luaState, int value);


        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_tolstring")]
        internal static extern IntPtr LuaToLString(this IntPtr luaState, int index, out uint strLen);

#if WSTRING
		[DllImport (LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint =
 "luanet_pushlwstring")]
#else
        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_pushlstring")]
#endif
        internal static extern void LuaNetPushLString(this IntPtr luaState, string str, uint size);

#if WSTRING
		[DllImport (LIBNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint =
 "luanet_pushwstring")]
#else
        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "lua_pushstring")]
#endif
        internal static extern void LuaPushString(this IntPtr luaState, string str);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luaL_newmetatable")]
        internal static extern int LuaLNewMetatable(this IntPtr luaState, string meta);

        [DllImport
            (Libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_getfield")]
        internal static extern void LuaGetField(this IntPtr luaState, int stackPos, string meta);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luaL_checkudata")]
        internal static extern IntPtr LuaLCheckUData(this IntPtr luaState, int stackPos, string meta);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luaL_getmetafield")]
        internal static extern int LuaLGetMetafield(this IntPtr luaState, int stackPos, string field);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_loadbuffer")]
        internal static extern int LuaNetLoadBuffer(this IntPtr luaState, string buff, uint size, string name);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_loadbuffer")]
        internal static extern int LuaNetLoadBuffer(this IntPtr luaState, byte[] buff, uint size, string name);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_loadfile")]
        internal static extern int LuaNetLoadFile(this IntPtr luaState, string filename);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_error")]
        internal static extern void LuaError(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_checkstack")]
        internal static extern int LuaCheckStack(this IntPtr luaState, int extra);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_next")]
        internal static extern int LuaNext(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_pushlightuserdata")]
        internal static extern void LuaPushLightUserData(this IntPtr luaState, IntPtr udata);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luaL_checkmetatable")]
        internal static extern int LuaLCheckMetatable(this IntPtr luaState, int obj);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gethookmask")]
        internal static extern int LuaGetHookMask(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_sethook")]
        internal static extern int LuaSetHook(this IntPtr luaState, IntPtr func, int mask, int count);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_gethookcount")]
        internal static extern int LuaGetHookCount(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_getinfo")]
        internal static extern int LuaGetInfo(this IntPtr luaState, string what, IntPtr ar);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_getstack")]
        internal static extern int LuaGetStack(this IntPtr luaState, int level, IntPtr n);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_getlocal")]
        internal static extern IntPtr LuaGetLocal(this IntPtr luaState, IntPtr ar, int n);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_setlocal")]
        internal static extern IntPtr LuaSetLocal(this IntPtr luaState, IntPtr ar, int n);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_getupvalue")]
        internal static extern IntPtr LuaGetUpValue(this IntPtr luaState, int funcindex, int n);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lua_setupvalue")]
        internal static extern IntPtr LuaSetUpValue(this IntPtr luaState, int funcindex, int n);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_tonetobject")]
        internal static extern int LuaNetToNetObject(this IntPtr luaState, int index);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_newudata")]
        internal static extern void LuaNetNewUData(this IntPtr luaState, int val);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_rawnetobj")]
        internal static extern int LuaNetRawNetObj(this IntPtr luaState, int obj);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_checkudata")]
        internal static extern int LuaNetCheckUData(this IntPtr luaState, int ud, string tname);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_gettag")]
        internal static extern IntPtr LuaNetGetTag();

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_pushglobaltable")]
        internal static extern void LuaNetPushGlobalTable(this IntPtr luaState);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_popglobaltable")]
        internal static extern void LuaNetPopGlobalTable(this IntPtr luaState);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_getglobal")]
        internal static extern void LuaNetGetGlobal(this IntPtr luaState, string name);

        [DllImport
        (
            Libname,
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi,
            EntryPoint = "luanet_setglobal")]
        internal static extern void LuaNetSetGlobal(this IntPtr luaState, string name);

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_registryindex")]
        internal static extern int LuaNetRegistryIndex();

        [DllImport(Libname, CallingConvention = CallingConvention.Cdecl, EntryPoint = "luanet_get_main_state")]
        internal static extern IntPtr LuaNetGetMainState(this IntPtr luaState);

    }
}