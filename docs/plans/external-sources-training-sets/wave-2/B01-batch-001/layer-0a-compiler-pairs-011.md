# Layer 0-A — Compiler Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-011-001 — v6-01: UI predicate missed when "screen" or "interface" present

```
source_id: xiigen-b01-0a-011-v601
source_title: XIIGen B01 compiler-011 — Corruption-v6-01: UI predicate suppressed on screen
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
corruption_type: Corruption-v6-01-ui-predicate-missed
```

A:
  run_id: "B01-0a-011-v601"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory every screen in the onboarding interface and map its interactions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B-correct:
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

  Predicate evidence:
    UI fires on: "screen" (UI lexicon), "interface" (UI lexicon),
      "interaction" (via "interactions", UI lexicon)

B-corrupt (v6-01):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match"

Corruption explanation:
  v6-01 occurs when the compiler incorrectly fails to fire UI despite one or
  more UI lexicon terms being present. In this case, "screen", "interface", and
  "interactions" all appear and all fire UI. The corrupt output treats this as
  a knowledge-edge case — which is wrong. Knowledge-edge only applies when the
  compiled predicate list is genuinely empty (no lexicon terms present at all).
  The correct output must contain ["UI"]; the corrupt output of [] causes
  downstream phantom routing to a synthetic knowledge-edge flow.

---

## B01-0A-011-002 — v6-02: DOCUMENT suppressed when UI also fires

```
source_id: xiigen-b01-0a-011-v602
source_title: XIIGen B01 compiler-011 — Corruption-v6-02: DOCUMENT suppressed in UI+DOCUMENT pair
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
corruption_type: Corruption-v6-02-document-suppressed-by-ui
```

A:
  run_id: "B01-0a-011-v602"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the checkout page interface flow and produce a specification for each screen."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B-correct:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

  Predicate evidence:
    UI fires on: "page" (UI lexicon), "interface" (UI lexicon), "screen" (UI lexicon)
    DOCUMENT fires on: "document" (DOCUMENT lexicon), "specification" (DOCUMENT lexicon)

B-corrupt (v6-02):
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Corruption explanation:
  v6-02 occurs when the DOCUMENT predicate is suppressed because UI also fired.
  Some model variants incorrectly treat UI as "dominant" and drop DOCUMENT from
  the output. This is wrong: predicates are independent; multiple predicates
  may fire from a single request. The corrupt output routes only to 1.1, missing
  the 2.1 routing for the specification artifact. The page specification is never
  produced.

---

## B01-0A-011-003 — v6-03: routing to 2.1 instead of 1.1 for UI predicate

```
source_id: xiigen-b01-0a-011-v603
source_title: XIIGen B01 compiler-011 — Corruption-v6-03: UI routes to wrong sub-phase
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
corruption_type: Corruption-v6-03-ui-wrong-sub-phase
```

A:
  run_id: "B01-0a-011-v603"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screen flows for the SaaS onboarding UI."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B-correct (compiler output):
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

  B-correct (router output — from Layer 0-B):
    PredictedSubPhaseId: "1.1"
    EvidencePredicates: ["UI"]
    KnowledgeEdge: false

B-corrupt (v6-03 — at router stage):
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false

Corruption explanation:
  v6-03 is a routing-layer corruption. The compiler output is correct (UI fires).
  The corruption occurs in the router: it routes UI to sub-phase 2.1 (Business
  Requirements) instead of the correct 1.1 (Screen Flow and UX Definition).
  The routing rule is: UI → 1.1. Sub-phase 2.1 is owned by DOCUMENT, not UI.
  This corruption causes the ScreenInventoryFlow11 to never be invoked; instead
  the BusinessRequirementsFlow21 runs for a screen mapping request.
