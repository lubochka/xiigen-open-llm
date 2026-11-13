# 01. Механизмы обучения: что было, что стало, где теряется эффект

19 октября 2026. Сначала читать [приоритетную главу](../00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md). Базовые snapshots: language_translate f6032aff81f1f83de7c09cf9b9efe6b058dd12b9; current a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6.

**Основной архитектурный риск — неполный перенос между разными обучаемыми получателями.** Правильная пара может изменить LoRA, но не память отвечающего Unit; обученная память может быть сохранена, но не прочитана consumer; положительная метрика может относиться к выбору структуры, хотя продукт проверяет свободный ответ. Ниже для каждой оси указан конкретный механизм и предел вывода.

Обозначения: **D** — исходниками подтверждено изменение между двумя snapshots; **T** — различие экспериментальной функции и её продуктового потребителя; **I** — механизм наследуется из baseline; **A** — исправление уже выполнено в изученном current; **C** — условный риск, влияние требует отдельного опыта. Ссылка на прежнюю карточку означает сохранение её авторства и статуса, а не новое открытие.

## 1. Положительные доноры и перенос функции из матрицы в KNN

| Донор / источник | Что хранится или обучается | Что на самом деле измерено | Что переносится, что требует новой проверки |
|---|---|---|---|
| E25 [JSON][e25], [source][e25driver] | Список POS-последовательностей с метками восьми структур, k=3, Jaccard POS-биграмм | 0.3438 → 0.875 на val32 из синтетических примеров | Обученная классификация структуры; отдельно text→POS и роль этой метки в выборе ответа |
| E26 [JSON][e26], [source][e26driver] | Default entries: слово, левый контекст, правый контекст, перевод, приоритет | 0.4 → 1.0 на фиксированных 15 cases | Самостоятельная полезная контекстная функция; новые значения из новых уроков этим runner не проверены |
| E09 [JSON][e09], [source][e09driver] | Структурные векторы соседних AST nodes и KNN target | 0.2413 → 0.3507; baseline и KNN используют разные размеры validation | Связь структуры с target; исправить сравнимость панели, identity labels и границы файлов в новом опыте |
| E43 [JSON][e43], [source][e43driver] | Bigram counts из новых пар и части старых | Old-panel retention 0.3226 → 0.3978 | Память старых примеров полезна; долю replay и эффект на новые навыки проверять по получателю |

E07 сохранил отрицательный результат **0.1502 → 0.131** для матричного structural-context варианта, а E09 — положительный результат структурного KNN. У ранних driver различаются представление, обучаемая функция и scorer. Следовательно, E07 не опровергает полезность AST-признаков внутри расстояния/поиска E09. При этом не установлено, что это ровно тот исторический переход, который пользователь помнит: связь «тот же корпус, тот же признак, только перенос в KNN» пока не подтверждена парным receipt. [E07 JSON][e07], [E09 source][e09driver], [существующий разбор E09][prevstruct].

Есть и другие сохранённые пересмотры: E24 был переосмыслен с многоканальной атрибуции на один домен для каждого сегмента; E32/E39 проверяли полезность выбора числа подзадач/проверок. Эти линии сохраняются в общей карте, но их улучшение не переносится на свободный ответ Student. Положительного matched receipt нового E24 consumer в прочитанном разборе нет. [E24][prevrouter], [E32/E39][prevdecomp].

## 2. Классификатор структуры: обученная память и фиксированная подстановка — T/I

**Работавшая функция проекта 12.** PatternClassifier.Train заменяет список примеров. Predict считает близость POS-биграмм, выбирает три примера и голосует. Последовательность соседних частей речи участвует в признаках; весь порядок предложения одним bag-of-words не заменён. Однако множество биграмм не сохраняет полную последовательность с произвольной дальностью и кратностью. [PatternClassifier][pc12].

