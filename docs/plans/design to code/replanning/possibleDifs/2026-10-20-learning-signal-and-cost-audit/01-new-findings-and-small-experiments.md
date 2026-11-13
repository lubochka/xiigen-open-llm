# Новые узкие находки и малые опыты

**20 октября 2026 · SOURCE_REVIEW / NOT_RUN.** Исходный core: bf9b543e149ef197da85a767821d98c9b67916d2; поздняя редакция: f5dd532045716495a2c7668573250e5d7cc46810; planning до дополнения: 1ad622fe1f6d1e8b0860265d8c208aeada3fefb0. Runtime source между этими core-ревизиями не менялся. LF-03 — новая для проверенного корпуса узкая гипотеза стоимости настоящего обучения. LF-02 независимо совпала с OCT20-R2 из параллельной публикации и не считается отдельным открытием. LF-01 — уже признанное в поздней редакции ограничение малого KNN-опыта и предложенный способ исследовать структурное различение. LF-04 ниже уточняет достижимость записи в новой карточке E1. Ни одна из этих находок не объявляется уже измеренным улучшением или доказанной причиной плохих свободных ответов RP01. [Проверка поздней редакции](07-late-revision-audit.md).

**Способ будущего выполнения всех LF-карточек:** точные экспериментальные копии выбранных исходников и один ограниченный host по [уточнению 62bcc](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L3-L26). Original code-to-fix означает источник и будущий адрес переноса; product source/tests в малом опыте не изменяются. Точные пути и условия подготовки — в [03](03-learning-first-work-order.md). Для LF-02/LF-03 не требуется предварительное расширение продуктового runner. Текущий результат — документы и fixtures, не новые runtime-прогоны.

**Позднее уточнение7c5718 применяется к каждому LF-prompt:** простой host с throw/try-catch/void/DTO допустим, продуктовый OperationResult для оболочки не нужен. Старые эксперименты, результаты и bin/obj сохраняются. Начатая подготовка выполняется по своему зафиксированному паспорту; этот документ не назначает её перенос, пересборку, повтор или смену исполнителя/reviewer. [Политика и граница версий](03-learning-first-work-order.md).

## LF-01 — уменьшенная N-KU-01 проверяет смену постоянного ответа

**Класс:** TRAIN_INPUT; исправление постановки существующей N-KU-01, а не новый конкурирующий эксперимент. **Домен:** C#, совет о необходимости цикла. **Получатель:** таблица FileActionKnnPredictor; нейронных весов и optimizer updates нет.

### Доказательство

В [действующей карточке 23, строки 75–101][card23] четыре учебных файла, неизменные features и k=3. В A метки K1, K2, K4 — ADD_LOOP, K3 — ADD_METHOD. В B исправляется только обнаружение DoStatement в учебном Label: K1, K2, K3 становятся ADD_METHOD, K4 остаётся ADD_LOOP.

[Настоящий Predict][knn] выбирает три ближайших записи и возвращает наиболее частый класс. В любой тройке из множества с составом 3:1 есть как минимум две записи большинства. Следовательно:

| Плечо после Train | Ответ на любой допустимый query vector | Правильно на четырёх задачах карточки | False ADD_LOOP на трёх завершённых обходах | Recall отсутствующего обхода | Balanced accuracy |
|---|---|---:|---:|---:|---:|
| A | ADD_LOOP | 1/4 | 3/3 | 1/1 | 1/2 |
| B | ADD_METHOD | 3/4 | 0/3 | 0/1 | 1/2 |

В [N-KU на7c5718](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L178-L186) gold KQ1–KQ3 уточнён как NO_EXTRA_LOOP: [evaluator](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L278-L286) принимает непустой actual, отличный от ADD_LOOP; KQ4 требует ADD_LOOP. Это шире проверки строкового равенства ADD_METHOD. При нынешних двух TRAIN-классах приведённая аналитика4/k3 не меняется. Она не является результатом шестистрочного варианта или замером.

