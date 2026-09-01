using System;
using GamePlay.Role.RoleData.BaseData;
using UnityEditor;
using UnityEngine;

namespace GamePlay.EditorTools
{
    /// <summary>
    /// Play 模式调参保存器：退出 Play 的捕获运行时调整的动作配置
    /// （motionCommands / hitBoxCommands），回到编辑模式后写回磁盘资产。
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeTuningSaver
    {
        /// <summary> 需要保存运行时调参数据的资产路径（新角色在此添加）</summary>
        private static readonly string[] SavePaths =
        {
            "Assets/Scripts/GamePlay/Role/RoleData/BaseData/Sscriptable/猴子.asset"
        };

        /// <summary> 调参字段快照（纯数据，不持有运行时 SO 引用）</summary>
        [Serializable]
        private class TuningSnapshot
        {
            public RoleBaseData.MotionCommand[] motionCommands;
            public RoleBaseData.HitBoxCommand[] hitBoxCommands;
        }

        private static TuningSnapshot snapshot;
        private static string snapshotPath;

        static PlayModeTuningSaver()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary> Play 模式下按 F5 重载所有角色的动作配置</summary>
        [MenuItem("Tools/角色动画/重载动作配置 _F5")]
        private static void ReloadAllActionConfigs()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[PlayModeTuningSaver] 仅 Play 模式下可重载动作配置");
                return;
            }

            var contexts = UnityEngine.Object.FindObjectsByType<GamePlay.Role.RoleData.RoleContext>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var context in contexts)
            {
                context.ReloadActionConfig();
            }
            Debug.Log($"[PlayModeTuningSaver] 已重载 {contexts.Length} 个 RoleContext 的动作配置");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    Capture();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    Apply();
                    break;
            }
        }

        /// <summary> 运行时 SO 即将销毁，此刻把调参字段的数组引用摘出来（数组是纯托管对象，销毁后依然有效）</summary>
        private static void Capture()
        {
            snapshot = null;
            snapshotPath = null;

            foreach (string path in SavePaths)
            {
                MonkeyData live = AssetDatabase.LoadAssetAtPath<MonkeyData>(path);
                if (live == null) continue;

                snapshot = new TuningSnapshot
                {
                    motionCommands = live.motionCommands,
                    hitBoxCommands = live.hitBoxCommands
                };
                snapshotPath = path;
                break;
            }
        }

        /// <summary> 磁盘资产已回滚为进 Play 前的状态，把调参结果写回</summary>
        private static void Apply()
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshotPath)) return;

            MonkeyData diskAsset = AssetDatabase.LoadAssetAtPath<MonkeyData>(snapshotPath);
            if (diskAsset == null)
            {
                Debug.LogWarning($"[PlayModeTuningSaver] 找不到资产：{snapshotPath}");
                snapshot = null;
                snapshotPath = null;
                return;
            }

            diskAsset.motionCommands = snapshot.motionCommands;
            diskAsset.hitBoxCommands = snapshot.hitBoxCommands;
            EditorUtility.SetDirty(diskAsset);
            Debug.Log($"[PlayModeTuningSaver] 已保存运行时调参数据：{snapshotPath}");

            snapshot = null;
            snapshotPath = null;
            AssetDatabase.SaveAssets();
        }
    }
}