**Найденный current consumer.** TransformerEmbeddingKnnLogicalUnit создаёт SemanticCompiler29.PatternClassifier. Он получает NormalizeRoots текста, определяет POS по фиксированным ActionWords/StructureWords и выдаёт несколько жёстко заданных классов. У него нет обучающей памяти E25; ветвь с POS=10 недостижима при показанном InferPos, а VSO не достигается этими условиями. Метка используется как bonus 0.20 за совпадение в fusion, рядом с vector 0.45 и lexical 0.35. Комментарий с +154.6% принадлежит другой функции. [Правила][pc29], [consumer и scoring][fusion].

**Ещё один current получатель.** TranslationSemanticTokenizer.ClassifyPattern использует свои правила firstVerb/hasPronBefore для IMPERATIVE_V/SOV/SVO. Это более поздний компонент проекта 27; он отсутствует в корне указанного baseline. Не описывать различие как удаление одного и того же класса между ветками. [Tokenizer][token27], [baseline tree][langtree].

**Следствие.** Недостаточно «поднять вес structural score»: без обученного признака это усилит те же правила. R25 сначала проверяет память на одинаковых POS, затем отдельно actual consumer при замороженном POS ingress. Различие было зарегистрировано раньше; новая здесь точная привязка к language_translate. [Предыдущая карточка][prevpos].

## 3. Контекстное значение: что именно передаётся — T/I/C

В E26 левый и правый POS-контекст хранятся раздельно; selector сравнивает обе стороны и затем priority. Поле k есть, но TranslateWithContext берёт первую отсортированную запись. Внутри каждой стороны расстояние по пересечению POS не является полным sequence matcher. Данный driver использует BuildDefault и фиксированные случаи: это контекстная память, без градиента. [ContextualDictionary][dict12], [driver][e26driver].

SemanticCompiler29.ContextualDictionary — другая статическая таблица для null/record/request с текстовыми context signals. В исследованном pipeline её обычный caller не установлен; существование файла не означает участие в RP01. Более поздний TranslationSemanticTokenizer.ResolveConcept принимает lemma, pos, **prevConcept**, lang, но не правый контекст; часть E26 mappings применяется только после Unknown в BaseConcepts. Это разные входные контракты. [Проект 29][dict29], [проект 27][token27], [ранее найденные consumers и ограничения][prevpos].

На уровне обученного ExecutorInput поле TopicDomain есть, но ранее зарегистрированные exact-memory keys его не содержат; запись chosen способна заместить другое значение того же source. Сохранение такого ключа не восстановит уже потерянную тему. Эта гипотеза относится к translation Units, не к прямому neural RP01. [Прежний разбор с цепочкой записи и чтения][prevsubunits].

**Проверка.** Один словарь, одни значения и одна панель; менять только доступность левого/правого контекста. Отдельным опытом — topic-aware memory key с положительным контролем переносимого между темами значения. Перевод технических текстов и документация многозначных терминов требуют контекста; механически добавлять переводческий POS-кодек в C# AST нельзя.

## 4. KNN: формула сохранилась, данные и условия вызова могли измениться — D/T/I

### 4.1. Что не изменилось

После удаления пяти copyright lines совпали KnnLogicalUnit, MethodKnn, FileActionKnn, DomainClassifierUnit. Для TransformerEmbeddingKnnLogicalUnit сохранены вычислительная формула, maxLen=64, HNSW m=8 / efConstruction=32 / efSearch=16 и fusion 0.45/0.35/0.20; current добавляет duplicate-ID guard и работу с runtime handle. У EmbeddingModel/MeanPooling также header-only diff. Это контрдоказательство общей формулировке «в новой ветке испортили расстояние KNN». [KnnLogicalUnit baseline][knnlogicalold], [current][knnlogical], [embedding baseline][oldembedding], [current][embedding], [pooling baseline][oldpooling], [current][pooling], [fusion][fusion].

Для воспроизводимого чтения точные пары файлов:

| Компонент | language_translate | current |
|---|---|---|
| KnnLogicalUnit | [source][knnlogicalold] | [source][knnlogical] |
| MethodKnn | [source][methodknnold] | [source][methodknn] |
| FileActionKnn | [source][actionold] | [source][action] |
| DomainClassifierUnit | [source][domainclassifierold] | [source][domainclassifier] |

