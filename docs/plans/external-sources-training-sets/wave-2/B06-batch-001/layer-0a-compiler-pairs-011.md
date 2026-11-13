# Layer 0-A — Compiler Pairs (011)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 011 covers the three v6 corruption types at the compiler level:
v6-01 (DOCUMENT missed on a requirements request), v6-02 (IMPLEMENT suppressed
when "write" fires alongside DOCUMENT), v6-03 (routing to single sub-phase
when multiple DOCUMENT sub-phases apply).

---

## B06-0A-011-001 — v6-01 corruption: DOCUMENT missed on "define use cases"

```
source_id: xiigen-b06-0a-011-001
source_title: XIIGen B06 compiler — v6-01 DOCUMENT missed
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
pair_kind: compiler
```

A:
  run_id: "B06-0a-011-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the use cases for the payment processing feature."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.92
  Note: "define" fires DOCUMENT, "use cases" (→ use-case) fires DOCUMENT.
    DOCUMENT must be compiled.

B-corrupt (v6-01):
  CompiledPredicateIds: []
  Confidence: 0.50
  KnownFailureMode: "no_predicate_match"

Corruption type: Corruption-v6-01 — DOCUMENT missed on requirements request.
Why it occurs: The model fails to recognize "define" as a DOCUMENT term,
  or fails to recognize "use cases" as mapping to "use-case" in the DOCUMENT lexicon.
Why harmful: The request is routed to knowledge-edge instead of 2.1. A valid
  requirements-authoring request is treated as an unrecognized capability.
  The checkout requirements are never authored.
Fix: Both "define" and "use-case" are in the DOCUMENT lexicon. The compiler
  must match them and produce CompiledPredicateIds: ["DOCUMENT"].

---

## B06-0A-011-002 — v6-02 corruption: IMPLEMENT suppressed in "write user stories"

```
source_id: xiigen-b06-0a-011-002
source_title: XIIGen B06 compiler — v6-02 IMPLEMENT suppressed
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
pair_kind: compiler
```

A:
  run_id: "B06-0a-011-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the user stories for the payment feature with acceptance criteria."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B-correct:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Note: "story" and "criteria" fire DOCUMENT; "write" fires IMPLEMENT.
    Both must be present in CompiledPredicateIds.

B-corrupt (v6-02):
  CompiledPredicateIds: ["DOCUMENT"]
  Note: IMPLEMENT dropped because the model infers documentation context
    and suppresses "write" from firing IMPLEMENT.

Corruption type: Corruption-v6-02 — IMPLEMENT suppressed when "write" fires
  alongside DOCUMENT.
Why it occurs: The model observes "write the user stories" (documentation context)
  and suppresses IMPLEMENT, reasoning that "write" here means "compose text" not
  "write code". This intent-based suppression is wrong.
Why harmful: The 4.1 (Backend Services) routing decision is lost. If any
  implementation work is tied to this request, it is never routed.
Fix: The compiler is lexicon-based. "write" is in IMPLEMENT lexicon. It fires
  regardless of whether the context is "write code" or "write documentation".
  Both predicates must be compiled.

---

## B06-0A-011-003 — v6-03 corruption: routing collapsed to single sub-phase

```
source_id: xiigen-b06-0a-011-003
source_title: XIIGen B06 compiler — v6-03 routing collapse at compiler level
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
pair_kind: compiler
```

A:
  run_id: "B06-0a-011-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the checkout requirements and document the stakeholder expectations for external partners."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Sub-phase routing signals in compiled document:
    "requirements" → 2.1 signal
    "stakeholder"  → 2.4 signal
  Both signals are embedded in the compiled output and passed to the router.
  The router must produce two routing decisions: 2.1 and 2.4.

B-corrupt (v6-03 at router level):
  Router produces only one routing decision (2.1), ignoring the 2.4 stakeholder signal.
  Note: This corruption manifests in the router, not the compiler. The compiler
  output is correct (["DOCUMENT"]). The router collapses two valid sub-phase
  destinations into one.

Corruption type: Corruption-v6-03 — routing to single sub-phase when two
  DOCUMENT sub-phase signals are present.
Why harmful: Stakeholder and external partner requirements (2.4) are never
  documented. The requirements artifact is incomplete. External compliance
  requirements for partners are missing.
Fix: The router must check all sub-phase signal words within the DOCUMENT
  predicate evidence and produce a routing decision for each matching sub-phase.
