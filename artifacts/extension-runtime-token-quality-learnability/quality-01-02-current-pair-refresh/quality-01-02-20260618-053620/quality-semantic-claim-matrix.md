# Quality semantic claim matrix

Run: `quality-01-02-20260618-053620`

| Claim | Allowed | Status | Reason |
|---|---:|---|---|
| `token_savings` | `false` | `CLOSED` | QUALITY-03 не должен открывать token savings без same-pair quality non-regression PASS; текущий scorer limited/inconclusive. |
| `quality_improvement` | `false` | `CLOSED` | Adaptive rows can be scored, but source-disjoint and controls are not proven. |
| `semantic_quality` | `false` | `CLOSED` | Deterministic helper scoring is not semantic authority. |
| `learnability` | `false` | `CLOSED` | No DPO/training/fresh-load/held-out/rollback chain was run. |
| `live_host_savings` | `false` | `CLOSED` | TOKEN-03/TOKEN-04 are handler-level captures, not live Claude/Cursor/Codex UI/chat send-message pairs. |
| `final_readiness` | `false` | `CLOSED` | This phase produces QUALITY-01/02 inputs only; final closure rows remain outside scope. |
| `QSG28 target promotion` | `false` | `CLOSED` | QSG28 target is staged/test-only; no production locator/checkpoint mutation. |
| `OrchestrationReasoning adaptive learnability` | `false` | `CLOSED` | OrchestrationReasoning user/staged asset was not loaded in TOKEN-04 and used common fallback. |

Все readiness/semantic/token/learning claims остаются closed. Это не финальный readiness report.