Наличие неизменённой формулы не гарантирует неизменного результата: важны corpus, порядок добавления, identities, представление, learned model для embedding и активный runtime. HNSW candidate budget, близкие/одинаковые signatures и частичное добавление ранее разобраны отдельно. [Index admission][prevadmission], [signature/candidate budget][prevhnsw].

### 4.2. Конкретный diff гранулярности

| Этап | language_translate | current |
|---|---|---|
| DPO material | branchPlans | branchPlans |
| Live KNN corpus | BuildKnnTrainingExamples(branchPlans) | BuildKnnTrainingCorpus(replayManifests, registrationPlans).RuntimeExamples |
| Склейка TrainingText одной subphase | Отдельная registration projection | Склеенная registration projection стала входом live KNN |
| Duplicate ExampleId | Несколько ветвей могли иметь один capability-row-subphase; index добавлял vectors, dictionary перезаписывал значение | Train отвергает duplicates |

Источники: [baseline DPO][oldpairs], [current DPO][newpairs], [baseline live KNN][oldgroup], [current live KNN][newgroup], [baseline grouping][oldregistration], [старые identities][oldexampleids], [старый Train][oldknntrain], [нынешний Train][knntrain].

Если различающая часть последней ветви находится после первых 64 embedding tokens, объединённая строка теряет её в vector score. Lexical/structural части fusion могут частично компенсировать эффект, поэтому один prefix trace ещё не доказывает неправильный ответ. B-arm должен вернуть отдельные примеры с **разрешёнными уникальными source-bound IDs**, а не удалить duplicate guard. Если выбранный ExampleId меняется, но output одной subphase одинаков, качество ещё не улучшилось. Это известный U-N2/PG-07, повторно проверенный на точном baseline. [Предыдущий разбор][prevgroup], [prefix ограничения][prevprefix].

### 4.3. Структурный KNN может существовать и не участвовать

UseStructuralKnn=false и UseFileActionKnn=false по умолчанию, UseMethodKnn=true. Facade учитывает язык и объём памяти: StructuralKnn требует C# и не менее 10 примеров; FileActionKnn — не менее 4. Compile может поместить прогноз в результат, но следующий вызывающий слой ещё должен его прочитать. [Options][compileroptions], [train entry points][facade], [predict gates][facadeconsumer], [Compile][facadecompile].

E09 использует локальные для файла IDs SyntaxKind, названные BpeId. Это не byte-BPE neural tokenizer. Новая проверка должна использовать единый target codec, делить целые файлы до извлечения пар и строить пары внутри каждого файла. Исправление scorer/labels — отдельная подготовка сопоставимого опыта; не приписывать его прирост включению новой метрики KNN. [E09 driver][e09driver], [предыдущий разбор][prevstruct].

## 5. Полная память и optimizer replay имеют разные задачи — D/C

Current BuildKnnTrainingCorpus берёт плоские строки из выбранных replay manifests и не восстанавливает все вложенные KnnExamples этим методом. Если только этот путь формирует serving memory, часть знания может отсутствовать, хотя выбранный optimizer replay batch корректен. Это ранее известный N27/P24/P92; наличие метода не доказывает потери всего корпуса во всех callers. [Current method][newknnrestore], [предыдущая сводка][prevgroup].

Для serving memory требуется полный корпус, которым владеет данный получатель; для очередного optimizer update может использоваться ограниченная репетиционная выборка. Увеличивать один replay ratio недостаточно, если ошибка в раскрытии вложенных rows. R-CORPUS сравнивает полный permitted corpus и нынешнюю реконструкцию при одинаковых весах/индексе; R43 отдельно проверяет вклад replay в приобретение и удержание навыка.

Новое уточнение **N48-03** ниже по тракту показывает ещё один риск: один урок имеет разные recipes TeachingVector до и после serialization/replay. Это inherited поведение, а не новый факт неполной памяти; оно разобрано отдельно в [02](02-NEW-HYPOTHESES.md).

