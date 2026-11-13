# WDA-01 Confidence And Wrong-Domain Guard Checklist
# Document to sub-phase classification
# Round A - Guard standard
# Date: 2026-05-04

---

## Purpose

WDA-01 must prove not only that every document can be assigned to a sub-phase,
but that the assignment is confident and not merely the best weak match in the
wrong pool.

This checklist carries forward the wrong-domain retrieval lesson from E51A and
the knowledge-edge confidence lesson from E51B.

---

## Confidence Rules

### Correct-domain confidence floor

Every ready-source probe must produce:

```text
confidence >= 0.70
```

If confidence is below 0.70, the classification may still be directionally
right, but the sub-phase is considered thin. It needs one original plus one
phrasing B pair for `sub-phase-router`.

### Wrong-domain confidence ceiling

Every probe must report its nearest wrong-domain score.

Required:

```text
nearest_wrong_sub_phase_score < 0.50
```

If any wrong-domain score reaches 0.50 or higher, the guard fires. The system
is too close to confidently retrieving from the wrong sub-phase.

### Source knowledge-edge handling

If no ready source document exists, the probe must say:

```text
source_status = knowledge_edge
knowledge_edge = true
```

The system may still classify the hand-authored packet, but the broad corpus
cannot count the packet as a real source.

---

## Required Per-Probe Checks

For each of 31 probes, verify:

1. source status is `ready` or `knowledge_edge`
2. predicted phase id equals oracle phase id
3. predicted sub-phase id equals oracle sub-phase id
4. top-three predicate profile overlaps oracle by at least 2/3
5. confidence is >= 0.70 when source status is `ready`
6. nearest wrong sub-phase score is < 0.50
7. missing real source is named as a knowledge edge

---

## Aggregate Pass Gates

| Gate | Required value |
|---|---|
| probes_checked | 31 |
| ready_source_wrong_phase_count | 0 |
| ready_source_wrong_sub_phase_count | 0 |
| ready_source_below_confidence_count | 0 |
| wrong_domain_high_confidence_count | 0 |
| missing_source_without_knowledge_edge_count | 0 |

---

## Failure Meaning

### Wrong phase

The phase-level predicate profile is not distinct enough. Add training pairs
that separate neighboring lifecycle phases.

### Right phase but wrong sub-phase

The sub-phase router lacks local structure. Add training pairs for the specific
neighboring sub-phases that confused the classifier.

### Low confidence

The source vocabulary is thin or the predicate profile is too generic. Add
one original and one phrasing B pair before broad corpus intake for that
sub-phase.

### High wrong-domain score

The wrong-domain guard has caught boundary bleed. Add a negative or contrastive
pair that shows why the wrong sub-phase is not correct.

### Missing source not treated as knowledge edge

The source authority layer is unsafe. Fix the source ledger and formatter
before adding training pairs.

---

## Human Review Requirement

The human-readable result must list:

1. all 31 probes
2. ready source count
3. source knowledge-edge count
4. any low-confidence sub-phase
5. any wrong-domain guard firing
6. exact next corpus action per failing probe

If the machine trace passes but the human result hides a low-confidence or
missing-source issue, WDA-01 fails.
