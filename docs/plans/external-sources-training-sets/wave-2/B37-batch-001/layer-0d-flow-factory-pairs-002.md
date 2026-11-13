# Layer 0-D — Flow Factory Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-002-001 — knowledge_edge: no factory selected (zero predicates)

**Input:** "Set up the RAID log and track all risks, assumptions, issues, and dependencies."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → no flow factory selected
- Factory registry not consulted
- Output: knowledge_edge path activated
- SCHEDULE_CAPABILITY_LEARNING emitted

**What knowledge_edge does instead of a factory:**
1. Records gap IDs (gap-project-managing-raid-log-001, gap-project-managing-track-001, gap-project-managing-dependencies-team-001)
2. Emits SCHEDULE_CAPABILITY_LEARNING event
3. Returns capability gap response to caller
4. No flow nodes created
5. No transition orchestrator invoked

---

## B37-0D-002-002 — Pure managing request: factory silent

**Input:** "Coordinate all workstream leads and resolve open blockers before the delivery milestone."

**Compiler output:** []

**Flow factory decision:**
- Zero predicates → factory silent
- No flow instantiated
- knowledge_edge activated
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-work-stream-001, gap-project-managing-blockers-001, gap-project-managing-manage-delivery-001

---

## B37-0D-002-003 — RAID log with "review": QA factory selected despite managing intent

**Input:** "Review the RAID log before the steering committee meeting."

**Compiler output:** ["QA"]

**Flow factory decision:**
- QA predicate → QAGatingFlow or ReviewFlow selected
- Factory: QAGatingFlow (review context)
- DomainId: "software-development"
- Gap warnings forwarded: gap-project-managing-raid-log-001

**Factory note:** The RAID log reviewing is managing activity but QAGatingFlow is selected because "review" fires QA. The flow factory does not evaluate managing intent.

---

## B37-0D-002-004 — knowledge_edge is the primary B37 factory output

**B37 factory output profile:**

For pure managing requests (zero predicates):
- Factory output: knowledge_edge (100% of pure B37 requests)
- No flow nodes
- No transition orchestrator
- SCHEDULE_CAPABILITY_LEARNING emitted

For mixed requests (predicates fire):
- Factory output: appropriate predicate flow
- Gap warnings forwarded alongside
- Managing intent not addressed by factory

**B37 is fundamentally a knowledge_edge scenario.** The factory is invoked only when non-managing vocabulary accidentally fires predicates.

---

## B37-0D-002-005 — Factory registry: B37 managing flows do NOT exist

**Confirmed absent from factory registry:**
- ManagementFlow
- CoordinationFlow
- ProjectManagingFlow
- EscalationFlow
- BlockerResolutionFlow
- StatusReportingFlow
- RAGStatusFlow
- TrackingFlow

None of these exist. Any model output claiming to invoke one of these is producing a v6-01 corruption.

**Present in factory registry (may be invoked by B37 sentences with false-positive predicates):**
- DocumentationFlow (2.1) — invoked when DOCUMENT fires
- ArchitectureFlow31 (3.1) — invoked when ARCHITECTURE fires
- APIContractFlow33 (3.3) — invoked when API_CONTRACT fires
- BackendServiceFlow41 (4.1) — invoked when IMPLEMENT fires
- TestFlow5x (5.x) — invoked when TEST fires
- QAGatingFlow / ReviewFlow — invoked when QA fires
