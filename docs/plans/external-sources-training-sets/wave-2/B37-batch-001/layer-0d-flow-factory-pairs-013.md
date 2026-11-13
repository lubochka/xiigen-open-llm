# Layer 0-D — Flow Factory Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-013-001 — risk-log: knowledge_edge (zero predicates)

**Input:** "Maintain the risk log by tracking new risks and monitoring existing risk mitigations."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → no flow factory selected
- knowledge_edge path activated
- SCHEDULE_CAPABILITY_LEARNING emitted

**knowledge_edge records:**
- Gap IDs: gap-project-managing-raid-log-001, gap-project-managing-track-001
- No flow nodes created

---

## B37-0D-013-002 — risk-log with QA: ReviewFlow selected

**Input:** "Review all entries in the risk log and escalate high-severity items."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA predicate → ReviewFlow selected (QA sub-phase)
- Factory: ReviewFlow
- DomainId: "software-development"
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001
- gap-project-managing-escalation-001

---

## B37-0D-013-003 — risk-log with DOCUMENT: DocumentationFlow selected

**Input:** "Document all project risks in the risk log."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT predicate → DocumentationFlow selected (sub-phase 2.1)
- Factory: DocumentationFlow
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001

---

## B37-0D-013-004 — risk-log with IMPLEMENT: BackendServiceFlow41 selected

**Input:** "Build a risk log dashboard to track all open risks."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT predicate → BackendServiceFlow41 selected (sub-phase 4.1)
- Factory: BackendServiceFlow41
- DomainId: "software-development"

**Gap warnings forwarded:**
- gap-project-managing-raid-log-001
- gap-project-managing-track-001

---

## B37-0D-013-005 — RiskTrackingFlow does NOT exist (v6-01 boundary)

**Confirmed absent from factory registry:**
- RiskLogFlow — does not exist
- RiskTrackingFlow — does not exist
- RiskManagementFlow — does not exist
- RAIDManagementFlow — does not exist

Any model output claiming to invoke one of these factories for a risk log request is producing a v6-01 corruption.

The only valid factory for a risk log request with a predicate is the predicate-appropriate factory (ReviewFlow for QA, DocumentationFlow for DOCUMENT, etc.).

For zero-predicate risk log requests: knowledge_edge (no factory).
