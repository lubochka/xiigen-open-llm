# Сначала восстановить PrepareToGPL: различия, доказательства и короткие эксперименты

18 октября 2026. Исследование для RP01. **PrepareToGPL принят исходной веткой для восстановления прежнего поведения.** Результат — два новых конкретных стыка (U-R2, U-N3), новый условный вопрос к neural restore caller (N-L1) и независимое подтверждение U-R1, сопоставление DPO/LoRA и функций KNN, а также небольшие эксперименты по доменам.

**Наиболее предметные находки:** old manifest не содержит вновь обязательное evidence; source PairId теряется в пространстве производных Unit ExampleIds; old Translation checkpoint не соответствует topology текущей машины. Отдельно обнаружено намеренное сохранение attached LoRA при legacy base-import и риск ошибочно назвать такую операцию полным восстановлением. Для этого последнего пункта найден существующий test contract, поэтому он не объявлен дефектом codec.

Все изменения продукта ниже — **предложения**. Малые эксперименты имеют статус **NOT_RUN**: их условия, измерения и опровержения подготовлены, обучение и inference в этом исследовании не выполнялись.

## 1. В первую очередь — эксперименты восстановления PrepareToGPL

Здесь приоритет означает близость к конкретному прежнему состоянию/алгоритму и дешевизну различающего опыта. Выбирать нужно ветвь своего actual consumer: проблема TranslationMachine не становится причиной neural RP01 только потому, что обе учатся на DPO.

| Порядок | Эксперимент | Что восстанавливает / различает | Домен и получатель | Минимальный объём |
|---|---|---|---|---|
| 1 | **U-R1 — старый Translation checkpoint** | Четыре прежних executor-направления и learned maps в нынешнем runtime из шести направлений | Перевод; TranslationMachine, Units и общий KNN | Старый loader / current / одна версия миграции; 0 обучения, до 8 запросов на вариант |
| 2 | **E-POS — прежний обученный classifier в fusion** | POS-bigram kNN из E25 против вызываемой сейчас rule-версии проекта29 | Перевод; структурный признак в ранжировании ответа | 24 train +16 контрольных POS-примеров; 8 actual fusion-запросов |
| 3 | **E-AST-ROUTE / E-MATRIX-KNN** | Реальное включение E09 и эффект одного структурного представления в таблице либо функции соседства | Программирование, C#; SemanticCompiler и найденный caller | 4–8 файлов, одна панель до100 переходов; отдельно 8 предметных запросов |
| 4 | **N-L1 — явный смысл neural restore** | Full donor против намеренного base-import, сохраняющего/инициализирующего LoRA | Документация C# либо перевод; LlmModel и StartService | 2 legacy loads, 1 full save/reopen, 1 strict отказ; 0 обучения |
| 5 | **E-HEAD — прежнее место адаптации** | Output-head adaptation против q/v-LoRA на одинаковом B0; сохранить нынешний корректный backward | Один neural домен и тот же генератор ответа | 8–16 пар, 8 контрольных запросов, сопоставимая ёмкость/бюджет |
| 6 | **U-R2 — legacy manifest** | Доступность старого asset через typed publish lifecycle с правдивым происхождением | Перевод; asset-store API, отдельно от CLI continuation | 4 predicate cases и 2 read/validation calls; 0 обучения/inference |
| 7 | **U-N3, затем E-RETENTION** | Продолжение от прежнего состояния без случайного дубля и с отдельно объявленным replay | Перевод; source→Unit→KNN→continuation | Сначала P,Q и один parent; затем 8 новых +4 replay урока при нужном вопросе |

**Почему это стоит перед общей настройкой hyperparameters:** в этих опытах есть конкретный кандидат утраты — сохранённый state, используемая функция, recipient или lineage. Даже правильный DPO не компенсирует загрузку другого состояния или отсутствие нужной обученной функции в ответе. Результат восстановления старого поведения при нуле updates считается успехом совместимости; новый learning gain требует отдельного обучения и предметного сравнения.

Подробные протоколы: [U-R1/U-R2/U-N3](01-translation-restore-and-unit-lineage.md), [N-L1/E-HEAD/E-BASE](02-neural-and-lora.md), [E-POS/E-AST/E-CONTEXT/E-RETENTION](03-historical-knn-and-domain-experiments.md). Качество source DPO и его разложение проверяются в [E-DATA/E-FANOUT](04-learning-path-crosswalk.md).

## 2. Новые находки и независимо подтверждённое различие

### U-R1 — независимо подтверждённая несовместимость старой машины

При финальной сверке опубликованного каталога найден параллельный [N41-T1](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md). Поэтому U-R1 **не считается новой гипотезой относительно итогового каталога**. Наше дополнение — два exact retained artifacts, их неизменные blob SHA, уточнение history normalization и опыт без нового обучения.

