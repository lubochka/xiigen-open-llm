# Held-Out and Corruption Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-003-001 — HO: "coordinate work streams" → knowledge_edge

**Held-out request:** "Coordinate all work streams and ensure blockers are escalated promptly."

**Expected output:**
- "Coordinate" → gap (Rule 11)
- "work streams" → gap (workstream = gap-project-managing-work-stream-001)
- "blockers" → gap
- "escalated" → gap (escalation family, Rule 13)
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-work-stream-001, gap-project-managing-blockers-001, gap-project-managing-escalation-001

---

## B37-HO-003-002 — COR: "coordinate" fires IMPLEMENT (wrong)

**Corrupted output:** "Coordinate workstreams" → ["IMPLEMENT"] — coordinate falsely mapped to IMPLEMENT

**Why this is wrong:** Rule 11 — "coordinate" is NOT in any predicate lexicon. IMPLEMENT fires on: build, implement, create, write (code), service, module. "coordinate" is not on this list.

**Correct output:** [] (zero predicates)

**Corruption label:** "coordinate" incorrectly firing IMPLEMENT (v6-01 variant — wrong predicate type)

---

## B37-HO-003-003 — HO: "coordinate" with document trigger

**Request:** "Coordinate the team and document the delivery criteria."

**Expected output:**
- "Coordinate" → gap (Rule 11)
- "document" → DOCUMENT fires (Rule 8)
- "delivery" → gap
- "criteria" → DOCUMENT fires (Rule 8)
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001

---

## B37-HO-003-004 — COR: "coordinate" fires MANAGEMENT (wrong)

**Corrupted output:** "Coordinate blockers" → ["MANAGEMENT"] — invented predicate

**Why this is wrong:** MANAGEMENT is not in the predicate registry. v6-01 corruption.

**Correct output:** [] (zero predicates; knowledge_edge; DomainId "project-management")

---

## B37-HO-003-005 — HO: "coordinate" with "API dependencies"

**Request:** "Coordinate the API dependencies between teams and the integration schedule."

**Expected output:**
- "Coordinate" → gap (Rule 11)
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API co-occurrence: "API", "integration")
- "integration" → API co-occurrence token
- "schedule" → not in any predicate lexicon
- Predicates: ["API_CONTRACT"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-coordinate-001

**Key:** "coordinate" is gap; "API dependencies" fires API_CONTRACT. Two different outcomes from the same sentence.
