# Cross-Layer Synthesis (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: gap vocabulary → knowledge-edge path ("add a navigation menu")

---

## Synthesis trace: gap vocabulary — knowledge-edge full pipeline

### Request
UserRequest: "Add a navigation menu to the top of every page in our app."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: []
  Evidence:
    - Term: "navigation", Lexicon: "NOT_IN_UI_LEXICON", Note: "navigation is confirmed gap vocabulary"
    - Term: "menu", Lexicon: "NOT_IN_ANY_LEXICON", Note: "menu has no predicate"
    - Term: "page", Lexicon: "UI — BUT see note below"
  Confidence: 0.60
  Warnings: ["no_predicate_matched: knowledge_edge_result"]
  Note: "page" alone in this context: the compiler identifies "page" is in the UI lexicon, BUT this is a boundary case — re-evaluation with full context: "add a navigation menu to every page" — the primary intent is navigation/menu addition, not screen inventory. However, "page" IS in the UI lexicon; strict rule application fires UI.

CORRECTION NOTE:
  Strict lexicon rule: "page" IS in the UI lexicon. CompiledPredicateIds: ["UI"] is the correct output.
  This trace is a teaching case for how lexicon strictness interacts with intent:
  even when the primary intent is navigation (gap), "page" still fires UI.
  CompiledPredicateIds: ["UI"] (corrected).

---

### Layer 0-B — MultiSubPhaseRouterUnit (corrected)

Input: CompiledPredicateIds: ["UI"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1"
      PredicateId: "UI"
      FlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
  DecisionCount: 1

Teaching note for this synthesis:
  The original request was designed to test whether "navigation" (gap) suppresses "page" (UI).
  It does NOT — each term is evaluated independently.
  "navigation" does not fire UI. "page" does fire UI.
  The presence of gap vocabulary does not suppress valid predicate matches.

---

### Layer 0-C — Knowledge-Edge Check (corrected)

Result: no knowledge-edge path invoked.
  "page" fires UI; CompiledPredicateIds is not empty. Normal routing proceeds.

Pure knowledge-edge scenario (alternate version):
  If request were "Add a navigation menu to our app" (no "page" term):
  CompiledPredicateIds: []
  Knowledge-edge path fires:
    KnowledgeEdge: true
    FailureClassification: "no_predicate_match"
    Verdict: KNOWLEDGE_EDGE
    Metrics: { total_tokens: 9, matched_predicates: 0, gap_terms: ["navigation", "menu"], ... }

---

### Layer 0-D — FlowFactory

Input: SubPhaseId: "1.1", TrustState: "deterministic_generic"
Output:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_SELECTED"

---

### Layer 1 — TransitionOrchestratorUnit

Input: CompiledPredicateIds: ["UI"], RoutingDecisions: [{1.1, UI}]

Output:
  SelectedLogicalTransition:
    TransitionId: "transition-B01-synth-006-1.1"
    TransitionType: "documentation"
    SubPhaseId: "1.1"
    PredicateIds: ["UI"]
    FlowsSelected: ["ScreenInventoryFlow11"]
    flow_execution_order: ["ScreenInventoryFlow11"]
    ContextCarryForward: true

Key rule applied:
  UI alone (no IMPLEMENT) → TransitionType: documentation.

---

### Layer 2 — ScreenInventoryFlow11

Node 01 — DomainSplitterUnit:
  Input: SelectedLogicalTransition
  Output: DomainPacket — UI (navigation menu context)
  PacketFocus: "Screen inventory for every page: page list, navigation placement per page"

Node 09 — FormatterUnit:
  Output:
    FinalHumanResult:
      ScreenCount: "all app pages"
      NavigationNote: "Navigation menu placement documented per page; implementation not in scope (no IMPLEMENT predicate)"
      TransitionType: "documentation"
      GateBResult: "PASS: UI routing correct; documentation TransitionType; navigation gap vocabulary correctly not elevated to UI"

---

### Layer 3 — Teaching units confirm

  PairParser: gap-vocabulary boundary trace complete
  ModelTrainer: all Gate B assertions pass for this trace
  WaveAcceptanceVerdict: ACCEPTED for this trace

---

### Synthesis summary

Key teaching point: gap vocabulary does NOT suppress valid UI matches in the same request.
  "navigation" (gap) + "page" (UI) → CompiledPredicateIds: ["UI"] → 1.1 → documentation.
  Pure gap request (no UI terms at all) → empty predicates → knowledge-edge path.
  The boundary: each term evaluated independently against the lexicon.