**Все числа этой таблицы — аналитическое следствие карточки и алгоритма, не результат исполнения.** До обучения facade возвращает null по guard; он не засчитывается правильным советом. Исходная карточка остаётся реальным опытом изменения учебных меток, но рост её узкой accuracy не доказывает различения AST, использование расстояния или перенос структурного знания. Снижение ложных ADD_LOOP полезно по своей оси; потеря recall показывается одновременно.

Общий риск majority-class в E14 известен [из прежнего аудита 12][prior-majority]. Строгая константность сокращённой N-KU-01 была отдельным результатом проверки исходного снимка, но **в поздней карточке f5dd она уже прямо описана**. Текущий статус LF-01 — `KNOWN_IN_LATE_REVISION`, не новая для f5dd гипотеза. Предыдущий [N60-план][n60-knn] имел восемь TRAIN, восемь EXAM и лестницу 4→6→8; защищённая карточка сократила материал до четырёх строк. Четыре строки остаются допустимым опытом влияния меток на постоянный ответ. Если цель — структурное различение, предлагается отдельная шестистрочная постановка ниже без возвращения всей лестницы. [Позднее уточнение][late-ku].

**Обновление последнего среза 62bcc8f2:** защищённая [полная карточка N-KU-01](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md) уже содержит проект отдельного runner, исходники подготовки и точный 4-row бюджет. Она по-прежнему NOT_RUN и честно ограничивает вывод сменой большинства. LF-01 ниже — **не обязательная замена и не блокер** этого подготовленного опыта; её выбирают только для отдельного структурного вопроса.

### Дополнительная структурная карточка LF-01 / N-KU-01-R

**Гипотеза:** исправление DoStatement в producer учебных меток уменьшит ложный совет ADD_LOOP на завершённых do-обходах при сохранении отдельно измеряемой способности советовать цикл там, где он отсутствует. Выигрыш не гарантируется: нынешние 30 признаков могут не разделить нужные случаи.

**Единственный фактор:** в ActionLabelGenerator.Label добавить DoStatement к hasLoop. Не менять features, расстояние, k, голоса, null-priority, RegexPreprocessor и fallback. ForEachVariableStatement — отдельная будущая карточка, не второе изменение здесь.

**TRAIN:** шесть исходников, одинаковых в A/B. Каждый метод оборачивается в class C. Предусловие задач: xs не null, 0<n<=xs.Length. Два do, два известных завершённых обхода, два действительно отсутствующих обхода:

| ID | Метод | Смысл / метки A→B |
|---|---|---|
| K1 | static int Sum(int[] xs,int n){int i=0,s=0;do{s+=xs[i];i++;}while(i<n);return s;} | Обход есть; ADD_LOOP→ADD_METHOD |
| K2 | static int CountPositive(int[] xs,int n){int i=0,c=0;do{if(xs[i]>0)c++;i++;}while(i<n);return c;} | Обход есть; ADD_LOOP→ADD_METHOD |
| K3 | static int Product(int[] xs,int n){int p=1;for(int i=0;i<n;i++)p*=xs[i];return p;} | Обход есть; ADD_METHOD в обоих |
| K4 | static int CountNegative(int[] xs,int n){int i=0,c=0;while(i<n){if(xs[i]<0)c++;i++;}return c;} | Обход есть; ADD_METHOD в обоих |
| K5 | static int CountZero(int[] xs,int n){return 0;} | Задание: посчитать нули; ADD_LOOP в обоих |
| K6 | static int SumPositive(int[] xs,int n){int s=0;return s;} | Задание: сумма положительных; ADD_LOOP в обоих |

Теперь A имеет 4:2, B2:4: в обоих плечах минимум два примера каждого класса. Поэтому обязательная константа из арифметики k больше не следует. Это устраняет арифметическую неизбежность константы для структурного вопроса, но не доказывает достаточности признаков или обучения. Для узкого label/prior-вопроса исходные четыре строки допустимы.

