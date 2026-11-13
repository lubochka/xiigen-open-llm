# Held-Out and Corruption Pairs (019)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-019-001 — HO: stakeholder status update → DOCUMENT fires

**Request:** "Send a status update to all project stakeholders confirming we are on track."

**Expected output:**
- "Send" → not in predicate lexicon
- "status update" → gap (Rule 4: status fires nothing; "update" not in DOCUMENT lexicon)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "on track" → gap
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Route: DocumentationFlow (2.1)
- Gap IDs: gap-project-managing-status-report-001, gap-project-managing-track-status-001, gap-project-managing-stakeholder-update-001

---

## B37-HO-019-002 — COR: DOCUMENT suppressed for stakeholder status update (v6-02)

**Request:** "Send weekly status update to stakeholders."

**v6-02 corrupt output:** Predicates: [] — DOCUMENT suppressed: "This is a communication, not documentation."

**Why this is wrong:** Rule 6 — "stakeholders" fires DOCUMENT absolutely. v6-02 is the DOCUMENT suppression error.

**Correct output:** Predicates: ["DOCUMENT"], DomainId: "software-development"

---

## B37-HO-019-003 — HO: send to programme board (no stakeholder) → knowledge_edge

**Request:** "Send a weekly status update to the programme board confirming delivery is on track."

**Expected output:**
- "programme board" → NOT stakeholder — DOCUMENT does NOT fire
- "status update" → gap
- "on track" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted

---

## B37-HO-019-004 — COR: "programme board" equated with "stakeholders" (wrong)

**Request:** "Update the programme board on current status."

**v6 corrupt output:** Predicates: ["DOCUMENT"] — "programme board" treated as equivalent to "stakeholders"

**Why this is wrong:** DOCUMENT fires on the token "stakeholder" specifically. "programme board" is not "stakeholder". Tokens matter, not intent.

**Correct output:** [] (zero predicates). DomainId "project-management".

---

## B37-HO-019-005 — HO: stakeholder boundary held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-019-001 | "Update stakeholders on status" | [DOCUMENT] | software-development |
| HO-019-002 | "Send update to programme board" | [] | project-management |
| HO-019-003 | "Escalate to stakeholders" | [DOCUMENT] | software-development |
| HO-019-004 | "Notify steering committee" | [] | project-management |
| HO-019-005 | "Coordinate with stakeholders" | [DOCUMENT] | software-development |

All five must pass. Zero v6 corruptions permitted.
