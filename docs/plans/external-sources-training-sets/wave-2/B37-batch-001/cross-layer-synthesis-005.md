# Cross-Layer Synthesis (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B37-CLS-005-001 — Pure gap: "track team progress" → knowledge_edge, DomainId: project-managing

**Input:** "Track team progress across all delivery workstreams and coordinate blockers."

**Full cross-layer trace: zero predicates → knowledge_edge, DomainId: project-managing**

---

### Layer 0-A (Compiler)

Token scan:
- "Track" → NOT in TEST lexicon (Rule 2: track ≠ TEST) → gap
- "team progress" → "progress" not in any predicate lexicon (Rule 12) → gap
- "delivery workstreams" → gap
- "coordinate" → not in any predicate lexicon (Rule 11) → gap
- "blockers" → not in any predicate lexicon → gap

Predicate list: []

---

### Layer 0-B (Router)

Predicate list empty → knowledge_edge
DomainId: "project-management"
Route: knowledge_edge

---

### Layer 0-C (Knowledge Edge)

Gap vocabulary identified:
- "track" → gap-project-managing-track-001
- "team progress" → gap-project-managing-track-progress-001 (compound), gap-project-managing-progress-001
- "delivery workstreams" → gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001
- "coordinate" → gap-project-managing-coordinate-001
- "blockers" → gap-project-managing-blockers-001
- "coordinate + blockers" → gap-project-managing-coordinate-blockers-001 (compound)

Knowledge edge payload assembled.

---

### Layer 0-D (Flow Factory)

Zero predicates → No factory selected.
knowledge_edge path activated.
SCHEDULE_CAPABILITY_LEARNING prepared.

---

### Layer 1 (Transition Orchestrator)

TransitionType: unknown
ContextCarryForward: false
Next state: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted

SCL payload:
- event_type: SCHEDULE_CAPABILITY_LEARNING
- scenario: B37
- domain: project-management
- gap_ids: [gap-project-managing-track-progress-001, gap-project-managing-track-001, gap-project-managing-progress-001, gap-project-managing-coordinate-blockers-001, gap-project-managing-coordinate-001, gap-project-managing-blockers-001, gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001]

---

### Layer 2 (Capability Gap Extended)

No nine-node flow generated (capability gap scenario).
Extended pair: knowledge_edge trace recorded.
Gap IDs forwarded to learning pipeline.

---

## B37-CLS-005-002 — Full output summary

```
Predicates: []
DomainId: "project-management"
Route: knowledge_edge
TransitionType: unknown
ContextCarryForward: false
SCHEDULE_CAPABILITY_LEARNING: emitted (1 event)
Gap IDs: [
  gap-project-managing-track-progress-001,
  gap-project-managing-track-001,
  gap-project-managing-progress-001,
  gap-project-managing-coordinate-blockers-001,
  gap-project-managing-coordinate-001,
  gap-project-managing-blockers-001,
  gap-project-managing-manage-delivery-001,
  gap-project-managing-work-stream-001
]
```

---

## B37-CLS-005-003 — Why this is a canonical knowledge_edge trace

"Track team progress" combines three of the most critical B37 zero-predicate tokens:
- "track" — the most important B37 boundary (track ≠ TEST)
- "progress" — zero-predicate confirmation
- "coordinate blockers" — compound gap ID trigger

No incidental predicate-firing tokens. Pure managing vocabulary. This is the expected output for pure managing requests.

---

## B37-CLS-005-004 — v6 failure modes

| v6 corruption | What goes wrong | Correct value |
|--------------|----------------|--------------|
| v6-01 | Predicates: ["MANAGEMENT"] | [] |
| v6-01 | Predicates: ["TEST"] — "track" fires TEST | [] |
| v6-03 | DomainId "project-management" with invented predicate | DomainId "project-management" is correct for [] |

v6-01 (TEST from "track") is the highest-risk corruption for this specific trace.
