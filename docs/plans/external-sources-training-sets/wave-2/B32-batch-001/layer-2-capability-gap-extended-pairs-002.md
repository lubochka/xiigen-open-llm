# Layer 2 — Capability Gap Extended Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-002-001 — "spike" request: full knowledge_edge trace with DomainId: software-development

**Request:** "Run a spike to explore the best approach for the recommendation engine."

### Full Layer Trace

**Layer 0-A (Compiler):**
```
Input: "Run a spike to explore the best approach for the recommendation engine."
Scan action predicates: "spike" → NOT in lexicon; "explore" → NOT in IMPLEMENT lexicon
Scan UI vocabulary: none detected
Scan ARCHITECTURE vocabulary: none detected
Scan other predicate lexicons: none detected
Gap vocabulary detected: "spike" → gap-vibe-coding-spike-001; "explore" → gap-vibe-coding-exploratory-001
Predicates: []
GapWarnings: [gap-vibe-coding-spike-001, gap-vibe-coding-exploratory-001]
```

**Layer 0-B (Router):**
```
Predicates: [] → route to knowledge_edge
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: emit once
FlowSelected: none
```

**Layer 0-D (Factory):**
```
Predicates: [] → factory bypassed
FlowFactory: none
```

**Layer 1 (Orchestrator):**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapIds: [gap-vibe-coding-spike-001, gap-vibe-coding-exploratory-001]
ContributionFlag: false
```

**Layer 2 (Capability Gap):**
```
CapabilityGapType: vocabulary_gap
GapCategory: exploratory_investigation_vocabulary
UnrecognizedTerms: ["spike", "explore"]
RegisteredGapIds: [gap-vibe-coding-spike-001, gap-vibe-coding-exploratory-001]
DomainId: "software-development"
LearningPriority: medium
TrainingDataNeeded: true
```

**Final result:** knowledge_edge + SCHEDULE_CAPABILITY_LEARNING (DomainId: software-development)

---

## B32-L2-002-002 — "spike" extended gap analysis

**Gap category:** exploratory_investigation_vocabulary

**What the user is trying to express:**
A "spike" in software development is a time-boxed investigation to reduce uncertainty about a technical approach. The user wants to explore options before committing to implementation.

**Why the system cannot route it:**
The system has no "INVESTIGATE" or "EXPLORE" predicate. Technical investigation vocabulary is not in the predicate lexicon. The learning pipeline should consider whether a new predicate (e.g., INVESTIGATE or SPIKE) should be added.

**Suggested learning action:**
1. Add "spike" to IMPLEMENT lexicon (treating investigation as implementation of a proof-of-concept) OR
2. Register a new INVESTIGATE predicate with "spike", "explore", "investigate", "research" as triggers
3. DomainId for learning: "software-development"

---
