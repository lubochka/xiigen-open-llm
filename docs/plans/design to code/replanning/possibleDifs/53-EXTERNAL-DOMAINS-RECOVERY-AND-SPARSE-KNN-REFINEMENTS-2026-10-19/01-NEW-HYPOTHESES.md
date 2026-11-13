# Три новых конкретных условия: доказательства и границы

19 октября 2026. **SOURCE_CONFIRMED / MECHANICAL_PROBE_PASSED / NATIVE_NOT_RUN / QUALITY_NOT_RUN.**

Здесь одна новая условная механистическая гипотеза и два новых конкретных уточнения известных семейств. Общие проблемы «AdamW», «confidence» и «AST» не объявляются заново найденными. Поиск проведён в обоих каталогах прежних работ; ближайшие владельцы и границы новизны приведены в [карте покрытия](03-MECHANISM-COVERAGE-AND-DEDUP.md). Приоритет восстановления успешной ветки — в [первой главе](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md).

| Условие | Что уже установлено | Чего пока не установлено |
|---|---|---|
| NEW-AGE: первый carrier градиента при старом общем возрасте AdamW | Формула и sparse continuation contract допускают отдельный возраст; математический размер первого update зависит от чужих прошлых шагов | Реальный поздно активированный параметр в dense RP01; улучшение обучения после изменения семантики |
| NEW-SCORE: сумма знаковых scores около нуля | Уверенность около60 и смена fallback при прежних IDs/rank; интерфейс не закрепляет неотрицательную массу | Установка именно этого consumer с отрицательными scores в обычном запуске; полезность большего контекста |
| NEW-POSITION: прямой ordinal AST ≥256 | Используемая позиция сливается с255; меняются ближайшие соседи в контролируемой модели | Частота таких узлов в реальном корпусе; полезность различия для обычного ответа |

Ни один из этих механизмов не объявлен новым source-diff, внесённым первым PR. Основная математика уже присутствовала в `external_domains`; для NEW-AGE дополнительно важен современный sparse continuation. Их возможное объяснение ухудшения — изменение активного пути, состояния или распределения данных. Пока этот переход не показан, это условные риски.

В этой сессии выполнены независимый просмотр кода и Python-пробы. [Исполнимый файл](evidence/mechanical-probes.py) не требует .NET, сети, моделей или дополнительных пакетов; [сырой результат](evidence/mechanical-results.json) явно отделяет математическое наблюдение от native и quality. Для KNN дополнительно [проверены15 Git blobs и пять пар исторических файлов](evidence/knn-source-integrity.json). Диагностические метки и векторы не являются DPO-материалом.

Во всех будущих опытах обе стороны DPO создаёт внешняя открытая китайская модель; CODEX с `astra 6 ultracode` исполняет человеческий выбор и независимую оценку. Здесь внешняя генерация и обучение не запускались.

## NEW-AGE. Sparse AdamW state имеет глобальный возраст, хотя отдельный параметр мог ещё не получать градиент

**Статус: NEW_CONDITIONAL_MECHANISM_CANDIDATE.** Конкретная семантика кода подтверждена; полезность изменения на реальном домене не измерена. Это унаследованная математическая особенность плюс современный контракт sparse continuation, а не заявленная регрессия neural RP01.

### Сейчас: три разных утверждения, которые нельзя смешивать

