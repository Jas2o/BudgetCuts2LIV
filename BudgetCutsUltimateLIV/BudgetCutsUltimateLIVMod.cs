using LIV.SDK.Unity;
using MelonLoader;
using System;
using System.Linq;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UniverseLib;
using static UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace BudgetCutsUltimateLIV {

    public class BudgetCutsUltimateLIVMod : MelonMod {
        private bool hasAutoFixed = false;
        public static Action OnPlayerReady;

        private GameObject livObject;
        private Camera spawnedCamera;
        private static LIV.SDK.Unity.LIV livInstance;

        public override void OnInitializeMelon() {
            base.OnInitializeMelon();

            SetUpLiv();
            ClassInjector.RegisterTypeInIl2Cpp<LIV.SDK.Unity.LIV>();
            OnPlayerReady += TrySetupLiv;

            SystemLibrary.LoadLibrary($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\LIV_Bridge.dll");
        }

        public override void OnUpdate() {
            base.OnUpdate();

            if (!hasAutoFixed) {
                TryFixGame();
            }

            if (Input.GetKeyDown(KeyCode.F3)) {
                MelonLogger.Msg(">>> F3: TrySetupLiv");
                TrySetupLiv();
                TryFixGame();
            }

            UpdateFollowSpawnedCamera();
        }

        public void TryFixGame() {
            var livCam = GameObject.Find("LIV Camera");
            if (livCam != null) {
                var roomieOccCam = livCam.GetComponent<RoomieOcclusionCamera>();
                if (roomieOccCam != null) {
                    roomieOccCam.renderEverythingClose = true;
                    hasAutoFixed = true;
                }
            }
        }

        private GameObject hackObscura;

        private void OnPreRender(SDKRender obj) {
            //This is the weird black out effect when teleporting
            hackObscura = GameObject.Find("Obscurer");
            if (hackObscura != null) {
                hackObscura.layer = (int)GameLayer.ExcludeFromLIV;
            }
        }

        private void OnPostRender(SDKRender obj) {
            if (hackObscura != null) {
                hackObscura.layer = (int)GameLayer.Portal;
            }
        }

        public void TrySetupLiv() {
            //Budget Cuts Ultimate
            //Unity 2019.4.40f1 Il2Cpp

            //Following not verified for BCU
            Camera[] arrCam = GameObject.FindObjectsOfType<Camera>().ToArray();
            //MelonLogger.Msg(">>> Camera count: " + arrCam.Length);
            foreach (Camera cam in arrCam) {
                if (cam.name.Contains("LIV ") || cam.name.StartsWith("Pad ")) {
                    continue;
                } else if (cam.name.StartsWith("Camera")) {
                    SetUpLiv(cam);
                    break;
                }
            }

            TryFixGame();
        }

        private void UpdateFollowSpawnedCamera() {
            var livRender = GetLivRender();
            if (livRender == null || spawnedCamera == null) return;

            // When spawned objects get removed in Boneworks, they might not be destroyed and just be disabled.
            if (!spawnedCamera.gameObject.activeInHierarchy) {
                spawnedCamera = null;
                return;
            }

            var cameraTransform = spawnedCamera.transform;
            livRender.SetPose(cameraTransform.position, cameraTransform.rotation, spawnedCamera.fieldOfView);
        }

        private static void SetUpLiv() {
            AssetManager assetManager = new AssetManager($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\");
            var livAssetBundle = assetManager.LoadBundle("liv-shaders");
            SDKShaders.LoadFromAssetBundle(livAssetBundle);
        }

        private static Camera GetLivCamera() {
            try {
                return !livInstance ? null : livInstance.HMDCamera;
            } catch (Exception) {
                livInstance = null;
            }
            return null;
        }

        private static SDKRender GetLivRender() {
            try {
                return !livInstance ? null : livInstance.render;
            } catch (Exception) {
                livInstance = null;
            }
            return null;
        }

        private void SetUpLiv(Camera camera) {
            if (!camera) {
                MelonLogger.Msg("No camera provided, aborting LIV setup.");
                return;
            }

            var livCamera = GetLivCamera();
            if (livCamera == camera) {
                MelonLogger.Msg("LIV already set up with this camera, aborting LIV setup.");
                return;
            }

            MelonLogger.Msg($"Setting up LIV with camera: {camera.name}...");
            if (livObject) {
                Object.Destroy(livObject);
            }

            var cameraParent = camera.transform.parent;
            var cameraPrefab = new GameObject("LivCameraPrefab");
            cameraPrefab.SetActive(false);
            var cameraFromPrefab = cameraPrefab.AddComponent<Camera>();
            cameraPrefab.AddComponent<RoomieOcclusionCamera>();
            cameraFromPrefab.allowHDR = false;
            cameraPrefab.transform.SetParent(cameraParent, false);

            livObject = new GameObject("LIV");
            livObject.SetActive(false);

            livInstance = livObject.AddComponent<LIV.SDK.Unity.LIV>();
            livInstance.HMDCamera = camera;
            livInstance.MRCameraPrefab = cameraFromPrefab;
            livInstance.stage = cameraParent;
            livInstance.fixPostEffectsAlpha = true;
            livInstance.spectatorLayerMask = ~0;
            livInstance.spectatorLayerMask &= ~(1 << (int)GameLayer.ExcludeFromLIV);
            livInstance.onPreRender += OnPreRender;
            livInstance.onPostRender += OnPostRender;

            livObject.SetActive(true);

            hasAutoFixed = false;
        }
    }
}