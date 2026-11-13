# Layer 0-A — Compiler Pairs (060)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-060-001 — Complete B37 all-round zero-predicate vocabulary master list

**All B37 vocabulary that produces zero predicates when used alone:**

Base (002-012): track, coordinate, blockers, on-track/off-track, RAG-status, RAID log, progress, status report, escalation, accountability, follow-up, action items, manage, delivery, workstream, dependencies (no API)
Extended (013-024): risk log, change request, project charter, decision log, lessons learned, RACI, sign-off, programme board, steering committee, ownership
Round 2 (025-036): milestone tracking, Gantt chart, resource allocation, capacity planning, dependency mapping (no API), critical path, budget tracking, vendor management, SLA tracking
Round 3a (037-048): escalation process, communication plan, project kickoff, change control board, scope creep, risk mitigation, quality gate, sign-off process, handover (alone), post-mortem
Round 3b (049-060): waterfall, agile transformation, SAFe, program increment, portfolio management, value stream, flow state, project closure, benefits realization, KPI monitoring/tracking, WBS (abbreviation)

**Exception (always fires ARCHITECTURE):** "work breakdown structure" (on "structure" token)

---

## B37-0A-060-002 — Complete B37 predicate-trigger master reference

**DOCUMENT fires:**
document, write, spec, describe, define, requirements, criteria, functional, behavior, use-case, story, stakeholder

**ARCHITECTURE fires:**
structure, design, architecture, architectural

**QA fires:**
review, validate

**IMPLEMENT fires:**
build, create, implement, write (code context), service, module

**API_CONTRACT fires:**
dependencies + (api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook) co-occurrence

**TEST fires:**
test, verify, assert, coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e

---

## B37-0A-060-003 — "track" boundary rule: master confirmation

**"track" does NOT fire TEST** — confirmed across all 60 files (002-060).

TEST fires: test, verify, assert, coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e.
"track" has NO entry in TEST lexicon. "track progress", "track milestones", "track SLA", "track SAFe", "track KPI" → all zero predicates.

---

## B37-0A-060-004 — "report" boundary rule: master confirmation

**"report" does NOT fire DOCUMENT** — confirmed across all 60 files.

DOCUMENT fires: document, write, spec, describe, define, requirements, criteria, functional, behavior, use-case, story, stakeholder.
"report" has NO entry in DOCUMENT lexicon. "status report", "progress report", "weekly report", "SAFe report", "KPI report" → all zero predicates from "report" token.

---

## B37-0A-060-005 — B37 compiler pairs complete: certification summary

**B37-batch-001 Layer 0-A training complete (002-060):**

Files: 59 compiler-pair files (layer-0a-compiler-pairs-002.md through -060.md)
Core teaching: Project managing vocabulary has zero registered predicates when used alone
Boundary rules: track ≠ TEST; report ≠ DOCUMENT; status → zero; "structure" → ARCHITECTURE
False positives: stakeholder → DOCUMENT; structure → ARCHITECTURE; review → QA; build/create/service/module → IMPLEMENT; API + dependencies → API_CONTRACT
DomainId: zero predicates → "project-management"; any predicate → "software-development"
Certification metric: 20/20 exam (059) + zero v6 corruptions
