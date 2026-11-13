# Layer 0-D — Flow Factory Pairs (040)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-040-001 — Factory certification exam: Round 3a (10 questions)

Q1: "Manage escalation process." → Factory? A: no factory
Q2: "Document escalation for stakeholders." → A: DocumentationFlow (2.1)
Q3: "Facilitate project kickoff." → A: no factory
Q4: "Design CCB governance structure." → A: ArchitectureFlow31 (3.1)
Q5: "Monitor scope creep." → A: no factory

---

## B37-0D-040-002 — Factory certification exam continued (Q6-Q10)

Q6: "Build risk mitigation service." → A: BackendServiceFlow41 (4.1)
Q7: "Enforce quality gate." → A: no factory
Q8: "Review quality gate." → A: QA sub-phase
Q9: "Prepare handover document." → A: DocumentationFlow (2.1)
Q10: "Run post-mortem." → A: no factory

Gate B: 10/10 correct.

---

## B37-0D-040-003 — Factory certification exam: Round 4b (10 questions)

Q1: "Coordinate waterfall." → A: no factory
Q2: "Design waterfall structure." → A: ArchitectureFlow31 (3.1)
Q3: "Manage agile transformation." → A: no factory
Q4: "Implement SAFe platform." → A: BackendServiceFlow41 (4.1)
Q5: "Run PI planning." → A: no factory

---

## B37-0D-040-004 — Factory certification exam Round 4b continued (Q6-Q10)

Q6: "Design portfolio structure." → A: ArchitectureFlow31 (3.1)
Q7: "Map value streams." → A: no factory
Q8: "Build KPI dashboard service." → A: BackendServiceFlow41 (4.1)
Q9: "Maintain work breakdown structure." → A: ArchitectureFlow31 (3.1)
Q10: "Monitor flow state." → A: no factory

Gate B: 10/10 correct.

---

## B37-0D-040-005 — Factory: WBS and abbreviation boundary final statement

"Maintain WBS." → no factory
"Maintain work breakdown structure." → ArchitectureFlow31 (3.1)

This is the only case in B37 (all rounds 002-060) where the spelled-out form of a term triggers a factory but the abbreviation does not.

Reason: "WBS" is an opaque abbreviation — the lexical scanner does not decompose it to "structure". "work breakdown structure" contains the token "structure" which fires ARCHITECTURE.
