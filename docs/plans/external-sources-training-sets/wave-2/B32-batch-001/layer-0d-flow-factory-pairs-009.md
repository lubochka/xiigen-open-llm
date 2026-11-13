# Layer 0-D — Flow Factory Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-009-001 — probationary_rejected: vibe-specific flow fails Gate B

**Scenario:** A factory is correctly selected (BackendServiceFlow41 for IMPLEMENT), but the factory's internal Gate B check fails because the request is too vague for the factory to construct a valid flow.

**Example request:** "Build something."

**Analysis:**
- "build" → IMPLEMENT fires → BackendServiceFlow41 selected
- No gap vocabulary (no vibe framing)
- Factory receives: {predicate: IMPLEMENT, request: "Build something."}
- Gate B check: "what is being built?" → insufficient context → Gate B fails

**Output:**
```
Factory: BackendServiceFlow41
GateBCheck: FAILED
Reason: insufficient context for flow construction
Status: probationary_rejected
Fallback: request clarification
```

**Note:** This is NOT a gap scenario. This is a factory Gate B failure. It is distinct from B32 zero-predicate gap cases. The predicate fired correctly; the factory failed due to insufficient request specificity.

---

## B32-0D-009-002 — Vibe framing + insufficient context: compound issue

**Example request:** "Vibe code something quick."

**Analysis:**
- "code" root → IMPLEMENT fires
- "vibe" → gap-vibe-coding-style-001
- "something" → no specific subject for the factory
- Gate B: what is being coded? → insufficient context

**Output:**
```
Factory: BackendServiceFlow41
GateBCheck: FAILED
Reason: insufficient context for flow construction
GapMetadata: {vibe-style: true}
Status: probationary_rejected
```

Both a gap warning and a Gate B failure can occur simultaneously. The gap warning is independent of the Gate B check.

---

## B32-0D-009-003 — probationary_rejected vs knowledge_edge: key distinction

| Status | Cause | Predicates | Factory |
|---|---|---|---|
| knowledge_edge | Zero predicates (gap vocabulary only) | [] | not invoked |
| probationary_rejected | Factory Gate B failure | non-empty | invoked, failed |

These are two different failure modes. knowledge_edge is the gap path. probationary_rejected is the factory validation failure path.

---

## B32-0D-009-004 — Recovery from probationary_rejected

**When probationary_rejected occurs:**

1. Session logs the failure
2. System prompts for clarification: "What specifically should be built?"
3. User provides more context
4. Factory is re-invoked with enriched request
5. Gate B re-evaluated
6. If Gate B passes: flow proceeds
7. If Gate B fails again: escalate or abandon

Gap warnings persist through the recovery cycle. The vibe framing is still noted even after clarification.

---
