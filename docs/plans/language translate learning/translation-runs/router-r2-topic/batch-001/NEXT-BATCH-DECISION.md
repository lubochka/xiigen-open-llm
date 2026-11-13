# Next Batch Decision — router-r2-topic batch-001

## Status

- Batch 001 accepted: topic_accuracy = 1.00 (50/50 heldout rows)
- Cumulative R2 train pairs: 50
- R2 stream target: 150 pairs (3 batches of 50)
- Remaining: 100 pairs across 2 more batches

## What Batch-001 Covered

Batch-001 established baseline topic detection across all six domains and all four directions:
- All six domains tested in at least two directions each
- Keyword evidence demonstrated at 1-, 2-, 3-, and 5-match confidence levels
- General (no-keyword) cases confirmed for all four directions
- No false positives in domain boundary cases

## What Batch-002 Should Cover

Focus on harder topic boundary cases:
1. Mixed-domain sentences (medical+legal, technical+cultural) — ensure highest-scoring domain wins
2. Single-keyword detection (confidence 0.85 tier) — verify borderline cases route correctly
3. UserDomainHint override cases — explicit hint beats keyword evidence
4. Multi-topic sentences where only one domain keyword appears
5. Edge cases: code-switched sentences with topic keywords in non-source script

## What Batch-003 Should Cover

1. Near-miss sentences where a keyword appears in a non-target domain word (false positive stress test)
2. Rare domains in underrepresented directions (cultural HE_EN, personal HE_EN)
3. Hebrew topic sentences with less common domain vocabulary

## Do Not Proceed To

- Translator streams until R1 (3 batches, 150 pairs, complete) + R2 (3 batches, 150 pairs) are both accepted
- HE translator streams require Hebrew tokenizer gate pass first
