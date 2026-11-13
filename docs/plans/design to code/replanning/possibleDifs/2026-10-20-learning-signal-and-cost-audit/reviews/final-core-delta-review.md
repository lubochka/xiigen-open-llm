# Ограниченный аудит финальной core-дельты 62bcc8f

**Проверенная версия:** `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`, 20.10.2026. Прочитаны новая карточка N-KU-01 целиком (552 строки) и изменения серии 23 относительно `f5dd532045716495a2c7668573250e5d7cc46810`; отдельно перечитаны N60-E1/E2 и нужные участки ранее сохранённого исходного кода. Старые защищённые snapshots не заменялись. Код проекта, карточки на GitHub, тесты, сборки и обучение не запускались и не изменялись.

## 1. Итог и отношение к нашему LF01

**Новая N-KU-01 теперь является конкретно описанным малым опытом по доставке учебной метки в память и голосование.** Она сохраняет четыре TRAIN и k=3, честно ограничивает вывод константным большинством и впервые содержит полный текст изолированного runner, подготовки копий, provenance и двух команд исполнения. Это техническая спецификация будущего запуска, не уже собранный host и не результат. [Карточка:3–9, 91][card-intro] [Прямое указание отсутствия проверки сборки и запуска:548–552][card-status]

**Наш LF01 с шестью TRAIN не следует объявлять обязательным исправлением или заменой этой новой карточки.** Четырёхстрочный опыт допускает полезный локальный эффект «меньше лишних ADD_LOOP» с явно показанным ухудшением обнаружения отсутствующего цикла. Он действительно включает native `Train` и последующий собственный `Predict`. Наш шестистрочный профиль отвечает на более сильный вопрос: остаётся ли эффект при отсутствии математически неизбежного константного большинства. Он сохраняет смысл как отдельный опыт на различение примеров, с отдельным ID и зафиксированным заранее набором; не добавляется к текущему запуску и не служит условием его допуска. Общее улучшение качества структурного обобщения не доказано ни для одного профиля.

## 2. Точный текущий N-KU-01

| Поле | Что предписывает новая карточка |
|---|---|
| Один фактор | В B добавить только `SyntaxKind.DoStatement` в `ActionLabelGenerator.Label/hasLoop`; механическая изоляция namespace в обеих копиях |
| Получатель | Настоящий `FileActionKnnPredictor` из полной копии исходного файла; фиксированы k=3, прежние 30 признаков, cosine и голосование |
| TRAIN | K1 Sum/do, K2 CountPositive/do, K3 Product/for, K4 CountZero/без обхода; один и тот же порядок и исходники A/B |
| Учебные метки | A: 3 ADD_LOOP + 1 ADD_METHOD. B: 1 ADD_LOOP + 3 ADD_METHOD |
| EXAM | KQ1 SumSquares/do, KQ2 AnyNegative/do, KQ3 CountOdd/while, KQ4 SumEven/без обхода; первые три gold ADD_METHOD как узкий fallback «не добавлять цикл», последний ADD_LOOP |
| Начальное состояние | Новый пустой predictor на плечо, размер 0; checkpoint, LoRA и RNG не нужны |
| Порог | Перенесённый guard `<4` возвращает null, не вызывает FeatureVector/Predict |
| Native Train | 1 на A + 1 на B = 2; переданы 4 + 4 = 8 учебных строк |
| Подготовка | 8 учебных FeatureVector и 8 учебных Label суммарно; EXAM Label=0 |
| Панели | 16 запросов A0/A1/B0/B1: 8 guard-null до обучения + 8 реальных Predict после; EXAM FeatureVector=8 |
| Прочие вызовы | Facade.Compile=0, optimizer=0, Flow=0, warmup=0, repetitions=0 |
| Процессы и сборка | Одна будущая сборка малого net9.0 exe, затем два свежих процесса A→B; без ProjectReference на общий проект |

Основания: [точные TRAIN/EXAM и аналитическое ограничение:71–91][card-data], [runner dispatch и guard:179–248][card-runner], [Train и счётчики:262–301][card-counts], [будущие команды:455–489][card-commands], [полный бюджет:491–515][card-metrics].

При любой тройке соседей A выбирает ADD_LOOP, B — ADD_METHOD. Следовательно, **аналитически ожидаются**, но пока не измерены: A1=1/4, B1=3/4; false ADD_LOOP 3→0; recall единственного missing-loop 1→0; balanced accuracy 1/2→1/2. A0/B0 дают null, которым назначена нулевая правильность в этой панели. Изменение размера таблицы 0→4 и реальные ответы будут свидетельствами доставки меток, но не структурного обобщения. Новая карточка прямо принимает эту границу и запрещает менять k/набор после результата. [Карточка:89–91, 530–536][card-limit]

