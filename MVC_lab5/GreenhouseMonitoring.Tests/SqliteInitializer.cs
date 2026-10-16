using SQLitePCL;

namespace GreenhouseMonitoring.Tests;

public static class SqliteInitializer
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Init()
    {
        Batteries.Init();
    }
}