**EXAM:** сохранить четыре KQ1–KQ4 из [оригинальной карточки][card23], их точные bytes, задачи и gold до любых ответов. KQ1/KQ2 — новые do-формы; KQ3 — завершённый while; KQ4 — отсутствующий обход. ADD_METHOD трактуется только как отсутствие ложного ADD_LOOP, а не как универсально правильная правка программы. Не использовать изменённый Label в качестве оценщика.

**До/после и бюджет:** два пустых независимых predictor в экспериментальной копии; адаптер переносит ровно исходную подготовку и guard из facade. Четыре query в каждом до и после одного native Train. Итого 2 native Train, 12 предъявлений учебных исходников, 16 запросов: до обучения 8 guard-return null, после 8 Predict. Facade.Compile=0, Flow=0; ненужный соседний memory writer не вызывается. Происхождение адаптера: [facade Train][facade-train] → Label + Features → predictor.Train; [facade consumer][facade-predict] → Predict. Это исполнение настоящего predictor с перенесённым узким adapter, не утверждение о полном facade execution. За основу можно взять новый N-KU runner и заменить одинаковые в A/B TRAIN на шесть строк этой отдельной карточки.

**Измерения:** сырые ответы/null по каждому ID; false ADD_LOOP, recall недостающего обхода, обе классовые полноты, balanced accuracy, gained/lost; фактические vectors, top3 IDs/distances/labels — объясняющий след использованного learner, не отдельный benchmark. Стоимость Label+Features+Train отдельно от Predict; bytes таблицы, CPU/RAM и число записей. Равенство vectors или соседей не маскировать переименованием задач.

**Где чинить:** original 12.SemanticCompiler/Prediction/FileActionKnn.cs, hasLoop в ActionLabelGenerator.Label, строки 41–55. В этом аудите source не менялся. Host, точные сохранённые bytes/config и разрешённые output paths должны быть материализованы перед исполнением; готового нового CLI здесь не заявляется.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения. Например, меньше false ADD_LOOP и хуже recall — успех по первой оси плюс регрессия по второй. Нулевой эффект реального Train допустим; отсутствие достигнутого Train — другая причина NOT_RUN.

**Промпт исполнителю:** «Исправь постановку N-KU-01 по LF-01: один DoStatement-фактор, native predictor с точным перенесённым guard и k=3, шесть указанных TRAIN и четыре неизменных EXAM. Сначала материализуй точные source bytes, caller/config/output и независимый gold в конкретный паспорт; защищённый каталог не редактируй. Работай на точной экспериментальной копии; основной код и продуктовые тесты не меняй; результаты и кандидат переноса передай пользователю без продуктового цикла. При разрешённом исполнении обучи две пустые таблицы по одному разу и сохрани 16 ответов адаптера, top3 и все gained/lost. Не подменяй Predict исправленным Label; не добавляй новые features или лестницу запусков. Любое измеренное улучшение полезного показателя — успех по показателю и домену без обязательного роста accuracy; все ухудшения рядом. Не приписывай успех RP01 LoRA. Старые skill-пороги и автоматическое повышение Tier не отменяют этот критерий».

## LF-02 — отдельный массив softmax на каждом response target

**Атрибуция перед публикацией:** в planning64aa3c75 обнаружена [OCT20-R2 другого исполнителя](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/06-new-R2-softmax-buffer.md) с тем же методом, переносом allocation и ограничениями. Это одна гипотеза. Наш бюджет 1 pair/1 update на плечо — короткий вариант её проверки; R2 задаёт 2 pairs/2 updates. Разница бюджета не создаёт новизну и не требует выполнять оба варианта.

**Класс:** TRAIN_UPDATE / стоимость actual backward. **Домен первой проверки:** C# documentation на существующем neural Student. Код общий для DPO; перенос результата по времени на другой домен/длины требует собственного измерения.

### Источник и гипотеза

[BackwardLogProbs, строки 1072–1120][backward] после чтения response mask входит в цикл target-позиций. Внутри каждой ненулевой позиции создаётся новый float[v], затем все его v элементов заполняются exp(logit−max). После использования массив не нужен. Это находится внутри реальной цепи [StepVectorCore][step] → два policy graph → BackwardLogProbs → logits.Backward → разрешённые LoRA A/B → AdamW.

