# 10 - Simulation Check

## Test Input

"Use this document to understand XIIGen V30X — Skills Factory & the retrieval system Quick Reference and keep its main responsibilities visible."

## Expected Node Behavior

1. Node 1 emits an array that includes user intent, creation evidence, decision trail, version change.
2. Node 2 translates each selected domain separately.
3. Node 3 creates separate work pieces with ordering, parallel work, waits, repair, and review.
4. Node 4 processes each work piece independently.
5. Node 5 turns each processed piece into a concrete result.
6. Node 6 translates each formal result while checking the full result collection.
7. Node 7 selects reviews before formatting.
8. Node 8 passes safe results or sends only the failed fragment back.
9. Node 9 formats only approved translated fragments.

## Failure Signs

- The output treats the source document line as the runtime input.
- The output collapses all domains into one task.
- Review happens after formatting.
- A failed review cannot send a fragment back for repair.
- The final answer loses the source meaning of XIIGen V30X — Skills Factory & the retrieval system Quick Reference.
