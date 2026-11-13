# TRANSLATION-CHECKPOINT-WRITER-GUARD-HOWTO

Purpose: practical checklist for issuing future TranslationDomain checkpoint or
training work orders after checkpoint contamination.

## 1. Architect Preflight

Before issuing a training writer, architect confirms:

```text
parent_session_role = ARCHITECT_ONLY
current_branch = current branch only; no branch switch
checkpoint_path = exact path
writer_agent_id = exact sub-agent id
sub_agent_model = gpt-5.5
reasoning_effort = xhigh
local alias = 5.5 Extra High / sonnet latest High when configured equivalent
source_file_or_corpus = exact path/name
MeetsPilotGate = true
```

If `MeetsPilotGate` is false or unknown, do not issue a writer for the active
checkpoint. Return source-gate evidence and schedule code enforcement as the
next repair task.

## 2. Work Order Bounds

One work order may authorize exactly one of:

```text
one code slice
one training chunk
```

The work order must name:

```text
allowed files
allowed commands
forbidden commands
checkpoint_path
writer_agent_id
source/cursor/range
max cycle or row boundary
heartbeat cadence
expected ACCURACY_SET fields
return packet fields
commit/push expectation
```

No sub-agent may chain a source rollover, runner restart, checkpoint write, or
next training chunk unless the work order names exact limits for that action.

## 3. Single-Writer Lock

Before training starts:

```text
checkpoint_path has no active writer
no run/build/training pipeline owns checkpoint_path
experiments use isolated checkpoint copies
active checkpoint write access is explicitly granted
```

During the chunk, only the assigned writer may write that checkpoint. If the
writer goes silent, architect may inspect logs/status read-only. Close or stop
the silent writer before assigning another writer to the same checkpoint.

## 4. Heartbeat

Long-running writers report every 10-15 minutes or after each 29-cycle chunk:

```text
checkpoint_path
writer_agent_id
source/cursor
last ACCURACY_SET
checkpoint_loaded
checkpoint_saved
next bounded action
```

No heartbeat means no second writer until architect performs read-only
inspection and closes/stops the first writer.

## 5. Mandatory 100-Set Output

Every 100-set must return:

```text
product
exact
bootstrap
train_rows
newPattern
seenTokens
promoted
promotionDecision
```

Promotion is allowed only when:

```text
newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
```

## 6. Recovery Mode

Contaminated checkpoints are not continuation sources.

Allowed recovery:

```text
rollback to a clean checkpoint
forensic replay of approved reports with explicit Luba approval
```

Forbidden recovery:

```text
resume from contaminated checkpoint
assign two writers to compare outcomes
write active checkpoint after failed or unknown source gate
```

## 7. Return Packet

The sub-agent returns:

```text
work_order_id
writer_agent_id
checkpoint_path
source gate evidence
commands actually run
files changed
ACCURACY_SET rows emitted
checkpoint_loaded
checkpoint_saved
promotionDecision
heartbeat log summary
commit hash and push status when allowed
remaining gaps
```

## Phase 38 — Complete Checkpoint Definition (Added 2026-06)

After Phase 38, a "complete" translation checkpoint requires more than a training run. The following must all be present:

### What constitutes a complete checkpoint (Phase 38+)

1. **Real KNN/trained state** — the checkpoint JSON must contain trained KNN (K-Nearest Neighbours) weights or equivalent learned representations. A checkpoint that only stores loop metadata (iteration count, last loss) is incomplete.
2. **manifest.json** — points to the checkpoint file with SHA-256 hash.
3. **runner-report.json** — contains numeric metrics: accuracy_percent, exact_percent, bootstrap_percent, new_tokens_percent, new_structures_percent, newPattern_percent, seenTokens_percent, train_rows, promotionDecision.
4. **corpus.jsonl.gz** — committed training corpus with no absolute private paths.
5. **training-manifest.json** — contains corpus_sha256 for reproducibility.

### Mandatory probes before promotion

Before any checkpoint is promoted to active:

1. **Fresh-load probe**: Run `import-translation-model --manifest <path>` on a clean session (no in-memory state from the training run). The model must load without error and produce the same metrics as the training report.

2. **Continue-training probe**: Run `continue-translation-training --from-corpus corpus.jsonl.gz`. This validates the corpus is committed, addressable, and produces a non-degraded training step. Using --private-pairs instead of the committed corpus fails the probe.

### Contaminated checkpoints

A checkpoint is contaminated if:
- It was written during a training run that had MeetsPilotGate=false or unknown
- It contains private pair data (not anonymised corpus)
- The fresh-load probe fails

Contaminated checkpoints are recovery artifacts. They must NOT be used as continuation points. Roll back to the last clean checkpoint or obtain a Luba-approved forensic replay.
