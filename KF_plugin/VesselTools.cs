﻿/*
 * KSP [0.23.5] Anti-Grav Repulsor plugin by Lo-Fi
 * HUGE thanks to xEvilReeperx for this water code, along with gerneral coding help along the way!
 * Still works in 1.0.2!
 */

using System;
using System.Linq;
using UnityEngine;
using System.Collections;

namespace KerbalFoundries
{
	public class ModuleWaterSlider : VesselModule
	{
		readonly GameObject _collider = new GameObject("ModuleWaterSlider.Collider", typeof(BoxCollider), typeof(Rigidbody));
		const float triggerDistance = 25f;
		bool isActive;
		Vessel _vessel;
		public float colliderHeight = -2.5f;
<<<<<<< HEAD
        bool isReady;
=======
		
		/// <summary>Local name of the KFLogUtil class.</summary>
		readonly KFLogUtil KFLog = new KFLogUtil();
		/// <summary>Name of the class for logging purposes.</summary>
		public string strClassName = "ModuleWaterSlider";
>>>>>>> 7ad8a03e2b2f132d7d82d51ef646b570316a0dfb

		void Start()
		{
			KFLog.Log("WaterSlider start.", strClassName);
			_vessel = GetComponent<Vessel>();
            Debug.LogWarning(_vessel.IsControllable + " is cont");
            Debug.LogWarning(_vessel.isCommandable + " is comm");
            Debug.LogWarning(_vessel.vesselType.ToString() + " Vesseltype");

			float repulsorCount = 0;
			foreach (Part PA in _vessel.parts)
			{
				foreach (KFRepulsor RA in PA.GetComponentsInChildren<KFRepulsor>())
					repulsorCount++;
				foreach (RepulsorWheel RA in PA.GetComponentsInChildren<RepulsorWheel>())
					repulsorCount++;
			}

			isActive |= repulsorCount > 0;
			if (!isActive)
				return;
            if (!_vessel.isCommandable)
                return;

			var box = _collider.collider as BoxCollider;
			box.size = new Vector3(300f, .5f, 300f); // Probably should encapsulate other colliders in real code

			var rb = _collider.rigidbody;
			rb.isKinematic = true;

			_collider.SetActive(true);

			var visible = GameObject.CreatePrimitive(PrimitiveType.Cube);
			visible.transform.parent = _collider.transform;
			visible.transform.localScale = box.size;
			visible.renderer.enabled = true; // enable to see collider
			UpdatePosition();
            isReady = true;
		}

		void UpdatePosition()
		{
			Vector3d oceanNormal = _vessel.mainBody.GetSurfaceNVector(_vessel.latitude, _vessel.longitude);
			Vector3 newPosition = (_vessel.ReferenceTransform.position - oceanNormal * (FlightGlobals.getAltitudeAtPos(_vessel.ReferenceTransform.position) - colliderHeight));
			_collider.rigidbody.position = newPosition;
			_collider.rigidbody.rotation = Quaternion.LookRotation(oceanNormal) * Quaternion.AngleAxis(90f, Vector3.right);
		}


		void FixedUpdate()
		{
            if (!isReady)
                return;
			if (Vector3.Distance(_collider.transform.position, _vessel.transform.position) > triggerDistance && isReady)

				UpdatePosition();
			colliderHeight = Mathf.Clamp((colliderHeight -= 0.1f), -10, 2.5f);
		}
	}

	public class ModuleCameraShot : VesselModule
	{
		const int resWidth = 6;
		const int resHeight = 6;
		public Color _averageColour = new Color(1, 1, 1, 0.025f);
		int frameCount = 0;
		const int threshHold = 1;
		Vessel _vessel;
		GameObject _cameraObject;
        Camera _camera;
        Texture2D groundShot;
		RenderTexture renderTexture;
        bool dustCam;

		/// <summary>Local name of the KFLogUtil class.</summary>
		readonly KFLogUtil KFLog = new KFLogUtil();
		/// <summary>Name of the class for logging purposes.</summary>
		public string strClassName = "ModuleCameraShot";

		/// <summary>The layer or layers that the camera will render.</summary>
		public int cameraMask;

		void Start()
		{
			_vessel = GetComponent<Vessel>();
			_cameraObject = new GameObject("ColourCam");
            
			_cameraObject.transform.parent = _vessel.transform;
            _cameraObject.transform.LookAt(_vessel.mainBody.transform.position);
            _cameraObject.transform.Translate(new Vector3(0, 0, -10));
            _camera = _cameraObject.AddComponent<Camera>();
            _camera.targetTexture = renderTexture;
            cameraMask = 32784;	// Layers 4 and 15, or water and local scenery.
            // Generated from the binary place value output of 4 and 15 added to each other.
            // (1 << 4) | (1 << 15) = (16) | (32768) = 32784
            _camera.cullingMask = cameraMask;

            _camera.enabled = false;
            renderTexture = new RenderTexture(resWidth, resHeight, 24);
            groundShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            dustCam = KFPersistenceManager.isDustCameraEnabled;
		}

        public void Update()
        {
            dustCam = KFPersistenceManager.isDustCameraEnabled;
            if (frameCount >= threshHold && Equals(_vessel, FlightGlobals.ActiveVessel) && dustCam)
            {
                frameCount = 0;

                //_cameraObject.transform.position = _vessel.transform.position;
                _cameraObject.transform.LookAt(_vessel.mainBody.transform.position);
                _camera.targetTexture = renderTexture;
                //Extensions.DebugLine(_cameraObject.transform.position, _cameraObject.transform.eulerAngles);
                _camera.enabled = true;
                
                _camera.Render();
                //Debug.LogError("rendered something...");
                RenderTexture.active = renderTexture;
                groundShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                _camera.targetTexture = null;
                _camera.enabled = false;
                RenderTexture.active = null; // JC: added to avoid errors

                Color[] texColors = groundShot.GetPixels();
                int total = texColors.Length;
                float divider = total * 1.25f;
                float r = 0;
                float g = 0;
                float b = 0;
                const float alpha = 0.007f;

                for (int i = 0; i < total; i++)
                {
                    r += texColors[i].r;
                    g += texColors[i].g;
                    b += texColors[i].b;
                }
                _averageColour = new Color(r / divider, g / divider, b / divider, alpha);
<<<<<<< HEAD
                //print(_averageColour);
=======
                KFLog.Log(string.Format("\"_averageColour\" = ", _averageColour), strClassName);
>>>>>>> 7ad8a03e2b2f132d7d82d51ef646b570316a0dfb
            }
            frameCount++;
        }

		public static string ScreenShotName(int width, int height)
		{
			return string.Format("{0}/screen_{1}x{2}_{3}.png", Application.dataPath, width, height, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}

		
	}
}
