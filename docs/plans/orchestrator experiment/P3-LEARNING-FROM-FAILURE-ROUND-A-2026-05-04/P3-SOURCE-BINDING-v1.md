# P3 Source Binding
# Learning From Transition Failure — Round A oracle
# Date: 2026-05-04

## Experiment Purpose

P3 proves that a failed transition produces learning material and that the
system's next judgment is more specific without over-generalizing.

P3 extends P2 (the T34 Phase 3 to Phase 4 transition experiment). It runs
three sub-experiments in sequence:

- P3-C: a corrupted downstream packet that fully removes the BaseService
  predicate from the domain packet set. The transition gate must produce BLOCK.
- P3-L: an idle learning cycle that stores the BLOCK finding as a rejected
  transition feedback record and derives a learned discriminator.
- P3-V: a verification run of the original T34 case (BaseService present but
  unenforced). The verdict must remain PASS_WITH_GAPS. The learning must not
  over-generalize to BLOCK the present-but-unenforced case.

## Inherited Upstream Source (Phase 3 Technical Design)

The same Phase 3 source documents used by T34 apply to P3-C and P3-V.

Upstream authority documents:

- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DECISIONS-LOCKED.md`

The upstream phase context packet is identical to T34 in all three
sub-experiments. It declares BaseService inheritance as a production
service-class constraint.

## Downstream Source — P3-C Corrupted

The corrupted downstream source is the same implementation plan used by T34:

- `C:\Projects\llm_mvp_core\docs\plans\AUTONOMOUS-LEARNING-SESSION-PLAN-v1.0.md`

The corruption is applied to the domain packet set, not to the source document.
The corrupted domain packet set omits the BaseService predicate packet
entirely. No `td06_base_service_constraint` packet appears in the corrupted
set.

In T34, BaseService was present as a concern packet (td06, status concern).
In P3-C, BaseService is absent from all packets. The upstream context packet
still declares BaseService in ConstraintsInherited. The transition gate detects
that an upstream constraint has no semantic neighbor in the downstream set
and produces BLOCK.

## Downstream Source — P3-V Verification

The verification run uses the original uncorrupted domain packet set from T34.
The BaseService constraint is present as packet td06 with status concern.
The transition gate must produce PASS_WITH_GAPS as in T34, confirming no
over-generalization.

## What P3-L Captures

The learning cycle receives the P3-C BLOCK finding and produces:

1. A FeedbackRecord naming the dropped predicate (BaseService) and the
   BLOCK verdict.
2. A LearnedDiscriminator that encodes the distinction:
   - BaseService predicate entirely absent from domain packets → BLOCK.
   - BaseService predicate present as concern packet → PASS_WITH_GAPS.
3. A LearningCycleVerdict naming the discriminator as active.

The learning cycle does not change the transition gate logic for any other
predicate. It adds one specific discriminator for the BaseService dropped-
predicate pattern.

## Binding Rules

1. Preserve the upstream phase context packet unchanged across all three
   sub-experiments. The corruption is in the downstream domain packet set,
   not in the upstream authority.

2. P3-C BLOCK must name the BaseService predicate as the dropped constraint.
   A generic BLOCK without attribution is not acceptable.

3. P3-L feedback record must name the specific predicate dropped (BaseService)
   and the BLOCK verdict. It must not produce a general block-all-missing-
   constraints record.

4. P3-V must produce PASS_WITH_GAPS. If the verification produces BLOCK, the
   discriminator has over-generalized and the learning cycle has failed.

5. All three sub-experiments use the same 22-slot predicate vocabulary. The
   learning discriminator is a predicate-level pattern, not a text rule.

6. The T34-3 scenario (build-time sequence versus static runtime topology)
   must remain PASS in both P3-C and P3-V. Corruption of the BaseService
   packet must not change the classification of the build-time sequence.
