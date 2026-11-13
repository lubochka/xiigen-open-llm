# MICRO38 — три быстрых независимых causal probes для потерянных C# KNN-функций

<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:START -->
## POST102 — один фактор и действующий оценщик полного набора обязательств

**NAMED_CHECK: POST102-E13-ONE-FACTOR, 02.11.2026.** Применяется отдельно к MICRO38-A/B/C и L2. [Адресный аудит POST102](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md) усиливает приёмку; исторические размеры панелей и роли моделей ниже не назначают новые исполнения. Текущего исполнителя, состояние и уже разрешённый бюджет определяет действующий паспорт, не название модели из примера.

**WRONG:** в одном плече исправить ExpressionBody, split, tie-break и multi-label, затем приписать рост AST-признаку; считать непустой KindBag или один найденный label доказательством learned gain. **CORRECT:** таблица diff каждого плеча содержит ровно свой extractor/feature mapping/target-schema фактор. Version Roslyn, train pair IDs, family-disjoint holdout, k, distance, tie-break и scorer зафиксированы; исправление общего split сначала отражается в исходном состоянии всех сравниваемых плеч, а не скрывается в B. Отдельно проверяется потребность downstream в multi-label: корректный single-label контракт сам по себе не дефект.

**Проверка L0/L1:** исходный JS proxy не заменяет Roslyn. У A меняются только ExpressionBody cases, block-bodied контроли сохраняются; у B обобщение lambda-kind не стирает arity-dependent закон. Для C эталоны и реальные outputs оценивает один callable по полному множеству obligations: gold `{Guard, Async}` не принимается за `{Guard}` и не принимает лишнюю несовместимую обязанность; порядок одинакового множества не меняет verdict. Тот же scorer считает set-exact, missing/extra и индивидуальные wrong→right/right→wrong; изменение готового булева поля не проверяет его. Constant/majority и уже назначенный shuffled-label контроль отделяются от прироста относительно A; дорогие дополнительные контроли не придумываются после прогона.

**Предел и продолжение L2:** назвать реальный вызов consumer, фактический loader памяти, schema/version, hash и сохранённые ответы до/после чтения. Наличие файла или локальной копии predictor не доказывает cold restore в продукте. Корректные ответы свежего reader сравниваются только в отдельно назначенном чтении; новый Train ради восстановления результата запрещён. Владелец MICRO38 сначала делает таблицу неизменяемых условий и связывает каждый вывод с имеющимся evidence; цена включает все прежние подготовки и вызовы, неполные сведения отмечаются UNKNOWN. Без обычного потребителя вывод остаётся на уровне E13, без причинного объяснения RP01.
<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:END -->

**Дата:** 16.10.2026. **Цель:** подтвердить/опровергнуть, что N38-01 (expression-bodied), N38-02-L (lambda-family), N38-02-C (composite-label) влияют на качество настоящего learned E13 MethodKnn, а не только меняют набор признаков. Статус: **PROTOCOL_READY / SOURCE-LOGIC PROXY RUN / NATIVE DOTNET NOT_RUN / END-TO-END NOT_MEASURED**. Приоритет — первый информативный микроопыт, не пересборка 500 DPO и не Windows CI.

## Принципы причинного сравнения и владения

- Зафиксировать конкретные git SHA core и planning, blob hash MethodKnn.cs, version Roslyn; исходный train/holdout corpus; paths→family ids→SHA; входное упорядочивание, tokenizer/encoder (для E13 не нужен BPE); k=5; tie-break; seed; mapping AST SyntaxKind→features; hash snapshot/restore.
- Никакой тестовый пример из закрытого holdout не может участвовать в train, family-based split по исходному проекту/шаблону обязателен. Если старый E13 Tier2 corpus не восстановлен, написать NEW PILOT, не выдавать 2/7→5/7 за повтор.
- Один фактор на опыт. **A0 frozen baseline** = существующий MethodExtractor + MethodKnnPredictor. Каждая B-ветка меняет лишь ОДИН extractor / feature mapping / target schema при **тех же gold, train IDs, test IDs, k, optimizer=none, model=none, bytes of source examples**. Для vector-only условий отдельный нейтральный точный Jaccard oracle, чтобы не вмешалась ANN stochastic topology.
- Мета-условие обучения: сравнить BEFORE empty memory (no learned neighbor), AFTER train и SHUFFLED LABELS negative control. Без роста на новых family-disjoint заданиях статус "представление улучшено, качество не доказано".
- Нельзя одновременно переписывать source parser, label schema и E13 split. На обычном потребителе сначала подтвердить его фактический вызов, иначе результат относится исключительно к E13 learned component.

