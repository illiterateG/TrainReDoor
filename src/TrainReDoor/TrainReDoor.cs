using System;
using System.Collections.Generic;
using System.IO;
using Game;
using Game.Modding;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Vehicles;
using Game.SceneFlow;
using Colossal.IO.AssetDatabase;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[assembly: System.Reflection.AssemblyTitle("Train ReDoor")]
[assembly: System.Reflection.AssemblyDescription("Shared opt-in sliding-door controller for train assets")]
[assembly: System.Reflection.AssemblyVersion("1.2.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.2.0.0")]
namespace TrainReDoor
{
    public sealed class Mod : IMod
    {
        private TrainDoorSystem system;
        private static bool loaded;
        // EN: Register one shared controller; assets opt in through bones or exact mappings.
        // JA: 共通コントローラーを登録し、ボーン名または明示的マッピングで対象を選択します。
        public void OnLoad(UpdateSystem updateSystem)
        {
            if (loaded) { Debug.LogWarning("[Train ReDoor] A controller is already registered; duplicate load ignored."); return; }
            Settings settings = Settings.Defaults();
            string path = "<unresolved>";
            try
            {
                string executablePath = null;
                ExecutableAsset asset;
                if (GameManager.instance != null && GameManager.instance.modManager != null &&
                    GameManager.instance.modManager.TryGetExecutableAsset(this, out asset)) executablePath = asset.path;
                path = DoorProtocol.ResolveSettingsPath(executablePath, typeof(Mod).Assembly.Location, Application.persistentDataPath);
                if (File.Exists(path)) settings = Settings.Parse(File.ReadAllText(path));
            }
            catch (Exception error)
            { Debug.LogError("[Train ReDoor] Invalid settings: " + error.Message + ". Using automatic protocol defaults; custom JSON bindings were not applied."); }
            TypeManager.GetSystemTypeIndex(typeof(TrainDoorSystem));
            system = (TrainDoorSystem)World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged(typeof(TrainDoorSystem));
            system.Configure(settings);
            // EN: Write poses before the game's animation preparation consumes the bone buffer.
            // JA: ゲームのアニメーション準備がボーンバッファを読む前に姿勢を書き込みます。
            updateSystem.UpdateBefore<TrainDoorSystem, AnimatedSystem.Prepare>(SystemUpdatePhase.Rendering);
            loaded = true;
            Debug.Log("[Train ReDoor 1.2.0] Loaded. All Train assets can opt in with ReDoor bones or exact JSON bindings; no vehicle-name whitelist. Config: " + path);
        }
        public void OnDispose()
        {
            if (system == null) return;
            try
            {
                if (system.World != null && system.World.IsCreated)
                {
                    system.RestoreClosedPoses();
                    system.Enabled = false;
                }
            }
            catch (Exception error) { Debug.LogWarning("[Train ReDoor] Could not restore door poses during world shutdown: " + error.Message); }
            loaded = false;
        }
    }

