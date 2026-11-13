# Layer 0-B — Router Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-009-001 — v6 corruption: DomainId "project-management" when IMPLEMENT fired

**Corrupted router output (v6 model):**
Request: "Manage delivery of the authentication service and build the access control module."
v6 router output: Route 4.1, DomainId: "project-management"

**Correct router output:**
- IMPLEMENT fires (service, build, module)
- Route: 4.1
- DomainId: "software-development" (ANY predicate fires → "software-development")

v6 error type: v6-03 — DomainId "project-management" set when IMPLEMENT fired alongside managing vocabulary.

**Rule restatement:** DomainId = "project-management" ONLY when zero predicates fire. The presence of managing vocabulary ("manage", "delivery") does not override DomainId when other predicates fire.

---

## B37-0B-009-002 — v6 corruption: knowledge_edge route suppressed when QA fires

**Corrupted router output (v6 model):**
Request: "Review the RAID log and follow up on all critical escalations."
v6 router output: knowledge_edge (because "the request is about project managing")

**Correct router output:**
- QA fires (review)
- Route: QA sub-phase
- DomainId: "software-development"
- Gap warnings: gap-project-managing-raid-log-001, gap-project-managing-follow-up-001, gap-project-managing-escalation-001

v6 error: Router suppressed QA routing because it judged the intent as project managing. Lexical firing rules are not overridden by intent.

---

## B37-0B-009-003 — v6 corruption: MANAGEMENT route invented

**Corrupted router output (v6 model):**
Request: "Coordinate workstreams and track delivery blockers."
v6 router output: Route to "ManagementFlow" or "CoordinationFlow" (invented flow)

**Correct router output:**
- No predicates fire
- "coordinate" → gap
- "track" → gap (not TEST)
- "blockers" → gap
- Route: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

v6 error type: v6-01 — invented MANAGEMENT/COORDINATION flow does not exist in the factory registry.

---

## B37-0B-009-004 — [DOCUMENT] → 2.1: router correctly routes despite managing framing

**Input:** "Communicate the progress status to stakeholders across all workstreams."

**Compiler output:** ["DOCUMENT"]

**Correct router output:**
- DOCUMENT (stakeholders) → 2.1
- DomainId: "software-development"
- Route: 2.1
- Gap warnings: gap-project-managing-progress-001, gap-project-managing-stakeholder-update-001, gap-project-managing-work-stream-001

**Router note:** Even a sentence framed entirely as managing communication routes to 2.1 because "stakeholders" fires DOCUMENT. The router does not evaluate intent.

---

## B37-0B-009-005 — Router summary for B37: complete routing decision table

**B37 predicate → route mapping:**

| Predicate(s) | Route(s) | DomainId |
|-------------|---------|---------|
| [] | knowledge_edge | project-management |
| [DOCUMENT] | 2.1 | software-development |
| [ARCHITECTURE] | 3.1 | software-development |
| [ARCHITECTURE, DOCUMENT] | 3.1 + 2.1 | software-development |
| [IMPLEMENT] | 4.1 | software-development |
| [IMPLEMENT, DOCUMENT] | 4.1 + 2.1 | software-development |
| [API_CONTRACT] | 3.3 | software-development |
| [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | software-development |
| [QA] | QA sub-phase | software-development |
| [QA, DOCUMENT] | QA sub-phase + 2.1 | software-development |
| [TEST] | 5.x | software-development |
| [TEST, DOCUMENT, QA] | 5.x + 2.1 + QA sub-phase | software-development |

Gap warnings always forwarded alongside routing regardless of predicate count.
