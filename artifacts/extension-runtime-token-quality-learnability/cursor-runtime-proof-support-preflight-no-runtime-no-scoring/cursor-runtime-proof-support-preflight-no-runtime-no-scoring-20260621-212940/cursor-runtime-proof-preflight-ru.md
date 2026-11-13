# Cursor runtime proof preflight: support-only, no runtime, no scoring

Пакет: `DOCS-QA-CURSOR-RUNTIME-PROOF-SUPPORT-PREFLIGHT-NO-RUNTIME-NO-SCORING-01`

Сгенерировано: `2026-06-21T21:29:40+03:00`

Этот пакет является только support-only preflight для будущего Cursor runtime proof. Он не является доказательством Cursor runtime proof, не является доказательством готовности host, продукта, качества, learnability или token savings, и создан без запуска Cursor. Он не запускает Cursor, Claude или Codex runtime, не запускает proof/eval/scoring runners, не читает raw private content, не делает внешних AI/web/model вызовов, не меняет код, model, checkpoint, locator, target, training или DPO artifacts.

## Что должен показать будущий Cursor proof

Будущий Cursor proof должен быть отдельным host-specific доказательством после свежего разрешения. Он должен показать:

- Extension Host evidence из того же запуска Cursor.
- Видимую UI/panel/chat evidence из Cursor host surface.
- CLI path evidence, если CLI путь применим к этому запуску.
- Same-run correlation refs, связывающие input, host log, core call, output и UI evidence.
- Redacted input/output refs, которые доказывают, что именно было отправлено и что было получено, без раскрытия raw private content.
- Видимый RequestedSkills/AvailableSkills binding, показывающий, какие skills были запрошены и какие были доступны.
- Negative/error path через тот же Cursor host surface.
- No-side-effect statement, доказывающий, что во время runtime proof не было model, checkpoint, training, DPO, locator или target mutation, если это не было отдельно и явно разрешено.

## Границы доказательства

Support packet, helper output, stdout, dashboard, direct-handler, command summary или panel support summary не могут заменить будущий visible Cursor UI/panel/chat proof. Этот preflight только фиксирует требования и блокировки; он не закрывает runtime proof queue и не доказывает host readiness.

Фразы-сторожа для будущего запуска остаются активными: `fresh host-specific approval` и `fresh host-specific Luba approval`. Свежая host-specific approval от Luba все еще требуется до любого Cursor runtime launch, scoring, proof/eval runner, training, DPO, checkpoint/model/locator/target mutation или raw private read.

## Текущий статус

Сейчас authorization flags остаются false. Runtime launch, scoring, training, raw private reads, external AI/web/model calls и proof/eval/scoring runner не авторизованы. Очередь runtime proof остается живой: следующий безопасный шаг - review этого Cursor preflight packet без запуска runtime и без scoring.
