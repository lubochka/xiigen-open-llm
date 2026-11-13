# N28 — Три быстрых однофакторных эксперимента, проверяющих именно качество обучения

**WORK ORDER / PLAN ONLY, .NET NATIVE NOT_RUN, CODEX/Astra INDEPENDENT JUDGE NOT_RUN** · 16.10.2026 · core pin `d5c2409b24292d361593b8262b0c54997b15f539`. Не запрашивать 500 DPO/Windows CI. Исторические данные, веса, checkpoints и каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` НЕ менять.

## Сначала границы ответственности

**Человеческий выбор урока за пользователя выполняет CODEX на Astra 6 UltraCode.** Если реально нужны финальные DPO-пары, **разрешённая внешняя открытая китайская модель создаёт chosen и rejected** с точной provenance/rights. **CODEX на Astra 6 UltraCode отдельно оценивает обе стороны, а после обновления Student — реальное качество ответа.** Ни один приведённый диагностический пример не является настоящей DPO-парой и до approval не является final gold. Для экспериментов на дискретной Unit-памяти финальные DPO могут вообще не понадобиться.

## EXP-28-U1 · Проверить восстановление смысла испанских акцентов

**Фактор:** A=current `NormalizeMapKey`; B=`Normalize(FormC)` *до* текущего key-normalizer, причём одновременно в train и inference. Другие predicates/KNN/topk/weights/seed/policy/verifier/router фиксированы; иначе `INVALID_CONFOUNDED`.

TRAIN (только для разработки, CODEX утверждает смысл): `si` → `if` и `si\u0301` → `yes`, два уникальных `ExampleId`, domain ES_EN. DEV: `sí` → `yes`. Для HIDDEN выделить 4 самостоятельные смысловые конструкции с «если/да» и двумя NFC/NFD реализациями, без source-family overlap; gold перед тестом получает независимый approval. Если exact+KNN обученный класс не переносит в source-disjoint phrasing — так и написать, не считать exact training-key успехом в learning gain.

Вызвать существующий `TranslationMachine.TrainStructuredBatchAsync`/`ExecutorUnit.TrainOnExample` и **тот же** `TrainedTranslationAdapter/TranslationRoutingFlow`; до/после снять `normalized_key`, `_exactSourceMap` size/overwrite (через isolated diagnostic), `selected_adapter`, `selected_source_id`, `TranslatedText`, `Verification`. Контроли: C0 no-training, C1 shuffled labels, train-key positive, negative `si` conditional, и **после чистого cold restore**. **PASS гипотезы причинного влияния**, если B повышает blind meaning-correct по сравнению с A без новых ошибок «если↔да» и результат сохраняется в cold restore. Иначе H остается H/NOT_CAUSAL.

## EXP-28-U2 · Ложное Ñ→mojibake и case parity

**Строго отдельный A/B**, исходный Unicode фиксирован (не включать NFC-fix). A=current `LooksLikeUtf8Mojibake`; B=детекция только действительно повреждённого байтового шаблона, не один `Ñ`. TRAIN `SEÑAL x!=y` с одобренным правилом; HELDOUT `señal x!=y` — тот же смысл, иной регистр; **adversarial** `señal x==y` — иной закон. Снять raw/source codepoints, normalized key, lookup ID, returned target/adapter, native output и cold restore.

**Заранее предупреждённый no-go:** B может уравнять регистр, но обе стороны при этом потеряют операторы `!=`/`==` (уже известный [отдельный gap](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md)). В таком случае `CASE_PARITY_PASS/Law FAIL` и **не** повышать качество. Следующим, только отдельно, проверяется operator-aware semantic key. Нельзя одновременно поменять mojibake guard и operator features в одном измерении.

## EXP-28-S · Хороший русский перевод с защищённым API нельзя отклонять как английский

**Фактор:** A=текущий strict EN_RU target gate; B=тот же strict gate, в котором латиница допускается **только в одобренных typed protected identifier spans**, и проверяются оригинальные copy/law/negation constraints. KNN candidate, exemplar, source, tokenizer, learner, count/weights/seed неизменны.

Диагностический TRAIN: `Validate the API request` → `Проверьте запрос API`; HIDDEN source-family: `Check API requests` с заранее подтверждённым target `Проверьте запросы API`. **Precondition:** оба A/B должны получить одного и того же KNN neighbor и одинаковый chosen output. Если это не так — остановиться и подготовить узкую fixture/instrumentation, не менять одновременно KNN. Negative controls: `Check API` целиком непереведённый (не должен пройти), `Проверьте интерфейс` (должен пройти), неподтверждённый ASCII span (должен быть отклонён).

Запуск 1: direct gate/veto от одного фиксированного candidate. Запуск 2: actual `ExecutorUnit.Execute` с тем же learned state. Запуск 3: обычный `TrainedTranslationAdapter.TranslateAsync`, зафиксировать `AdapterId, confidence, verification.Pass, fallback` и **CODEX blind semantic verdict, если реально вызван**. Запуск 4: frozen cold reopen. **PASS** лишь при real improvement по законным protected names с неизменными false-accept/negation/source-copy; `confidence=0.88` сам по себе не показатель качества.

## Минимальный протокол и falsifiers

| Проверка | Сохранить | Fail-fast |
|---|---|---|
| Identity | core SHA, original file:line, domain/direction, exact learner recipient и ordinary caller | wrong path → `NOT_APPLICABLE` к RP01 neural |
| Isolation | A/B diff одной функции, common ordered corpus, matched seed/index/weights | >1 factor → `INVALID` |
| Training | 2–4 законных train rows, минимум 2 negative, 4–8 source-family-disjoint heldout | copied train prompts in holdout → `INVALID` |
| Evaluation | raw actual translated answer, selected neighbor, law correctness, coverage, abstention, refusals, latency | only changed key/gradient/tests without useful answer → `NOT_PROVEN` |
| Persistence | exact checkpoint+tokenizer+learned maps+index, independent cold process | live≠cold → separate restore bug |
| Review | independent CODEX/Astra actual receipts, no reconstructed fictional votes | no receipt → `AWAITING_INDEPENDENT_REVIEW` |

**Возможные вердикты:** `SOURCE_ONLY`, `CAUSAL_GAIN`, `NON_CAUSAL`, `PARTIAL_UNSAFE`, `INVALID_CONFOUNDED`, `NOT_APPLICABLE`. Обязательная запись: `run_id, branch, source_sha, exact dataset hashes and source groups, training_recipient, A/B_factor, key_hex, selected_memory_id, actual_answer, verifier_result, CODEX quality verdict/NOT_RUN, old law retention, cold_restore_hash, wall_clock, fail_reason`. Действующий CODEX для оценки не подменять нашим source-proxy.
