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

        private void CheckMesh(MeshRenderer Mesh)
        {
            if (Mesh.gameObject.name.StartsWith("Foliage_") || Mesh.gameObject.name.StartsWith("DetailPatch_Foliage_"))
            {
                Mesh.enabled = false;
                return;
            }
            switch (Mesh.gameObject.name)
            {
                case "Cockpit_TransparentGlass_Structural":
                    Mesh.enabled = false;
                    break;
                case "Hatch_TransparentGlass":
                    Mesh.enabled = false;
                    break;
                case "Campfire_Flames":
                    Mesh.gameObject.DestroyAllComponents<MeshRenderer>();
                    break;
                default:
                    break;
            }
        }

        private void CheckSphere(TessellatedSphereRenderer Sphere, Mesh SphereMesh)
        {
            switch (Sphere.gameObject.name)
            {
                case "Ocean":
                    GameObject VisibleSphere = new GameObject("VisibleSphere_Ocean");
                    Sphere.gameObject.AddComponent<MeshRenderer>();
                    Sphere.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    Sphere.gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
                    Sphere.gameObject.AddComponent<MeshFilter>();
                    Sphere.gameObject.GetComponent<MeshFilter>().mesh = SphereMesh;

                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 1f, 0.5f);
                    break;
                case "Ocean_GD":
                    Sphere.gameObject.AddComponent<MeshRenderer>();
                    Sphere.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    Sphere.gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
                    Sphere.gameObject.AddComponent<MeshFilter>();
                    Sphere.gameObject.GetComponent<MeshFilter>().mesh = SphereMesh;
                    Sphere.gameObject.AddComponent<MeshRenderer>().material.doubleSidedGI = true;
                    Sphere.gameObject.AddComponent<MeshRenderer>().material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.75f, 0.75f, 0.5f);
                    break;
                case "SandSphere":
                    Sphere.gameObject.AddComponent<MeshRenderer>();
                    Sphere.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    Sphere.gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
                    Sphere.gameObject.AddComponent<MeshFilter>();
                    Sphere.gameObject.GetComponent<MeshFilter>().mesh = SphereMesh;

                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0.75f, 1f);
                    break;
                case "Surface":
                    Sphere.gameObject.AddComponent<MeshRenderer>();
                    Sphere.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    Sphere.gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
                    Sphere.gameObject.AddComponent<MeshFilter>();
                    Sphere.gameObject.GetComponent<MeshFilter>().mesh = SphereMesh;

                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.25f, 1f);
                    break;
                default:
                    break;
            }
        }

        private void Setup()
        {
            // Stolen from https://github.com/Owen013/Smol-Hatchling
            if (LoadManager.GetCurrentScene() != OWScene.SolarSystem && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
            {
                if (LoadManager.GetCurrentScene() != OWScene.TitleScreen)
                    return;
                ModHelper.Console.WriteLine("Running on title screen.", MessageType.Info);
                Camera MenuCam = FindObjectOfType<Camera>();
                MenuCam.renderingPath = RenderingPath.VertexLit;
                MenuCam.allowHDR = false;
                FindObjectOfType<StarfieldController>().gameObject.SetActive(false);
                return;
            }
            ModHelper.Console.WriteLine("Running in game.", MessageType.Info);
            Camera[] Cameras = FindObjectsOfType<Camera>();
            foreach (Camera CurrentCamera in Cameras)
            {
                CurrentCamera.renderingPath = RenderingPath.VertexLit;
            }
            OWCamera[] OWCameras = FindObjectsOfType<OWCamera>();
            foreach (OWCamera CurrentCamera in OWCameras)
            {
                try
                {
                    CurrentCamera.postProcessingSettings.bloomEnabled = false;
                    CurrentCamera.postProcessingSettings.colorGradingEnabled = false;
                    CurrentCamera.postProcessingSettings.phosphenesEnabled = false;
                    CurrentCamera.postProcessingSettings.vignetteEnabled = false;
                    CurrentCamera.postProcessingSettings.userLutEnabled = false;
                    CurrentCamera.renderSkybox = false;
                }
                catch { }
            }

			switch (LoadManager.GetCurrentScene())
			{
				case OWScene.SolarSystem:
                    foreach (MeshRenderer CurrentMesh in FindObjectsOfType<MeshRenderer>())
                    {
                        CheckMesh(CurrentMesh);
                    }


                    GameObject TempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    TempSphere.transform.position = new Vector3(0, 0, 0);
                    Mesh SphereMesh = TempSphere.GetComponent<MeshFilter>().mesh;
                    Destroy(TempSphere);

                    // https://answers.unity.com/questions/523289/change-size-of-mesh-at-runtime.html
                    Vector3[] baseVertices;
                    baseVertices = SphereMesh.vertices;
                    Vector3[] vertices = new Vector3[baseVertices.Length];
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var vertex = baseVertices[i];
                        vertex.x = vertex.x * 2f;
                        vertex.y = vertex.y * 2f;
                        vertex.z = vertex.z * 2f;

                        vertices[i] = vertex;
                    }
                    SphereMesh.vertices = vertices;
                    if (false)
                        SphereMesh.RecalculateNormals();
                    SphereMesh.RecalculateBounds();


                    foreach (TessellatedSphereRenderer CurrentSphere in FindObjectsOfType<TessellatedSphereRenderer>())
                    {
                        CheckSphere(CurrentSphere, SphereMesh);
                    }
                    break;
                case OWScene.EyeOfTheUniverse:

                    break;
                default:
					break;
			}
		}


    }
}