**Предлагаемая единственная правка:** после проверки cnt>0 создать один локальный float[v] на весь вызов BackwardLogProbs и переиспользовать его на следующих строках. Все элементы каждый раз перезаписываются; max, expSum, порядок операций, targetScale, masks, logits.Grad и порядок graph backward остаются прежними. Не вводить глобальный cache, пул, новый softmax или иной dtype.

Пусть N — число фактически скорируемых target-позиций, V — vocab. При N>0 массивы expVals дают 4NV байт payload выделений в A и 4V в B, то есть ожидаемую разницу 4V(N−1), без заголовков массивов. Это **анализ структуры выделений**, не measured RAM или время. Текущая пиковая живая память не обязана расти как NV: старые массивы может освобождать GC. При N=1 выгоды этого изменения не ожидается.

### Малая карточка LF-02

- **A/B:** исходный код и только локальное переиспользование expVals.
- **Материал:** точная существующая пара `simulation-csharp-doc-f06-008` о postfix increment: [одна исходная строка](fixtures/csharp-one-pair.jsonl), [provenance и hashes](fixtures/source-selection.json). Prompt: `int x=10; int y=x++; System.Console.Write(y);`; chosen объясняет вывод 10, rejected — вывод 11. Bytes побайтно сверены с первой строкой primary train500 на bf9b543e; simulation provenance и `csharp_runtime_verified=false` сохранены. Actual model-visible IDs/masks материализуются штатным exact tokenizer; перед запуском установить N>1 на обеих сторонах. Цель — стоимость, не новая оценка компетентности на одном примере.
- **Старт:** две независимые копии одного полного допустимого B0 с одним frozen R0, tokenizer, LoRA topology, optimizer/moments/step, RNG и всеми scalar settings. Exact-v2, dropout0; не менять число токенов или padding.
- **Действие:** один настоящий StepVector с одной парой и один optimizer update на плечо. Два updates суммарно; внутри каждого — два policy и два reference Forward, два policy backward. Итого восемь Forward и четыре backward на A+B, без дополнительных scorers. Вынесенный softmax без model graph/optimizer не является этим опытом.
- **Измеритель:** allocated bytes на фактическом синхронном тренировочном потоке вокруг StepVector и/или backward, длительность backward и всего StepVector раздельно, GC collections с оговоркой process scope, peak RAM как отдельная ось. Один и тот же observer и одинаковые границы в A/B. JIT/порядок и шум записать; не обещать ускорение по одному наблюдению.
- **До/после:** hashes B0/R0/optimizer, фактические значения loss/reward, возвращённые Step, конечные A/B tensors, optimizer state и finite deltas. Возвращённые Step-метрики относятся к forward **до** update; они не переименовываются в post-update score. Дополнительный post-update scoring в этот минимальный бюджет не входит. Для точной арифметически эквивалентной замены ожидание — одинаковый результат обучения, но это проверяемое ожидание. Generate не обязателен: качество свободного ответа — NOT_MEASURED.
- **Фальсификаторы:** ветвь не достигнута; N<=1; измеритель не охватывает target loop; появились численные различия; экономия allocated bytes не наблюдается. Отсутствие ускорения при снижении allocations не отменяет успех по allocations.
- **Реализация запуска:** изолированный host над точной копией DpoTrainer и нужных зависимостей по [03](03-learning-first-work-order.md). [Действующий RP01 host][host] имеет pinned calibrate/main и не используется как повторный A/B. Расширение production dispatch не prerequisite. Статус NOT_RUN / PREPARABLE_EXPERIMENT_HOST.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Все остальные измерения и ухудшения рядом; не измеренное — NOT_MEASURED. Это гипотеза экономии настоящего обучения, не утверждение о причине C#0/3.

