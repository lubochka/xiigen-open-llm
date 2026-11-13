# Held-Out and Corruption Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (014)

---

### Held-Out Pair B01-HO-014-001 — NotificationCenter rephrased routes to 1.1

```
source_id: xiigen-b01-held-out-014-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-014-001"
  user_request: "We want a complete inventory of the notification inbox UI including all the component states and modal dialogs."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI"]
  Confidence: 0.94
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

Generalization test:
  "ui" fires UI. "component" fires UI. "modal" fires UI.
  Three independent UI lexicon hits confirm strong UI signal.
  "inbox", "notification", "states" do not independently fire.

---

### Held-Out Pair B01-HO-014-002 — DOCUMENT+UI: spec for notification panel

```
source_id: xiigen-b01-held-out-014-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-014-002"
  user_request: "Document all functional requirements for the notification settings screen."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.91
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "screen" fires UI. "Document", "functional", "requirements" fire DOCUMENT.
  Two predicates → two independent routing decisions. TransitionType: documentation.

---

## Section 2 — Corruption Pairs (014)

---

### Corruption Pair B01-COR-014-001 — v6-02: probationary_rejected FallbackUsed suppressed

```
source_id: xiigen-b01-corruption-014-001
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-02-flow-factory-fallback-suppression
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "notification-center-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
  fallback_flow_id: "ScreenInventoryFlow11"

B-correct:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "notification-center-flow-1.1-candidate"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt:
  SelectedFlowId: "notification-center-flow-1.1-candidate"
  TrustState: "probationary"
  FallbackUsed: false
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Corruption type: Corruption-v6-02 — FallbackUsed suppressed for probationary_rejected candidate. Correct behavior: FallbackUsed: true always.
