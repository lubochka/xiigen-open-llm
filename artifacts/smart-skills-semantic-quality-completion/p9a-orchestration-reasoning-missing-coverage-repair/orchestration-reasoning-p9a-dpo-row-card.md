# P9A DPO row card

## P9A-ORCH-TRAIN-001

Teaches: diagnose a verification failure, choose one bounded repair strategy, and preserve unaffected work.

Chosen: compare expected and observed evidence first, then choose a bounded repair branch.

Rejected: repeat the whole plan or modify neighboring work without diagnosing the failure cause.

Leakage controls: exact prompt, filename/path/title, source family, row id, nearest-only, repair label only.

## P9A-ORCH-TRAIN-002

Teaches: keep scope or flow state isolated across independent contexts.

Chosen: check scope boundaries, filter reads, and block state transfer across independent flows.

Rejected: merge all records found by a shared flow marker and treat them as one evidence pool.

Leakage controls: exact prompt, filename/path/title, source family, row id, nearest-only, scope marker only.
