# TOKEN-02: схема трассировки и учета токенов

Run id: token-02-20260618-030854
Фаза: TOKEN-02
Статус: статическая схема и контракт учета подготовлены для review.

TOKEN-02 является только schema/static setup. Эта фаза не запускала baseline или adaptive runtime runs, не измеряла фактические токены в живом сценарии, не запускала внешнее AI-решение задач, не начинала обучение, не меняла checkpoint и не продвигала target. Поэтому TOKEN-02 сам по себе не доказывает экономию токенов, улучшение качества, обучаемость или финальную готовность.

## Что означает trace/accounting schema

Trace schema задает единый формат следа будущего запуска. Для каждой пары TOKEN-01 будущие фазы должны сохранить raw/private trace, который можно проверить внутри проекта, и redacted/shared trace, который можно безопасно использовать как обзорное доказательство.

Accounting schema задает единый формат учета токенов для той же пары. Она фиксирует, какие input/output spans считаются, каким счетчиком они считаются, какая версия tokenizer/accounting использована, какие input/output/total token counts записаны и почему пара считается валидной или невалидной для будущего сравнения.

Главная цель TOKEN-02: заранее запретить сравнение baseline и adaptive "на глаз" или задним числом. Все будущие trace/accounting записи должны быть сопоставимы по pair_id, host, method, scenario, prompt hash, evidence class/live scope, target locator mode, redaction policy и tokenizer/accounting version.

## Обязательные поля будущих baseline/adaptive traces

Будущие TOKEN-03 и TOKEN-04 trace-записи должны включать:

- `pair_id`, `host`, `method`, `scenario_id`.
- `prompt_hash`, `prompt_ref` и нормализованные ссылки на prompt.
- `evidence_class` и `live_scope`.
- `run_kind` со значением `baseline` или `adaptive`.
- `target_locator_mode` и `target_locator_ref`.
- `tokenizer_accounting_version`, `token_counter_name`, `token_counter_version`.
- `redaction_policy`, `raw_private_trace_ref`, `redacted_trace_ref`.
- `input_text_ref_or_hash` и `output_text_ref_or_hash`.
- `input_token_count`, `output_token_count`, `total_token_count`.
- `quality_non_regression_ref`.
- Safety flags: `no_external_ai_task_solving=true`, `no_training_started=true`, `no_model_mutation=true`, `claim_allowed=false`.

Pair-level accounting report должен связать baseline trace ref и adaptive trace ref, total counts, delta field, failure labels, pairing integrity status и ссылку на QUALITY-03 same-pair non-regression. Даже если в будущей фазе delta станет числом, `claim_allowed` остается `false`, пока не пройдут pairing integrity checks и QUALITY-03.

## Проверка TOKEN-01 matrix

TOKEN-02 мапит 54 строки TOKEN-01 matrix на placeholders для будущих baseline trace, adaptive trace, pair accounting report и QUALITY-03 ref. Все 54 строки сохраняют `claim_allowed=false`.

Отсутствующие runtime traces в TOKEN-02 не считаются провалом экономии, потому что эта фаза только статически готовит схему. Такие отсутствующие следы должны быть помечены как `MISSING_TRACE_NOT_A_SAVINGS_FAILURE_YET`. Реальные missing/invalid labels станут содержательными только после TOKEN-03, TOKEN-04 и QUALITY-03.

Результат TOKEN-02 по матрице означает только то, что форма будущего учета применима к 54 строкам TOKEN-01. Это не означает, что уже существуют baseline/adaptive runtime traces или что есть доказанная экономия токенов.

## Что требуется перед любой savings claim

Перед любой заявкой на экономию нужны три независимые группы доказательств:

- TOKEN-03 должен заполнить baseline common/no-adaptive trace records по этой схеме.
- TOKEN-04 должен заполнить adaptive/user-local trace records по тем же `pair_id`, `host`, `method`, prompt hash, evidence class/live scope, tokenizer/accounting version и redaction policy.
- QUALITY-03 должен дать same-pair quality non-regression для тех же пар.

Только после этого pair accounting report может сравнивать valid pairs. Invalid или missing pairs должны сохранять failure labels и не должны превращаться в заявленную экономию.

## Что подтверждено

- Подготовлен статический TOKEN-02 trace/accounting contract для будущих запусков.
- Описаны обязательные поля trace records и pair-level accounting report.
- 54 строки TOKEN-01 matrix сопоставлены с будущими baseline/adaptive/accounting/QUALITY-03 placeholders.
- Missing traces для этой статической фазы имеют метку `MISSING_TRACE_NOT_A_SAVINGS_FAILURE_YET`.

## Что не подтверждено

- Экономия токенов не подтверждена.
- Улучшение качества, semantic quality и learnability не подтверждены.
- DPO, training, checkpoint readiness, target promotion, rollback readiness и final readiness не подтверждены.
- Full Claude Desktop chat UI, full Cursor chat participant UI и full Codex chat/app send-message token savings не подтверждены.

## Следующий независимый трек

Следующая вероятная фаза: TOKEN-03 baseline trace capture по этому контракту. После нее нужны TOKEN-04 adaptive trace capture и QUALITY-03 same-pair quality non-regression. До прохождения этих фаз TOKEN-02 остается только статической подготовкой схемы и учета.
