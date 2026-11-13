# QUALITY-03 runner-ready input/ref acquire-or-defer - no scoring

## Цель

Acquire, bind, or explicitly defer current row-specific Quality-03 input refs for the 9 TOKEN-PAIR rows: approved non-private/redacted baseline/adaptive refs, approved eval input manifest, reviewer notes/adjudication/regression guard, and source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization controls.

## Allowed scope

- New timestamped folder under artifacts/extension-runtime-token-quality-learnability/quality-03-runner-ready-input-ref-acquire-or-defer-no-scoring/
- Minimal state entry in docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json

## Forbidden

- git commands
- dotnet test
- external AI/web/model calls
- host runtime launch
- proof runner execution
- quality scorer execution
- raw-private content reads
- training, DPO, checkpoint/model/locator/target mutation
- code changes
- token savings, quality improvement, runner-ready, or quality-pass claims from manifest presence

## Gate

Only if all 63/63 blocking proof refs PASS and the broader current runner-ready input fields are exact PASS may a later separate work order emit runner-ready eval inputs. Otherwise keep runner-ready 0/9.

Parent will perform commit/push after review. Git authority for the sub-agent: forbidden.
