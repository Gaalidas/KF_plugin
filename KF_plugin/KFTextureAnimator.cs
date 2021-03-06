﻿using System;
using UnityEngine;
using KerbalFoundries;

namespace KerbalFoundries.TextureTools
{
	public class KFTextureAnimator : PartModule
	{
		[KSPField]
		public string ObjectName;
		
		[KSPField]
		public float minSpeedU = 5f;
		
		[KSPField]
		public float maxSpeedU = 15f;
		
		[KSPField]
		public float minSpeedV;
		
		[KSPField]
		public float maxSpeedV = 30f;
		
		[KSPField]
		public float smoothSpeed = 10f;
		
		[KSPField]
		public float minOffsetU = -0.1f;
		
		[KSPField]
		public float maxOffsetU = 0.1f;
		
		[KSPField]
		public float minOffsetV = -0.1f;
		
		[KSPField]
		public float maxOffsetV = 0.1f;
		
		[KSPField]
		public bool additiveMode;
		
		float timeU;
		float timeV;
		float offsetU;
		float offsetV;
		float smoothedU;
		float smoothedV;
		
		public bool isReady;
		
		Transform _mesh;
		
		/// <summary>Logging utility.</summary>
		/// <remarks>Call using "KFLog.log_type"</remarks>
		readonly KFLogUtil KFLog = new KFLogUtil("KFAPUController");
		
		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);
			_mesh = transform.TexAnimSearch(ObjectName);
			isReady |= HighLogic.LoadedSceneIsFlight;
			
			#if DEBUG
			KFLog.Log("Starting texture animator.");
			#endif
		}
		
		public void Update()
		{
			if (isReady)
			{
				if (timeU <= 0f)
				{
					timeU = UnityEngine.Random.Range(minSpeedU, maxSpeedU);
					offsetU = UnityEngine.Random.Range(minOffsetU, maxOffsetU);
				}
				if (timeV <= 0f)
				{
					timeV = UnityEngine.Random.Range(minSpeedV, maxSpeedV);
					offsetV = UnityEngine.Random.Range(minOffsetV, maxOffsetV);
				}
				timeU--;
				timeV--;
				smoothedU = Mathf.Lerp(smoothedU, offsetU, Time.deltaTime * smoothSpeed);
				smoothedV = Mathf.Lerp(smoothedV, offsetV, Time.deltaTime * smoothSpeed);
				Material material = _mesh.renderer.material;
				Vector2 vector = material.mainTextureOffset;
				if (additiveMode)
					vector += new Vector2(smoothedU, smoothedV);
				else
					vector = new Vector2(smoothedU, smoothedV);
				material.SetTextureOffset("_MainTex", vector);
				material.SetTextureOffset("_BumpMap", vector);
				material.SetTextureOffset("_Emissive", vector);
			}
		}
	}
}
