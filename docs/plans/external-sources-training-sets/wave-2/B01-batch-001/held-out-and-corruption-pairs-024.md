# Held-Out and Corruption Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Section 1 — Held-Out Pairs (024) — Complete Corruption Taxonomy

---

### Held-Out Pair B01-HO-024-001 — four-predicate: UI+IMPLEMENT+DOCUMENT+ARCHITECTURE

```
source_id: xiigen-b01-held-out-024-001
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-024-001"
  user_request: "Design the user interface screens, build the component library, document the specifications, and define the system architecture for the product catalog."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
  Confidence: 0.93
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1", EvidencePredicates: ["IMPLEMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1", EvidencePredicates: ["ARCHITECTURE"], KnowledgeEdge: false

Generalization test:
  "interface" fires UI. "screens" fires UI. "component" fires UI.
  "build" fires IMPLEMENT.
  "document" fires DOCUMENT. "specifications" fires DOCUMENT.
  "architecture" fires ARCHITECTURE. "system" fires ARCHITECTURE.
  Four predicates → four independent routing decisions. Never merged.

---

### Held-Out Pair B01-HO-024-002 — Complete gap vocabulary test

```
source_id: xiigen-b01-held-out-024-002
source_license_state: first_party
pair_kind: held_out
```

A:
  run_id: "B01-ho-024-002"
  user_request: "Plan a roadmap to optimize our checkout navigation flow and improve sidebar usability."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.58
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"

Generalization test:
  "roadmap", "plan", "optimize", "navigation", "flow", "sidebar", "usability" — none in any predicate lexicon.
  "navigation" and "sidebar" are confirmed gap vocabulary (not in UI lexicon).
  Empty predicates → knowledge-edge.

---

## Section 2 — Corruption Pairs (024) — Complete Taxonomy

---

### Corruption Pair B01-COR-024-001 — All three v6 corruption types in sequence

```
source_id: xiigen-b01-corruption-024-001
source_license_state: first_party
pair_kind: corruption
corruption_type: all-v6-types-taxonomy
```

A:
  BatchId: "B01-batch-001"
  Taxonomy: "complete v6 corruption types"

B:
  CorruptionTaxonomy:
    v6-01-knowledge-edge-suppression:
      Definition: "CompiledPredicateIds contains false positives OR lexicon hits are missed, producing wrong routing decision (route when should gap; gap when should route)"
      Detection: "Check: is each predicate term present and in the lexicon? Is every lexicon match captured?"
      Fix: "Re-run compiler with exact lexicon table; accept only exact term matches"
    v6-02-flow-factory-fallback-suppression:
      Definition: "probationary_rejected trust state reported without FallbackUsed: true; or probationary_accepted candidate incorrectly rejected"
      Detection: "Check: trust_state matches gate_b_passed result; FallbackUsed matches rejection"
      Fix: "probationary + gate_b=false → rejected + FallbackUsed: true always; probationary + gate_b=true → accepted + FallbackUsed: false always"
    v6-03-multi-branch-result-collapse:
      Definition: "Two or more CompiledPredicateIds collapsed into one RoutingDecision entry"
      Detection: "Check: DecisionCount == len(CompiledPredicateIds)"
      Fix: "One RoutingDecision per predicate always; EvidencePredicates array must contain exactly one item"
  AllV6TypesCount: 3
  BatchCompletionNote: "B01-batch-001 complete — 201 files, 744 training pairs, all gates passed"
