﻿using UnityEngine;

namespace KerbalFoundries
{
	/// <summary>This class adds a button to the stock toolbar and displays a configuration window.</summary>
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class KFGUIManager : MonoBehaviour
	{
		#region Initialization
		// AppLauncher Elements.
		ApplicationLauncherButton appButton;
		Texture2D appTextureGrey;
		Texture2D appTextureColor;
		
		// Icon Constants
		const string strIconBasePath = "KerbalFoundries/Assets";
		const string strIconGrey = "KFIconGrey";
		const string strIconColor = "KFIconColor";
		
		// GUI Constants
		public Rect settingsRect;
		const int GUI_ID = 1200;
				
		// Boolean for the visible states of GUI elements.
		public static bool isGUIEnabled;
		
		/// <summary>Local name of the KFLogUtil class.</summary>
		readonly KFLogUtil KFLog = new KFLogUtil();
		/// <summary>Name of the class for logging purposes.</summary>
		public string strClassName = "KFGUIManager";
		
		#endregion Initialization
		
		#region Startup
		
		/// <summary>Called when the Behavior wakes up.</summary>
		void Awake()
		{
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor || HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				GameEvents.onGUIApplicationLauncherReady.Add(SetupAppButton);
                GameEvents.onGameSceneSwitchRequested.Add(OnSwitchScene);
			}
		}
		
		void OnSwitchScene(GameEvents.FromToAction<GameScenes, GameScenes> action)
		{
			KFLog.Log("Scene switch requested.", strClassName);
			KFPersistenceManager.SaveConfig();
			
			GameEvents.onGUIApplicationLauncherReady.Remove(SetupAppButton);
			GameEvents.onGameSceneSwitchRequested.Remove(OnSwitchScene);

			DestroyAppButton();
		}
		
		/// <summary>Retrieves button textures.</summary>
		void InitGUIElements()
		{
			appTextureGrey = GameDatabase.Instance.GetTexture(string.Format("{0}/{1}", strIconBasePath, strIconGrey), false);
			appTextureColor = GameDatabase.Instance.GetTexture(string.Format("{0}/{1}", strIconBasePath, strIconColor), false);
		}
		
		#endregion Startup
		
		#region AppLauncher Button
		
		/// <summary>Called when the AppLauncher reports that it is awake.</summary>
		void SetupAppButton()
		{
			InitGUIElements();
			if (Equals(appButton, null))
			{
				bool isThere;
				ApplicationLauncher.Instance.Contains(appButton, out isThere);
				if (isThere)
				{
					ApplicationLauncher.Instance.RemoveModApplication(appButton);
					KFLog.Log("Removed leftover app button.", strClassName);
				}
				
				appButton = ApplicationLauncher.Instance.AddModApplication(onTrue, onFalse, onHover, onNotHover, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.VAB, appTextureGrey);
				KFLog.Log("App button added.", strClassName);
			}
		}
		
		/// <summary>Called when the ApplicationLauncher gets destroyed.</summary>
		void DestroyAppButton()
		{
			if (!Equals(appButton, null))
			{
				ApplicationLauncher.Instance.RemoveModApplication(appButton);
				isGUIEnabled = false;
				KFLog.Log("App button destroyed.", strClassName);
			}
		}
		
		/// <summary>Called when the button is put into a "true" state, or when it is activated.</summary>
		void onTrue()
		{
			appButton.SetTexture(appTextureColor);
			isGUIEnabled = true;
		}
		
		/// <summary>Called when the button is in a "false" state.  Saves configuration.</summary>
		void onFalse()
		{
			appButton.SetTexture(appTextureGrey);
			isGUIEnabled = false;
			KFPersistenceManager.SaveConfig();
		}
		
		/// <summary>Called when the cursor enters a hovered state over the button.</summary>
		void onHover()
		{
			appButton.SetTexture(appTextureColor);
		}
		
		/// <summary>Called when the cursor leaves the hovering state over the button.</summary>
		void onNotHover()
		{
			if (!isGUIEnabled)
				appButton.SetTexture(appTextureGrey);
		}
		
		#endregion AppLauncher Button
		
		#region GUI Setup
		
		/// <summary>Called by Unity when it's time to draw the GUI.</summary>
		void OnGUI()
		{
			if (isGUIEnabled)
			{
				settingsRect = new Rect(Screen.width - 258f, 42f, 256f, 160f);
				GUI.Window(GUI_ID, settingsRect, DrawWindow, "Kerbal Foundries Settings");
			}
		}
		
		/// <summary>Creates the GUI content.</summary>
		/// <param name="windowID">ID of the window to create the content for.</param>
		void DrawWindow(int windowID)
		{
			GUI.skin = HighLogic.Skin;
			
			if (HighLogic.LoadedSceneIsFlight || Equals(HighLogic.LoadedScene, GameScenes.SPACECENTER))
			{
				KFPersistenceManager.isDustEnabled = GUI.Toggle(new Rect(8f, 24f, 240f, 24f), KFPersistenceManager.isDustEnabled, "Enable DustFX");
				if (KFPersistenceManager.isDustEnabled)
					KFPersistenceManager.isDustCameraEnabled = GUI.Toggle(new Rect(8f, 56f, 240f, 24f), KFPersistenceManager.isDustCameraEnabled, "Enable DustFX Camera");
			}
            
			if (HighLogic.LoadedSceneIsEditor || Equals(HighLogic.LoadedScene, GameScenes.SPACECENTER))
				KFPersistenceManager.isMarkerEnabled = GUI.Toggle(new Rect(8f, 88f, 240f, 24f), KFPersistenceManager.isMarkerEnabled, "Enable Orientation Markers");
			
			// The following button refuses to work no matter what I do.  It it exactly like the button in the BDArmory settings GUI, except for the rect postition and the commands it's told to execute.
			// There is absolutely NO reason for a Nullref when clicking it, but that's all I get!  It won't even execute the commands under the "if" statement.
			// It just appears and then refuses to do anything.
			//if (GUI.Button(new Rect(8f, 120f, 240f, 24f), "Save and Close"))
			//{
			//	appButton.SetFalse();
			//	KFLog.Debug("Save-button clicked, called \"appButton.SetFalse()\"", strClassName);
			//}
		}
		
		#endregion GUI Setup
	}
}
