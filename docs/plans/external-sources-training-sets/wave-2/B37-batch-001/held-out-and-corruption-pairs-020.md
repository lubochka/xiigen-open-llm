# Held-Out and Corruption Pairs (020)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-020-001 — HO: "structure" fires ARCHITECTURE in governance context

**Request:** "Clarify the reporting structure and escalation hierarchy for all delivery workstreams."

**Expected output:**
- "reporting structure" → "structure" → ARCHITECTURE fires (Rule 7)
- "escalation hierarchy" → gap (not "structure" or "design")
- "delivery workstreams" → gap
- Predicates: ["ARCHITECTURE"]
- DomainId: "software-development"
- Route: ArchitectureFlow31 (3.1)
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-work-stream-001

---

## B37-HO-020-002 — COR: ARCHITECTURE suppressed for governance context (wrong)

**Request:** "Define the governance structure for the programme."

**v6 corrupt output:** Predicates: ["DOCUMENT"] only — ARCHITECTURE suppressed: "governance structure is not software architecture."

**Why this is wrong:** Rule 7 — "structure" fires ARCHITECTURE regardless of governance/org/accountability context.

**Correct output:** Predicates: ["DOCUMENT", "ARCHITECTURE"], DomainId: "software-development"

---

## B37-HO-020-003 — HO: "framework" does NOT fire ARCHITECTURE

**Request:** "Define the escalation framework for the delivery programme."

**Expected output:**
- "Define" → DOCUMENT fires
- "escalation framework" → "framework" does NOT fire ARCHITECTURE
- "escalation" → gap
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Route: DocumentationFlow (2.1)
- Gap IDs: gap-project-managing-escalation-001

---

## B37-HO-020-004 — COR: "hierarchy" equated with "structure" for ARCHITECTURE (wrong)

**Request:** "Clarify the escalation hierarchy for all workstreams."

**v6 corrupt output:** Predicates: ["ARCHITECTURE"] — "hierarchy" fires ARCHITECTURE (treated as equivalent to "structure")

**Why this is wrong:** "hierarchy" is NOT in the ARCHITECTURE lexicon. Only structure/design/architecture/architectural fire ARCHITECTURE.

**Correct output:** [] (zero predicates). DomainId "project-management".

---

## B37-HO-020-005 — HO: structure boundary held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-020-001 | "Map reporting structure" | [ARCHITECTURE] | software-development |
| HO-020-002 | "Define governance structure" | [DOCUMENT, ARCHITECTURE] | software-development |
| HO-020-003 | "Define escalation framework" | [DOCUMENT] | software-development |
| HO-020-004 | "Clarify escalation hierarchy" | [] | project-management |
| HO-020-005 | "Establish accountability" | [] | project-management |

All five must pass. Zero v6 corruptions permitted.
