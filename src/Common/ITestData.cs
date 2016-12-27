namespace Common
{
    public interface ITestData
    {
        string[] TestArray { get; }
        int TestInt { get; }
        string TestString { get; }
        string TestFunction(int arg1, string arg2);
    }
    public interface ITestData1
    {
        string[] TestArray { get; }
        int TestInt { get; }
        string TestString { get; }
        string TestFunction(int arg1, string arg2);
    }
}