В PrepareToGPL создавались `RU_EN, EN_RU, HE_EN, EN_HE`. Нынешний runtime добавляет `ES_EN, EN_ES`. Проверенный исходный translation-v1 checkpoint и его продолженная phase38 версия остались с **четырьмя** executors и с теми же Git blob SHA. Нынешний importer требует все **шесть** направлений до мутации.

Следствие по исходнику — `EXECUTOR_DIRECTIONS_INCOMPLETE` для этих старых bytes. Это прямое несовпадение данных и предусловия метода, ещё не выполненный runtime-result. Предлагается распознаваемая versioned migration, сохраняющая прежние learned maps и добавляющая только пустое состояние новых ES executors. Сохраняются нынешний preflight и защита от повреждённых checkpoint. [Точная цепочка и before/after](01-translation-restore-and-unit-lineage.md).

### U-R2 — manifest прежней модели отвергается новым контрактом

Нынешний `TranslationModelManifestContract` требует `TrainingDataEvidence`, хотя `SchemaVersion` по-прежнему1. Проверенный старый manifest этого поля не содержит и не изменился по blob SHA.

В typed publish API, если корпус прошёл предшествующий admission, предсказывается `TRANSLATION_MODEL_MANIFEST_INVALID`. CLI continuation читает manifest другим способом, поэтому U-R2 и U-R1 — **два разных барьера**. Решение — явная migration с доказательствами именно исходного корпуса и parent checkpoint; подставлять сегодняшние расширенные данные как происхождение старой памяти нельзя. [Доказательства и контроль неверного provenance](01-translation-restore-and-unit-lineage.md).

### U-N3 — исходная пара повторно становится «новой» после разложения

`BuildGeneralBatch` превращает P в `P:semantic`, `P:executor`, `P:verifier`. Эти ExampleIds попадают в checkpoint. Continuation затем сравнивает их с исходным `row.PairId=P`. Для такого составного caller прежняя пара отсутствует в наборе IDs и может снова попасть в training, при положительном `noReplayProven`.

Это **унаследованный стык**, имеющийся и в PrepareToGPL. Новизна — конкретная причина обхода no-replay upstream, а не давно известная возможность дублей в KNN. Предложена typed lineage с отдельными SourcePairId/UnitExampleId/UnitId/content hash; простое отрезание суффикса небезопасно для авторских IDs. [Путь от builder до повторной записи](01-translation-restore-and-unit-lineage.md).

### N-L1 — намеренный base-import и ожидание полного восстановления

Нынешний legacy loader пишет `_baseParameters` и сохраняет существующие A/B. Найденный тест специально требует такого поведения. Поэтому **сохранение LoRA — предусмотренный режим**, который нужно оставить.

Новый проверяемый вопрос относится к caller: может ли он принять old base-only файл как полный LoRA checkpoint и выдать слишком сильное evidence? В StartService найден fallback construct→attach→LoadCheckpoint без strict full-topology validator; fresh construction защищает его от прошлых A/B живого объекта, но созданные adapters отсутствовали в legacy файле. Для реальной проблемы требуется совместимый запечатанный manifest и caller, который действительно обещает восстановление адаптации. Наличие такого production specimen пока не установлено.

У сохранённых C50/C100 импортов RP01 есть отдельный strict v2 gate — это явное контрдоказательство распространению N-L1 на те запуски. [Код, существующий тест и небольшой опыт](02-neural-and-lora.md).

## 3. Историческое успешное обучение не потеряно из анализа

| Исторический механизм | Сохранённый положительный след | Что переносить с проверкой |
|---|---|---|
| E09 structural KNN | 0.2413→0.3507; подтверждён Git-коммит переноса в проект12 | Структурное представление, память и actual consumer; одинаковые target labels и контрольная панель |
| E25 POS classifier | 0.3438→0.875 на32 validation cases | Реальный обученный kNN, который отличается от нынешнего найденного rule-caller |
| E26 context dictionary | 0.4→1.0 на15 случаях | Левый/правый контекст и выбор значения; acquisition новых значений проверяется отдельно |
| E43 replay | Retention 0.3226→0.3978 | Явный replay и сохранение старых законов при добавлении новых |
| Phase38 Translation continuation | Два source-урока вместо одного; old exact и nonexact KNN ответ сохраняются | Состояние, его восстановление и продолжение по идентичности исходного урока |
| ProjectWiki neural donor | Сохранённые положительные preference-метрики, разные neural states в предыдущем подробном исследовании | Место адаптации, один objective и собственный ответ того же neural consumer |

Первичные receipts, смысл каждой метрики и границы сравнения приведены в [исторической главе](03-historical-knn-and-domain-experiments.md) и [нейронной главе](02-neural-and-lora.md). Ранний отрицательный E07 не отменяет поздний положительный E09. Одновременно найденный E09 порт не является завершённым доказательством, что именно тот же неудачный matrix-run превратился в успешный KNN-run: различаются target codec и панели. E-MATRIX-KNN подготовлен для проверки этого вопроса на одинаковой задаче.

