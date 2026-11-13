# Layer 0-A — Compiler Pairs (048)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-048-001 — Long PM sentence with only Round 3 vocabulary (037-048)

**Input:** "Manage the escalation process, coordinate the steering committee, track scope creep, run the post-mortem, and oversee sign-off."

Token scan:
- "Manage" → gap
- "escalation process" → gap
- "coordinate" → gap
- "steering committee" → gap
- "track" → NOT TEST → gap
- "scope creep" → gap
- "run" → gap
- "post-mortem" → gap
- "oversee" → gap
- "sign-off" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-048-002 — Combined Round 1+2+3 long PM sentence — zero predicates

**Input:** "Track milestones, coordinate blockers, manage the risk log, run the escalation process, facilitate project kickoff, and coordinate the change control board."

Token scan: all gap vocabulary → Predicates: []
DomainId: "project-management"
SCL: emitted (once, not per gap ID)

---

## B37-0A-048-003 — Combined Round 1+2+3 with one predicate trigger

**Input:** "Track milestones, coordinate blockers, manage the risk log, run the escalation process, and document the communication plan for stakeholders."

Token scan:
- All managing vocabulary → gap
- "document" → DOCUMENT fires
- "communication plan" → gap
- "stakeholders" → DOCUMENT fires

ONE DOCUMENT predicate (despite two triggers)
Predicates: ["DOCUMENT"]
DomainId: "software-development"
SCL: NOT emitted

---

## B37-0A-048-004 — "QA" fires on quality gate review even with PM context

**Input:** "Run the quality gate review, track sign-off, and manage escalations."

Token scan:
- "Run" → gap
- "quality gate" → gap
- "review" → QA fires
- "track" → NOT TEST → gap
- "sign-off" → gap
- "manage" → gap
- "escalations" → gap

Predicates: ["QA"]
DomainId: "software-development"
GapIds: manage-delivery-001, track-001, escalation-001 forwarded

---

## B37-0A-048-005 — v6 corruption types for Round 3 vocabulary (037-048)

**v6-01 examples:**
- "Manage escalation process" → ["ESCALATION"] — invented predicate
- "Run post-mortem" → ["POST_MORTEM"] — invented predicate
- "Coordinate change control board" → ["CHANGE_CONTROL"] — invented predicate
Correct for all three: Predicates: []

**v6-02 examples:**
- "Send communication plan to stakeholders" → DOCUMENT suppressed — v6-02
- "Escalate to stakeholders" → DOCUMENT suppressed — v6-02
Correct: DOCUMENT fires on "stakeholders" — no suppression.

**v6-03 examples:**
- "Define escalation criteria" → DomainId "project-management" — v6-03 (DOCUMENT fired)
- "Review quality gate" → DomainId "project-management" — v6-03 (QA fired)
Correct: DomainId "software-development" when any predicate fires.
