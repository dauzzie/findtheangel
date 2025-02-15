using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.ShortcutManagement;

namespace UnityEditor.Experimental.TerrainAPI
{
    internal class ThermalErosionTool : TerrainPaintTool<ThermalErosionTool>
    {
#if UNITY_2019_1_OR_NEWER
        [Shortcut("Terrain/Select Thermal Erosion Tool", typeof(TerrainToolShortcutContext))]               // tells shortcut manager what to call the shortcut and what to pass as args
        static void SelectShortcut(ShortcutArguments args) {
            TerrainToolShortcutContext context = (TerrainToolShortcutContext)args.context;          // gets interface to modify state of TerrainTools
            context.SelectPaintTool<ThermalErosionTool>();                                                                        // set active tool
        }
#endif

        [SerializeField]
        IBrushUIGroup commonUI = new DefaultBrushUIGroup("ThermalErosion");

        Erosion.ThermalEroder m_Eroder = null;

        
        [SerializeField]
        private int m_MaterialPaintStrength = 50;

        [SerializeField]
        UnityEngine.TerrainLayer m_SelectedTerrainLayer = null;

        public override void OnEnable() {
            base.OnEnable();
            m_Eroder = new Erosion.ThermalEroder();
            m_Eroder.OnEnable();
        }

        public override void OnEnterToolMode() {
            base.OnEnterToolMode();
            commonUI.OnEnterToolMode();
        }

        public override void OnExitToolMode() {
            base.OnExitToolMode();
            commonUI.OnExitToolMode();
        }

