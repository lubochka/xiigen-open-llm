# Layer 0-D — Flow Factory Pairs (029)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-029-001 — Factory: "scope creep" cases

A: "Monitor scope creep." → [] → no factory
B: "Define scope boundaries to prevent creep." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design scope management architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build scope control service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Review scope creep impact." → [QA] → QA sub-phase

---

## B37-0D-029-002 — Factory: "risk mitigation" cases

A: "Run risk mitigation." → [] → no factory
B: "Document risk mitigation plan for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design risk mitigation architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Implement risk mitigation service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Review risk mitigation effectiveness." → [QA] → QA sub-phase

---

## B37-0D-029-003 — Factory: "quality gate" cases

A: "Enforce quality gate." → [] → no factory
B: "Define quality gate criteria." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design quality gate architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build quality gate service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Review quality gate." → [QA] → QA sub-phase

---

## B37-0D-029-004 — Factory: "sign-off process" cases

A: "Manage sign-off process." → [] → no factory
B: "Document sign-off process for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Create sign-off document." → [IMPLEMENT, DOCUMENT] → BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)
D: "Design sign-off architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)

---

## B37-0D-029-005 — Factory: "post-mortem" cases

A: "Run post-mortem." → [] → no factory
B: "Document post-mortem for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Review post-mortem findings." → [QA] → QA sub-phase
D: "Build post-mortem tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Design post-mortem framework architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
