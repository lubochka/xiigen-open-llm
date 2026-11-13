# Held-Out and Corruption Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: held_out | corruption
# Focus: HO "responsive layout typography" → UI; COR: typography not in UI

---

## Section 1 — Held-Out Pair HO-010-A

```
source_id: xiigen-b01-held-out-010-a
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-010-a"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the responsive layout and typography system for the onboarding screens across all breakpoints."
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
  "responsive" → UI (term 10). "layout" → UI (term 8). "typography" → UI (term 19).
  "screens" → UI (term 4). "breakpoints" — not a UI lexicon term; context modifier.
  Four UI terms present. "Design" without a structural noun does not fire ARCHITECTURE
  in this context — layout and typography are UI nouns.
  No DOCUMENT verb present. Result: UI alone → 1.1.

---

## Section 1 — Held-Out Pair HO-010-B

```
source_id: xiigen-b01-held-out-010-b
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-010-b"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the typography and color system for the onboarding design system and document the usage rules."
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
  "typography" → UI (term 19). "color" → UI (term 18). "design-system" → UI (term 21).
  "document" → DOCUMENT. "Specify" — not DOCUMENT (see pair 004; "Specify" ≠ "specification").
  But "document" fires DOCUMENT. Two predicates → two routing decisions.

---

## Section 2 — Corruption Pair COR-010

```
source_id: xiigen-b01-corruption-010
source_license_state: first_party
pair_kind: corruption
corruption_type: Corruption-v6-01-typography-not-recognized-as-UI
```

A:
  DocumentId: "B01-010-typo-prompt"
  CompiledText: "Define the typography scale for the onboarding screens — heading levels, body text, captions, and link styles."
  CompiledPredicateIds_correct: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.85
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1
  Note: "'typography' not recognized as UI term; 'screens' missed"

Corruption type: Corruption-v6-01 — typography and screens not recognized as UI terms.
  "typography" is term 19 in the UI lexicon. "screens" is term 4. Both are tier-1 UI members.
  The corrupted output fires only DOCUMENT via "define" and routes only to 2.1.

Why this is harmful:
  The screen inventory work for the typography system (sub-phase 1.1, ScreenInventoryFlow11)
  is never executed. "typography" is a first-class UI lexicon member — it controls visual
  rendering of every screen in the inventory. The correct output fires UI via "typography"
  (and confirms via "screens"), producing both 1.1 and 2.1 routing decisions.
