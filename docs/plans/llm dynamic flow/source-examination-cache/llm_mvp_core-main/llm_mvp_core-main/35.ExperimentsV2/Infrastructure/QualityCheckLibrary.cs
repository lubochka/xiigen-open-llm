namespace ExperimentsV2;

// 16 mandatory quality checks with their applicability conditions.
// Source: SESSION-START-PROMPT v5.5 — MANDATORY CHECKS BEFORE EVERY STOP.
public static class QualityCheckLibrary
{
    public const int CODE_SERVICE = 0;
    public const int PLAN_DOC    = 1;
    public const int REACT_PAGE  = 2;
    public const int ANALYSIS_DOC = 3;
    public const int ANY_OUTPUT  = 4;

    public static readonly string[] OutputTypeNames =
        ["CODE_SERVICE", "PLAN_DOC", "REACT_PAGE", "ANALYSIS_DOC", "ANY_OUTPUT"];

    public record QualityCheck(
        int    Number,
        string Name,
        string Description,
        int[]  AppliesTo);

    public static readonly IReadOnlyList<QualityCheck> All = new QualityCheck[]
    {
        new(1,  "OUTPUT_CONTRACT",
            "Binary done criterion met — yes or no, no interpretation",
            new[] {ANY_OUTPUT}),
        new(2,  "MISSION_PROGRESS",
            "Mission progress check — are we advancing the session goal",
            new[] {ANY_OUTPUT}),
        new(3,  "ISSUE_INVENTORY",
            "All found issues addressed — FIXED or DEFERRED with carry-forward",
            new[] {ANY_OUTPUT}),
        new(4,  "TEST_GATE",
            "failures === 0 absolute — not delta, not no new failures",
            new[] {CODE_SERVICE}),
        new(5,  "SCOPE_ISOLATION",
            "scope_isolation arbiter present in every engine/flows/ node",
            new[] {CODE_SERVICE}),
        new(6,  "PIPELINE_CONTRACT",
            "Pipeline position question answered for GENERATION/PLANNING/MATERIALIZATION",
            new[] {CODE_SERVICE, PLAN_DOC}),
        new(7,  "DOCUMENTATION_ARTIFACTS",
            "Markdown companion for every structured file — Rule 17",
            new[] {CODE_SERVICE, PLAN_DOC}),
        new(8,  "RECONNAISSANCE_EVIDENCE",
            "Reconnaissance tracking record saved — Gate 3 routing — Gate 5 hypotheses",
            new[] {ANALYSIS_DOC}),
        new(9,  "CLAIMS_VERIFIED",
            "No needs-to-be-confirmed blocking claims from claim-as-hypothesis",
            new[] {ANY_OUTPUT}),
        new(10, "GOAL_REMINDER",
            "Two-layer goal reminder block visible — verbatim goal quoted",
            new[] {ANY_OUTPUT}),
        new(11, "GOAL_DELIVERY_VERDICT",
            "Goal delivery completeness verdict — plan reviews only",
            new[] {PLAN_DOC}),
        new(12, "ARCHITECT_HABITS",
            "Architect habits discipline check — architect behavior classifier",
            new[] {ANALYSIS_DOC}),
        new(13, "TOOLS_BEFORE_PERSON",
            "Tools called before answering — no tool-free answers about codebase state",
            new[] {ANY_OUTPUT}),
        new(14, "RESPONSE_CONSTRUCTION",
            "Response construction protocol compliance — all 6 steps",
            new[] {ANY_OUTPUT}),
        new(15, "UI_UX_COMPLIANCE",
            "UI/UX compliance check — if one or more React pages produced",
            new[] {REACT_PAGE}),
        new(16, "HUMAN_LANGUAGE_GATE",
            "Zero machine shortcodes in any output — scan before emitting",
            new[] {ANY_OUTPUT}),
    }.AsReadOnly();

    public static IReadOnlyList<QualityCheck> ForOutputType(int outputType)
        => All.Where(c =>
            c.AppliesTo.Contains(ANY_OUTPUT) ||
            c.AppliesTo.Contains(outputType))
        .ToList().AsReadOnly();

    public static bool[] GetCheckVector(int outputType)
        => All.Select(c =>
            c.AppliesTo.Contains(ANY_OUTPUT) ||
            c.AppliesTo.Contains(outputType)).ToArray();
}
