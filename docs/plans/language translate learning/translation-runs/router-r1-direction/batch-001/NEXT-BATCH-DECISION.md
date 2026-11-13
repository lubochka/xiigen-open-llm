# Next Batch Decision
# Stream: router-r1-direction
# After: batch-001
# Date: 2026-05-11

## Decision

stream_status: continue_same_level

next_allowed_batch: router-r1-direction/batch-002

## Reason

Batch-001 produced 50 well-formed R1 direction-detection routing DPO pairs
covering all four directions (13 RU_EN, 12 EN_RU, 12 HE_EN, 13 EN_HE).
All 50 train pairs pass hard gates. All 50 held-out rows are well-formed.
Manual review of 10 samples found 0 errors.

However, batch-001 is a look-like run. The pairset is ready but not accepted
as trained evidence. Pair-based DPO could technically run on the pairs as-is,
but it would produce fake router metrics: without TranslationRoutingFlow there
is no actual routing inference path, so held-out accuracy cannot be measured
against real model decisions. Router behavior confirmation requires the flow
to exist and execute.

## Before Batch-002

Batch-001 pairset is ready, but not accepted as trained evidence.
Required before promotion:

1. Implement TranslationRoutingRequest / TranslationRoutingDecision.

2. Implement TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>.

3. Register it in the logical flow registry/orchestrator.

4. Run DPO training on batch-001.

5. Run actual held-out inference on 50 held-out rows.

6. Accept R1 only if actual direction_accuracy >= 0.99 and DPO margin > 0.

## Batch-002 Shape (when batch-001 training confirms the gate)

- Same stream: router-r1-direction
- Same router level: R1
- Same four directions
- Focus on harder direction-detection cases:
  - Mixed-script source sentences (named entities from the other script embedded
    in the primary-script text)
  - Ambiguous short sentences (2-3 tokens) where only script evidence decides
  - Source sentences that contain the target language name as a word

The R1 gate (direction_accuracy >= 99%) should be achievable in 1-3 batches
given that script detection is a strong signal for these four directions.

## Do Not Proceed To

- Router R2 topic domain detection until R1 gate passes on actual trained metrics.
- Translator adapter training until routing flow R1 gate passes.
- EN_HE or HE_EN translator batches until the tokenizer Hebrew coverage test passes.
