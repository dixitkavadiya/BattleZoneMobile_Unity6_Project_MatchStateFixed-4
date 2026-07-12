#if UNITY_EDITOR
using BattleZoneMobile;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleZoneMobile.Editor
{
    public static class RepairPlayerMovementTool
    {
        private const string ScenePath = "Assets/BattleZoneMobile/Scenes/BZ_Main.unity";

        [MenuItem("BattleZone Tools/Repair Player Movement")]
        public static void RepairPlayerMovement()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = FindPlayerObject();
            if (player == null)
            {
                Debug.LogWarning("No edit-time Player object exists in BZ_Main. BattleZoneRuntimeBuilder creates Player at runtime and now installs ReliablePlayerMovement automatically.");
                EditorSceneManager.SaveScene(scene);
                return;
            }

            RepairPlayerObject(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("BattleZone player movement repaired in BZ_Main.");
        }

        public static void RepairPlayerObject(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("RepairPlayerObject failed: player is null.");
                return;
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("RepairPlayerObject failed: Player has no existing CharacterController.");
                return;
            }

            foreach (ThirdPersonMobileController oldController in player.GetComponents<ThirdPersonMobileController>())
            {
                oldController.enabled = false;
                EditorUtility.SetDirty(oldController);
            }

            ReliablePlayerMovement reliableMovement = player.GetComponent<ReliablePlayerMovement>();
            if (reliableMovement == null)
            {
                reliableMovement = player.AddComponent<ReliablePlayerMovement>();
            }

            reliableMovement.ConfigureForRuntime(characterController, FindMainCamera(), FindJoystick());
            reliableMovement.enabled = true;
            EditorUtility.SetDirty(reliableMovement);
            EditorUtility.SetDirty(player);
        }

        private static GameObject FindPlayerObject()
        {
            GameObject player = null;
            try
            {
                player = GameObject.FindWithTag("Player");
            }
            catch (UnityException)
            {
                player = null;
            }

            if (player != null)
            {
                return player;
            }

            foreach (GameObject candidate in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            {
                if (candidate != null && candidate.name == "Player")
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Camera FindMainCamera()
        {
            Camera camera = Camera.main;
            return camera != null ? camera : Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
        }

        private static FloatingJoystick FindJoystick()
        {
            return Object.FindAnyObjectByType<FloatingJoystick>(FindObjectsInactive.Include);
        }
    }
}
#endif
