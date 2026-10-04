using System.Reflection;
using TOP4HoneyChains.Web.Tests;

// Minimal test runner: every public static void method of a class marked [Tests] is a test
var failed = 0;
var passed = 0;
foreach (var type in typeof(TestsAttribute).Assembly.GetTypes().Where(t => t.GetCustomAttribute<TestsAttribute>() != null))
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0))
    {
        try
        {
            method.Invoke(null, null);
            passed++;
            Console.WriteLine($"PASS {type.Name}.{method.Name}");
        }
        catch (TargetInvocationException e)
        {
            failed++;
            Console.WriteLine($"FAIL {type.Name}.{method.Name}: {e.InnerException?.Message}");
        }
    }
}
Console.WriteLine($"{passed} passed, {failed} failed");
return failed == 0 && passed > 0 ? 0 : 1;

namespace TOP4HoneyChains.Web.Tests
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestsAttribute : Attribute { }

    public static class Assert
    {
        public static void Equal<T>(T expected, T actual, string? what = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception($"{what ?? "value"}: expected <{expected}>, got <{actual}>");
        }

        public static void Null(object? actual, string? what = null)
        {
            if (actual != null)
                throw new Exception($"{what ?? "value"}: expected null, got <{actual}>");
        }

        public static void NotNull(object? actual, string? what = null)
        {
            if (actual == null)
                throw new Exception($"{what ?? "value"}: expected a value, got null");
        }
    }
}
