# kNN, предметные представления и превращение урока в работающий юнит

Самое существенное различие маршрутов: прежний capability-путь использовал обученную память примеров, представление трансформера, нормализованные корни, BM25 и форму фразы. Нынешний simulation-прогон RP01 проверяет обучение токенной policy и её свободный ответ. Успех первого механизма не переносится автоматически на второй; отсутствие гибридного пути во втором не доказывает, что он бесполезен.

## Г-01. Найден реальный прежний путь функций kNN; точное воспоминание «из матрицы в функцию» пока не локализовано полностью

Причинная проверка механизмов: [Э-01/02/03](05-small-discriminating-experiments.md). Историческое происхождение и качество — два открытых вопроса.

Первичная цепочка документов вне репозитория:

- `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator implementation/PLAN-KNN-MATRIX-INTEGRATION-v1.md:5–26`: два исходных сигнала — transformer embedding 0,55 и lexical score 0,45; описаны пропуски E15-нормализации, E18 и третьего E25-сигнала.
- `.../PLAN-KNN-MATRIX-INTEGRATION-v3.md:18–29,71–80`: различены два потребителя — `MultiSubPhaseRouterUnit → HybridRanker` и `GeneratedCapabilityFlow → TransformerEmbeddingKnnLogicalUnit.ScoreCandidates`. Текст описывает недоступный trained-классификатор и отсутствие нормализации в обоих путях. Это историческая диагностика, не заявление о нынешней неисправности.
- `.../experiment_knn_state.json:2–37,116–128`, дата 7 мая: состав уже 0,45/0,35/0,20, E15/project-23 roots и E25; отмечено подключение classifier как `ILogicalUnit` и накопление его примеров. Это исторический реестр, проверенный ниже по коду; его слово `proven` само по себе не доказательство.

Найденная git-линия:

1. `dc1eed214d` — 5 мая, BPE + предикаты + HNSW в `KnnLogicalUnit`.
2. `10b665a67d` — интеграция положительных experiment optimizations в trainer, learned units и orchestrator.
3. `a98afb7ccf065bf626dcf619c57c1d30863df85b` — 7 мая, compiled prompt, E15/E25 ranking, перевод E18/E24/E51A на соответствующие пути.
4. `57ecddffdc` — перенос файлов обучения из проекта 21 в `18.CapabilityTraining`; в показанном git stat файлы trainer/units перемещены без изменения содержимого. Это перенос проекта, а не доказанный перенос алгоритма «из матрицы в функцию».
5. `61d25ab5c7` — исправлены комментарии: `Return the selected stored example output verbatim` вместо заявления о новом learned output. Показанный diff меняет описание, не алгоритм выбора.
6. В текущем коде функции сохранились: [LlmIngredientLogicalUnits.cs](../../../18.CapabilityTraining/LlmIngredientLogicalUnits.cs):217–293. `git branch --all --contains a98afb7ccf` включает текущую ветку.

**Классификация:** подтверждён существующий исторический и текущий гибридный маршрут. Версия «весь этот код потерян при переносе веток» опровергнута для перечисленного commit. Версия «нынешний RP01 не использует этот маршрут» подтверждается ниже. **Точная** родословная названных Любой экспериментов, ошибочно оценённых как неудачные именно из-за помещения в матрицу, остаётся не установленной. Эти найденные семейства — кандидаты, не подмена её воспоминания.

Самая дешёвая недостающая проверка: найти один конкретный старый experiment ID/дифф переноса по уже найденным документам и соседним refs, затем связать его с поздним raw результатом. Новая широкая серия прогонов для этого не нужна. Никакой ремонт не рекомендуется до определения этого конкретного донора.

## Г-02. В удачном E34V2 подбирался вес функции смешивания; текущий consumer использует фиксированные веса и другой набор сигналов

Причинная проверка: [Э-02/02b](05-small-discriminating-experiments.md), experimental pending; кандидаты и признаки внутри контраста одинаковы.

Человеческий эффект: прежний алгоритм мог научиться отдавать почти весь вес предметным тегам, если они лучше различали правильный эпизод. Нынешняя функция выбора capability не подбирает этот вес на данных: её коэффициенты заданы в коде.

