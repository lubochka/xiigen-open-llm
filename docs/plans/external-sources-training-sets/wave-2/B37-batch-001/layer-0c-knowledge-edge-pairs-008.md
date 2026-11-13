# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-008-001 — DomainId: "project-management" for zero-predicate requests in both B36 and B37

**Knowledge edge: DomainId rule for gap requests**

Rule 14: DomainId = "project-management" ONLY when zero predicates fire.
Rule 18: DomainId = "project-management" for zero-predicate requests in BOTH B36 and B37.

**This means:**
- B36 zero-predicate request → DomainId "project-management"
- B37 zero-predicate request → DomainId "project-management"
- Mixed B36+B37 zero-predicate request → DomainId "project-management"

The DomainId "project-management" is shared between B36 and B37. The distinction lies only in the gap IDs, not the DomainId.

---

## B37-0C-008-002 — DomainId flip: zero → one predicate

**DomainId transition rule:**

Zero predicates → "project-management"
One or more predicates → "software-development"

This is a binary switch. There is no gradation.

**Examples:**
1. "Coordinate workstreams and track blockers." → [] → DomainId "project-management"
2. "Coordinate workstreams, track blockers, and review the action items." → ["QA"] (review) → DomainId "software-development"
3. "Track progress and notify stakeholders." → ["DOCUMENT"] (stakeholders) → DomainId "software-development"
4. "Track progress and send an update." → [] → DomainId "project-management"

The only difference between cases 3 and 4 is "stakeholders" vs "update". Adding "stakeholders" flips DomainId from "project-management" to "software-development".

---

## B37-0C-008-003 — DomainId "software-development" is not intent-based

**What "software-development" DomainId means in B37 context:**
It does NOT mean the request is about software development. It means a predicate fired.

**Managing requests that still get "software-development" DomainId:**
1. "Update stakeholders on project progress." → DOCUMENT (stakeholders) → DomainId "software-development"
2. "Review the escalation log." → QA (review) → DomainId "software-development"
3. "Define the accountability structure." → ARCHITECTURE (structure) + DOCUMENT (define) → DomainId "software-development"
4. "Build the status tracking module." → IMPLEMENT (build, module) → DomainId "software-development"

None of these are software development requests in intent, but all produce DomainId "software-development" because predicates fire.

---

## B37-0C-008-004 — DomainId preservation across multi-predicate requests

**Multi-predicate requests:**
When multiple predicates fire, DomainId remains "software-development" — it does not change based on the number of predicates or which predicates fire.

| Predicates | DomainId |
|-----------|---------|
| [DOCUMENT] | software-development |
| [ARCHITECTURE, DOCUMENT] | software-development |
| [TEST, DOCUMENT, QA] | software-development |
| [IMPLEMENT, ARCHITECTURE, DOCUMENT, QA] | software-development |

The DomainId for "software-development" is invariant once any predicate fires.

---

## B37-0C-008-005 — DomainId corruption detection

**Detecting v6-03 corruption (DomainId bleeding):**

Test: Does the output set DomainId "project-management" while also listing predicates?

If predicates = [] AND DomainId = "project-management" → correct
If predicates = [...] AND DomainId = "project-management" → v6-03 corruption

**Correction:**
Whenever predicates list is non-empty, DomainId MUST be "software-development".
No exception for:
- Managing vocabulary surrounding the predicate
- B37 context
- High proportion of gap vocabulary in the sentence
- The predicate firing as a false positive
