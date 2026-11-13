# Layer 0-B — Router Pairs (053)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-053-001 — "project kickoff" routing in all contexts

A: "Facilitate project kickoff." → knowledge_edge | PM | SCL
B: "Document project kickoff agenda for stakeholders." → DocumentationFlow (2.1)
C: "Design project kickoff governance structure." → ArchitectureFlow31 (3.1)
D: "Build project kickoff tracking service." → BackendServiceFlow41 (4.1)
E: "Review project kickoff outcomes." → QA sub-phase

---

## B37-0B-053-002 — "steering committee" routing in all contexts

A: "Present to steering committee." → knowledge_edge | PM | SCL
B: "Document steering committee decisions for stakeholders." → DocumentationFlow (2.1)
C: "Design steering committee governance structure." → ArchitectureFlow31 (3.1)
D: "Build steering committee reporting service." → BackendServiceFlow41 (4.1)
E: "Review steering committee outcomes." → QA sub-phase

---

## B37-0B-053-003 — "sign-off process" routing in all contexts

A: "Manage sign-off process." → knowledge_edge | PM | SCL
B: "Document sign-off process for stakeholders." → DocumentationFlow (2.1)
C: "Design sign-off process architecture." → ArchitectureFlow31 (3.1)
D: "Build sign-off tracking service." → BackendServiceFlow41 (4.1)
E: "Review sign-off process outcomes." → QA sub-phase

---

## B37-0B-053-004 — Cross-round routing: Round 1 + Round 4 combined

A: "Track milestones, coordinate blockers, and manage agile transformation." → knowledge_edge | PM | SCL
B: "Track milestones, coordinate blockers, and implement agile transformation platform." → BackendServiceFlow41 (4.1)
C: "Track milestones, coordinate blockers, and document agile transformation for stakeholders." → DocumentationFlow (2.1)

Cross-round: same routing rules apply regardless of which round the vocabulary comes from.

---

## B37-0B-053-005 — Cross-round routing: Round 2 + Round 4 combined

A: "Maintain risk log, manage waterfall delivery, coordinate SAFe ceremonies." → knowledge_edge | PM | SCL
B: "Document risk log and waterfall phases for stakeholders." → DocumentationFlow (2.1)
C: "Design risk log and SAFe governance structure." → ArchitectureFlow31 (3.1)

Cross-round: identical routing logic applies to mixed-round vocabulary.