**Точность счёта в серии 23:** её строка 136 продолжает говорить о двух внешних и двух вложенных Train. Это обозначение логической подготовки/обёртки и native вызовов, а не четыре обучения. В полном runner нет отдельного метода `TrainFileActionKnn`: подготовка перенесена в `Measure(...prepare...)`, а callback вызывает native `Train` один раз на плечо. Для итогового отчёта использовать фактический native count=2 из новой карточки и при необходимости отдельно подписать логические уровни. [Серия 23:133–143][series-ku] [Runner:262–301][card-counts]

## 3. Что означает новая policy-вставка

Diff серии 23 имеет только три области: добавление начала 1–21, добавление адресов экспериментальных копий/host в N-KU-01, замена полного facade/Compile-пути изолированным адаптером и обновление его prompt. N60-E1/E2 и нейронные MASK/STATE/PAD/ORDER в этой дельте не переписаны. Сохранён локальный `23-vs-f5dd.diff`.

Вставка задаёт следующий смысл будущей реализации: копия нужного исходного механизма с одним фактором A/B; оригинал остаётся источником и будущим адресом переноса; ограниченный exe компилирует только выбранные файлы; никаких автоматических широких тестов, CI, Main500, warmup или повторов. Недостающие доступные входы готовит исполнитель, вместо нового личного разрешения на каждый ID/SHA. Снимок bytes/hash фиксируется один раз; чужое движение Markdown HEAD не требует бесконечного перечитывания и перезапуска. Решение о переносе в продукт отделено от малого опыта. [Серия 23:3–21][series-policy]

Policy также сохраняет отдельный LEARNED_OUTPUT: допускается общий сохранённый обученный state для зависимого от обучения участка. Для нового learning claim требуется реальное изменение памяти/модели, для чтения уже обученного результата повторный Train ради формальности не нужен. Полезная метрика может дать успех независимо от accuracy; ухудшения соседних метрик показываются рядом. [Серия 23:10–11, 24–26][series-policy]

Это изложение требований внешнего документа, **не самостоятельное поручение запускать опыт в текущем документном аудите**. Наличие паспорта и отсутствие нового approval-gate не следует смешивать с фактом materialization или runtime.

## 4. N60-E1: «быстро/тихо» не исправлены

**Ни примеры, ни marker-путь в E1 эта дельта не чинит.** В актуальной серии 23 по-прежнему V1 «Он быстро открыл дверь»/quickly с совпавшим отрицательным текстом и V2 «Она тихо закрыла окно»/quietly с реальным omission; те же marker-семейства присутствуют в EXAM. По-прежнему записано NEEDS_INPUT и указание не чинить одновременно N57, если marker пуст. [Серия 23:213–243][series-e1]

Адресное чтение исходного кода объясняет техническую проблему материала:

- `быстро` и `тихо` отсутствуют в `RuAdverbAllowList` и не заканчиваются на поддержанные нормальные UTF-8 суффиксы `но/ко/ло/тельно/щенно/ённо/енно`. В этих предложениях наречие не получает ADV по указанным ветвям. [Tokenizer:89–96, 260–284, 413–429, 574–585][tokenizer]
- Формы прошедшего времени дают непустые RuleTags. Поэтому fallback `ExtractSourceMarker` не принимает произвольное UNKNOWN-слово: требуется `LooksLikeMannerMarker`. Этот helper содержит mojibake-окончания и `ly`, а не нормальные окончания `быстро/тихо`. [Tokenizer:153–177][tags] [Verifier:1265–1299][marker]
- Нужная запись `_requiredTermsBySourceMarker[marker]` появляется только при непустом marker. Прямой вывод чтения: заявленная marker-ветка для данных примеров не достигается. Это не проведённый runtime-замер. Сам подсчёт 11 Unit calls и 3 Verifier calls ещё не доказывает, что требуемое состояние записалось. [Verifier:1095–1143][writer]

Минимальная будущая правка **материала**: выбрать для обоих плеч одинаковые заведомо поддержанные ADV из текущего allowlist, например `нежно/gently` и `дерзко/boldly` (окончательная LF-04 выбрала `нежно/gently` и `горестно/sadly`), и согласовать TRAIN, EXAM и независимый gold до запуска. Тогда единственным A/B-фактором остаётся admission совпавшего verifier candidate. Добавлять одновременно новый tokenizer/marker-fix нельзя: получится второй фактор. Это предложение не внесено в защищённую карточку и не объявлено выполненным.

