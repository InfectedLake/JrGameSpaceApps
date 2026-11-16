using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crater.Core;
using Crater.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Crater.Services
{
    // ---------- Modelos JSON de DONKI (JsonUtility ignora los campos que no declaramos) ----------
    [Serializable]
    public class DonkiFlare
    {
        public string flrID;
        public string beginTime;
        public string peakTime;
        public string classType;
        public string sourceLocation;
        public string link;
    }

    [Serializable]
    public class DonkiSep
    {
        public string sepID;
        public string eventTime;
        public string link;
    }

    [Serializable]
    class DonkiFlareList { public DonkiFlare[] items; }

    /// <summary>Formato de los archivos en Resources/DonkiArchive (respuestas reales de DONKI recortadas).</summary>
    [Serializable]
    public class DonkiArchiveFile
    {
        public string id;
        public string title;
        public string startDate;
        public string endDate;
        public string source;
        public string retrieved;
        public DonkiFlare[] flares;
        public DonkiSep[] seps;
    }

    public enum NasaStatus { Idle, Loading, Live, Archive, Offline }

    /// <summary>Muestra de actividad solar transformada en intensidad de juego.</summary>
    public class StormSample
    {
        public int intensity;
        public DataKind kind;
        public string label;
        public string detail;
    }

    /// <summary>
    /// Servicio de datos de NASA DONKI.
    ///
    /// ENDPOINT VERIFICADO (15-nov-2026): https://ccmc.gsfc.nasa.gov/DONKI-API/get/FLR?startDate=AAAA-MM-DD&amp;endDate=AAAA-MM-DD
    /// responde JSON sin clave. La ruta "DONKIAPI/get/" del documento de diseño respondió 404, y con barra
    /// final ("FLR/") también responde 404.
    ///
    /// Modo en vivo: consulta los últimos 30 días. Modo archivo: usa respuestas reales guardadas en
    /// Resources/DonkiArchive y SIEMPRE muestra la etiqueta de datos archivados.
    /// GameObject: "NASADataService".
    /// </summary>
    public class NASADataService : MonoBehaviour
    {
        [Tooltip("Ruta base verificada del servicio DONKI del CCMC.")]
        public string baseUrl = "https://ccmc.gsfc.nasa.gov/DONKI-API/get/";
        [Range(1, 30)] public int windowDays = 30;
        public int timeoutSeconds = 12;
        [Tooltip("Carpeta dentro de Resources con las ventanas históricas.")]
        public string archiveFolder = "DonkiArchive";

        public NasaStatus Status { get; private set; } = NasaStatus.Idle;
        // El texto de estado se genera en el idioma actual cada vez que se lee (cambia en vivo con el idioma).
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case NasaStatus.Loading: return Loc.T("NASA_LOADING");
                    case NasaStatus.Live: return Loc.T("NASA_LIVE", flares.Count, WindowLabel);
                    case NasaStatus.Archive: return Loc.T("ARCHIVE_LABEL") + " (" + WindowLabel + ").";
                    case NasaStatus.Offline: return Loc.T("NASA_OFFLINE");
                    default: return Loc.T("NASA_IDLE");
                }
            }
        }

        string windowTitle = "", windowStart = "", windowEnd = "";
        public string WindowLabel => string.IsNullOrEmpty(windowStart) ? "" :
            string.IsNullOrEmpty(windowTitle) ? windowStart + " - " + windowEnd : Loc.T("NASA_WINDOW", windowTitle, windowStart, windowEnd);
        public event Action StatusChanged;

        readonly List<DonkiFlare> flares = new List<DonkiFlare>();
        Coroutine loading;

        /// <summary>Inicia la carga. Si live = false o falla la red, usa el archivo histórico.</summary>
        public void Initialize(bool live)
        {
            if (loading != null) StopCoroutine(loading);
            loading = StartCoroutine(Load(live));
        }

        IEnumerator Load(bool live)
        {
            SetStatus(NasaStatus.Loading);
            flares.Clear();

            if (live)
            {
                var end = DateTime.UtcNow.Date;
                var start = end.AddDays(-(windowDays - 1));
                string url = baseUrl.TrimEnd('/') + "/FLR?startDate=" + start.ToString("yyyy-MM-dd") + "&endDate=" + end.ToString("yyyy-MM-dd");

                using (var req = UnityWebRequest.Get(url))
                {
                    req.timeout = timeoutSeconds;
                    yield return req.SendWebRequest();

                    string error = null;
                    if (req.result != UnityWebRequest.Result.Success) error = "Error de red: " + req.error;
                    else
                    {
                        var parsed = ParseFlares(req.downloadHandler.text, out error);
                        if (parsed != null && parsed.Count > 0)
                        {
                            flares.AddRange(parsed);
                            windowTitle = ""; windowStart = start.ToString("dd/MM/yyyy"); windowEnd = end.ToString("dd/MM/yyyy");
                            SetStatus(NasaStatus.Live);
                            loading = null;
                            yield break;
                        }
                        if (error == null) error = "DONKI no registró llamaradas en los últimos " + windowDays + " días";
                    }
                    Debug.Log("[CRATER] DONKI en vivo no disponible (" + error + "). Se usa el archivo histórico.");
                }
            }

            if (LoadArchive()) SetStatus(NasaStatus.Archive);
            else SetStatus(NasaStatus.Offline);
            loading = null;
        }

        /// <summary>Procesa la respuesta (un arreglo JSON). Devuelve null si el formato no es válido.</summary>
        public static List<DonkiFlare> ParseFlares(string json, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json)) { error = "Respuesta vacía"; return null; }
            json = json.Trim().TrimStart('﻿');
            if (!json.StartsWith("[")) { error = "Formato inesperado"; return null; }
            try
            {
                var list = JsonUtility.FromJson<DonkiFlareList>("{\"items\":" + json + "}");
                if (list?.items == null) { error = "Formato inesperado"; return null; }
                return list.items.Where(f => f != null && !string.IsNullOrEmpty(f.classType)).ToList();
            }
            catch (Exception e) { error = "JSON inválido: " + e.Message; return null; }
        }

        bool LoadArchive()
        {
            var files = Resources.LoadAll<TextAsset>(archiveFolder);
            if (files == null || files.Length == 0) return false;
            var asset = files[UnityEngine.Random.Range(0, files.Length)];
            try
            {
                var data = JsonUtility.FromJson<DonkiArchiveFile>(asset.text.TrimStart('﻿'));
                if (data?.flares == null || data.flares.Length == 0) return false;
                flares.AddRange(data.flares.Where(f => f != null && !string.IsNullOrEmpty(f.classType)));
                windowTitle = data.title; windowStart = data.startDate; windowEnd = data.endDate;
                return flares.Count > 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CRATER] Archivo DONKI inválido " + asset.name + ": " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// [JUEGO] Convierte una llamarada real en intensidad: clase X = 3, M = 2, otras = 1.
        /// Las fechas de los registros NO son una predicción del impacto sobre la base.
        /// </summary>
        public StormSample Sample(IRandom rng, BalanceConfig c)
        {
            if ((Status == NasaStatus.Live || Status == NasaStatus.Archive) && flares.Count > 0)
            {
                var f = flares[rng.Range(0, flares.Count)];
                int intensity = IntensityFromClass(f.classType);
                string date = string.IsNullOrEmpty(f.peakTime) ? f.beginTime : f.peakTime;
                if (!string.IsNullOrEmpty(date) && date.Length >= 10) date = date.Substring(0, 10);
                if (Status == NasaStatus.Live)
                    return new StormSample
                    {
                        intensity = intensity, kind = DataKind.Live,
                        label = Loc.T("NASA_SAMPLE_LIVE"),
                        detail = Loc.T("NASA_SAMPLE_LIVE_DETAIL", f.classType, date)
                    };
                return new StormSample
                {
                    intensity = intensity, kind = DataKind.Archive,
                    label = Loc.T("ARCHIVE_LABEL"),
                    detail = Loc.T("NASA_SAMPLE_ARCHIVE_DETAIL", f.classType, date, WindowLabel)
                };
            }
            return new StormSample
            {
                intensity = EventScheduler.RandomIntensity(c, rng), kind = DataKind.Game,
                label = Loc.T("NASA_SAMPLE_GAME"),
                detail = Loc.T("NASA_SAMPLE_GAME_DETAIL")
            };
        }

        public static int IntensityFromClass(string cls)
        {
            if (string.IsNullOrEmpty(cls)) return 1;
            char k = char.ToUpperInvariant(cls[0]);
            return k == 'X' ? 3 : (k == 'M' ? 2 : 1);
        }

        void SetStatus(NasaStatus s)
        {
            Status = s;
            StatusChanged?.Invoke();
        }
    }
}
