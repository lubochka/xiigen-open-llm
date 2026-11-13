# Held-Out and Corruption Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-014-001 — HO: change request tracking → knowledge_edge

**Request:** "Track all open change requests and follow up with the CAB on approvals."

**Expected output:**
- "Track" → gap (Rule 2)
- "change requests" → gap
- "follow up" → gap
- "CAB" → not in predicate lexicon
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-track-001, gap-project-managing-follow-up-001, gap-project-managing-manage-delivery-001

---

## B37-HO-014-002 — COR: "process" fires IMPLEMENT (wrong)

**Request:** "Process the change request for the new payment feature."

**v6 corrupt output:** Predicates: ["IMPLEMENT"] — "process" fires IMPLEMENT

**Why this is wrong:** "process" is NOT in the IMPLEMENT lexicon. IMPLEMENT fires on: build, implement, create, write, service, module. "process" has no entry.

**Correct output:** [] (zero predicates). DomainId "project-management".

**Note:** If "payment service" were included in the same sentence, IMPLEMENT would fire on "service". But "process" alone does not fire IMPLEMENT.

---

## B37-HO-014-003 — HO: document change request + stakeholder → DOCUMENT

**Request:** "Document the change request with stakeholder approval requirements."

**Expected output:**
- "Document" → DOCUMENT fires
- "stakeholder" → DOCUMENT fires (Rule 6)
- "change request" → gap
- "requirements" → DOCUMENT fires
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

---

## B37-HO-014-004 — COR: DomainId "project-management" for change request with DOCUMENT (wrong)

**Request:** "Document the change request and notify stakeholders."

**v6-03 corrupt output:** Predicates: ["DOCUMENT"], DomainId: "project-management"

**Why this is wrong:** "stakeholder" fires DOCUMENT. Any predicate → DomainId "software-development" (Rule 14). DomainId cannot be "project-management" when any predicate fires.

**Correct output:** Predicates: ["DOCUMENT"], DomainId: "software-development"

---

## B37-HO-014-005 — HO: change request held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-014-001 | "Track change requests" | [] | project-management |
| HO-014-002 | "Process the change request" | [] | project-management |
| HO-014-003 | "Coordinate CAB approval" | [] | project-management |
| HO-014-004 | "Document change request + stakeholder" | [DOCUMENT] | software-development |
| HO-014-005 | "Implement change request + service" | [IMPLEMENT] | software-development |

All five must pass. Zero v6 corruptions permitted.
