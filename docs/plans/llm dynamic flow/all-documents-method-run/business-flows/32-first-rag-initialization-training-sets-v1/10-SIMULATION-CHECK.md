# 10 - Simulation Check

## Purpose

This file checks whether the nine node files can reproduce the intended first retrieval initialization result without borrowing logic from another document.

## Test Input

A human asks:

"Launch retrieval on day one using existing skills, task types, and source documents."

## Expected Node Behavior

1. Node 1 should emit several domain labels, not one label. Expected coverage includes: source and skill inventory, metadata schema, task type to source mapping.
2. Node 2 should translate each selected domain into precise domain wording, one domain at a time.
3. Node 3 should create work pieces that preserve order where needed and parallel work where possible.
4. Node 4 should process each work piece separately and expose success, waiting, and failure states.
5. Node 5 should produce concrete outcomes for each processed piece.
6. Node 6 should translate formal results into readable language while checking the full result collection for contradictions.
7. Node 7 should select review units from formal results before formatting.
8. Node 8 should pass safe fragments and send only broken fragments back for reprocessing.
9. Node 9 should combine approved translated fragments into one answer for the chosen audience.

## Missing-Content Tests

If the simulated output omits source and skill inventory, revisit Node 1 and Node 2.
If the output flattens metadata schema and task type to source mapping into one undivided task, revisit Node 3.
If it has no waiting state or recovery route, revisit Node 4 and Node 5.
If the user-facing answer exposes internal formal wording, revisit Node 6 and Node 9.
If risky, private, contradictory, or unsupported results reach the final answer, revisit Node 7 and Node 8.

## Success Standard

The simulation succeeds only when the final answer preserves the document's first retrieval initialization meaning, keeps all active branches visible, keeps review before formatting, and does not collapse array nodes into single units.
