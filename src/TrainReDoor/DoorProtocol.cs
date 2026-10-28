using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TrainReDoor
{
    // EN: A mapping targets one exact RenderPrefab/bone pair, with translation in parent-space metres.
    // JA: マッピングは RenderPrefab とボーン名の組を指定し、移動量は親座標系のメートル単位です。
    [DataContract]
    public sealed class BindingRule
    {
        [DataMember(Name = "mesh", IsRequired = true)] public string Mesh;
        [DataMember(Name = "bone", IsRequired = true)] public string Bone;
        [DataMember(Name = "side", IsRequired = true)] public string Side;
        [DataMember(Name = "deltaMeters", IsRequired = true)] public float[] DeltaMeters;
    }

    [DataContract]
    public sealed class Settings
    {
        [DataMember(Name = "formatVersion", IsRequired = true)] public int FormatVersion;
        [DataMember(Name = "openSeconds", IsRequired = true)] public float OpenSeconds;
        [DataMember(Name = "closeSeconds", IsRequired = true)] public float CloseSeconds;
        [DataMember(Name = "bindings")] public BindingRule[] Bindings;
        [DataMember(Name = "excludedMeshes")] public string[] ExcludedMeshes;

        // EN: Automatic naming needs no vehicle list or per-asset registration.
        // JA: 自動命名を使う場合、車両リストやアセットごとの登録は不要です。
        public static Settings Defaults()
        {
            return new Settings { FormatVersion = 1, OpenSeconds = 1f, CloseSeconds = 1f,
                Bindings = new BindingRule[0], ExcludedMeshes = new string[0] };
        }
        public static Settings Parse(string json)
        {
            Settings value;
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                value = (Settings)new DataContractJsonSerializer(typeof(Settings)).ReadObject(stream);
            if (value == null) throw new FormatException("Settings must be an object.");
            value.Validate();
            return value;
        }
        public void Validate()
        {
            if (FormatVersion != 1) throw new FormatException("Unsupported formatVersion; expected 1.");
            if (!DoorProtocol.Finite(OpenSeconds) || OpenSeconds < .05f || OpenSeconds > 60f ||
                !DoorProtocol.Finite(CloseSeconds) || CloseSeconds < .05f || CloseSeconds > 60f)
                throw new FormatException("openSeconds/closeSeconds must be finite, from 0.05 to 60.");
            if (Bindings == null) Bindings = new BindingRule[0];
            if (ExcludedMeshes == null) ExcludedMeshes = new string[0];
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            foreach (BindingRule rule in Bindings)
            {
                if (rule == null || String.IsNullOrWhiteSpace(rule.Mesh) || String.IsNullOrWhiteSpace(rule.Bone))
                    throw new FormatException("Each binding needs an exact mesh name and bone name.");
                if (rule.Side != "Left" && rule.Side != "Right" && rule.Side != "Auto")
                    throw new FormatException("side must be Left, Right or Auto.");
                if (rule.DeltaMeters == null || rule.DeltaMeters.Length != 3)
                    throw new FormatException("deltaMeters needs exactly three parent-space components.");
                double length = 0;
                foreach (float v in rule.DeltaMeters)
                {
                    if (!DoorProtocol.Finite(v)) throw new FormatException("deltaMeters must be finite.");
                    length += (double)v * v;
                }
                if (length < .000001 || length > 100.0)
                    throw new FormatException("Door travel must be from 0.001 to 10 metres.");
                // EN: Length-prefix the mesh name to avoid ambiguous composite keys.
                // JA: メッシュ名の長さを付け、区切り文字を含む名前でもキーの衝突を防ぎます。
                string key = rule.Mesh.Length.ToString(CultureInfo.InvariantCulture) + ":" + rule.Mesh + rule.Bone;
                if (!names.Add(key)) throw new FormatException("Duplicate mesh/bone binding: " + rule.Mesh + "/" + rule.Bone);
            }
            foreach (string mesh in ExcludedMeshes)
                if (String.IsNullOrWhiteSpace(mesh)) throw new FormatException("excludedMeshes must contain non-empty exact names.");
        }
    }

    public struct DoorSpec
    {
        public bool Left;
        public float X, Y, Z;
        public string Source;
    }

    public static class DoorProtocol
    {
        public static string ResolveSettingsPath(string executableAssetPath, string assemblyLocation, string userDataPath)
        {
            // EN: Byte-loaded mods may have no Assembly.Location; prefer the game's asset path.
            // JA: バイト列からロードされた場合は Assembly.Location が空になるため、ゲームのパスを優先します。
            foreach (string candidate in new string[] { executableAssetPath, assemblyLocation })
            {
                if (String.IsNullOrWhiteSpace(candidate)) continue;
                string folder = Path.GetDirectoryName(candidate);
                if (!String.IsNullOrEmpty(folder)) return Path.Combine(folder, "TrainReDoor.json");
            }
            if (String.IsNullOrWhiteSpace(userDataPath)) throw new ArgumentException("Cannot resolve the mod settings directory.");
            return Path.Combine(userDataPath, "Mods", "TrainReDoor", "TrainReDoor.json");
        }
        public static bool Finite(float v) { return !Single.IsNaN(v) && !Single.IsInfinity(v); }
        public static bool IsExcluded(string mesh, Settings settings)
        {
            if (settings.ExcludedMeshes == null) return false;
            foreach (string value in settings.ExcludedMeshes)
                if (String.Equals(mesh, value, StringComparison.Ordinal)) return true;
            return false;
        }
        public static bool TryResolve(string mesh, string bone, float nativeObjectX, Settings settings, out DoorSpec result)
        {
            result = new DoorSpec();
            if (String.IsNullOrEmpty(mesh) || String.IsNullOrEmpty(bone) || !Finite(nativeObjectX) || IsExcluded(mesh, settings)) return false;
            // EN: Exact mappings override naming rules and allow explicit centre-line side selection.
            // JA: 明示的マッピングを命名規則より優先し、中心線上のボーンも左右を指定できます。
            if (settings.Bindings != null)
                foreach (BindingRule rule in settings.Bindings)
                    if (String.Equals(mesh, rule.Mesh, StringComparison.Ordinal) && String.Equals(bone, rule.Bone, StringComparison.Ordinal))
                    {
                        if (rule.Side == "Auto" && Math.Abs(nativeObjectX) < .0001f) return false;
                        result = new DoorSpec { Left = rule.Side == "Left" || (rule.Side == "Auto" && nativeObjectX < 0),
                            X = rule.DeltaMeters[0], Y = rule.DeltaMeters[1], Z = rule.DeltaMeters[2], Source = "JSON" };
                        return true;
                    }
            bool standard = bone.StartsWith("ReDoor_", StringComparison.Ordinal) || bone.StartsWith("TrainReDoor_", StringComparison.Ordinal);
            if (!standard || Math.Abs(nativeObjectX) < .0001f) return false;
            string[] fields = bone.Split(new char[] { '_' });
            if (fields.Length != 5 || (fields[1] != "L" && fields[1] != "R")) return false;
            int group;
            if (!Int32.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out group) || group < 1 ||
                (fields[3] != "0" && fields[3] != "1")) return false;
            float travel;
            if (!Single.TryParse(fields[4], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out travel) ||
                !Finite(travel) || travel < 1 || travel > 10000) return false;
            // EN: Native X selects the physical side after exporter axis conversion.
            // JA: エクスポート時の軸変換後も、ゲーム座標の X 位置で実際の左右を判断します。
            result = new DoorSpec { Left = nativeObjectX < 0,
                Z = (fields[3] == "0" ? 1f : -1f) * travel * .001f,
                Source = "ReDoor protocol" };
            return true;
        }
        // EN: Durations are global simulated seconds; keep each side's opening continuous.
        // JA: 開閉時間は全車共通のシミュレーション秒で、左右の開度を連続的に更新します。
        public static float Advance(float current, bool open, float dt, Settings settings)
        {
            if (!Finite(current)) current = 0;
            current = Math.Max(0f, Math.Min(1f, current));
            if (!Finite(dt) || dt <= 0) return current;
            return open ? Math.Min(1f, current + dt / settings.OpenSeconds)
                : Math.Max(0f, current - dt / settings.CloseSeconds);
        }
        public static float Ease(float fraction)
        {
            fraction = Math.Max(0f, Math.Min(1f, fraction));
            return fraction * fraction * (3f - 2f * fraction);
        }
    }
}