**Промпт исполнителю:** «Подготовь, а при разрешённом исполнении проведи только LF-02 на настоящем DpoTrainer: один local expVals buffer per BackwardLogProbs, одна фиксированная C# пара и один update на плечо из одинакового полного B0. Сохрани existing admission, exact masks, R0, keyed LoRA recipient и replay history. Нужен отдельный experimental host с настоящим copied learner, без вызова calibrate/main. Сравни allocated bytes и время на одинаковых границах реального update, также numerical state до/после. Generate не обязателен для этого training-resource опыта. Любой измеренный полезный выигрыш — успех по показателю и домену без обязательного роста accuracy, все регрессии рядом. Работай на точной экспериментальной копии; основной код и продуктовые тесты не меняй; передай результаты и кандидат переноса без продуктового цикла. Не выдавай теоретическое число байт за замер и не расширяй опыт автоматически».

## LF-03 — initialGradient копируется дважды перед graph backward

**Класс:** TRAIN_UPDATE / стоимость переноса исходного градиента. **Домен первой проверки:** тот же C# neural path, отдельный A/B от LF-02.

### Доказательство и единственная правка

[Tensor.Backward(initialGradient), строка 99][tensor-backward] выполняет new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape). [Конструктор Tensor(float[], shape)][tensor] передаёт массив в TensorStorage; [TensorStorage(float[]), строки 20–24][storage] снова делает data.ToArray(). В DPO этот путь вызывается из logits.Backward(logits.Grad), то есть охватывает весь tensor logits, включая его нулевые gradient positions.

**B:** заменить только new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape) на new Tensor(initialGradient.Data, initialGradient.Shape). Защитная копия конструктора остаётся, внешний alias не возникает. Не присваивать Grad=initialGradient, не менять TensorStorage и не добавлять in-place reuse: это были бы другие контракты.

Для M элементов градиента удаляется один массив payload4M байт и одно копирование M элементов на вызов. Это source-level расчёт, не measured peak RAM. При нынешнем профиле V=629 и фактической длине T gradient имеет 1×T×629 элементов; T берётся из реального material, не приравнивается автоматически context2048. [Профиль][config]. При двух policy backward на одну пару экономия зависит от длины обеих graph; сюда не включаются остальные allocations модели.

### Малая карточка LF-03

Та же фиксированная пара `simulation-csharp-doc-f06-008` из [fixtures/csharp-one-pair.jsonl](fixtures/csharp-one-pair.jsonl), два одинаковых полных B0/R0/optimizer/RNG, один StepVector и один optimizer update на плечо. Общий native бюджет, как LF-02: два updates, восемь Forward, четыре backward, ноль Generate. A — исходный source; B — удалено только внешнее ToArray. LF-02 не применять в том же контрасте. До/после сохраняются LoRA state и moments; реальные gradients наблюдаются до ApplyOptimizer/ZeroGrad. Возвращённые Step-метрики описывают pre-update forward; post-update objective остаётся NOT_MEASURED, поскольку дополнительных scorer calls в этот бюджет не включено. С теми же observer boundaries измеряются allocated bytes синхронного training thread, backward/StepVector time, CPU/GC/peak RAM раздельно. Отдельная проверка copy semantics допустима как проверка изменённого training seam, но сама по себе не заменяет этот learning experiment. Generate для resource-оси не обязателен.

**Где чинить:** 00.TensorCore/Core/Tensor.cs, Backward(Tensor), строка 99. **Reachability:** StepVectorCore → BackwardLogProbs → Tensor.Backward(Tensor) → настоящий autograd → AdamW. **Фальсификация:** не достигнут вызов; constructor перестал делать copy в фактической сборке; выходное состояние отличается; замер не показывает ожидаемой экономии. Влияние на семантическое качество RP01 не установлено.

**Готовность:** NOT_RUN / PREPARABLE_EXPERIMENT_HOST, как LF-02. Подготовить точную копию и isolated caller; не повторять уже issued production calibration.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Показывать остальные измерения и ухудшения. Снижение allocations при неизменном ответе — успех по allocations; это не доказательство роста качества генерации.

