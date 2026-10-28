using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using TrainReDoor;

// EN: Synthetic cases exercise body/glass agreement and independent widths, groups and travel.
// JA: 合成ケースで車体とガラスの一致、車幅・門組・行程の違いを検証します。
#pragma warning disable 0649 // EN: Assigned by the serializer. JA: デシリアライザーが設定します。
[DataContract] internal sealed class AssetCase
{
    [DataMember(Name="asset")] public string Asset;
    [DataMember(Name="fixtureKind")] public string FixtureKind;
    [DataMember(Name="bones")] public BoneCase[] Bones;
}
[DataContract] internal sealed class BoneCase
{
    [DataMember(Name="name")] public string Name;
    [DataMember(Name="x")] public float X;
    [DataMember(Name="expectedControlled")] public bool ExpectedControlled;
    [DataMember(Name="expectedLeft")] public bool ExpectedLeft;
    [DataMember(Name="expectedZ")] public float ExpectedZ;
}
#pragma warning restore 0649
internal static class GenericAssetTests
{
    public static int Main(string[] args)
    {
        string library=Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender,ResolveEventArgs ev)
        {
            string path=Path.Combine(Path.GetDirectoryName(library),new AssemblyName(ev.Name).Name+".dll");
            return File.Exists(path)?Assembly.LoadFrom(path):null;
        };
        return Run(args);
    }
    private static int Run(string[] args)
    {
        int controlled=0, cases=0;
        for(int i=1;i<args.Length;++i)
        {
            AssetCase data;
            using(FileStream stream=File.OpenRead(args[i]))
                data=(AssetCase)new DataContractJsonSerializer(typeof(AssetCase)).ReadObject(stream);
            if(data.FixtureKind!="synthetic") throw new Exception("Expected a synthetic fixture");
            foreach(BoneCase bone in data.Bones)
            {
                DoorSpec spec;
                bool match=DoorProtocol.TryResolve(data.Asset,bone.Name,bone.X,Settings.Defaults(),out spec);
                if(match!=bone.ExpectedControlled || match && (spec.Left!=bone.ExpectedLeft || Math.Abs(spec.Z-bone.ExpectedZ)>.00001f))
                    throw new Exception("Synthetic binding mismatch: "+data.Asset+"/"+bone.Name);
                if(match)++controlled;
            }
            ++cases; Console.WriteLine("PASS synthetic asset case: "+data.Asset);
        }
        Console.WriteLine("TOTAL_SYNTHETIC_CASES="+cases);
        Console.WriteLine("TOTAL_SYNTHETIC_BINDINGS="+controlled);
        Console.WriteLine("Synthetic inputs only; no imported model or gameplay validation.");
        return 0;
    }
}
