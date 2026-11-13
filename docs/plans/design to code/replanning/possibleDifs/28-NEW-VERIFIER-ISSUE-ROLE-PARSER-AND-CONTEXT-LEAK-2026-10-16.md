# N28-A — Верификатор может переучиться на ошибочно переставленное правило

**Дата:** 2026-10-16. **Статус:** SOURCE-CONFIRMED CODE MECHANISM; NATIVE QUALITY `NOT_RUN`; причинная связь с RP01 neural free answers `NOT_PROVEN`. **Редакция:** независимое дополнение к `possibleDifs`. **Код core READ_ONLY**, только planning — write. Один и тот же механизм найден и в [старой ветке DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/27.TranslationDomain/TranslationTrainedUnits.cs#L1215-L1250): это не новое изменение RP01.

## Почему это отдельная гипотеза, а не повтор других агентов

Документы [23 verifier AND/direction](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md), [26 role/polarity](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md) и [27 metric bias](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md) уже рассматривают область действия правила, накопление требований, полярность и односторонний heldout. **Дополнительный конкретный seam здесь — именно превращение текстового `for concept {src} in context {ctx}` в структуру с переставленными ролями на основании порядка enum, а не порядка токенов текста.** Поиск этих файлов не выявил описания данной функции как отдельной причины; весь каталог и все соседние отчёты не индексировались полнотекстово.

## Прямая улика: producer → parser → persisted rule → consumer

1. Контракт самой системы: [`QualityVerifierUnit.Verify`, строки 1071–1080](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1071-L1080) формирует диагностическое выражение именно как `expected '{required}' for concept {src} in context {ctx}`; это источник формата, не наша придуманная строка.
2. Обучение: [`TrainOnExample`, строки 1094–1119](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1094-L1119) принимает выбранный FAIL-output, разбирает каждое issue, кладёт `_conceptRules[(src,ctx)]=required`; дополнительно записывает требование в `_requiredTermsBySignature`.
3. **Ошибка ролей:** [`TryParseIssue`, строки 1216–1247](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1216-L1247) обходит `Enum.GetNames<SemanticConcept>()` и дважды делает `rest.Contains(name,...)`. Первый найденный *по порядку объявления enum* становится `src`, второй — `ctx`; слов `for concept` и `in context` парсер не учитывает.
4. [`SemanticConcept`, строки 59–77](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L59-L77): `ADD_OP` объявлен раньше `LAUNCH_OP`. Значит для допустимого текста `expected 'ready' for concept LAUNCH_OP in context ADD_OP` получим **`src=ADD_OP, ctx=LAUNCH_OP`** вместо **`src=LAUNCH_OP, ctx=ADD_OP`**. Это детерминистическое следствие текущего кода, не оценка ML.
5. [`Verify`, строки 1051–1091](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1051-L1091) применяет глобальное `_conceptRules` при наличии `src` в sourceTokens, **не проверяя присутствие `ctx`**. Таким образом неправильный `src` вызывает ложный отказ для других source signatures с `ADD_OP`, если в candidate нет слова `ready`. Это *два отдельных эффекта*: неверно восстановленная роль и уже известное отсутствие scope-check.
6. [`ExportCheckpoint/ImportCheckpoint`, строки 1173–1213](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1173-L1213) сериализуют и восстанавливают ошибочную пару `(src,ctx)`, поэтому cold restore не очищает дефект.

### Почему частичная проверка может скрыть ошибку

При проверке того же `SemanticSignature`, на котором учили, `_requiredTermsBySignature` всё равно требует `ready` и создаёт иллюзию правильного Verifier. Ошибка `_conceptRules` проявляется особенно **на другой сигнатуре, где есть `ADD_OP`, но нет `LAUNCH_OP`**. Поэтому exact-source и самоповторение training examples не являются доказательством качества переноса.

Условный пример: модель училась на конструкции «LAUNCH_OP в ADD_OP контексте требует ready»; после тренировки она может отклонить перевод в совершенно другой фразе с ADD_OP, где ready не нужен. Это прогноз, а не сохранённый реальный инцидент.

## Новая причинная гипотеза и границы

**H28-A1 (парсер):** исправление `TryParseIssue` на строгое выделение `concept` и `context` уменьшит ложные отказы на source-disjoint ADD_OP-only запросах при неизменных учебных данных и неизменной проверке контекста. **H28-A2 (контекст):** отдельное включение проверки `ctx` при `Verify` улучшит перенос на действительно контекстно-зависимых задачах, но может понизить recall там, где исторические примеры использовали `ctx` как необязательный признак. **H28-A3 (потеря на consumer):** эффект должен достигать ordinary `TrainedTranslationAdapter.TranslateAsync`, который при `verification.Pass=false` у non-phrase кандидата отдаёт Bootstrap ([строки 115–142](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L142)); phrase-map exception нужно исключить из проверки, иначе эффект замаскирован.

**Не переносить на все домены:** сильная relevance для RU↔EN/HE↔EN семантического перевода, правил с двумя разными `SemanticConcept`, source signature generalisation и learned verifier. Никакого доказанного эффекта для C# DPO LoRA или generic GPU-модели нет.

**Falsifier:** если реальные accepted FAIL pairs не содержат диагностики с двумя распознаваемыми concept names или `TrainedTranslationAdapter` никогда не потребляет trained `QualityVerifierUnit`, механизм остаётся latent. Если исправленный native arm не улучшает blind correctness на сопоставимых source-disjoint запросах, пометить `NO_GAIN`, не объявлять регрессию доказанной.

**Микропроверка:** [28-FAST-CAUSAL-PILOTS-VERIFIER-AND-ORCHESTRATION-2026-10-16.md](28-FAST-CAUSAL-PILOTS-VERIFIER-AND-ORCHESTRATION-2026-10-16.md), сценарий V28. Прежние H23/H26 остаются самостоятельными; не считать их повторно «новыми» находками.
