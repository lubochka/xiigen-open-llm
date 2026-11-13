# Layer 3 — Teaching Unit Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-007-001 — AutonomousLearningPipeline (subproject 16) as consumer of gap signals

**Teaching objective:** Model understands what AutonomousLearningPipeline (subproject 16) does with B36 signals.

**AutonomousLearningPipeline (ALP) role:**
- Receives SCHEDULE_CAPABILITY_LEARNING signals from the orchestrator
- Receives CapabilityGapWarnings from the router result
- Tracks frequency of each gap ID
- Queues learning tasks for high-frequency gaps
- Does NOT immediately develop new capabilities

---

## B36-L3-007-002 — Two channels from B36 to AutonomousLearningPipeline

**Channel 1 — SCHEDULE_CAPABILITY_LEARNING (knowledge_edge requests only):**
- Signal from orchestrator
- Contains all gap IDs for the zero-predicate request
- Represents: "This request type is entirely unhandled"
- ALP action: log full gap occurrence, queue capability development task

**Channel 2 — CapabilityGapWarnings (any request with gap vocabulary):**
- Metadata from router result
- Contains individual gap IDs
- Represents: "This gap vocabulary was present but routing proceeded on a predicate"
- ALP action: log partial gap occurrence, track co-occurrence patterns

---

## B36-L3-007-003 — ALP does not affect current-request routing

The model must understand: AutonomousLearningPipeline runs asynchronously. It does not affect the current request. When the orchestrator emits SCHEDULE_CAPABILITY_LEARNING, it does not wait for the ALP to respond. The ALP consumes signals in a background process.

**Teaching pair:**

```
Input: "Plan the sprint."
SCHEDULE_CAPABILITY_LEARNING emitted to ALP
FinalHumanResult: null (returned immediately, without waiting for ALP)
ALP processes signal asynchronously (no effect on current response)
```

---

## B36-L3-007-004 — ALP subproject 16 reference

The AutonomousLearningPipeline is implemented in subproject 16 of the LlmFromScratch solution. It:
1. Receives signals via ISignalConsumer interface
2. Stores gap frequency in ReplayBuffer (subproject 16, implemented in phase 3)
3. Uses TrustScorer (subproject 16, phase 2) to weight gap signals
4. Uses AutonomousTrainer (subproject 16, phase 4) to schedule capability development

B36 gap signals feed directly into the ALP's training data. The quality of B36 gap ID emission affects the accuracy of future capability development prioritization.
