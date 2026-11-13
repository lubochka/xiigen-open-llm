# Layer 3 — Teaching-Unit Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-009-001 — Teaching: "manage", "coordinate", "progress", "escalate" — none in any lexicon

**Four zero-predicate managing tokens:**

**"manage"** (Rule 10): Not in TEST, QA, DOCUMENT, ARCHITECTURE, IMPLEMENT, or API_CONTRACT lexicon.
**"coordinate"** (Rule 11): Not in any predicate lexicon.
**"progress"** (Rule 12): Not in any predicate lexicon.
**"escalate" / "escalation"** (Rule 13): Not in any predicate lexicon.

**These four tokens represent the core B37 managing vocabulary.** They are the most commonly expected managing words, and none of them fire any predicate.

---

## B37-L3-009-002 — Teaching: "manage" in detail

"manage" → Rule 10: NOT in any predicate lexicon.

Variants also not firing:
- manages → not firing
- managed → not firing
- management → not firing
- managing → not firing
- manager → not firing

Example: "Manage the project delivery, the team, and the escalations." → []

Note: "management" in a compound noun like "project management portal" → portal is also not a predicate trigger. "Management" itself never fires.

---

## B37-L3-009-003 — Teaching: "coordinate" in detail

"coordinate" → Rule 11: NOT in any predicate lexicon.

Variants also not firing:
- coordinates → not firing
- coordinated → not firing
- coordination → not firing

Example: "Coordinate all workstream activities and dependencies." → []

Note: "coordinate" is the B37 token most commonly targeted by v6-01 corruption (inventing COORDINATION predicate). The correct response is always zero predicates for "coordinate".

---

## B37-L3-009-004 — Teaching: "progress" and "escalate" in detail

**"progress"** (Rule 12): NOT in any predicate lexicon.
- progress update → gap
- progress report → gap
- track progress → gap (both track and progress are gap)
- delivery progress → gap

**"escalate" / "escalation"** (Rule 13): NOT in any predicate lexicon.
- escalate the risk → gap
- escalation process → gap
- escalation criteria → gap (BUT "criteria" fires DOCUMENT)
- escalation structure → gap (BUT "structure" fires ARCHITECTURE)

**Key distinction for escalation:** "escalation" itself is gap, but "escalation structure" fires ARCHITECTURE (via "structure") and "escalation criteria" fires DOCUMENT (via "criteria").

---

## B37-L3-009-005 — Teaching: exam test cases for the four zero-predicate tokens

**"manage" test:**
Q: "Manage the service delivery." — what fires?
A: ["IMPLEMENT"] — "service" fires IMPLEMENT. "manage" and "delivery" are gap.

**"coordinate" test:**
Q: "Coordinate the API dependencies." — what fires?
A: ["API_CONTRACT"] — "dependencies" + "API" co-occurrence fires API_CONTRACT. "coordinate" is gap.

**"progress" test:**
Q: "Track and report progress to stakeholders." — what fires?
A: ["DOCUMENT"] — "stakeholders" fires DOCUMENT. "track", "progress" are gap.

**"escalation" test:**
Q: "Define the escalation criteria." — what fires?
A: ["DOCUMENT"] — "define" and "criteria" fire DOCUMENT. "escalation" is gap.