**Промпт исполнителю:** «Подготовь, а при разрешённом исполнении проведи только LF-03: убери наружный ToArray в Tensor.Backward(Tensor), сохрани constructor copy и все математические операции. По одному настоящему DPO update в A/B из одного исходного полного состояния, одна пара; без LF-02 и иных оптимизаций. Зафиксируй actual caller/admission, reached Backward, measured allocated bytes и время, gradients до ApplyOptimizer/ZeroGrad, LoRA/optimizer до и после. Не подменяй обучение standalone copy benchmark. Любой измеренный полезный выигрыш — успех по показателю и домену без обязательного роста accuracy; регрессии и NOT_MEASURED оси рядом. Работай на точной экспериментальной копии; основной код и продуктовые тесты не меняй; передай результаты и кандидат переноса без продуктового цикла. Не выполняй автоматические повторения и не объявляй это объяснением плохих ответов без отдельного доказательства».

## LF-04 — два учебных примера N60-E1 не записывают требуемое правило Verifier

**Класс:** TRAIN_INPUT → конкретный Unit writer. **Домен:** RU→EN, проверка сохранения наречия. **Статус:** SOURCE_CONFIRMED / NOT_RUN; `PREPARABLE_CARD_REPAIR`. Общий дефект распознавания кириллических окончаний уже относится к N57; новое здесь — точная недостижимость в только что добавленной [N60-E1][e1].

### Почему вызовы Train не доказывают нужное обучение

В E1 V1 использует `Он быстро открыл дверь.` / `He quickly opened the door.` / `[WRONG:tense]`, V2 — `Она тихо закрыла окно.` / `She quietly closed the window.` / `She closed the window.`. [BuildGeneralBatch][builder] создаёт для каждой пары три разных Unit-урока. Для V1 отрицательный carrier не содержит отдельного текста; BuildRejectedCandidate возвращает chosen. A принимает обе verifier-строки, B пропускает только строку с совпавшим candidate.

Однако цепочка именно этого Verifier writer на указанных текстах обрывается до изменения памяти:

1. [Русский tokenizer][tokenizer] не включает «быстро» и «тихо» в allowlist; его суффиксы для ADV тоже не распознают эти слова. На этом пути они становятся NOUN, а не ADV.
2. У предложения есть RuleTags от местоимения/глагола. Поэтому запасной путь ExtractSourceMarker не может принять произвольный unknown noun только по пустому набору тегов.
3. [LooksLikeMannerMarker][marker] содержит испорченные строки окончаний вида `Ð¾`, а также английское `ly`; обычные кириллические «быстро»/«тихо» им не соответствуют. Marker остаётся пустым.
4. В [TrainOnExample][verifier-writer] issue имеет форму `expected 'quickly' for translated quality marker`. Его хвост не содержит ни одного имени [SemanticConcept][concepts], поэтому TryParseIssue не открывает альтернативную запись concept/signature. В ветви TryParseRequiredTerm значение qualityMarkerIssue=true также исключает запись по signature. При пустом marker не меняется и `_requiredTermsBySourceMarker`.

**Статическое следствие для пустого Verifier:** A0/A1/B0/B1 возвращают Pass на всех четырёх исходных E1 EXAM; обе omission-ошибки пропускаются. Это не новый измеренный результат. SemanticTokenizer и Executor в том же batch могут писать своё состояние: утверждение «вообще никакого обучения не было» было бы неверным. Правильный диагноз — выбранные уроки не достигают **заявленной записи правила Verifier**. Общее число 11 Unit-вызовов этой связи не доказывает.

### Исправленная малая карточка LF-04 / N60-E1-R

Сохранить фактор E1 — admission совпавшего отрицательного candidate. Для подготовки **обоих** плеч заменить TRAIN на слова из уже существующего ADV allowlist, не чиня одновременно N57 код:

| ID | Source | Chosen | Rejected carrier |
|---|---|---|---|
| V1 | `Он нежно открыл дверь.` | `He gently opened the door.` | `[WRONG:tense]` |
| V2 | `Она горестно закрыла окно.` | `She sadly closed the window.` | `She closed the window.` |

