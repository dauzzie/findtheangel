﻿using System.IO;
using UnityEngine;

namespace UnityEditor.Experimental.TerrainAPI
{
	internal class TerrainToolboxWindow : EditorWindow
	{
#if UNITY_2019_1_OR_NEWER
		[MenuItem("Window/Terrain/Terrain Toolbox", false, 1)]
		static void CreateMangerWindow()
		{
			TerrainToolboxWindow window = GetWindow<TerrainToolboxWindow>("Terrain Toolbox");
			window.minSize = new Vector2(200, 150);
			window.Show();
		}
#endif

		TerrainManagerMode m_SelectedMode = TerrainManagerMode.CreateTerrain;

		enum TerrainManagerMode
		{
			CreateTerrain = 0,
			Settings = 1,
			Utilities = 2
		}

		TerrainToolboxCreateTerrain m_CreateTerrainMode;
		TerrainToolboxSettings m_TerrainSettingsMode;
		TerrainToolboxUtilities m_TerrainUtilitiesMode;

		const string PrefName = "TerrainToolbox.Window.Mode";

		static class Styles
		{
			public static readonly GUIContent[] ModeToggles =
			{
				EditorGUIUtility.TrTextContent("Create New Terrain"),
				EditorGUIUtility.TrTextContent("Terrain Settings"),
				EditorGUIUtility.TrTextContent("Terrain Utilities")
			};

			public static readonly GUIStyle ButtonStyle = "LargeButton";
		}

		public void OnEnable()
		{
			m_CreateTerrainMode = new TerrainToolboxCreateTerrain();
			m_TerrainSettingsMode = new TerrainToolboxSettings();
			m_TerrainUtilitiesMode = new TerrainToolboxUtilities();
			
			m_CreateTerrainMode.LoadSettings();
			m_TerrainSettingsMode.LoadSettings();
			m_TerrainUtilitiesMode.LoadSettings();
			m_TerrainUtilitiesMode.OnLoad();
			LoadSettings();
		}

		public void OnDisable()
		{
			m_CreateTerrainMode.SaveSettings();
			m_TerrainSettingsMode.SaveSettings();
			m_TerrainUtilitiesMode.SaveSettings();
			SaveSettings();
		}

		public void OnGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();

			ToggleManagerMode();

			//if (GUILayout.Button("Wiki", GUILayout.Width(60)))
			//{
			//	System.Diagnostics.Process.Start("https://github.com/Unity-Technologies/TerrainTools/wiki");
			//}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			switch (m_SelectedMode)
			{
				case TerrainManagerMode.CreateTerrain:
					m_CreateTerrainMode.OnGUI();
					break;

				case TerrainManagerMode.Settings:
					m_TerrainSettingsMode.OnGUI();
					break;

				case TerrainManagerMode.Utilities:
					m_TerrainUtilitiesMode.OnGUI();
					break;
			}
		}

		void ToggleManagerMode()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			EditorGUI.BeginChangeCheck();
			m_SelectedMode = (TerrainManagerMode)GUILayout.Toolbar((int)m_SelectedMode, Styles.ModeToggles, Styles.ButtonStyle, GUI.ToolbarButtonSize.FitToContents);
			if (EditorGUI.EndChangeCheck())
			{
				GUIUtility.keyboardControl = 0;
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		void SaveSettings()
		{
			string filePath = ToolboxHelper.GetPrefFilePath(ToolboxHelper.ToolboxPrefsWindow);
			File.WriteAllText(filePath, ((int)m_SelectedMode).ToString());
		}

		void LoadSettings()
		{
			string filePath = ToolboxHelper.GetPrefFilePath(ToolboxHelper.ToolboxPrefsWindow);
			if (File.Exists(filePath))
			{
				string windowSettingsData = File.ReadAllText(filePath);
				int value = 0;
				if (int.TryParse(windowSettingsData, out value))
				{
					m_SelectedMode = (TerrainManagerMode)value;
				}				
			}
		}
	}
}
