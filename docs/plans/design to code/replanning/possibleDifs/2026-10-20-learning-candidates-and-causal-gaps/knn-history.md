# KNN, структурные признаки и сохранённые положительные результаты

Дата: 20.10.2026. Режим: чтение исходников и существующих результатов, документирование. Новых запусков, обучения, C# parser, inference, продуктовых изменений или правок защищённых карточек в этом подаудите нет.

Проверенный core: `9622b575a6aabb79939b12cc9f5d96313f1bf370`; planning corpus для проверки новизны: `de270758087502a350c4fcf460acdf1638bdd318`; соседняя ветка `language_translate`: `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`.

## 1. N-KU-01 действительно затрагивает обучение и заслуживает статуса кандидата

По исходникам подтверждена цепь **исходный C# → учебная метка/признаки → Train → сохранённые пары → Predict → совет**. Это не отдельное сравнение значений Label вне learner. [Экспериментальный вход L135–168][nku-entry] сохраняет настоящую подготовку строк и порог из [Facade TRAIN L194–203][facade] и [reader L327–333][facade-reader]; `_train(examples)` изменяет память, последующий `_predict` читает именно её. Копии A/B создают отдельные `FileActionKnnPredictor(k:3)`.

Сопоставление сохранённых source files: нынешний продуктовый `FileActionKnn.cs` и A отличаются только namespace на строке6. A и B отличаются namespace и строкой53: B добавляет `SyntaxKind.DoStatement` к условию существующего цикла. Ни feature extractor, ни cosine, ни k, ни голосование между плечами не изменились. [A][nku-a], [B][nku-b], [original L21–159][action].

Сохранённый [отчёт N-KU-01][nku-report] фиксирует положительное изменение: ложные советы ADD_LOOP на KQ1–KQ3 **3→0**, выполнение заданного требования на четырёхслучайной панели **1/4→3/4** (KQ1–KQ3 требуют NOT_ADD_LOOP; это не exact-label accuracy); одновременно полезный ADD_LOOP на KQ4 **1/1→0/1**. У обоих вариантов BEFORE-память пуста и facade-guard возвращает null. Это успех по устранению ложных советов в этом C# сценарии; ухудшение полноты полезных советов остаётся рядом. По правилу пользователя потеря одной метрики не аннулирует измеренное улучшение другой.

Три метки одного класса из четырёх при k=3 математически создают постоянное большинство: A отвечает ADD_LOOP, B — ADD_METHOD. Этот факт уже честно указан в отчёте; он ограничивает утверждение об обобщении, но не отменяет настоящий эффект изменения учебной метки на последующий learned answer. Опытом не проверяется весь `Compile`, автоматическое включение feature flag или RP01 DPO/LoRA.

**Решение для документации:** сохранить N-KU-01 как `POSITIVE_METRIC_CANDIDATE_WITH_TRADEOFF`, связать с уже созданным [product-change-proposal.diff][nku-diff]. Предложение продукта — добавление DoStatement в original `ActionLabelGenerator.Label`; самостоятельно переносить его в production или прогонять полную цепочку сейчас не требуется. Наличие этого кандидата не означает решения пользователя о внедрении.

Не объявлять новыми гипотезы «нет DoStatement в 30D features», «нужен ForEachVariableStatement», «k3 при4 уроках даёт большинство»: эти ограничения/продолжения уже есть в текущем отчёте и прежних possibleDifs.

## 2. Что подтвердили первичные исторические результаты

В таблице приведены сохранённые числа из первичных JSON. Повторный запуск не делался. Сохранённый JSON доказывает наличие исторического результата; он не аттестует, что сегодняшняя сборка выполнит тот же код после всех последующих изменений. `passed` — историческое поле программы с прежним threshold, а не действующее пользовательское определение успеха.

