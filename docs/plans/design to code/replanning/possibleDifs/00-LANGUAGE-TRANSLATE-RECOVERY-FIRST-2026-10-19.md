# Сначала восстановить обучение из language_translate

Дата: 19 октября 2026. Приоритетная глава для RP01 и каталога possibleDifs.

**Первое действие — воспроизвести положительные механизмы именно из language_translate, затем проверить их нынешний получатель Unit/Flow.** Ниже отделены реально сохранённые успехи, различия переноса, уже исправленные ошибки и новые причинные гипотезы. Этот пакет содержит исследование и проекты небольших опытов; исправление качества Student в нём не объявляется выполненным.

## 1. Какой успех восстанавливаем первым

Сравнение закреплено за language_translate **f6032aff81f1f83de7c09cf9b9efe6b058dd12b9** и codex/dpo-implementation **a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6**. Проценты ниже взяты из JSON, прочитанных непосредственно в первом snapshot. Они не перенесены из другой ветки и не являются результатами нового запуска. Наличие JSON и source в одном snapshot не аттестует соответствие бинарника исторического запуска этому source.

| Очередь | Проверенный донор из language_translate | Положительный сохранённый результат | Какой механизм возвращать в проверяемый путь | Первый небольшой опыт |
|---|---|---|---|---|
| 1 | E25, обученная память POS-последовательностей | 34,38% → 87,5%; validation 32 | Train сохраняет примеры; Predict выбирает k=3 по Jaccard POS-биграммам и голосует по меткам структуры | R25-MEMORY: без памяти / после Train / переставленные метки на одной новой панели; затем отдельный R25-CONSUMER |
| 2 | E26, словарь с левым и правым POS-контекстом | 40% → 100%; 15 фиксированных случаев | Выбор значения по двум сторонам контекста и приоритету; это ближайшая запись, не голосование k соседей | R26-CONTEXT: одна память, baseline selector против contextual selector, затем передача полей в Unit |
| 3 | E09, структурная функция KNN для C# | 24,13% → 35,07% | AST-признаки поступают в обученный StructuralKnnPredictor и читаются нужным потребителем | R09-CONSUMER: целые файлы отдельно в TRAIN/EXAM, единые labels, одинаковая панель |
| 4 | E43, старые примеры при новом обучении памяти | Retention 32,26% → 39,78% | Сохранение старой обученной памяти при добавлении новых данных | R43-RETENTION: новый корпус против того же корпуса плюс фиксированная часть старого; старые и новые задачи отдельно |
| 5 | Ветвевые примеры logical capability trainer | Source diff: branchPlans → registrationPlans в live KNN | Восстановить различимость ветвей и полный корпус, сохранив нынешнюю проверку уникальных ExampleId | R-GRANULARITY / R-CORPUS: без изменения нейронных весов |
| 6 | Полное состояние конкретного обученного владельца | В современной линии есть positive memory и fresh-process receipts | Проверить corpus/правила/индекс Unit отдельно от base/LoRA/optimizer/RNG Student | R-RESTORE: тот же набор через новый процесс; отдельное продолжение одного обновления |

Источники четырёх архивов: [E25][e25], [E26][e26], [E09][e09], [E43][e43]. Исходники функций: [PatternClassifier][pc12], [ContextualDictionary][dict12], [StructuralKnn][structural12], [E43 driver][e43driver]. Гранулярность: [baseline caller][oldgroup], [current caller][newgroup]. Все протоколы — в [03-SMALL-DOMAIN-EXPERIMENTS.md](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/03-SMALL-DOMAIN-EXPERIMENTS.md).

### Что ограничивает эти числа

E25 измеряет классификацию синтаксической структуры на синтетических POS-примерах, а не полный перевод. E26 использует заранее заполненный словарь и те же 15 тестовых случаев; это положительный результат контекстной функции, без доказательства нового градиентного обучения. В E09 baseline оценивает весь validation, а KNN — первые 500: при новом сравнении нужна одна панель. E43 переобучает bigram-count память; из него не следует, что 30% replay оптимальны для LoRA или другого домена. Эти ограничения задают честный способ воспроизведения и сохраняют полезный результат каждого механизма. [E25 driver][e25driver], [E26 driver][e26driver], [E09 driver][e09driver], [E43 driver][e43driver].

## 2. Почему важен путь обучения, а не только файл с похожим именем

