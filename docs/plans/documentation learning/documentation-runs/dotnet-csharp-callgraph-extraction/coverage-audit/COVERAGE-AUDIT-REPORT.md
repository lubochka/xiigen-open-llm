# Code Documentation Inventory Coverage Audit

Generated: 2026-05-14T07:58:33.3484101+03:00

## Totals

- Methods: 444 / 444 covered
- Uncovered methods: 0
- Method coverage ratio: 1
- Edges: 430 / 430 covered
- Uncovered edges: 0
- Edge coverage ratio: 1

## Why Things Remain Uncovered

The remaining gaps are not one thing. They split into:

- leaf methods with no local call edges that have not yet been sampled;
- single-edge callers needing more positive transit sampling;
- multi-edge callers needing more complete/incomplete transit pairs;
- framework lifecycle edges that need more thin-project sampling;
- ASP.NET 4.x/WebForms lifecycle requiring more external MIT/Apache authority.

See UNCOVERED-METHODS.jsonl and UNCOVERED-EDGES.jsonl for exact symbols.