| Механизм, домен и источник | До → после | Что реально обучается / граница | Остальные показатели |
|---|---|---|---|
| [E09, 7 мая][e09-result], C# структурный next-AST target | top1 `0.2593→0.3607` | StructuralKnn хранит vector→next-kind пары; receipt: 5806 узлов, 4643 train pairs. Это структурная задача, не свободная генерация кода | recorded time `5.8→635 ms`; условия timed участков и разные размеры eval в source не позволяют выдавать эти числа за полный lifecycle cost |
| [E13, 7 мая][e13-result], C# следующий тип метода | top1 `0.2857→0.7143` | 48 методов,31 соседняя пара,24 trainPairs в notes. Поздний положительный результат нельзя заменять ранним `0.3333→0.3333` | time `1.9→7.8 ms`; baseline и enhanced строят TRAIN из разных единиц — пар против префикса allMethods, см. [E13 L49–78][e13-source] |
| [E14, 7 мая tier2][e14-result], C# file action | top1 `0.75→1.0` на4 validation files | 16 TRAIN /4 VAL; learned memory воспроизводит автоматические action labels | time `5.4→7.9 ms`. Это маленький scope. При [tier3][e14-result2]/[tier4][e14-result3] того же архива top1 `0.9247→0.9247`, time хуже; положительный tier2 и отсутствие прироста tier3 показывать раздельно |
| [E18, 7 мая tier2][e18-result], маршрутизация prompt | top1 `0.825→0.85`, несмотря `passed=false` | 200 examples /40 VAL, +1 верный ответ на этой панели; локальный положительный результат по точности | time `0.3→12.7 ms`; ранние/поздние receipt не являются независимыми репликациями одной панели |
| [E22, 7 мая tier3][e22-result], predicates→dataset buckets | macro-F1 `0.4709→0.5646` | 360 TRAIN /90 VAL. Положительный KNN результат сохранён; полное отождествление corpus и labels старой/новой реализации отсутствует | inference timing `0.1→100.5 ms`. F1 по TypeScript/Tests=0. Положительный общий F1 не даёт успех всем доменам |
| [E25, 7 мая][e25-result], порядок POS | top1 `0.3438→0.875` | 128 TRAIN /32 VAL из160 синтетических POS последовательностей; соседняя ветка хранит пары и использует Jaccard POS-биграмм | inference timing `0.4→6.6 ms`; hard pattern accuracy `24/28≈0.8571`; не полное качество перевода |
| [E26, 7 мая tier1][e26-result], RU→EN выбор смысла слова | disambiguation `0.4→1.0` на15 случаях | `BuildDefault` заполняет ручной словарь, затем сравниваются два reader; в этом E26 run нет этапа Train.6 hard cases. Это положительный контекстный результат, **не before/after обучения**, не аналог DPO весов Student | time `0.7→1.5 ms`. Тот же набор в нескольких tiers не считать ростом покрытия |

E26 сохранён здесь как исторический контекст, чтобы не потерять пользу POS признаков. [E26.Run L20–46][e26-source] и [BuildDefault L27–63][e26-dict] не дают считать эту строку выполненным обучающим экспериментом; новый неучебный повтор не предлагается. Для будущего learning опыта нужен настоящий изменяемый получатель урока и последующий reader.

Дополнительный полезный пример: E22 1 мая имеет macro-F1 `0.4892→0.4944`, хотя `passed=false`. По действующему правилу это положительное измеренное изменение данной метрики на данной панели; оно не становится доказательством большого обобщения. [Первичный receipt][e22-small].

### Перенос структурных функций в KNN

[E07][e07-source] строит многомерную n-gram таблицу по BPE и AST metadata. [E09][e09-source] обучает KNN на структурных AST-векторах. [E07 поздний tier2 receipt][e07-result]: `0.1502→0.1310`; E09 поздний положительный receipt приведён выше. Это доказательства того, что отрицательный результат одного способа включения структуры **не является общим запретом** полезных структурных признаков. Но это не чистый причинный опыт «та же функция в матрице против KNN»: различаются target, representation, split и размер eval. Название «matrix→KNN» не должно скрывать эту границу.

