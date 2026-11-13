# 10 - Simulation Check

## Test Input

"Use this document to understand Root agent instruction file loaded before every Claude Code session. Establishes 7 absolute operational rules OP-1 through OP-7 : never override user instructio and keep its main responsibilities visible."

## Expected Node Behavior

1. Node 1 emits an array that includes source inventory, metadata and tags, retrieval routing, graph relationships.
2. Node 2 translates each selected domain separately.
3. Node 3 creates separate work pieces with ordering, parallel work, waits, repair, and review.
4. Node 4 processes each work piece independently.
5. Node 5 turns each processed piece into a concrete result.
6. Node 6 translates each formal result into readable language while checking the full result collection.
7. Node 7 selects reviews before formatting.
8. Node 8 passes safe results or sends only the failed fragment back.
9. Node 9 formats only approved translated fragments.

## Missing-Content Tests

If source inventory is missing, revisit Node 1 and Node 2.
If metadata and tags and retrieval routing are collapsed into one task, revisit Node 3.
If no waiting or repair path appears, revisit Node 4 and Node 5.
If review happens after formatting, revisit Node 7 and Node 8.
If the final answer loses the source meaning of Root agent instruction file loaded before every Claude Code session. Establishes 7 absolute operational rules OP-1 through OP-7 : never override user instructio, revisit Node 9.
