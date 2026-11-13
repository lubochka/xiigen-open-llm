# Held-Out and Corruption Pairs (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "page accessibility audit" → UI; COR: accessibility→DOCUMENT

---

## Section 1 — Held-Out Pair HO-007-A

```
source_id: xiigen-b01-held-out-007-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-007-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Audit all onboarding pages for keyboard navigation accessibility and screen reader compatibility."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false

Generalization test:
  "pages" — plural of "page" (UI term 5). Fires UI.
  "accessibility" → UI (term 11). Two UI terms.
  "keyboard navigation" — "navigation" is NOT in UI lexicon; "keyboard" is not a UI term.
  But "page" and "accessibility" together already confirm UI routing.
  "screen reader" — "screen" → UI (term 4). Three UI terms.
  Result: UI alone, routes to 1.1. "Audit" does not fire any predicate as a verb.

---

## Section 1 — Held-Out Pair HO-007-B

```
source_id: xiigen-b01-held-out-007-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-007-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Run an accessibility review of the form components on the signup and activation screens, and write a remediation spec for each failure found."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false

Generalization test:
  "accessibility" → UI. "form" → UI. "components" → UI. "screens" → UI. Multiple UI terms.
  "write" → IMPLEMENT (unconditional). "spec" → DOCUMENT (term 2 in DOCUMENT lexicon).
  Three predicates → three routing decisions.

---

## Section 2 — Corruption Pair COR-007

```
source_id: xiigen-b01-corruption-007
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-accessibility-misrouted-to-DOCUMENT
```

A:
  DocumentId: "B01-007-access-prompt"
  CompiledText: "Audit the accessibility of all onboarding screens for WCAG AA compliance."
  CompiledPredicateIds_correct: ["UI"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.80
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1
  Note: "'accessibility' misclassified as DOCUMENT; 'screens' missed"

Corruption type: Corruption-v6-01 — "accessibility" misrouted to DOCUMENT.
  "accessibility" is term 11 in the UI lexicon. It unconditionally fires UI.
  "screens" is term 4 in the UI lexicon. Both terms are missed in the corrupted output.
  The corrupted model apparently infers that "compliance documentation" context fires DOCUMENT,
  but DOCUMENT only fires when explicit DOCUMENT lexicon terms appear.

Why this is harmful:
  The accessibility audit of the screen inventory (sub-phase 1.1) is never executed.
  Instead, only BusinessRequirementsFlow21 (2.1) runs, producing requirements documents
  without any screen-level accessibility analysis. The correct behavior routes to
  ScreenInventoryFlow11 at 1.1, where the nine-node flow performs the screen audit
  and produces per-screen accessibility findings.
