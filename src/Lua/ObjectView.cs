namespace Lua
{
    interface IObjectView
    {
        string AsString {get;}
        object AsObject {get;}
        string ToDebugString {get;}
        object Value {get;}
    }
}