## 6. DPO → subunits: от смысловой пары до самостоятельных целей — T/I/C

RAG-план задаёт для unit-level row отдельные Level, NodeOrEdgeId, общий InputContext и provenance. Ordered pattern IDs должны сохранять порядок в chosen/rejected; служебные адреса/позиции не должны становиться подсказкой правильного ответа. Полная пара — источник, из которого необходимо получить пригодные цели **для конкретного Unit**, а не одинаковый общий текст для всех узлов. [RAG unit envelope][ragunit], [полный DPO builder plan][ragbuilder], [dynamic flow plan][ragflow].

| Возможный пробел | Конкретный механизм / контроль | Где уже доказано или зарегистрировано |
|---|---|---|
| Учат общую подфазу вместо решения | Synthetic capability route строит chosen о создании capability и rejected «не создавать»; это не прикладной ответ | [Current projector][projectcurrent], [previous review][prevgroup] |
| Отрицательная сторона не обучает Unit | Некоторые TrainOnExample сохраняют chosen и игнорируют rejected; это supervised memory, а не pairwise DPO | [TranslationTrainedUnits][trainedunits], [DPO→subunits разбор][prevsubunits] |
| Связь слов выведена из позиции | Первый допустимый source token связывается с первым словом chosen; при свободном порядке это может быть неверно | [Builder][unitbuilder], [word alignment source review][prevroles] |
| Объяснение ошибки попадает в положительную память | RuleTags/markers из rejected участвуют в построении ключей или mappings выбранного ответа | [Role/key review][prevroles], [context review][prevcontext] |
| Условие правила потеряно | Concept/context могут переставиться при строковом parser либо context не проверяется при применении | [Source-chain review][prevroles] |
| Под-Unit обучается и проверяется разными токенизаторами | Executor/Verifier local scorers могут использовать базовый tokenizer или фиксированный язык, а ordinary adapter — trained tokenizer | [Source-chain review][prevroles] |
| Адрес Unit/Flow и порядок перепутаны | Level/NodeOrEdgeId/order-level/consumes должны обозначать actual node, сохранение графа само по себе не доказывает это | [RAG envelope][ragunit], [деcomposition/consumer review][prevdecomp] |

Пример позиционной ошибки: «Книгу долго читал я» → «I read the book for a long time». Полная пара может быть правильной, а правило «первый source token → первое слово chosen» даёт книгу→I. Другой допустимый перевод «For a long time I read the book» может дать книгу→For. До заявления о вреде требуется вывести фактически созданный mapping и его origin: earlier marker/phrase/exact ветвь способна скрыть или изменить этот путь. [Разбор с контрфакторами][prevroles].

Ремонт проверяется локально: один и тот же source/chosen/rejected → A/B только одного правила projection → actual state конкретного Unit → новый source-family-disjoint запрос через ordinary adapter. Одновременная замена alignment, ключа, verifier и fallback не позволит установить причину.

## 7. DPO, LoRA и веса: сохранить нынешние исправления — D/I/C

Для response-masked exact objective предмет проверки — относительная вероятность выбранного ответа против отвергнутого, с отдельной frozen reference:

```math
L = -\log\sigma\!\left(\beta\left[
(\log\pi(c\mid p)-\log\pi(r\mid p))
-(\log\pi_{ref}(c\mid p)-\log\pi_{ref}(r\mid p))
\right]\right)
```

Формула задаёт сравнение вероятностей, а не автоматически свободное владение языком. В actual code дополнительно действуют material version, response masks, pair/vector weights и optimizer. Logical default LegacyVersion и exact RP01 нельзя считать одним режимом. [DPO StepVector][dpostep], [logical default][legacydefault].

