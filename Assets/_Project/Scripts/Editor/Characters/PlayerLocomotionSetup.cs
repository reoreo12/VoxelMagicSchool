// 텃밭 등 3D 맵 플레이어 이동·애니메이션(복도와 같은 방식)과 동작(낚시·물 주기) 연결 에디터 도구: 공용 이동 설정(MoveConfig, 없을 때만)을 만들고,
// 지금 열린 씬의 플레이어에 연결한다 (Model에 CharacterAnimatorView·AvatarAnimationSpeed, 옛 PlayerAvatarAnimator는 끄기만). 씬 저장은 사용자가.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;

namespace VoxelMagic.EditorTools
{
    public static class PlayerLocomotionSetup
    {
        /// <summary>복도를 뺀 3D 맵(텃밭·연못·뒷산 등) 플레이어가 함께 쓰는 이동 설정.</summary>
        public const string MoveConfigPath = "Assets/_Project/Data/Config/MoveConfig.asset";
        private const string AnimationConfigPath = "Assets/_Project/Data/SideScroll/CharacterAnimationConfig.asset";

        [MenuItem("VoxelMagic/캐릭터/텃밭 플레이어 이동·애니 통일")]
        public static void ApplyToOpenScene()
        {
            var mover = Object.FindFirstObjectByType<PlayerMover>(FindObjectsInactive.Include);
            if (mover == null)
            {
                EditorUtility.DisplayDialog("이동·애니 통일", "지금 열린 씬에서 PlayerMover를 찾지 못했어요. 텃밭 씬을 열고 다시 실행해 주세요.", "확인");
                return;
            }
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
            var playerConfig = bootstrap != null ? new SerializedObject(bootstrap).FindProperty("_player").objectReferenceValue as PlayerConfig : null;
            if (!ApplyTo(mover.gameObject, playerConfig != null ? playerConfig.Avatar : null))
            {
                EditorUtility.DisplayDialog("이동·애니 통일", "플레이어 아래에서 아바타 Animator를 찾지 못했어요.", "확인");
                return;
            }
            EditorSceneManager.MarkSceneDirty(mover.gameObject.scene);
        }

        /// <summary>플레이어에 텃밭 이동 설정과 공용 애니메이션 뷰를 연결한다. 여러 번 실행해도 안전.</summary>
        public static bool ApplyTo(GameObject player, AvatarSetup avatar)
        {
            var animator = player.GetComponentInChildren<Animator>(true);
            if (animator == null) return false;
            var moveConfig = GetOrCreateMoveConfig();

            var view = GetOrAdd<CharacterAnimatorView>(animator.gameObject);
            view.SetRef("_animator", animator)
                .SetRef("_config", AssetDatabase.LoadAssetAtPath<CharacterAnimationConfig>(AnimationConfigPath));
            var speed = GetOrAdd<AvatarAnimationSpeed>(animator.gameObject);
            speed.SetRef("_animator", animator).SetRef("_moveConfig", moveConfig);
            if (avatar != null) speed.SetRef("_setup", avatar);

            var mover = player.GetComponent<PlayerMover>();
            Undo.RecordObject(mover, "텃밭 이동 설정 연결");
            mover.SetRef("_moveConfig", moveConfig).SetRef("_animationView", view);

            // 동작(낚시·물 주기): 공용 Controller의 Override, 클립 이벤트 수신(모델), 동작 뷰(플레이어)
            if (avatar != null && avatar.Controller != null && animator.runtimeAnimatorController != avatar.Controller)
            {
                Undo.RecordObject(animator, "공용 Controller 연결");
                animator.runtimeAnimatorController = avatar.Controller;
            }
            var events = GetOrAdd<CharacterAnimationEvents>(animator.gameObject);
            GetOrAdd<CharacterActionView>(player)
                .SetRef("_animator", animator)
                .SetRef("_events", events)
                .SetRef("_mover", mover)
                .SetRef("_config", CharacterControllerBuilder.GetOrCreateActionConfig());

            var legacy = player.GetComponent<PlayerAvatarAnimator>();
            if (legacy != null && legacy.enabled)
            {
                Undo.RecordObject(legacy, "옛 애니메이터 끄기");
                legacy.enabled = false;
            }
            Debug.Log($"[캐릭터] '{player.name}' 이동·애니 통일 (이동 설정 {moveConfig.name}, 아바타 {(avatar != null ? avatar.name : "연결 안 됨")}, 옛 PlayerAvatarAnimator 끔: {(legacy != null ? "예" : "없음")}). 확인 후 씬을 저장해 주세요.", player);
            return true;
        }

        /// <summary>3D 맵 공용 이동 설정. 없을 때만 만든다: 걷기 4·달리기 9(2.25배), 거의 바로 붙는 가속·감속, 재생 배율 걷기 1.6·달리기 3.6(2.25배).</summary>
        public static CharacterMoveConfig GetOrCreateMoveConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<CharacterMoveConfig>(MoveConfigPath);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<CharacterMoveConfig>();
            AssetDatabase.CreateAsset(config, MoveConfigPath);
            var so = new SerializedObject(config);
            so.FindProperty("_walkSpeed").floatValue = 4f;
            so.FindProperty("_runSpeed").floatValue = 9f;
            so.FindProperty("_acceleration").floatValue = 40f;
            so.FindProperty("_deceleration").floatValue = 50f;
            so.FindProperty("_walkAnimationScale").floatValue = 1.6f;
            so.FindProperty("_runAnimationScale").floatValue = 3.6f;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return config;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var existing = target.GetComponent<T>();
            return existing != null ? existing : Undo.AddComponent<T>(target);
        }
    }
}