У E25 есть обучаемый класс в проекте 12. Найденный нынешний transformer-KNN consumer использует другой класс, SemanticCompiler29.PatternClassifier: фиксированные словари слов и условия, без Train и памяти восьми структур. Результат E25 в комментарии этого класса не доказывает перенос алгоритма. Сам такой разрыв уже описан другими исследователями и существовал до RP01; здесь он привязан к запрошенному language_translate snapshot. [Обучаемый класс][pc12], [правила проекта 29][pc29], [fusion consumer][fusion], [предыдущий разбор][prevpos].

При этом общий KnnLogicalUnit, MethodKnn, FileActionKnn и DomainClassifierUnit совпадают между изученными ветками после пяти строк copyright; основная формула transformer-KNN тоже не была заменена. Следовательно, первая проверка — какие примеры, признаки и обученные состояния действительно используются. Общий откат KNN-формулы не следует из обнаруженного diff. [Сравнение и точные границы](./48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/01-MECHANISMS-AND-EVIDENCE.md).

**LoRA назад не откатывать.** Старый LoraApplicator создавал внешние адаптеры без показанного подключения к model forward; старый DPO backward не вызывал logits.Backward. Нынешняя цепочка содержит применение адаптера и обратное распространение. Положительные результаты памяти не требуют возвращения этих старых недостатков. [Старое применение LoRA][oldlora], [нынешний forward][loraforward], [старый backward][oldbackward], [нынешний backward][newbackward].

## 3. Что уже работало в соседней, более поздней линии

В pinned language_translate нет каталога 27.TranslationDomain. Поэтому современные TranslationMachine/translation-unit receipts нельзя автоматически назвать историей этой ветки. Их стоит использовать как отдельный положительный эталон действующего обучения памяти. [Корень language_translate][langtree].

Сохранённый native опыт 18 октября сравнивал одни 8 уроков и 24 задания. Исправление передачи phrase_mappings через реальный reader и builder изменило результат **11/24 → 19/24**; новые комбинации — **0/8 → 8/8**. До исправления материализовалось 0 частей фраз, после — 16. Тот же Domain DLL обучался и отвечал через ordinary orchestrator. Это конкретное доказательство влияния доставки обучающих полей. В данном snapshot исправление уже выполнено: повторно записывать его в backlog как отсутствующее нельзя. Полный CLI с registry/manifest в том опыте не запускался. [Архив ingress][ingress].

Есть и ограничение: неверно принятых ответов стало **1 → 3**, крайние случаи остались **0/5**. Например, отрицание могло исчезнуть из команды. Поэтому восстановление памяти следует проверять вместе с условиями применения выученного правила, а не только по числу принятых ответов. Отдельный cold-process опыт восстановил сохранённое состояние P125 B_after и сохранил качество **19/24** и дал **24/24 совпадений ответов и маршрутов**; последнее означает паритет восстановления, а не 100% правильных переводов. [Ingress][ingress], [P125 memory][memory125], [P25 cold restore][cold25].

## 4. Новые гипотезы после recovery-очереди

Основной поиск новизны охватил 665 извлечённых текстовых файлов прежних possibleDifs/core исследований, включая 168 Markdown во вложенном experimets. Перед публикацией сверены ещё 26 новых файлов других агентов в двух последовательных обновлениях ветки; объединённый перечень — 691 разных файлов. Общие темы scope, replay-density и численной устойчивости уже известны; новыми названы только следующие конкретные цепочки.

| ID | Что обнаружено | Почему это может ухудшать обучение | Статус и очередь |
|---|---|---|---|
| N48-01 | Null-guard в невызванной lambda/local function подавляет ADD_NULL_CHECK для незащищённого внешнего dereference | Неправильная метка попадает в E14 KNN; тот же генератор делает validation gold, скрывая ошибку обучения | Новый конкретный producer defect при заданном C# contract; inherited; после проверки достижимости |
| N48-02 | Конечный FP32 gradient может сделать AdamW second moment бесконечным при конечном весе; restore сохраняет это состояние | Адаптивное обновление координаты становится нулевым; изменение веса только от decay может маскировать остановку обучения | Новая численная цепочка; арифметический proxy выполнен, native/качество не проверены; условный P2 |
| N48-03 | Один урок строит TeachingVector по typed compilation при current и по расширенному тексту metadata при replay | При разном числе ненулевых slots меняется относительная сила пар в gradient mixture | Новое уточнение P06/N26 и N38/P89; inherited; сначала trace, без заявления о доказанном росте loss или ухудшении |

