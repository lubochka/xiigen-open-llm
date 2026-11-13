# Layer 0-A — Compiler Pairs (027)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-027-001 — "SLA tracking" gap vs IMPLEMENT when building SLA system

**Input A:** "Track SLA compliance for the vendor contract."

Token scan:
- "Track" → NOT TEST (Rule 2) → gap
- "SLA compliance" → zero predicates → gap
- "vendor contract" → "contract" alone without API co-occurrence → gap

Predicates: []
DomainId: "project-management"
GapId: gap-project-managing-track-001, gap-project-managing-manage-delivery-001

**Input B:** "Implement an SLA tracking service for the contract management module."

Token scan:
- "Implement" → IMPLEMENT fires (Rule 9)
- "SLA tracking" → gap vocabulary present
- "service" → IMPLEMENT fires (Rule 9)
- "contract management module" → "contract" without API co-occurrence → gap; "module" → IMPLEMENT fires

Predicates: ["IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: gap-project-managing-track-001, gap-project-managing-manage-delivery-001

---

## B37-0A-027-002 — PM boundary reference: combined gap vocabulary confirmation

**Confirm zero-predicate status for Round 2 PM vocabulary:**

| Token | Predicate fires? | Correct result |
|-------|----------------|----------------|
| status report | No | [] |
| progress update | No | [] |
| milestone tracking | No | [] |
| Gantt chart | No | [] |
| resource allocation | No | [] |
| capacity planning | No | [] |
| dependency mapping (no API) | No | [] |
| critical path | No | [] |
| budget tracking | No | [] |
| vendor management | No | [] |
| SLA tracking | No | [] |

All eleven tokens: zero predicates. DomainId "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B37-0A-027-003 — False positive summary for Round 2 vocabulary

**False positives that apply to new Round 2 vocabulary:**

1. "document the status report" → DOCUMENT fires on "document"
2. "define milestone criteria" → DOCUMENT fires on "define" + "criteria"
3. "review resource allocation" → QA fires on "review"
4. "design capacity planning structure" → ARCHITECTURE fires on "design" + "structure"
5. "create Gantt chart document" → IMPLEMENT fires on "create" + DOCUMENT fires on "document"
6. "implement SLA tracking service" → IMPLEMENT fires on "implement" + "service"
7. "stakeholder resource allocation update" → DOCUMENT fires on "stakeholder"

All follow existing false positive rules. No new predicate lexicon entries for Round 2 vocabulary.

---

## B37-0A-027-004 — Mixed sentence: Round 2 vocabulary + multiple predicates

**Input:** "Track resource allocation, review budget utilisation, and document capacity plans for stakeholders."

Token scan:
- "Track" → NOT TEST → gap
- "resource allocation" → gap
- "review" → QA fires
- "budget utilisation" → gap
- "document" → DOCUMENT fires
- "capacity plans" → gap
- "stakeholders" → DOCUMENT fires

Multiple DOCUMENT triggers → ONE DOCUMENT
QA fires independently

Predicates: ["QA", "DOCUMENT"]
DomainId: "software-development"
GapIds forwarded: track-001, manage-delivery-001
SCL: NOT emitted

---

## B37-0A-027-005 — DomainId rule for Round 2 PM vocabulary: confirmation

**DomainId decision tree for Round 2 vocabulary:**

Zero-predicate requests (DomainId = "project-management"):
- Pure status report management
- Pure progress update reporting
- Pure milestone tracking
- Pure Gantt chart maintenance
- Pure resource allocation coordination
- Pure capacity planning sessions
- Pure dependency mapping (no API co-occurrence)
- Pure critical path analysis
- Pure budget tracking
- Pure vendor management
- Pure SLA tracking

One-predicate requests (DomainId = "software-development"):
- Any of the above + "review" → ["QA"]
- Any of the above + "stakeholder" → ["DOCUMENT"]
- Any of the above + "define/document/criteria" → ["DOCUMENT"]
- Any of the above + "structure/design" → ["ARCHITECTURE"]
- Any of the above + "build/implement/service/module" → ["IMPLEMENT"]
- Any of the above + API co-occurrence + "dependencies" → ["API_CONTRACT"]

Rule 14 is absolute: ANY predicate → "software-development". No exceptions.
