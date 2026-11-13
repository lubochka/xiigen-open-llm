# Layer 3 — Teaching Unit Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B01-L3-013-001 — PairParserUnit: files 013 expansion

```
source_id: xiigen-b01-l3-013-001
pair_kind: teaching_unit
```

A:
  BatchId: "B01-batch-001"
  Files: ["layer-0a-compiler-pairs-013.md", "layer-0b-router-pairs-013.md", "layer-0c-knowledge-edge-pairs-013.md", "layer-0d-flow-factory-pairs-013.md", "layer-1-transition-orchestrator-pairs-013.md", "layer-2-nine-node-1.1-pairs-013.md", "layer-3-teaching-unit-pairs-013.md"]

B:
  ParseStatus: "ready"
  ParsedPairs:
    - Layer: "0-A" | PairCount: 5 | File: "013" | Theme: "UserProfilePage UI cases"
    - Layer: "0-B" | PairCount: 4 | File: "013" | Theme: "UserProfilePage routing"
    - Layer: "0-C" | PairCount: 3 | File: "013" | Theme: "boundary: navigation vs. button"
    - Layer: "0-D" | PairCount: 3 | File: "013" | Theme: "trusted_learned, deterministic, probationary_rejected"
    - Layer: "1" | PairCount: 4 | File: "013" | Theme: "UserProfilePage, NotificationCenter, Settings orchestration"
    - Layer: "2" | PairCount: 9 | File: "013" | Theme: "UserProfilePage nine-node"
    - Layer: "3" | PairCount: 3 | File: "013" | Theme: "teaching unit 013"
  TotalPairCount: 31
  ChainIntegrityCheck: "passed"

---

## B01-L3-013-002 — ModelTrainerUnit: files 013 training metrics

```
source_id: xiigen-b01-l3-013-002
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser — 31 pairs, file 013]

B:
  TrainingStatus: "complete"
  Metrics:
    total_pairs_ingested: "31"
    pairs_accepted: "31"
    pairs_rejected: "0"
    knowledge_edge_pairs: "3"
    chain_integrity: "passed"
    min_confidence: "0.88"
    gate_b_passed: "true"
  GateBAssertions:
    - Assertion: "total_pairs >= 20" | Result: "pass (31)"
    - Assertion: "pairs_rejected == 0" | Result: "pass"
    - Assertion: "knowledge_edge_pairs >= 1" | Result: "pass (3)"
  WaveAcceptanceVerdict: "ACCEPTED"

---

## B01-L3-013-003 — Teaching summary: file 013 key lessons

```
source_id: xiigen-b01-l3-013-003
pair_kind: teaching_unit
```

Key lessons from file 013 expansion:
  1. "UI" (direct term) fires the UI predicate — it is in its own lexicon
  2. "navigation" does NOT fire UI — even when immediately followed by "page"
  3. trusted_learned selection produces empty Warnings; probationary_accepted has 3 warnings
  4. FallbackUsed: false for deterministic_generic (no candidate ever proposed)
  5. Node 01 A must always be SelectedLogicalTransition — never raw text