Direction=RU_EN, Structural signature, heldOut=false, разные IDs, одинаковые train_ready/source поля; точные typed records материализуются исполнителем. Пустые исходные Units независимы, A/B получают одинаковые две parent-пары. `нежно` и `горестно` уже распознаются как ADV, поэтому marker writer достижим через первую ветвь. Изменение общей панели при подготовке одинаково для A/B; оно не является вторым фактором между плечами. После начала измерения панель не подбирается по результатам.

**Original code-to-fix для изучаемого фактора:** TranslationStructuredBatchBuilder.BuildGeneralBatch, добавление verifier example около строк 127–141. В B вычислять существующий rejectedCandidate как обычно и не добавлять **только** verifier-row, когда candidate совпадает с chosen по заранее объявленному точному сравнению текста; semantic/executor rows сохраняются байт-в-байт. Не менять BuildRejectedCandidate, tokenizer, marker helper, synonyms или Verify. Исправление N57 в TranslationTrainedUnits.LooksLikeMannerMarker — другой опыт, не скрытая часть этого A/B.

**EXAM и независимый gold:**

| ID | Source | Candidate | Ожидание по смыслу |
|---|---|---|---|
| VQ1 | `Он нежно закрыл окно.` | `He tenderly closed the window.` | Pass: допустимая парафраза |
| VQ2 | `Он нежно закрыл окно.` | `He closed the window.` | Fail: пропуск «нежно» |
| VQ3 | `Она горестно открыла дверь.` | `She sadly opened the door.` | Pass |
| VQ4 | `Она горестно открыла дверь.` | `She opened the door.` | Fail: пропуск «горестно» |

Это четыре наблюдения двух marker-семейств, не четыре независимых языковых закона. Gold фиксирует узкую приемлемость tenderly/gently и не выводится из правил Verifier. Читатель здесь — собственный QualityVerifierUnit.Verify, не ordinary translation Flow.

**Бюджет:** два TrainStructuredBatchAsync, 11 вложенных Unit.TrainOnExample (A:6, B:5); из них три вызова обучения Verifier (A:2, B:1). 16 Verify: четыре задачи в A0/A1/B0/B1. Neural optimizer updates=0, ordinary Translate/Flow=0. Считать отдельно фактически записанные marker→required-term записи, а не называть каждый вызов ненулевым update. Измерять подготовку, Train и Verify раздельно; CPU, выделения памяти и размер состояния указывать с границами измерителя.

**До/после:** пустые state identities, typed parent/projected rows, marker и term, state после каждой TRAIN строки, все Issues/Pass. Статическое ожидание после Train, если нет иных правил: A=[Fail,Fail,Pass,Fail], B=[Pass,Pass,Pass,Fail]. Это даёт одинаковую accuracy 3/4, false reject 1/2→0/2 и false accept 0/2→1/2. До Train оба Verifier принимают все ответы. **Эти числа — прогноз по коду, actual поля остаются пустыми.** Фактическое несовпадение требует показать реальную ветвь, а не подгонять измерение.

**Фальсификаторы:** intended marker writer не достигнут; иной verifier rule определяет решение; A/B различаются не только admission; критерий gold либо consumer изменены после ответов. Честный нулевой эффект достигнутого урока не invalid. Если ordinary Flow выбирает другой маршрут, direct Verify не доказывает улучшения итогового перевода.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy не обязателен. Все остальные показатели, включая ухудшения, показываются рядом. Возможный меньший false reject и больший false accept — два отдельных вывода.

