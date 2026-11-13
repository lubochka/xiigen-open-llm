# FIX-PQ — строить набор POS-биграмм запроса один раз при чтении обученной памяти

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-PQ исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-PQ. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-PQ. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

**Усиление после R4:** exact128TRAIN/32EXAM и resource check настоящего reader уже внесены. Good oracle — ordered memory/input+gold каждой строки, before11/32→after28/32 и одинаковые A/B answers, плюс allocations на том же reader. Плохой — helper принимает HashSet, но callback всё равно копирует его на каждую строку; одна сигнатура не проверяет reuse. Второй Train должен заменить назначенную память, а не молча удвоить128→256; expected не получать текущим Predict. Набор биграмм принадлежит запросу; другой query не должен получать старый set. Resource improvement не объявлять новым accuracy gain, если ответ A/B равен. Новые assertions используют существующие outputs, не дополнительные fit. [67](67-transfer-repair-followup-audit-2026-10-27.md), [03§25](03-test-and-execution-contract.md#repair-followup-20261027).

**Уточнение исполнения, 27 октября 2026.** Код внесён в `4ce8bdd0dd3`. Проверка типа private SequenceDistance не обнаружит сбор HashSet внутри OrderBy для каждой строки. Подтвердить число сборок/расход настоящего after-reader вместе с назначенной PQ панелью128/32, сохранив все before/after ответы и потери. Expected не строится вызовом изменённого classifier. Empty fallback остаётся перед подготовкой query; размеры памяти и unchanged method signature не являются измерением обучения.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

ID: **FIX-PQ**. Дата: 22 октября 2026 UTC. Статус: **кандидат узкого переноса; сейчас только план**. Основание — завершённый **POS-QUERY-SET-ONCE-01 / PQ01**, опубликованный в новом commit [a87fef91ba52e81c69dd63fbc094daa776f57095](https://github.com/lubochka/llm_mvp_core/commit/a87fef91ba52e81c69dd63fbc094daa776f57095). Номер старого опыта50 и номер этого документа51 не задают очередь: порядок и следующий ID берутся только из [реестра](01-registry.md).

**Решение:** в native `SemanticCompiler.PatternClassifier` проекта12 вынести создание query bigram set из каждого сравнения с выученной строкой в начало одного `Predict`, после проверки пустой памяти. В исторической панели32чтений памяти128строк allocation уменьшился **3 050 752→1 799 040 байт, −1 251 712, −41,03%**, при той же памяти и тех же ответах. Качество после настоящего обучения одинаково A/B: **11/32→28/32**. Это успех по стоимости использования результата обучения; нейронное обучение, качество перевода текста и маршрутизация доменов этим опытом не измерены.

## 1. Доказательства, ограничения и новизна

| Первичный источник на a87fef91… | Что проверяется |
|---|---|
| [Report](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md) | Фактор, источник данных, scope и исторические ограничения |
| [Raw A](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/A/arm.json), [raw B](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/B/arm.json) | `answers_before/after`, `memory_before/after_train`, `rows`, `composition`, три cost windows, actual budget |
| [Один diff](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/one-factor.diff) | Только Predict и private SequenceDistance; Train и fallback не менялись |
| [Fixture](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/fixture.json), [source manifest](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/source-manifest.json) | Сохранённые160строк и точные hashes; повтор генератора не нужен |
| [Historical host](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/host/Program.cs#L128-L164) | Один native Train между32before/32after; раздельные окна чтения и записи |

Source pin самого опыта — `d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19`, Git blob classifier `4d7a4c6ba2c9bb84bc915adebf7bd818135ebeba`, SHA256 source/A-copy `ccf8dd99d8e071c551eb176e1487d4c0272f6b32aa4979041f1fc650e25c468c`, B-copy `33875c4d9ce0594f116f5fbbf4d267146f6f7ad0859bfd55d74e7c50cbdad8a1`. Source-copy опыт не выдаётся за уже выполненный перенос на native ProjectReference. Нынешняя документная проверка прочитала raw A/B и сопоставила полные memory и answer arrays; нового Train/Predict не выполнялось.

| Измерение | A | B | Вывод |
|---|---:|---:|---|
| Память после Train | 128строк | Те же128строк, тот же порядок | Настоящее заполнение памяти0→128 |
| Correct before→after | 11/32→28/32 | 11/32→28/32 | Приобретение есть; factor accuracy delta0 |
| Стали верными / стали неверными / ответы изменились | 19 /2 /22 | То же | Все строки остаются в отчёте |
| Разных gold-классов среди правильных after | 6из8 | 6из8 | Не constant-label результат |
| Allocation32after Predict | 3 050 752B | 1 799 040B | −1 251 712B, −41,03% |
| Allocation32before Predict | 3 336B | 3 336B | Frozen empty-memory fallback не затронут |
| Allocation одного Train | 2 072B | 2 072B | Стоимость записи не улучшена этим фактором |
| Wall32after Predict | 20,5231ms | 16,0949ms | −4,4282ms, наблюдение одной пары |
| CPU32after Predict | 31,25ms | 15,625ms | Дельта одного кванта CPU-счётчика |
| Wall одного Train | 0,6109ms | 0,5236ms | Train побайтно одинаков; сдвиг не приписывается query cache |
| CPU before-readout | 0ms | 15,625ms | Ухудшение наблюдения, несмотря на неизменённую fallback ветвь |
| Peak working set процесса | 35 950 592B | 34 705 408B | Шумный process maximum; не отдельное доказательство причинности |
| GC0/1/2 во всех трёх окнах | 0/0/0 | 0/0/0 | Allocation delta не подменяется retained-heap оценкой |

**Любое измеренное улучшение полезного показателя — успех по этому показателю и профилю; growth accuracy не требуется.** Здесь измерен полезный расход reader обученной памяти. Устойчивое ускорение от одного фиксированного порядка A→B не установлено; будущий native процент заранее не обещается.

Состав данных существенно ограничивает вывод: TRAIN128 содержит49уникальных последовательностей и79повторов; EXAM32 содержит19уникальных последовательностей. **26/32 EXAM дословно присутствуют в TRAIN** и дают9/26→26/26; остальные6дают2/6→2/6. Весь чистый прирост+17 приходится на знакомые последовательности. Две потери — zero-based EXAM indexes11,21, обе SOV; после обучения предсказываются TOPIC_OV7 и VSO2. Это сохранённые недостатки ученика, не regression B против A. Все четыре ошибочные after строки11,21,24,30 остаются в raw и отчёте; новые данные для исправления этих ошибок здесь не назначены.

Две неточности historical prose не наследуются. Во-первых, raw TRAIN counts показывают **классы5и7 по19строк**, поэтому constant label5 действительно выбирается меньшим ID при равенстве; он даёт1/32. Фраза report «tie не понадобился» неточна. Во-вторых,308байт ниже — среднее по конкретной панели, не гарантированная цена HashSet любого размера.

[TEST-POS34](34-pos-bigram-learning-coverage.md) уже задаёт отдельный6-row/4-query тест приобретения SVO/VSO **без product diff**. FIX-PQ добавляет новый measured reader factor и native ресурсное сравнение на сохранённых PQ160данных. Файлы34, его fixture, бюджет и прежний E25 результат не переписываются и автоматически не исполняются.

## 2. Точный код и путь использования обученного состояния

Единственный production file: [12.SemanticCompiler/Translation/PatternClassifier.cs](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/12.SemanticCompiler/Translation/PatternClassifier.cs), класс **`SemanticCompiler.PatternClassifier`**, не одноимённый `SemanticCompiler29.PatternClassifier`.

| Участок | Назначенный diff / сохраняемый контракт |
|---|---|
| `Train`, строки39–43 | **Не менять**: `_train.Clear(); _train.AddRange(examples)`. Это запись размеченной памяти, сохраняющая порядок и ссылки на массивы |
| `Predict`, строки46–56 | После `if (_train.Count == 0) return PredictByRules(posSequence);` создать один `HashSet<(int,int)> queryBigrams`; заполнить соседними парами в прежнем порядке; передать его в каждый distance call |
| `SequenceDistance`, строки78–87 | Сменить private первый аргумент `int[] a` на `HashSet<(int,int)> bigramsA`; удалить только построение query set. Построение set строки памяти `bigramsB`, intersection/union и `union==0 ? 0` сохранить |
| kNN/vote | `.OrderBy(...).Take(_k).GroupBy(...).OrderByDescending(g=>g.Count()).First().Key` сохраняется целиком, включая стабильный порядок ничьих |
| `PredictByRules` и `GenerateExamples` | Не менять; не переносить вычисление query set перед empty-memory guard |

Точный заменяемый фрагмент:

~~~csharp
if (_train.Count == 0) return PredictByRules(posSequence);
var queryBigrams = new HashSet<(int, int)>();
for (int i = 0; i < posSequence.Length - 1; i++)
    queryBigrams.Add((posSequence[i], posSequence[i + 1]));
return _train
    .OrderBy(t => SequenceDistance(queryBigrams, t.PosSeq))
    .Take(_k)
    .GroupBy(t => t.Pattern)
    .OrderByDescending(g => g.Count())
    .First().Key;
~~~

Private distance принимает уже построенный set **только для чтения**. Не добавлять `IntersectWith`, сортировку query, mutable static cache, dictionary по ссылке массива, кэш строк памяти, dedup учебных строк или новую API. Метод остаётся синхронным; отдельный runtime switch по arm не нужен. Входные и учебные массивы не изменяются между/во время этой операции. Concurrent mutation native API сейчас не защищает; cached query не объявляется исправлением потокобезопасности.

Проверенный caller — [E25PatternClassifier.Run:20–47](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L20-L47): Train128→Predict validation; [GlobalUsings:11](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/35.ExperimentsV2/GlobalUsings.cs#L11) направляет alias в project12. Native тест данного плана использует те же public Train/Predict напрямую. Поэтому измеряется конкретный reader, от которого зависит результат обучения, а не отвлечённый HashSet benchmark.

Важная граница consumer: [18.CapabilityTraining/LlmIngredientLogicalUnits.cs:213–238](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L213-L238) создаёт **другой** `SemanticCompiler29.PatternClassifier` и даёт его rule-label вклад0.20 в fusion. PQ не доказывает улучшение этого пути. Нельзя повторять вводную report как доказательство вызова project12 из данного fusion. FIX-PQ не меняет project29/18, lexical weights, semantic authority или маршрут запроса.

## 3. Математика сохранения ответа и стоимости

Для последовательности s определим множество соседних **упорядоченных** пар:

\[
B(s)=\{(s_i,s_{i+1}):0\le i<|s|-1\},\qquad
d(q,t)=\begin{cases}0,&|B(q)\cup B(t)|=0,\\
1-\frac{|B(q)\cap B(t)|}{|B(q)\cup B(t)|},&\text{иначе.}\end{cases}
\]

При неизменном q множество B(q) не зависит от выбираемой строки t. Перенос его построения за цикл не меняет ни cardinalities, ни double-деление для любой строки. Внутри `SequenceDistance` переданный set не изменяется. Значит весь ordered list расстояний прежний; прежние stable OrderBy/Take/group order дают тот же top-k и голос. Empty-memory ветвь возвращается до построения set. При длине0или1 set пуст; при двух пустых sets distance остаётся0. Повтор пары не увеличивает её кратность — это прежняя set-семантика, а не новое упрощение предложения.

Это самостоятельное доказательство для данного native алгоритма, а не утверждение, что POS-биграммы сохраняют полный синтаксис. Например `[3,8,7,3]` и `[3,8,7,3,8,7,3]` имеют один set пар; global repetition теряется у обеих реализаций. Направление пар сохраняется, но долгие зависимости, отрицание, AST и качество перевода слов здесь не доказываются.

Пусть N — число строк памяти, Q — число ответов. A строит query set QNраз, B — Qраз; memory-row sets остаются QNраз. Здесь N128,Q32, значит **4096→32**query constructions, экономится4064. По allocation delta средняя цена подавленной сборки `1 251 712 / 4064 = 308B`. Эквивалентно, сумма цен query sets по32входам панели равна9856B; среднее308не является универсальной константой для длинной последовательности/другого runtime. Общий kNN всё ещё перебирает Nстрок и сортирует расстояния; асимптотику всего поиска нельзя назвать постоянной.

Математика поддерживает приоритет узкого нейтрального изменения, а измеренные bytes подтверждают пользу именно после обучения этой памяти. Ресурс Train не обязан снижаться: его код не меняется. Сохранение ответов не превращается в утверждение о новом усвоении от оптимизации reader.

## 4. Точный fixture, состояние и повторное использование

Будущие файлы:

- `Tests/Fixtures/ExperimentFixes/v1/PosQueryBigram01/fixture.json` — **byte-exact копия** сохранённого PQ fixture из§1;
- `Tests/Fixtures/ExperimentFixes/v1/PosQueryBigram01/manifest.json` — source/blob/SHA, split, k, domain/profile, expected composition и ownership;
- `Tests/ExperimentFixes/PosQueryBigramFixture.cs` — native loader, раздельные TRAIN/query/gold views, read-only snapshot и receipts; не копия classifier.

Fixture SHA256 **`a62864516bc053f1d5a99f46dc9f913b3c13b776d9fe1fec02e70538b2a4027c`**. JSON schema фактически: `Seed=42`, `PerPattern=20`, `Split=128`, `Rows`160с `PosSeq`/`Pattern`. Строки0..127 — TRAIN,128..159 — EXAM, без пересортировки/дедупликации/нового split. `GenerateExamples` в будущем переносе вызывается **0раз**; seed описывает историческое происхождение, не назначает повтор генератора.

Ordered TRAIN digest **`186baee96795c3822a96c07f86c45d1645728529f232758af082b302b80870a9`**, EXAM **`47d7861f1bb208b2c62248a08ae11669f6d42d99e6a46af6ee6ec8131512adf6`**. Алгоритм hash из host: SHA256 UTF-8 `string.Join("|", rows.Select(r => string.Join(".", r.PosSeq) + "#" + r.Pattern))`; никаких завершающих newline. Manifest хранит также raw SHA fixture, чтобы canonical digest не скрыл изменение bytes.

Тема — **перевод**, профиль **порядок слов по POS,8synthetic классов**. Языковые направления EN/RU и текстовый POS tagger не используются. Значения классов0SVO,1SOV,2VSO,3IMPERATIVE_V,4IMPERATIVE_VS,5QUESTION_SV,6QUESTION_VS,7TOPIC_OV сохраняются. Теги0..11 — native словарь; examiner передаёт в Predict только int[] и после чтения сопоставляет с отдельным gold. TRAIN labels предназначены Train; EXAM labels не поступают в classifier и не выбирают k, бюджет, checkpoint или поднабор.

**Готового native save/load API у класса нет.** Train — единственный public способ заполнить таблицу. Поэтому не обещать восстановление learned S0 из JSON памяти через private reflection, новый конструктор или сериализацию всего объекта. На каждой из двух native сторон создаётся один `PatternClassifier(k:3)`, выполняется один128-row fit, затем тот же instance используется всеми32after запросами. Это ровно два заранее назначенных acquisition, а не незаметный producer на каждый assert. Обучение из состояния другой карточки/по шестистрочному TEST-POS fixture не подходит для ресурса памяти128строк.

После fit сохраняются `state/learned-memory.json` и `state/manifest.json` с ordered sequences/labels, input hash, native identity и result receipt. Это **evidence snapshot, не restore checkpoint**. Сравнение/отчёт/поздняя проверка читает эти файлы без Train/Predict. Дополнительный живой consumer может повторно использовать тот же instance только внутри данного назначенного процесса; новый процесс потребовал бы новый явно назначенный fit и здесь не запускается. Отсутствующий/непригодный fixture — setup defect, без генерации другой выборки.

## 5. Native обучающий Fact и доказательство сохранения памяти

Новый запланированный файл `Tests/ExperimentFixes/PosQueryBigramLearningTests.cs`, namespace `LlmFromScratch.Tests.ExperimentFixes`, единственный Fact **`OnePinnedBatch_PreservesLearnedAnswersAndMeasuresReaderCost`**. Использовать обычный ProjectReference на native project12 из `Tests/LlmFromScratch.Tests.csproj`. Тип в коде писать полностью `SemanticCompiler.PatternClassifier`; одноимённый project29 не подходит. До вызовов зафиксировать assembly Location/SHA/MVID и source stage SHA. Никаких Compile Include copied classifier или исторической PQ assembly.

Одна рука, один свежий OS test process, точный порядок:

1. Проверить root/ownership/fixture hashes и состав128/32; создать query views без EXAM gold в learner; заранее выделить два массива answers32 и prepared Train list. Создать classifier k3. Снять read-only memory snapshot: count0.
2. В одном измеряемом окне выполнить **32 Predict before** по EXAM в исходном порядке. Это native rule fallback; его11/32не выдаются за обученное S0.
3. В отдельном окне **один `Train(train128)`**. После окна снять memory snapshot128и ordered digest, сверить все строки/метки с input. Ненулевой after результат далее доказывает участие памяти; число entries само по себе не заменяет качество.
4. В третьем окне **32 Predict after**, тот же EXAM и порядок, тот же instance. Сохранить каждую метку. Никаких повторов/прогрева/отдельной hard-panel call.
5. Только по сохранённым arrays/gold вычислить before11/32,after28/32,19acquired/2lost/22changed,6разных правильных классов, constant1/32, known26иunseen6группы. A и B должны совпасть построчно до/после и по learned-memory snapshot. Если native A расходится с pinned semantics, записать drift, не править gold/данные ради28.
6. Записать один terminal receipt; все дальнейшие assertions/comparison/report работают с ним без нового fit/ответов.

Read-only проверка actual `_train` допустима здесь как явно назначенная test instrumentation из [host:275–289](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/host/Program.cs#L275-L289): `GetField("_train", Instance|NonPublic)`, чтение фактического `List<(int[] PosSeq,int Pattern)>`, немедленная отдельная копия для snapshot. Отсутствие/другой тип поля — named compatibility failure. **Запись private state/reflection restore запрещена**; production API и visibility не меняются. Такое наблюдение не переносится в TEST-POS34, где отдельный договор private reflection не назначает.

Измерять по [host Measure:249–273](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/experiments-v2/RP01-MICRO/PQ01/host/Program.cs#L249-L273): current-thread allocated bytes, monotonic wall, process CPU и GC0/1/2. Data load, memory reflection, hash, scoring, IO и snapshots вне окон; answers arrays уже выделены. Записать managed-thread identity до/после, same runtime/ISA/config. CPU/peak отдельные наблюдения; не складывать окна с process lifetime. Не прогревать Predict и не выполнять forced GC. Main measured success — расход **after reader**, а не Train и не пустая fallback панель.

Основной бюджет: **2Train,256предъявлений,128Predict** — в каждой руке1Train128строк и32before+32after. Generator, нейронные updates, optimizer, DPO, внешние модели, warmups, repeats, additional hard readouts —0. Hard subset определяется только по уже сохранённым before-ошибкам; его19правильных after из21читаются из тех же arrays, новых Predict не назначается.

## 6. Два узких контракта и существующие проверки

Новый `Tests/ExperimentFixes/PosQueryBigramContractTests.cs` содержит два Facts. Это маленькие контракты native reader, а не новые самостоятельные экспериментальные гипотезы. Их расходы объявлены отдельно и не входят в32after allocation window.

**`EmptyMemory_UsesRulesWithoutMutation`**: один новый k3classifier,0Train. Четыре вызова Predict: `[]→3`, `[7]→0`, `[8]→3`, `[3,7,8]→1`. Expected заданы явно по native rule contract, не вычисляются вызовом изменяемой функции. Проверить неизменность входных массивов. Это защищает early return, который должен остаться перед query set.

**`LearnedMemory_ZeroUnionRepeatedBigramsAndTiesAreStable`**: один новый k3classifier, один Train четырёх строк **в этом порядке**:

| TRAIN row | POS | Label |
|---|---|---:|
| T1 | `[3,7,8]` | SOV1 |
| T2 | `[8,3,7]` | VSO2 |
| T3 | `[3,8,7]` | SVO0 |
| T4 | `[]` | IMPERATIVE_V3 |

Шесть Predict: `[]→3`, `[7]→3`, `[8]→3`, `[6,6]→1`, `[3,8,7,3]→0`, `[3,8,7,3,8,7,3]→0`. Объяснение oracle: empty/singleton query имеет empty set и distance0кT4; для `[6,6]` все distances1, поэтому stable top3=T1,T2,T3 и первым при равном голосе остаётся label1; последние два query имеют одинаковый set и ближайший T3. Все входные и учебные массивы сравнить с исходными копиями после этих шести чтений. Непривычный inputNUMNUM проверяет deterministic tie contract, не объявляется правильной синтаксической интерпретацией естественного предложения.

Итого contracts A+B: **2Train,8предъявлений,20Predict**; никаких private method calls/second fit ради отдельных assertions. Матрица проверяет union0, singleton, пустую память, set repeat, stable ties и немутируемость; отдельные дополнительные контракты не назначаются автоматически.

Шесть existing rule Facts сохраняют assertions и не вызывают Train:

| Полный FQN после `LlmFromScratch.Tests.` | Решение |
|---|---|
| `E25E26E27Tests.E25_PredictByRules_ImperativeV_DetectedWhenVerbFirst` | Прежний verb-first expected |
| `E25E26E27Tests.E25_PredictByRules_SVO_WhenNounBeforeVerb` | Прежний SVO expected |
| `E25E26E27Tests.E25_PredictByRules_SOV_WhenVerbLast` | Прежний SOV expected |
| `SemanticCompilerTests.SemanticCompiler_PatternClassifier_Imperative_VerbFirst` | Прежний native rule contract |
| `SemanticCompilerTests.SemanticCompiler_PatternClassifier_SVO_NounVerbNoun` | Прежний native rule contract |
| `SemanticCompilerTests.SemanticCompiler_PatternClassifier_SOV_EndsWithVerb` | Прежний native rule contract |

Sources: [ExperimentsV2Tests:1589–1614](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/Tests/ExperimentsV2Tests.cs#L1589-L1614), [SemanticCompilerTests:159–182](https://github.com/lubochka/llm_mvp_core/blob/a87fef91ba52e81c69dd63fbc094daa776f57095/Tests/SemanticCompilerTests.cs#L159-L182). На две руки это12прямых `PredictByRules`,0Train. Существующие `E25_Run_Tier1_ProducesResult`, `E25_Run_EnhancedNotWorseThanBaseline`, `E25_GenerateExamples_HasAllPatterns` сохраняются, но не запускаются данным filter: они добавили бы генерацию/fit и дополнительные hard calls за пределами бюджета.

Свежие падения **не воспроизводились и не объявлены установленными**. Если A проходит/Bпадает, чинить единственный factor: query set mutation, перенос до guard, потерянную stable-sort/group order или неверный private аргумент. Не менять old gold/expected/k и не ослаблять accuracy допуск прежнего E25. Если память получила другой порядок, исправить fixture loader/непреднамеренный Train diff. Wrong assembly исправляется ProjectReference/type binding, не копированием classifier. Build blocker — только общая [02§10 prerequisite](02-learning-state-and-fixtures.md#native-build-prerequisite), одинаковая A/B; весь solution не ремонтируется.

## 7. Exact native A/B, команды и конечный бюджет

Применить [03§9](03-test-and-execution-contract.md#native-ab-transfer). Будущие общие helpers `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` / `Compare-NativeAB.ps1` расширяются одной записью **FIX-PQ**. Сейчас они этим документом **не создаются и не запускаются**. Canonical root `C:/Projects/llm_mvp_core`, branch `codex/dpo-implementation`.

Сначала общий harness commit: новые tests/helper/fixture и только необходимая одинаковая build prerequisite при неизменённом product A. Его SHA — `$baselineCommit`. Выполнить terminal A; затем в том же корне вне runner внести только два участка PatternClassifier, review/commit → `$candidateCommit`; выполнить B. Native A/B не подменяются flag в одном candidate, worktree, sibling clone или прежними PQ source-copy DLL.

~~~powershell
Set-Location C:/Projects/llm_mvp_core
$baselineCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-PQ -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

После terminal A и отдельного product commit:

~~~powershell
$candidateCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-PQ -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

На stage: один `dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild`, один discovery **9cases=6existing+2contracts+1learning**, затем три отдельных test processes **existing→contracts→learning**. Все execution calls `-c Release --no-build`, собственные TRX/stdout/result paths. Ноль/лишние discovered tests — setup defect. Existing filter — OR шести точных FQN из§6 с `FullyQualifiedName=`. Contracts: `FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.PosQueryBigramContractTests.`. Learning:

~~~text
FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.PosQueryBigramLearningTests.OnePinnedBatch_PreservesLearnedAnswersAndMeasuresReaderCost
~~~

| Часть A+B | Train | Предъявленных строк | Predict | Прямые PredictByRules |
|---|---:|---:|---:|---:|
| Основное128-row acquisition/readout | 2 | 256 | 128 | 0 |
| Два новых contract Facts на руку | 2 | 8 | 20 | 0 |
| Шесть existing Facts на руку | 0 | 0 | 0 | 12 |
| Compare/report/snapshot analysis | 0 | 0 | 0 | 0 |
| **Общий предел** | **4** | **264** | **148** | **12** |

Всего6test processes/2Rebuild/2discovery. Внутренний вызов fallback из empty-memory Predict не считается отдельным внешним `PredictByRules`; отдельная последняя колонка — лишь12existing прямых вызовов. Hard-panel повторов0, генератора0. Deadline5min/test process,15min/Rebuild; срок не увеличивает число fits.

Output **`artifacts/fbe/FIX-PQ/attempt-0001/`**: `baseline/A`, `contracts/A`, `learning/A`, `after/B`, `contracts/B`, `learning/B`. CreateNew `intent.json` до первого содержательного вызова, `candidateCommit:null`; B отдельно создаёт `candidate-binding.json` с SHA/ancestry/intent digest. Native DLL Location/SHA/MVID, test/fixture/source hashes и runtime/ISA проверяются на каждой стадии. B принимает только свои пересобранные native assemblies, не старый A binary.

Learning arm сохраняет `arm.json` с всеми before/after arrays, `costs.json`, `input-manifest.json`, `state/learned-memory.json`, `state/manifest.json`, `ownership.json`, `status.json`, native provenance, completed/not_reached counts. В finally сохранить доступные raw перед исходной ошибкой; отсутствующее after не записывать как измеренные0correct. Final comparer читает готовые A/B и пишет `comparison.json`, русский `report.md` и `report-for-user.md`. Отсутствие/ошибка formatter не назначает ещё один fit.

Существующий completed output читается; live handle ожидается; неполный/неизвестный owner оформляется defect. Папки/claims не удалять, attempt-0002 не выбирать автоматически. Нативный measured allocation advantage оценивается фактически, без жёсткого threshold41,03%. Не воспроизведённый выигрыш фиксируется честно; полезное улучшение другой оси всё равно показывается со своими ограничениями и всеми регрессиями, но не заменяет подтверждение именно allocation-гипотезы.

## 8. Prompt→domain, итог и готовый промпт

Этот factor не создаёт learned route и не преобразует class0 в domain0. [FIX-E2E20](20-prompt-domain-e2e.md) сохраняет собственные selector/FlowRef/receiver/data/budget; FIX-PQ добавляет к нему0Train/Route/HTTP. Присоединить PQ receipt можно как provenance только фактически используемого project12 reader. Смена project29 fusion на project12 потребовала бы отдельного положительного опыта text→POS→выбранный receiver→ответ и здесь не назначена. Не объявлять прямые POS predictions выполненным пользовательским E2E.

Отчёт по-русски: точный native learner и factor → память0→128 и ответы11→28 → знакомые26/новые6 → все четыре after ошибки и две потери → source/memory/answer parity → after-reader allocation рядом с before/Train/CPU/peak → доменные/consumer/state-restore ограничения → кандидат решения. Исходные PQ raw/отчёт и TEST-POS34 не исправлять задним числом; выявленные неточности report объяснены выше отдельно.

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/51-pos-query-bigram-reuse.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только отдельно назначенный FIX-PQ; сейчас это документный план.
Модель исполнителя — default текущей сессии, без override; отчёты по-русски:
artifacts/fbe/FIX-PQ/attempt-0001/report.md и report-for-user.md.
Прочитай AGENTS.md/CLAUDE.md,02/03 и эту карточку. Skills в docs/skills:
architect-behavior-classifier-SKILL.md,trainable-architecture-decision-SKILL.md,
intent-to-plan-SKILL.md,phase-planning-SKILL.md,plan-review-SKILL.md,
code-examination-SKILL.md,generated-code-review-SKILL.md,test-integrity-SKILL.md,
prompt-patch-authoring-SKILL.md,human-communication-GLOSSARY.md,
human-readable-architecture-audit-SKILL.md. Не ищи другой host/model/corpus.
Native type SemanticCompiler.PatternClassifier project12; k3; pinnedPQfixture.
Один factor: queryHashSet раз наPredict послеemptyguard; память и голос прежние.
Main:2Train/256rows/128Predict; с точными contracts общий4Train/264rows/
148Predict+12directrulecalls,6testprocesses. Generator0;TEST-POS/E25Runне запускать.
Native Save/Load отсутствует:0reflection restore; один fit/руку и reuseinstance/raw.
Любой полезный measuredgain — успех оси/профиля,accuracy расти не обязана.
Не называй TRAIN-overlap generalization и не выдавай project29fusion за project12.
Default executor не заменяет CODEX astra 6 ultracode в отдельной роли выбора/
quality DPO. Внешняя открытая китайская модель генерирует новые DPO только по
отдельному назначению; здесь DPO/teacher0.
Один живой process/handle: execsession ждётся write_stdin того жеsession_id;
yielded functions.exec — functions.wait того жеcell_id только послеyield.
Иные tools сначала изучи по схеме; жди terminal в том же ходе порциями≤60секунд.
Не завершай раунд вместо ожидания;не делай blind polling,watcher loop,relaunch.
При STOP/NEXT немедленно прекрати прежнийID: запрещено дописывать егоreport,
tests,commit,cleanup. NEXT только поstableID реестра01,не старому номеруопыта.
Старые experiments/results сохраняй;ready receipts используй без новогоTrain.
~~~

Нынешний документ не выполнял product edits, build, tests, Train/Predict или публикацию обученного состояния. Будущий результат должен отдельно назвать execution status, learning validity, measured useful axes и ограничения; одних parity или количества записей недостаточно для заявления об обучении.