Текущий `E09StructuralKnn` обучает по `train.Zip(train.Skip(1))`; baseline оценивает весь val, enhanced — первые500. `BpeId` здесь фактически локальный индекс SyntaxKind для каждого файла. Эти известные ограничения сопоставимости сохраняются; они не отменяют сам исторический положительный receipt и не доказывают, что он переносится на произвольный домен.

В current product [StructuralKnn][structural] по-прежнему присутствуют syntactic role, три AST ancestor types, position, scope/depth. Учебный caller [TrainStructuralKnn][facade] строит within-file pairs, а consumer работает при UseStructuralKnn и достаточной памяти. Для текста на естественном языке нужны свои POS/порядок/роль/отрицание; копирование Roslyn признаков в перевод не следует из C# успеха.

### Реальный donor/current разрыв E25

В `language_translate` [PatternClassifier L29–49][donor-pattern] содержит `_train`, `Train`, kNN по POS-биграммам. В current [29.SemanticCompiler/Translation/PatternClassifier L24–65][current-pattern] используются только эвристические word sets/положение первого глагола. В комментарии current сохраняется ссылка на исторический +154.6%, хотя сама эта функция не читает память E25. Это подтверждённый разрыв механизма, уже известный семье P28/P107/P122; его нельзя повторно продавать как новую гипотезу. Восстановительная карточка должна называть настоящего learned consumer, а не только добавлять название KNN.

## 3. Новое уточнение известного gate: sparse batch может стереть ранее выученное

**ID `KH-EXT-01`; статус `EXTENSION_OF_KNOWN_MECHANISM`, SOURCE_CONFIRMED / QUALITY_NOT_RUN.**

Ранее [LT-N02, смежный gate L48][prior-method-gate] уже обнаружил несовпадение единиц: training admission считает методы, reader — пары. Поэтому это не новая фундаментальная гипотеза. Новое уточнение для покрытия — последовательный T1→T2 сценарий сохранности уже выученной памяти.

Цепь на current source:

1. [Facade L178–190][facade] накапливает все методы batch и вызывает `_methodKnn.Train(allMethods)` при **allMethods.Count≥4**.
2. [MethodKnn.Train L119–130][method] сначала `_pairs.Clear()`, затем добавляет только соседей одного FileId с последовательными IndexInFile. Ни HasTrainingData, ни отказа при недостаточном количестве пригодных пар внутри этого метода нет.
3. Четыре файла по одному методу дают4 allMethods, но0 допустимых пар. Ранее непустая `_pairs` будет очищена. Четыре метода в одном файле дают3 пары; reader тоже закрыт.
4. [Facade L314–323][facade-reader] возвращает null при `_methodKnn.TrainingSize<4`; свойство [MethodKnn L187–189][method] возвращает именно `_pairs.Count`.
5. Объект `_methodKnn` сохраняется внутри facade между вызовами, [поле L22, constructor L42][facade-fields]. Значит повторное обучение того же экземпляра может отключить ранее доступный learned answer.

В этом аудите не запускались parser/Train/Predict, поэтому не заявляется измеренное падение. Условия риска — **ранее обученный экземпляр**, новый batch≥4 методов, но<4 eligible pairs, включённый consumer. Если callers всегда присылают полноценный replacement corpus, проблема может не проявиться; активный повторный caller в RP01 не доказан. У MethodKnn флаг `UseMethodKnn=true` по умолчанию ([Options L17–18][options]); это отличается от opt-in FileAction/Structural. Одного default flag недостаточно для непустой памяти: нужен настоящий Train caller. В прочитанном подмножестве исходников установлен public `TrainMethodKnn` и последующий `Compile` consumer, но не найден отдельный production caller, который гарантированно делает T1 и T2 на одном экземпляре.

Прямое сравнение донорских FileActionKnn, MethodKnn и Facade с current после удаления5 copyright lines дало полное равенство текста. Это унаследованный риск; не заявлять donor→current регрессию.

