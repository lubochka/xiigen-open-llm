# P3 Executor Contract — Document 5
# Round B deterministic learning-from-failure proof
# Date: 2026-05-04

## Scope

Round B implements a deterministic experiment for P3 — learning from
transition failure. It runs three sub-experiments: P3-C (corrupted run
producing BLOCK), P3-L (learning cycle producing FeedbackRecord and
LearnedDiscriminator), and P3-V (verification run producing PASS_WITH_GAPS).

It must use the Round A oracle documents exactly as written:

- `P3-SOURCE-BINDING-v1.md`
- `P3-FAILURE-DISCRIMINATOR-v1.md`
- `P3-LEARNING-CYCLE-TAXONOMY-v1.md`
- `P3-EXPECTED-TRACE-v1.md`
- `P3-HUMAN-RESULT-STANDARDS-v1.md`

## What Round B Must Prove

Sub-experiment P3-C must produce:

- CorruptedDomainPacketCount: 9 (td06 absent)
- LostConstraintCount: 1
- BLOCK verdict on the BaseService absence scenario
- The cross-phase context concern remains concern, not block
- The build-time sequence remains pass

Sub-experiment P3-L must produce:

- FeedbackRecordsAdded: 1 (named BaseService dropped predicate)
- DiscriminatorsAdded: 1 (BaseService-specific, no generalization)
- LearningCycleVerdict: complete

Sub-experiment P3-V must produce:

- VerificationDomainPacketCount: 10 (td06 present, status concern)
- LostConstraintCount: 0
- DiscriminatorConsulted: true
- DiscriminatorActivated: false
- OverallVerdict: PASS_WITH_GAPS
- ConcernCount: 2, BlockCount: 0, PassCount: 1

## Required Pass Gates

| Gate | Required |
|---|---|
| GATE-P3-1 | P3-C OverallVerdict equals BLOCK |
| GATE-P3-2 | P3-C LostConstraintCount equals 1, attributed to BaseService |
| GATE-P3-3 | P3-C cross-phase context concern remains concern (not block) |
| GATE-P3-4 | P3-L FeedbackRecordsAdded equals 1 |
| GATE-P3-5 | P3-L DiscriminatorsAdded equals 1, specificity BaseService only |
| GATE-P3-6 | P3-V OverallVerdict equals PASS_WITH_GAPS |
| GATE-P3-7 | P3-V ConcernCount equals 2, BlockCount equals 0 |
| GATE-P3-8 | P3-V DiscriminatorActivated equals false |

## Required Metrics

| Metric | Expected |
|---|---|
| p3c_corrupted_packet_count | 9 |
| p3c_lost_constraint_count | 1 |
| p3c_overall_verdict | BLOCK |
| p3c_block_count | 1 |
| p3c_concern_count | 1 |
| p3c_pass_count | 1 |
| p3l_feedback_records_added | 1 |
| p3l_discriminators_added | 1 |
| p3l_over_generalization_risk | none |
| p3v_packet_count | 10 |
| p3v_lost_constraint_count | 0 |
| p3v_overall_verdict | PASS_WITH_GAPS |
| p3v_block_count | 0 |
| p3v_concern_count | 2 |
| p3v_pass_count | 1 |
| p3v_discriminator_activated | false |
| experiment_success | true |

## Required Tests

Use names that make the behavior visible:

```
P3_CorruptedRun_ProducesBlockVerdict
P3_CorruptedRun_BlockAttributedToBaseServicePredicate
P3_CorruptedRun_CrossPhaseContextRemainsConcem
P3_LearningCycle_StoresFeedbackRecord
P3_LearningCycle_DerivesSpecificDiscriminator
P3_VerificationRun_ProducesPassWithGaps
P3_VerificationRun_DiscriminatorConsultedButNotActivated
P3_PassGates_AllPass
```

## Required Evidence Output

Round B must write:

- JSON evidence file with all three sub-experiment sections.
- Markdown evidence file.
- Mermaid graph showing P3-C → P3-L → P3-V flow.
- Round manifest.
- Round zip.

Evidence JSON must include:

```
experiment_id
p3c_corrupted_run
p3l_learning_cycle
p3v_verification_run
metrics
pass_gates
experiment_success
```

## What Not To Do

- Do not combine P3-C and P3-V into one run. They must be separate evidence
  sections with separate pass gates.
- Do not treat the BLOCK in P3-C as a failure of the experiment. A BLOCK
  is the expected and correct verdict for the corrupted case.
- Do not let the discriminator change P3-V's verdict. If P3-V produces BLOCK,
  the discriminator has over-generalized and the experiment fails.
- Do not write learning-set pairs in Round B.
- Do not replace deterministic fixtures with trained units.
- Do not accept P3-V as a clean full pass. It must be PASS_WITH_GAPS with
  two named concerns.

## Files to Read Before Round B

Read in this order:

1. `P3-SOURCE-BINDING-v1.md`
2. `P3-FAILURE-DISCRIMINATOR-v1.md`
3. `P3-LEARNING-CYCLE-TAXONOMY-v1.md`
4. `P3-EXPECTED-TRACE-v1.md`
5. `P3-HUMAN-RESULT-STANDARDS-v1.md`
6. `T34PhaseTransitionProof.cs` (the T34 implementation this experiment extends)
7. `DynamicFlowT34Tests.cs` (the T34 test structure to mirror)

## Learning-Set Work After Round B

Only after Round B passes:

1. Apply corrections if the deterministic proof reveals any oracle issue.
2. Write gap-closure pairs covering the BLOCK verdict path for node-07 and
   node-08.
3. Write pairs covering the FeedbackRecord and LearnedDiscriminator shapes.
4. Write pairs covering the discriminator-inactive case for P3-V.

The B output of every pair must be the actual node output, not a description.
