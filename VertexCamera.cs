using System;
using OWML.ModHelper;
using OWML.Common;
using HarmonyLib;
using UnityEngine;
using System.Linq;

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
                //MenuCam.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
                RenderSettings.ambientLight = new Color(0.125f, 0.125f, 0.125f, 1f);
                MenuCam.allowHDR = false;
                FindObjectOfType<StarfieldController>().gameObject.SetActive(false);
                return;
            }

            ModHelper.Console.WriteLine("Running in game.", MessageType.Info);
            FixCameras();

            switch (LoadManager.GetCurrentScene())
            {
                case OWScene.SolarSystem:
                    foreach (MeshRenderer CurrentMesh in FindObjectsOfType<MeshRenderer>())
                    {
                        CheckMesh(CurrentMesh);
                    }

                    GameObject TempObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    TempObject.transform.position = new Vector3(0, 0, 0);
                    Mesh SphereMesh = ModHelper.Assets.GetMesh("ModAssets/Sphere.obj");
                    Mesh InvertedSphereMesh = TempObject.GetComponent<MeshFilter>().mesh;
                    Material DefaultMaterial = TempObject.GetComponent<MeshRenderer>().material;
                    Destroy(TempObject);

                    // http://answers.unity.com/answers/523351/view.html
                    Vector3[] baseVertices = InvertedSphereMesh.vertices;
                    Vector3[] vertices = new Vector3[baseVertices.Length];
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var vertex = baseVertices[i];
                        vertex.x *= 2.005f;
                        vertex.y *= 2.005f;
                        vertex.z *= 2.005f;

                        vertices[i] = vertex;
                    }
                    InvertedSphereMesh.vertices = vertices;
                    InvertedSphereMesh.RecalculateBounds();

                    // http://answers.unity.com/answers/476854/view.html
                    InvertedSphereMesh.triangles = InvertedSphereMesh.triangles.Reverse().ToArray();

                    foreach (TessellatedSphereRenderer CurrentSphere in FindObjectsOfType<TessellatedSphereRenderer>())
                    {
                        CheckSphere(CurrentSphere, SphereMesh, DefaultMaterial, InvertedSphereMesh);
                    }
                    RenderSettings.ambientLight = new Color(0.125f, 0.125f, 0.125f, 1f);

                    foreach (ParticleSystemRenderer CurrentParticle in FindObjectsOfType<ParticleSystemRenderer>())
                    {
                        CheckParticle(CurrentParticle);
                    }



                    //TODO: Fix nomai text and decals
                    //foreach (NomaiTextLine CurrentText in FindObjectsOfType<NomaiTextLine>())
                    //{
                    //    try
                    //    {
                    //        //CurrentText.gameObject.GetComponent<MeshFilter>().mesh = InvertedSphereMesh;
                    //        CurrentText.gameObject.GetComponent<MeshRenderer>().material = DefaultMaterial;
                    //        CurrentText.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);
                    //    }
                    //    catch { }
                    //}
                    //foreach (NomaiWallText CurrentText in FindObjectsOfType<NomaiWallText>())
                    //{
                    //    try
                    //    {
                    //        //CurrentText.gameObject.GetComponent<MeshFilter>().mesh = InvertedSphereMesh;
                    //        CurrentText.gameObject.GetComponent<MeshRenderer>().material = DefaultMaterial;
                    //        CurrentText.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
                    //    }
                    //    catch { }
                    //}
                    //foreach (OWEmissiveRenderer CurrentEmissive in FindObjectsOfType<OWEmissiveRenderer>())
                    //{
                    //    try
                    //    {
                    //        //CurrentEmissive.gameObject.GetComponent<MeshFilter>().mesh = InvertedSphereMesh;
                    //        CurrentEmissive.gameObject.GetComponent<MeshRenderer>().material = DefaultMaterial;
                    //        CurrentEmissive.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
                    //    }
                    //    catch { }
                    //}

                    break;
                case OWScene.EyeOfTheUniverse:
                    ModHelper.Console.WriteLine("The Eye isn't supported yet!", MessageType.Warning);
                    break;
                default:
                    break;
            }
        }

        private void FixCameras()
        {
            Camera[] Cameras = FindObjectsOfType<Camera>();
            foreach (Camera CurrentCamera in Cameras)
            {
                string[] CameraNames = new string[] { CurrentCamera.gameObject.name };
                if (System.Array.IndexOf(CameraNames, CurrentCamera.gameObject.name) != -1)
                {
                    CurrentCamera.renderingPath = RenderingPath.VertexLit;
                    //if (CurrentCamera.gameObject.name != "HUDCamera")
                    //    CurrentCamera.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
                }
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
        }

        private void CheckMesh(MeshRenderer Mesh)
        {
            if (Mesh.gameObject.name.StartsWith("Foliage_") || Mesh.gameObject.name.StartsWith("DetailPatch_Foliage_"))
            {
                //Was causing all sorts of issues
                //Mesh.gameObject.DestroyAllComponents<MeshRenderer>();
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
                case "ShipComputer_Screen":
                    Mesh.enabled = false;
                    break;
                case "LandingCamScreen":
                    Mesh.gameObject.GetComponent<LandingCameraDashboardRenderer>().renderer.allowOcclusionWhenDynamic = false;
                    break;
                case "Campfire_Flames":
                    Mesh.gameObject.DestroyAllComponents<MeshRenderer>();
                    break;
                // https://github.com/FrostBird347/VertexCamera/issues/11
                //case "Props_NOM_Orb":
                //    Mesh.gameObject.transform.localScale = new Vector3(2, 2, 2);
                //    break;
                case "CloudsTopLayer_GD":
                    Mesh.material.color = new Color(0f, 0.5f, 0.225f, 0.5f);
                    break;
                case "CloudsTopLayer_GD (1)":
                    Mesh.material.color = new Color(0f, 0.5f, 0.125f, 0.5f);
                    break;
                default:
                    break;
            }
            try
            {
                string[] TwinsNames = new string[] { "Terrain_QM_CaveTwinSurface_mat (Instance)", "Terrain_HCT_Caves_mat (Instance)", "Terrain_HCT_Surface_mat (Instance)", "Terrain_QM_TT_Cliffs_mat (Instance)" };
                if (System.Array.IndexOf(TwinsNames, Mesh.material.name + " (Instance)") != -1)
                    Mesh.material.color = new Color(1f, 0.5f, 0.25f, 1f);
                if (System.Array.IndexOf(TwinsNames, Mesh.material.name) != -1)
                    Mesh.material.color = new Color(1f, 0.5f, 0.25f, 1f);
            }
            catch { }
        }

        private void CheckSphere(TessellatedSphereRenderer Sphere, Mesh SphereMesh, Material DefaultMaterial, Mesh InvertedSphereMesh)
        {
            string[] Spheres = new string[] { "Ocean_GD", "Ocean", "SandSphere", "Surface", "CloudsBottomLayer_QM", "CloudsBottomLayer_GD" };
            if (System.Array.IndexOf(Spheres, Sphere.gameObject.name) != -1)
            {
                //Spheres[System.Array.IndexOf(Spheres, Sphere.gameObject.name)] = "REMOVED_SPHERE_CHECK";
                Sphere.gameObject.AddComponent<MeshRenderer>();
                Sphere.gameObject.AddComponent<MeshFilter>();
                Sphere.gameObject.GetComponent<MeshFilter>().mesh = SphereMesh;
                Sphere.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Sphere.gameObject.GetComponent<MeshRenderer>().receiveShadows = false;
                Sphere.gameObject.GetComponent<MeshRenderer>().material = DefaultMaterial;
                Sphere.cullingMode = Tessellation.Patch.CullingMode.None;
                Sphere.enabled = false;
                //Sphere.gameObject.AddComponent<MeshRenderer>().material.doubleSidedGI = true;
                //Sphere.gameObject.AddComponent<MeshRenderer>().material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            switch (Sphere.gameObject.name)
            {
                case "Ocean":
                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 1f, 0.5f);
                    break;
                case "Ocean_GD":
                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.75f, 0.75f, 0.5f);
                    break;
                case "SandSphere":
                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0.75f, 1f);
                    break;
                case "Surface":
                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.25f, 1f);
                    Sphere.gameObject.GetComponent<MeshFilter>().mesh = InvertedSphereMesh;
                    break;
                case "CloudsBottomLayer_QM":
                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.125f, 0.5f, 0.5f);
                    break;
                case "CloudsBottomLayer_GD":
                    Sphere.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0f, 0.75f, 0.25f, 0.5f);
                    // DON'T DO THIS:
                    //  Sphere.gameObject.DestroyAllComponents<TessSphereSectorToggle>();
                    //  Sphere.gameObject.DestroyAllComponents<TessellatedSphereRenderer>();
                    Sphere.enabled = false;
                    Sphere.gameObject.GetComponent<TessSphereSectorToggle>().enabled = false;
                    break;
                default:
                    break;
            }

            Sphere.gameObject.name += "_DONE";
        }

        private void CheckParticle(ParticleSystemRenderer Particle)
        {
            if (Particle.name.StartsWith("ThrusterWash_") || Particle.name.StartsWith("PlayerImpact_") || Particle.name.StartsWith("Waterfall"))
            {
                Particle.enabled = false;
            }
        }
    }
}