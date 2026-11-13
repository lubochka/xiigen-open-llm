namespace ExperimentsV2;

// Skill trigger library from HOW-TO-USE-SKILLS-v5.4.md.
// Each trigger: condition → skill name → category.
public static class SkillTriggerLibrary
{
    public const int GOVERNANCE  = 0;
    public const int CODE_REVIEW = 1;
    public const int TESTING     = 2;
    public const int PLANNING    = 3;
    public const int AUTH        = 4;
    public const int UI          = 5;

    public static readonly string[] CategoryNames =
        ["GOVERNANCE", "CODE_REVIEW", "TESTING", "PLANNING", "AUTH", "UI"];

    public record SkillTrigger(
        string Condition,
        string SkillName,
        int    Category,
        bool   AlwaysOn);

    public static readonly IReadOnlyList<SkillTrigger> All = new SkillTrigger[]
    {
        // ── GOVERNANCE ───────────────────────────────────────────────────────
        new("every output without exception",
            "human-language-gate", GOVERNANCE, true),
        new("every session start reconnaissance before anything",
            "reconnaissance-gate", GOVERNANCE, true),
        new("any session with declared mode",
            "response-construction-protocol", GOVERNANCE, true),
        new("any session with user-stated goal",
            "goal-context-persistence", GOVERNANCE, true),
        new("any session with user assertion about existing state",
            "claim-as-hypothesis", GOVERNANCE, false),
        new("any architect or plan-review output specificity",
            "specificity-calibration", GOVERNANCE, false),
        new("any ARCHITECT PLANNER REVIEWER MATERIALIZATION session",
            "architect-habits-guide", GOVERNANCE, false),
        new("any PLANNING MATERIALIZATION ARCHITECT REVIEW session mode",
            "session-mode-declaration", GOVERNANCE, false),
        new("any plan review goal delivery completeness first arbiter",
            "goal-delivery-check", GOVERNANCE, false),
        new("any session referencing design artifacts completeness",
            "design-artifact-completeness", GOVERNANCE, false),
        new("any tenant-facing work round-trip verification",
            "mvp-round-trip-verification", GOVERNANCE, false),
        new("any MATERIALIZATION session type",
            "materialization-session-type", GOVERNANCE, false),

        // ── CODE_REVIEW ──────────────────────────────────────────────────────
        new("new service ts file in engine flows portability auth SDK imports",
            "dna-compliance-guard", CODE_REVIEW, false),
        new("after Phase B generates service code before DPO capture all 5 layers",
            "generated-code-review", CODE_REVIEW, false),
        new("portability or auth violation found fix propagation",
            "retroactive-development", CODE_REVIEW, false),
        new("auth fix applied to existing service verification category",
            "self-verification", CODE_REVIEW, false),
        new("new ES document TypeScript service file data connection",
            "data-connection-classification", CODE_REVIEW, false),
        new("controller generated without UseGuards or Roles auth declaration",
            "dna-compliance-guard auth checks", CODE_REVIEW, false),
        new("SDK direct import found in service file no SDK outside provider",
            "retroactive-development SDK fix", CODE_REVIEW, false),

        // ── TESTING ──────────────────────────────────────────────────────────
        new("any new spec file in engine flows behavioral assertions Rule 7 auth",
            "test-integrity", TESTING, false),
        new("flow phase close before portability check state file",
            "flow-implementation-guide portability gate", TESTING, false),
        new("flow phase close before auth check state file authentication status",
            "flow-implementation-guide auth gate", TESTING, false),
        new("flow phase close before certification tier check",
            "flow-implementation-guide cert tier gate", TESTING, false),
        new("phase preflight default check auth infrastructure 4-component",
            "phase-preflight", TESTING, false),
        new("TEACH-QA R0 produced scope isolation count zero NDJSON coverage",
            "arbiter-ndjson-requirements", TESTING, false),
        new("Phase A NDJSON fixture produced scope isolation count",
            "arbiter-ndjson-requirements", TESTING, false),
        new("QA session runs on Phase G H I close protocol completeness",
            "qa-session-type", TESTING, false),

        // ── PLANNING ─────────────────────────────────────────────────────────
        new("active promotion distributable flow Phase E lifecycle writes",
            "flow-lifecycle", PLANNING, false),
        new("distributable flow reaching Phase F without tenant certification",
            "flow-implementation-guide Phase I cert", PLANNING, false),
        new("authentication deployment prerequisite check targeting Tier C",
            "flow-lifecycle auth deployment check", PLANNING, false),
        new("any stop with improvement metric measurement protocol",
            "improvement-measurement", PLANNING, false),
        new("any stop claiming fleet progress ui fleet completion",
            "ui-fleet-completion-criteria", PLANNING, false),
        new("any stop claiming result output skepticism",
            "output-skepticism", PLANNING, false),
        new("any multi-phase plan before Phase 1 self-validation",
            "plan-self-validation", PLANNING, false),

        // ── AUTH ─────────────────────────────────────────────────────────────
        new("flow prerequisites TIER 1 portability check 5 auth infrastructure",
            "flow-prerequisites auth check", AUTH, false),
        new("TIER 2 3 renumbered checks portability",
            "flow-prerequisites portability", AUTH, false),
        new("Phase H mandatory auth decoration checks Rule 7 tests",
            "flow-implementation-guide Phase H auth", AUTH, false),
        new("Phase I mandatory cert tier Tier C checklist auth deployment",
            "flow-implementation-guide Phase I", AUTH, false),

        // ── UI ───────────────────────────────────────────────────────────────
        new("session examining or repairing existing React pages UI examination",
            "flow-ui-examination", UI, false),
        new("first React page for flow impeccable md absent product design context",
            "product-design-context", UI, false),
        new("any session producing React pages UI UX compliance",
            "ui-ux-compliance", UI, false),
        new("Phase 7 Step 5 screen craft audit",
            "screen-craft-audit", UI, false),
        new("N 5 instances of same entity type per-entity examination",
            "per-entity-examination", UI, false),
        new("any UI UX session before first file work scope inventory",
            "work-scope-inventory", UI, false),
        new("any PNG examination per-image visual validation",
            "per-image-validation", UI, false),
        new("fleet level UI UX improvement visual examination round",
            "visual-examination-round", UI, false),
        new("any stop claiming visual improvement coverage matrix",
            "coverage-matrix", UI, false),
    }.AsReadOnly();

    public static IReadOnlyList<SkillTrigger> ForCategory(int category)
        => All.Where(t => t.Category == category || t.AlwaysOn).ToList().AsReadOnly();

    public static IReadOnlyList<SkillTrigger> AlwaysRequired
        => All.Where(t => t.AlwaysOn).ToList().AsReadOnly();
}
