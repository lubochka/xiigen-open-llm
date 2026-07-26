# LearningLocalWikiData

This subproject stores project-local wiki learning batches, runner outputs,
resource reports, held-out evaluation artifacts, checkpoint references,
manifests, and the training asset registry used by `42.ProjectWikiDomain`.

It is data, not runtime logic.

Runtime logic remains in:

```text
C:\Projects\llm_mvp_core\42.ProjectWikiDomain
```

Large checkpoint files are tracked through Git Large File Storage when they are
kept in git. Model weights that live outside git must be referenced through
manifest files and `IModelRepository` keys.

## Artifact Layout

```text
43.LearningLocalWikiData/
  registry/          # schema, registry, blocking state, migration report
  reports/           # durable cleanup reports, next-action reports, and summaries
  manifests/         # accepted unit manifests and checkpoint references
  artifacts/
    runner-metrics/  # training batches, runner reports, checkpoints
    phase-metrics/   # CPU/RAM/time and project validation reports
    base-wiki-runs/  # base wiki model run inputs and reports
```

## Ownership Rule

Generic wiki capability learning data belongs here.

Project-specific runtime wiki evidence belongs in the target project:

```text
<project>/.xiigen/wiki-domain/
```

The target project may store domain manifests, evidence indexes, accepted local
documentation, local adapter manifests, and local cache references. It must not
become the home for generic wiki training logic or generic batches.

## Report Rule

No user-facing training metric should be reported unless a durable report
artifact exists under this subproject or another explicitly named runner output
path.

If CPU, RAM, accuracy, loss, reward margin, or held-out metrics are missing from
the artifact, the metric is `UNKNOWN`.

## Continuation Rule

New training is blocked until `registry/TRAINING-CONTINUATION-BLOCK.json` says
that registry validation passed. Rejected checkpoints may never be selected as
continuation sources.

After registry validation, the next unit is selected from registry weak signals
and written to `reports/PROJECT-WIKI-NEXT-TRAINING-ACTION.json`. That report is
planning only: it must say `training_started: false` and `new_batches_created: 0`.
