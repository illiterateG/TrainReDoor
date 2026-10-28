using System;
using System.IO;
using System.Reflection;
using TrainReDoor;

// EN: Exercise the public protocol and configuration against the actual delivery assembly.
// JA: 実際の配布アセンブリに対して公共規則と設定を検証します。
internal static class ProtocolTests
{
    private static int count;
    private static void Check(bool pass, string name)
    { if (!pass) throw new Exception(name); ++count; Console.WriteLine("PASS " + name); }
    private static bool Near(float a, float b) { return Math.Abs(a - b) < .00001f; }
    private static bool Resolve(string mesh, string bone, float x, Settings cfg, out DoorSpec spec)
    { return DoorProtocol.TryResolve(mesh, bone, x, cfg, out spec); }
    private static void RejectJson(string text, string name)
    {
        bool rejected = false;
        try { Settings.Parse(text); } catch { rejected = true; }
        Check(rejected, name);
    }
    private const string JsonStart = "{\"formatVersion\":1,\"openSeconds\":1,\"closeSeconds\":2,";
    public static int Main(string[] args)
    {
        string modPath = Path.GetFullPath(args[0]); string managed = args[1];
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs ev)
        {
            string file = new AssemblyName(ev.Name).Name + ".dll";
            string local = Path.Combine(Path.GetDirectoryName(modPath), file);
            string game = Path.Combine(managed, file);
            return File.Exists(local) ? Assembly.LoadFrom(local) : File.Exists(game) ? Assembly.LoadFrom(game) : null;
        };
        try { return Run(modPath); }
        catch (Exception error)
        {
            Console.WriteLine("FAIL_TYPE=" + error.GetType().FullName);
            Console.WriteLine("FAIL_MESSAGE=" + error.Message);
            return 1;
        }
    }
    private static int Run(string modPath)
    {
        Settings cfg = Settings.Defaults(); DoorSpec spec;
        Check(DoorProtocol.ResolveSettingsPath(@"G:\User\Mods\TrainReDoor\TrainReDoor.dll", "", @"G:\User") == @"G:\User\Mods\TrainReDoor\TrainReDoor.json", "Memory-loaded mod resolves JSON from game ExecutableAsset path");
        Check(DoorProtocol.ResolveSettingsPath(@"G:\AssetCache\TrainReDoor.dll", @"D:\Other\TrainReDoor.dll", @"G:\User") == @"G:\AssetCache\TrainReDoor.json", "Game-provided executable path takes priority over assembly location");
        Check(DoorProtocol.ResolveSettingsPath(null, @"D:\Local\TrainReDoor.dll", @"G:\User") == @"D:\Local\TrainReDoor.json", "File-loaded assembly configuration fallback");
        Check(DoorProtocol.ResolveSettingsPath(null, "", @"G:\User") == @"G:\User\Mods\TrainReDoor\TrainReDoor.json", "Empty Assembly.Location uses user Mods fallback");
        bool noPath=false;try{DoorProtocol.ResolveSettingsPath(null,"",null);}catch(ArgumentException){noPath=true;}
        Check(noPath, "Unresolvable configuration path fails clearly without invalid Path.Combine");
        foreach (string mesh in new[] { "RegionalTrain", "ExpressBody", "ExpressGlass", "UrbanTrain", "WideTrain_Glass", "汎用列車" })
            Check(Resolve(mesh, "ReDoor_L_01_0_850", -1.8f, cfg, out spec) && spec.Left && Near(spec.Z, .85f), "No mesh whitelist: " + mesh);
        Check(Resolve("Other", "TrainReDoor_R_15_1_1250", 2.2f, cfg, out spec) && !spec.Left && Near(spec.Z, -1.25f), "Group >4, wide body, alternate prefix and independent stroke");
        Check(Resolve("Any", "ReDoor_L_01_0_640.5", -1.4f, cfg, out spec) && Near(spec.Z, .6405f), "Invariant fractional-millimetre stroke");
        Check(Resolve("Any", "ReDoor_L_01_0_720", 1.28f, cfg, out spec) && !spec.Left, "Native X controls platform side when exporter reverses authoring L/R");
        foreach (string bone in new[] { "BodyRoot", "BogieFront", "Pantograph0", "WalkwayFront", "Door_L_1_0", "ReDoor_L_0_0_720", "ReDoor_L_-1_0_720", "ReDoor_X_1_0_720", "ReDoor_L_1_2_720", "ReDoor_L_1_0_0", "ReDoor_L_1_0_10001", "ReDoor_L_1_0_NaN", "ReDoor_L_1_0_Infinity", "ReDoor_L_1_0_720_extra", "ReDoor_L_2147483648_0_720" })
            Check(!Resolve("Any", bone, -1.2f, cfg, out spec), "Ignore unrelated/invalid bone: " + bone);
        Check(!Resolve("Any", "ReDoor_L_1_0_720", 0, cfg, out spec), "Ambiguous centre-line side rejected for automatic protocol");
        Check(!Resolve("Any", "ReDoor_L_1_0_720", Single.NaN, cfg, out spec), "Nonfinite native location rejected");
        cfg = Settings.Parse(JsonStart + "\"excludedMeshes\":[\"Disabled\"],\"bindings\":[{\"mesh\":\"CustomTrain\",\"bone\":\"PassengerDoor_A\",\"side\":\"Left\",\"deltaMeters\":[0.04,0,0.93]}]}");
        Check(Resolve("CustomTrain", "PassengerDoor_A", 0, cfg, out spec) && spec.Left && Near(spec.X, .04f) && Near(spec.Z, .93f), "JSON arbitrary bone, explicit side at centre and non-longitudinal delta");
        Check(!Resolve("OtherCustomTrain", "PassengerDoor_A", -1.3f, cfg, out spec), "JSON exact mesh scope does not affect a different train");
        Check(!Resolve("CustomTrain", "PassengerDoor_a", -1.3f, cfg, out spec), "JSON bone names are exact and case-sensitive");
        Check(!Resolve("Disabled", "ReDoor_L_01_0_720", -1.3f, cfg, out spec), "Excluded mesh overrides automatic protocol");
        Check(Resolve("StillAutomatic", "ReDoor_L_01_0_720", -1.3f, cfg, out spec), "JSON custom bindings preserve automatic discovery for other trains");
        Settings over = Settings.Parse(JsonStart + "\"bindings\":[{\"mesh\":\"CustomTrain\",\"bone\":\"ReDoor_L_01_0_720\",\"side\":\"Right\",\"deltaMeters\":[0,0,-0.44]}]}");
        Check(Resolve("CustomTrain", "ReDoor_L_01_0_720", -1.3f, over, out spec) && !spec.Left && Near(spec.Z, -.44f), "Explicit rule overrides automatic convention");
        Settings auto = Settings.Parse(JsonStart + "\"bindings\":[{\"mesh\":\"Custom\",\"bone\":\"Existing\",\"side\":\"Auto\",\"deltaMeters\":[0,0,0.5]}]}");
        Check(Resolve("Custom", "Existing", -2.1f, auto, out spec) && spec.Left, "JSON Auto side supports a wide body");
        Check(!Resolve("Custom", "Existing", 0, auto, out spec), "JSON Auto rejects ambiguous centre line");
        RejectJson("{}", "Missing required configuration rejected");
        RejectJson("null", "Null configuration rejected");
        RejectJson(JsonStart.Replace("\"formatVersion\":1", "\"formatVersion\":2") + "\"bindings\":[]}", "Unknown format rejected");
        RejectJson(JsonStart.Replace("\"openSeconds\":1", "\"openSeconds\":0") + "\"bindings\":[]}", "Zero opening time rejected");
        RejectJson(JsonStart + "\"bindings\":[{\"mesh\":\"A\",\"bone\":\"B\",\"side\":\"Left\",\"deltaMeters\":[0,0,0]}]}", "Zero displacement rejected");
        RejectJson(JsonStart + "\"bindings\":[{\"mesh\":\"A\",\"bone\":\"B\",\"side\":\"Left\",\"deltaMeters\":[0,0,11]}]}", "Unreasonable displacement rejected");
        RejectJson(JsonStart + "\"bindings\":[{\"mesh\":\"A\",\"bone\":\"B\",\"side\":\"wrong\",\"deltaMeters\":[0,0,1]}]}", "Invalid side rejected");
        RejectJson(JsonStart + "\"bindings\":[{\"mesh\":\"A\",\"bone\":\"B\",\"side\":\"Left\",\"deltaMeters\":[0,1]}]}", "Invalid vector arity rejected");
        string duplicate = "{\"mesh\":\"A\",\"bone\":\"B\",\"side\":\"Left\",\"deltaMeters\":[0,0,1]}";
        RejectJson(JsonStart + "\"bindings\":[" + duplicate + "," + duplicate + "]}", "Duplicate mesh/bone rules rejected");
        Check(Near(DoorProtocol.Advance(.4f, true, 0, cfg), .4f), "Pause preserves opening");
        Check(Near(DoorProtocol.Advance(.4f, true, -1, cfg), .4f), "Clock reset does not move doors backwards");
        Check(Near(DoorProtocol.Advance(.4f, true, Single.NaN, cfg), .4f), "Invalid delta-time does not corrupt door state");
        Check(Near(DoorProtocol.Advance(.95f, true, .2f, cfg), 1), "Open endpoint clamp");
        Check(Near(DoorProtocol.Advance(.05f, false, .2f, cfg), 0), "Close endpoint clamp");
        float left = 0, right = 0;
        for (int i = 0; i < 60; ++i) { left = DoorProtocol.Advance(left, true, 1f / 60, cfg); right = DoorProtocol.Advance(right, false, 1f / 60, cfg); }
        Check(Near(left, 1) && right == 0, "Only selected platform side opens in configured one second");
        for (int i = 0; i < 60; ++i) left = DoorProtocol.Advance(left, false, 1f / 60, cfg);
        Check(Near(left, .5f), "Closing obeys independent two-second duration");
        for (int i = 0; i < 60; ++i) left = DoorProtocol.Advance(left, false, 1f / 60, cfg);
        Check(Near(left, 0), "Closing reaches the rest pose");
        Settings equal = Settings.Defaults();
        Check(Near(DoorProtocol.Advance(DoorProtocol.Advance(.3f, true, .1f, equal), false, .1f, equal), .3f), "Mid-motion reversal is continuous");
        Check(Near(DoorProtocol.Ease(0), 0) && Near(DoorProtocol.Ease(.5f), .5f) && Near(DoorProtocol.Ease(1), 1), "Smoothstep endpoints and midpoint");
        float previous = 0;
        for (int i = 0; i <= 100; ++i) { float eased = DoorProtocol.Ease(i / 100f); if (eased < previous) throw new Exception("Nonmonotonic easing"); previous = eased; }
        Check(true, "Easing is monotonic through the complete door stroke");
        Assembly assembly = Assembly.LoadFrom(modPath);
        Check(assembly.GetName().Name == "TrainReDoor" && assembly.GetName().Version.ToString() == "1.2.0.0", "Actual delivery assembly identity and version");
        bool imod = false;
        foreach (Type iface in assembly.GetType("TrainReDoor.Mod", true).GetInterfaces()) if (iface.FullName == "Game.Modding.IMod") imod = true;
        Check(imod, "Actual delivery implements the installed game's IMod");
        foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
            Check(File.Exists(Path.Combine(Path.GetDirectoryName(modPath), reference.Name + ".dll")) || File.Exists(Path.Combine(Path.GetFullPath(Environment.GetCommandLineArgs()[2]), reference.Name + ".dll")), "Game assembly dependency available: " + reference.Name);
        Console.WriteLine("TOTAL_PASS=" + count);
        Console.WriteLine("Tests used the actual built DLL; no game simulation or runtime boarding test executed.");
        return 0;
    }
}
