﻿using System.Collections.Generic;
using UnityEngine;

namespace KerbalFoundries
{
	// SharpDevelop Suppressions.
	// disable ConvertToStaticType
	
	/// <summary>Loads, contains, and saves global configuration nodes.</summary>
	/// <remarks>Brain-Child of *Aqua* and without which we would never have created a working GUI config system.</remarks>
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class KFPersistenceManager : MonoBehaviour
	{
		#region Log Parameters
		
		/// <summary>Local name of the KFLogUtil class.</summary>
		static KFLogUtil KFLog;
		
		/// <summary>A KFLog definition for an initial log entry.</summary>
		static KFLogUtil KFLogInit;

		/// <summary>Path\Settings\KFGlobals.txt</summary>
		static string configFileName = string.Format("{0}GameData/KerbalFoundries/Settings/KFGlobals.txt", KSPUtil.ApplicationRootPath);

		/// <summary>Path\Settings\DustColors.cfg</summary>
		static string dustColorsFileName = string.Format("{0}GameData/KerbalFoundries/Settings/DustColors.cfg", KSPUtil.ApplicationRootPath);
        
		/// <summary>Log related.  Moved to the outer scope so I can manipulate it later.</summary>
		static string strClassName = "KFPersistenceManager";
		
		#endregion Log Parameters
		
		#region Initialization
		/// <summary>Makes sure the global configuration is good to go.</summary>
		/// <remarks>This is a static constructor. It's called once when the class is loaded by Mono.</remarks>
		static KFPersistenceManager()
		{
			writeToLogFile = false; // This makes sure that the logging thread can't start before all of the configuration is read or bad things will happen
			KFLog = new KFLogUtil(strClassName);
			KFLogInit = new KFLogUtil();
            
			KFLogInit.Log(string.Format("Version: {0}", KFVersion.versionString));

			ReadConfigFile();
			ReadDustColor();
		}
		
		#endregion Initialization
		
		#region Read & write
		/// <summary>Retrieves the settings which are stored in the configuration file and are auto-loaded by KSP.</summary>
		static void ReadConfigFile()
		{
			// KFGlobals.txt
			ConfigNode configFile = ConfigNode.Load(configFileName);
			if (Equals(configFile, null) || !configFile.HasNode("KFGlobals")) // KFGlobals-node doesn't exist
			{
				CreateConfig();
				configFile = ConfigNode.Load(configFileName);
			}

			ConfigNode configNode = configFile.GetNode("KFGlobals");
			if (Equals(configNode, null) || Equals(configNode.CountValues, 0)) // KFGlobals-node is empty
			{
				CreateConfig();
				configNode = configFile.GetNode("KFGlobals");
			}

			bool _isDustEnabled = false;
			if (bool.TryParse(configNode.GetValue("isDustEnabled"), out _isDustEnabled))
				isDustEnabled = _isDustEnabled;
			
			bool _isDustCameraEnabled = false;
			if (bool.TryParse(configNode.GetValue("isDustCameraEnabled"), out _isDustCameraEnabled))
				isDustCameraEnabled = _isDustCameraEnabled;
			
			bool _isMarkerEnabled = false;
			if (bool.TryParse(configNode.GetValue("isMarkerEnabled"), out _isMarkerEnabled))
				isMarkerEnabled = _isMarkerEnabled;
			
			bool _isRepLightEnabled = false;
			if (bool.TryParse(configNode.GetValue("isRepLightEnabled"), out _isRepLightEnabled))
				isRepLightEnabled = _isRepLightEnabled;

			float _dustAmount = 1;
			if (float.TryParse(configNode.GetValue("dustAmount"), out _dustAmount))
				dustAmount = _dustAmount;
            
			float _suspensionIncrement = 5;
			if (float.TryParse(configNode.GetValue("suspensionIncrement"), out _suspensionIncrement))
				suspensionIncrement = _suspensionIncrement;
			
			bool _isDebugEnabled = false;
			if (bool.TryParse(configNode.GetValue("isDebugEnabled"), out _isDebugEnabled))
				isDebugEnabled = _isDebugEnabled;
			
			bool _writeToLogFile = false;
			if (bool.TryParse(configNode.GetValue("writeToLogFile"), out _writeToLogFile))
			{
				writeToLogFile = _writeToLogFile;
				if (writeToLogFile)
					KerbalFoundries.Log.KFLog.StartWriter();
			}
			
			string _logfile = configNode.GetValue("logFile");
			logFile = Equals(_logfile, string.Empty) || Equals(_logfile, null) ? "KF.log" : _logfile;
			
			int _cameraRes = 6;
			if (int.TryParse(configNode.GetValue("cameraRes"), out _cameraRes))
				cameraRes = _cameraRes;
			
			int _cameraFraterate = 10;
			if (int.TryParse(configNode.GetValue("cameraFramerate"), out _cameraFraterate))
				cameraFramerate = _cameraFraterate;
			
			LogConfigValues();
		}

		static void LogConfigValues()
		{
			KFLog.Log("Configuration Settings are:");
			KFLog.Log(string.Format("  isDustEnabled = {0}", isDustEnabled));
			KFLog.Log(string.Format("  isDustCameraEnabled = {0}", isDustCameraEnabled));
			KFLog.Log(string.Format("  isMarkerEnabled = {0}", isMarkerEnabled));
			KFLog.Log(string.Format("  isRepLightEnabled = {0}", isRepLightEnabled));
			KFLog.Log(string.Format("  dustamount = {0}", dustAmount));
			KFLog.Log(string.Format("  suspensionIncrement = {0}", suspensionIncrement));
			KFLog.Log(string.Format("  isDebugEnabled = {0}", isDebugEnabled));
			if (isDebugEnabled)
				KFLog.Log(string.Format("    debugIsWaterColliderVisible = {0}", debugIsWaterColliderVisible));
			KFLog.Log(string.Format("  writeToLogFile = {0}", writeToLogFile));
			KFLog.Log(string.Format("  logFile = {0}", logFile));
			KFLog.Log(string.Format("  cameraRes = {0}", cameraRes));
			KFLog.Log(string.Format("  cameraFramerate = {0}", cameraFramerate));
		}
		
		/// <summary>Retrieves the dust colors which are stored in the DustColors-file and are auto-loaded by KSP.</summary>
		static void ReadDustColor()
		{
			// DustColors.cfg
			//dustColorsFileName = string.Format("{0}GameData/KerbalFoundries/DustColors.cfg", KSPUtil.ApplicationRootPath);
			DustColors = new Dictionary<string, Dictionary<string, Color>>();
            
			ConfigNode configFile = ConfigNode.Load(dustColorsFileName);
			if (Equals(configFile, null) || !configFile.HasNode("DustColorDefinitions"))  // DustColorDefinitions node doesn't exist.
			{
				KFLog.Warning("DustColors.cfg is missing or damaged!");
				return;
			}
			
			ConfigNode configNode = configFile.GetNode("DustColorDefinitions");
			if (Equals(configNode, null) || Equals(configNode.CountNodes, 0)) // DustColorDefinitions node is empty.
			{
				KFLog.Warning("Dust color definitions not found or damaged!");
				return;
			}
			
			foreach (ConfigNode celestialNode in configNode.GetNodes()) // For each celestial body do this:
			{
				var biomes = new Dictionary<string, Color>();
				foreach (ConfigNode biomeNode in celestialNode.GetNodes()) // For each biome of that celestial body do this:
				{
					float r = 0f;
					float.TryParse(biomeNode.GetValue("Color").Split(',')[0], out r);
					float g = 0f;
					float.TryParse(biomeNode.GetValue("Color").Split(',')[1], out g);
					float b = 0f;
					float.TryParse(biomeNode.GetValue("Color").Split(',')[2], out b);
					float a = 0f;
					float.TryParse(biomeNode.GetValue("Color").Split(',')[3], out a);
					biomes.Add(biomeNode.name, new Color(r, g, b, a));
				}
				
				DustColors.Add(celestialNode.name, biomes);
				if (Equals(biomes.Count, 0))
					KFLog.Error(string.Format("No biome colors found for {0}!", celestialNode.name));
				else
					KFLog.Log(string.Format("Found {0} biome color definitions for {1}.", biomes.Count, celestialNode.name));
			}
		}
		
		/// <summary>Saves the settings to the configuration file.</summary>
		internal static void SaveConfig()
		{
			// KFGlobals.cfg
			System.IO.File.Delete(configFileName);
            
			// Sync debug state with debug options.
			if (!isDebugEnabled && debugIsWaterColliderVisible)
				debugIsWaterColliderVisible = false;
            
			var configFile = new ConfigNode();
			configFile.AddNode("KFGlobals");
			ConfigNode configNode = configFile.GetNode("KFGlobals");
            
			configNode.SetValue("isDustEnabled", isDustEnabled.ToString(), true);
			configNode.SetValue("isDustCameraEnabled", isDustCameraEnabled.ToString(), true);
			configNode.SetValue("isMarkerEnabled", isMarkerEnabled.ToString(), true);
			configNode.SetValue("isRepLightEnabled", isRepLightEnabled.ToString(), true);
			configNode.SetValue("dustAmount", Mathf.Clamp(dustAmount.RoundToNearestValue(0.25f), 0f, 3f).ToString(), true);
			configNode.SetValue("suspensionIncrement", Mathf.Clamp(suspensionIncrement.RoundToNearestValue(5f), 5f, 20f).ToString(), true);
			configNode.SetValue("isDebugEnabled", isDebugEnabled.ToString(), true);
			configNode.SetValue("debugIsWaterColliderVisible", debugIsWaterColliderVisible.ToString(), true);
			configNode.SetValue("writeToLogFile", writeToLogFile.ToString(), true);
			configNode.SetValue("logFile", logFile, true);
			configNode.SetValue("cameraRes", cameraRes.ToString(), true);
			configNode.SetValue("cameraFramerate", cameraFramerate.ToString(), true);
			
			configFile.Save(configFileName);

			KFLog.Log("Global Settings Saved.");
			LogConfigValues();
		}

		/// <summary>Creates configuration file with default values.</summary>
		static void CreateConfig()
		{
			isDustEnabled = true;
			isDustCameraEnabled = true;
			isMarkerEnabled = true;
			isRepLightEnabled = true;
			
			dustAmount = 1f;
			suspensionIncrement = 5f;
			
			isDebugEnabled = false;
			debugIsWaterColliderVisible = false;
            
			writeToLogFile = false;
			logFile = "KF.log";
            
			cameraRes = 6;
			cameraFramerate = 10;
			
			KFLog.Log("Default Config Created.");
			SaveConfig();
		}
		#endregion Read & write
		
		#region Global Config Properties
		/// <summary>If dust is displayed.</summary>
		public static bool isDustEnabled
		{
			get;
			set;
		}
		
		/// <summary>If a camera is used to identify ground color for setting the correct dust color.</summary>
		public static bool isDustCameraEnabled
		{
			get;
			set;
		}
		
		/// <summary>If orientation markers on wheels are displayed in the VAB/SPH.</summary>
		public static bool isMarkerEnabled
		{
			get;
			set;
		}
		
		/// <summary>If repulsor lighting is enabled.</summary>
		public static bool isRepLightEnabled
		{
			get;
			set;
		}

		/// <summary>The amount of dust to be emitted.</summary>
		public static float dustAmount
		{
			get;
			set;
		}

		/// <summary>The incremental value to change the ride height by when using action groups.</summary>
		/// <remarks>Should be rounded to the nearest whole number before the setter is called.</remarks>
		public static float suspensionIncrement
		{
			get;
			set;
		}
        
		/// <summary>Tracks whether or not the debug options should be made visible or not.</summary>
		public static bool isDebugEnabled
		{
			get;
			set;
		}
        
		public static bool debugIsWaterColliderVisible
		{
			get;
			set;
		}

		/// <summary>If all KF log messages should also be written to a log file.</summary>
		/// <remarks>logFile must be specified in the global config!</remarks>
		public static bool writeToLogFile
		{
			get;
			set;
		}

		/// <summary>Path of the log file</summary>
		public static string logFile
		{
			get;
			set;
		}
		
		/// <summary>Resolution for the camera in ModuleCameraShot.</summary>
		public static int cameraRes
		{
			get;
			set;
		}
		
		/// <summary>Framerate for the camera is ModuleCameraShot.</summary>
		public static int cameraFramerate
		{
			get;
			set;
		}
		#endregion Global Config Properties

		#region DustFX
		/// <summary>Dust colors for each biome.</summary>
		/// <remarks>Key = celestial body name, Value(s) = { Key = biome_name, Value = color }</remarks>
		public static Dictionary<string, Dictionary<string, Color>> DustColors
		{
			get;
			set;
		}
		
		/// <summary>Use this color of there's no biome dust color defined.</summary>
		public static readonly Color DefaultDustColor = new Color(0.75f, 0.75f, 0.75f, 0.007f);
		#endregion DustFX

		#region Part Icon Fix
		static void OnDestroy() // Last possible point before the loading scene switches to the Main Menu scene.
		{
			FindKFPartsToFix().ForEach(FixPartIcon);
		}

		/// <summary>Finds all KF parts which part icons need to be fixed</summary>
		/// <returns>List of parts with KFIconOverride configNode</returns>
		static List<AvailablePart> FindKFPartsToFix()
		{
			strClassName += ": Icon Fixer";
			List<AvailablePart> KFPartsList = PartLoader.LoadedPartsList.FindAll(IsAKFPart);
			
			#if DEBUG
			KFLog.Log("\nAll KF Parts:");
			KFPartsList.ForEach(part => KFLog.Log(string.Format("  {0}", part.name)));
			#endif

			List<AvailablePart> KFPartsToFixList = KFPartsList.FindAll(HasKFIconOverrideModule);
			
			#if DEBUG
			KFLog.Log("\nKF Parts which need an icon fix:");
			KFPartsToFixList.ForEach(part => KFLog.Log(string.Format("  {0}", part.name)));
			#endif

			return KFPartsToFixList;
		}

		/// <summary>Fixes incorrect part icon in the editor's parts list panel for every part which has a KFIconOverride node.
		/// The node can have several attributes.
		/// Example:
		/// KFIconOverride
		/// {
		///     Multiplier = 1.0      // for finetuning icon zoom
		///     Pivot = transformName // transform to rotate around; rotates around CoM if not specified
		///     Rotation = vector     // offset to rotation point
		/// }
		/// All parameters are optional. The existence of an KFIconOverride node is enough to fix the icon.
		/// Example:
		/// KFIconOverride {}
		/// </summary>
		/// <param name="partToFix">part to fix</param>
		/// <remarks>This method uses code from xEvilReepersx's PartIconFixer.
		/// See https://bitbucket.org/xEvilReeperx/ksp_particonfixer/src/7f2ac4094c19?at=master for original code and license.</remarks>
		static void FixPartIcon(AvailablePart partToFix)
		{
			strClassName += ": Icon Fixer";
			KFLog.Log(string.Format("Fixing icon of \"{0}\"", partToFix.name));

			// Preparations
			GameObject partToFixIconPrefab = partToFix.iconPrefab;
			Bounds bounds = CalculateBounds(partToFixIconPrefab);

			// Retrieve icon fixes from cfg and calculate max part size
			float max = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

			float multiplier = 1f;
			if (HasKFIconOverrideMultiplier(partToFix))
				float.TryParse(partToFix.partConfig.GetNode("KFIconOverride").GetValue("Multiplier"), out multiplier);

			float factor = 40f / max * multiplier;
			factor /= 40f;

			string pivot = string.Empty;
			if (HasKFIconOverridePivot(partToFix))
				pivot = partToFix.partConfig.GetNode("KFIconOverride").GetValue("Pivot");

			Vector3 rotation = Vector3.zero; 
			if (HasKFIconOverrideRotation(partToFix))
				rotation = KSPUtil.ParseVector3(partToFix.partConfig.GetNode("KFIconOverride").GetValue("Rotation"));

			// Apply icon fixes
			partToFix.iconScale = max; // affects only the meter scale in the tooltip
            
			partToFixIconPrefab.transform.GetChild(0).localScale *= factor;
			partToFixIconPrefab.transform.GetChild(0).Rotate(rotation, Space.Self);

			// After applying the fixes the part could be off-center, correct this now
			if (string.IsNullOrEmpty(pivot))
			{
				Transform model = partToFixIconPrefab.transform.GetChild(0).Find("model");
				if (Equals(model, null))
					model = partToFixIconPrefab.transform.GetChild(0);

				Transform target = model.Find(pivot);
				if (!Equals(target, null))
					partToFixIconPrefab.transform.GetChild(0).position -= target.position;
			}
			else
				partToFixIconPrefab.transform.GetChild(0).localPosition = Vector3.zero;
		}
		
		/// <summary>Checks if a part velongs to Kerbal Foundries.</summary>
		/// <param name="part">part to check</param>
		/// <returns>true if the part's name starts with "KF."</returns>
		/// <remarks>KSP converts underscores in part names to a dot ("_" -> ".").</remarks>
		static bool IsAKFPart(AvailablePart part)
		{
			return part.name.StartsWith("KF.", System.StringComparison.Ordinal);
		}
		
		/// <summary>Checks if a part has an KFIconOverride node in it's config.</summary>
		/// <param name="part">Part to check.</param>
		/// <returns>True if a KFIconOverride node is there.</returns>
		static bool HasKFIconOverrideModule(AvailablePart part)
		{
			return part.partConfig.HasNode("KFIconOverride");
		}
		
		/// <summary>Checks if there's a Multiplier node in KFIconOverride.</summary>
		/// <param name="part">part to check</param>
		/// <returns>True if IconOverride->Multiplier exists</returns>
		static bool HasKFIconOverrideMultiplier(AvailablePart part)
		{
			return part.partConfig.GetNode("KFIconOverride").HasNode("Multiplier");
		}
		
		/// <summary>Checks if there's a Pivot node in KFIconOverride.</summary>
		/// <param name="part">Part to check.</param>
		/// <returns>True if KFIconOverride->Pivot exists.</returns>
		static bool HasKFIconOverridePivot(AvailablePart part)
		{
			return part.partConfig.GetNode("KFIconOverride").HasNode("Pivot");
		}
		
		/// <summary>Checks if there's a Rotation node in KFIconOverride.</summary>
		/// <param name="part">Part to check.</param>
		/// <returns>True if KFIconOverride->Rotation exists.</returns>
		static bool HasKFIconOverrideRotation(AvailablePart part)
		{
			return part.partConfig.GetNode("KFIconOverride").HasNode("Rotation");
		}
		
		/// <summary>Calculates the bounds of a game object.</summary>
		/// <param name="partGO">part which bounds have to be calculated</param>
		/// <returns>bounds</returns>
		/// <remarks>This code is copied from xEvilReepersx's PartIconFixer and is slightly modified.
		/// See https://bitbucket.org/xEvilReeperx/ksp_particonfixer/src/7f2ac4094c19?at=master for original code and license.</remarks>
		static Bounds CalculateBounds(GameObject partGO)
		{
			var renderers = new List<Renderer>(partGO.GetComponentsInChildren<Renderer>(true));
			
			if (Equals(renderers.Count, 0))
				return default(Bounds);
			
			var boundsList = new List<Bounds>();
			
			renderers.ForEach(thisRenderer =>
			{
				// disable once CanBeReplacedWithTryCastAndCheckForNull
				if (thisRenderer is SkinnedMeshRenderer)
				{
					var skinnedMeshRenderer = thisRenderer as SkinnedMeshRenderer;
					var thisMesh = new Mesh();
					skinnedMeshRenderer.BakeMesh(thisMesh);
					
					Matrix4x4 m = Matrix4x4.TRS(skinnedMeshRenderer.transform.position, skinnedMeshRenderer.transform.rotation, Vector3.one);
					var meshVertices = thisMesh.vertices;
					var skinnedMeshBounds = new Bounds(m.MultiplyPoint3x4(meshVertices[0]), Vector3.zero);
					
					for (int i = 1; i < meshVertices.Length; ++i)
						skinnedMeshBounds.Encapsulate(m.MultiplyPoint3x4(meshVertices[i]));

					Destroy(thisMesh);
					if (Equals(thisRenderer.tag, "Icon_Hidden"))
                        Destroy(thisRenderer);
					boundsList.Add(skinnedMeshBounds);
				}
				else if (thisRenderer is MeshRenderer)
				{
					thisRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
					boundsList.Add(thisRenderer.bounds);
				}
			});

			Bounds partBounds = boundsList[0];
			boundsList.RemoveAt(0);
			// disable ConvertClosureToMethodGroup
			boundsList.ForEach(b => partBounds.Encapsulate(b)); // Do not change that to boundsList.ForEach(bounds.Encapsulate)!

			return partBounds;
		}
		#endregion Part Icon Fix
	}
}
