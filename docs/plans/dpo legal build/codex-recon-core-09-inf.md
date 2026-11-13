# Codex Recon — core-09-inf: Инференс-пайплайн (09.Inference)

Подзадача рекона: «Инференс-пайплайн: как юнит выдаёт результаты на входах.»
Область: `C:/Projects/llm_mvp_core/09.Inference`. Только чтение + запись этого файла.

## Что это и где (точные ключевые пути)

Маленький, самодостаточный модуль авторегрессивной генерации текста на сыром
трансформере. Всего три файла кода + один csproj:

- `C:/Projects/llm_mvp_core/09.Inference/Inference.csproj`
  — ссылается на `00.TensorCore`, `01.Tokenizer`, `03.TransformerModel`.
- `C:/Projects/llm_mvp_core/09.Inference/TextGenerator.cs`
  — оркестратор генерации (prefill + decode цикл). Класс `TextGenerator`.
- `C:/Projects/llm_mvp_core/09.Inference/Sampler.cs`
  — статические стратегии выборки токена. `static class Sampler`.
- `C:/Projects/llm_mvp_core/09.Inference/KvCache.cs`
  — кэш ключей/значений на слой. Класс `KvCache : IDisposable`.

Кто использует (по `*.csproj` ProjectReference и grep по коду): только тест-проект
`C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj` (строка 31) и его
тесты `Tests/SamplerTests.cs`, `Tests/KvCacheTests.cs`, `Tests/IntegrationTests2.cs`.
Никакой из проектов оркестрации/обучения (например `15.OrchestratorModel`,
ILearningMachine-иерархия) на `09.Inference` НЕ ссылается. Прочие совпадения —
копии в `.claude/worktrees/*` (зеркала, не отдельный код).

## Как устроено/работает (кратко)

Пайплайн «юнит выдаёт результаты на входах» здесь = генерация токенов из текста:

1. `TextGenerator.Generate(prompt, maxNewTokens, temperature, topK, topP)`:
   - `prompt` -> `BpeTokenizer.Encode` -> список id;
   - PREFILL: `IdsToTensor(inputIds)` ([1, T]) -> `LlmModel.Forward` -> логиты
     [1, T, vocab]; берём логиты последней позиции (`ExtractLastLogits`) ->
     `Sampler.Sample` -> первый новый токен;
   - DECODE: цикл до `maxNewTokens` или до EOS; на каждом шаге один токен ->
     `LlmModel.Forward` ([1,1,vocab]) -> последняя позиция -> `Sampler.Sample`;
   - результат: `BpeTokenizer.Decode` только новых токенов (срез после prompt).
2. `Sampler.Sample`: temperature<=0 -> greedy `TensorOps.Argmax`; иначе масштаб
   на 1/temperature, затем top-K (`TopKFilter`) ИЛИ top-P/nucleus (`TopPFilter`)
   ИЛИ чистый `Softmax`; финал — `MultinomialSample` (случайная выборка по
   вероятностям через `Random`).
3. `KvCache.Update(layer, newK, newV)`: первый токен — копия; далее
   `TensorOps.Concat` по оси 2 (ось последовательности); `SeqLen` по слою 0.
   ВАЖНО: `KvCache` в текущем `TextGenerator` НЕ используется — в decode-цикле
   модель зовётся только на новом токене без передачи накопленного кэша
   (потенциальная заготовка/несвязанный компонент).

Численные значения по умолчанию (не magic, заданы параметрами):
`maxNewTokens=200, temperature=0.8, topK=50, topP=0.95`.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

НИЗКАЯ-КОСВЕННАЯ. Это слой исполнения языковой модели (декодинг токенов), а НЕ
часть trainable-иерархии DPO/графа. Тем не менее для плана важно:

- DPO/обучение: модуль НЕ строит и не потребляет DPO-пары, нет learned state,
  нет checkpoint/manifest/export/import, нет метрик (accuracy/loss/reward).
  Обучаемость живёт в `03.TransformerModel`/тренировочных проектах, не здесь.
- Граф/оркестрация: `TextGenerator` — линейный prefill->decode цикл, не граф,
  без узлов/рёбер, без fan-out/fan-in, без async-графа. Не ILearningDynamicGraph.
- Teacher/provenance gate: если ILearningDynamicGraph будет САМ генерировать
  тексты-кандидаты (chosen/rejected) через этот локальный движок на собственной
  open-weight модели, это НЕ закрытый проприетарный teacher (Claude/OpenAI/
  Gemini запрещены как distillation-teacher). То есть локальный `TextGenerator`
  как генератор кандидатов — легально допустимый источник по
  Legal-Teacher-гейту скила, но провенанс (model_id, license, access_mode)
  всё равно надо фиксировать у вызывающей стороны.
