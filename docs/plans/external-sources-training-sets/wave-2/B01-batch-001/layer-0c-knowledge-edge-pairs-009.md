# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-009-001 — v6 corruption types in B01: complete reference

```
source_id: xiigen-b01-0c-009-001
source_title: XIIGen B01 knowledge-edge-009 — v6 corruption type reference
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-009-001-prompt"
  CompiledText: "Describe all v6 corruption types relevant to B01."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

v6 corruption types in B01:

v6-01 — UI predicate missed (compiler stage):
  Trigger condition: "screen" or "interface" present in request
  Corrupt output: CompiledPredicateIds: [] (empty — treats as knowledge-edge)
  Correct output: CompiledPredicateIds: ["UI"]
  Impact: ScreenInventoryFlow11 never invoked; knowledge-edge synthetic flow runs

v6-01 (router variant) — UI dropped at routing stage:
  Trigger condition: CompiledPredicateIds includes UI but router omits 1.1 routing
  Corrupt output: RoutingDecisions missing 1.1 entry
  Correct output: RoutingDecisions contains 1.1 entry with EvidencePredicates: ["UI"]
  Impact: Same as compiler-stage v6-01

v6-02 — DOCUMENT suppressed when UI fires (compiler or router stage):
  Trigger condition: Both "screen"/"interface" AND "document"/"specification" present
  Corrupt output: Only UI fires; DOCUMENT dropped from CompiledPredicateIds
  Correct output: Both ["UI", "DOCUMENT"] present
  Impact: 2.1 routing never produced; requirements/specification artifact missing

v6-03 — Wrong sub-phase routing:
  Trigger condition: UI predicate present but router assigns PredictedSubPhaseId: "2.1"
  Corrupt output: EvidencePredicates: ["UI"] with PredictedSubPhaseId: "2.1"
  Correct output: PredictedSubPhaseId must be "1.1" when EvidencePredicates is ["UI"]
  Impact: BusinessRequirementsFlow21 runs for screen mapping request

---

## B01-0C-009-002 — v6 corruption detection checklist

```
source_id: xiigen-b01-0c-009-002
source_title: XIIGen B01 knowledge-edge-009 — v6 detection checklist
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Runtime checks to detect v6 corruptions in B01 routing output:

Check 1 (v6-01 detection):
  IF original request contains any of: screen, interface, ux, page, form, button,
    input, component, layout, responsive, accessibility, visual, wireframe, prototype,
    interaction, typography, color, spacing, design-system, theme, modal, ui
  AND CompiledPredicateIds does NOT contain "UI"
  THEN: v6-01 corruption detected; re-run compiler

Check 2 (v6-02 detection):
  IF CompiledPredicateIds contains "UI"
  AND original request contains any of: document, specification, requirement, describe,
    define, functional, behavior, criteria, story, use-case, narrative, capability, epics
  AND CompiledPredicateIds does NOT contain "DOCUMENT"
  THEN: v6-02 corruption detected; re-run compiler

Check 3 (v6-03 detection):
  FOR EACH RoutingDecision in RoutingDecisions:
    IF EvidencePredicates == ["UI"]
    AND PredictedSubPhaseId != "1.1"
    THEN: v6-03 corruption detected; re-route UI to 1.1

---

## B01-0C-009-003 — v6 corruption priority and remediation

```
source_id: xiigen-b01-0c-009-003
source_title: XIIGen B01 knowledge-edge-009 — v6 remediation steps
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Remediation priority for v6 corruptions:

  Priority 1 (highest impact): v6-03 — wrong sub-phase
    Why: Runs entirely the wrong flow; all output is for wrong purpose
    Fix: Re-examine routing rules; UI always routes to 1.1 in Phase 1

  Priority 2: v6-01 — predicate missed
    Why: Entire predicate omitted; one or more routing branches absent
    Fix: Re-run compiler with strict lexicon scan; check all 22 UI terms

  Priority 3: v6-02 — predicate suppressed
    Why: One predicate dropped; one routing branch absent
    Fix: Re-run compiler/router with independence rule enforced

All v6 corruptions are recoverable — the correct output can be produced by
re-running the affected unit with the correct rules. None require knowledge-edge
fallback because the original request IS routable; the error is in the processing.
