# Held-Out and Corruption Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "design system theme" → UI; COR: theme→gap

---

## Section 1 — Held-Out Pair HO-008-A

```
source_id: xiigen-b01-held-out-008-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-008-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the design system theme for the onboarding flow — token colors, typography scale, spacing grid, and dark/light mode variants."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false

Generalization test:
  "design-system" → UI (term 21, hyphenated form). "theme" → UI (term 22).
  "typography" → UI (term 19). "spacing" → UI (term 20). "colors" — adjacent to "color" (UI term 18).
  Multiple UI terms confirm 1.1 routing.
  "define" → DOCUMENT. Results in [UI, DOCUMENT].

---

## Section 1 — Held-Out Pair HO-008-B

```
source_id: xiigen-b01-held-out-008-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-008-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Update the onboarding screens to use the new brand theme — new color palette, updated typography, revised spacing."
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
  "screens" → UI. "theme" → UI. "color" → UI. "typography" → UI. "spacing" → UI.
  Five UI terms present. "Update" — not in IMPLEMENT lexicon as written here (not "write", "build",
  "implement", "create" in production context); context-dependent. In this request "update"
  signals a visual change request — no IMPLEMENT fired since no explicit implementation verb.
  Result: UI alone → 1.1.

---

## Section 2 — Corruption Pair COR-008

```
source_id: xiigen-b01-corruption-008
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-theme-misidentified-as-gap
```

A:
  DocumentId: "B01-008-theme-prompt"
  CompiledText: "Set up the design system theme with color, typography, and spacing tokens for the onboarding screens."
  CompiledPredicateIds_correct: ["UI"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1

B-corrupt:
  CompiledPredicateIds: []
  Confidence: 0.70
  RoutingDecisions:
    - PredictedSubPhaseId: null
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match"
  DecisionCount: 0
  Note: "'theme', 'color', 'typography', 'spacing', 'design-system', 'screens' all missed"

Corruption type: Corruption-v6-01 — "theme" (and co-present UI terms) misidentified as gap vocabulary.
  In B01, "theme" is term 22 in the UI lexicon. It is NOT a gap term.
  The corrupted output treats "theme" as a context word with no predicate membership,
  ignoring all five co-present UI terms (design-system, color, typography, spacing, screens).

Why this is harmful:
  All five UI terms are missed. The request goes to the knowledge-edge path.
  The design system / theme update for the onboarding screens — a primary B01 screen
  inventory activity — is never executed. The correct behavior fires UI via any one
  of the five terms and routes to ScreenInventoryFlow11 at sub-phase 1.1.