У E2 сохранился `OPEN_STATE_AND_READER`: нужен точный versioned source-support writer и настоящий ordinary consumer, которых текст ещё не задаёт. Его «runtime calls=0» прямо относится к документной фазе, а не новому разрешению пользователя. [Серия 23:245–255][series-e2]

## 5. Техническая готовность и действительные результаты

| Уровень | Текущий статус |
|---|---|
| N-KU-01: дизайн и полный текст host | SPECIFIED; счётчики и связь Label→Train→Predict согласованы с узким выводом |
| Файлы runner/копии/provenance | Только будущие пути и текст в Markdown; создание не заявлено |
| Сборка, зависимости SDK/package, actual A/B | NOT_RUN; успешная сборка/время/память не доказаны чтением |
| Числа 1/4→3/4 и цена recall | ANALYTICAL_EXPECTATION, не actual receipt |
| N60-E1 | Технический дефект выбранного marker-материала остаётся; новый personal approval не нужен для его подготовки в уже порученной программе |
| N60-E2 | OPEN_STATE_AND_READER; полная команда и native count пока не специфицированы |
| Материал protected results | В текущей дельте мной не прочитан; отсутствие проверенных receipts не превращать в утверждение, что никаких внешних результатов вообще не существует |

Описанные измерения N-KU-01 различают wall, process CPU, current-thread allocation, managed heap, RSS и process peak. Peak честно включает startup/JIT/измеритель/сохранение и не выдаётся за Train-only RAM; submitted table не называется native checkpoint export. Однократный cold-замер допускает описательную дельту, но не устойчивый speedup. Для этого узкого опыта новые обязательные повторы или facade-контроль не требуются. [Карточка:503–517][card-metrics]

## Сохранённые источники

- `N-KU-01-do-loop-label.md`: Git blob `0a86e79086c67ba3a1140991cb8cb91471b39d68`, SHA256 `b0dc8b9cb8ebad0ebd37d06fe2eb1112821cd520320a53e80e28e3dce4b49537`.
- `23-additions-n60-small-learning.md`: Git blob `2d2078598762956a8cffb58a39664310ae5698f7`, SHA256 `8c5449600770d4e18f0b181fada5e716c7b0b18c4c42a095d31b9db492841ec7`.

Git blob hashes локальных UTF-8 bytes совпали с ответами GitHub connector. Это проверка сохранённых документов, не запуск кода проекта.

