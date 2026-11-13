# Layer 0-B — Router Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: router-pairs

---

## B37-0B-012-001 — Complete B37 routing decision table

**Full B37 routing reference:**

| Predicates | Routes | DomainId | Notes |
|-----------|--------|---------|-------|
| [] | knowledge_edge | project-management | SCHEDULE_CAPABILITY_LEARNING emitted |
| [TEST] | 5.x | software-development | "test" fires even in planning context |
| [QA] | QA sub-phase | software-development | "review"/"validate" fire even in managing context |
| [DOCUMENT] | 2.1 | software-development | "stakeholder" false positive common |
| [ARCHITECTURE] | 3.1 | software-development | "structure" false positive common |
| [IMPLEMENT] | 4.1 | software-development | "build"/"service"/"module" fire even in managing sentences |
| [API_CONTRACT] | 3.3 | software-development | "dependencies" only with API co-occurrence |
| [ARCHITECTURE, DOCUMENT] | 3.1 + 2.1 | software-development | Common in governance sentences |
| [API_CONTRACT, DOCUMENT] | 3.3 + 2.1 | software-development | API dep + document pattern |
| [TEST, DOCUMENT, QA] | 5.x + 2.1 + QA | software-development | "test estimates with stakeholders" |
| [IMPLEMENT, DOCUMENT] | 4.1 + 2.1 | software-development | Build + document pattern |
| [QA, DOCUMENT] | QA + 2.1 | software-development | Review + criteria pattern |

---

## B37-0B-012-002 — Gap warning forwarding rules

**Rule: CapabilityGapWarnings always forwarded alongside routing**

When predicates fire and managing vocabulary is present:
1. Route predicates normally
2. Collect all gap vocabulary tokens
3. Map each to its gap ID
4. Forward gap warnings to gap handler alongside routing
5. SCHEDULE_CAPABILITY_LEARNING is NOT emitted when predicates fire (only for zero-predicate requests)

**Gap warning forwarding is NOT suppressed by:**
- Presence of predicates
- Number of predicates
- DomainId value
- Request framing (managing, technical, mixed)

---

## B37-0B-012-003 — SCHEDULE_CAPABILITY_LEARNING emission rules

**Emitted WHEN:**
- Zero predicates fire
- DomainId = "project-management"
- Request routed to knowledge_edge
- B37 gap vocabulary present in request

**NOT emitted WHEN:**
- Any predicate fires (even one)
- DomainId = "software-development"
- Request routed to any sub-phase (2.1, 3.1, 3.3, 4.1, 5.x, QA sub-phase)

**One SCHEDULE_CAPABILITY_LEARNING per request:**
Even if multiple gap IDs are present, only one learning event is emitted per request.

---

## B37-0B-012-004 — B37 router boundary: "status" vocabulary never routes

**Tokens that do NOT produce routing by themselves:**
- status
- report
- progress
- track
- coordinate
- manage
- escalation
- escalate
- blockers
- follow-up
- action items
- on-track / off-track
- RAG-status
- RAID log
- accountability
- workstream
- delivery (in managing context)

All of the above produce zero predicates and route to knowledge_edge when alone.

Only the compound tokens that coincidentally overlap with predicate lexicons (stakeholder→DOCUMENT, structure→ARCHITECTURE, review→QA, test→TEST, build→IMPLEMENT) cause routing from B37 sentences.

---

## B37-0B-012-005 — B37 router summary: key facts for certification

1. B37 vocabulary is all gap — no predicate registered for any managing token
2. Routing only happens when incidental predicate-eligible tokens appear in managing sentences
3. DomainId = "project-management" ↔ exactly zero predicates
4. DomainId = "software-development" ↔ one or more predicates (regardless of intent)
5. Gap warnings are always forwarded, never suppressed
6. SCHEDULE_CAPABILITY_LEARNING: zero-predicate requests only, once per request
7. v6 corruptions: v6-01 (invent predicate), v6-02 (suppress DOCUMENT), v6-03 (wrong DomainId)
8. knowledge_edge is the default B37 route for pure managing requests
