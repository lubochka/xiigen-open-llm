# Code Documentation Depth Coverage Audit

Generated: 2026-05-14T07:10:06.4725954+03:00

## Method Depth

- Methods total: 387
- Methods missing positive-ready examples: 75
- Methods missing negative gate examples: 0
- Methods missing D4 caller/workflow examples: 198

## Edge Depth

- Edges total: 376
- Edges missing positive-ready propagation examples: 0
- Edges missing missing-callee negative examples: 40

## Interpretation

This audit is stricter than inventory coverage. Inventory coverage only asks
whether a method or edge appeared at least once. Depth coverage asks whether it
has enough positive, negative, and caller/workflow examples to be robust.

See METHOD-DEPTH-COVERAGE.jsonl and EDGE-DEPTH-COVERAGE.jsonl for exact
symbols and gap reasons.