[card-intro]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L3-L91
[card-status]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L548-L552
[card-data]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L71-L91
[card-runner]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L179-L248
[card-counts]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L262-L301
[card-commands]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L455-L489
[card-metrics]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L491-L517
[card-limit]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L530-L536
[series-ku]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L133-L143
[series-policy]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L3-L26
[series-e1]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L213-L243
[series-e2]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L245-L255
[tokenizer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L413-L429
[tags]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L153-L177
[marker]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1299
[writer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1143


---

# Финальная сверка перед публикацией: core 7c571

**Последний зафиксированный pin этого аудита:** `7c57183670ccaf554a47c0a4f44cabadde8c303d`. После него HEAD повторно не проверяется. Готовый planning-пакет не изменялся. Core только прочитан; запусков project code, tests, builds, training и inference не было.

## 1. Код исполнения не изменился

[Exact compare 62bcc → bcf][prepub-compare-bcf]: один коммит, 145 изменённых Markdown, только protected `experimets/`; одинаковое добавление про сохранение существующих экспериментов, результатов и bin/obj. `a8222d1b15745fe6207f36cd294fd3fb61c01d3b` — прямой родитель уже проверенного 62bcc, а не новая после него версия.

[Exact compare 62bcc → 7c571][prepub-compare-current]: пять коммитов, **148 файлов: 145 modified и 3 added; все Markdown внутри protected experimets**. Изменённых `.cs`, `.csproj`, весов, входных JSON или бинарников нет. Список путей/blob hashes и шесть уникальных patch-текстов сохранены в `compare-manifest.json`; повторное чтение 125 одинаковых policy-вставок не требовалось.

Три новых документа: `cards/N61-dpo-reference-reuse.md`, `cards/N61-mode1-retention-and-identity.md`, `results/2026-10-20/N-KU-01/preregistration.md`. Последний записан до сборки/запуска, не является result receipt.

## 2. Какие правила и пути учитывать

| Точное место | Требование для соответствующей версии будущего опыта |
|---|---|
| [P01:5][prepub-p01] | Малый host может использовать throw, try/catch, void, DTO и свой журнал; продуктовый OperationResult/code-slice workflow не требует переделывать экспериментальный API. Ошибку сохранять с этапом/счётчиками и ненулевым exit code |
| [P01:10–13][prepub-p01] | Копии: `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/<ID>/`. Host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/<ID>/Runner.csproj` и `Program.cs`; конкретный паспорт может задавать более точное имя. Results: `.../docs/plans/replanning-possible-diffs/experimets/results/<дата>/<ID>/`. Оригинал — source и будущий target переноса |
| [P01:17][prepub-p01] | В этом общем паспорте executor/reviewer gpt-6-astra/ultra, последовательно, максимум один worker; это описание внешней policy, не глобальное назначение для наших LF-prompts или уже начатого чужого опыта |
| [P01:21–23][prepub-p01] | Сохранить существующие эксперименты, исходники, results, bin/obj; новая редакция карточки не назначает второй запуск, cleanup, перенос или перезапись. Работать по зафиксированному снимку |
| [N-KU:56–57, 73, 110–114][prepub-ku-paths] | Для ещё не начатого нового варианта: копии `NKu01/FileActionKnn.SourceA.cs`, `SourceB.cs`; host `.../N-KU-01/NKu01Runner.csproj`. Уже начатые `NKu01/VariantA`, `VariantB` и `N-KU-01/runner` сохраняются без автоматической миграции/пересборки |

Новый N61 cache-паспорт отдельно задаёт `.../experiments-v2/RP01-MICRO/N61-REF-CACHE/Runner.csproj` и копии `.../35.ExperimentsV2/Experiments/Rp01Micro/N61RefCache/`. Он прямо требует **не добавлять режим в оригинальный dispatcher и не ослаблять guards**; source-linked dependency/capability контракт ещё должен быть спроектирован. Это техническая подготовка, а не новый approval blocker. [N61:59–65][prepub-n61-cache]

**Различать два N-KU-паспорта.** Новая dedicated card задаёт no-warmup вариант и reviewer Astra; preregistration старого снимка указывает VariantA/VariantB, `runner/`, нейтральный warmup, post-measure product-copy control, Claude/Kiro executor и Kiro reviewer. Это claims разных документов. Они не являются нашим назначением ролей и не дают оснований мигрировать уже начатый опыт. Текущая dedicated card:73 прямо сохраняет старый порученный снимок; preregistration:3 говорит «до сборки и запуска». [Preregistration:15–23, 57–68][prepub-prereg]

## 3. NO_EXTRA_LOOP: аналитика LF01 сохраняется

В новой dedicated N-KU-card gold KQ1–KQ3 изменён с `ADD_METHOD` на `NO_EXTRA_LOOP`; KQ4 остался `ADD_LOOP`. Изменена и проверка в тексте runner: [строка 285][prepub-ku-predicate].

```csharp
actual is not null &&
    (row.Gold == "NO_EXTRA_LOOP" ? actual != "ADD_LOOP" : actual == row.Gold)
```

Это более широкий predicate, а не простое переименование: теоретически он принимает и другой непустой совет, кроме ADD_LOOP. Но **в фиксированном четырёхстрочном наборе и предложенном нами шестистрочном наборе учебные labels только ADD_LOOP/ADD_METHOD**. Поэтому на этих профилях прежняя бинарная семантика совпадает с новой.

Для **4 TRAIN/k=3** аналитические ожидания остаются: A1=1/4, B1=3/4; false ADD_LOOP 3→0; missing-loop recall 1→0; balanced accuracy 1/2→1/2. A0/B0 null не засчитывается. Это не actual measurements. Для **optional 6 TRAIN/k=3** заранее известна только отмена математической неизбежности константного большинства; ни 1/4→3/4, ни полезный эффект, ни структурное обобщение на него не переносятся.

**Готовая короткая замена для root:**

> На pin 7c571 gold первых трёх задач — NO_EXTRA_LOOP: непустой ответ, отличный от ADD_LOOP; четвёртой — ADD_LOOP. На фиксированном наборе доступны только labels ADD_LOOP/ADD_METHOD, поэтому аналитика четырёхстрочного опыта не меняется: 1/4→3/4 по доле правильных ответов, balanced accuracy 1/2→1/2, с потерей обнаружения KQ4. Шестистрочный LF01 — отдельный необязательный профиль без гарантии результата; он не заменяет и не перезапускает уже начатый N-KU-01.

Кроме gold новая card добавила `EnableDefaultNoneItems=false`, явную запись Binding learner/assembly/Label/Feature/Predict, подготовку каталога и запрет импровизировать другой entry. Это изменения **кода в Markdown**, не `.cs/.csproj` проекта. В серии 23 дублирующий N-KU-паспорт заменён ссылкой на dedicated card; прежняя неточность «внешние+вложенные Train» больше не является актуальной отдельной инструкцией. Четыре TRAIN, k=3, два native Train и 16 запросов сохранены. [Card:131–153, 344–355][prepub-ku-binding] [Series23:109–119][prepub-series-ku]

## 4. E1/E2 и покрытие новых ресурсных гипотез

Текст от `## N60-E1` до следующего общего crosswalk в серии 23 **побайтно совпадает по содержимому с проверенным 62bcc**. Никакой правки быстро/тихо, marker-helper или E2 writer/reader здесь не произошло. Сохраняются установленная чтением проблема недостижимости intended marker на выбранных E1 примерах и `OPEN_STATE_AND_READER` E2. [Series23:187–229][prepub-series-e1e2]

Две новые N61-card прочитаны адресно по фактору, owner, состоянию, бюджету, размещению и измерениям; весь корпус заново не аудировался.

| Наша конкретная причина | Что содержат новые N61-card | Вердикт покрытия этой дельтой |
|---|---|---|
| DpoTrainer.BackwardLogProbs: `expVals = new float[v]` внутри target-loop; один scratch per invocation | N61 cache переиспользует **готовые reference scores** в StepVectorCore при повторных updates; policy forward/backward намеренно сохраняет. Это другой расчёт и другое изменение | Точного expVals-содержания/исправления нет; reference-cache не закрывает эту причину |
| Tensor.Backward(initialGradient).Data.ToArray → TensorStorage повторно ToArray | Ни N61 reference-cache, ни Mode1 retention/identity не изменяют Tensor/TensorStorage или seed-gradient ownership | Точного содержания/исправления нет |
| E1 быстро/тихо → empty marker → required-term writer не достигается | N61 Mode1 меняет merge feature memory и reader flow identity; N61 reference-cache не меняет перевод | Не покрыто; E1-body неизменён |

Поиск по обеим новым card не нашёл `expVals`, `BackwardLogProbs`, `initialGradient`, `TensorStorage`, `ToArray`, `N60-E1`, `NO_DISTINCT_NEGATIVE`. Слово «быстро» в Mode1 встречается лишь в исходной общей просьбе. Проверка смысла фактора подтверждает, что это не пропущенные синонимы нашей причины. Это **ограниченный novelty join двух новых карточек**, не глобальное доказательство отсутствия любых прежних идей.

N61 Mode1 отдельно правильно предупреждает, что `GetAllocatedBytesForCurrentThread` нельзя переносить через произвольный await; предлагает process-wide allocation delta с явной областью. Это методика измерений, не устранение двух описанных training-allocation причин. [Mode1:390–396][prepub-n61-mode1]

## 5. Граница результата

В compare появились только документы. Ни новые N61-specifications, ни N-KU preregistration не доказывают фактический запуск. Dedicated N-KU:588 и новые N61 явно оставляют actual-поля неизмеренными. Не следует писать, что никаких внешних runtime-результатов вообще не существует: данный bounded join не исследовал пользовательскую рабочую машину. Достаточное утверждение — **в этой GitHub-дельте runtime source/receipts не изменены; preregistration не receipt**.

[prepub-compare-bcf]: https://github.com/lubochka/llm_mvp_core/compare/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39...bcf880f232d090e3cfcb459c108d3945ce6dbc15
[prepub-compare-current]: https://github.com/lubochka/llm_mvp_core/compare/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39...7c57183670ccaf554a47c0a4f44cabadde8c303d
[prepub-p01]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L5-L25
[prepub-ku-paths]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L56-L114
[prepub-ku-predicate]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L279-L286
[prepub-ku-binding]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L344-L355
[prepub-series-ku]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L109-L119
[prepub-series-e1e2]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L187-L229
[prepub-n61-cache]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L38-L65
[prepub-n61-mode1]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L390-L396
[prepub-prereg]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/preregistration.md#L15-L68
