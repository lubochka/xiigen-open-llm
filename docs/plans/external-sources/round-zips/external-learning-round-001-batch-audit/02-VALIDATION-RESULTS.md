# Validation Results — External Learning Round 001

## Counts

- Batch files examined: 75
- Batch row IDs found: 1500
- Unique batch row IDs: 1500
- Duplicate row ID groups: 0
- Coverage ledger rows: 1500
- Fenced JSON blocks: 1324
- Fenced JSON blocks parsed cleanly: 1218
- Fenced JSON parse failures: 106
- Files with no JSON blocks: 9
- Files with JSON parse failures: 7

## Plan-format checks

- Final Pair format entries found: 0
- Rephrasing pairs found: 0
- Required output folder shape found: no
- Required per-node files found: no
- Required simulation checks found: no

## License gate

- License values scanned: 1500
- Clear by simple plan match: 954
- Hold or policy decision needed: 542
- Blocked by GPL string: 2

## Identifier/provenance checks

- Batch IDs not in ledger before normalization: 400
- Ledger IDs not in batch before normalization: 400
- Batch IDs not in ledger after suffix normalization: 160
- Ledger IDs not in batch after suffix normalization: 160

## Result

These batches should remain in candidate/preprocessed status. They must not be imported into local learning sets until the next round normalizes schema, repairs invalid records, applies the license gate, and writes final node A-to-B pairs with rephrasing.
