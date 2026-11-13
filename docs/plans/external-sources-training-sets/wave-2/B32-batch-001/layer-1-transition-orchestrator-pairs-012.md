# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-012-001 — v6 ordering violations for B32

**v6 ordering violation type 1: QA placed before IMPLEMENT**

**CORRUPTED orchestration:**
```
FlowOrder: [QAFlow, BackendServiceFlow41, TestFlow]  ← QA first (WRONG)
```

**CORRECT:**
```
FlowOrder: [BackendServiceFlow41, TestFlow, QAFlow]  ← QA last
```

**Why this occurs in v6:** The model processes predicates in input order ("QA the implementation" appeared before "build the service" in some v6 training examples).

**Why it is wrong:** QA depends on implementation output. It cannot execute before the implementation exists.

---

## B32-L1-012-002 — v6 ordering violation type 2: parallel IMPLEMENT and TEST

**CORRUPTED orchestration:**
```
FlowOrder: [BackendServiceFlow41 (parallel with) TestFlow]  ← parallel (WRONG)
```

**CORRECT:**
```
FlowOrder: Phase 1: [BackendServiceFlow41] → Phase 2: [TestFlow]
```

**Why it is wrong:** Tests test the implementation. TestFlow cannot run in parallel with BackendServiceFlow41 because it needs the implementation to exist first.

---

## B32-L1-012-003 — v6 ordering violation type 3: ARCHITECTURE after IMPLEMENT

**CORRUPTED orchestration:**
```
FlowOrder: [BackendServiceFlow41, ArchitectureFlow31]  ← Architecture after implementation (WRONG)
```

**CORRECT:**
```
FlowOrder: Phase 1: [ArchitectureFlow31] → Phase 2: [BackendServiceFlow41]
```

**Why it is wrong:** Architecture should guide implementation. If ARCHITECTURE and IMPLEMENT are both present, the architecture flow should produce output that IMPLEMENT can reference.

---

## B32-L1-012-004 — Ordering invariants summary for B32

| Rule | Description |
|---|---|
| QA last | QAFlow is always in the final phase |
| TEST before QA | TestFlow precedes QAFlow |
| IMPLEMENT before TEST | BackendServiceFlow41 precedes TestFlow |
| ARCHITECTURE before IMPLEMENT | ArchitectureFlow31 precedes or is parallel with IMPLEMENT phase |
| ARCHITECTURE || API_CONTRACT | These two can be parallel |
| UI || IMPLEMENT | These two can be parallel |

These ordering rules are B32-invariant: vibe framing never changes them.

---