Старое первичное измерение: [E34V2 run 30 апреля](../../../35.ExperimentsV2/results/E34V2-HYBRID-MATCHER/results/run-2026-04-30-001.json):5–17 — `hit_at_1`, 0,7222 → 0,7778, относительные +7,69%, `bestAlpha=0.10`, train 72 / val 18. [Поздний run 1 мая](../../../35.ExperimentsV2/results/E34V2-HYBRID-MATCHER/results/run-2026-05-01-001.json):7–17 содержит тот же результат. Это первичные сохранённые JSON, не число из плана.

Старый алгоритм: [E34HybridMatcher.cs](../../../35.ExperimentsV2/Experiments/E34HybridMatcher.cs):88–112 соединяет нормированный BM25 и максимум Jaccard по `FamilyCollapser`-тегам; строки 115–133 перебирают alpha от 0,1 до 0,9 по первой половине validation. При alpha 0,1 веса составляют BM25 0,1 и tag kNN 0,9. Это настройка функции на данных, хотя в ней нет нейронного DPO-градиента.

Важное ограничение результата: в `a91e9efcec:35.ExperimentsV2/Experiments/E34HybridMatcher.cs:28–35` baseline считается на всех 18 val-строках; `:41–52` alpha настраивается на первой половине, enhanced оценивается на второй. Значит +7,69% не является чистым парным эффектом на одних и тех же заданиях. Это конкретный дефект сравнения в той версии, а не основание вычеркнуть метод. Дальнейший одинаковый результат в мае не исправляет определение метрики.

Текущий путь: [LlmIngredientLogicalUnits.cs](../../../18.CapabilityTraining/LlmIngredientLogicalUnits.cs):181–203 — embedding → HNSW candidates → `ScoreCandidates` → сохранённый `example.Output`. В строках 225–238 функция использует cosine-подобный vector score, lexical BM25 и совпадение `PatternClassifier`; точная формула: `vectorScore * 0.45 + normalizedLexical * 0.35 + patternScore * 0.20`. В :276–293 используется `TrainingVectorBuilder.NormalizeRoots`. Поэтому это **не тот же алгоритм**, что E34V2 с tuned alpha и Jaccard-тегами, несмотря на общий маркер E34V2.

Классификация: **подтверждённое различие алгоритмов**; возможная потеря полезной настройки, но причинная связь с RP01 пока гипотеза. Контрдоказательство простой версии «забыли всю семантическую обработку»: roots и pattern-сигнал реально присутствуют. Контрдоказательство объяснению текущей свободной генерации: этот selector в ней вообще не вызван.

Самый дешёвый различающий вопрос: на одинаковых сохранённых кандидатах и запросах из первичного успешного опыта меняет ли выбор замена fixed fusion на его tuned функцию? Сначала можно пересчитать сохранённые scores; если их нет, понадобится разрешённый отдельно малый вызов существующего scorer. Условный минимальный ремонт — перенос только подтверждённой функции/параметров в её реальный consumer с сохранением версии пространства, не замена всего обучения.

## Г-03. Структурный kNN — предметная функция расстояния, а не только число соседей

Причинная проверка: [Э-01/03/15](05-small-discriminating-experiments.md), отдельно для порядка действий и C# AST, experimental pending.

Старый E09 использовал пять вещественных координат AST и HNSW, затем голосование: [E09ContextVectorKnn.cs](../../../34.Experiments/Experiments/E09ContextVectorKnn.cs):11–21,64–79. Новый structural kNN хранит другой набор признаков: тип узла, три родителя, позицию, siblings, глубину, scope, file ID и синтаксическую роль. Его [StructuralKnn.cs](../../../12.SemanticCompiler/Prediction/StructuralKnn.cs):12–35 задаёт специальное расстояние: несовпадение роли стоит 4, первого родителя — 3, типа узла — 2, второго родителя — 2, третьего — 1,5; позиция и глубины добавляют малые штрафы. Не все объявленные поля участвуют в расстоянии: например FileId и SiblingCount в этой формуле не используются.

Маршрут: `Train/TrainFromSource` сохраняет пары «контекст узла → следующий узел» (:116–141); `Predict` вычисляет это расстояние, берёт ближайшие и голосует (:145–160). История переноса в production: `27ae03266a` и `08ce8832c6` для structural/method/file-action функций. Для method kNN используется Jaccard по `KindBag`, [MethodKnn.cs](../../../12.SemanticCompiler/Prediction/MethodKnn.cs):120–155.