## Этап L0 — source-contract: 4 C# methods, ≤ один Native .NET small run

С помощью Microsoft.CodeAnalysis.CSharp ParseText на одном файле (отдельно проверить semantic compilability с minimal stubs) извлечь:

1. expression-bodied async method: async Task<int> Fetch() => await ReadAsync();
2. expression-bodied call method: int Get() => Calculate();
3. block-bodied async method: async Task<int> Fetch2() { return await ReadAsync(); }
4. block-bodied call method: int Get2() { return Calculate(); }

Указать using System.Threading.Tasks и фиктивные методы ReadAsync/Calculate для compile validity. Для каждого вывести Roslyn SyntaxKind, Body==null, ExpressionBody!=null, legacy KindBag/PackedString, enhanced KindBag/packed, pairwise Jaccard. В legacy ожидаются KindBag=[] для строк 1/2, distance=0; enhanced для строки 1 [Await,Call] и строки 2 [Call], distance=0.5. Block-bodied negative controls должны сохранить старый результат. Поскольку в сессии доступен только Node, уже выполнен **JS shadow source logic**, а не Roslyn. Его checksum/result опубликованы отдельно. Если Roslyn L0 не подтвердит пустоту, остановиться: SOURCE_INFERENCE_REFUTED.

### MICRO38-A — только новое чтение ExpressionBody

- **Train:** 12–24 C# методов (6–12 family pairs) с выражениями, guard, async/await, void event handlers и ordinary invocation; **heldout:** 12–24 новых family-disjoint похожих ситуаций; ≥ 6 expression-bodied и ≥ 6 block-bodied контролей.
- **A:** точный E13 extractor: Body или пусто.
- **B:** если Body is null и ExpressionBody is not null, и только тогда заменить пустоту набором структурных descendants ExpressionBody.Expression.DescendantNodesAndSelf(), применив тот же AbbreviateKind и сортировку сохранения; иначе путь должен быть byte-equivalent A.
- **Gold:** next-method CanonicalType на следующем реальном исходном методе (не подставлять query answer как label). Для оценки семантической корректности дополнительно CODEX/Astra 6 UltraCode проверяет source-meaning/expected policy отдельно от Roslyn label.
- **Primary:** paired heldout top1 on expression-bodied positions и change in wrong-law route; **secondary:** empty-bag rate, exact nearest-id changes, macro-F1, retention on block-bodied, latency, Jaccard margin. Запись по каждому case, не только средние значения.
- **Pass:** не просто непустой вектор. B должен изменить learned decision к правильной метке у source-disjoint expression cases без регрессии block controls; если sample tiny — только предварительный сигнал, не production gate.

### MICRO38-B — только lambda-kind feature canonicalization

- Подтвердить реальные Roslyn kind names SimpleLambdaExpression и ParenthesizedLambdaExpression в 4-8 C# methods.
- **A:** текущие признаки SimpleLambda/ParenthesizedLambda.
- **B1:** оба → Lambda, raw arity удалён только в эксперименте.
- **B2:** общий Lambda плюс отдельно arity/role feature, сохраняющий важный синтаксис. B1 и B2 — разные arms, нельзя делать их результаты причинным доказательством друг друга.
- **Corpus:** 8–12 paired train + 8–12 source-family-disjoint heldout lambda-use contexts; 50% same-intent/different syntax, 50% arity-dependent law. Измерить neighbor ids, Jaccard distance, correct next-law label и **false merge of arity-dependent cases**.
- **Interpretation:** B2 улучшает одинаковый intent и не портит различающиеся arity → потенциальный перенос. B1 улучшает recall, но допускает wrong-law → NOT_ACCEPTED. Нет changed decisions → FEATURE_SPACE_ONLY.

