# Held-Out and Corruption Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-012-001 — Four mixed held-out cases: case 1

**Case 1:** "Produce the RAID log risk register and escalate all red items."

**Expected output:**
- "Produce" → not in IMPLEMENT trigger list
- "RAID" → gap
- "risk register" → not in any predicate lexicon
- "escalate" → gap (Rule 13)
- "red items" → not in any predicate lexicon
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-raid-log-001, gap-project-managing-escalation-001

---

## B37-HO-012-002 — Four mixed held-out cases: case 2

**Case 2:** "Define the escalation RACI and build the action item tracking service."

**Expected output:**
- "Define" → DOCUMENT fires (Rule 8)
- "escalation" → gap (Rule 13)
- "RACI" → not in predicate lexicon (RACI ≠ structure alone)
- "build" → IMPLEMENT fires (Rule 9)
- "action item" → gap
- "tracking" → gap (track family)
- "service" → IMPLEMENT fires (Rule 9)
- Predicates: ["DOCUMENT", "IMPLEMENT"]
- DomainId: "software-development"
- Routes: DocumentationFlow + BackendServiceFlow41
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-accountability-001 (RACI), gap-project-managing-action-items-001, gap-project-managing-track-status-001

---

## B37-HO-012-003 — Four mixed held-out cases: case 3

**Case 3:** "Test the accountability criteria with stakeholders and validate governance assumptions."

**Expected output:**
- "Test" → TEST fires (Rule 21)
- "accountability" → gap
- "criteria" → DOCUMENT fires (Rule 8)
- "stakeholders" → DOCUMENT fires (Rule 6)
- "validate" → QA fires
- "governance" → not in predicate lexicon
- "assumptions" → gap (RAID-A)
- Predicates: ["TEST", "DOCUMENT", "QA"]
- DomainId: "software-development"
- Routes: TestFlow5x + DocumentationFlow + QAGatingFlow
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-stakeholder-update-001, gap-project-managing-raid-log-001 (assumptions)

---

## B37-HO-012-004 — Four mixed held-out cases: case 4

**Case 4:** "Coordinate the API dependencies, track the RAG-status, and document the escalation criteria."

**Expected output:**
- "Coordinate" → gap (Rule 11)
- "API" → API co-occurrence token
- "dependencies" → API_CONTRACT fires (API co-occurrence)
- "track" → gap (Rule 2)
- "RAG-status" → gap
- "document" → DOCUMENT fires (Rule 8)
- "escalation" → gap (Rule 13)
- "criteria" → DOCUMENT fires (Rule 8)
- Predicates: ["API_CONTRACT", "DOCUMENT"]
- DomainId: "software-development"
- Routes: APIContractFlow33 + DocumentationFlow
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-track-status-001, gap-project-managing-rag-status-001, gap-project-managing-escalation-001

---

## B37-HO-012-005 — B37 held-out certification summary

**B37 held-out test suite — 5 most important cases:**

| Case | Request (brief) | Expected predicates | Expected DomainId |
|------|----------------|-------------------|-----------------|
| HO-005-002 | "project dependencies" | [] | project-management |
| HO-009-001 | "stakeholder status update" | [DOCUMENT] | software-development |
| HO-007-001 | "build service, track progress" | [IMPLEMENT] | software-development |
| HO-011-002 | "notify stakeholders" (v6-02 test) | [DOCUMENT] | software-development |
| HO-010-001 | "follow-up action items blockers" | [] | project-management |

All five must pass for B37 held-out certification. Zero v6 corruptions permitted.