Это реальное обучение памяти примеров с предметной функцией. В просмотренном коде коэффициенты расстояния и k фиксированы при создании; градиентного обучения расстояния, LoRA-проекции этой функции или динамической k-policy здесь не найдено. Это ограничение именно этих классов, не всех kNN проекта.

Классификация: **подтверждённое различие представлений и функций**. Возможная польза такого структурного донора для задач кода требует сохранения входного AST и правильного потребителя. Он не получает отношения субъекта/объекта переводимого предложения автоматически. Минимальный опровергающий шаг — проверить, различает ли эта функция критический контраст нужного домена до обучения; условный ремонт возможен только в соответствующем доменном mapper, а не добавлением имени kNN к RP01.

## Г-04. В capability-пути урок раскладывается по ветвям и становится памятью юнита; simulation RP01 обходится без этой композиции

Причинная проверка: [Э-06/07/08](05-small-discriminating-experiments.md), experimental pending.

Сейчас в [DefaultLogicalCapabilityTrainer.cs](../../../18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs):524–543 каждая branch plan даёт `TrainingText`, `ChosenText`, `RejectedText`, `TeachingVector`; затем добавляется replay. После обучения (:949–969) `BuildKnnTrainingCorpus` создаёт корпус, а `TransformerEmbeddingKnnLogicalUnit.Train` строит рабочую retrieval-функцию на обученной модели. В :3481–3501 в память юнита попадают training text, **chosen output**, predicate IDs и domain tags. Создание `GeneratedCapabilityFlowFactory` находится в :1067; здесь есть путь от обучения к unit/flow registration.

Здесь два разных эффекта: изменение параметров модели и сохранение правильных ответов в памяти. `TransformerEmbeddingKnnLogicalUnit.Train` (:114–153 в файле units) вычисляет embeddings и строит HNSW; его собственный manifest честно указывает `LoraAdapterCount:0`, `DpoStepCount:0`. Это не означает, что переданная модель никогда не обучалась: обучение находится у предыдущего owner. Исполнение (:198–203) выдаёт сохранённый ответ выбранного примера, а не свободно сочинённый текст. Поэтому успешное поведение такого юнита — реальный результат системы, но для отделения вклада DPO от памяти нужен отдельный контраст.

У нынешнего RP01 иной конкретный маршрут: [CodeDocumentationSimulationDpoExperiment.cs](../../../14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs):114–130 превращает плоские simulation-пары в exact token batches; :160–186 вызывает один LoRA trainer по минибатчам. Здесь нет вызова `DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync`, создания корпуса capability или `ScoreCandidates`. [Evaluation.cs](../../../14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs):80–96 вызывает likelihood и прямую генерацию. Для EN→RU [ResearchCheckpointEnRuAdapter.cs](../../../27.TranslationDomain/ResearchCheckpointEnRuAdapter.cs):25–37 вызывает `_student.Generate`, сохраняя даже неразборчивый ответ.

**Классификация:** подтверждённое различие действующих маршрутов. Нынешний free-answer результат не проверяет успешность прежней композиции unit/flow, памяти и функций kNN. Но отсутствие graph/unit decomposition не объясняет само по себе малую величину LoRA-градиента; это отдельная гипотеза о полезном системном ответе. Правильное сохранение DPO тройки не означает её автоматическое разложение в обучаемые подфункции графа.

Самый дешёвый будущий контраст: на одном и том же замороженном checkpoint и наборе входов сравнить прямой Student с уже существующим capability-consumer; отдельно удержать память постоянной при сравнении до/после DPO. Только это различит «потеряли полезный маршрут» и «параметры не приобрели нужный навык». Условный ремонт — восстановить доказанный consumer bridge, если именно он был заказан и его польза подтвердится. Никакого нового graph framework из этих фактов не следует.

## Где проходит граница детерминизма

Воспроизводимость kNN не делает его бесполезным. Обучение BPE, пополнение памяти, подбор alpha, нейронное изменение embeddings и фиксированная функция смешивания — разные механизмы. В прочитанном коде установлены:

| Механизм | Что меняется от данных | Что фиксировано | Реальный выход |
|---|---|---|---|
| `KnnLogicalUnit` | BPE vocabulary и HNSW-примеры | hash buckets, вес predicate=2, k при создании | output ближайшего примера |
| Structural/method kNN | память структурных пар | формула distance/Jaccard и voting | следующий класс/тип |
| E34V2 | индекс и tuned alpha | FamilyCollapser/Jaccard/BM25-формулы | ранжированный тип эпизода |
| Transformer capability | переданная модель + embeddings примеров | fusion 0,45/0,35/0,20, pattern rule, confidence transform | сохранённый chosen output |
| RP01 simulation | разрешённые LoRA-матрицы | reference, tokenizer, доменный prompt contract | token likelihood / свободный ответ |

