using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Crater.EditorTools
{
    /// <summary>
    /// Configuración y generación de builds de distribución (itch.io).
    ///  - CRATER &gt; Builds &gt; Aplicar configuración de distribución: escena Crater.unity como única escena del build,
    ///    nombre del producto, tamaño Web 1280×720 y compresión Web Gzip con Decompression Fallback.
    ///  - CRATER &gt; Builds &gt; Web (itch.io HTML5)  → Builds/Web/index.html (+ Build/, TemplateData/)
    ///  - CRATER &gt; Builds &gt; Windows 64-bit         → Builds/Windows/CRATER.exe (+ CRATER_Data/, UnityPlayer.dll…)
    /// También se pueden llamar en modo batch:
    ///   Unity.exe -batchmode -quit -projectPath . -executeMethod Crater.EditorTools.CraterBuild.BuildWeb
    /// La carpeta Builds/ está en .gitignore: los builds son artefactos reproducibles.
    /// </summary>
    public static class CraterBuild
    {
        public const string ScenePath = "Assets/Scenes/Crater.unity";
        public const string ProductName = "CRATER";
        public const string WebDir = "Builds/Web";
        public const string WindowsDir = "Builds/Windows";

        [MenuItem("CRATER/Builds/Aplicar configuración de distribución", priority = 100)]
        public static void ApplySettings()
        {
            if (!File.Exists(ScenePath)) throw new Exception("No existe " + ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = ProductName;

            // Web: el juego está diseñado a 1920×1080 (16:9); 1280×720 encaja en la página de itch.io sin barras.
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            // Gzip: compatible con todos los navegadores (HTTP y HTTPS); itch.io detecta el contenido gzip y envía
            // Content-Encoding: gzip. Decompression Fallback hace que el cargador descomprima en JavaScript si un
            // servidor no envía ese encabezado, así el build carga igual (a costa de un cargador algo más grande).
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;

            AssetDatabase.SaveAssets();
            Debug.Log("[CRATER] Configuración de distribución aplicada: escena " + ScenePath + ", Web Gzip + fallback, 1280×720.");
        }

        [MenuItem("CRATER/Builds/Web (itch.io HTML5)", priority = 110)]
        public static void BuildWeb() => Build(BuildTarget.WebGL, WebDir, WebDir);

        [MenuItem("CRATER/Builds/Windows 64-bit", priority = 111)]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, Path.Combine(WindowsDir, ProductName + ".exe"), WindowsDir);

        static void Build(BuildTarget target, string location, string folder)
        {
            ApplySettings();
            if (Directory.Exists(folder)) Directory.Delete(folder, true);   // build limpio (solo la carpeta de salida)
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = location,
                target = target,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            string msg = $"[CRATER] Build {target}: {s.result} · {s.totalSize / (1024f * 1024f):0.0} MB · {s.totalTime:mm\\:ss} · {Path.GetFullPath(folder)}";
            if (s.result != BuildResult.Succeeded) throw new Exception(msg + " · errores: " + s.totalErrors);
            if (target == BuildTarget.WebGL) EnableWebAutoSync(Path.Combine(folder, "index.html"));
            Debug.Log(msg);
        }

        /// <summary>
        /// La partida se guarda como archivo en Application.persistentDataPath. En Web ese archivo vive en memoria y
        /// solo llega a IndexedDB (y sobrevive a recargar la página) si la configuración del cargador incluye
        /// autoSyncPersistentDataPath: true. La plantilla de Unity trae esa línea comentada: aquí se activa.
        /// </summary>
        public static void EnableWebAutoSync(string indexHtml)
        {
            if (!File.Exists(indexHtml)) { Debug.LogWarning("[CRATER] No se encontró " + indexHtml); return; }
            string html = File.ReadAllText(indexHtml);
            // Plantilla Default de Unity 6: "// config.autoSyncPersistentDataPath = true;" (comentada).
            string patched = System.Text.RegularExpressions.Regex.Replace(html, @"//\s*(config\.autoSyncPersistentDataPath\s*=\s*true;)", "$1");
            if (patched == html) patched = System.Text.RegularExpressions.Regex.Replace(html, @"//\s*autoSyncPersistentDataPath:\s*true,?", "autoSyncPersistentDataPath: true,");
            if (patched == html && System.Text.RegularExpressions.Regex.IsMatch(html, @"^\s*(config\.)?autoSyncPersistentDataPath\s*[:=]\s*true", System.Text.RegularExpressions.RegexOptions.Multiline)) return;   // ya activo
            if (patched == html)
            {
                // Plantilla sin la línea comentada: se agrega dentro del objeto config, después de productVersion.
                patched = System.Text.RegularExpressions.Regex.Replace(html, @"(productVersion:\s*""[^""]*"",)", "$1\n        autoSyncPersistentDataPath: true,");
            }
            if (patched == html) { Debug.LogWarning("[CRATER] No se pudo activar autoSyncPersistentDataPath en " + indexHtml + ": revisa la plantilla."); return; }
            File.WriteAllText(indexHtml, patched);
            Debug.Log("[CRATER] Web: autoSyncPersistentDataPath activado (la partida guardada persiste en IndexedDB).");
        }
    }
}
