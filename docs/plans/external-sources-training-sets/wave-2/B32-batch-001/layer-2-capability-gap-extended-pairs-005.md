# Layer 2 — Capability Gap Extended Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-005-001 — "hack together" / "throwaway": full knowledge_edge trace

**Request:** "Hack together a quick throwaway solution for the cache invalidation."

**Layer 0-A (Compiler) trace:**
```
Token: "hack together" → gap-vibe-coding-hack-001 (NOT in action predicate lexicon)
Token: "quick" → general vibe qualifier
Token: "throwaway" → gap-vibe-coding-throwaway-001
Token: "solution" → domain noun; not an action predicate
Token: "cache invalidation" → domain noun; not an action predicate

Action predicate scan:
- implement: NO
- build: NO
- create: NO
- write: NO
- code: NO (no root match in this request)

Predicates: []
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-throwaway-001]
```

**Layer 0-B (Router) trace:**
```
Predicates: [] → knowledge_edge
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: emit (once)
```

**Layer 0-D (Factory):**
```
Factory: none (bypassed)
```

**Layer 1 (Orchestrator):**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapIds: [gap-vibe-coding-hack-001, gap-vibe-coding-throwaway-001]
```

**Layer 2 (Capability Gap):**
```
CapabilityGapType: vocabulary_gap
GapCategory: informal_assembly_vocabulary
UnrecognizedTerms: ["hack together", "throwaway"]
Learning signal: SCHEDULE_CAPABILITY_LEARNING for software-development domain
```

**Final result:** knowledge_edge

---

## B32-L2-005-002 — "hack" variations: all produce knowledge_edge without action verb

| Request | Predicates | Route |
|---|---|---|
| "Hack together a solution" | [] | knowledge_edge |
| "Cobble together a service" | [] | knowledge_edge |
| "Throw together a handler" | [] | knowledge_edge |
| "Build something — hack it together" | [IMPLEMENT] | BackendServiceFlow41 |

The last case fires IMPLEMENT because "build" is present. "Hack it together" is gap framing but does not suppress "build".

---

## B32-L2-005-003 — Extended capability gap analysis for "hack" vocabulary

**User intent:** The user wants to assemble something quickly without following best practices. They are indicating that the quality bar is low and the solution is informal.

**What the system needs to learn:** "hack together", "cobble together", "throw together" are informal assembly verbs. The learning pipeline should evaluate whether to:
1. Map them to IMPLEMENT (treating informal assembly as implementation)
2. Register them as recognized style qualifiers (gap warnings with no predicate change)
3. Leave them as unrecognized gap vocabulary

Current treatment: unrecognized gap vocabulary → knowledge_edge when no other action verbs present.

---
