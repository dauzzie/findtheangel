using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.ShortcutManagement;

namespace UnityEditor.Experimental.TerrainAPI
{
    //[FilePathAttribute("Library/TerrainTools/SetHeight", FilePathAttribute.Location.ProjectFolder)]
    internal class SetHeightTool : TerrainPaintTool<SetHeightTool>
    {
        private static bool s_showToolControls = true;

#if UNITY_2019_1_OR_NEWER
        [Shortcut("Terrain/Select Set Height Tool", typeof(TerrainToolShortcutContext))]
        static void SelectShortcut(ShortcutArguments args) {
            TerrainToolShortcutContext context = (TerrainToolShortcutContext)args.context;
            context.SelectPaintTool<SetHeightTool>();
        }
#endif

        [SerializeField]
        IBrushUIGroup commonUI = new DefaultBrushUIGroup("SetHeightTool");


        private static Material m_material;
        public static Material material
        {
            get
            {
                if(m_material == null)
                {
                    m_material = new Material(Shader.Find("Hidden/TerrainTools/SetExactHeight"));
                }

                return m_material;
            }
        }


        const string toolName = "Set Height";

        [SerializeField]
        float m_HeightWorldSpace;

        private static class Styles
        {
            public static readonly GUIContent controlHeader = EditorGUIUtility.TrTextContent("Set Height Controls");
            public static readonly GUIContent description = EditorGUIUtility.TrTextContent("Left click to set the height.\n\nHold Ctrl and left click to sample the target height.");
            public static readonly GUIContent flattenAll = EditorGUIUtility.TrTextContent("Flatten all", "If selected, it will traverse all neighbors and flatten them too");
            public static readonly GUIContent height = EditorGUIUtility.TrTextContent("Height", "You can set the Height property manually or you can Ctrl-click on the terrain to sample the height at the mouse position (rather like the 'eyedropper' tool in an image editor).");
            public static readonly GUIContent flatten = EditorGUIUtility.TrTextContent("Flatten", "The Flatten button levels the whole terrain to the chosen height.");
        }

        public override string GetName()
        {
            return toolName;
        }

        public override string GetDesc()
        {
            return Styles.description.text;
        }

        public override void OnEnterToolMode() {
            base.OnEnterToolMode();
            commonUI.OnEnterToolMode();
        }

        public override void OnExitToolMode() {
            base.OnExitToolMode();
            commonUI.OnExitToolMode();
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            commonUI.OnSceneGUI2D(terrain, editContext);

            // only do the rest if user mouse hits valid terrain or they are using the
            // brush parameter hotkeys to resize, etc
            if (!editContext.hitValidTerrain && !commonUI.isInUse)
            {
                return;
            }

            // update brush UI group
            commonUI.OnSceneGUI(terrain, editContext);

            // dont render preview if this isnt a repaint. losing performance if we do
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Texture brushTexture = editContext.brushTexture;
            
            using(IBrushRenderPreviewUnderCursor brushRender = new BrushRenderPreviewUIGroupUnderCursor(commonUI, "SetHeightTool", brushTexture))
            {
                if(brushRender.CalculateBrushTransform(out BrushTransform brushTransform))
                {
                    Rect brushBounds = brushTransform.GetBrushXYBounds();
                    PaintContext paintContext = brushRender.AcquireHeightmap(false, brushBounds, 1);
                    Material material = TerrainPaintUtilityEditor.GetDefaultBrushPreviewMaterial();

                    brushRender.RenderBrushPreview(paintContext, TerrainPaintUtilityEditor.BrushPreview.SourceRenderTexture, brushTransform, material, 0);

                    // draw result preview
                    {
                        ApplyBrushInternal(paintContext, brushRender, commonUI.brushStrength, brushTexture, brushTransform, terrain);

                        // restore old render target
                        RenderTexture.active = paintContext.oldRenderTexture;

                        material.SetTexture("_HeightmapOrig", paintContext.sourceRenderTexture);

                        brushRender.RenderBrushPreview(paintContext, TerrainPaintUtilityEditor.BrushPreview.DestinationRenderTexture, brushTransform, material, 1);
                    }
                }
            }
        }

