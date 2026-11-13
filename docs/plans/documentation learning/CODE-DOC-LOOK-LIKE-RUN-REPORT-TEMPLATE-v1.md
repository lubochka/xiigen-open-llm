# Code Documentation Look-Like Run Report Template
# Version: 1.0 - 2026-05-11

---

## Report Header

```text
Run ID:
Date:
Executor:
Stream:
Repository source:
Commit/tag:
License state:
Documentation level:
Stack label:
Batch size:
Held-out size:
Batch state: PAIRSET_READY_TRAINING_PENDING | TRAINED_EVALUATION_PENDING | ACCEPT_BATCH | REPAIR_BATCH | REJECT_BATCH
Metric mode: actual | projected | not_run
Decision:
```

---

## Resource Metrics

```text
Elapsed time:
CPU average:
CPU peak:
RAM average:
RAM peak:
Disk read:
Disk write:
GPU/accelerator:
```

---

## Training Metrics

```text
Training executed: yes | no
Training loss:
DPO accuracy:
Reward margin:
Baseline compared:
Regression:
If not executed, blocker:
```

---

## Documentation Metrics

```text
Held-out inference executed: yes | no
Factual accuracy:
Member/API coverage:
Side-effect recall:
Error-path recall:
Call-edge preservation:
Framework lifecycle accuracy:
Invented behavior count:
Missing side-effect count:
Missing error-path count:
Wrong framework count:
Generic name-echo count:
Metric note:
```

Projected metrics must be labelled `projected`. They cannot be used as
acceptance evidence.

---

## Sample Outputs

Include five held-out samples:

1. best output
2. acceptable output
3. borderline output
4. failed output
5. most informative rejected-side failure

For each:

```text
File:
Symbol:
Expected documentation:
Model documentation:
Score:
Failure notes:
```

---

## Next Batch Decision

```text
Decision:
Reason:
Next stream:
Next level:
Repair instructions:
Runtime blocker:
```
