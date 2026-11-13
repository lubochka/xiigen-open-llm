# 18 — Проверенный разрыв путей ProductHost: наличие обученного KNN Unit ≠ его влияние на GenerateAsync

**16.10.2026 · SOURCE-CONFIRMED ROUTE DIFFERENCE / CAUSAL IMPACT NOT MEASURED.** Это уточнение [DIF-08](01-PRIORITIZED-DIFFERENCES.md), [DIF-25/26](06-UNITS-FLOWS-GRAPH.md) и [трассы RP01](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md). Здесь впервые приложены **конкретные реализации двух методов ProductHost**, а не только предположение «возможен обход Unit».

## N18-A — Generation endpoint выполняет TextGenerator, а не KNN.ExecuteAsync

[CapabilityServingCompositionRoot.BuildGenerationRuntimes, 644–694](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/50.ProductHost/CapabilityServing/CapabilityServingCompositionRoot.cs#L644-L694):

1. Регистр требует `TransformerEmbeddingKnnLogicalUnit<string>` и проверяет identity Unit и manifest.
2. Вызывает `unit.GetServingRuntime()`.
3. Создаёт `new TextGenerator(retainedRuntime.Model, retainedRuntime.Tokenizer)`.
4. Для сценария кэширует `CapabilityServingGenerationRuntime`, который содержит **модель и генератор**, но здесь **не выполняет `unit.ExecuteAsync` и не передаёт индекс/hit evidence в `TextGenerator`**.

[GenerateAsync, 503–638](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/50.ProductHost/CapabilityServing/CapabilityServingCompositionRoot.cs#L503-L638) принимает пользовательский `UserRequest`, токенизирует его, проверяет длину, вызывает `runtime.Runner(runtime.Generator,...)`. Дефолтный [RunTextGenerator, 694–706](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/50.ProductHost/CapabilityServing/CapabilityServingCompositionRoot.cs#L694-L706) вызывает `generator.Generate(prompt,...)`.

В отдельном маршруте [доказательная проверка RunAsync, 465–501](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/50.ProductHost/CapabilityServing/CapabilityServingCompositionRoot.cs#L465-L501) требует одновременно `retrieval_unit_selected=True` и `learned_unit_selected=False` (это **другой путь**, не гарантия learned KNN в `GenerateAsync`).

### Точное следствие

Выученный nearest-neighbour label или память `example.Output` может увеличивать качество [TransformerEmbeddingKnnLogicalUnit.ExecuteAsync, 166–206](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L166-L206), **не изменяя GenerateAsync**, потому что вызов генерации проходит напрямую через текстовую модель. Поэтому оценка пользовательского качества на `GenerateAsync` *сама по себе* не измеряет, насколько обучился KNN Unit. Обратное тоже верно: высокая KNN accuracy не означает улучшения свободного ответа Student.

Это **не доказанный баг проектирования**: ProductHost явно охраняет `no-training boundary` и может специально предоставлять отдельную «чистую генерацию». Вопрос к требованиям — какой маршрут назван конечным пользовательским `learning recipient` и где должна происходить композиция Unit/Flow/learned evidence. Нельзя насильно подключать KNN к endpoint, пока этот product decision не сверён с действующим контрактом.

## N18-B — подтверждённая проверяемая возможность ложной метрики «KNN-память принята приложением»

Если metric/promotion говорит `KNN trained`, а пользователь обращается к `GenerateAsync`, отсутствие uplift может быть **несоответствием consumer** — даже когда обе подсистемы по отдельности работают. Список `UnitIds` или `LlmIngredientManifest` не является proof of invocation. Нужна *живая трасса*: `request_id → route_id → unit_execute_invoked → retrieved_example_id → selected_output → final_user_output`. Если значимые поля для чистой генерации пусты, её нельзя считать оценкой применения KNN.

## MICRO-CONSUMER-01 — проверка влияния learned memory на именно пользовательский ответ

**Минимальная постановка:** 8 новых проверенных уроков с source-disjoint примерами и 8–12 blind heldout задачами. Использовать готовый [15-E51A source-disjoint fixture](15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json), если его output contract подходит к Unit; иначе выбрать один EN→RU Unit с известным обучаемым `TrainOnExample` из [TranslationTrainedUnits](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L445). Новые DPO здесь не создаются. **Нельзя** проверять на тех же exact тренировочных inputs.

Один frozen `policy_sha`, одна и та же генерация/decoding, один `unit_id`, один law oracle, фиксированный heldout:

| Arm | Память Unit | Реальный путь | Что проверяет |
| --- | --- | --- | --- |
| G0 | не вызывается | `GenerateAsync` как есть | чистая free-generation база |
| U0 | обучена 8 lawful lessons | прямой `ExecuteAsync` | способен ли Unit давать качественный heldout output |
| U1 | те же lessons | **explicit shadow compositor**: `GenerateAsync` + фактический `ExecuteAsync`, law-aware selection (оба результата логируются) | влияет ли доступ learned memory на **final user answer**, не переписывая production host |
| Ubad | те же embeddings/inputs, shuffled *training outputs only* | тот же compositor | отрицательный контроль: выигрыш связан с учебной меткой, а не с жёстким правилом или oracle в оценщике |

**Первичный результат:** blind `correct lawful final answer / heldout`; отдельно `KNN hit@k`, error severity, coverage, abstain, retrieved example IDs, время. **Ключевой контраст U1↔Ubad**, где меняются только выученные labels; G0↔U1 — влияние **подключения** памяти, а не изменения весов. `G0` не обязан измениться после Unit.Train: это ожидаемый negative control для generation route. Никакой хороший результат U1 не докажет научение LoRA, если `policy_sha` одинаков.

**Условия опровержения:** если U0 не улучшает heldout или U1 не влияет на final user answer относительно G0 при валидном Unit output — данная composition не дала качества на выборке; нельзя объявлять «одна лишь KNN решает проблему». Если U1 выигрывает над G0 и Ubad с реальным `unit_execute_invoked=true`, подтвердится **пользовательское влияние применения memory**; причинность отсутствия RP01 free-generation gain требует отдельного сопоставления actual RP01 route.

**Бюджет:** 1 короткое Train memory + 8–12×4 вызовов, 0 optimizer steps, 0 Windows workflow, 0 широких regressions; запуск только локально на выбранном существующем harness.

## Архитектурный барьер перед исправлением

Если intended product route = *чистая генерация*, сохранить `GenerateAsync` неизменным, а learned composition вынести в явный `AnswerWithEvidence` c JSON trace. Если intended product route = *адаптивный обучаемый ассистент*, определить authoritatively where Unit/Flow and law arbiter enter, затем доказать `actual consumer` before/after. Избегать shortcut «возвращать ground truth из теста», как в уже выявленной исторической E51B-ветви.

**Итог по статусам:** Route split **C**, историческая модель/Unit learning **M/C по отдельным источникам**, качество от добавления memory **H / NOT MEASURED**. Сборка, запуск генератора, независимый CODEX/Astra review здесь **НЕ проводились**.
