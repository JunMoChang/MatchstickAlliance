using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GamePlay.EditorTools
{
    /// <summary>
    /// 将 AnimatorController 中指向"副本 .anim"（或断
    /// 裂 / Sprite 引用）的状态运动，
    /// 按状态名重新绑定到 .aseprite 导入生成的 AnimationClip。
    /// 背景：动画事件迁移到代码/配置驱动后不再需要复制动画剪辑，
    /// </summary>
    public static class AsepriteAnimationRebinder
    {
        private const string AnimationsRoot = "Assets/Characters/Roles/Animations";

        [MenuItem("Tools/角色动画/重新绑定 Aseprite 动画引用")]
        public static void RebindAll()
        {
            int rebound = 0, warn = 0;
            foreach (string asepritePath in Directory.GetFiles(AnimationsRoot, "*.aseprite", SearchOption.AllDirectories))
            {
                string dir = Path.GetDirectoryName(asepritePath);
                foreach (string controllerPath in Directory.GetFiles(dir, "*.controller", SearchOption.TopDirectoryOnly))
                {
                    RebindController(controllerPath, asepritePath, ref rebound, ref warn);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[AsepriteRebind] 完成：替换 {rebound} 处引用，{warn} 处警告（见 Console）");
        }

        [MenuItem("Tools/角色动画/删除已解绑的副本动画")]
        public static void DeleteOrphanCopies()
        {
            List<string> stillReferenced = new();
            List<string> toDelete = new();
            foreach (string animPath in Directory.GetFiles(AnimationsRoot, "*.anim", SearchOption.AllDirectories))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
                if (clip == null) continue;
                // 副本特征：引用的是 aseprite 导入的 sprite 帧（m_PPtrCurves 的 guid 指向 .aseprite 文件）
                if (!IsCopiedClip(clip)) continue;
                if (IsReferencedByAnyController(clip)) stillReferenced.Add(animPath);
                else toDelete.Add(animPath);
            }

            if (toDelete.Count == 0)
            {
                Debug.Log("[AsepriteRebind] 没有可删除的副本动画（可能已全部删除，或全部仍被引用）");
                if (stillReferenced.Count > 0)
                {
                    foreach (string p in stillReferenced)
                    {
                        Debug.LogWarning($"[AsepriteRebind] 仍被 controller 引用，跳过：{p}");
                    }
                }
                return;
            }

            if (!EditorUtility.DisplayDialog("删除副本动画",
                    $"将删除 {toDelete.Count} 个未被引用的副本动画：\n{string.Join("\n", toDelete)}\n\n确认删除？",
                    "删除", "取消"))
            {
                return;
            }

            foreach (string p in toDelete)
            {
                AssetDatabase.DeleteAsset(p);
                Debug.Log($"[AsepriteRebind] 已删除：{p}");
            }
            AssetDatabase.SaveAssets();
            foreach (string p in stillReferenced)
            {
                Debug.LogWarning($"[AsepriteRebind] 仍被 controller 引用，跳过：{p}");
            }
        }

        private static void RebindController(string controllerPath, string asepritePath, ref int rebound, ref int warn)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) return;

            // aseprite 导入生成的 clip：按名索引
            Dictionary<string, AnimationClip> asepriteClips = new();
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(asepritePath))
            {
                if (asset is AnimationClip clip) asepriteClips[clip.name] = clip;
            }
            if (asepriteClips.Count == 0)
            {
                Debug.LogWarning($"[AsepriteRebind] {asepritePath} 未生成 AnimationClip（检查 aseprite 是否含动画 tag）");
                return;
            }
            Debug.Log($"[AsepriteRebind] {Path.GetFileName(asepritePath)} 可用 clip：{string.Join(", ", asepriteClips.Keys)}");

            int reboundBefore = rebound;
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                WalkStateMachine(layer.stateMachine, controller, asepriteClips, asepritePath, ref rebound, ref warn);
            }
            if (rebound > reboundBefore)
            {
                EditorUtility.SetDirty(controller);
            }
        }

        private static void WalkStateMachine(AnimatorStateMachine machine, AnimatorController controller,
            Dictionary<string, AnimationClip> clips, string asepritePath, ref int rebound, ref int warn)
        {
            foreach (ChildAnimatorState child in machine.states)
            {
                RebindStateMotion(child.state, controller, clips, asepritePath, ref rebound, ref warn);
            }
            foreach (ChildAnimatorStateMachine child in machine.stateMachines)
            {
                WalkStateMachine(child.stateMachine, controller, clips, asepritePath, ref rebound, ref warn);
            }
        }

        private static void RebindStateMotion(AnimatorState state, AnimatorController controller,
            Dictionary<string, AnimationClip> clips, string asepritePath, ref int rebound, ref int warn)
        {
            Motion motion = state.motion;
            if (motion is BlendTree tree)
            {
                RebindBlendTree(tree, controller, clips, asepritePath, ref rebound, ref warn);
                return;
            }

            // 已是 aseprite 导入 clip 则跳过；null 表示断裂/Sprite 等无效引用，同样按名尝试修复
            if (motion is AnimationClip clip && AssetDatabase.GetAssetPath(clip) == asepritePath) return;
            // 优先按当前引用 clip 名匹配（状态名与 aseprite tag 名可能不同，如 Skill_1 状态引用 FirstSkill.anim），
            // 其次按状态名匹配（断裂/Sprite 引用拿不到 clip 名时）
            string matchName = motion is AnimationClip current && !string.IsNullOrEmpty(current.name) ? current.name : state.name;
            if (!clips.TryGetValue(matchName, out AnimationClip target) && !clips.TryGetValue(state.name, out target))
            {
                warn++;
                Debug.LogWarning($"[AsepriteRebind] 状态「{state.name}」的引用非 aseprite（当前 clip：{matchName}），但 {Path.GetFileName(asepritePath)} 无同名 clip，已跳过（请检查 aseprite tag 名）");
                return;
            }
            Undo.RecordObject(controller, "Rebind motion to aseprite clip");
            state.motion = target;
            rebound++;
            Debug.Log($"[AsepriteRebind] 状态「{state.name}」→ {Path.GetFileName(asepritePath)}:{target.name}");
        }

        private static void RebindBlendTree(BlendTree tree, AnimatorController controller,
            Dictionary<string, AnimationClip> clips, string asepritePath, ref int rebound, ref int warn)
        {
            ChildMotion[] children = tree.children;
            bool changed = false;
            for (int i = 0; i < children.Length; i++)
            {
                Motion motion = children[i].motion;
                if (motion is BlendTree subTree)
                {
                    RebindBlendTree(subTree, controller, clips, asepritePath, ref rebound, ref warn);
                    continue;
                }
                if (motion is AnimationClip clip && AssetDatabase.GetAssetPath(clip) == asepritePath) continue;

                string name = motion != null ? motion.name : null;
                if (string.IsNullOrEmpty(name) || !clips.TryGetValue(name, out AnimationClip target))
                {
                    warn++;
                    Debug.LogWarning($"[AsepriteRebind] 混合树「{tree.name}」第 {i} 个子运动（{name ?? "null"}）无法自动匹配，" +
                                     $"请在 Inspector 手动指定 aseprite 的 Idle/Move clip（可用：{string.Join(", ", clips.Keys)}）");
                    continue;
                }
                Undo.RecordObject(controller, "Rebind blend child to aseprite clip");
                children[i].motion = target;
                changed = true;
                rebound++;
                Debug.Log($"[AsepriteRebind] 混合树「{tree.name}」子运动 → {target.name}");
            }
            if (changed)
            {
                tree.children = children;
                EditorUtility.SetDirty(tree);
            }
        }

        /// <summary> 副本动画特征：纯 Sprite 帧引用曲线（m_PPtrCurves），来源是 aseprite 导入的 sprite</summary>
        private static bool IsCopiedClip(AnimationClip clip)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            bool hasSpriteCurve = false;
            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                {
                    hasSpriteCurve = true;
                    break;
                }
            }
            return hasSpriteCurve;
        }

        private static bool IsReferencedByAnyController(AnimationClip clip)
        {
            foreach (string controllerPath in Directory.GetFiles(AnimationsRoot, "*.controller", SearchOption.AllDirectories))
            {
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                if (controller == null) continue;
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    if (MachineReferencesClip(layer.stateMachine, clip)) return true;
                }
            }
            return false;
        }

        private static bool MachineReferencesClip(AnimatorStateMachine machine, AnimationClip clip)
        {
            foreach (ChildAnimatorState child in machine.states)
            {
                if (ReferencesClip(child.state.motion, clip)) return true;
            }
            foreach (ChildAnimatorStateMachine child in machine.stateMachines)
            {
                if (MachineReferencesClip(child.stateMachine, clip)) return true;
            }
            return false;
        }

        private static bool ReferencesClip(Motion motion, AnimationClip clip)
        {
            switch (motion)
            {
                case AnimationClip c when c == clip:
                    return true;
                case BlendTree tree:
                {
                    foreach (ChildMotion child in tree.children)
                    {
                        if (ReferencesClip(child.motion, clip)) return true;
                    }
                    break;
                }
            }
            return false;
        }
    }
}
