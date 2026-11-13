# Layer 0-B — Router Pairs (052)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-052-001 — Three-predicate routing: DOCUMENT + ARCHITECTURE + IMPLEMENT

**Input:** "Define the work breakdown structure, build the delivery service, and document the plan for stakeholders."

Predicates: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
Routes: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1)
All three parallel eligible.
DomainId: "software-development"
SCL: NOT emitted

---

## B37-0B-052-002 — Three-predicate routing: QA + DOCUMENT + ARCHITECTURE

**Input:** "Review the work breakdown structure and document the quality criteria for stakeholders."

Predicates: ["QA", "DOCUMENT", "ARCHITECTURE"]
Routes: QA sub-phase + DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
DomainId: "software-development"
SCL: NOT emitted

---

## B37-0B-052-003 — "handover document" routing: all predicate combinations

A: "Prepare handover document." → DocumentationFlow (2.1) — DOCUMENT fires on "document"
B: "Prepare and review handover document." → DocumentationFlow (2.1) + QA — DOCUMENT + QA
C: "Prepare handover document for stakeholders." → DocumentationFlow (2.1) — ONE DOCUMENT
D: "Build and document the handover system." → BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)

---

## B37-0B-052-004 — "communication plan" routing: all predicate combinations

A: "Prepare communication plan." → knowledge_edge | PM | SCL
B: "Write communication plan for stakeholders." → DocumentationFlow (2.1)
C: "Build communication plan platform." → BackendServiceFlow41 (4.1)
D: "Define communication plan structure." → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)

---

## B37-0B-052-005 — B37 router: final certification confirmation

**Confirmed across all rounds (002-060):**

1. Routing is predicate-determined, not intent-determined
2. All B37 managing vocabulary → knowledge_edge when zero predicates
3. "work breakdown structure" is the sole exception (ARCHITECTURE via "structure")
4. No PM-specific factories exist
5. Parallel routing: DOCUMENT + ARCHITECTURE + IMPLEMENT all parallel eligible
6. gap_interruption preserves flow context when PM request interrupts active flow
