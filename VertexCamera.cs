using System;
using OWML.ModHelper;
using OWML.Common;
using HarmonyLib;
using UnityEngine;

namespace VertexCamera
{
    public class VertexCamera : ModBehaviour
    {
        private void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(VertexCamera)} has loaded.", MessageType.Success);
            //Load on title screen when the game starts
            if (LoadManager.GetCurrentScene() == OWScene.TitleScreen)
                Setup();
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                Setup();
            };
        }

        private void FixCamera(UnityEngine.Camera TargetCamera)
		{

		}

        private void Setup()
        {
            // Stolen from https://github.com/Owen013/Smol-Hatchling
            if (LoadManager.GetCurrentScene() != OWScene.SolarSystem && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
            {
                if (LoadManager.GetCurrentScene() != OWScene.TitleScreen)
                    return;
                ModHelper.Console.WriteLine("Running simple title screen fix.", MessageType.Success);
                UnityEngine.Camera MenuCam = LoadManager.FindObjectOfType<UnityEngine.Camera>();
                MenuCam.renderingPath = RenderingPath.VertexLit;
                MenuCam.allowHDR = false;
                LoadManager.FindObjectOfType<StarfieldController>().gameObject.SetActive(false);
                return;
            }
            ModHelper.Console.WriteLine("Setup triggered in game.", MessageType.Success);
        }


    }
}