**Адресное предлагаемое исправление:** до разрушительного `Train` посчитать пригодные пары по тому же FileId/IndexInFile условию; при count<4 оставить прежнюю память и сообщить `RejectedInsufficientPairs` в лабораторном receipt. Product admission/result API выбирать только после решения пользователя. Не менять learner на append одновременно: append — другое вмешательство и другая политика памяти.

**Малый протокол, только описание.** Изолированная копия существующего MethodKnn и узкий adapter двух facade methods; production не менять. T1: один файл с5 методами, все successor labels Loop, дающий4 pairs; T2: четыре отдельных файла по одному методу,0 pairs. Оба плеча получают тот же T1/T2. A — нынешний methods-count admission; B — eligible-pairs admission, сохраняющий T1 память при T2. Снимки A0/A1/A2 и B0/B1/B2; четыре frozen C# query sources с Loop next-kind contract, фактический reader и guard. Критерий — retained correct answers/non-null coverage после T2 относительно собственного T1; одновременно размеры памяти, rejected update count, T1 acquisition, Train time/allocations. Любое измеренное улучшение полезной метрики — успех этой метрики/домена; accuracy growth сверх T1 не обязателен. Нельзя считать один подсчёт0/4pairs отдельным learning экспериментом без настоящего T1→T2→reader.

Этот протокол — дополнение к известной семье LT-N02; его нельзя включать в число совершенно новых гипотез. Полная отдельная карточка с exact files, class, project, source copies, данными, командами и конфликтами: [method-memory-card.md](method-memory-card.md). Сейчас новых runs не назначено.

## 4. Новый прямой кандидат улучшения стоимости учебного входа

**ID `KH-COST-01`: два Roslyn parse на один FileAction учебный source. Статус SOURCE_CONFIRMED / HYPOTHESIS / NOT_RUN.**

В [Facade L196–199][facade] каждый TRAIN source независимо передаётся `FeatureVector(source)` и `Label(source)`. [FileActionKnn L24 и L78][action] вызывают `CSharpSyntaxTree.ParseText(source).GetRoot()` в обоих методах. Одна синтаксическая структура строится дважды внутри настоящей подготовки обучающих rows; эти rows затем передаются `FileActionKnnPredictor.Train`. Это не внеучебная проверка и не benchmark произвольного parser.

**Гипотеза:** в одном training row использовать одно immutable SyntaxNode root для обеих функций; уменьшить allocations/CPU подготовки и обучения при том же тексте, тех же labels, том же30D vector, k и голосовании. Влияние на accuracy не требуется для успеха по стоимости обучения. Само отсутствие второго вызова parser подтверждает механизм, но сокращение wall-time/allocations в конкретном runtime ещё не измерено.

Предлагаемое изменение original после решения пользователя: overload `Label(SyntaxNode root)` и `FeatureVector(SyntaxNode root)` в FileActionKnn; существующие string overloads сохраняются для остальных callers. В `TrainFileActionKnn` parse один раз на source, использовать root overloads. Другие callers не переподключать до выбранного product scope. Не смешивать с DoStatement fix,31-м признаком, иной null-label логикой или изменением k.

**Граница новизны:** финальный адресный поиск по всем **662 Markdown possibleDifs** на planning pin и **296 Markdown** защищённого core corpus проверил имена `FileActionKnn`, `TrainFileActionKnn`, `ActionLabelGenerator`, `FeatureVector(source)`, `Label(source)`, `ParseText`, сочетания duplicate/double/twice/reuse/cache/shared parse и русские двойной/повторный/общий разбор. Точная гипотеза о двух parse на один реально обучаемый row не обнаружена. Ближайшие документы — LT-N01/LT-N02 и N48 false-label scope: они обсуждают признаки/метки, не повторный разбор и стоимость подготовки Train. Это ограниченная новизна в указанном corpus, не абсолютная новизна во всех ветках/внешних обсуждениях. Полное последовательное чтение958 документов не заявляется; выполнен полнотекстовый адресный поиск и чтение релевантных совпадений.

Полная карточка с точными путями, бюджетом, командами и разрешением конфликтов находится в отдельном документе `knn-cost-card.md`. Она только документирует возможный следующий опыт; существующий успешный N-KU-01 не повторяется.

