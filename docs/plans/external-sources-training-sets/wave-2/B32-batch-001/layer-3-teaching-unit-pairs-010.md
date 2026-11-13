# Layer 3 — Teaching Unit Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-010-001 — v6 corruption patterns for B32: teaching reference

**Teaching unit:** Three v6 corruption patterns are specific to B32. Each represents a systematic error that a model might make when processing vibe-coded requests.

---

## B32-L3-010-002 — v6-01: VIBE_CODING predicate invented

**What the corruption does:**
```
Input: "Vibe code the service."
v6-01 output: Predicates: ["VIBE_CODING"], RouteTarget: VibeCodingFlow
```

**Why a model makes this error:**
The model recognizes that "vibe coding" is a pattern in B32 requests and generalizes it into a predicate, similar to how "build" maps to IMPLEMENT. The model creates a VIBE_CODING predicate by analogy.

**Why it is wrong:**
- Only registered predicates can appear in the output
- "VIBE_CODING" is not registered
- "VibeCodingFlow" does not exist
- The closed-world assumption is violated

**Correct output:**
```
Predicates: ["IMPLEMENT"] (from "code" root match)
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: BackendServiceFlow41
```

---

## B32-L3-010-003 — v6-02: IMPLEMENT suppressed by vibe framing

**What the corruption does:**
```
Input: "Just make it work — implement the handler."
v6-02 output: Predicates: [] (IMPLEMENT suppressed)
```

**Why a model makes this error:**
The model treats the sentence holistically and concludes that "just make it work" signals an informal, low-priority request where formal routing should be bypassed.

**Why it is wrong:**
- "implement" is a direct IMPLEMENT lexicon match
- Style framing cannot suppress registered predicates
- This causes a valid implementation request to route to knowledge_edge

**Correct output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-just-works-001]
RouteTarget: BackendServiceFlow41
```

---

## B32-L3-010-004 — v6-03: DomainId "project-management" for B32

**What the corruption does:**
```
Input: "Spike the auth module implementation."
v6-03 output: DomainId: "project-management"
```

**Why a model makes this error:**
The model associates informal, exploratory development (spikes, prototypes) with project management planning activities. Both are "planning" in some sense.

**Why it is wrong:**
- B32 is software-development, not project-management
- "spike" is a software development investigation, not a project management concept
- The incorrect DomainId sends SCHEDULE_CAPABILITY_LEARNING to the wrong pipeline

**Correct output:**
```
DomainId: "software-development"
```

**Summary of all three corruptions:**
| Type | Error | Impact |
|---|---|---|
| v6-01 | Invents VIBE_CODING predicate | Unregistered predicate; factory rejects |
| v6-02 | Suppresses IMPLEMENT | Valid requests route to knowledge_edge |
| v6-03 | Wrong DomainId | Learning signals misrouted |

---