### MICRO38-C — только multi-label next-method obligations

- Зафиксировать тот же parser/extractor: не добавлять ExpressionBody и lambda changes из A/B.
- Создать sealed ontology [Guard, Async, ErrorHandling, Loop, Simple/Mixed] с gold multi-hot для Next method. У одного next method могут законно быть сразу Guard+Async или Async+ErrorHandling. Требование из кодовой задачи/метода должно независимо указывать, какие obligations действительно значимы.
- **A:** существующий single CanonicalType, end-to-end проверяется как один label.
- **B:** KNN над теми же Current.KindBag/Jaccard, но голосование по каждой frozen multi-hot bit, threshold фиксирован по train-only; никакого добавочного rule hardcode в обычный вывод. **Контроль:** shuffled multi-hot labels при той же памяти/метрике.
- **Train/Holdout:** ≥12/12 source-family-disjoint method pairs с ≥4 composite heldout. Мерить set-exact accuracy, micro/macro-F1, false missing obligations (в частности auth/await/dispose), legacy top1 и latency. Единичный рост recall за счёт неконтролируемых лишних labels не принимается.
- **Важно:** прежний single-label task может быть корректен по своему контракту. До доказанной потребности downstream multi-label — это **не production bug**, а гипотеза о неспособности использовать learned Unit для более сложной задачи.

## Этап L2 — только после положительного L1: обычный продуктовый маршрут

Повторить минимально на реальном consumer (CodeDocs / Design→Code) с **одной новой разрешённой обучающей парой** и locked exam families: parent / enhanced / label-shuffle / no-learning. Обязательные record: learner_id; train state hash before/after; actual query; retrieval candidate IDs/distances; Unit output; routing/flow; final answer; compile/domain rubric; retention across old C# families; time/memory cost; restored-cold result. Если current RP01 Student не вызывает E13 learned Unit — пометка NOT_CONNECTED_TO_RP01, нельзя приписывать ему изменение 0/3.

## Минимальный результат и порядок решения

| Запуск | Когда прекращать | Что доказывает |
| --- | --- | --- |
| L0 Roslyn AST | 4 метода достаточно; если false, остановить MICRO38-A | Факт feature erase |
| L1-A 12+12 E13 | При отсутствии изменений выбора — stop | Возможный causal KNN gain для expression-bodied |
| L1-B 8+8 | Если отождествление arity ломает закон — reject B1 | Обобщение lambda feature vs false merge |
| L1-C 12+12 | Если downstream не нуждается в composition — stop | Составные learned obligations |
| L2 consumer | Только один подтверждённый победитель | Можно ли вернуть реальные правильные ответы |

## Evidence hygiene, human-model roles, review

**Frozen RP01 scientific results и старые E13 receipts не изменять.** Отличать SOURCE_CONFIRMED / PROXY_RUN / NATIVE_PASS / LEARNING_EFFECT / ORDINARY_CONSUMER_EFFECT / INDEPENDENT_REVIEW_PASS. Тест не равно качество. Агенты-проверяющие должны проверить: честный same-case split, compiler validity, отсутствие false laws, отдельные domain applicability, нет переписывания labels post hoc, нет смешения двух гипотез.

Для DPO-only последующего опыта: **CODEX с Astra 6 UltraCode** исполняет работу человека по выбору lessons; **внешняя открытая китайская модель** генерирует обе стороны final chosen/rejected; **CODEX с Astra 6 UltraCode** отдельно оценивает качество самой пары и независимые outputs Student. В этом MICRO38 новая DPO-пара НЕ СОЗДАВАЛАСЬ и evaluator Astra НЕ ВЫЗЫВАЛСЯ.

Предметные находки: [N38-01](38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md), [N38-02](38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md). Исторический baseline и split caveat: [N17](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md).