**Промпт исполнителю:** «Подготовь N60-E1 по LF-04: используй одинаковые в A/B две приведённые пары с нежно/горестно и четыре фиксированных EXAM. Проверь source путь до actual marker writer; доступные typed поля и bounded caller заполни самостоятельно, без нового общего согласования. Меняй только допуск verifier-row с candidate==chosen в original BuildGeneralBatch; N57, tokenizer, synonyms и другие Unit-уроки не менять. В рамках выполнения выбранного опыта сохрани два native batch, 11 Unit-вызовов, 16 Verify, фактические marker/term/state и сырые ответы. Любой измеренный полезный сдвиг — успех по показателю и RU→EN профилю без требования accuracy; ухудшения рядом. Не выдавай статический прогноз за замер, direct Verify — за перевод, вызов Train — за достигнутую запись. Защищённую папку не редактируй. Работай на точной экспериментальной копии; основной код и продуктовые тесты не меняй; передай результаты и кандидат переноса без продуктового цикла».

## Объём проверки новизны

Весь Markdown-срез possibleDifs — 555 документов из 869 файлов — скачан на planning pin и проверен адресным текстовым поиском; отдельно 156 Markdown защищённых экспериментов вне results на исходном снимке. Затем скачаны все 152 изменённых Markdown поздней f5dd-редакции и повторён адресный поиск новых allocation-механизмов; совпадений не обнаружено. Для LF-02/LF-03 искались expVals, new float[v], BackwardLogProbs, scratch/buffer/softmax, initialGradient, TensorStorage, двойное копирование/clone и связанные resource-формулировки. Общие статьи autograd, numerical instability и N60 padding уже существуют; эти две конкретные причины выделений в них не обнаружены. Это ограниченное заявление о проверенном корпусе, не гарантия абсолютной новизны во всех ветках/неопубликованной работе.

При последнем readback planning продвинулся до fb9aadf69e0669d36b820d61f76fe3ecc8697a7d; прочитаны/адресно проверены 24 изменённых Markdown, в том числе два параллельных пакета. Их LC-REF-CACHE, KN-ROUTER и два Mode1-фактора — другие механизмы; LF-02/LF-03 в них не обнаружены. Они признаны как чужие находки в [07](07-late-revision-audit.md), без присвоения новизны. Для core дополнительно рассмотрена дельта до 62bcc8f2368840e22d2ea98ef16d9bd526ad9a39; E1 материал не менялся, появился отдельный N-KU runner proposal.

LF-01 уже описана в поздней защищённой редакции. LF-04 — проверка конкретных новых E1 fixtures против известных N57 tokenizer/marker ограничений, а не новая общая гипотеза о кодировке. Две ресурсные карточки не заменяют MASK/STATE/ORDER и восстановление реального полезного обучения.

**Итоговая поправка новизны:** до записи появилась дельта planning fb9aadf6→64aa3c75: 61 путь, включая 26 Markdown. Все 26 Markdown получены и адресно проверены; точная LF-02 уже опубликована как OCT20-R2, поэтому прежний отрицательный поиск относится только к прежним pins. Для initialGradient/TensorStorage/двойной копии и конкретной E1 marker-недостижимости совпадений в этой дельте не найдено. Positive-feedback candidate пакета62 касается другого chosen.Pass=true пути без установленного producer; у LF-04 есть реальный builder и chosen.Pass=false. Полная атрибуция и receipt — в [07](07-late-revision-audit.md).

[card23]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L75-L101
[knn]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/FileActionKnn.cs#L121-L141
[facade-train]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/SemanticCompilerFacade.cs#L193-L202
[facade-predict]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/SemanticCompilerFacade.cs#L327-L333
[prior-majority]: https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md#L21
[n60-knn]: https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/60-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/02-KNN-UNITS-AND-HISTORICAL-LEARNING.md
[backward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1072-L1120
[step]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L832-L926
[tensor]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/Tensor.cs#L39-L46
[tensor-backward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/Tensor.cs#L94-L103
[late-ku]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L109-L116
[storage]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/TensorStorage.cs#L20-L24
[host]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L78-L90
[config]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/config/code-documentation/generation-v1.json


[e1]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L188-L218
[builder]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L80-L141
[tokenizer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L89-L177
[marker]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1263-L1303
[verifier-writer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1093-L1143
[concepts]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationUnitContracts.cs#L59-L78
