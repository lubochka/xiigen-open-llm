# Held-Out and Corruption Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-015-001 — HO: project charter circulation → knowledge_edge

**Request:** "Finalise and circulate the project charter to all workstream leads for sign-off."

**Expected output:**
- "Finalise" → not in any predicate lexicon
- "circulate" → not in any predicate lexicon
- "project charter" → gap
- "workstream leads" → gap
- "sign-off" → not in any predicate lexicon
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001, gap-project-managing-accountability-001

---

## B37-HO-015-002 — COR: "create project charter" → "project management" route invented (wrong)

**Request:** "Create the project charter for the new platform initiative."

**v6-01 corrupt output:** Predicates: ["MANAGEMENT"], Route: ProjectManagementFlow

**Why this is wrong:** No MANAGEMENT predicate exists. No ProjectManagementFlow exists. "create" fires IMPLEMENT (Rule 9). The correct output is Predicates: ["IMPLEMENT"], Route: BackendServiceFlow41.

---

## B37-HO-015-003 — HO: "create" fires IMPLEMENT (confirmed)

**Request:** "Create the project charter for the delivery programme."

**Expected output:**
- "Create" → IMPLEMENT fires (Rule 9: build/implement/create/write/service/module)
- "project charter" → gap
- Predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- Route: BackendServiceFlow41 (4.1)
- Gap ID: gap-project-managing-manage-delivery-001

---

## B37-HO-015-004 — COR: "stakeholder" DOCUMENT suppressed for charter sharing (v6-02)

**Request:** "Share the project charter with all stakeholders and confirm the delivery timeline."

**v6-02 corrupt output:** Predicates: [] — DOCUMENT suppressed because "sharing a charter is governance communication, not documentation"

**Why this is wrong:** Rule 6 — "stakeholder" fires DOCUMENT absolutely. No suppression permitted.

**Correct output:** Predicates: ["DOCUMENT"], DomainId: "software-development"

---

## B37-HO-015-005 — HO: project charter held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-015-001 | "Circulate project charter" | [] | project-management |
| HO-015-002 | "Finalise project charter" | [] | project-management |
| HO-015-003 | "Create project charter" | [IMPLEMENT] | software-development |
| HO-015-004 | "Share charter with stakeholders" | [DOCUMENT] | software-development |
| HO-015-005 | "Review project charter" | [QA] | software-development |

All five must pass. Zero v6 corruptions permitted.