| Узел | Baseline | Current / вывод |
|---|---|---|
| Применение LoRA | Apply создаёт adapters, но показанный model forward ими не обёрнут | Attention.ForwardProjection вызывает установленный adapter; сохранить |
| BackwardLogProbs | (softmax−oneHot)×scale; показанный метод заканчивается без logits.Backward | (oneHot−softmax)×targetScale и logits.Backward; сохранить |
| LoRA формула | Низкоранговая добавка к base projection | y=base(x)+(alpha/r)·(dropout(x)Aᵀ)Bᵀ; B нулевая при свежем создании адаптера, смысл targetModules и rank проверять по actual forward |
| AdamW | FP32 m/v | Та же арифметика, добавлены portable state и restore; новая численная гипотеза N48-02 касается значений, а не формы state |
| Нейронная компетенция | Успех конкретных старых KNN функций не является мерой генерации этого LLM | B0 RP01 случайный/необученный; пригодность q/v-only DPO на таком B0 требует маленького опыта |

Источники: [old LoRA][oldlora], [current forward][loraforward], [LoraLinear][loralinear], [old backward][oldbackward], [current backward][newbackward], [old AdamW][oldadam], [current AdamW][adam], [RP01 result][rpresult].

Сохранённые RP01 данные подтверждают: EN→RU500, 50 новых updates, global step 100, 16 LoRA tensors / 53 248 elements изменились; 39 base tensors / 11 920 896 elements сохранились. Free correctness 0/4 до и после, chosen-preferred 12/20 до и после. Это причина отдельно проверять качество и исходную компетенцию, а не отрицать факт updates. Dropout=0 задан config и required simulation preflight; гипотеза о случайной маске LoRA именно этого опыта не активна. [Result][rpresult], [config][rpconfig], [simulation][simulation].

Тема прежних положительных neural состояний не закрывается этим ограничением. В соседней wiki-линии зарегистрированы реальные checkpoint differences и положительные preference metrics; найденный предметный planner имел другой consumer. Такие результаты сохраняются в истории с точным типом успеха и не становятся автоматически donor свободной генерации. [Предыдущий первичный разбор][prevnn].

## 8. Токенизация: четыре разных пространства, четыре проверки

| Представление | Реальный смысл | Что проверять |
|---|---|---|
| Byte-BPE IDs Student | Идентичности подслов для embedding/LM head и masked response | Codec fingerprint, token→ID, encode/decode, prompt/response границы, длина обоих ответов и budget |
| Transformer embedding KNN | Вектор pooled hidden states; первые maxLen=64 tokens плюс другие fusion признаки | Какие смысловые различия реально вошли в 64 tokens, какой encoder/index использован |
| Semantic TeachingVector | Структурированные root/predicate channels длиной 60 | Recipe/version; реальные predicates; consumer может свести вектор к плотности, а не читать направление |
| AST/POS feature vector | Структура программы или предложения для предметной функции | Стабильный codec и scope, order, parent, context, совпадение training/inference semantics |

В BpeTokenizer Encode и train merge algorithm между прочитанными snapshots не найдена новая вычислительная регрессия; current добавляет durable/canonical проверки. Это не исключает несовместимых корпусов, IDs или переобучения tokenizer поверх существующих весов. Vocab 629 и успешный UTF-8 roundtrip не измеряют фрагментацию, различимость понятий или полезность для EN→RU. Проверить реальные token lengths и masks можно до нового обучения. [Old BPE][oldbpe], [current BPE][bpe], [BPE identity review][prevbpe], [config][rpconfig], [RP01 recovery][rpplan].

TeachingVector в показанном DPO consumer используется как count(abs(value)>1e-6), преобразованный в scale [1,2]. Объявлять его координатные направления обучаемой семантической геометрией этого neural шага неверно. Для KNN/другого Unit может существовать другой consumer; проверяется конкретный вызов. [VectorSignal][vectorsignal], [ранее найденный scaling counterfactor][prevscale].

## 9. Восстановление: что должно быть тем же после загрузки