Ни один из этих механизмов не следует объявлять «всем kNN проекта». Проверка иных доменных learned distance/projection/negative-memory функций остаётся в карте покрытия; этого расследования недостаточно, чтобы объявить их отсутствующими во всех ветках.

## Г-05. Новый первичный след: отрицательный E07 не проверял заявленное добавление parent

По указателю помощника прочитан [E07StructuralContextToken.cs](../../../35.ExperimentsV2/Experiments/E07StructuralContextToken.cs):87–113,138–225,275–315 и snapshot `88a880abf4`. Сообщение tier2 говорит `ast_parent added`, но learner использует один ключ `(BpeId, AstNodeType, AstDepth)`; parent/file/scope не входят в решение. BPE ordinal применяется к карте равномерных символьных позиций, не к настоящим token offsets. [Raw tier2](../../../35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json):7–17 сообщает .1502→.131, −12,78% с этим описанием. Snapshot 7 мая подтверждён; точный build 1 мая не установлен.

Классификация: source-расхождение описания и алгоритма, потенциально неверная интерпретация отрицательного опыта. Оно совместимо с уточнением Любы, но не устанавливает literal E07→kNN port. Получатель здесь N-gram next-token, а не RP01 generator. Контрдоказательство чрезмерному выводу: даже исправленная структура может не помочь при разреженной памяти. [Э-03/03b](05-small-discriminating-experiments.md) по одному меняют alignment и parent key; quality-effect experimental pending. Подробные источники — H-01 в [06](06-helper-findings-reconciliation.md).

## Г-06. E51 подтверждает иной результат обучения: выбранные предикаты и маршрут

Первичные archived MD на `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` сообщают E51A overlap .205→1 и 100% trace на 13 V; E51B 100% на 4 TE и 2 CH. [E51A](../../../35.ExperimentsV2/Experiments/E51ACodeContextPipeline.cs):254–275: compiled prompt → domain → Jaccard neighbors → `best.RequiredPredicates`. :854–893 показывает знакомый train CodeContext у всех 13 V. [E51B](../../../35.ExperimentsV2/Experiments/E51BDynamicFlowMiniE2E.cs):210–240 реально выбирает subject/arbiter, но четыре содержательных поля возвращает из `ExpectedHeldOutTraces`; :247 выбирает особую ветку по CH-ID. Diff выбранного ref до HEAD у этих двух файлов — только license headers.

Классификация: реальные сохранённые положительные результаты узкой памяти/routing и source-ограничение интерпретации. Копирование oracle не равно обучению generated Flow; повтор context не проверяет неизвестный код. Это не отрицание полезности kNN. Actual RP01 simulation не вызывает эти experiment consumers. [Э-06/07](05-small-discriminating-experiments.md) проверяют unseen contexts, memory-label dependence, настоящий output supplier и ID invariance; experimental pending. Минимальное исправление зависит от того, какой consumer действительно должен решать задачу, а не от замены названия PASS.

## Г-07. Переводческая память имеет дополнительные функции, которые не создаёт один DPO update

[TranslationTrainedUnits.cs](../../../27.TranslationDomain/TranslationTrainedUnits.cs):70–114 хранит tokenizer corrections; :232–259 — shared KNN, word/phrase/exact maps; :403–450 `TrainOnExample` записывает chosen в exact/signature/prefix/structure память и word mappings. Gender alternate — явно детерминированное расширение примера. Source подтверждает разные получатели и механизмы; весь Execute/verifier/restore в этом проходе не прочитан, успешный отдельный EN→RU quality-run к ним здесь не привязан.

Классификация: подтверждённое наличие доменной памяти и функций; её отсутствие в прямом `ResearchCheckpointEnRuAdapter → _student.Generate` — различие путей. Причинная польза для нынешнего EN→RU experimental pending [Э-08/09](05-small-discriminating-experiments.md). Контрдоказательство простой версии «подключить kNN и всё заработает»: неправильная память/сигнатура может копировать ошибочный перевод; нужен matched role/negation экзамен. Условный ремонт — только подтверждённый bridge/представление, без объявления rule augmentation обучением LoRA.
