# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-009-001 — v6 corruption types in B32: complete reference

**Knowledge edge:** Three distinct v6 corruption patterns exist for B32. Each represents a different type of systematic error.

### v6-01: VIBE_CODING predicate invented

**Pattern:** The system creates a new predicate "VIBE_CODING" and a new flow "VibeCodingFlow" to handle vibe requests.

**Why it occurs:** The model generalizes incorrectly — seeing that vibe vocabulary consistently appears in B32 requests, it invents a predicate for it.

**Why it is wrong:**
- "VIBE_CODING" is not in the registered predicate set
- No "VibeCodingFlow" exists in the flow registry
- The system must operate in closed-world mode: only registered predicates and flows are valid
- "vibe" vocabulary is gap by definition; it belongs in the learning system, not the routing system

**Detection:** Any output containing "VIBE_CODING" in the Predicates array is v6-01 corrupt.

---

## B32-0C-009-002 — v6-02: predicate suppressed by style framing

**Pattern:** The system correctly identifies an action predicate (IMPLEMENT, ARCHITECTURE, etc.) but then suppresses it because the request contains vibe/style framing.

**Example of v6-02:**
```
Request: "Just make it work — implement a session handler."
WRONG output: Predicates: [] (IMPLEMENT suppressed by "just make it work")
CORRECT output: Predicates: ["IMPLEMENT"] (style framing cannot suppress predicates)
```

**Why it occurs:** The model treats style qualifiers as overrides rather than annotations.

**Why it is wrong:**
- Rule 2 of B32 Core Rules: style qualifiers NEVER suppress action predicates
- "just make it work" → gap warning; no effect on predicate detection
- This corruption causes valid IMPLEMENT/UI/ARCHITECTURE requests to be misrouted to knowledge_edge

**Detection:** Any case where a registered action verb is present but predicates = [] is v6-02 suspect (unless the action verb genuinely fails to fire on its own merits).

---

## B32-0C-009-003 — v6-03: DomainId wrongly set to "project-management" or invented value

**Pattern:** The system sets DomainId to "project-management" (B36/B37 value) for a B32 vibe coding gap request.

**Example of v6-03:**
```
Request: "Vibe code a spike prototype — throw it together."
WRONG output: DomainId: "project-management"
CORRECT output: DomainId: "software-development"
```

**Why it occurs:** The model conflates vibe coding (which is informal/unstructured, like some project management conversations) with project management.

**Why it is wrong:**
- B32 is a software-development scenario
- DomainId drives which learning pipeline receives SCHEDULE_CAPABILITY_LEARNING
- "project-management" sends the signal to the wrong pipeline

**Detection:** Any B32 zero-predicate output with DomainId ≠ "software-development" is v6-03 corrupt.

---

## B32-0C-009-004 — Corruption summary and detection rules

| Corruption | Detection signal | Correct behavior |
|---|---|---|
| v6-01 | "VIBE_CODING" in Predicates array | Gap warning only; no predicate |
| v6-02 | Action verb present, Predicates = [] | Predicate fires; style → warning |
| v6-03 | DomainId ≠ "software-development" for B32 zero-pred | Always "software-development" |

---
