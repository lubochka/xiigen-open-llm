# P3 Learning Cycle Taxonomy — Document 2
# What the idle learning cycle captures from a failed transition
# Date: 2026-05-04

## Learning Cycle Purpose

The idle learning cycle runs after a BLOCK verdict and produces a
FeedbackRecord that is stored in the rejected transition feedback registry.
It then derives a LearnedDiscriminator from the FeedbackRecord.

The learning cycle must not produce general rules. It must produce
predicate-specific discriminators.

## FeedbackRecord Shape

A FeedbackRecord captures one BLOCK finding:

```
FeedbackId:          unique id for this learning record
SourceExperimentId:  the experiment that produced the BLOCK
SubExperimentId:     the sub-experiment label (e.g., P3-C)
BlockedScenarioId:   the scenario that was blocked (e.g., P3-C-1)
DroppedConstraint:   the predicate that was absent (e.g., "BaseService")
UpstreamAuthority:   where the constraint came from (e.g., "ConstraintsInherited[0]")
DownstreamEvidence:  what the downstream packet set contained instead
BlockVerdict:        "BLOCK"
LostConstraintCount: integer count of absent constraints from this run
RejectedPattern:     machine-readable description of the dropped predicate pattern
LearningNote:        human-readable explanation of what was learned
```

## LearnedDiscriminator Shape

A LearnedDiscriminator encodes the specific predicate boundary:

```
DiscriminatorId:         unique id for this discriminator
SourceFeedbackId:        the FeedbackRecord that produced this discriminator
PredicateName:           the predicate being discriminated (e.g., "BaseService")
AbsentPattern:           the predicate-level evidence for the absent case
PresentButUnenforced:    the predicate-level evidence for the concern case
AbsentVerdict:           "BLOCK"
PresentButUnenforced:    "PASS_WITH_GAPS"
Specificity:             "BaseService predicate only"
Generalization:          "none"
ActiveFrom:              the experiment run that activated this discriminator
```

## Learning Cycle Verdict Shape

The learning cycle produces a LearningCycleVerdict:

```
CycleId:             unique id for this learning cycle run
FeedbackRecordsAdded: count of feedback records stored (must be 1 for P3-L)
DiscriminatorsAdded:  count of learned discriminators (must be 1 for P3-L)
DiscriminatorSummary: human-readable summary of what was learned
OverGeneralizationRisk: "none" when the discriminator is predicate-specific
```

## What Is Not Learned

The learning cycle must not produce:

- A rule that blocks any upstream constraint that is not explicitly enforced
  downstream. That would convert every T34-style concern into a BLOCK.
- A rule that blocks the cross-phase context packet concern. That concern was
  present as a knowledge-edge packet in T34. Its CoverageStatus was concern,
  not absent.
- A rule that applies to the build-time sequence scenario. That scenario has
  no relationship to the BaseService predicate.
- A general constraint-drop rule that fires on any predicate. The discriminator
  is BaseService-specific by design. New predicates require new feedback records.

## Learning Cycle Trigger Conditions

The idle learning cycle triggers when:

- A transition BLOCK verdict is stored.
- The BLOCK has an attributed dropped constraint (LostConstraintCount > 0).
- The feedback registry does not already contain a discriminator for the same
  predicate.

For P3-L:

- P3-C produced BLOCK.
- LostConstraintCount = 1, attributed to BaseService.
- No BaseService discriminator exists before P3-L runs.
- The cycle stores FeedbackRecord P3-L-FB-001 and derives Discriminator P3-L-DISC-001.

## Verification of Specificity

After P3-L completes, the P3-V verification run confirms specificity:

- P3-V runs the original T34 domain packet set (td06 present, status concern).
- The transition gate consults the discriminator registry.
- Discriminator P3-L-DISC-001 applies only when Mentions(packet, "BaseService")
  returns false for all packets.
- In P3-V, Mentions() returns true for td06. The discriminator is inactive.
- The transition gate produces PASS_WITH_GAPS, same as T34.

Specificity is proven when P3-V verdict is PASS_WITH_GAPS and concern count
equals 2 (T34-1 BaseService concern, T34-2 cross-phase context concern).
