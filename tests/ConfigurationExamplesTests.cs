using System;
using System.IO;
using System.Reflection;
using TrainReDoor;

// EN: Parse bundled examples with the real runtime configuration reader.
// JA: 同梱サンプルを実行用 DLL の設定リーダーで解析します。
internal static class ConfigurationExamplesTests
{
    public static int Main(string[] args)
    {
        string library = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs ev)
        {
            string local = Path.Combine(Path.GetDirectoryName(library), new AssemblyName(ev.Name).Name + ".dll");
            string game = Path.Combine(args[1], new AssemblyName(ev.Name).Name + ".dll");
            return File.Exists(local) ? Assembly.LoadFrom(local) : File.Exists(game) ? Assembly.LoadFrom(game) : null;
        };
        try { return Run(args); }
        catch (Exception error) { Console.WriteLine("FAIL_TYPE=" + error.GetType().FullName); Console.WriteLine("FAIL_MESSAGE=" + error.Message); return 1; }
    }
    private static int Run(string[] args)
    {
        Settings defaults = Settings.Parse(File.ReadAllText(args[2]));
        if (defaults.Bindings.Length != 0) throw new Exception("Default settings changed");
        Settings sample = Settings.Parse(File.ReadAllText(args[3]));
        if (sample.Bindings.Length != 4) throw new Exception("Expected two door leaves on body and glass");
        foreach (BindingRule rule in sample.Bindings)
        {
            DoorSpec result;
            if (!DoorProtocol.TryResolve(rule.Mesh, rule.Bone, -1.5f, sample, out result) || result.Source != "JSON")
                throw new Exception("Example binding not resolved: " + rule.Mesh + "/" + rule.Bone);
        }
        Console.WriteLine("PASS actual runtime default JSON and four bundled example mappings parsed by delivery DLL");
        return 0;
    }
}