У каталогов `06.VectorSearch` и `12.SemanticCompiler` одинаковые Git trees в обеих ветках. У `TranslationTrainedUnits.cs` и semantic tokenizer одинаковые blob SHA. Поэтому для этих частей особенно важно исследовать **state и внешнее использование функции**, а не приписывать регрессию переписанному внутреннему алгоритму.

Есть и **сохранённый результат реального обучения нынешней переводческой Unit**, а не только экспериментальные проценты: [8 одинаковых уроков до/после исправления доставки phrase mappings](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md) дали **11/24→19/24** правильных ответов; все8 новых сочетаний стали правильными. Одновременно неверно принятые ответы выросли **1→3**. Это уже выполненная другим исследованием правка доставки урока в Executor и ordinary Flow, с0 neural steps. Она не считается нашим новым открытием или результатом PrepareToGPL; её нужно учитывать как положительный и ограниченный образец того, какое отличие реально способно улучшить обучение.

## 4. Почему нельзя просто вернуть весь код обучения из PrepareToGPL

Сопоставление показывает полезные нынешние изменения: LoRA включена в model forward, исправлен знак производной log-probability, RmsNorm сохраняет autograd, exact DPO отделяет response от prompt, full-v2 checkpoint проверяет ключи и topology. Само AB-разложение LoRA по смыслу сохранилось. [Before/after по10 частям](02-neural-and-lora.md).

Сохранённые результаты RP01 подтверждают, что обучение действительно меняло разрешённые tensors:

| Кампания | Параметры и восстановление | Preference | Собственный ответ |
|---|---|---|---|
| C#500 | 50 updates; 16 A/B tensors changed; 39 base unchanged; weights/optimizer/RNG reopened | 16/25→16/25; mean margin +0.0012275314 | 0/3→0/3 |
| ENRU500 | 50 новых updates, global step100; 16 A/B tensors changed; 39 base unchanged; full state reopened | 12/20→12/20; mean margin +0.0001100063 | 0/4→0/4 |

Источники: [C# raw result][csharp-result], [ENRU raw result][enru-result]. Это прочитанные результаты прежних запусков, не повторные измерения этой сессии.

Следовательно, для neural RP01 остаются содержательные вопросы DPO, стартовой компетенции B0, места/масштаба адаптации, objective, tokenizer и генеративного consumer. Они уже подробно присутствуют в каталогах и связаны с этим пакетом через [карту всех осей](04-learning-path-crosswalk.md). Ни один новый restore-стык здесь не выдан за установленную единственную причину нулевого качества.

## 5. Где что читать

| Документ | Содержание |
|---|---|
| [01 — Translation restore и Unit lineage](01-translation-restore-and-unit-lineage.md) | U-R1, U-R2, U-N3; точные APIs, состояния, предложенные миграции и малые опыты |
| [02 — Neural DPO и LoRA](02-neural-and-lora.md) | Before/after Transformer/DPO/LoRA/tokenizer; N-L1, полезные repairs, E-HEAD/E-BASE |
| [03 — Исторический KNN и домены](03-historical-knn-and-domain-experiments.md) | E07/E09/E25/E26/E43, port commit, POS/AST/context/replay protocols |
| [04 — Карта всей цепочки обучения](04-learning-path-crosswalk.md) | Все оси запроса; DPO quality, vectorization, Units/Flow, scoring, E-DATA/E-FANOUT |
| [05 — Источники, новизна, review](05-sources-novelty-review.md) | Фиксированные SHA, оба каталога, пределы просмотра соседних веток, контрдоказательства и проверка документов |

Для новых DPO роли закреплены так: **внешняя открытая китайская модель создаёт chosen и rejected; CODEX (запрошенный `astra 6 ultracode`) выбирает задания и независимо оценивает качество**. Домен — перевод, программирование, документация и т. п.; EN→RU/RU→EN — профили перевода; KNN/LoRA — механизмы. Это помогает не переносить AST-улучшение на обычную фразу или показатель одного learner на ответ другого.

Основа доказательств: PrepareToGPL `31346d8928be4469b01aaf313780e8f950ff91e2`; current `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`; planning catalog `a10bbde116515b1453d9ec6bfb103725ad99d477`. Первоначальный полнотекстовый поиск охватил **544 текста**; перед публикацией дополнительно проверены **11 новых документов** и обновлённый README. Итого555 уникальных текстовых путей, глубокое чтение — адресное. U-R1 после этой сверки переведён в независимое подтверждение. Публикация ограничена этой папкой и навигацией общего README.

[csharp-result]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json
[enru-result]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json

