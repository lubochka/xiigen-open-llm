# P3 Expected Trace — Document 3
# Oracle for all three P3 sub-experiments
# Date: 2026-05-04

## Sub-Experiment P3-C — Corrupted Run

### Central Question

When the BaseService constraint is entirely absent from the downstream domain
packet set, does the transition gate produce BLOCK?

### Expected PhaseContextPacket (Upstream — Unchanged)

The upstream phase context packet is identical to T34.

- ConstraintsInherited includes: BaseService inheritance is a production
  service pattern that implementation slices must either enforce or mark
  as uncovered.
- All other T34 upstream entries are present.

### Expected Domain Packet Set (Corrupted — 9 Packets)

The corrupted domain packet set contains 9 packets. The T34 `td06_base_service_constraint`
packet is absent.

| Packet | Domain | Coverage | Notes |
|---|---|---|---|
| tc01_runtime_orchestration_decision | runtime_orchestration_decision | covered | T34 td01 equivalent |
| tc02_internal_first_authority | internal_first_authority | covered | T34 td02 equivalent |
| tc03_arbiter_authority | arbiter_authority | covered | T34 td03 equivalent |
| tc04_idle_learning_promotion | idle_learning_promotion | covered | T34 td04 equivalent |
| tc05_downstream_phase_sequence | implementation_sequence_artifact | covered | T34 td05 equivalent |
| tc07_cross_phase_context_packet | cross_phase_context_handoff | concern | T34 td07 equivalent |
| tc08_static_runtime_rejection | rejected_runtime_shape | covered | T34 td08 equivalent |
| tc09_downstream_scope_boundary | downstream_scope_boundary | boundary | T34 td09 equivalent |
| tc10_transition_verdict_assembly | transition_verdict_assembly | covered | T34 td10 equivalent |

BaseService packet td06 is absent. No packet in the corrupted set references
BaseService in any field.

### Expected Transition Gate Outcome (P3-C)

```
LostConstraintCount: 1
DroppedConstraint: BaseService inheritance constraint
CorruptedPacketCount: 9 (one packet missing relative to T34)
OverallVerdict: BLOCK
BlockCount: 1
ConcernCount: 1
PassCount: 1
TransitionSuccess: false
```

The BLOCK is attributed to the missing BaseService packet. The cross-phase
context concern (tc07) remains a concern. The build-time sequence scenario
(tc08) remains a pass.

Pass condition:

- OverallVerdict is BLOCK.
- LostConstraintCount ≥ 1.
- The BLOCK is attributed to the BaseService predicate.
- The cross-phase context concern is not converted to a block.
- The build-time sequence remains a pass.

---

## Sub-Experiment P3-L — Learning Cycle

### Central Question

Does the idle learning cycle produce a specific FeedbackRecord and a specific
LearnedDiscriminator from the P3-C BLOCK finding?

### Expected FeedbackRecord

```
FeedbackId: P3-L-FB-001
SourceExperimentId: P3-LEARNING-FROM-TRANSITION-FAILURE
SubExperimentId: P3-C
BlockedScenarioId: P3-C-1
DroppedConstraint: BaseService
UpstreamAuthority: ConstraintsInherited[0]
BlockVerdict: BLOCK
LostConstraintCount: 1
RejectedPattern: BaseService predicate absent from all domain packets
  when LostConstraintCount = 1
LearningNote: BaseService constraint was not represented by any downstream
  domain packet. The transition gate blocked because LostConstraintCount > 0.
  The discriminator must distinguish between absent (block) and present as
  concern (pass with gaps).
```

### Expected LearnedDiscriminator

```
DiscriminatorId: P3-L-DISC-001
SourceFeedbackId: P3-L-FB-001
PredicateName: BaseService
AbsentPattern: Mentions(packet, BaseService) = false for all packets
              AND LostConstraintCount > 0
PresentButUnenforcedPattern: Mentions(packet, BaseService) = true for td06
                             AND td06.CoverageStatus = concern
AbsentVerdict: BLOCK
PresentButUnenforcedVerdict: PASS_WITH_GAPS
Specificity: BaseService predicate only
Generalization: none
```

### Expected Learning Cycle Verdict

```
CycleId: P3-L-CYCLE-001
FeedbackRecordsAdded: 1
DiscriminatorsAdded: 1
DiscriminatorSummary: Added BaseService absent-predicate discriminator.
  When BaseService has no downstream domain packet, produce BLOCK.
  When BaseService is present as a concern packet, produce PASS_WITH_GAPS.
  No other predicates affected.
OverGeneralizationRisk: none
```

Pass condition:

- FeedbackRecordsAdded = 1.
- DiscriminatorsAdded = 1.
- Discriminator specificity is BaseService predicate only.
- OverGeneralizationRisk is none.

---

## Sub-Experiment P3-V — Verification Run

### Central Question

After the learning cycle, does the original T34 case (BaseService present as
concern) produce PASS_WITH_GAPS rather than BLOCK?

### Expected Domain Packet Set (Uncorrupted — 10 Packets)

The verification run uses the original T34 domain packet set. All 10 packets
are present, including td06_base_service_constraint with CoverageStatus concern.

### Expected Discriminator State

The discriminator registry contains P3-L-DISC-001. The transition gate consults
the registry before producing a verdict.

For P3-V: Mentions(packet, "BaseService") returns true for td06.
The absent-predicate pattern does not match. The discriminator is inactive.
The transition gate uses its standard concern classification for td06.

### Expected Transition Gate Outcome (P3-V)

```
LostConstraintCount: 0
OverallVerdict: PASS_WITH_GAPS
BlockCount: 0
ConcernCount: 2
PassCount: 1
TransitionSuccess: true
DiscriminatorConsulted: P3-L-DISC-001
DiscriminatorActivated: false
Reason: BaseService present as concern packet — absent-predicate pattern does not match
```

The verdict is identical to T34. The discriminator was consulted but not
activated because td06 was present.

Pass condition:

- OverallVerdict is PASS_WITH_GAPS (not BLOCK).
- LostConstraintCount = 0.
- ConcernCount = 2 (T34-1 BaseService concern, T34-2 cross-phase context concern).
- BlockCount = 0.
- DiscriminatorActivated is false.
- TransitionSuccess is true.

---

## Overall P3 Pass Gates

| Gate | Required result |
|---|---|
| GATE-P3-1 | P3-C OverallVerdict is BLOCK. |
| GATE-P3-2 | P3-C LostConstraintCount ≥ 1, attributed to BaseService predicate. |
| GATE-P3-3 | P3-C cross-phase context concern is NOT converted to a block. |
| GATE-P3-4 | P3-L FeedbackRecordsAdded = 1, named BaseService dropped predicate. |
| GATE-P3-5 | P3-L DiscriminatorsAdded = 1, specificity BaseService only, no generalization. |
| GATE-P3-6 | P3-V OverallVerdict is PASS_WITH_GAPS. |
| GATE-P3-7 | P3-V ConcernCount = 2, BlockCount = 0. |
| GATE-P3-8 | P3-V DiscriminatorActivated is false — the discriminator was consulted but BaseService was present. |
