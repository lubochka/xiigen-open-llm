# QUALITY-03 runner-ready input/ref repair - no scoring

## Цель

Закрыть именно недостающие Quality-03 input refs для 9 rows: row-specific approved non-private/redacted baseline/adaptive refs, approved eval manifest, held-out/source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization controls, reviewer notes and regression guard. Scorer запускать нельзя, пока все refs не будут доказаны.

## Разрешённый scope

- new timestamped folder under artifacts/extension-runtime-token-quality-learnability/quality-03-runner-ready-input-ref-repair-no-scoring/
- docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json

## Запрещено

- git commands
- dotnet test
- external AI/web/model calls
- host runtime launch
- proof runner execution
- quality scorer execution
- raw-private content reads
- training/DPO/checkpoint/model/locator/target mutation
- token savings or quality pass claims from manifest presence

## Gate

Only if all 63/63 blocking refs PASS may a later separate work order emit runner-ready eval inputs. This proposal itself keeps runner-ready 0/9.

Parent will perform commit/push after review. Git authority for the sub-agent: forbidden.
