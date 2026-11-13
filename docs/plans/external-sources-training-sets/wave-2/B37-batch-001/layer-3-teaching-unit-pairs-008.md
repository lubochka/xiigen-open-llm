# Layer 3 — Teaching-Unit Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-008-001 — Teaching: predicate dominance in project managing sentences

**Core concept:** When a predicate-firing token appears in a managing sentence, the predicate dominates. Managing vocabulary becomes gap warnings. The predicate determines routing.

**Three examples of predicate dominance:**

Example 1: "Review the RAID log and escalate blockers."
- "Review" → QA dominates → ["QA"], DomainId "software-development"
- RAID + escalate + blockers → gap warnings (dominated, not routing)

Example 2: "Update stakeholders on delivery progress."
- "stakeholders" → DOCUMENT dominates → ["DOCUMENT"], DomainId "software-development"
- delivery + progress → gap warnings

Example 3: "Build the tracking service for the delivery team."
- "build" + "service" → IMPLEMENT dominates → ["IMPLEMENT"], DomainId "software-development"
- tracking + delivery → gap warnings

---

## B37-L3-008-002 — Teaching: dominant predicate + dominated gap vocabulary

**Framework for B37 mixed sentences:**

| Component | Role | Effect |
|-----------|------|--------|
| Predicate-firing token | Dominant | Sets predicate, routing, DomainId |
| Gap vocabulary token | Dominated | Becomes gap warning only |
| Managing framing | Dominated | Does not affect predicate or routing |

"Manage the delivery of the [service]" → "service" dominates; "manage the delivery of" is dominated managing framing.

---

## B37-L3-008-003 — Teaching: predicate count and dominance

**Multiple predicates, multiple dominants:**

"Build the service, review the criteria, and notify stakeholders of the escalation."

- "build" → IMPLEMENT (dominant 1)
- "service" → IMPLEMENT (confirms dominant 1)
- "review" → QA (dominant 2)
- "criteria" → DOCUMENT (dominant 3)
- "stakeholders" → DOCUMENT (confirms dominant 3)
- "escalation" → gap (dominated)
- "notify" → not in DOCUMENT trigger (gap context)

Result: ["IMPLEMENT", "QA", "DOCUMENT"], DomainId "software-development"

Three dominants, one dominated (escalation → gap warning).

---

## B37-L3-008-004 — Teaching: zero dominants = full gap

**When no predicate-firing token is present, there are no dominants:**

"Manage delivery, track progress, coordinate workstreams, escalate blockers."

- "Manage" → dominated (gap — no dominant)
- "delivery" → dominated
- "track" → dominated (not TEST)
- "progress" → dominated
- "coordinate" → dominated
- "workstreams" → dominated
- "escalate" → dominated
- "blockers" → dominated

Result: [] — no dominants → all gap → knowledge_edge

---

## B37-L3-008-005 — Teaching: dominance is not about token count

**Wrong assumption:** "There are more managing words than predicate words, so managing should win."

The predicate dominance is not majority voting. ONE predicate token is sufficient to:
- Set a predicate
- Route to a flow
- Set DomainId "software-development"
- Prevent SCL from emitting

One "stakeholders" token in a ten-word managing sentence fires DOCUMENT and routes to DocumentationFlow. The nine managing words do not outvote the one "stakeholders" token.
