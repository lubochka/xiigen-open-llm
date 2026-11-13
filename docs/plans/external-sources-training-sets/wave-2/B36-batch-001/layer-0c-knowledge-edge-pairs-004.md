# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-004-001 — "update" in planning context → NOT IMPLEMENT

**B36 Rule 12:** "update" in project/planning context does NOT fire IMPLEMENT. IMPLEMENT needs code-level co-occurrence: function / class / logic / module / code.

**Planning context (no IMPLEMENT fire):**

| Request | "update" target | IMPLEMENT fires? | Reasoning |
|---------|----------------|-----------------|-----------|
| "Update the roadmap" | roadmap (planning artifact) | NO | No code-level co-occurrence |
| "Update the sprint backlog" | backlog (planning artifact) | NO | No code-level co-occurrence |
| "Update the milestone dates" | dates (planning artifact) | NO | No code-level co-occurrence |
| "Update the burndown chart" | chart (planning artifact) | NO | No code-level co-occurrence |

---

## B36-0C-004-002 — "update" fires IMPLEMENT only with code-level co-occurrence

**Code context (IMPLEMENT fires):**

| Request | "update" target | IMPLEMENT fires? | Reasoning |
|---------|----------------|-----------------|-----------|
| "Update the authentication function" | function (code entity) | YES | function is code-level |
| "Update the UserClass module" | module (code entity) | YES | module is code-level |
| "Update the database logic" | logic (code entity) | YES | logic is code-level |
| "Update the service code" | code (code entity) | YES | code is code-level |

---

## B36-0C-004-003 — "update the roadmap" → knowledge_edge trace

**Input:** "Update the roadmap for Q3 and add the new sprint milestones."

**Token analysis:**
- update: no code-level co-occurrence → no predicate
- roadmap: GAP (roadmap-001)
- Q3: no predicate
- add: no predicate
- sprint: GAP (sprint-001)
- milestones: GAP (milestone-001)

**KNOWLEDGE_EDGE state:**
- predicates: []
- FinalHumanResult: null
- DomainId: "project-management"
- CapabilityGapWarnings: roadmap-001, sprint-001, milestone-001
- recommended_action: SCHEDULE_CAPABILITY_LEARNING

---

## B36-0C-004-004 — "update" boundary test cases

**Test A:** "Update the sprint velocity targets." → predicates: [] → KNOWLEDGE_EDGE
**Test B:** "Update the velocity function in the tracker." → predicates: ["IMPLEMENT"] → Phase 4.1
**Test C:** "Update the backlog and implement the login feature." → predicates: ["IMPLEMENT"] → Phase 4.1 ("implement" fires, "update" alone does not, "feature" also fires)
**Test D:** "Update the release cycle documentation." → predicates: ["DOCUMENT"] via "documentation" substring? → check: "documentation" contains "document" → DOCUMENT fires. So predicates: ["DOCUMENT"]

**Key pattern:** "update" on its own never fires. IMPLEMENT fires only when a code-level entity is also present in the sentence (function, class, module, logic, code). The co-occurrence check is required.
