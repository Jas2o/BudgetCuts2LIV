using System;
using System.Linq;
using LIV;
using LIV.SDK.Unity;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BudgetCuts2LIV
{
	public class BudgetCuts2LIVMod : MelonMod
	{
		private bool hasAutoFixed = false;
		public static Action OnPlayerReady;

		private GameObject livObject;
		private Camera spawnedCamera;
		private static LIV.SDK.Unity.LIV livInstance => LIV.SDK.Unity.LIV.Instance;

		public override void OnApplicationStart()
		{
			base.OnApplicationStart();

			SetUpLiv();
			ClassInjector.RegisterTypeInIl2Cpp<LIV.SDK.Unity.LIV>();
			OnPlayerReady += TrySetupLiv;
			
			SystemLibrary.LoadLibrary($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\LIV_Bridge.dll");
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if(!hasAutoFixed) {
				//Seems to only need to be done once at the start of the game menu
                TryFixGame();
            }

			if (Input.GetKeyDown(KeyCode.F3))
			{
                MelonLogger.Msg(">>> F3: TrySetupLiv");
                TrySetupLiv();
				TryFixGame();
            }

			UpdateFollowSpawnedCamera();
		}

		public void TryFixGame() {
            // Try to fix what breaks the game starting with ExternalCamera.cfg present that LIV likes to make.
            GameObject exCam = GameObject.Find("External Camera");
            if (exCam != null) {
                MelonLogger.Msg("Disabling 'External Camera' to fix game.");
                exCam.active = false;
				hasAutoFixed = true;
            }

            GameObject exCon = GameObject.Find("Controller (third)");
            if (exCon != null) {
                MelonLogger.Msg("Disabling 'Controller (third)' to fix game.");
                exCon.active = false;
                hasAutoFixed = true;
            }
	    
	    // The blue ring under the player causes alpha issues depending on camera distance.
            GameObject trackingRing = GameObject.Find("Tracking ring");
            trackingRing.layer = (int)GameLayer.ExcludeFromLIV;
        }

		public void TrySetupLiv() {
            //Budget Cuts 1 (Arcade) and Budget Cuts 2
            //Both Unity 2018.4.23f1
            //Doesn't use SteamVR_Camera, Externalcamera.cfg will break the game.
            //Camera can change between scenes
            //Yes: Camera - This is in the title scene
            //Yes: Camera (rendering) - This is in menu and levels
            //No: Pad Eye (rendering) - this is the teleporter
            //No: Magnifying glass camera

            //Approx hierarchy
            //	Player body (origin) > Player (game logic head) > Camera

            Camera[] arrCam = GameObject.FindObjectsOfType<Camera>().ToArray();
            //MelonLogger.Msg(">>> Camera count: " + arrCam.Length);
            foreach (Camera cam in arrCam) {
                if (cam.name.Contains("LIV ") || cam.name.StartsWith("Pad ")) {
                    continue;
                } else if (cam.name.StartsWith("Camera")) {
                    //Budget Cuts 2
                    //MelonLogger.Msg(cam.name + " # LIV");
                    SetUpLiv(cam);
                    break;
                }
                //else MelonLogger.Msg(cam.name);
            }

            TryFixGame();
        }

		private void UpdateFollowSpawnedCamera()
		{
			var livRender = GetLivRender();
			if (livRender == null || spawnedCamera == null) return;

			// When spawned objects get removed in Boneworks, they might not be destroyed and just be disabled.
			if (!spawnedCamera.gameObject.activeInHierarchy)
			{
				spawnedCamera = null;
				return;
			}

			var cameraTransform = spawnedCamera.transform;
			livRender.SetPose(cameraTransform.position, cameraTransform.rotation, spawnedCamera.fieldOfView);
		}

		private static void SetUpLiv()
		{
            AssetManager assetManager = new AssetManager($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\");
			var livAssetBundle = assetManager.LoadBundle("liv-shaders");
            SDKShaders.LoadFromAssetBundle(livAssetBundle);
		}

		private static Camera GetLivCamera()
		{
			try
			{
				return !livInstance ? null : livInstance.HMDCamera;
			}
			catch (Exception)
			{
				LIV.SDK.Unity.LIV.Instance = null;
			}
			return null;
		}


		private static SDKRender GetLivRender()
		{
			try
			{
				return !livInstance ? null : livInstance.render;
			}
			catch (Exception)
			{
				LIV.SDK.Unity.LIV.Instance = null;
			}
			return null;
		}

		private void SetUpLiv(Camera camera)
		{
			if (!camera)
			{
				MelonLogger.Msg("No camera provided, aborting LIV setup.");
				return;
			}

			var livCamera = GetLivCamera();
			if (livCamera == camera)
			{
				MelonLogger.Msg("LIV already set up with this camera, aborting LIV setup.");
				return;
			}

			MelonLogger.Msg($"Setting up LIV with camera: {camera.name}...");
			if (livObject)
			{
				Object.Destroy(livObject);
			}

			var cameraParent = camera.transform.parent;
			var cameraPrefab = new GameObject("LivCameraPrefab");
			cameraPrefab.SetActive(false);
			var cameraFromPrefab = cameraPrefab.AddComponent<Camera>();
			cameraFromPrefab.allowHDR = false;
			cameraPrefab.transform.SetParent(cameraParent, false);

			livObject = new GameObject("LIV");
			livObject.SetActive(false);

			var liv = livObject.AddComponent<LIV.SDK.Unity.LIV>();
			liv.HMDCamera = camera;
			liv.MRCameraPrefab = cameraFromPrefab;
			liv.stage = cameraParent;
			liv.fixPostEffectsAlpha = true;
			liv.spectatorLayerMask = -1073741825; //ExcludeFromLiv
			
			livObject.SetActive(true);
		}
	}
}