        #region Resources
        Material m_Material = null;
        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("Hidden/TerrainTools/SimpleHeightBlend"));
            return m_Material;
        }

        Material m_SplatMaterial = null;
        Material GetSplatMaterial()
        {
            if (m_SplatMaterial == null)
                m_SplatMaterial = new Material(Shader.Find("Hidden/TerrainTools/SedimentSplat"));
            return m_SplatMaterial;
        }

        ComputeShader m_ComputeShader = null;
        ComputeShader GetComputeShader() {
            if (m_ComputeShader == null) {
                m_ComputeShader = (ComputeShader)Resources.Load("Hidden/TerrainTools/TerraceErosion");
            }
            return m_ComputeShader;
        }
        #endregion

        #region GUI
        public override string GetName()
        {
            return "Erosion/Thermal";
        }

        public override string GetDesc()
        {
            return "Thermal Erosion\n" +
                "Simulates thermal erosion from freezing / thawing processes, and the resulting avalanching of debris\n";
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

            using(IBrushRenderPreviewUnderCursor brushRender = new BrushRenderPreviewUIGroupUnderCursor(commonUI, "ThermalErosion", editContext.brushTexture))
            {
                if(brushRender.CalculateBrushTransform(out BrushTransform brushXform))
                {
                    PaintContext ctx = brushRender.AcquireHeightmap(false, brushXform.GetBrushXYBounds(), 1);
                
                    brushRender.RenderBrushPreview(ctx, TerrainPaintUtilityEditor.BrushPreview.SourceRenderTexture, brushXform, TerrainPaintUtilityEditor.GetDefaultBrushPreviewMaterial(), 0);
                }
            }
        }

        bool m_ShowControls = true;
        bool m_ShowAdvancedUI = false;
        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            EditorGUI.BeginChangeCheck();

            commonUI.OnInspectorGUI(terrain, editContext);

            m_ShowControls = TerrainToolGUIHelper.DrawHeaderFoldout(Erosion.Styles.m_ThermalErosionControls, m_ShowControls);
            if (m_ShowControls) {

                EditorGUILayout.BeginVertical("GroupBox");

                /*m_AffectHeight = EditorGUILayout.Toggle(Erosion.Styles.m_AffectHeight, m_AffectHeight);

                if (m_AffectHeight) {
                    m_AddHeightAmt = EditorGUILayout.IntSlider(Erosion.Styles.m_AddHeight, m_AddHeightAmt, 0, 100);
                }*/

                string[] matNames = {
                    "Custom",
                    "Dry Ash",    // 40
                    "Chalk",      // 45
                    "Dry Clay",   // 25 - 40
                    "Wet Clay",   // 15
                    "Soil",       // 30-45
                    "Granite Scree", // 35 - 40
                    "Gravel", //45 - 45
                    "Dry Sand",
                    "Wet Sand",
                    "Quick Sand",
                    "Snow"
                };

                float[,] tauValues = new float[,] {
                    { -1.0f, -1.0f },  //custom
                    { 38.0f, 42.0f },  //dry ash
                    { 45.0f, 45.0f },  //chalk
                    { 25.0f, 40.0f },  //dry clay (25 - 40)
                    { 15.0f, 15.0f },  //wet clay 
                    { 30.0f, 45.0f },  //soil (30-45)
                    { 35.0f, 40.0f },  //crushed granite (35 - 40) 
                    { 45.0f, 45.0f },  //gravel
                    { 32.0f, 36.0f },  //dry sand
                    { 45.0f, 45.0f },  //wet sand
                    { 15.0f, 30.0f },  //quicksand (15-30)
                    { 38.0f, 38.0f }   //snow
                };

                EditorGUI.BeginChangeCheck();
                m_Eroder.m_MatPreset = EditorGUILayout.Popup(Erosion.Styles.m_MatPreset, m_Eroder.m_MatPreset, matNames);
                if (EditorGUI.EndChangeCheck() && m_Eroder.m_MatPreset != 0) {
                    m_Eroder.m_AngleOfRepose.x = tauValues[m_Eroder.m_MatPreset, 0];
                    m_Eroder.m_AngleOfRepose.y = tauValues[m_Eroder.m_MatPreset, 1];
                }

                EditorGUI.indentLevel++;
                m_ShowAdvancedUI = EditorGUILayout.Foldout(m_ShowAdvancedUI, "Advanced");
                if (m_ShowAdvancedUI) {
                    m_Eroder.m_ThermalIterations = EditorGUILayout.IntSlider(Erosion.Styles.m_NumIterations, m_Eroder.m_ThermalIterations, 1, 1000);
                    m_Eroder.m_dt = EditorGUILayout.Slider(Erosion.Styles.m_TimeDelta, m_Eroder.m_dt, 0.00001f, 0.05f);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.MinMaxSlider(Erosion.Styles.m_AngleOfRepose, ref m_Eroder.m_AngleOfRepose.x, ref m_Eroder.m_AngleOfRepose.y, 0.0f, 90.0f);
                    if (EditorGUI.EndChangeCheck()) {
                        m_Eroder.m_MatPreset = 0; //we changed the angle of repose, so now we should switch the UI to "Custom"
                    }


                    m_Eroder.m_ReposeJitter = EditorGUILayout.IntSlider(Erosion.Styles.m_AngleOfReposeJitter, (int)m_Eroder.m_ReposeJitter, 0, 100);
                    //m_ReposeNoiseScale = EditorGUILayout.Slider("Material Variation Scale", m_ReposeNoiseScale, 0.0001f, 1.0f);
                    //m_ReposeNoiseAmount = EditorGUILayout.Slider("Material Variation Amount", m_ReposeNoiseAmount, 0.0f, 1.0f);
                }

                /*
                m_ShowMaterialUI = EditorGUILayout.Foldout(m_ShowMaterialUI, "Material");
                if (m_ShowMaterialUI) {
                    m_AffectMaterial = EditorGUILayout.Toggle(Erosion.Styles.m_AffectMaterial, m_AffectMaterial);
                    if (m_AffectMaterial) {
                        m_MaterialPaintStrength = EditorGUILayout.IntSlider(Erosion.Styles.m_MaterialPaintStrength, m_MaterialPaintStrength, -100, 100);

                        EditorGUI.BeginChangeCheck();

                        int layerIndex = TerrainPaintUtility.FindTerrainLayerIndex(terrain, m_SelectedTerrainLayer);
                        layerIndex = TerrainLayerUtility.ShowTerrainLayersSelectionHelper(terrain, layerIndex);
                        if (EditorGUI.EndChangeCheck()) {
                            if (layerIndex != -1)
                                m_SelectedTerrainLayer = terrain.terrainData.terrainLayers[layerIndex];

                            Save(true);
                        }
                    }
                }
                */
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck()) {
                Save(true);
            }
        }
        #endregion

        #region Paint
        private void PaintSplatMap(Terrain terrain, IOnPaint editContext, RenderTexture sedimentRT) {
            using(IBrushRenderUnderCursor brushRender = new BrushRenderUIGroupUnderCursor(commonUI, "ThermalErosion", editContext.brushTexture))
            {
                if(brushRender.CalculateBrushTransform(out BrushTransform brushXform))
                {
                    PaintContext paintContext = brushRender.AcquireTexture(true, brushXform.GetBrushXYBounds(), m_SelectedTerrainLayer);
                    if (paintContext == null)
                        return;

                    Material mat = GetSplatMaterial();

                    // apply brush
                    float splatStrength = Mathf.Lerp(-1000.0f, 1000.0f, 0.005f * (float)(m_MaterialPaintStrength + 100));
                    Vector4 brushParams = new Vector4(commonUI.brushStrength, splatStrength, 0.0f, 0.0f);
                    mat.SetTexture("_BrushTex", editContext.brushTexture);
                    mat.SetTexture("_MaskTex", sedimentRT);
                    mat.SetVector("_BrushParams", brushParams);

                    brushRender.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
                    brushRender.RenderBrush(paintContext, mat, 0);
                }
            }
        }

        private void AddHeight(Terrain terrain, IOnPaint editContext) {
            using(IBrushRenderUnderCursor brushRender = new BrushRenderUIGroupUnderCursor(commonUI, "ThermalErosion", editContext.brushTexture))
            {
                if(brushRender.CalculateBrushTransform(out BrushTransform brushXform))
                {
                    PaintContext paintContext = brushRender.AcquireHeightmap(true, brushXform.GetBrushXYBounds(), 1);
                
                    paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
                    Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
                    float brushStrength = Event.current.shift ? -m_Eroder.m_AddHeightAmt : m_Eroder.m_AddHeightAmt;
                    brushStrength *= (commonUI.brushStrength);

                    if(Event.current.control) { brushStrength = 0.0f; }

                    Vector4 brushParams = new Vector4(0.0001f * brushStrength, 0.0f, 0.0f, 0.0f);
                    mat.SetTexture("_BrushTex", editContext.brushTexture);
                    mat.SetVector("_BrushParams", brushParams);

                    brushRender.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
                    brushRender.RenderBrush(paintContext, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.RaiseLowerHeight);
                }
            }
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            commonUI.OnPaint(terrain, editContext);

            if(!commonUI.allowPaint) { return false; }

            ComputeShader cs = GetComputeShader();

            int[] numWorkGroups = { 8, 8, 1 };

            using(IBrushRenderUnderCursor brushRender = new BrushRenderUIGroupUnderCursor(commonUI, "ThermalErosion", editContext.brushTexture))
            {
                if(brushRender.CalculateBrushTransform(out BrushTransform brushXform))
                {
                    PaintContext paintContext = brushRender.AcquireHeightmap(true, brushXform.GetBrushXYBounds(), 1);
                    paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
    
                    //figure out what size we need our render targets to be
                    Rect brushRect = brushXform.GetBrushXYBounds();

                    m_Eroder.inputTextures["Height"] = paintContext.sourceRenderTexture;
                    
                    Vector2 texelSize = new Vector2(terrain.terrainData.size.x / terrain.terrainData.heightmapResolution,
                                                    terrain.terrainData.size.z / terrain.terrainData.heightmapResolution);
                    m_Eroder.ErodeHeightmap(terrain.terrainData.size, brushRect, texelSize);

                    Material mat = GetPaintMaterial();
                    Vector4 brushParams = new Vector4(commonUI.brushStrength, 0.0f, 0.0f, 0.0f);
                    mat.SetTexture("_BrushTex", editContext.brushTexture);
                    mat.SetTexture("_NewHeightTex", m_Eroder.outputTextures["Height"]);
                    mat.SetVector("_BrushParams", brushParams);

                    brushRender.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
                    brushRender.RenderBrush(paintContext, mat, 0);
                }
            }

            return true;
        }
        #endregion


        }
}
