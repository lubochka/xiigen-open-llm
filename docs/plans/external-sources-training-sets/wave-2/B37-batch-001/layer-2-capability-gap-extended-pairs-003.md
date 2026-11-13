# Layer 2 — Capability-Gap-Extended Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-003-001 — "coordinate blockers": full knowledge_edge trace; compound gap ID

**Input:** "Coordinate with all team leads to resolve the blockers preventing timely delivery."

**Full knowledge_edge trace:**

Step 1 — Compiler scan:
- "Coordinate" → not in any predicate lexicon (Rule 11) → gap
- "team leads" → not in any predicate lexicon
- "resolve" → not in any predicate lexicon
- "blockers" → not in any predicate lexicon → gap
- "preventing" → not in any predicate lexicon
- "timely" → not in any predicate lexicon
- "delivery" → gap

Step 2 — Predicate list: []

Step 3 — Gap mapping:
- "coordinate" → gap-project-managing-coordinate-001
- "blockers" → gap-project-managing-blockers-001
- "coordinate + blockers" → gap-project-managing-coordinate-blockers-001 (compound)
- "delivery" → gap-project-managing-manage-delivery-001

Step 4 — DomainId: "project-management"

Step 5 — Route: knowledge_edge

Step 6 — SCHEDULE_CAPABILITY_LEARNING emitted
- gap_ids: [gap-project-managing-coordinate-blockers-001, gap-project-managing-coordinate-001, gap-project-managing-blockers-001, gap-project-managing-manage-delivery-001]

**Gap: gap-project-managing-coordinate-blockers-001 (compound)**

---

## B37-L2-003-002 — "coordinate" alone: no blockers

**Input:** "Coordinate the release activities across all delivery teams."

**Trace:**
- "Coordinate" → gap (Rule 11)
- "release" → not in any predicate lexicon
- "delivery" → gap
- "teams" → not in any predicate lexicon

Result: []

Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001

**Gap: gap-project-managing-coordinate-001 (individual, no compound)**

---

## B37-L2-003-003 — "coordinate" + "dependencies" without API: two gap IDs

**Input:** "Coordinate the team dependencies and manage the delivery blockers."

**Trace:**
- "Coordinate" → gap (Rule 11)
- "team dependencies" → no API co-occurrence → gap (Rule 5)
- "manage" → gap (Rule 10)
- "delivery" → gap
- "blockers" → gap

Result: []

Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-dependencies-team-001, gap-project-managing-blockers-001

**Gap: gap-project-managing-coordinate-001 + gap-project-managing-dependencies-team-001 + gap-project-managing-blockers-001**

---

## B37-L2-003-004 — "coordinate" + "API dependencies": API_CONTRACT fires on dependencies

**Input:** "Coordinate the API dependencies across teams and manage the delivery."

**Trace:**
- "Coordinate" → gap (Rule 11)
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API co-occurrence: "API" present — Rule 5)
- "manage" → gap (Rule 10)
- "delivery" → gap

Result: ["API_CONTRACT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001

**Key: "coordinate" is gap; "API dependencies" fires API_CONTRACT. Different from "coordinate team dependencies" (gap only).**

---

## B37-L2-003-005 — Compound gap ID assignment rule

**When to use compound gap IDs:**

gap-project-managing-coordinate-blockers-001: Use when both "coordinate" AND "blockers" appear in the same request with compound managing intent.

gap-project-managing-track-progress-001: Use when "track progress" appears as a compound phrase.

gap-project-managing-escalation-accountability-001: Use when both "escalation" AND "accountability" appear together.

**Individual IDs always accompany compound IDs:**
If gap-project-managing-coordinate-blockers-001 is emitted, also emit:
- gap-project-managing-coordinate-001
- gap-project-managing-blockers-001

The compound ID captures the compound managing pattern; the individual IDs capture each component for learning granularity.