    public sealed partial class TrainDoorSystem : GameSystemBase
    {
        private struct Binding { public int Index, BindIndex; public DoorSpec Spec; }
        private sealed class CachedBinding { public Binding[] Values; public long RefreshAt; }
        private struct Opening { public float Left, Right; }
        private readonly Dictionary<Entity, CachedBinding> bindings = new Dictionary<Entity, CachedBinding>();
        private readonly Dictionary<Entity, Opening> openings = new Dictionary<Entity, Opening>();
        private readonly List<Entity> expired = new List<Entity>();
        private EntityQuery query;
        private PrefabSystem prefabs;
        private SimulationSystem simulation;
        private Settings settings = Settings.Defaults();
        private double lastTime = -1;
        private long ticks;
        public void Configure(Settings value)
        {
            value.Validate(); settings = value; bindings.Clear(); openings.Clear(); lastTime = -1; ticks = 0; Enabled = true;
        }
        // EN: Eligibility comes from runtime components, never a vehicle brand or asset title.
        // JA: 対象は実行時コンポーネントで判断し、車種名やアセット名では制限しません。
        protected override void OnCreate()
        {
            base.OnCreate();
            prefabs = World.GetOrCreateSystemManaged<PrefabSystem>();
            simulation = World.GetOrCreateSystemManaged<SimulationSystem>();
            query = GetEntityQuery(ComponentType.ReadOnly<Train>(), ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<InterpolatedTransform>(), ComponentType.ReadWrite<Bone>(), ComponentType.ReadWrite<Skeleton>());
            RequireForUpdate(query);
        }
        private Binding[] GetBindings(Entity mesh)
        {
            CachedBinding cached;
            if (bindings.TryGetValue(mesh, out cached) && ticks < cached.RefreshAt) return cached.Values;
            RenderPrefab prefab;
            ProceduralAnimationProperties animation;
            List<Binding> found = new List<Binding>();
            string meshName = "";
            if (prefabs.TryGetPrefab<RenderPrefab>(mesh, out prefab) &&
                !DoorProtocol.IsExcluded(prefab.name, settings) &&
                prefab.TryGet<ProceduralAnimationProperties>(out animation) && animation.m_Bones != null &&
                EntityManager.HasBuffer<ProceduralBone>(mesh))
            {
                meshName = prefab.name;
                DynamicBuffer<ProceduralBone> source = EntityManager.GetBuffer<ProceduralBone>(mesh, true);
                for (int i = 0; i < source.Length; ++i)
                {
                    ProceduralBone bone = source[i];
                    if (bone.m_BindIndex < 0 || bone.m_BindIndex >= animation.m_Bones.Length || bone.m_Type != BoneType.None ||
                        !math.all(math.isfinite(bone.m_Position)) || !math.all(math.isfinite(bone.m_ObjectPosition))) continue;
                    DoorSpec spec;
                    if (!DoorProtocol.TryResolve(meshName, animation.m_Bones[bone.m_BindIndex].name, bone.m_ObjectPosition.x, settings, out spec)) continue;
                    found.Add(new Binding { Index = i, BindIndex = bone.m_BindIndex, Spec = spec });
                }
            }
            Binding[] result = found.ToArray();
            // EN: Cache misses too; retry periodically after editor reimports or buffer regeneration.
            // JA: 未一致もキャッシュし、再インポートやバッファ再生成に備えて定期的に再確認します。
            bindings[mesh] = new CachedBinding { Values = result, RefreshAt = ticks + (result.Length > 0 ? 300 : 150) };
            if (result.Length > 0 && (cached == null || cached.Values.Length != result.Length))
                Debug.Log("[Train ReDoor] Bound " + result.Length + " moving door bones on " + meshName);
            return result;
        }
        // EN: Restore the closed bind pose when the controller is disposed.
        // JA: コントローラー終了時は閉扉状態のバインド姿勢へ戻します。
        public void RestoreClosedPoses()
        {
            EntityManager.CompleteAllTrackedJobs();
            using (NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp))
                for (int i = 0; i < entities.Length; ++i) Apply(entities[i], new Opening(), true);
            bindings.Clear(); openings.Clear();
        }
        private bool Apply(Entity entity, Opening opening, bool restore)
        {
            PrefabRef reference = EntityManager.GetComponentData<PrefabRef>(entity);
            if (!EntityManager.HasBuffer<SubMesh>(reference.m_Prefab)) return false;
            DynamicBuffer<SubMesh> meshes = EntityManager.GetBuffer<SubMesh>(reference.m_Prefab, true);
            DynamicBuffer<Skeleton> skeletons = EntityManager.GetBuffer<Skeleton>(entity, false);
            DynamicBuffer<Bone> bones = EntityManager.GetBuffer<Bone>(entity, false);
            bool controlled = false;
            for (int j = 0; j < math.min(meshes.Length, skeletons.Length); ++j)
            {
                Entity mesh = meshes[j].m_SubMesh;
                Binding[] map = GetBindings(mesh);
                if (map.Length == 0 || !EntityManager.HasBuffer<ProceduralBone>(mesh)) continue;
                Skeleton skeleton = skeletons[j];
                if (skeleton.m_BufferAllocation.Empty) continue;
                DynamicBuffer<ProceduralBone> rest = EntityManager.GetBuffer<ProceduralBone>(mesh, true);
                bool changed = false;
                for (int k = 0; k < map.Length; ++k)
                {
                    Binding binding = map[k];
                    if (binding.Index < 0 || binding.Index >= rest.Length || rest[binding.Index].m_BindIndex != binding.BindIndex ||
                        rest[binding.Index].m_Type != BoneType.None) continue;
                    int index = skeleton.m_BoneOffset + binding.Index;
                    if (index < 0 || index >= bones.Length) continue;
                    controlled = true;
                    float eased = restore ? 0f : DoorProtocol.Ease(binding.Spec.Left ? opening.Left : opening.Right);
                    // EN: Offset from the bind pose each frame; never accumulate translation drift.
                    // JA: 毎フレームの基準をバインド姿勢に固定し、移動量の累積ずれを防ぎます。
                    float3 position = rest[binding.Index].m_Position + new float3(binding.Spec.X, binding.Spec.Y, binding.Spec.Z) * eased;
                    Bone bone = bones[index];
                    if (math.lengthsq(bone.m_Position - position) <= 1e-12f) continue;
                    bone.m_Position = position; bones[index] = bone; changed = true;
                }
                if (changed) { skeleton.m_CurrentUpdated = true; skeletons[j] = skeleton; }
            }
            return controlled;
        }
        protected override void OnUpdate()
        {
            EntityManager.CompleteAllTrackedJobs();
            // EN: Simulation time preserves the current opening while the game is paused.
            // JA: シミュレーション時刻を使い、一時停止中は開度を保持します。
            double now = ((double)simulation.frameIndex + simulation.frameTime) / 60.0;
            float dt = lastTime < 0 || now < lastTime ? 0f : (float)Math.Min(now - lastTime, .25);
            lastTime = now;
            using (NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp))
                for (int i = 0; i < entities.Length; ++i)
                {
                    Entity entity = entities[i];
                    InterpolatedTransform transform = EntityManager.GetComponentData<InterpolatedTransform>(entity);
                    // EN: Each platform side follows its own boarding signal.
                    // JA: 左右それぞれの乗降信号に従って独立して開閉します。
                    bool left = (transform.m_Flags & Game.Objects.TransformFlags.BoardingLeft) != 0;
                    bool right = (transform.m_Flags & Game.Objects.TransformFlags.BoardingRight) != 0;
                    Opening value;
                    if (!openings.TryGetValue(entity, out value)) value = new Opening();
                    value.Left = DoorProtocol.Advance(value.Left, left, dt, settings);
                    value.Right = DoorProtocol.Advance(value.Right, right, dt, settings);
                    if (Apply(entity, value, false)) openings[entity] = value;
                }
            ++ticks;
            if (ticks % 600 != 0) return;
            expired.Clear();
            foreach (Entity entity in openings.Keys) if (!EntityManager.Exists(entity)) expired.Add(entity);
            foreach (Entity entity in expired) openings.Remove(entity);
            expired.Clear();
            foreach (Entity entity in bindings.Keys) if (!EntityManager.Exists(entity)) expired.Add(entity);
            foreach (Entity entity in expired) bindings.Remove(entity);
        }
    }
}