| Владелец | Состояние, нужное для сохранения поведения | Что дополнительно требуется для продолжения обучения |
|---|---|---|
| Student | Base weights, LoRA topology/weights/targets, tokenizer IDs, generation framing/config | AdamW m/v/step, RNG и порядок следующих данных; фактический следующий update |
| KNN Unit | Corpus rows и outputs, IDs, feature recipe/model, index/candidate settings, correction priorities | Способ добавления/замены примера; encoder-index consistency |
| Semantic/Executor/Verifier Unit | Learned signatures, mappings, rules и их условия, direction/topic binding | Правильная запись нового урока без стирания чужого scope; согласованность разных Unit |
| Flow/Router | Выбор зарегистрированных Unit, граф/адреса, порядок и применимость | Тот же actual consumer после обновления/переоткрытия |

Совпадение shape/hash не доказывает численную валидность optimizer moments, а одинаковые weights не доказывают наличие всей памяти Unit. Это разные проверки. Для нынешнего ENRU neural archive полный reopen state заявлен и сохранён в первичном результате; для современного memory cold-run отдельно показан паритет 24/24 при качестве 19/24. Не представлять restore как повсеместно сломанный. [RP01][rpresult], [memory restore][cold25], [N48-02](02-NEW-HYPOTHESES.md).

Кандидат о фильтрации WordMap при ImportCheckpoint не найден активным в двух проверенных phase38 artifacts: все executor word_map/surface_word_map пусты. Исторический artifact с непустой отбрасываемой картой здесь не найден. Это условный риск миграции, не новая установленная потеря. [Import guard][wordimport], [границы проверки](04-SOURCES-COVERAGE-AND-REVIEW.md).

## 10. Скрытые фиксированные решения и метрики

Детерминированные операции нужны для индексации, адресов, порядка и известных законов. Пробел возникает, когда фиксированное правило подменяет объявленный обучаемый результат либо скрывает его отсутствие.

| Участок | Условный механизм ухудшения | Быстро различающий контроль |
|---|---|---|
| E14 label producer | Guard вложенного невызванного callable считается внешней защитой | Та же внешняя функция с/без uncalled guard; независимый semantic gold — N48-01 |
| EpisodeDecomposer | Табличный предел и Take могут отсечь смысловые требования; эксперимент E32 измерял другой KNN count | Несколько требований выше/ниже cap; считать сохранённые требования и actual subtasks |
| QualityOrchestrator/bridge | Перечень возможных проверок не равен их выполнению; null checker может стать quality=passed в конкретном bridge | Корректный ответ, неверный длинный ответ, unavailable checker; проверить downstream решение |
| Confidence/fallback | Фиксированные 0.85/0.86 и phrase exception не являются измеренной вероятностью правильности | Сохранить правильные/неправильные accepted отдельно; отрицание/порядок/роль в контрольных заданиях |
| Feedback/correction | Feedback существует, но имеет приоритет/ключ, по которому consumer его не читает | Один исправленный урок и новые запросы с тем же условием; trace актуального правила |
| Holdout/reporting | Local Unit accuracy, preference margin, route PASS, exact recall и free answer смешиваются | Один frozen panel с раздельными уровнями результата и raw output |

Ранее установленные цепочки и ограничения: [decomposer/quality][prevdecomp], [confidence/negation][prevconfidence], [feedback][prevfeedback], [evaluation alignment][prevquality]. Новая source-проверка E14 отделена от этих старых тем.

## 11. Предел общего вывода

Изученная совокупность поддерживает несколько конкретных причинных направлений, но не устанавливает единственную причину неудачи RP01. Прямой neural examiner, logical transformer-KNN и translation Units — разные pathways. Для каждой гипотезы требуется собственный «вход урока → запись состояния → фактический потребитель → ответ на новый случай».

Следующий шаг конкретен: [R25-MEMORY, затем R25-CONSUMER и остальные recovery-карточки](03-SMALL-DOMAIN-EXPERIMENTS.md). Новые N48 не вытесняют их из начала списка. Условный численный риск без следа в actual state не является основанием повторять большой training cycle.

