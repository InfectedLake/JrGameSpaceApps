using Crater.Core;

namespace Crater.Data
{
    /// <summary>
    /// Textos de los ScriptableObjects en el idioma actual. Si la clave existe en la tabla de localización se usa;
    /// si no (contenido nuevo escrito solo en el asset), se muestra el texto del asset tal cual.
    /// </summary>
    public static class LocalizedContent
    {
        static string E(EventData d, string field) => "EVENT_" + Loc.EventKey(d.type) + "_" + field;

        public static string LTitle(this EventData d) => Loc.TOr(E(d, "TITLE"), d.title);
        public static string LDescription(this EventData d) => Loc.TOr(E(d, "DESC"), d.description);
        public static string LWarning(this EventData d) => Loc.TOr(E(d, "WARN"), d.warningText);
        public static string LEducation(this EventData d) => Loc.TOr(E(d, "EDU"), d.educationalMessage);
        public static string LSource(this EventData d) => Loc.TOr(E(d, "SOURCE"), d.source);
        public static string LOptionTitle(this EventData d, int i) => Loc.TOr(E(d, "OPT" + i + "_TITLE"), i < d.options.Count ? d.options[i].title : "");
        public static string LOptionDesc(this EventData d, int i) => Loc.TOr(E(d, "OPT" + i + "_DESC"), i < d.options.Count ? d.options[i].description : "");

        static string B(BuildingData b, string field) => "BUILDING_" + Loc.BuildingKey(b.stats.id) + "_" + field;
        public static string LName(this BuildingData b) => Loc.Building(b.stats.id);
        public static string LDescription(this BuildingData b) => Loc.TOr(B(b, "DESC"), b.description);
        public static string LHowItWorks(this BuildingData b) => Loc.TOr(B(b, "HOW"), b.howItWorks);

        static string S(ScienceData s, string field) => "SCI_" + (s.id ?? "").ToUpperInvariant() + "_" + field;
        public static string LTitle(this ScienceData s) => Loc.TOr(S(s, "TITLE"), s.title);
        public static string LExplanation(this ScienceData s) => Loc.TOr(S(s, "EXPL"), s.explanation);
        public static string LFact(this ScienceData s) => Loc.TOr(S(s, "FACT"), s.fact);
        public static string LSource(this ScienceData s) => Loc.TOr(S(s, "SOURCE"), s.source);
        public static string LGameNote(this ScienceData s) => Loc.TOr(S(s, "NOTE"), s.gameNote);
    }
}
