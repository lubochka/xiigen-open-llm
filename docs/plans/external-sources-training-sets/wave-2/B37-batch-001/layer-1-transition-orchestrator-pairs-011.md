# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-011-001 — Canonical B37 request patterns: orchestrator reference

**Pattern A: Pure gap (most common)**
Vocabulary: track, coordinate, blockers, escalation, progress, RAG-status, RAID, follow-up, action items, on-track, off-track
Orchestrator output: TransitionType "unknown", knowledge_edge, DomainId "project-management", SCL emitted

**Pattern B: Stakeholder mention**
Vocabulary: stakeholders + managing framing
Orchestrator output: TransitionType "documentation", DocumentationFlow, DomainId "software-development"

**Pattern C: Governance structure**
Vocabulary: structure + define/document + managing framing
Orchestrator output: TransitionType "architecture_design + documentation", 3.1+2.1, DomainId "software-development"

**Pattern D: Build in managing sentence**
Vocabulary: build/service/module + managing framing
Orchestrator output: TransitionType "feature_addition", BackendServiceFlow41, DomainId "software-development"

**Pattern E: Review in managing sentence**
Vocabulary: review + managing framing
Orchestrator output: TransitionType "no_contribution", ReviewFlow, DomainId "software-development"

---

## B37-L1-011-002 — Pattern frequency in B37

**Expected frequency of orchestrator outputs for B37 requests:**

1. knowledge_edge (TransitionType "unknown") — ~60% of B37 requests (pure managing vocabulary)
2. documentation only — ~15% (stakeholder mentions)
3. architecture_design + documentation — ~10% (governance structure sentences)
4. feature_addition — ~8% (build/service mentions)
5. no_contribution (QA) — ~5% (review mentions)
6. Other combinations — ~2%

The dominant orchestrator output for B37 is knowledge_edge. This is the expected pattern for a capability gap scenario.

---

## B37-L1-011-003 — Orchestrator stability: managing framing does not change output

**Stability test:** Adding managing vocabulary to a predicate-triggering sentence does not change the orchestrator output.

Base: "Define the structure." → architecture_design + documentation
Modified: "Define the accountability structure for the project delivery team." → architecture_design + documentation (same)
Modified: "Define the governance structure, coordinate teams, and track escalations." → architecture_design + documentation (same; coordinate and track are gap)

**Rule:** Managing vocabulary in the sentence does not modify TransitionType or DomainId. Only predicate-firing tokens affect orchestrator output.

---

## B37-L1-011-004 — Orchestrator handling of compound gap + predicate sentence

**Input:** "Coordinate escalations, track blockers, notify stakeholders, and build the tracking module."

Token-by-token:
- "Coordinate" → gap (Rule 11)
- "escalations" → gap (Rule 13)
- "track" → gap (Rule 2)
- "blockers" → gap
- "notify" → not in DOCUMENT trigger list
- "stakeholders" → DOCUMENT fires (Rule 6)
- "build" → IMPLEMENT fires (Rule 9)
- "tracking" → gap (track family)
- "module" → IMPLEMENT fires (Rule 9)

Result: ["DOCUMENT", "IMPLEMENT"]

**Orchestrator:**
- TransitionType: documentation + feature_addition
- Next states: DocumentationFlow (2.1) + BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Parallel: Yes
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-track-status-001, gap-project-managing-blockers-001, gap-project-managing-stakeholder-update-001

---

## B37-L1-011-005 — Orchestrator gap warning handling rules

**Gap warning handling in the orchestrator:**

1. Gap IDs collected during compiler scan
2. Gap IDs forwarded to orchestrator as metadata
3. Orchestrator attaches gap IDs to the transition event
4. Gap IDs are NOT routing instructions — they do not create additional transitions
5. Gap IDs are NOT consumed by the flow — they persist as metadata
6. Gap IDs are forwarded to the SCHEDULE_CAPABILITY_LEARNING payload (if SCL emitted)
7. Gap IDs are forwarded to the flow context (if predicate fires) for advisory logging