[e25]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e25driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs
[e26]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json
[e26driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs
[e09]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[e09driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E09StructuralKnn.cs
[e43]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
[e43driver]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs
[e07]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json
[prevstruct]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/38-e09-transfer-labels-and-opt-in-consumer.md
[prevrouter]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/46-e24-rehabilitation-and-unused-trained-router.md
[prevdecomp]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/34-decomposition-quality-and-execution-verdicts.md
[pc12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L29-L86
[pc29]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/29.SemanticCompiler/Translation/PatternClassifier.cs#L24-L65
[fusion]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L205-L300
[token27]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationSemanticTokenizer.cs#L294-L363
[langtree]: https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9
[prevpos]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md
[dict12]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/ContextualDictionary.cs#L22-L110
[dict29]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/29.SemanticCompiler/Translation/ContextualDictionary.cs#L21-L60
[prevsubunits]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md
[knnlogicalold]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs
[knnlogical]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs
[oldembedding]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/05.Embeddings/EmbeddingModel.cs
[embedding]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/05.Embeddings/EmbeddingModel.cs
[oldpooling]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/05.Embeddings/MeanPooling.cs
[pooling]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/05.Embeddings/MeanPooling.cs
[prevadmission]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/51-vector-index-admission-and-partial-state.md
[prevhnsw]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/60-knn-signature-alias-and-candidate-budget.md
[oldpairs]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L423-L461
[newpairs]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L525-L544
[oldgroup]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L284-L290
[newgroup]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L952-L969
[oldregistration]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L836-L850
[oldexampleids]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L762-L767
[oldknntrain]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L85-L98
[knntrain]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L100-L161
[prevgroup]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-UNITS-RESTORE-2026-10-18.md
[prevprefix]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/21-transformer-knn-prefix-and-fallback.md
[compileroptions]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerOptions.cs
[facade]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L179-L215
[facadeconsumer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L295-L334
[facadecompile]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L116-L176
[newknnrestore]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L3467-L3508
[ragunit]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-07-dpo-builder.md#L241-L276
[ragbuilder]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/dpo%20legal%20build/active-codex-execution-plan-07-dpo-builder.md
[ragflow]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/plan%20the%20learning%20set/DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md
[projectcurrent]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L102-L156
[trainedunits]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationTrainedUnits.cs
[unitbuilder]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationStructuredBatchBuilder.cs
[prevroles]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/44-structured-lessons-roles-keys-and-word-alignment.md
[prevcontext]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/23-language-context-and-learned-rule-scope.md
[dpostep]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L864-L925
[legacydefault]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L456-L479
[oldlora]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/10.LoRA/LoraApplicator.cs#L11-L48
[loraforward]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L253-L264
[loralinear]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/10.LoRA/LoraLinear.cs
[oldbackward]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/11.DPO/DpoTrainer.cs#L219-L265
[newbackward]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1072-L1121
[oldadam]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/04.TrainingLoop/Optimizers/AdamW.cs#L54-L71
[adam]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Optimizers/AdamW.cs#L56-L106
[rpresult]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json
[rpconfig]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/config/code-documentation/generation-v1.json
[simulation]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L108-L156
[prevnn]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md
[oldbpe]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/BpeTokenizer.cs
[bpe]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/01.Tokenizer/BpeTokenizer.cs
[prevbpe]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/12-bpe-retrain-identity-and-consumer-causal-gap.md
[rpplan]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md
[vectorsignal]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1159-L1169
[prevscale]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/30-teaching-vector-adamw-and-current-recipient.md
[cold25]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[wordimport]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationTrainedUnits.cs#L524-L584
[prevconfidence]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/72-negation-confidence-and-e46-feedback-consumers.md
[prevfeedback]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/75-translation-feedback-learning-consumer.md
[prevquality]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/14-data-quality-and-evaluation-alignment.md

[methodknnold]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/MethodKnn.cs

[methodknn]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/MethodKnn.cs

[actionold]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/FileActionKnn.cs

[action]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/FileActionKnn.cs#L22-L72

[domainclassifierold]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/23.SemanticCompilerAdapter/DomainClassifierUnit.cs

[domainclassifier]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/23.SemanticCompilerAdapter/DomainClassifierUnit.cs
