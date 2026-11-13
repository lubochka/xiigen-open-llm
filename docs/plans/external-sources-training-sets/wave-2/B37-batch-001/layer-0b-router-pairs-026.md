# Layer 0-B — Router Pairs (026)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-026-001 — Router certification Q&A: Round 2 vocabulary part 1

Q1: "Send the status report to the team." → Route?
A: knowledge_edge. DomainId "project-management". SCL emitted.

Q2: "Document the status report for stakeholders." → Route?
A: 2.1 (DOCUMENT). DomainId "software-development".

Q3: "Review the Gantt chart." → Route?
A: QA sub-phase (QA fires on "review"). DomainId "software-development".

Q4: "Maintain the Gantt chart." → Route?
A: knowledge_edge. DomainId "project-management". SCL emitted.

Q5: "Create the Gantt chart." → Route?
A: 4.1 (IMPLEMENT fires on "create"). DomainId "software-development".

---

## B37-0B-026-002 — Router certification Q&A: Round 2 vocabulary part 2

Q1: "Track resource allocation." → Route?
A: knowledge_edge. "track" ≠ TEST. DomainId "project-management".

Q2: "Review resource allocation and define the team structure." → Route?
A: QA sub-phase + 3.1 + 2.1 (QA + ARCHITECTURE + DOCUMENT). DomainId "software-development".

Q3: "Run capacity planning." → Route?
A: knowledge_edge. DomainId "project-management".

Q4: "Design the capacity planning structure." → Route?
A: 3.1 (ARCHITECTURE fires on "design" + "structure"). DomainId "software-development".

Q5: "Map team dependencies." → Route?
A: knowledge_edge. No API co-occurrence. DomainId "project-management".

---

## B37-0B-026-003 — Router certification Q&A: Round 2 vocabulary part 3

Q1: "Map REST API dependencies between services." → Route?
A: 3.3 + 4.1 (API_CONTRACT + IMPLEMENT). DomainId "software-development".

Q2: "Identify the critical path." → Route?
A: knowledge_edge. DomainId "project-management".

Q3: "Define the critical path criteria." → Route?
A: 2.1 (DOCUMENT fires on "define" + "criteria"). DomainId "software-development".

Q4: "Track the budget." → Route?
A: knowledge_edge. "track" ≠ TEST. DomainId "project-management".

Q5: "Build the budget tracking service." → Route?
A: 4.1 (IMPLEMENT fires on "build" + "service"). DomainId "software-development".

---

## B37-0B-026-004 — Round 2 gap warning forwarding rules

**Gap warnings forwarded alongside routing for Round 2 vocabulary:**

When predicates fire and Round 2 managing vocabulary is present:
- gap-project-managing-status-report-001 → forwarded when "status report" present
- gap-project-managing-progress-001 → forwarded when "progress update" present
- gap-project-managing-track-001 → forwarded when "milestone tracking", "SLA tracking", "budget tracking" present
- gap-project-managing-manage-delivery-001 → forwarded when "Gantt chart", "critical path", "vendor management", "capacity planning", "SLA" present
- gap-project-managing-dependencies-team-001 → forwarded when "dependency mapping" without API co-occurrence present

Gap warnings are NEVER suppressed when predicates fire.

---

## B37-0B-026-005 — Round 2 router summary

**B37 Round 2 router behaviour (025-036):**

1. All new PM vocabulary (status report, progress update, milestone tracking, Gantt chart, resource allocation, capacity planning, dependency mapping, critical path, budget tracking, vendor management, SLA tracking) → zero predicates when used alone
2. False positives still fire: review → QA; define/document/criteria/stakeholders → DOCUMENT; structure/design/architecture → ARCHITECTURE; create/build/implement/write/service/module → IMPLEMENT; API + dependencies → API_CONTRACT
3. "report" rule unchanged: "report" alone or in "status report" → zero predicates (Rule 3)
4. "track" rule unchanged: "track" ≠ TEST in any context (Rule 2)
5. DomainId rule unchanged: zero predicates → "project-management"; any predicate → "software-development"
6. SCL emission unchanged: only for zero-predicate requests
7. No new flow factories — B37 remains fundamentally a knowledge_edge scenario
