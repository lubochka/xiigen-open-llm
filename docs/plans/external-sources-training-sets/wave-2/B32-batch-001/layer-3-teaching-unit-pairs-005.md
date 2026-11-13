# Layer 3 — Teaching Unit Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-005-001 — B32 vs B36/B37 DomainId distinction: software-development vs project-management

**Teaching unit:** B32 (vibe coding) and B36/B37 (project management) are different scenarios with different DomainIds. This distinction is critical because it determines which learning pipeline receives SCHEDULE_CAPABILITY_LEARNING events.

**The distinction:**

| Scenario | Domain | DomainId |
|---|---|---|
| B32 — Vibe coding | Software development done informally | software-development |
| B36 — Project planning | Planning and managing software projects | project-management |
| B37 — Sprint/agile | Agile methodology and sprint management | project-management |

---

## B32-L3-005-002 — How to distinguish B32 from B36/B37

**B32 indicators (vibe coding):**
- Presence of software development vocabulary (service, module, API, screen, component)
- Presence of informal coding style vocabulary (vibe, spike, hack, prototype)
- The activity being requested is CODE CREATION, even if informal

**B36/B37 indicators (project management):**
- Presence of project management vocabulary (milestone, roadmap, sprint, backlog)
- Presence of team management vocabulary (assign, estimate, velocity)
- The activity being requested is PLANNING or MANAGING, not code creation

**Test:** Is the user trying to create software artifacts (B32), or manage a software project (B36/B37)?

---

## B32-L3-005-003 — Edge case: vibe coding + project planning mixed request

**Request:** "Vibe plan the sprint backlog and spike the API design."

**Analysis:**
- "vibe" → B32 gap vocabulary
- "plan" → B36-type vocabulary (if "plan" is in B36 lexicon)
- "sprint backlog" → B36/B37 vocabulary
- "spike" → B32 gap vocabulary
- "API" → API_CONTRACT predicate fires

**DomainId determination:**
When B32 and B36 vocabulary are mixed, the system determines DomainId based on the registered predicates that fire. If API_CONTRACT fires (software-development), DomainId = "software-development". The project planning vocabulary would be gap for a B32 session.

**Result (B32 perspective):** [API_CONTRACT] with gap warnings for "vibe", "spike", and B36 vocabulary; DomainId: "software-development".

---

## B32-L3-005-004 — DomainId consistency requirement across all B32 outputs

**Every B32 output must have DomainId "software-development".**

This applies to:
- Zero-predicate knowledge_edge results
- Single-predicate routing results
- Multi-predicate routing results
- All gap configurations
- All vibe vocabulary combinations

There are no B32 cases that use DomainId "project-management".

---
