# TOKEN-01: настройка парного учета токенов baseline/adaptive

Run id: `token-01-20260618-024009`
Фаза: `TOKEN-01`
Статус: только настройка. Эта фаза создает матрицу парного учета токенов и правила сопоставления baseline/adaptive, но не доказывает экономию токенов.

## Что такое token pairing в этом проекте

Token pairing здесь означает, что baseline-запуск и adaptive-запуск можно сравнивать только как одну строгую пару. Для такой пары должны совпадать host, runtime/surface, `scenario_id`, prompt hash, метод, debug setting, evidence class/live scope, target locator mode, tokenizer/accounting version, redaction policy и набор raw/private и redacted references. Различаться должна только явно разрешенная переменная baseline/adaptive: baseline идет как common/no-adaptive, adaptive идет как user-local target с точным checkpoint/overlay reference.

Идентификатор `pair_id` фиксирует эту связку. Даже когда все ссылки baseline/adaptive появятся, token delta можно оценивать только после same-pair quality non-regression gate. В текущем TOKEN-01 созданы setup, refs и accounting frame, но валидных paired runs еще нет.

## Покрытие матрицы

Матрица `token-01-pairing-matrix.json` содержит 54 строки для 12 сценариев из `scenario-pack-token-quality-v1.jsonl`.

| Измерение | Покрытие |
|---|---|
| Hosts | Claude, Cursor, Codex |
| Строк на host | 18 для Claude, 18 для Cursor, 18 для Codex |
| Methods | `prompt-to-task`, `orchestration-plan`, `skill-selection` |
| Строк на method | 18 для каждого method |
| Verdict | `NO_VALID_PAIRED_TOKEN_RUNS_YET` |
| Accounting version | `xiigen-heuristic-word-punctuation/v1/heuristic-estimate` |
| Scope | setup only; no host runs; no external AI; no training; no checkpoint mutation; no token savings claim |

Host scopes из accepted runtime/core-boundary evidence:

| Host | Accepted scope evidence | Можно заявлять live host savings сейчас? | Причина |
|---|---|---:|---|
| Claude | Claude Code plugin/session hook; full Claude Desktop/chat UI не доказан в accepted context | Нет | Нужны свежие TOKEN-03 baseline и TOKEN-04 adaptive refs на том же Claude scope, затем same-pair quality non-regression. |
| Cursor | Cursor.exe Extension Host command/dev-load path; full Cursor chat participant UI не доказан в accepted context | Нет | Нужны свежие TOKEN-03/TOKEN-04 refs на том же Cursor Extension Host scope; helper/dashboard/API refs не заменяют host pair. |
| Codex | Installed-cache plugin entrypoint/core-boundary; прямой `codex.exe` из shell получил Access denied; full Codex chat/app send-message не доказан | Нет | Scope не является full live chat/app class; для live host savings нужны baseline/adaptive пары именно на Codex app/plugin/chat surface. |

## Какие refs нужны для каждой пары

Для каждого `pair_id` матрица требует:

- baseline common/no-adaptive: свежий TOKEN-03 trace с `model_mode=common_baseline`, `adaptive_enabled=false`, без чтения user-local checkpoint, на том же host/scope/method/prompt/debug/tokenizer/redaction.
- adaptive/user-local target: свежий TOKEN-04 trace с `adaptive_enabled=true` и точным `user_local_checkpoint_id` или `target_checkpoint_id`, на том же host/scope/method/prompt/debug/tokenizer/redaction.
- raw private trace refs: private input/output refs для baseline и adaptive, без копирования private content в shared report.
- redacted trace refs: redacted input/output/debug/core request/response refs, пригодные для review.
- token accounting report ref для того же `pair_id`.
- quality non-regression ref: QUALITY-03 same-pair PASS для того же `pair_id` до любой заявки на экономию.

Исторические файлы `baseline-token-traces.*`, `adaptive-token-traces.*` и `quality-comparison-report.*` существуют в artifact root и записаны только как historical candidate refs. Они не закрывают этот свежий TOKEN-01, потому что текущий work order требует fresh setup, а historical adaptive validation говорит, что user adaptive checkpoint не был загружен.

## Почему handler/API/dashboard/helper traces не доказывают live host token savings

Handler/API/dashboard/helper traces полезны для проверки schema, accounting, privacy и core-boundary diagnostics. Но они не доказывают live savings для Claude, Cursor или Codex, потому что host surface является частью пары. Baseline через dashboard/API/helper и adaptive через host runtime относятся к разным evidence classes, поэтому сравнение получает статус `INVALID_PAIR_DIFFERENT_EVIDENCE_CLASS`.

Из-за этого такие traces можно хранить как diagnostics или non-live evidence, но нельзя использовать как доказательство live host token savings.

## Почему сначала нужна quality non-regression

Сокращение токенов само по себе не является успехом. Более короткий adaptive output может быть хуже: он может пропустить обязательные gates, скрыть debug evidence, не описать rollback, пропустить source/license review или выбрать skill deterministic shortcut вместо корректной reasoning path.

Поэтому для того же `pair_id` сначала нужен same-pair quality non-regression PASS по quality rubric, включая PromptToTask, OrchestrationReasoning, SkillSelection и итоговую полезность. Только после этого token delta можно рассматривать как meaningful accounting result.

## Что доказано

- Создан свежий TOKEN-01 run folder для `token-01-20260618-024009`.
- Создан machine-readable matrix seed для всех scenario/host/method строк существующего scenario pack: 54 строки, 12 сценариев, Claude/Cursor/Codex, методы `prompt-to-task`, `orchestration-plan`, `skill-selection`.
- Матрица фиксирует недостающие baseline/adaptive refs, redaction refs, accounting version, quality dependency, host runtime scope и blockers.
- Deterministic token-count estimate считает только сохраненный prompt text сценариев и помечен как setup/estimate.

## Что не доказано

- Экономия токенов не доказана.
- Улучшение качества, semantic quality и learnability не доказаны.
- DPO, training, checkpoint readiness, target promotion и rollback readiness не доказаны.
- Full Claude Desktop chat UI, full Cursor chat participant UI и full Codex chat/app send-message token savings не доказаны.
- Никакая product readiness или final readiness из TOKEN-01 не следует.

## Следующий независимый трек

Следующая вероятная фаза: `TOKEN-02`, чтобы создать или отремонтировать стабильную trace/accounting schema для строк этой матрицы. До любой заявки на savings также нужны `QUALITY-01/02/03`, чтобы same-pair quality non-regression мог пройти после появления baseline/adaptive traces.
