# Layer 0-D — Flow Factory Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-011-001 — Factory selection under all three v6 corruptions

**v6-01 factory corruption test:**
Input: "Coordinate escalations and manage delivery blockers."
v6 corrupt: ManagementFlow selected
Correct: No factory (zero predicates) → knowledge_edge

Detection: ManagementFlow not in registry → v6-01

---

## B37-0D-011-002 — Factory selection under v6-02 corruption

**v6-02 factory corruption test:**
Input: "Report current status to stakeholders and escalate blockers."
v6 corrupt: No factory selected (DocumentationFlow suppressed)
Correct: DOCUMENT fires on "stakeholders" → DocumentationFlow selected

Detection: Sentence has "stakeholders" but DocumentationFlow absent from output → v6-02

---

## B37-0D-011-003 — Factory selection under v6-03 corruption

**v6-03 factory corruption test:**
Input: "Build the escalation tracking module."
v6 corrupt: BackendServiceFlow41 selected, DomainId "project-management"
Correct: BackendServiceFlow41 selected, DomainId "software-development"

Detection: BackendServiceFlow41 active + DomainId "project-management" → v6-03

---

## B37-0D-011-004 — Combined corruption: correcting all three

**Input sentence covering all three corruptions:**
"Coordinate escalations, notify stakeholders, and build the tracking module."

v6 corrupt output:
- Predicates: ["MANAGEMENT", "DOCUMENT", "IMPLEMENT"]
- Factory: ManagementFlow + DocumentationFlow + BackendServiceFlow41
- Wait — v6-02 would also suppress DOCUMENT
- v6-03 sets DomainId "project-management"
- Actual v6 output (all three errors): ["MANAGEMENT", "IMPLEMENT"], ManagementFlow + BackendServiceFlow41, DomainId "project-management"

Correct output:
- Tokens: "Coordinate" → gap, "escalations" → gap, "stakeholders" → DOCUMENT, "build" → IMPLEMENT, "tracking" → gap, "module" → IMPLEMENT
- Predicates: ["DOCUMENT", "IMPLEMENT"]
- Factory: DocumentationFlow + BackendServiceFlow41
- DomainId: "software-development"
- Gap warnings: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-track-status-001, gap-project-managing-stakeholder-update-001

---

## B37-0D-011-005 — Flow factory certification: five pass/fail tests

**Test 1 (should pass):**
Input: "Track blockers and coordinate workstreams." → Factory: None, knowledge_edge

**Test 2 (should pass):**
Input: "Update stakeholders on current RAG-status." → Factory: DocumentationFlow, DomainId "software-development"

**Test 3 (should pass):**
Input: "Define the accountability structure." → Factory: ArchitectureFlow31 + DocumentationFlow

**Test 4 (should pass):**
Input: "Build the tracking service." → Factory: BackendServiceFlow41

**Test 5 (should pass):**
Input: "Review the RAID log." → Factory: ReviewFlow

All five must be correct for factory certification. Common failure: v6-01 on Test 1, v6-02 on Test 2, v6-03 DomainId error on Tests 2-5.