1. `AdamW.Step` увеличивает один общий `_step`, вычисляет bias-correction по нему, затем пропускает каждый параметр с `Grad == null`. Если состояние параметра ещё отсутствует, оно создаётся с нулевыми `m/v`; отдельного счётчика его наблюдений нет. [Current AdamW, 72–110](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/04.TrainingLoop/Optimizers/AdamW.cs#L72-L110). Тот же основной порядок уже был в [external_domains AdamW, 35–74](https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/04.TrainingLoop/Optimizers/AdamW.cs#L35-L74).
2. Portable state хранит общий `Step` и список entries. Экспорт пропускает параметры, для которых состояния ещё нет; импорт проверяет присланные entries и устанавливает общий `_step`, но не вводит возраст отдельного параметра. [Portable state, 78–125](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/04.TrainingLoop/Optimizers/AdamWPortableState.cs#L78-L125), [AdamW export/import, 187–309](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/04.TrainingLoop/Optimizers/AdamW.cs#L187-L309).
3. Production exact continuation **намеренно разрешает sparse optimizer map**: `OptimizerParameterMap` должен быть подмножеством `AllowedParameterMap`; runtime отдельно сверяет весь active trainable map с Allowed, затем валидирует только сохранённые entries. Комментарий прямо говорит, что unused/frozen bindings не требуют entry. [Manifest subset, 713–714](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs#L713-L714), [runtime, 751–797](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs#L751-L797), [recipient subset validation](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/11.DPO/DpoTrainer.cs#L270-L280).

### Почему это отдельный возможный провал обучения

Допустим, до первого градиента параметра другие параметры сделали 49 шагов. Его первый ненулевой градиент вычисляется с нулевыми `m/v`, но поправка считает его пятидесятым. При фактических `beta1=.9`, `beta2=.95` и первом градиенте 1 получаем:

\[
u_t=\frac{(1-\beta_1)/(1-\beta_1^t)}{\sqrt{(1-\beta_2)/(1-\beta_2^t)}+\epsilon},\qquad u_1\simeq1,\quad u_{50}\simeq0.43189.
\]

Это не означает, что любое sparse правило обязано иметь отдельный счётчик. Требуется явно выбрать смысл отсутствия градиента. При dense zero-gradient трактовке старые `m/v` должны затухать и в пропущенные шаги; нынешний код их сохраняет. При per-parameter observation трактовке возраст должен расти только при фактическом обновлении данного параметра; нынешний общий `_step` продолжает расти. Смешение этих двух семантик способно менять скорость и направление повторной адаптации, особенно после ремонта autograd или позднего включения модуля.

**Это сильнее, чем общий вопрос «сохранили ли optimizer».** Байт-в-байт восстановленный sparse файл может честно воспроизводить именно такую траекторию. Restore parity само по себе не проверяет выбранную семантику возраста. И наоборот, сам факт отсутствия entry не доказывает повреждение: код допускает действительно неиспользовавшийся параметр.

### Выполненное доказательство и предел

Файл [mechanical-probes.py](evidence/mechanical-probes.py) содержит отдельную scalar FP32 формулу. Все операции приводятся к float32; `MathF.Pow` не исполнялся, поэтому полная native побитовая идентичность не заявляется. При zero initial weight исключён weight-decay вклад. Результат:

| Предыдущие общие шаги без градиента данного параметра | Отношение первого обновления к cold first-gradient при тех же гиперпараметрах |
| ---: | ---: |
| 0 | 1.000000 |
| 1 | 0.734960 |
| 9 | 0.434945 |
| 49 | 0.431890 |
| 99 | 0.445900 |
| 999 | 0.447214 |

На шаге 50 при первом gradient carrier величина первого scalar decrement равна примерно `4.318897e-6`, тогда как у cold age1 — `9.9999997e-6` при `lr=1e-5`. Все величины finite. Отдельный отрицательный контроль включает два dense параметра на протяжении 50 шагов, причём у второго 10 gradient carriers явно равны нулю. Возраст наблюдения обоих к концу равен 50; максимальная разница `weight/m/v` между global-age и observation-age вариантами на всех шагах равна **0**. Итоговые веса `−0.0004999983357265592` и `−0.00013089959975332022` finite. Это показывает, что предложенное различие в возрасте не меняет данный dense сценарий.

Это формульные результаты про scalar update, не обучение C#, перевода или SQL. Точный вывод и отрицательный контроль сохранены в [evidence/mechanical-results.json](evidence/mechanical-results.json); native .NET равенство, compile/JIT rounding и реальная learning quality не проверялись.

### Где гипотеза не активна

Фактический RP01 EN→RU `ReadState` дополнительно требует, чтобы optimizer entries в точности совпали с 16 allowed tensors, суммарно 53 248 scalars; проверяются также finite moments. Это контрдоказательство попытке назвать sparse-missing-entry причиной уже выполненного перехода C50→100: [SimulationEnRuContinuation, 250–266](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L250-L266).

При неизменном dense q/v-LoRA графе все подключённые A/B tensors получают gradient carrier, даже если первый A-gradient равен нулю из-за B=0. `zero gradient` и `Grad == null` — разные случаи. Этот аудит не нашёл actual RP01 trace с поздним появлением нового optimizer entry.

### Малый различающий опыт SPARSE-AGE-01

**Компонентный этап, минуты.** В disposable native harness взять два скалярных `Parameter` и существующий `AdamW`. Первый получает gradient на всех 50 шагах, второй не входит в parameter list/имеет null Grad первые 49 шагов и впервые получает 1 на шаге 50. Сохранить и открыть portable state после шага 49, проверить равенство live/resumed current результатов. Отдельно сравнить тот же первый градиент нового параметра при observation age1. Контроль: оба параметра с gradient carrier с первого шага; другой контроль — явный нулевой gradient против null. Никаких production checkpoint подмен и выдуманных continuation proofs.

**Admission этап.** Сначала trace действительного production exporter→manifest→loader: какой active parameter отсутствует, почему, какие gradients он получал раньше. Если dense exact caller никогда не производит sparse карту, оставить вопрос как контрактный риск; запускать большой доменный A/B не требуется. Проверить refusal искусственно неполного *одноразового тестового* dense manifest, сохраняя все production checks.

**Предметный этап, только при найденной поздней активации.** 8 допустимых учебных пар и 8 новых source-family-disjoint задач: отдельно C# документация редких операторов/исключений и EN→RU с редкими токенами. Использовать один фактический competent base, один tokenizer, same masks/data/seed/learning-rate и один frozen reference. Изменяемый фактор — возраст обновления поздно активированного параметра, не его стартовые веса. 10 новых updates на плечо; после native наблюдения состояния измерить ordinary full-answer law correctness, 4 ранее правильно решавшихся retention cases, cold-load parity и CPU/RAM. Если не найден actual late-active параметр, не создавать отдельный продуктовый модуль ради доказательства гипотезы.

**Фальсификатор:** в настоящем caller у каждого active tensor присутствует корректно исторический state с первого шага, либо finite native A/B не даёт ожидаемой разницы under specified sparse schedule. Это снимает причинную применимость, хотя математическая особенность общего AdamW остаётся.

### Конкретный вариант исправления после подтверждения

Для dense exact continuation минимальное решение — потребовать полноту optimizer map, как уже делает RP01 simulation, и отказать до мутации при необъяснённой недостаче. Не заполнять пропуск нулями и не называть это exact continuation.

Если реальному sparse caller нужны dormant/new parameters, в существующий `AdamW` и версионированный portable state добавить per-parameter update count и выбрать observation semantics. Внешние record/serializer, manifest profile и compatibility loader обязаны переносить этот count. Миграция старого sparse файла без истории не может восстановить неизвестный возраст; это отдельный обозначенный research restart, а не silent upgrade. Архитектурно AdamW остаётся внутренней математикой существующего learner; новый semantic Unit не нужен.


## NEW-SCORE: отрицательные similarities превращают «долю уверенности» в величину около 60

### 1.1. Что действительно делает код

KnnSkillFilter получает `ISemanticSearchProvider`, запрашивает topK и считает:

```text
confidence = first.Score / (sum(all returned scores) + 1e-9)
if confidence < 0.40:
    return all eligible skills
else:
    return eligible skills whose IDs occur in search results
```

[Код фильтра, current L19–42][filter-current]. Интерфейс provider объявляет `double Score`, но не задаёт диапазон, знак, калибровку или смысл вероятности. [Контракт, current L41–45][search-contract].

Для конечной неотрицательной массы с положительной суммой отношение ограничено от 0 до 1. С отрицательными scores такого свойства нет: частичное взаимное погашение положительных и отрицательных чисел делает знаменатель малым или отрицательным. Это не NaN и не бесконечные входы: контрпример использует обычные конечные нормированные двумерные векторы.

В репозитории есть raw cosine: `Distances.CosineSimilarity` возвращает скалярное произведение, делённое на нормы. HnswIndex не переводит его в неотрицательный диапазон перед выдачей `score`. [Cosine, current L10–20][cosine]; [поиск и возврат, L99–110][hnsw-search]; [расчёт кандидатов, L208–222][hnsw-scores]. **Это доказательство допустимости знаковых similarities в библиотеке, а не доказательство реальной цепочки HNSW → KnnSkillFilter. Такая product-цепочка этим аудитом не установлена.**

Существующий AstRelevanceRanker реализует совместимый provider. При корректной однократной индексации его BM25-компоненты неотрицательны. Повторный Index одного и того же ChunkId может привести к отрицательному IDF: `_index[id]` заменяется, а `_docFreq` увеличивается; `N` остаётся числом уникальных документов. Это уже NK-01 пакета 51, здесь не объявляется новым дефектом. [Index L60–65][ast-index]; [IDF L75–93][ast-idf]; [предыдущий разбор NK-01][prior51].

### 1.2. Достижимый вызывающий код и граница вывода

ContextWindowManager вызывает введённый `ISkillFilter`, после чего передаёт оставшиеся навыки на ранжирование и в бюджет контекста. Но его значение по умолчанию — PassThroughSkillFilter. [Конструктор L22–33 и consumer L42–53][manager-current]. В существующем тесте показано явное внедрение KnnSkillFilter с заглушкой provider. [Тест current L1089–1110][filter-test].

Следовательно:

- Механизм влияет на набор навыков **при явной установке этого фильтра**.
- По умолчанию этот manager его не запускает.
- Нельзя переносить вывод на другой одноимённый skill retriever, generic KnnLogicalUnit или RP01 без трассы вызова.
- Существование совместимых интерфейсов не доказывает, что два компонента были соединены в успешном или неуспешном запуске.
- Фильтр сам не обучается. Потенциальный эффект на качество обучения возникает только через изменённый контекст выбора, составление урока или consumer ответа. До такой трассы это дефект предпосылки score-контракта, а не дефект DPO-градиента.

### 1.3. Выполненный локальный контрпример

Есть шесть допустимых навыков `skill-0 … skill-5`. Provider возвращает первые пять, все пять ID принадлежат допустимому списку. Query = `[1, 0]`; vectors = `[s, sqrt(1-s²)]`, с FP32-приведением для `s = [0.60, 0.20, -0.20, -0.29, -0.30]`. Скрипт повторяет FP32 арифметику исходной cosine-функции и затем double-формулу фильтра.

| Вариант | Наблюдение |
|---|---|
| A, текущая формула | Sum = 0.010000020265579224; confidence = 59.99987479097672; fallback=false; возвращаются 5 навыков |
| Только последний cosine изменён с ≈−0.30 на ≈−0.32 | Лучший score, IDs и порядок всех результатов прежние; сумма становится отрицательной, confidence отрицателен; fallback=true, возвращаются 6 навыков |
| B, диагностический контракт неотрицательной массы | Конечная отрицательная оценка считается неподдерживаемым входом именно для этой формулы; используется уже существующий fallback; возвращаются 6 навыков |
| Контроль B: `[.90,.04,.03,.02,.01]` | A и B одинаково выбирают top5 |
| Контроль B: `[.22,.21,.20,.19,.18]` | A и B одинаково используют fallback |

Таким образом, существующая ветка выбора может меняться около нуля **общей знаковой суммы**, хотя лучший кандидат, его score, ранги и принадлежность всех IDs не менялись. Это подтверждённое свойство локального алгоритма; «шестой навык нужен для правильного ответа» здесь не предполагалось и не измерялось.

В JSON отдельно сохранён перевод `(cosine + 1) / 2`: confidence ≈0.31936128 при тех же рангах. **Это только иллюстрация зависимости порога от смысла шкалы. Такой перевод не является калиброванной вероятностью или предложенной готовой починкой. Он может изменить решение и на полностью положительных входах; его нельзя выдавать за отрицательный контроль.** Основной B-контроль выше ничего не меняет на допустимой неотрицательной массе.

### 1.4. Чем это отличается от существующих находок

Пакет 51 NK-03 рассматривает topK глобального индекса, который не совпадает с допустимым набором навыков: высокая уверенность может привести к пустому пересечению. Здесь все пять возвращённых ID допустимы, пересечение не пусто, k/IDs/scope/rank фиксированы. Проблема находится раньше пересечения — в арифметическом смысле score. [NK-03][prior51].

NK-01 уже описывает ошибку повторного Index. Мы используем её только как известный возможный producer отрицательных чисел, не повторяем его исправление и не меняем Index в A/B score-потребителя. Core P93 рассматривает калибровку generic KnnLogicalUnit; это другой owner, хотя общая потребность в ясном смысле score родственна. [P93][prior93].

Затронутые KnnSkillFilter, ContextWindowManager и AstRelevanceRanker совпадают с external_domains после пяти строк лицензии. [Исторический фильтр][filter-external]; [исторический manager][manager-external]; [исторический AST provider][ast-external]. Не установлено, что первый PR ввёл данную формулу или изменил provider.

### 1.5. Малый причинный опыт

**T0 — 30–60 минут, настоящий фильтр, без обучения.** В существующем C# test-проекте вызвать именно KnnSkillFilter через provider с пятью вычисленными cosine-оценками и шестью допустимыми навыками. Повторить таблицу выше, затем пустой список результатов, нулевую массу и отрицательную сумму. Сначала зафиксировать score kind/domain владельца. A — текущая формула; B — только явная проверка применимости неотрицательной массы с тем же существующим fallback. IDs, ранги, k, score-значения, порядок candidates и threshold одинаковы. Сохранять все входы, raw scores, сумму, confidence, выбранные IDs и причину fallback.

B не должен менять retrieval, исправлять известный NK-01, переставлять кандидатов или вводить новый обученный классификатор. Если провайдер имеет собственную корректную калибровку, отдельным следующим опытом проверяется именно она; threshold выбирается по dev-панели, а не по итоговому test.

**T1 — только после установленного ordinary consumer.** Предметы: документация по C# и выполнение небольших задач программирования. Восемь заданий на предмет, 12 фиксированных доступных навыков, неизменный бюджет контекста. Определить выбранные/утраченные инструкции, затем качество обычного ответа. Gold и критические ошибки фиксируются до A/B; оценка вслепую CODEX с Astra 6 ultracode. Фильтр не требует DPO-обучения для этой проверки. Если параллельно измеряется learned unit, внешний китайский open model создаёт окончательные chosen/rejected, CODEX выбирает и проверяет; один и тот же замороженный набор уроков подаётся обеим рукам.

**Фальсификаторы и стоп-условия:** фильтр не установлен; provider гарантирует и проверяет неотрицательные scores; fallback не меняет доставленный контекст; на heldout исходы одинаковы. В этих случаях finding не повышается до объяснения падения качества. Одна только величина confidence>1 не доказывает, что fallback или topK полезнее для задачи.

**Метрики:** время операции и количество search/LLM calls; размер selected context; correct/critical-error/abstention по предмету; RAM индекса и суммарного процесса. Neural updates = 0 для T0/T1 без обучаемой единицы; model layers/trainable layers = n/a, Unit/Flow levels и executed depth — зафиксировать либо unchanged с причиной. Нельзя представлять время Python-модели как native стоимость или обучение.

## NEW-POSITION: используемый порядковый признак теряет различие 255/256

### 2.1. Код и непосредственный механизм

StructuralVector хранит PositionInParent в `byte`. Extract вычисляет нулевой индекс узла среди `node.Parent.ChildNodes()` и записывает `min(index, 255)`. DistanceTo добавляет 0.3, только если закодированные PositionInParent различны. [Поля и distance, current L15–35][struct-fields]; [Extract, L52–59][struct-extract].

Векторы с одинаковыми остальными признаками поэтому получают:

| Прямые позиции узлов, zero-based | Текущие закодированные позиции | Текущий вклад в distance | При снятии только cap PositionInParent |
|---|---|---|---|
| 254 и 255 | 254 и 255 | 0.3 | 0.3 |
| 255 и 256 | 255 и 255 | 0 | 0.3 |
| 256 и 257 | 255 и 255 | 0 | 0.3 |

Это **позиция прямого ребёнка у родителя**, а не общий номер узла в обходе всего дерева. Файл с тысячей AST-узлов сам по себе не удовлетворяет условию.

Predict сортирует пары по DistanceTo, берёт k=5, выбирает большинство next-token, а при равенстве — меньший numeric label. [Train L117–141 и Predict L145–161][struct-train]. Два gold класса с полностью одинаковым закодированным context невозможно различить этим predictor независимо от числа повторных уроков; одинаковый context даст одинаковый ответ. Здесь появляется конкретная возможность потери order-сигнала в обучаемой памяти.

SiblingCount тоже capped на255, но его **вообще нет в текущей DistanceTo**. [Тот же код distance][struct-fields]. Наш основной A/B не добавляет SiblingCount, не меняет scope/depth, role vocabulary, modulo-кодек SyntaxKind или правила голоса. Их одновременное изменение помешает установить причину.

### 2.2. Реальный train/predict caller и область действия

SemanticCompilerFacade.TrainStructuralKnn извлекает pairs из C# источников и обучает память при числе пар ≥10. PredictStructuralNextTokens вызывает этот predictor только для CSharp при `UseStructuralKnn=true` и TrainingSize≥10. [Обучение current L206–225][facade-train]; [вызов current L298–310][facade-predict]. Значение UseStructuralKnn по умолчанию false. [Options L14–15][options].

Следовательно, у механизма есть реальный typed caller, но дальнейшая связь с обычным ответом конкретного RP01 ещё должна быть показана. Не следует выдавать наличие поля с прогнозами за доказанную власть над выпуском кода.

### 2.3. Выполненный локальный контрпример

В скрипте шесть диагностических training rows: три с position255/label10, затем три с position256/label20. Все остальные признаки одинаковы, k=5 и порядок вставки фиксирован.

| Запрос | A: cap255 | B: только расширенная PositionInParent |
|---|---|---|
| position255 | label10 | label10 |
| position256 | label10 | label20 |
| Контроль с training/query positions254/255 | [label10,label20] | [label10,label20] |

Это source-equivalent вычисление cap, distance, stable nearest selection и голосования. **Числа10/20 здесь диагностические метки, не Roslyn SyntaxKind. Не выполнялись парсинг C#, Extract, native predictor или компиляция результатов. Таблица не является улучшением code accuracy с50% до100%.**

Для native T0 можно построить два ExpressionStatementSyntax под одним BlockSyntax с одинаковыми ancestors/role/depth, но различным первым потомком: invocation против assignment. В descendant-парах этот потомок является next label. Конкретную конструкцию, позиции и различие `NodeTypeId` надо сначала подтвердить настоящим Roslyn; это предложение фикстуры, не уже полученный результат.

### 2.4. Дедупликация и отношение к успешной ветке

Core P26.4 уже прямо фиксирует saturation **SiblingCount** на255 и его предел для предложенного добавления sibling-расстояния. [P26.4 L31–45][prior26]. Поэтому заголовок «впервые нашли переполнение byte в AST» был бы неверным.

Дополнение здесь узкое: **существующий используемый ordinal-признак** PositionInParent; граничные пары254/255/256; явное одинаковое encoded context → одинаковый выбор label; однофакторный опыт widening без добавления отсутствовавшего sibling feature. Это NEW_REFINEMENT известного семейства рисков в AST-признаках.

StructuralKnn и SemanticCompilerFacade содержательно совпадают с external_domains после лицензионного заголовка. [Исторический StructuralKnn][struct-external]; [историческая facade][facade-external]. Поэтому гипотеза может объяснить ухудшение при переходе к длинным методам или иной структуре корпуса, **если такой переход подтверждён**, но не доказывает потерянную при переносе реализацию E09.

### 2.5. Малый причинный опыт по предмету C#

**T0 — 45–90 минут, native Roslyn + настоящий predictor.**

1. Создать C# метод с не менее257 прямыми statement children. Сначала извлечь узлы на positions254/255/256, сохранить Kind/RawKind, parent chain, role, depths, encoded position и реальный successor label. Проверить отсутствие синтаксических diagnostics. Убрать из сравнения пары, которые различаются другими используемыми признаками или слились ещё и в известном modulo-кодеке.
2. A — текущая byte PositionInParent. B — расширить только это поле до int и снять только cap этой позиции. Все остальные поля, labels, k, corpus order, eligibility и search одинаковы.
3. Обучить через обычный Train на проверенных native pairs, затем при применимости повторить через TrainStructuralKnn с ≥10 фактическими парами и явной опцией. Сохранять каждый nearest neighbor, distance-компоненты и selected label. Сначала подтвердить граничный контрпример, затем отрицательный контроль ниже256.
4. Фикстуру с 255/256 сделать в обеих последовательностях training rows: порядок разрешения полного tie может менять A, но оба закодированных запроса A всё равно получают одинаковый результат. Не смешивать этот контроль с изменением k или tie-policy.

**T1 — 6–12 малых training files, отдельные heldout files.** Предмет: программирование C#, задача — продолжение длинных генерируемых методов и builders, где различие порядковых позиций реально предсказывает следующую AST-конструкцию. Для каждой пары условий нужны source hash, заранее проверенные gold next kinds и одинаковая работа с tokenizer/labels. Сначала измерить долю nodes с raw PositionInParent≥256 на реальном наборе. Если таких узлов нет, этот опыт не поможет восстановлению.

Для проверки полезности через обычный output требуется трасса «native predictor → выбранная единица/Flow → конкретный выход → проверка». Иначе вердикт ограничить точностью predictor. Не использовать heldout-файл для выбора веса0.3, k или новой схемы. Измерять gained/lost cases и критические ошибки, а не только среднее попадание по AST-kind.

**Предметные границы.** Применимость ожидается прежде всего у длинных C# методов, сгенерированных schema/builders и больших последовательностей операций. Это не автоматически функция для перевода естественного языка, SQL, TypeScript или обычного порядка предложения: там нужен собственный совпадающий parser/representation/caller. Короткие C# фрагменты являются отрицательным профилем, если все raw позиции <256.

**Фальсификаторы:** нет saturated nodes; иные признаки уже различают целевые случаи; order не связан с gold; widening не меняет выбор; локальный выбор не используется обычным output; heldout качество не улучшается или ухудшается из-за переобучения на абсолютную позицию. Удаление cap сохраняет больше информации, но само по себе не гарантирует обобщение и может быть бесполезным расходом памяти.

**Метрики:** pair-count/index bytes и RAM процесса; extraction и Predict latency; correct next-kind и critical output errors отдельно; cost/data effort; training-memory write time и число pairs. Neural updates = 0 для этого isolated KNN опыта, model total/trainable layers=n/a; уровни Unit/Flow и фактическая глубина вызова фиксируются отдельно. Для source-модели скрипта никакое native время не заявляется.


[filter-current]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/22.AgentAlgorithms/Context/KnnSkillFilter.cs#L19-L42
[search-contract]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/22.AgentAlgorithms/Fabric/Interfaces.cs#L41-L45
[cosine]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/06.VectorSearch/Distances.cs#L10-L20
[hnsw-search]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/06.VectorSearch/HnswIndex.cs#L99-L110
[hnsw-scores]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/06.VectorSearch/HnswIndex.cs#L208-L222
[ast-index]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L60-L65
[ast-idf]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L75-L93
[manager-current]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/22.AgentAlgorithms/Context/ContextWindowManager.cs#L22-L53
[filter-test]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/Tests/AgentAlgorithmsTests.cs#L1089-L1110
[filter-external]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/22.AgentAlgorithms/Context/KnnSkillFilter.cs
[manager-external]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/22.AgentAlgorithms/Context/ContextWindowManager.cs
[ast-external]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/22.AgentAlgorithms/Context/AstRelevanceRanker.cs
[struct-fields]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Prediction/StructuralKnn.cs#L15-L35
[struct-extract]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Prediction/StructuralKnn.cs#L52-L59
[struct-train]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/Prediction/StructuralKnn.cs#L117-L161
[facade-train]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L206-L225
[facade-predict]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerFacade.cs#L298-L310
[options]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/12.SemanticCompiler/SemanticCompilerOptions.cs#L14-L15
[struct-external]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/Prediction/StructuralKnn.cs
[facade-external]: https://github.com/lubochka/llm_mvp_core/blob/3036a01acec938f0b83fa4ffbda26202cc35d639/12.SemanticCompiler/SemanticCompilerFacade.cs
[prior51]: https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/03-NEW-KNN-CONSUMER-GAPS.md
[prior93]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P93-confidence-abstention.md
[prior26]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md#L31-L45
