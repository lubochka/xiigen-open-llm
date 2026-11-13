# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-012-001 — B37 knowledge edge summary: all rules in one reference

**Complete B37 rule reference for knowledge-edge certification:**

| Rule # | Rule |
|--------|------|
| Rule 1 | GAP vocabulary: track, coordinate, blockers, dependencies (no API), stakeholder-update, escalation, accountability, follow-up, action-items, on-track/off-track, RAG-status, RAID-log, progress, status-report, manage, delivery-status |
| Rule 2 | "track" ≠ TEST. TEST fires on: test, verify, assert, coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e. "track" has NO entry. |
| Rule 3 | "report" ≠ DOCUMENT. DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case. |
| Rule 4 | "status" → NOT in any predicate lexicon (zero predicates) |
| Rule 5 | "dependencies" → API_CONTRACT ONLY with API co-occurrence; team/project dependencies → gap |
| Rule 6 | "stakeholder" → DOCUMENT fires ALWAYS (even in status report sentence) |
| Rule 7 | "structure" → ARCHITECTURE fires ALWAYS (even in governance/org context) |
| Rule 8 | define/document/requirements/criteria/specification → DOCUMENT |
| Rule 9 | build/implement/create/write/service/module → IMPLEMENT |
| Rule 10 | "manage" → NOT in any predicate lexicon |
| Rule 11 | "coordinate" → NOT in any predicate lexicon |
| Rule 12 | "progress" → NOT in any predicate lexicon |
| Rule 13 | "escalate/escalation" → NOT in any predicate lexicon |
| Rule 14 | DomainId "project-management" ONLY when zero predicates; "software-development" when ANY predicate fires |
| Rule 15 | B36 gap = "project-planning"; B37 gap = "project-managing" |
| Rule 16 | Gap ID format: gap-project-managing-{term(s)}-{sequence} |
| Rule 17 | SCHEDULE_CAPABILITY_LEARNING emitted for zero-predicate gap requests |
| Rule 18 | DomainId "project-management" for zero-predicate in both B36 and B37 |
| Rule 19 | "review" → QA fires (even in project managing context) |
| Rule 20 | "architecture"/"design" → ARCHITECTURE fires even in governance sentences |
| Rule 21 | "test" → TEST fires even if context is "test the estimates" |

---

## B37-0C-012-002 — v6 corruption quick reference

| Code | Pattern | Symptom | Correction |
|------|---------|---------|-----------|
| v6-01 | MANAGEMENT/COORDINATION invented | Predicate list has non-registry token | Zero predicates; knowledge_edge |
| v6-02 | DOCUMENT suppressed for stakeholder | Sentence has "stakeholder" but DOCUMENT absent | DOCUMENT fires always on stakeholder |
| v6-03 | DomainId "project-management" when predicate fires | Predicate non-empty + DomainId "project-management" | DomainId must be "software-development" |

---

## B37-0C-012-003 — The three critical boundaries (exam-level)

**Boundary 1: track vs test**
"track" → NOTHING. "test" → TEST. These are the most easily confused in B37.

**Boundary 2: dependencies with vs without API co-occurrence**
"project dependencies" → gap. "API dependencies" → API_CONTRACT.

**Boundary 3: status/report vs stakeholder**
"status report" → NOTHING. "status report to stakeholders" → DOCUMENT (on "stakeholders").

---

## B37-0C-012-004 — Five canonical B37 knowledge-edge test cases

1. "Track and coordinate delivery blockers." → [], project-management, knowledge_edge
2. "Review the escalation log." → [QA], software-development, QA sub-phase
3. "Update stakeholders on delivery status." → [DOCUMENT], software-development, 2.1
4. "Define the accountability structure." → [ARCHITECTURE, DOCUMENT], software-development, 3.1+2.1
5. "Build the tracking service and manage the rollout." → [IMPLEMENT], software-development, 4.1

---

## B37-0C-012-005 — Layer 2 type note: capability-gap-extended-pairs (not nine-node)

**B37 is a CAPABILITY GAP scenario.**

Layer 2 for B37 = capability-gap-extended-pairs (NOT nine-node pairs).

This means:
- No full nine-node flow is generated for B37 managing requests
- The extended-pairs format documents the gap, traces the knowledge_edge path, and records CapabilityGapWarnings
- No flow factory is invoked for pure gap requests
- SCHEDULE_CAPABILITY_LEARNING is the output for zero-predicate requests

**Contrast with B36:** B36 planning scenarios are also capability gaps but have a different gap domain ("project-planning"). Both use capability-gap-extended-pairs format.
