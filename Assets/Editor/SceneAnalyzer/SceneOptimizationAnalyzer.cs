#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SceneOptimizationAnalyzer : EditorWindow
{
    // ============================================================
    // DATA
    // ============================================================

    private class SceneReport
    {
        public string sceneName;

        public int totalObjects;
        public int activeObjects;
        public int inactiveObjects;

        public int meshRenderers;
        public int skinnedMeshRenderers;
        public int meshFilters;

        public long vertices;
        public long triangles;
        public long subMeshes;

        public int materials;
        public int uniqueMaterials;

        public int textures;
        public long textureMemory;
        public int texturesOver2K;
        public int texturesOver4K;

        public int lights;
        public int realtimeLights;
        public int mixedLights;
        public int bakedLights;
        public int shadowCastingLights;

        public int cameras;
        public int audioSources;
        public int particleSystems;

        public int rigidbodies;
        public int colliders;
        public int canvasCount;
        public int uiGraphics;

        public int scripts;
        public int missingScripts;

        public int staticObjects;

        public int reflectionProbes;
        public int lightProbes;

        public int deepHierarchyObjects;
        public int maximumHierarchyDepth;

        public int transparentMaterials;
        public int emissionMaterials;

        public int skinnedMeshesWithShadows;
        public int meshesWithShadows;

        public int disabledComponents;

        public float score;

        public List<string> criticalProblems = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> recommendations = new List<string>();
        public List<string> positive = new List<string>();
    }

    // ============================================================
    // WINDOW
    // ============================================================

    private Vector2 scroll;
    private SceneReport report;

    private bool showScene = true;
    private bool showGeometry = true;
    private bool showTextures = true;
    private bool showLighting = true;
    private bool showPhysics = true;
    private bool showUI = true;
    private bool showScripts = true;
    private bool showHierarchy = true;
    private bool showProblems = true;
    private bool showRecommendations = true;

    [MenuItem("Tools/Scene Optimization Analyzer")]
    public static void OpenWindow()
    {
        SceneOptimizationAnalyzer window =
            GetWindow<SceneOptimizationAnalyzer>("Scene Analyzer");

        window.minSize = new Vector2(700, 650);
        window.Show();
    }

    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("ANALYZE SCENE", GUILayout.Height(40)))
        {
            AnalyzeScene();
        }

        GUI.enabled = report != null;

        if (GUILayout.Button("EXPORT REPORT", GUILayout.Height(40)))
        {
            ExportReport();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        if (report == null)
        {
            EditorGUILayout.HelpBox(
                "Натисніть ANALYZE SCENE, щоб виконати повний аналіз відкритої сцени.",
                MessageType.Info
            );

            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawScore();

        EditorGUILayout.Space(10);

        DrawSceneSection();
        DrawGeometrySection();
        DrawTextureSection();
        DrawLightingSection();
        DrawPhysicsSection();
        DrawUISection();
        DrawScriptsSection();
        DrawHierarchySection();
        DrawProblemsSection();
        DrawRecommendationsSection();

        EditorGUILayout.Space(20);

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        GUIStyle title = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label("SCENE OPTIMIZATION ANALYZER", title);

        GUIStyle subtitle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label(
            "Аналіз оптимізації Unity-сцени без сторонніх пакетів",
            subtitle
        );
    }

    // ============================================================
    // SCORE
    // ============================================================

    private void DrawScore()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle scoreStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 36,
            alignment = TextAnchor.MiddleCenter
        };

        Color oldColor = GUI.color;

        if (report.score >= 90)
            GUI.color = Color.green;
        else if (report.score >= 75)
            GUI.color = new Color(0.5f, 1f, 0.5f);
        else if (report.score >= 60)
            GUI.color = Color.yellow;
        else if (report.score >= 40)
            GUI.color = new Color(1f, 0.65f, 0.2f);
        else
            GUI.color = Color.red;

        GUILayout.Label(
            Mathf.RoundToInt(report.score) + " / 100",
            scoreStyle
        );

        GUI.color = oldColor;

        string status;

        if (report.score >= 90)
            status = "ВІДМІННО";
        else if (report.score >= 75)
            status = "ДОБРЕ";
        else if (report.score >= 60)
            status = "ПОТРЕБУЄ ОПТИМІЗАЦІЇ";
        else if (report.score >= 40)
            status = "ПРОБЛЕМНА СЦЕНА";
        else
            status = "КРИТИЧНА ОПТИМІЗАЦІЯ";

        GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label(status, statusStyle);

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // SECTIONS
    // ============================================================

    private void DrawSceneSection()
    {
        showScene = EditorGUILayout.BeginFoldoutHeaderGroup(showScene, "📦 SCENE");
        try
        {
            if (showScene)
            {
                BeginBox();
                Row("Scene", report.sceneName);
                Row("GameObjects", report.totalObjects);
                Row("Active Objects", report.activeObjects);
                Row("Inactive Objects", report.inactiveObjects);
                Row("Static Objects", report.staticObjects);
                EndBox();
            }
        }
        finally
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    private void DrawGeometrySection()
    {
        showGeometry = EditorGUILayout.BeginFoldoutHeaderGroup(
            showGeometry,
            "🔺 GEOMETRY / RENDERING"
        );

        if (showGeometry)
        {
            BeginBox();

            Row("Mesh Renderers", report.meshRenderers);
            Row("Skinned Mesh Renderers", report.skinnedMeshRenderers);
            Row("Mesh Filters", report.meshFilters);

            Row("Vertices", FormatNumber(report.vertices));
            Row("Triangles", FormatNumber(report.triangles));
            Row("Sub Meshes", report.subMeshes);

            Row("Materials", report.materials);
            Row("Unique Materials", report.uniqueMaterials);

            Row("Mesh Shadow Casters", report.meshesWithShadows);
            Row(
                "Skinned Shadow Casters",
                report.skinnedMeshesWithShadows
            );

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTextureSection()
    {
        showTextures = EditorGUILayout.BeginFoldoutHeaderGroup(
            showTextures,
            "🖼 TEXTURES / MATERIALS"
        );

        if (showTextures)
        {
            BeginBox();

            Row("Textures", report.textures);

            Row(
                "Estimated Texture Memory",
                FormatMemory(report.textureMemory)
            );

            Row("Textures > 2048", report.texturesOver2K);
            Row("Textures > 4096", report.texturesOver4K);

            Row("Transparent Materials", report.transparentMaterials);
            Row("Emission Materials", report.emissionMaterials);

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawLightingSection()
    {
        showLighting = EditorGUILayout.BeginFoldoutHeaderGroup(
            showLighting,
            "💡 LIGHTING"
        );

        if (showLighting)
        {
            BeginBox();

            Row("Lights", report.lights);
            Row("Realtime Lights", report.realtimeLights);
            Row("Mixed Lights", report.mixedLights);
            Row("Baked Lights", report.bakedLights);
            Row("Shadow Casting Lights", report.shadowCastingLights);

            Row("Reflection Probes", report.reflectionProbes);
            Row("Light Probes", report.lightProbes);

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPhysicsSection()
    {
        showPhysics = EditorGUILayout.BeginFoldoutHeaderGroup(
            showPhysics,
            "⚙ PHYSICS"
        );

        if (showPhysics)
        {
            BeginBox();

            Row("Rigidbody", report.rigidbodies);
            Row("Colliders", report.colliders);
            Row("Audio Sources", report.audioSources);
            Row("Particle Systems", report.particleSystems);

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawUISection()
    {
        showUI = EditorGUILayout.BeginFoldoutHeaderGroup(
            showUI,
            "🖥 UI"
        );

        if (showUI)
        {
            BeginBox();

            Row("Canvas", report.canvasCount);
            Row("UI Graphics", report.uiGraphics);

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawScriptsSection()
    {
        showScripts = EditorGUILayout.BeginFoldoutHeaderGroup(
            showScripts,
            "💻 SCRIPTS / COMPONENTS"
        );

        if (showScripts)
        {
            BeginBox();

            Row("MonoBehaviours", report.scripts);
            Row("Missing Scripts", report.missingScripts);
            Row("Disabled Components", report.disabledComponents);

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawHierarchySection()
    {
        showHierarchy = EditorGUILayout.BeginFoldoutHeaderGroup(
            showHierarchy,
            "🌳 HIERARCHY"
        );

        if (showHierarchy)
        {
            BeginBox();

            Row(
                "Maximum Hierarchy Depth",
                report.maximumHierarchyDepth
            );

            Row(
                "Objects with Deep Hierarchy",
                report.deepHierarchyObjects
            );

            EndBox();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // ============================================================
    // PROBLEMS
    // ============================================================

    private void DrawProblemsSection()
    {
        showProblems = EditorGUILayout.BeginFoldoutHeaderGroup(
            showProblems,
            "🚨 PROBLEMS & WARNINGS"
        );

        if (showProblems)
        {
            if (report.criticalProblems.Count == 0 &&
                report.warnings.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Критичних проблем не знайдено.",
                    MessageType.Info
                );
            }

            foreach (string problem in report.criticalProblems)
            {
                EditorGUILayout.HelpBox(
                    problem,
                    MessageType.Error
                );
            }

            foreach (string warning in report.warnings)
            {
                EditorGUILayout.HelpBox(
                    warning,
                    MessageType.Warning
                );
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // ============================================================
    // RECOMMENDATIONS
    // ============================================================

    private void DrawRecommendationsSection()
    {
        showRecommendations = EditorGUILayout.BeginFoldoutHeaderGroup(
            showRecommendations,
            "💡 RECOMMENDATIONS"
        );

        if (showRecommendations)
        {
            if (report.positive.Count > 0)
            {
                EditorGUILayout.LabelField(
                    "ПОЗИТИВНІ МОМЕНТИ",
                    EditorStyles.boldLabel
                );

                foreach (string item in report.positive)
                {
                    EditorGUILayout.HelpBox(
                        "✓ " + item,
                        MessageType.Info
                    );
                }
            }

            if (report.recommendations.Count > 0)
            {
                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField(
                    "ЩО ПОКРАЩИТИ",
                    EditorStyles.boldLabel
                );

                foreach (string item in report.recommendations)
                {
                    EditorGUILayout.HelpBox(
                        "→ " + item,
                        MessageType.Warning
                    );
                }
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // ============================================================
    // ANALYSIS
    // ============================================================

    private void AnalyzeScene()
    {
        if (UnityEditor.SceneManagement.EditorSceneManager
            .GetActiveScene().isLoaded == false)
        {
            EditorUtility.DisplayDialog(
                "Scene Analyzer",
                "Немає відкритої сцени.",
                "OK"
            );

            return;
        }

        report = new SceneReport();

        var scene =
            UnityEditor.SceneManagement.EditorSceneManager
            .GetActiveScene();

        report.sceneName = scene.name;

        GameObject[] roots = scene.GetRootGameObjects();

        List<GameObject> objects = new List<GameObject>();

        foreach (GameObject root in roots)
        {
            AddHierarchy(root, objects, 0);
        }

        report.totalObjects = objects.Count;

        foreach (GameObject go in objects)
        {
            if (go.activeInHierarchy)
                report.activeObjects++;
            else
                report.inactiveObjects++;

            AnalyzeObject(go);
        }

        CalculateScore();

        GenerateRecommendations();

        Repaint();

        Debug.Log(
            "[Scene Optimization Analyzer] Analysis completed. " +
            "Score: " + Mathf.RoundToInt(report.score) + "/100"
        );
    }

    private void AddHierarchy(
        GameObject root,
        List<GameObject> list,
        int depth)
    {
        list.Add(root);

        report.maximumHierarchyDepth =
            Mathf.Max(
                report.maximumHierarchyDepth,
                depth
            );

        if (depth >= 8)
            report.deepHierarchyObjects++;

        foreach (Transform child in root.transform)
        {
            AddHierarchy(
                child.gameObject,
                list,
                depth + 1
            );
        }
    }

    // ============================================================
    // OBJECT ANALYSIS
    // ============================================================

    private void AnalyzeObject(GameObject go)
    {
        if (go.isStatic)
            report.staticObjects++;

        Component[] components =
            go.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
            {
                report.missingScripts++;
                continue;
            }

            Behaviour behaviour =
                component as Behaviour;

            if (behaviour != null &&
                !behaviour.enabled)
            {
                report.disabledComponents++;
            }

            MeshRenderer meshRenderer =
                component as MeshRenderer;

            if (meshRenderer != null)
            {
                AnalyzeMeshRenderer(meshRenderer);
                continue;
            }

            SkinnedMeshRenderer skinned =
                component as SkinnedMeshRenderer;

            if (skinned != null)
            {
                AnalyzeSkinnedRenderer(skinned);
                continue;
            }

            MeshFilter meshFilter =
                component as MeshFilter;

            if (meshFilter != null)
            {
                report.meshFilters++;

                if (meshFilter.sharedMesh != null)
                {
                    AnalyzeMesh(
                        meshFilter.sharedMesh
                    );
                }

                continue;
            }

            Light light = component as Light;

            if (light != null)
            {
                AnalyzeLight(light);
                continue;
            }

            Camera camera = component as Camera;

            if (camera != null)
            {
                report.cameras++;
                continue;
            }

            Rigidbody rb =
                component as Rigidbody;

            if (rb != null)
            {
                report.rigidbodies++;
                continue;
            }

            Collider collider =
                component as Collider;

            if (collider != null)
            {
                report.colliders++;
                continue;
            }

            ParticleSystem particles =
                component as ParticleSystem;

            if (particles != null)
            {
                report.particleSystems++;
                continue;
            }

            AudioSource audio =
                component as AudioSource;

            if (audio != null)
            {
                report.audioSources++;
                continue;
            }

            Canvas canvas =
                component as Canvas;

            if (canvas != null)
            {
                report.canvasCount++;
                continue;
            }

            Graphic graphic =
                component as Graphic;

            if (graphic != null)
            {
                report.uiGraphics++;
                continue;
            }

            ReflectionProbe probe =
                component as ReflectionProbe;

            if (probe != null)
            {
                report.reflectionProbes++;
                continue;
            }

            LightProbeGroup lightProbe =
                component as LightProbeGroup;

            if (lightProbe != null)
            {
                report.lightProbes++;
                continue;
            }

            MonoBehaviour script =
                component as MonoBehaviour;

            if (script != null)
            {
                report.scripts++;
            }
        }
    }

    // ============================================================
    // MESH ANALYSIS
    // ============================================================

    private void AnalyzeMeshRenderer(
        MeshRenderer renderer)
    {
        report.meshRenderers++;

        if (renderer.shadowCastingMode !=
            ShadowCastingMode.Off)
        {
            report.meshesWithShadows++;
        }

        AnalyzeMaterials(renderer.sharedMaterials);
    }

    private void AnalyzeSkinnedRenderer(
        SkinnedMeshRenderer renderer)
    {
        report.skinnedMeshRenderers++;

        if (renderer.shadowCastingMode !=
            ShadowCastingMode.Off)
        {
            report.skinnedMeshesWithShadows++;
        }

        if (renderer.sharedMesh != null)
        {
            AnalyzeMesh(renderer.sharedMesh);
        }

        AnalyzeMaterials(renderer.sharedMaterials);
    }

    private void AnalyzeMesh(Mesh mesh)
    {
        if (mesh == null)
            return;

        report.vertices += mesh.vertexCount;

        int[] indices = mesh.triangles;

        if (indices != null)
            report.triangles += indices.Length / 3;

        report.subMeshes += mesh.subMeshCount;
    }

    // ============================================================
    // MATERIAL ANALYSIS
    // ============================================================

    private void AnalyzeMaterials(
        Material[] materials)
    {
        if (materials == null)
            return;

        foreach (Material material in materials)
        {
            if (material == null)
                continue;

            report.materials++;

            if (material.HasProperty("_Mode"))
            {
                float mode =
                    material.GetFloat("_Mode");

                if (mode != 0)
                    report.transparentMaterials++;
            }

            if (material.HasProperty("_Surface"))
            {
                float surface =
                    material.GetFloat("_Surface");

                if (surface != 0)
                    report.transparentMaterials++;
            }

            if (material.IsKeywordEnabled("_EMISSION"))
            {
                report.emissionMaterials++;
            }

            AnalyzeMaterialTextures(material);
        }

        report.uniqueMaterials =
            CountUniqueMaterials();
    }

    private HashSet<Material> GetAllMaterials()
    {
        HashSet<Material> result = new HashSet<Material>();

        Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.gameObject.scene.name == null)
                continue;

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material != null)
                    result.Add(material);
            }
        }

        return result;
    }

    private int CountUniqueMaterials()
    {
        return GetAllMaterials().Count;
    }

    // ============================================================
    // TEXTURE ANALYSIS
    // ============================================================

    private void AnalyzeMaterialTextures(
        Material material)
    {
        Shader shader = material.shader;

        if (shader == null)
            return;

        int propertyCount =
            shader.GetPropertyCount();

        for (int i = 0; i < propertyCount; i++)
        {
            if (shader.GetPropertyType(i) !=
                ShaderPropertyType.Texture)
            {
                continue;
            }

            string propertyName =
                shader.GetPropertyName(i);

            Texture texture =
                material.GetTexture(propertyName);

            Texture2D texture2D =
                texture as Texture2D;

            if (texture2D == null)
                continue;

            report.textures++;

            int width = texture2D.width;
            int height = texture2D.height;

            if (width > 2048 || height > 2048)
                report.texturesOver2K++;

            if (width > 4096 || height > 4096)
                report.texturesOver4K++;

            report.textureMemory +=
                EstimateTextureMemory(texture2D);
        }
    }

    private long EstimateTextureMemory(
        Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        int bytesPerPixel = 4;

        if (texture.format == TextureFormat.RGB24)
            bytesPerPixel = 3;

        if (texture.format == TextureFormat.RGBA32)
            bytesPerPixel = 4;

        if (texture.format == TextureFormat.ARGB32)
            bytesPerPixel = 4;

        if (texture.format == TextureFormat.R8)
            bytesPerPixel = 1;

        return (long)width *
               height *
               bytesPerPixel;
    }

    // ============================================================
    // LIGHT ANALYSIS
    // ============================================================

    private void AnalyzeLight(Light light)
    {
        report.lights++;

        if (light.shadows != LightShadows.None)
            report.shadowCastingLights++;

        switch (light.lightmapBakeType)
        {
            case LightmapBakeType.Realtime:
                report.realtimeLights++;
                break;

            case LightmapBakeType.Mixed:
                report.mixedLights++;
                break;

            case LightmapBakeType.Baked:
                report.bakedLights++;
                break;
        }
    }

    // ============================================================
    // SCORE
    // ============================================================

    private void CalculateScore()
    {
        float score = 100f;

        // Geometry
        if (report.triangles > 500000)
            score -= 5;

        if (report.triangles > 1000000)
            score -= 7;

        if (report.triangles > 3000000)
            score -= 10;

        if (report.triangles > 5000000)
            score -= 15;

        // Objects
        if (report.totalObjects > 1000)
            score -= 3;

        if (report.totalObjects > 2500)
            score -= 5;

        if (report.totalObjects > 5000)
            score -= 10;

        if (report.totalObjects > 10000)
            score -= 15;

        // Materials
        if (report.uniqueMaterials > 100)
            score -= 3;

        if (report.uniqueMaterials > 250)
            score -= 5;

        if (report.uniqueMaterials > 500)
            score -= 8;

        // Textures
        if (report.texturesOver2K > 10)
            score -= 3;

        if (report.texturesOver4K > 0)
            score -= 5;

        if (report.texturesOver4K > 10)
            score -= 5;

        // Lighting
        if (report.realtimeLights > 5)
            score -= 4;

        if (report.realtimeLights > 10)
            score -= 6;

        if (report.shadowCastingLights > 10)
            score -= 4;

        // Physics
        if (report.rigidbodies > 50)
            score -= 3;

        if (report.rigidbodies > 150)
            score -= 5;

        if (report.colliders > 300)
            score -= 3;

        if (report.colliders > 700)
            score -= 6;

        // Particles
        if (report.particleSystems > 30)
            score -= 4;

        if (report.particleSystems > 100)
            score -= 6;

        // UI
        if (report.canvasCount > 10)
            score -= 3;

        if (report.uiGraphics > 300)
            score -= 4;

        // Scripts
        if (report.scripts > 200)
            score -= 3;

        if (report.scripts > 500)
            score -= 5;

        // Missing scripts
        if (report.missingScripts > 0)
            score -= 5;

        if (report.missingScripts > 5)
            score -= 10;

        // Hierarchy
        if (report.maximumHierarchyDepth > 8)
            score -= 3;

        if (report.maximumHierarchyDepth > 15)
            score -= 5;

        // Clamp
        report.score =
            Mathf.Clamp(score, 0f, 100f);
    }

    // ============================================================
    // RECOMMENDATIONS
    // ============================================================

    private void GenerateRecommendations()
    {
        // --------------------------------------------------------
        // MISSING SCRIPTS
        // --------------------------------------------------------

        if (report.missingScripts > 0)
        {
            report.criticalProblems.Add(
                "Знайдено " +
                report.missingScripts +
                " Missing Script."
            );

            report.recommendations.Add(
                "Видаліть Missing Scripts або відновіть необхідні компоненти."
            );
        }
        else
        {
            report.positive.Add(
                "Missing Scripts не знайдено."
            );
        }

        // --------------------------------------------------------
        // TRIANGLES
        // --------------------------------------------------------

        if (report.triangles > 500000)
        {
            report.warnings.Add(
                "Кількість трикутників перевищує 500 000: " +
                FormatNumber(report.triangles)
            );

            report.recommendations.Add(
                "Перевірте найважчі моделі, використовуйте LOD, " +
                "decimation та оптимізовані mesh."
            );
        }
        else
        {
            report.positive.Add(
                "Геометричне навантаження перебуває в прийнятному діапазоні."
            );
        }

        // --------------------------------------------------------
        // OBJECTS
        // --------------------------------------------------------

        if (report.totalObjects > 2500)
        {
            report.warnings.Add(
                "У сцені дуже багато GameObject: " +
                report.totalObjects
            );

            report.recommendations.Add(
                "Об'єднайте дрібні статичні об'єкти там, де це доцільно, " +
                "та використовуйте batching/LOD."
            );
        }

        // --------------------------------------------------------
        // MATERIALS
        // --------------------------------------------------------

        if (report.uniqueMaterials > 250)
        {
            report.warnings.Add(
                "Велика кількість унікальних матеріалів: " +
                report.uniqueMaterials
            );

            report.recommendations.Add(
                "Зменште кількість матеріалів та використовуйте texture atlases."
            );
        }

        // --------------------------------------------------------
        // TEXTURES
        // --------------------------------------------------------

        if (report.texturesOver4K > 0)
        {
            report.warnings.Add(
                "Знайдено " +
                report.texturesOver4K +
                " текстур із роздільністю понад 4096 px."
            );

            report.recommendations.Add(
                "Перевірте, чи дійсно потрібні текстури 8K/16K."
            );
        }

        if (report.texturesOver2K > 20)
        {
            report.recommendations.Add(
                "Оптимізуйте великі текстури та використовуйте mipmaps."
            );
        }

        // --------------------------------------------------------
        // LIGHTS
        // --------------------------------------------------------

        if (report.realtimeLights > 10)
        {
            report.warnings.Add(
                "Велика кількість Realtime Lights: " +
                report.realtimeLights
            );

            report.recommendations.Add(
                "Розгляньте можливість перевести частину освітлення " +
                "на Baked/Mixed."
            );
        }

        if (report.shadowCastingLights > 10)
        {
            report.warnings.Add(
                "Багато джерел світла створюють тіні: " +
                report.shadowCastingLights
            );

            report.recommendations.Add(
                "Вимкніть тіні для другорядних джерел світла."
            );
        }

        // --------------------------------------------------------
        // PHYSICS
        // --------------------------------------------------------

        if (report.rigidbodies > 150)
        {
            report.warnings.Add(
                "У сцені більше 150 Rigidbody."
            );

            report.recommendations.Add(
                "Перевірте, чи всі Rigidbody необхідні та чи можна " +
                "використовувати кінематичні об'єкти."
            );
        }

        if (report.colliders > 700)
        {
            report.warnings.Add(
                "У сцені дуже багато Collider."
            );

            report.recommendations.Add(
                "Замініть складні MeshCollider на прості Box/Capsule/Sphere Collider, " +
                "де це можливо."
            );
        }

        // --------------------------------------------------------
        // PARTICLES
        // --------------------------------------------------------

        if (report.particleSystems > 100)
        {
            report.warnings.Add(
                "Дуже багато Particle Systems: " +
                report.particleSystems
            );

            report.recommendations.Add(
                "Перевірте Particle Systems та їхню максимальну кількість частинок."
            );
        }

        // --------------------------------------------------------
        // UI
        // --------------------------------------------------------

        if (report.canvasCount > 10)
        {
            report.warnings.Add(
                "У сцені багато Canvas: " +
                report.canvasCount
            );

            report.recommendations.Add(
                "Об'єднайте UI там, де це можливо, та перевірте Canvas rebuild."
            );
        }

        // --------------------------------------------------------
        // HIERARCHY
        // --------------------------------------------------------

        if (report.maximumHierarchyDepth > 15)
        {
            report.warnings.Add(
                "Максимальна глибина ієрархії: " +
                report.maximumHierarchyDepth
            );

            report.recommendations.Add(
                "Спростіть надто глибокі Transform hierarchy."
            );
        }

        // --------------------------------------------------------
        // GENERAL
        // --------------------------------------------------------

        if (report.recommendations.Count == 0)
        {
            report.positive.Add(
                "Сценарій не має очевидних проблем за встановленими евристиками."
            );
        }
    }

    // ============================================================
    // EXPORT
    // ============================================================

    private void ExportReport()
    {
        if (report == null)
            return;

        string path =
            EditorUtility.SaveFilePanel(
                "Export Scene Optimization Report",
                "",
                report.sceneName +
                "_Optimization_Report.txt",
                "txt"
            );

        if (string.IsNullOrEmpty(path))
            return;

        StringBuilder sb =
            new StringBuilder();

        sb.AppendLine(
            "UNITY SCENE OPTIMIZATION REPORT"
        );

        sb.AppendLine(
            "================================"
        );

        sb.AppendLine();

        sb.AppendLine(
            "Scene: " + report.sceneName
        );

        sb.AppendLine(
            "Optimization Score: " +
            Mathf.RoundToInt(report.score) +
            "/100"
        );

        sb.AppendLine();

        sb.AppendLine(
            "SCENE"
        );

        sb.AppendLine(
            "GameObjects: " +
            report.totalObjects
        );

        sb.AppendLine(
            "Active Objects: " +
            report.activeObjects
        );

        sb.AppendLine(
            "Inactive Objects: " +
            report.inactiveObjects
        );

        sb.AppendLine(
            "Static Objects: " +
            report.staticObjects
        );

        sb.AppendLine();

        sb.AppendLine(
            "GEOMETRY"
        );

        sb.AppendLine(
            "Mesh Renderers: " +
            report.meshRenderers
        );

        sb.AppendLine(
            "Skinned Mesh Renderers: " +
            report.skinnedMeshRenderers
        );

        sb.AppendLine(
            "Vertices: " +
            FormatNumber(report.vertices)
        );

        sb.AppendLine(
            "Triangles: " +
            FormatNumber(report.triangles)
        );

        sb.AppendLine(
            "Sub Meshes: " +
            report.subMeshes
        );

        sb.AppendLine();

        sb.AppendLine(
            "MATERIALS / TEXTURES"
        );

        sb.AppendLine(
            "Materials: " +
            report.materials
        );

        sb.AppendLine(
            "Unique Materials: " +
            report.uniqueMaterials
        );

        sb.AppendLine(
            "Textures: " +
            report.textures
        );

        sb.AppendLine(
            "Estimated Texture Memory: " +
            FormatMemory(report.textureMemory)
        );

        sb.AppendLine(
            "Textures > 2K: " +
            report.texturesOver2K
        );

        sb.AppendLine(
            "Textures > 4K: " +
            report.texturesOver4K
        );

        sb.AppendLine();

        sb.AppendLine(
            "LIGHTING"
        );

        sb.AppendLine(
            "Lights: " +
            report.lights
        );

        sb.AppendLine(
            "Realtime Lights: " +
            report.realtimeLights
        );

        sb.AppendLine(
            "Mixed Lights: " +
            report.mixedLights
        );

        sb.AppendLine(
            "Baked Lights: " +
            report.bakedLights
        );

        sb.AppendLine(
            "Shadow Casting Lights: " +
            report.shadowCastingLights
        );

        sb.AppendLine();

        sb.AppendLine(
            "PHYSICS"
        );

        sb.AppendLine(
            "Rigidbody: " +
            report.rigidbodies
        );

        sb.AppendLine(
            "Colliders: " +
            report.colliders
        );

        sb.AppendLine(
            "Particle Systems: " +
            report.particleSystems
        );

        sb.AppendLine(
            "Audio Sources: " +
            report.audioSources
        );

        sb.AppendLine();

        sb.AppendLine(
            "UI"
        );

        sb.AppendLine(
            "Canvas: " +
            report.canvasCount
        );

        sb.AppendLine(
            "UI Graphics: " +
            report.uiGraphics
        );

        sb.AppendLine();

        sb.AppendLine(
            "SCRIPTS"
        );

        sb.AppendLine(
            "MonoBehaviours: " +
            report.scripts
        );

        sb.AppendLine(
            "Missing Scripts: " +
            report.missingScripts
        );

        sb.AppendLine();

        sb.AppendLine(
            "HIERARCHY"
        );

        sb.AppendLine(
            "Maximum Depth: " +
            report.maximumHierarchyDepth
        );

        sb.AppendLine(
            "Deep Hierarchy Objects: " +
            report.deepHierarchyObjects
        );

        sb.AppendLine();

        sb.AppendLine(
            "PROBLEMS"
        );

        foreach (string problem in
                 report.criticalProblems)
        {
            sb.AppendLine(
                "[CRITICAL] " + problem
            );
        }

        foreach (string warning in
                 report.warnings)
        {
            sb.AppendLine(
                "[WARNING] " + warning
            );
        }

        sb.AppendLine();

        sb.AppendLine(
            "RECOMMENDATIONS"
        );

        foreach (string recommendation in
                 report.recommendations)
        {
            sb.AppendLine(
                "- " + recommendation
            );
        }

        sb.AppendLine();

        sb.AppendLine(
            "POSITIVE"
        );

        foreach (string positive in
                 report.positive)
        {
            sb.AppendLine(
                "+ " + positive
            );
        }

        System.IO.File.WriteAllText(
            path,
            sb.ToString(),
            Encoding.UTF8
        );

        AssetDatabase.Refresh();

        EditorUtility.RevealInFinder(path);
    }

    // ============================================================
    // UI HELPERS
    // ============================================================

    private void BeginBox()
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );
    }

    private void EndBox()
    {
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);
    }

    private void Row(
        string label,
        object value)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            label,
            GUILayout.Width(260)
        );

        EditorGUILayout.LabelField(
            value.ToString()
        );

        EditorGUILayout.EndHorizontal();
    }

    // ============================================================
    // FORMATTERS
    // ============================================================

    private string FormatNumber(long value)
    {
        return value.ToString("N0");
    }

    private string FormatMemory(long bytes)
    {
        if (bytes < 1024)
            return bytes + " B";

        if (bytes < 1024 * 1024)
            return
                (bytes / 1024f).ToString("F1") +
                " KB";

        if (bytes < 1024L * 1024L * 1024L)
            return
                (bytes / (1024f * 1024f)).ToString("F1") +
                " MB";

        return
            (bytes / (1024f * 1024f * 1024f)).ToString("F2") +
            " GB";
    }
}

#endif
