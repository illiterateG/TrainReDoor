using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

internal static class BindingLogRegression
{
    private static Type bindingType;
    private static Type specType;
    private static MethodInfo remember;
    private static object system;
    private static int checks;

    private static Array Layout(float travel, int bindIndex = 13, bool left = true, string source = "ReDoor protocol")
    {
        object spec = Activator.CreateInstance(specType);
        specType.GetField("Left").SetValue(spec, left);
        specType.GetField("Z").SetValue(spec, travel);
        specType.GetField("Source").SetValue(spec, source);
        object binding = Activator.CreateInstance(bindingType);
        bindingType.GetField("Index").SetValue(binding, 5);
        bindingType.GetField("BindIndex").SetValue(binding, bindIndex);
        bindingType.GetField("Spec").SetValue(binding, spec);
        Array result = Array.CreateInstance(bindingType, 1);
        result.SetValue(binding, 0);
        return result;
    }

    private static bool TryLog(string mesh, Array layout)
    {
        return (bool)remember.Invoke(system, new object[] { mesh, layout });
    }

    private static void Check(bool pass, string label)
    {
        if (!pass) throw new InvalidOperationException(label);
        ++checks;
        Console.WriteLine("PASS " + label);
    }

    public static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, arg) =>
        {
            string path = Path.Combine(args[1], new AssemblyName(arg.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        Assembly mod = Assembly.LoadFrom(args[0]);
        Type systemType = mod.GetType("TrainReDoor.TrainDoorSystem", true);
        bindingType = systemType.GetNestedType("Binding", BindingFlags.NonPublic);
        specType = mod.GetType("TrainReDoor.DoorSpec", true);
        remember = systemType.GetMethod("RememberLoggedBinding", BindingFlags.Instance | BindingFlags.NonPublic);
        // EN: Run the managed logging policy in isolation from ECS and Unity native logging.
        // JA: ECS と Unity のネイティブログを使わず、ログの重複排除を検証します。
        system = FormatterServices.GetUninitializedObject(systemType);
        FieldInfo field = systemType.GetField("loggedBindings", BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(system, Activator.CreateInstance(field.FieldType));

        Check(TryLog("Middle", Layout(.85f)), "First layout is logged");
        int repeats = 0;
        for (int i = 0; i < 1000; ++i)
            if (TryLog("Middle", Layout(.85f))) ++repeats;
        Check(repeats == 0, "1000 independently allocated duplicate layouts are suppressed");
        Check(TryLog("Middle_Gls", Layout(.85f)), "Separate glass mesh is logged independently");
        Check(TryLog("Middle", Layout(.9f)), "Equal-count travel change is logged");
        Check(TryLog("Middle", Layout(.85f, 14)), "Equal-count bind-index change is logged");
        Check(TryLog("Middle", Layout(.85f, left: false)), "Equal-count side change is logged");
        Check(TryLog("Middle", Layout(.85f, source: "JSON")), "Binding source change is logged");
        Check(!TryLog("Middle", Layout(.85f)) && !TryLog("Middle", Layout(.9f)), "Alternating previously recorded layouts stay quiet");
        Console.WriteLine("BINDING_LOG_CHECKS=" + checks);
        return 0;
    }
}
