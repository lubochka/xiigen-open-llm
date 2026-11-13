# S23 whole-PC budget, concurrency and cost protocol

## Device binding, not an assumed RTX profile
Before any resource-based decision record actual machine_id/OS/arch, GPU/driver/backend, CPU threads, installed/free RAM, total/free VRAM, process RSS, disk free/temp space, model dtype/quantization, context length, batch, candidate fanout, max branches, local inference load, parent/reference/trial loader and owner permissions. Historical RTX 3050/8GB references or adapter sizes are not an observed footprint. Numeric limit and threshold must be owner chosen BEFORE measurement; missing values = UNKNOWN, not 0.

## P0–P8 resource ledger for one pair
| Phase | Function | Concurrent resources that must be observed | Mutation |
|---|---|---|---|
| P0 | preflight/admission | process baseline; model/base/reference/tokenizer; disk rollback reserve | no |
| P1 | frozen parent heldout | inference, index, graph, caches, evaluator | no |
| P2 | CODEX chooser + Chinese both sides + separate exact CODEX judge | external tokens/calls/latency + on-PC traces | pair persisted only if granted |
| P3 | checkpoint reserve | parent/trial weights, adapter, optimizer, scheduler, RNG, journal, snapshots & temp duplicate | durable pre-Step |
| P4 | exactly one optimizer step | model/reference/trial gradients/activation/KV/optimizer; CPU/GPU overlap; idle gate | one eligible Step |
| P5 | independently blind evaluation | heldout/inference load, graph/kNN and trial | no further gradient |
| P6 | accept/revert | immutable snapshots, pointer CAS/readback, cache invalidation | atomically controlled |
| P7 | cold ordinary user call | actual installed candidate/accepted pointer, index/tokenizer/graph | no training |
| P8 | new user feedback | capture/quarantine/index/update queue | no synchronous gradient |

Store wall time and stage times, CPU user and peak RSS, GPU VRAM observed vs reserved, disk cumulative/peak temporary, all successful/failed API calls and their token/money cost, all async launched and canceled branches, index/cache reload costs, energy ONLY IF METERED, p50/p95 with N where meaningful. Record WARM/COLD, machine/load/time confounders. For sequential phases, peak is max of measured simultaneous residency, not an invented sum or just LoRA filesize. Include failed attempts in denominator and consumption.

## Resource admission
Do not start mutation without reserve for accepted parent+trial+optimizer/RNG+journal+rollback+temporary atomic rename/migration copy, and buffer for concurrent ordinary requests. For each quantity: observed capacity, predicted worst case with source, safety margin selected before data, and stop/fallback. Automatic retries have bounded attempt count and overall wall/remote budgets; growing Graph fanout does not gain free compute. If disk or VRAM unknown, first do read-only measurement; HOLD the affected training lane but not independent research/E1 elsewhere when safe.

## Growth without losing ability to resume
Project quota for DPO data, pair ledger, old-version weights/LoRA and optimizer, tokenizer and vector/codebook, kNN/ANN/BM25 indices, DynamicGraph state and caches, trial+rollback and source provenance. Report bytes added per truly new bulk, compaction/reindex double-copy, retention by subject and compatibility. Protect accepted parent, audit journal, repeat-prevention identities, rollback until release policy; no destructive eviction in S23 planning. Local canonical artifacts belong in K/codex/dpo-implementation, not P. Remote LFS qualification remains historical S05 owner and must be separately verified.