        private void ApplyBrushInternal(PaintContext paintContext, IBrushRenderUnderCursor brushRender, float brushStrength, Texture brushTexture, BrushTransform brushTransform, Terrain terrain)
        {
            Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
            float terrainHeight = Mathf.Clamp01((m_HeightWorldSpace - terrain.transform.position.y) / terrain.terrainData.size.y);
            Vector4 brushParams = new Vector4(brushStrength * 0.01f, 0.5f * terrainHeight, 0.0f, 0.0f);
            
            mat.SetTexture("_BrushTex", brushTexture);
            mat.SetVector("_BrushParams", brushParams);

            brushRender.SetupTerrainToolMaterialProperties(paintContext, brushTransform, mat);
            brushRender.RenderBrush(paintContext, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SetHeights);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            commonUI.OnPaint(terrain, editContext);

            if(commonUI.allowPaint)
            {
                if(Event.current.control)
                {
                    Terrain currentTerrain = commonUI.terrainUnderCursor;
                    m_HeightWorldSpace = currentTerrain.terrainData.GetInterpolatedHeight(editContext.uv.x, editContext.uv.y) + currentTerrain.transform.position.y;
                    editContext.Repaint();
                    SaveSetting();
                    return true;
                }
                else
                {
                    Texture brushTexture = editContext.brushTexture;
                    
                    using(IBrushRenderUnderCursor brushRender = new BrushRenderUIGroupUnderCursor(commonUI, "SetHeightTool", brushTexture))
                    {
                        if(brushRender.CalculateBrushTransform(out BrushTransform brushTransform))
                        {
                            Rect brushBounds = brushTransform.GetBrushXYBounds();
                            PaintContext paintContext = brushRender.AcquireHeightmap(true, brushBounds);
                        
                            ApplyBrushInternal(paintContext, brushRender, commonUI.brushStrength, brushTexture, brushTransform, terrain);
                        }
                    }
                }
            }

            return true;
        }

        void Flatten(Terrain terrain)
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Set Height - Flatten Tile");

            RenderTexture heightmap = terrain.terrainData.heightmapTexture;
            
            Material mat = material;

            float terrainHeight = Mathf.Clamp01((m_HeightWorldSpace - terrain.transform.position.y) / terrain.terrainData.size.y);

            Vector4 brushParams = new Vector4(0, 0.5f * terrainHeight, 0.0f, 0.0f);
            mat.SetVector("_BrushParams", brushParams);

            RenderTexture temp = RenderTexture.GetTemporary(heightmap.descriptor);

            Graphics.Blit(temp, heightmap, mat, 1);

            RenderTexture.ReleaseTemporary(temp);

            terrain.terrainData.DirtyHeightmapRegion(new RectInt(0, 0, heightmap.width, heightmap.height), TerrainHeightmapSyncControl.HeightAndLod);
            terrain.terrainData.SyncHeightmap();
        }

        void FlattenAll(Terrain terrain)
        {
            Terrain[] terrains = TerrainFillUtility.GetTerrainsInGroup(terrain);

            for(int i = 0; i < terrains.Length; ++i)
            {
                Flatten(terrains[i]);
            }
        }
        bool m_initialized = false;
        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            if (!m_initialized)
            {
                LoadSettings();
                m_initialized = true;
            }
            EditorGUI.BeginChangeCheck();

            commonUI.OnInspectorGUI(terrain, editContext);

            s_showToolControls = TerrainToolGUIHelper.DrawHeaderFoldout(Styles.controlHeader, s_showToolControls);

            if (s_showToolControls)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                {
                    GUILayout.BeginHorizontal();
                    {
                        m_HeightWorldSpace = EditorGUILayout.Slider(Styles.height, m_HeightWorldSpace, 0, terrain.terrainData.size.y);
                        if (GUILayout.Button(Styles.flatten, GUILayout.ExpandWidth(false)))
                                Flatten(terrain);
                        if (GUILayout.Button(Styles.flattenAll, GUILayout.ExpandWidth(false)))
                            FlattenAll(terrain);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                SaveSetting();
                Save(true);
            }
        }

        private void SaveSetting()
        {
            EditorPrefs.SetFloat("Unity.TerrainTools.SetHeight.Height", m_HeightWorldSpace);

        }

        private void LoadSettings()
        {
            m_HeightWorldSpace = EditorPrefs.GetFloat("Unity.TerrainTools.SetHeight.Height", 0.0f);

        }
    }
}