Новые соседние находки по [отсутствующему/упорядоченному контексту E26][latee26], [старому R1/R2 routing contract][laterouting] и [семантическим ограничениям ProductHeldOut][lateeval] сохранены в общей карте с их авторством. Также сверены шесть механизмов свежего [соседнего пакета 47][late47]; наши три цепочки с ними не смешиваются.

Полные доказательства, контрпримеры, варианты опровержения и граница новизны: [02-NEW-HYPOTHESES.md](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md). Все три остаются гипотезами **предметного влияния**; ни одна не объявлена установленной причиной RP01.

## 5. Что означает результат нынешнего RP01

Первичный EN→RU500 report подтверждает 500 пар, 50 новых updates, 16 изменившихся LoRA tensors и 39 неизменных base tensors; полезные свободные ответы остались **0/4 → 0/4**. Исходный B0 был случайным и необученным. Конфигурация — vocab 629, hidden 512, четыре слоя, context 2048, LoRA rank 8 / alpha 16 / dropout 0 / q_proj,v_proj. Это самостоятельный нейронный опыт с другим получателем и другой задачей относительно E25/E26/E09/E43. [Scientific result][rpresult], [config][rpconfig].

Поэтому после восстановления памяти нужен отдельный короткий neural контраст: способен ли actual Student переобучить 8–16 хороших пар, меняется ли нужный ответ, достаточна ли исходная компетенция B0. Он уже предусмотрен recovery-планом и не является новой находкой этого пакета. Не заменять им первые четыре донорских опыта и не повторять автоматически Main500. [RP01 recovery][rpplan].

## 6. Навигация и передача исполнителю

- [01 — Что было / что стало: механизмы и доказательства](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/01-MECHANISMS-AND-EVIDENCE.md).
- [02 — Две новые цепочки и одно уточнение replay](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md).
- [03 — Короткие доменные эксперименты, recovery первым](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/03-SMALL-DOMAIN-EXPERIMENTS.md).
- [04 — Покрытие требований, источники и независимая проверка](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/04-SOURCES-COVERAGE-AND-REVIEW.md).
- [05 — Задание CODEX на один различающий опыт](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/05-CODEX-EXPERIMENT-WORK-ORDER.md).
- [06 — Машиночитаемый перечень обследованного корпуса](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/06-CORPUS-INVENTORY.json).

Контракт создания новых DPO: **внешняя открытая китайская модель создаёт обе стороны; CODEX с astra 6 ultracode выполняет человеческий отбор и отдельную независимую оценку качества точных пар**. В этой работе такие вызовы не выполнялись; фактическая модель нынешнего аудита этим контрактом не аттестуется. Для сохранённых архивных материалов происхождение остаётся исходным.

Предметный домен — перевод, документация, программирование или оркестрация; направление EN→RU — профиль, KNN/LoRA — механизм, Unit/Flow — получатель. AST C# и порядок слов естественного языка получают разные представления. Размеры новых панелей в протоколах — небольшие стартовые бюджеты, а не нормативы числа тестов.

**Статус пакета:** исследование исходников, чтение сохранённых результатов и один явно отмеченный NumPy FP32 расчёт. Новое native обучение и предметные A/B не запускались. Изменения предназначены только для planning/possibleDifs; защищённый core/docs/plans/replanning-possible-diffs использован для чтения.

[e25]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e26]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json
[e09]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[e43]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
[pc12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L86
[dict12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/ContextualDictionary.cs#L22-L110
[structural12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/StructuralKnn.cs
[e43driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs
[oldgroup]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L284-L290
[newgroup]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L952-L969
[e25driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs
[e26driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs
[e09driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs
[pc29]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/29.SemanticCompiler/Translation/PatternClassifier.cs#L24-L65
[fusion]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L205-L300
[prevpos]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md
[oldlora]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/10.LoRA/LoraApplicator.cs#L11-L48
[loraforward]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L253-L264
[oldbackward]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/11.DPO/DpoTrainer.cs#L219-L265
[newbackward]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1072-L1121
[langtree]: https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9
[ingress]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[memory125]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[cold25]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[rpresult]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json
[rpconfig]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/config/code-documentation/generation-v1.json
[rpplan]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md

[latee26]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md

[laterouting]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md

[lateeval]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md

[late47]: https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/README.md
