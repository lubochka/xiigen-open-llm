# RUNTIME-EVIDENCE-CLASSIFICATION-NORMALIZATION-01

Дата: 2026-06-18 18:41:48 +03:00
Ветка: `codex/chat-gpt-5.5`
Режим: no-eval, no external AI/model calls, no source changes, no git writes.

## Доска

| Поле | Состояние |
|---|---|
| Всего | 39 runtime/host/token evidence families по Claude, Cursor, Codex и shared API/token пакетам |
| Сделано | Все существующие runtime evidence families нормализованы в 7 evidence classes |
| Сейчас | Самый сильный текущий уровень для всех трех хостов - `narrow_live_host_surface`; `full_live_ui_chat_app_proof` = 0 |
| Осталось | Следующий честный шаг: `RUNTIME-CLAUDE-FULL-CHAT-UI-NO-EVAL-PROOF-01` |

## Что закрыто

- Закрыта классификация старых Claude/Cursor/Codex runtime artifacts: static maps, handler tests, CLI helpers, API/core traces, narrow live host surfaces, missing/full proof gaps.
- Закрыта защита от главной ошибки: `runtimeEvidence=true`, `runtimeProofAcceptedCandidate=true`, `extension_host_started=true`, `experimental_app_server_chat_message_surface_proven=true` и runtime evidence refs больше не считаются полным host readiness без класса `full_live_ui_chat_app_proof`.
- Закрыт per-host strongest evidence summary:
  - Claude: narrow Claude Code SessionStart/plugin-hook path.
  - Cursor: narrow Cursor Extension Host command/dev-load path.
  - Codex: narrow Codex app-server/debug send-message-v2 or installed-cache/plugin-entrypoint path.
- Claim locks остались закрыты: token savings, quality improvement, semantic quality, learnability, training, target promotion, final/product readiness не заявлены.

## Что НЕ закрыто

- Claude: full Claude Desktop/chat UI journey не доказан.
- Cursor: full Cursor chat participant UI/panel UI journey не доказан.
- Codex: full Codex desktop chat/app UI proof не доказан; app-server/debug path не равен desktop UI proof.
- Token savings, quality improvement, semantic quality, learnability, DPO/training, checkpoint/locator/target promotion и final/product readiness остаются закрыты.

## Счёт по evidence class

| Evidence class | Count |
|---|---:|
| `static_code_map` | 3 |
| `handler_level_test` | 9 |
| `cli_helper` | 4 |
| `dashboard_api_core` | 5 |
| `narrow_live_host_surface` | 15 |
| `full_live_ui_chat_app_proof` | 0 |
| `missing_or_not_applicable` | 3 |
| **Всего** | **39** |

## Сильнейшее доказательство по хостам

| Host | Strongest evidence | Что доказывает | Чего не доказывает |
|---|---|---|---|
| Claude | `narrow_live_host_surface` | Claude Code SessionStart/plugin-hook smoke reaches llm_mvp_core and produces request/response/trace/visible hook evidence | Full Claude Desktop/chat UI readiness |
| Cursor | `narrow_live_host_surface` | Cursor Extension Host command/dev-load path reaches adapter/core with target-locator/debug evidence | Full Cursor chat participant UI and panel/UI journey |
| Codex | `narrow_live_host_surface` | Codex app-server/debug or installed-cache plugin-entrypoint can execute adapter/core boundary for redacted smoke | Full Codex desktop chat/app UI proof |

## Flagged overclaim risks

1. `claude-runtime-qa-report.json`: `runtimeProofAcceptedCandidate=true` means named SessionStart hook only, not full Claude UI.
2. `cursor-runtime-proof-rerun.json` and `cursor-runtime-qa-report.json`: `runtimeProofAcceptedCandidate=true` means Cursor Extension Host command only, not chat participant UI.
3. `host-impl-cursor-01-report.json`: `extension_host_started=true` and `extension_activated=true` are not full UI readiness.
4. `codex-runtime-qa-report.json`: `codex_exec_model_backed_runtime=true` is Codex exec/source-entrypoint evidence, not full plugin/chat UI.
5. `codex-runtime-proof-04f.json` and `codex-runtime-proof-04g.json`: app-server/chat message surface proof is narrow/debug/app-server evidence, not desktop UI proof.
6. `token-03-baseline-traces.jsonl`: 54 rows reference narrow runtime evidence, but their own class is handler/core baseline.
7. `token-04-adaptive-traces.jsonl`: 54 rows reference narrow runtime evidence, but their own live scope says no live host UI/chat capture.
8. `quality-semantic-gates-20-host-evidence-rows.jsonl`: 12 rows are `HANDLER_CLI` or `EXTENSION_HOST` with `NOT_*` live UI/chat status.
9. `parent-review-HOST-IMPL-02.md`: historical reviewer text mentions `RuntimeEvidence=true` derived from `Success`; this remains a quarantined overclaim risk.
10. QSG21 Claude/Codex packets used external AI in their historical runtime probe scope; this classification did not call external AI and does not convert those packets into token/quality/semantic proof.

## Следующее действие

Classification is complete. Next action in state should be:

```text
RUNTIME-CLAUDE-FULL-CHAT-UI-NO-EVAL-PROOF-01
```

Этот следующий work order должен prove or honestly gap full Claude chat/UI proof without eval, without external AI/model calls, without token/quality/learnability claims, and without mutating training/checkpoint/locator/target assets.
