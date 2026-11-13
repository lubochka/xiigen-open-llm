# Layer 0-B — Router Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-024-001 — Complete B37 extended routing table (013-024 vocabulary)

**Extended B37 routing reference for new vocabulary (013-024):**

| Input pattern | Predicates | Routes | DomainId |
|--------------|-----------|--------|---------|
| risk log maintenance | [] | knowledge_edge | project-management |
| review risk log | [QA] | QA sub-phase | software-development |
| document risk log | [DOCUMENT] | 2.1 | software-development |
| change request coordination | [] | knowledge_edge | project-management |
| document change request + stakeholder | [DOCUMENT] | 2.1 | software-development |
| implement approved change request + service | [IMPLEMENT] | 4.1 | software-development |
| create project charter | [IMPLEMENT] | 4.1 | software-development |
| define project charter + stakeholder | [DOCUMENT] | 2.1 | software-development |
| decision log update | [] | knowledge_edge | project-management |
| document architectural decisions | [DOCUMENT, ARCHITECTURE] | 3.1+2.1 | software-development |
| lessons-learned session | [] | knowledge_edge | project-management |
| define lessons-learned structure | [DOCUMENT, ARCHITECTURE] | 3.1+2.1 | software-development |
| accountability coordination | [] | knowledge_edge | project-management |
| define accountability structure | [DOCUMENT, ARCHITECTURE] | 3.1+2.1 | software-development |
| team dependencies | [] | knowledge_edge | project-management |
| API dependencies + service | [API_CONTRACT, IMPLEMENT] | 3.3+4.1 | software-development |
| managing vocab + stakeholder | [DOCUMENT] | 2.1 | software-development |
| managing vocab + structure | [ARCHITECTURE] | 3.1 | software-development |

---

## B37-0B-024-002 — SCL emission rule for extended vocabulary

**SCHEDULE_CAPABILITY_LEARNING (SCL) emission confirmed for extended B37 vocabulary:**

Emitted (zero predicates):
- risk log maintenance
- change request coordination/tracking
- project charter circulation
- decision log tracking/updating
- lessons-learned sessions
- accountability coordination
- team/cross-workstream dependency mapping

NOT emitted (any predicate fires):
- Any sentence with: stakeholder, structure, design, review, define, document, build, create, implement, service, module, API co-occurrence + dependencies

---

## B37-0B-024-003 — Gap warning forwarding rule for extended vocabulary

**Gap warnings are always forwarded, never suppressed:**

When predicates fire and managing vocabulary is present:
- gap-project-managing-raid-log-001 → forwarded when "risk log" present
- gap-project-managing-manage-delivery-001 → forwarded when "change request"/"decision log"/"project charter"/"lessons learned" present
- gap-project-managing-accountability-001 → forwarded when "accountability"/"RACI"/"ownership" present
- gap-project-managing-dependencies-team-001 → forwarded when "team/workstream dependencies" present
- gap-project-managing-stakeholder-update-001 → forwarded when "stakeholder" present (alongside DOCUMENT)

Gap warnings are NEVER suppressed by the presence of predicates.

---

## B37-0B-024-004 — Extended B37 routing certification questions

Q1: "Review the risk log and escalate high items." → Route?
A: QA sub-phase (QA fires on "review"). DomainId "software-development".

Q2: "Track change requests and escalate overdue items." → Route?
A: knowledge_edge (zero predicates). DomainId "project-management". SCL emitted.

Q3: "Define the accountability structure for governance." → Route?
A: 3.1 + 2.1 (ARCHITECTURE + DOCUMENT). DomainId "software-development".

Q4: "Map API dependencies between the auth service and the endpoint." → Route?
A: 3.3 + 4.1 (API_CONTRACT + IMPLEMENT). DomainId "software-development".

Q5: "Document lessons learned and circulate to stakeholders." → Route?
A: 2.1 (DOCUMENT). DomainId "software-development".

---

## B37-0B-024-005 — Extended B37 router summary

**Summary: B37 extended vocabulary (013-024) router behaviour**

1. All new vocabulary (risk log, change request, project charter, decision log, lessons-learned, accountability) → zero predicates when used alone
2. False positives still fire: structure → ARCHITECTURE, stakeholder → DOCUMENT, review → QA, define → DOCUMENT
3. API co-occurrence rule unchanged: "team dependencies" → gap; "API/integration dependencies" → API_CONTRACT
4. DomainId rule unchanged: zero predicates → "project-management"; any predicate → "software-development"
5. SCL emission unchanged: only for zero-predicate requests
6. Gap warnings always forwarded alongside any predicate routing
7. No new factories added — B37 remains fundamentally a knowledge_edge scenario
