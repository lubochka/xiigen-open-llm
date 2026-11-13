# E51A Tier 2 Coverage-First Plan v1

## Purpose

E51A Tier 1 now passes all gates, but the evidence shows three thin areas that should be cleaned before adding more files.

The goal of Tier 2 is not to add examples blindly.

The goal is to improve coverage first, then scale.

## Locked rule

Tier promotion requires coverage verification, not only metric passage.

This is recorded as D-13 in `docs/DECISIONS-LOCKED.md`.

## Current Tier 1 status

Focused E51 tests passed: `15/15`.

Full solution tests passed: `892/892`.

Latest E51A evidence:

`experiments-v2/E51A-CODE-CONTEXT-PIPELINE/results/run-2026-05-04-073844.json`

Tier 1 metrics:

1. baseline predicate overlap: `0.283`,
2. enhanced predicate overlap: `0.967`,
3. domain accuracy: `100%`,
4. trace success: `100%`,
5. failures: none.

## Risk 1: V07 architecture density

Nature:

Training density problem.

Current behavior:

`V07` routes correctly, but it depends heavily on one architecture neighbor.

Required Tier 2 fix:

Add two or three more `ARCHITECTURE` training examples with distinct predicate profiles.

Required profiles:

1. interface-heavy: `DECLARE`, `RETURN`,
2. structural-planning-heavy: `CREATE`, `ITERATE`,
3. constraint-enforcement-heavy: `BRANCH`, `FAIL`.

Expected result:

`V07` should have a real architecture neighborhood, not a single-point attractor.

## Risk 2: V03, V04, V09 shared loop signature

Nature:

Code context construction problem.

Current behavior:

`V03`, `V04`, and `V09` share the same loop-shaped predicate signature even though they belong to different prompt intents.

Required Tier 2 fix:

Make their code contexts produce distinct predicate profiles.

Required context shapes:

1. bug-fix loop context: loop plus `NULL_GUARD` or `FAIL` inside the iteration,
2. question-explanation loop context: read-only call/return shape with minimal mutation,
3. orchestration-planning context: multi-step setup with `DECLARE` and `ASSIGN` across stages.

Expected result:

The three examples should no longer land in the same nearest-neighbor cluster.

## Risk 3: V10 IEquatable intent

Nature:

Specific code context mismatch.

Current behavior:

`V10` passes, but the context leans too much toward null-check/equality failure rather than interface implementation.

Required Tier 2 fix:

Swap the context to an actual `Equals` plus `GetHashCode` implementation.

Required signature:

1. explicit type check,
2. null handling as secondary,
3. clear `BRANCH` and `RETURN` structure,
4. implementation-focused shape rather than failure-focused shape.

Expected result:

`V10` should pass with a stronger overlap and a cleaner implementation-signature neighborhood.

## Required sequence

Do this in order.

1. Fix the three existing context risks.
2. Run focused E51 tests.
3. Regenerate E51A evidence.
4. Verify trace success remains `100%`.
5. Verify V07, V03/V04/V09, and V10 have improved neighborhood evidence using the concrete checks below.
6. Only then expand Tier 2 to `02.DatasetPipeline` and `05.Embeddings`.

## Why sequence matters

Adding new examples before fixing the existing contexts can hide the problem.

The next examples might make the metrics better while leaving the old weak contexts unchanged.

The correct proof is:

1. the existing Tier 1 examples are clean,
2. the known thin spots are resolved,
3. the new Tier 2 examples expand coverage rather than masking old weaknesses.

## Tier 2 expansion target

After the fixes, increase E51A to:

1. `30` training examples,
2. `10` or more held-out test examples,
3. source coverage from `01.Tokenizer`, `02.DatasetPipeline`, and `05.Embeddings`.

The Tier 2 expansion should intentionally add examples for:

1. architecture,
2. bug-fix diagnostic repair,
3. read-only code questions,
4. orchestration sequences,
5. code changes with delete/remove intent,
6. implementation interfaces.

## Promotion gate

Tier 2 is ready only when:

1. E51A focused tests pass,
2. full solution tests pass,
3. E51A evidence shows `100%` trace success or a named, accepted thin-case list,
4. architecture examples have more than one strong neighbor,
5. bug/question/orchestration loop examples no longer share one identical neighborhood,
6. V10 has a stronger implementation-shaped context.

## Concrete coverage checks

The Tier 2 session must verify these checks directly from the E51A evidence JSON before expanding the dataset.

### V07 architecture density check

Current risk pattern:

`T06:1.0`, then weak secondary neighbors around `0.25`.

Required improvement:

`V07` must have at least two nearest neighbors with score `>= 0.60`.

Why:

One strong architecture neighbor is a single-point attractor. Two or more strong neighbors means the architecture case has a real local neighborhood.

Evidence check:

`V07.nearest_neighbors.Count(score >= 0.60) >= 2`

### V03/V04/V09 distinct loop-neighborhood check

Current risk pattern:

`V03`, `V04`, and `V09` share the same nearest-neighbor set:

`T02:1`, `T04:1`, `T08:1`.

Required improvement:

The top-neighbor signatures for `V03`, `V04`, and `V09` must not all be identical.

Preferred improvement:

Each record should have a first neighbor that matches its intended domain:

1. `V03` should be closest to a bug-fix loop context,
2. `V04` should be closest to a read-only question/explanation loop context,
3. `V09` should be closest to an orchestration/planning loop context.

Evidence check:

`Distinct(top3_neighbor_id_sequence for V03, V04, V09).Count > 1`

Stronger evidence check:

`V03.nearest_neighbors[0]`, `V04.nearest_neighbors[0]`, and `V09.nearest_neighbors[0]` should not all be the same id.

### V10 implementation-context check

Current risk pattern:

`V10` passes with overlap `0.667`, but the produced predicate shape is `NULL_GUARD`, `FAIL`, `BRANCH`.

Required improvement:

`V10` predicate overlap should reach `1.0`.

The produced predicates should include at least one implementation-shape predicate:

1. `RETURN`, or
2. `DECLARE`.

Evidence check:

`V10.predicate_overlap == 1.0`

and

`V10.produced_predicates contains RETURN or DECLARE`

Why:

The `IEquatable<T>` case should be represented as implementation structure, not only null/fail equality handling.
