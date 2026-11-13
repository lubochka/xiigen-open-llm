# N28-B — Новая гипотеза ложного учебного результата: held-out Unit ≠ ordinary serving и Verifier видит только отрицательные примеры

**Дата:** 2026-10-16. **Классификация:** SOURCE-CONFIRMED EVALUATION DESIGN / RUNTIME CAUSAL EFFECT OPEN. **На текущем срезе real Student, native .NET и independent judge не запускались.**  
**Домен:** `TranslationMachine.TrainStructuredBatchAsync` и `TrainedTranslationAdapter`; не приписывать эту ошибку отдельно оцениваемому RP01 C#500 / ENRU500 neural Student.

## Суть

Здесь можно получить высокую `AccuracyAfter`, не научившись правильно отвечать. Дело не просто в «слабом тестовом покрытии». Встроенный учебный скоринг проверяет **другой путь** и, для автоматически построенного Verifier held-out, **другую по смыслу задачу**.

В частности:
1. На training/held-out каждого Executor используется заново созданный rule-based tokenizer, тогда как обычный запрос через adapter использует общий обученный SemanticTokenizer.
2. Held-out QualityVerifier заново токенизирует source с жёстко указанным `"ru"`. `VerifierInput` не содержит языка; live adapter получает настоящий `request.SourceLanguage` через SemanticTokenizer.
3. `TranslationStructuredBatchBuilder.BuildGeneralBatch` в verifier-примере формирует `chosen Pass=false` для *rejectedCandidate*. Если held-out составлен этим builder, **все verifier cases отрицательные**. Поэтому классификатор, который всегда отклоняет кандидат, может прекрасно выглядеть на этом условном тесте, хотя будет отклонять все правильные пользовательские переводы.
4. `TrainStructuredBatchAsync` считает среднее от результатов отдельных Unit, а не долю правильных финальных ответов через полный Flow. Успехи на трёх изолированных подзадачах не означают, что правильный текст проходит общий pipeline.

## Доказательства

| ID | Кодовая улика | Условный эффект |
| --- | --- | --- |
| N28B-C1 | [ExecutorUnit.MeasureAccuracy:458–484](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L458-L484): `var tokenizer = new TranslationSemanticTokenizer();`, затем `tokenizer.Tokenize`. | Executor held-out не учитывает ранее изученную semantic correction, которая применяется при ordinary inference. |
| N28B-C2 | [QualityVerifierUnit.MeasureAccuracy:1148–1172](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1148-L1172): `Tokenize(input.SourceText, "ru")`. | Контекстные признаки проверки могут не совпасть с фактическим source language live запроса. |
| N28B-C3 | [VerifierInput:50–55](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L50-L55): только `SourceText` и `CandidateText`. | В этом DTO нельзя корректно перенести или оценить язык/направление без отдельного дополнительного контракта. |
| N28B-C4 | [BuildGeneralBatch:124–141](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L124-L141): verifier `Input=VerifierInput(source,rejectedCandidate)`, chosen `Pass=false`, rejected `Pass=true`. | Builder выдаёт **только негативный класс** для verifier при штатной форме этих данных; accuracy on held-out не измеряет false rejections lawful answers. |
| N28B-C5 | [TranslationMachine.MeasureHeldOutAccuracy:662–673](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L662-L673) — группировка по `UnitId` и `Average()`; [TrainStructuredBatchAsync:125–145](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L125-L145) использует это значение как `AccuracyAfter`. | Значение не является end-to-end accuracy обычного запроса и может вводить в заблуждение при сравнении. |
| N28B-C6 | [TrainedTranslationAdapter:115–143](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L143): learned tokenizer → Executor → Verifier → fallback; особое phrase-map exception. | Именно здесь проверяется полезность пользовательского результата, а не на отрицательных примерах в Unit-тесте. |

**Важная оговорка:** разбиение на отрицательные `VerifierOutput.Pass=false` логично для обучения отвергать *плохой* кандидат. Багом является вывод «проверяющий научился принимать хорошие ответы» только из этой одноклассовой held-out метрики. Чтобы обвинить production путь, нужно измерить оба класса и итоговый ответ через adapter.

## Дополнительный условный seam: неверный язык при запоминании source marker

[QualityVerifierUnit.TrainOnExample:1095–1145](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1145) извлекает marker из `input.SourceText`. Перегрузка [ExtractSourceMarker(string):1265–1270](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1270) всегда вызывает `new TranslationSemanticTokenizer().Tokenize(sourceText, "ru")`. Live `Verify` получает semantic tokens из фактического запроса. Это дополнительный train/serve asymmetry **при условии**, что обе языковые разметки дают разные POS/marker; для многих английских фраз оба режима дадут те же признаки, и тогда эффекта нет. Нельзя автоматически объявлять все EN→RU уроки сломанными.