## 5. Проверка существующего покрытия

Полнотекстовая сверка662 planning и296 protected Markdown отделяет три статуса:

| Деталь | Существующее покрытие | Действие этого пакета |
|---|---|---|
| DoStatement в teacher label FileAction | N-KU-01 card/implementation/raw/report уже существуют | Source verification и сохранение положительного кандидата; не новый опыт |
| Methods-count против pairs-count | LT-N02 L48 уже описал gate; P91 и прочие MethodKnn cards описывают другие splits/labels | KH-EXT-01 добавляет точный sequential retention protocol. Не заявляется совершенно новая причина |
| Два ParseText на одну реально обучаемую строку FileAction | Адресного опыта с этим фактором не найдено в662+296 MD | KH-COST-01 как новая небольшая карточка вне защищённой папки; NOT_RUN |
| E25 POS learned predictor заменён heuristics | P28/P107/P122 и donor audits уже описывают разрыв | Сохранены pinned source links и первичный положительный receipt; не добавлять дублирующую гипотезу |
| E26 контекстный словарь | Существующий положительный receipt и prior interpretation | Явно отделена context comparison от training experiment; не повторять read-only benchmark |

Это targeted coverage выбранных KNN деталей, не заявление о проверке содержания каждого из125 каталогизированных экспериментов. Полный coverage всего пользовательского запроса ведётся в общем пакете.

## 6. Источники и применённые правила

Использованы `learning-path-audit` (прочитан полностью), `architect-behavior-classifier` (doc-first, evidence, scope/claim классы), `trainable-architecture-decision` (mechanism/owner/consumer/lifecycle). Общие product-проверки скил не превращены в запрет маленького изолированного опыта: текущие11 правил пользователя выше них. Родитель ведёт общий coverage/protocol audit.

[facade]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L178-L223
[method]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/MethodKnn.cs#L119-L189
[action]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/FileActionKnn.cs#L21-L159
[structural]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Prediction/StructuralKnn.cs#L26-L177
[nku-entry]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/Rp01Micro/NKu01/NKu01FileActionEntry.cs#L97-L168
[nku-a]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/Rp01Micro/NKu01/VariantA/FileActionKnn.cs#L6-L159
[nku-b]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/Rp01Micro/NKu01/VariantB/FileActionKnn.cs#L6-L159
[nku-report]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md
[nku-diff]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/product-change-proposal.diff
[e09-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E09-STRUCTURAL-KNN/results/run-2026-05-07-001.json#L5-L17
[e13-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E13-METHOD-KNN/results/run-2026-05-07-001.json#L5-L17
[e14-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E14-CONTEXT-ACTION-KNN/results/run-2026-05-07-001.json#L5-L17
[e18-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E18-DOMAIN-CLASSIFIER/results/run-2026-05-07-002.json#L5-L17
[e22-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json#L5-L17
[e22-small]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-002.json#L5-L17
[e25-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E25-PATTERN-CLASSIFIER/results/run-2026-05-07-001.json#L5-L17
[e26-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-07-001.json#L5-L17
[e07-source]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L41-L105
[e09-source]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L83
[e13-source]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/E13MethodLevelKnn.cs#L49-L78
[donor-pattern]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L81
[current-pattern]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/29.SemanticCompiler/Translation/PatternClassifier.cs#L24-L65
[prior-method-gate]: https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md#L48

[facade-reader]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L313-L333
[facade-fields]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerFacade.cs#L17-L43

[options]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/SemanticCompilerOptions.cs#L11-L21

[e07-result]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json#L5-L17
[e14-result2]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E14-CONTEXT-ACTION-KNN/results/run-2026-05-07-002.json#L5-L17
[e14-result3]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E14-CONTEXT-ACTION-KNN/results/run-2026-05-07-003.json#L5-L17
[e26-source]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L20-L46
[e26-dict]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/12.SemanticCompiler/Translation/ContextualDictionary.cs#L27-L63
