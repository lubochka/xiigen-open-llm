# Layer 0-B — Router Pairs (035)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-035-001 — "progress update" in context of all B37 vocabulary families

**Full B37 vocabulary combinations with "progress update":**

A: "Track progress, coordinate workstreams, update RAID log, and send progress updates."
→ [] → knowledge_edge | project-management | SCL emitted (all B37 gap vocabulary)

B: "Track progress, update RAID log, and send progress updates to stakeholders."
→ ["DOCUMENT"] → 2.1 | software-development (stakeholders fires; all other tokens = gap)

C: "Define progress update criteria and document RAID log structure."
→ ["DOCUMENT", "ARCHITECTURE"] → 2.1 + 3.1 (define + document → DOCUMENT; structure → ARCHITECTURE)

---

## B37-0B-035-002 — "SLA tracking" boundary: "SLA" vs "service level agreement"

**Both phrasings confirm zero-predicate:**

A: "Track SLA compliance." → [] → knowledge_edge
B: "Track service level agreement compliance." → [] → knowledge_edge
C: "Monitor SLA breaches." → [] → knowledge_edge
D: "Implement SLA monitoring service." → ["IMPLEMENT"] → 4.1 (implement + service)
E: "Define service level agreement criteria." → ["DOCUMENT"] → 2.1 (define + criteria)

SLA (Service Level Agreement) and its expanded form are both zero-predicate PM vocabulary.

---

## B37-0B-035-003 — "budget tracking" with multiple false positive triggers

**Cascading false positives:**

Input: "Define budget tracking criteria, design the reporting structure, and document the findings for stakeholders."

Token scan:
- "Define" → DOCUMENT fires
- "budget tracking criteria" → "criteria" → DOCUMENT fires
- "design" → ARCHITECTURE fires
- "reporting structure" → "structure" → ARCHITECTURE fires
- "document" → DOCUMENT fires
- "findings" → gap
- "stakeholders" → DOCUMENT fires

Predicates: ["DOCUMENT", "ARCHITECTURE"]
Routes: 2.1 + 3.1 (parallel eligible)
DomainId: "software-development"
GapIds forwarded: track-001, manage-delivery-001, status-report-001

---

## B37-0B-035-004 — "vendor management" gap_interruption routing

**Scenario:** User is in active ArchitectureFlow31 (3.1), then issues:
"Coordinate vendor onboarding and track SLA compliance."

Router analysis:
- All tokens = gap vocabulary
- TransitionType: gap_interruption
- ContextCarryForward: true (ArchitectureFlow31 context preserved)
- Route: knowledge_edge
- DomainId: "project-management"
- SCL: emitted

After gap handling: ArchitectureFlow31 resumable.

---

## B37-0B-035-005 — Round 2 routing certification: final 10 questions

Q1: "Track budget and report to the programme board." → Route? A: knowledge_edge | project-management
Q2: "Track budget and report to stakeholders." → Route? A: 2.1 (DOCUMENT) | software-development
Q3: "Define capacity planning structure." → Route? A: 3.1 (ARCHITECTURE) | software-development
Q4: "Manage vendor contracts." → Route? A: knowledge_edge (contract alone ≠ API_CONTRACT) | project-management
Q5: "Manage vendor REST API contracts." → Route? A: 3.3 (API_CONTRACT) | software-development
Q6: "Map team dependencies across workstreams." → Route? A: knowledge_edge | project-management
Q7: "Review SLA compliance." → Route? A: QA sub-phase | software-development
Q8: "Run capacity planning." → Route? A: knowledge_edge | project-management
Q9: "Create a milestone tracking dashboard." → Route? A: 4.1 (IMPLEMENT on "create") | software-development
Q10: "Maintain the Gantt chart." → Route? A: knowledge_edge | project-management