## Отличие от уже опубликованных findings

- [N23 Verifier accumulated AND + direction scope](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) говорит о **конфликте самих learned rules**; N28-B — о **метрике** и несовпадении вызываемого алгоритма, даже если все правила корректны.
- [N26 metric-direction bias](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md) говорит о `MeasureUnitCoverage` без held-out и жёстком RU_EN executor. N28-B относится к `MeasureHeldOutAccuracy` с held-out, отдельному одноклассовому verifier и обходу обученного SemanticTokenizer.
- [N21 self-label](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) — другое: что считается правильной меткой upstream. [N28-A](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md) — неверный downstream индекс по этим признакам.
- [N26 polarity](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md) проверяет валидность лексического закона, а не репрезентативность панели оценивания.

## Кому это релевантно

- **P1, multilingual structured translation:** когда принимается решение о продвижении Unit на основании `AccuracyAfter`.
- **P1, обучаемые составные системы:** если upstream feature extractor меняется при обучении, но его состояние не используется downstream evaluator.
- **P3 для neural-only RP01:** метрика конкретного `TranslationMachine` не доказана действующей в этом Student-пути.
- **Other domains:** переносить принцип **evaluate same actual consumer + balanced lawful/unlawful examples**, но не русский `SourceLang` или грамматику в области C#/UI/API, где используются другие tokenizer/Verifier.

## Малая фальсифицирующая проверка

[Микроэксперимент N28](28-MICRO-N28-CAUSAL-AB-EXPERIMENT-2026-10-16.md), отдельный фактор B: один и тот же trained-state и те же 8–16 frozen answers проверяются **(A)** штатным `MeasureAccuracy` и **(B)** в реальном adapter с проверкой независимым oracle на обоих классах. В первую очередь нужна `confusion matrix` (valid accepted / valid rejected / invalid accepted / invalid rejected), не новая зелёная тестовая квота.

**Статус проверки:** ошибки сравниваемых маршрутов подтверждены исходным кодом; оценка величины false-pass/false-fail и причинного влияния на пользователя — **NOT_RUN**.


## Усиление после аудита переносов — 12 ноября 2026

Исторические исходники и статус native NOT_RUN выше сохраняются. Новая опора: [аудит текущего FIX-P56](https://github.com/lubochka/llm_mvp_core/blob/5afb9d265fb62db8c9d4ea00f61e5f2ed9e9fc3a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/codex-product-review.md). У него уже есть обычный orchestrator и disk→fresh object; утверждать, что обычный путь вообще нигде не проверяется, теперь неверно. Его область — signature requirements и exact-source candidate, не все ветви verifier и не новое доказательство N28-B.

В будущем сравнении N28-B на одном frozen состоянии составить для каждого RowId цепочку «фактические tokens/signature → candidate → unit verdict → final adapter/output». Четыре источника отличий проверять раздельно: tokenizer state, source language, состав классов good/bad, правило окончательного допуска. Поменять все четыре в одном B и назвать причину одним tokenizer нельзя. Сначала диагностическая таблица на том же checkpoint; только затем отдельный однофакторный ремонт установленной причины.

| Требование к выводу | Различающий контроль |
|---|---|
| Проверяющий умеет принимать правильные ответы | Valid candidate при неизменном обученном состоянии; always-reject получает ноль в этой ячейке даже при 100% на negative-only панели |
| Внутренний отказ защищает пользователя | Сохранить actual final adapter/output; отдельно пометить phrase-map exception. Unit Fail при принятом final answer не считается защитой |
| Используется обученный tokenizer | Тот же source до/после его урока, реальные signatures и потреблённый ключ; ID урока в JSON недостаточно |
| Показано обобщение | Exact pair / seen input with new target / unseen input относительно TRAIN каждого плеча; новая метка или RowId не делают input новым |
| Restore повторяет проверенный путь | Те же RowId и literal outputs; same-process fresh object и отдельный process — разные статусы |

Confusion matrix публикуется рядом с Unit scores, ordinary accuracy и constant-answer baseline **той же панели**. Нулевая поддержка класса — UNMEASURED; значение другого класса туда не подставлять. Все метрики используют один зафиксированный comparator и независимые gold, а не expected, полученный из текущего consumer.

Это усиление плана и границ доказательства. Новых Train/Flow/Load, изменения защитных порогов или запуска native этим дополнением не выполнено; исходный bounded N28 эксперимент не расширяется скрыто.
