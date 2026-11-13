# Layer 0-A — Compiler Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-012-001 — B37 compiler pattern summary: complete predicate/gap table

**Complete B37 predicate firing table:**

| Token / Phrase | Predicate | Notes |
|---------------|-----------|-------|
| test | TEST | ALWAYS — even "test the estimates" |
| verify | TEST | |
| assert | TEST | |
| validate | QA | validate → QA (not TEST) |
| coverage | TEST | |
| acceptance | TEST | "acceptance criteria" still fires TEST on "acceptance" |
| unit, regression, e2e | TEST | |
| review | QA | ALWAYS — even in project managing review context |
| document | DOCUMENT | |
| define | DOCUMENT | |
| describe | DOCUMENT | |
| requirement(s) | DOCUMENT | |
| story | DOCUMENT | |
| criteria | DOCUMENT | |
| stakeholder | DOCUMENT | ALWAYS — false positive in managing context |
| functional | DOCUMENT | |
| behavior | DOCUMENT | |
| use-case | DOCUMENT | |
| spec | DOCUMENT | |
| structure | ARCHITECTURE | ALWAYS — even "accountability structure", "org structure" |
| architecture | ARCHITECTURE | |
| design | ARCHITECTURE | |
| build | IMPLEMENT | |
| implement | IMPLEMENT | |
| create | IMPLEMENT | |
| write (code) | IMPLEMENT | |
| service | IMPLEMENT | |
| module | IMPLEMENT | |
| dependencies + API co-occurrence | API_CONTRACT | API co-occurrence: api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook |

---

## B37-0A-012-002 — Complete B37 gap vocabulary table (zero predicates)

| Token / Phrase | Gap ID | Notes |
|---------------|--------|-------|
| track | gap-project-managing-track-001 | NOT TEST |
| track progress | gap-project-managing-track-progress-001 | compound |
| track status | gap-project-managing-track-status-001 | compound |
| coordinate | gap-project-managing-coordinate-001 | NOT in any lexicon |
| blockers | gap-project-managing-blockers-001 | |
| coordinate + blockers | gap-project-managing-coordinate-blockers-001 | compound |
| dependencies (no API) | gap-project-managing-dependencies-team-001 | Rule 5 |
| stakeholder update | gap-project-managing-stakeholder-update-001 | intent gap (DOCUMENT fires on token) |
| escalation | gap-project-managing-escalation-001 | Rule 13 |
| escalation + accountability | gap-project-managing-escalation-accountability-001 | compound |
| accountability | gap-project-managing-accountability-001 | |
| follow-up | gap-project-managing-follow-up-001 | |
| action items | gap-project-managing-action-items-001 | |
| on-track / off-track | gap-project-managing-track-status-001 | |
| RAG-status | gap-project-managing-rag-status-001 | |
| RAID log | gap-project-managing-raid-log-001 | |
| progress | gap-project-managing-progress-001 | Rule 12 |
| status report | gap-project-managing-status-report-001 | Rule 3 |
| manage delivery | gap-project-managing-manage-delivery-001 | Rule 10 |
| workstream / work stream | gap-project-managing-work-stream-001 | |

---

## B37-0A-012-003 — DomainId decision rules for B37

**DomainId = "project-management":**
- Zero predicates fired
- All vocabulary is B37 gap vocabulary
- SCHEDULE_CAPABILITY_LEARNING emitted

**DomainId = "software-development":**
- ANY predicate fired (TEST, QA, DOCUMENT, ARCHITECTURE, IMPLEMENT, API_CONTRACT)
- Even if the primary intent is project managing
- Even if most vocabulary is gap
- Managing vocabulary present → CapabilityGapWarnings only

**v6-03 error:** Setting DomainId = "project-management" when any predicate has fired. This violates Rule 14.

---

## B37-0A-012-004 — Critical B37 boundaries (exam-level summary)

**Boundary 1: track ≠ TEST**
"track progress" → zero predicates. TEST fires on: test, verify, assert, validate (QA actually), coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e. "track" is NOT on this list.

**Boundary 2: report ≠ DOCUMENT**
"status report" → zero predicates from "report". DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case.

**Boundary 3: dependencies context-sensitivity**
"team dependencies", "project dependencies" → gap (no API co-occurrence)
"API dependencies", "service API dependencies", "endpoint dependencies" → API_CONTRACT fires

**Boundary 4: manage/coordinate/progress/escalate → zero predicates**
None of these four tokens appear in any predicate lexicon.

**Boundary 5: stakeholder → DOCUMENT always**
Even in "send status report to stakeholders" — DOCUMENT fires. This is a false positive but lexicon-correct.

**Boundary 6: structure → ARCHITECTURE always**
Even in "accountability structure", "governance structure", "org structure" — ARCHITECTURE fires.

---

## B37-0A-012-005 — B36 vs B37 gap domain distinction

**B36 gap domain = "project-planning"**
Gap vocabulary: sprint, roadmap, milestone, planning, estimation, backlog, capacity, velocity
Gap ID format: gap-project-planning-{term}-{sequence}

**B37 gap domain = "project-managing"**
Gap vocabulary: track, coordinate, blockers, dependencies (no API), stakeholder-update, escalation, accountability, follow-up, action-items, on-track/off-track, RAG-status, RAID-log, progress, status-report, manage, delivery-status
Gap ID format: gap-project-managing-{term(s)}-{sequence}

**DomainId for zero predicates in BOTH B36 and B37:**
DomainId = "project-management" (same domain for both — Rule 18)

**Critical difference:** The gap_id sub-domain differs: "project-planning" (B36) vs "project-managing" (B37). Do not use "project-planning" for B37 gaps. Do not use "project-managing" for B36 gaps.