- Измерение/недетерминизм: `Sampler` использует общий статический `Random`
  без seed -> генерация недетерминирована. Для воспроизводимых DPO/оценочных
  прогонов это РИСК (нельзя повторить ровно тот же rejected/chosen). Если граф
  будет опираться на этот движок для измеримых паков — нужен инъектируемый seed.
- Роль в графе: правильное место этого модуля — внутренний детерминированный
  (по семантике, не по сэмплингу) хелпер/исполнитель инференса ВНУТРИ
  допустимого уровня (например ILogicalUnit/ILearningMachine для языковой
  модели), а не самостоятельный trainable-уровень.

## Классификация по уровням (правила скила trainable-architecture-decision)

Применяю Decision Procedure (шаги 1-6) и Layer 5A `generated-code-review`.

- `TextGenerator` (TextGenerator.cs:10) — capability: «сгенерировать текст по
  prompt». Механизм: оркестрация вызовов `LlmModel.Forward` + `Sampler`.
  Allowed level: НЕ один из пяти (это не ILogicalOrchestrator — у него нет
  `RunAsync(LogicalOrchestratorRequest)`, нет OperationResult, нет контрактов
  иерархии). Классификация: внутренний детерминированный исполнитель инференса
  (decode-loop helper). Runtime authority: да, но как механика декодинга, не как
  обучаемое решение. Semantic claim: отсутствует. Verdict: `OK_INTERNAL_DETAIL`
  — допустим как деталь внутри будущего ILogicalUnit/ILearningMachine языковой
  модели; сам по себе не trainable-уровень.
- `Sampler` (Sampler.cs:7) — selector следующего токена (greedy/temp/top-k/
  top-p/multinomial). По правилу скила «scorer/selector/... не являются
  архитектурными паттернами», это внутренний детерминированный (стохастический)
  хелпер выборки. Verdict: `OK_INTERNAL_DETAIL`. Не выдаёт себя за trainable —
  честно. Замечание: общий `Random` без seed (недетерминизм) — не нарушение
  иерархии, но риск воспроизводимости для DPO/измерений.
- `KvCache` (KvCache.cs:7) — структура состояния (кэш K/V), `TensorOps.Concat`.
  Это инфраструктурный буфер, не selector/обучаемое состояние. Verdict:
  `OK_INTERNAL_DETAIL`. Замечание: фактически не подключён к `TextGenerator`
  (висячий компонент) — для плана это «есть заготовка кэша, но decode её не
  использует».

Скрытого детерминизма, выдающего себя за trainable (правило R4 / Deterministic-
Boundary gate), здесь НЕТ: ни один класс не заявляет себя моделью/обучаемым
юнитом. Это честный низкоуровневый инференс. Нарушение возникло бы ТОЛЬКО если
будущий план ILearningDynamicGraph объявит `Sampler`/`TextGenerator` «trained
unit» или «final authority» semantic-решения — тогда verdict станет
`FORBIDDEN_RUNTIME_SHORTCUT` / `NEEDS_ILOGICALUNIT_WRAPPER`.

Все методы — НЕ `OperationResult<T>` (Rule 1 проекта); это самостоятельный
низкоуровневый модуль вне внешнего API-контракта (как и TensorCore). Для плана:
если граф будет звать инференс как сервис, обёртка должна вернуть
`OperationResult<T>` и зафиксировать provenance.

## Пробелы / что доисследовать

- Контракт `LlmModel.Forward` и форма логитов ([1,T,vocab]) — определены в
  `03.TransformerModel` (не читал в этом реконе); если граф зовёт инференс,
  нужно сверить точную сигнатуру и батчинг.
- `BpeTokenizer.Encode/Decode/EosToken` — в `01.Tokenizer` (не читал); влияет
  на провенанс/детерминизм входов.
- `TensorOps` (Argmax/Scale/Softmax/SortDescending/CumSum/Concat) — в
  `00.TensorCore`; детали не входили в область 09.
- Висячий `KvCache`: подтвердить по тестам (`Tests/KvCacheTests.cs`,
  `IntegrationTests2.cs`), используется ли кэш где-то ещё или это чистая
  заготовка под оптимизацию decode.
- Недетерминизм `Sampler` (статический `Random` без seed): решить на уровне
  плана, нужен ли seed-инъекшн для воспроизводимых DPO/оценочных прогонов, если
  ILearningDynamicGraph будет генерировать кандидатов этим движком.
- Связь с обучаемой иерархией: на сегодня ОТСУТСТВУЕТ (ни один не-тестовый
  проект не ссылается на 09.Inference). Подтвердить, что план НЕ предполагает
  09 как trainable-узел, а только как исполнителя языковой модели.
