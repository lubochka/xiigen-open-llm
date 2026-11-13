# Малые эксперименты: что именно проверить и каким результатом это опровергнуть

**Исторические проекты проверок.** Для нового поручения пользователя действуют [81 — практическая польза по любой измеренной оси](81-validation-program.md), [83 — предметные карточки](83-check-catalog-and-prompts.md), [85 — домены](85-domain-protocols.md) и [87 — новые поступления](87-new-checks-38-39.md). Числа `+2/6`, `+2/8` и подобные пороги ниже не становятся обязательной квотой нового опыта. Уже выполненные или зафиксированные исторические опыты задним числом не пересматриваются. Ни старый, ни новый документ сам по себе не разрешает запуск.

**Текущая фаза — READ_ONLY_RESEARCH.** Карточки ниже — проекты будущих проверок и сами по себе не разрешают реализацию/запуск. Э-01 был ошибочно запущен по отозванному work order оркестратора; след сохранён в [07](07-study-log-2026-10-16.md). Остальные карточки не исполнены. Причинное влияние не закрывается source review или согласием авторов. Область любого будущего вывода: **гипотеза × домен × тип задачи × фактический consumer**.

Формулировки «следующий исполнитель», «выбранный объём», команды и budgets ниже относятся к зафиксированному дизайну, а не к текущему поручению. После пользовательской коррекции их исполнение отменено. SHA fixture в Э-01 относится к версии, прочитанной до последующего обновления помощниками; его нельзя присвоить новым bytes файла с тем же именем.

Ниже сначала одна готовая малая проверка с уже существующим fixture, затем три следующие постановки и очередь других адресных контрастов. Это не предел числа гипотез: общий runner должен переиспользовать фиксацию входов, per-case сравнение и учёт состояния, сохраняя отдельные вмешательства. Нельзя объединять несколько новых признаков/целей/decoder-настроек в один «улучшенный» arm.

## Э-01 — первый: порядок действий в обученной памяти kNN

**Домен и вопрос.** Классификация порядка двух действий над одним объектом; ответ SAFE/UNSAFE. Проверяем Г-03 и helper H-04: мешает ли unordered representation обученному `KnnLogicalUnit` различать противоположные порядки? Это не C# code generation и не EN→RU. Закон fixture: backup должен предшествовать delete. Никаких файловых действий по этим текстам не выполняется.

**Уже имеющийся материал.** [Карточка помощника](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md>) и [fixture](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/11A-MICRO-KNN-ORDER-FIXTURE.json>). SHA-256 прочитанного fixture: `C1576AFB7D915252E99D3C6C01750AAF19936E3761CF8F75E6D24FCC4DF431A7`. Train: 16 строк, asset101…108; held-out: 16 строк, asset201…208. Это **simulation / диагностический набор**, не внешние DPO-пары. Два текста в паре имеют одинаковые слова: `plan backup asset201 before delete asset201` и обратный порядок.

**Что переиспользовать — первоначальный дизайн до ошибочного Э-01.** Настоящие [KnnLogicalUnit.cs](../../../21.AgentInfrastructure/Logical/KnnLogicalUnit.cs):103–153 `Train`, :202–256 `ExecuteAsync`, :266–299 `Vectorize`; HNSW и BPE из core. На момент первоначальной записи runner `tools/Rp01MechanismProbe/` ещё не существовал; позднее он был создан и запущен вне разрешённой фазы, см. [07, этап 9](07-study-log-2026-10-16.md). Дизайн предусматривал адаптер fixture, два набора `LogicalKnnTrainingExample<string,string>`, collector реальных outputs и inspector BPE/vector. Это историческая постановка, не новое задание на реализацию.

**Поправка к предложению помощника.** Его изменение textSelector дописывает слова и тем самым переобучает другой BPE (`Train:138–139`). Это мешает выделить роль order-признака. В нашей первой постановке `textSelector` — identity в обоих arms; BPE обучается по одним raw train-текстам в одном порядке. Меняется только наличие relation predicate:

| Условие | A | B |
|---|---|---|
| Train/input/output | те же 16 строк и метки | те же |
| Raw text, tokenizer corpus, order | исходные, без tags | те же |
| PredicateIds train и request metadata `predicate_ids` | пустые | один `FIRST_ACTION_BACKUP` либо `FIRST_ACTION_DELETE` |
| Parser признака | не используется | читает только первое действие из фиксированной грамматики; не читает label/SAFE/UNSAFE |
| Unit settings | dimension 64, k=1, floor=.70, domain `micro-order` | те же |
| Выдача | реальный Output через ExecuteAsync | то же |

Встроенный вес predicate=2 сохраняется. Повторы одного predicate бессмысленны: `NormalizePredicates` превращает их в HashSet (:303–309). Нельзя подбирать другое имя/вес после held-out оценки. Если два заранее объявленных predicate попали в один hash slot или не различили пары, это отдельный отрицательный инженерный результат, а не повод тихо сменить fixture.

**Preflight, затем качество.** До открытия held-out labels проверить одинаковые bytes сохранённого BPE A/B и токеновый слой векторов, disjoint IDs, отсутствие label в extractor. Вызвать настоящий private `Vectorize` через ограниченный диагностический inspector или read-only reflection в runner; не дублировать реализацию. Для каждой из восьми инверсий A ожидается равный вектор; B должен отличаться только внесённым predicate и итоговой нормировкой. Если A уже различает пары, исходная гипотеза об инвариантности не выполнена: `INVALID_PREMISE`, quality можно сохранить как разведочное, но не заявлять её доказанной.

**Seed и индекс.** `HnswIndex.cs:36` использует static unseeded Random; public `Train` не принимает seed. Поэтому утверждать matched seed для существующего API нельзя. Для строгого парного ANN-эффекта следующий исполнитель должен добавить минимальную opt-in диагностическую передачу одинакового RNG seed в Train→HnswIndex, сохранив текущий default, либо оставить ANN-результат явно exploratory. Предлагаемый фиксированный seed 42 и одинаковый порядок вставки делают графовый RNG парным, но не требуют одинакового графа при разных векторах. Это одна узкая вспомогательная возможность, не смена алгоритма поиска.

**Выбранный объём первого исполнения:** без production seed seam; использовать существующий core, три rebuild и exact-vector контроль. Возможность seed остаётся отдельным будущим опытом, если ANN-вариативность окажется существенной. Не расширять первый runner ради неё.

Если такой seam не включён в bounded work order, первый опыт всё равно может измерить механическую коллизию и реальный Unit на трёх заранее заданных rebuild; **его causal verdict ограничивается представлением и memory-dependent поведением, ANN-разность не считается строго изолированной**. Exact cosine по тем же полученным core vectors — диагностический контроль доступности правильного соседа, не подмена основного ExecuteAsync.

**Бюджет.** Три A/B rebuild: 6 вызовов Train по 16 примеров, 96 held-out ExecuteAsync. Один B с инвертированными только train labels: ещё 1 Train и 16 ExecuteAsync. Один replay сохранённого набора/параметров B: ещё 1 Train и 16 ExecuteAsync. Всего максимум **8 Train, 128 ExecuteAsync; 0 Transformer/Teacher forward, 0 DPO updates**. Exact-vector контроль — не более 7×16×16 сравнений cosine; никаких дополнительных seed поисков. Время заранее не обещается. Это обучение памяти/индекса, не вызов генеративной модели.

**Контроль зависимости от урока и state.** Сохранять ordered train IDs/labels, tokenizer bytes/hash, векторы/hash, выбранные neighbor IDs и полный Output. Label-flip должен изменить ответы при неизменных текстах и признаках: минимум 12 из 16 ответов B должны инвертироваться относительно обычного B. Это исключает helper, который сам возвращает SAFE/UNSAFE. Replay — восстановление по сохранённому recipe и проверка векторов/выходов; у `KnnLogicalUnit` в просмотренном API нет готового export/import полного trained unit, поэтому replay **не** объявлять проверкой cold load бинарного индекса. Проверку собственно persistence вынести в Э-09.

**Предзаданный результат.** На каждом из трёх повторов B ≥12/16, B−A ≥4/16, число ложных SAFE на UNSAFE не больше A; label-flip и representation gates пройдены. Это `LOCAL_MEMORY_QUALITY_GAIN` на одном законе с новыми именами объектов, не статистически уверенная многодоменная generalization. Если улучшились векторы, но не ответы — `REPRESENTATION_ONLY`. Нестабильность или малый прирост — `INCONCLUSIVE`; устойчивое ухудшение — `LOCAL_REGRESSION`; нарушенный контроль — `INVALID_COMPARISON`. Отрицательный итог ограничивает пользу **этого признака, веса и домена**, не весь kNN и не DPO.

**Изоляция — первоначальная постановка.** Предусматривался отдельный run directory `.kiro/xiigen-slices/work/results/MICRO-KNN-ORDER-<unique-id>/`, чтение исходного fixture/checkpoint/grades и фиксация intent до оценки. Ошибочный запуск позднее создал runner и JSON artifacts; их след и исходный SHA сохранены в [07, этап 9](07-study-log-2026-10-16.md). Seed seam не создавался. Сейчас эти материалы сохраняются без изменений; новых запусков нет.

## Э-02 — intent/episode: обучается ли полезный коэффициент смешивания

Связь: Г-02, И-02. Переиспользовать `35.ExperimentsV2/Experiments/E34HybridMatcher.cs:15–58,80–133`, `EpisodeCorpusLoader.Expand()` и BM25/FamilyCollapser. Исполнение через существующий dispatcher `RunE34V2` требует отдельного matched-evaluation метода: текущий Run имеет неодинаковые denominator.

Один и тот же корпус из 90 строк, прежний shuffle `Random(42)`: 72 memory train, первые 9 val — tune, оставшиеся 9 — закрытый test. Это перенос между фразами тех же типов эпизода, **не независимые новые классы**. Одинаковые кандидаты, словарь типов, нормализованный BM25 и Jaccard. Единственный фактор — alpha: A фиксирован .5, B выбран из .1… .9 по tune (tie: меньший alpha). Вторичный диагностический arm alpha=1 показывает BM25-only на **тех же** девяти test. Никакой коэффициент не подбирается по test.

Бюджет: 1 index build, 81 tune ranking (9 alpha×9 queries), 27 test rankings (3 arms×9), 9 replay после сохранения alpha; **0 neural calls/updates**. Обязательно хранить per-case top candidate и два component scores. State — выбранный alpha и memory fingerprint, после загрузки результаты должны совпасть. Техническое изменение alpha без test gain — только факт настройки.

Порог разведочного положительного результата: B имеет минимум 2 дополнительных правильных top1 из 9 против A и выигрышей больше проигрышей; replay равен. Иначе `NO_GAIN_ON_THIS_PANEL` или `INCONCLUSIVE`, без универсального FAIL. Малый test не даёт уверенной оценки общего качества. Внешняя семантическая метрика здесь — корректный **тип эпизода**; текст ответа не генерируется.

Этот опыт проверяет историческую настройку функции. Он не переносит E34 Jaccard в нынешний consumer автоматически. Следующий отдельный контраст Э-02b использует **нынешние** три сохранённых component scores и formula `a*lexical + (1-a)*(0.45/0.65*vector + 0.20/0.65*pattern)`: A a=.35 точно восстанавливает нынешние веса; B подбирает только a на tune. Candidate bank/features неизменны; если scores ещё не сохранены, нужны 90 embedding calls на одном frozen checkpoint/tokenizer, не DPO. Этот отдельный бюджет не входит в первый запуск.

## Э-03 — C# next-token: исправить только координаты BPE→AST

Связь: Г-05/H-01. Реальный runner — `E07StructuralContextToken.Run`, mapper :138–225 и `MultidimensionalNGram.Train/Predict:283–315`. Не исправлять tier keys одновременно с alignment. A использует текущий `posMap[tokenOrdinal]`; B использует начало фактического BPE token в исходном тексте. Остальная логика выбора AST metadata, ключ `(bpe,ast,depth)`, fallback и tie-break одинаковы.

Материал: по одному frozen snapshot следующих файлов core: train — `01.Tokenizer/BpeTokenizer.cs`, `01.Tokenizer/RegexSplitter.cs`, `06.VectorSearch/HnswIndex.cs`, `12.SemanticCompiler/Prediction/StructuralKnn.cs`; test — `09.Inference/TextGenerator.cs`, `04.TrainingLoop/Optimizers/AdamW.cs`. Исходники не редактируются. BPE512 обучается только на train, один раз; оба arms получают одинаковый tokenizer. Ограничение: первые 4 096 train tokens и первые 128 допустимых next-token позиций каждого test-файла, всего 256 test. Если позиций меньше, не дополнять другой выборкой после результата.

Нужен исследовательский offset mapper, проверяющий реконструкцию UTF-8 bytes; нельзя принять декодирование каждого отдельного BPE token за корректный Unicode offset без проверки. Бюджет: 1 BPE fit, 2 N-gram Train, 512 next-token predictions; neural calls/updates 0. Данные для коррекции alignment не содержат next-token label.

Наблюдения: доля изменившейся AST metadata; counts обученных переходов; paired correct/incorrect next token; fallback rate; отдельно два source files. Порог: минимум +8 correct из 256 без ухудшения ни на одном из двух файлов. Если alignment изменился, а gain нет — гипотеза полезности этой коррекции на панели не поддержана; маленький корпус и разреженность позволяют `INCONCLUSIVE`, не общий приговор структуре. Это качество структурного next-token learner, не доказательство правильного объяснения C#.

Отдельный Э-03b меняет **только** key с triple на triple+parent при уже одном фиксированном alignment. Э-03c сравнивает E09 bigram/structural kNN на тех же первых 256 AST transitions, с отдельным контрастом local/global SyntaxKind labels. Эти исправления нельзя слить в один treatment.

## Э-04 — токенная policy: сумма response likelihood против среднего

Связь: Д-03, Д-05, И-04. Переиспользовать `LogicalDpoPreparedBatchMaterializer`, `DpoExactPairMaterial`, `DpoTrainer.CreateForProjectLora/StepVector`, нынешний evaluator и `tools/Rp01SimulationDpo`. Public runner требует 500-source contract, поэтому **не обходить этот guard**: добавить отдельную research-команду на прямых core owners с изолированными входами и выходами. Нынешний main500 не запускать.

Первая область — C# documentation. Из frozen simulation train выбрать 8 пар: по первому ID из 8 разных существующих template groups; до score объявить 8 held-out из других групп/кода, удовлетворяющих тем же конкретным законам, и ручную рубрику смысла. Если такой независимой панели нет, её подготовка — незавершённая предпосылка, не право назвать имеющийся same-template test independent. Не создавать новые teacher/DPO ответы в этом work order.

Оба arm стартуют из **одних bytes C50**, с одним tokenizer, optimizer/RNG snapshot, разрешёнными 16 LoRA и замороженной отдельной clone reference. Данные/order/seed/LR/beta/decoder/max tokens одинаковы. Только objective: A response sum; B response mean с согласованным backward scale. Target masks остаются response-only. Не одновременно менять base, LoRA rank, EOS, teaching vector или данные.

Бюджет: по 4 обновления с batch2, одна презентация каждой пары; всего **8 optimizer updates**. При текущем вычислении policy/reference chosen/rejected — плановый верхний счёт 4 forward на пару в StepVector: 64 последовательностных forward, 32 backward; исполнитель должен вывести фактические счётчики, не принять оценку за measured. Baseline+два post-state: 3×8×2=48 scoring forward, 3×8=24 свободных generate calls (длина фиксируется до запуска). Без самостоятельного повторения с другими seed после плохого результата.

State control: базовые tensors/ref неизменны, разрешённые LoRA finite и действительно изменились, checkpoint reopen совпадает; zero-update control можно получить повторной оценкой baseline без нового Train. Метрики отдельно: objective loss, mean/sum preference gap и wins, реальные правильно объяснённые ответы, критические смысловые ошибки. Положительный exploratory quality-effect: B даёт минимум 2 дополнительных правильных из 8 против A и baseline, без новых критических ошибок. Только изменение градиента или preference — `ENGINEERING_OR_PROBABILITY_EFFECT`, не semantic learning gain.

Отрицательный исход с почти случайной базой и восемью парами имеет слабую силу против всей идеи DPO: он ограничивает этот локальный objective-контраст. Результат на C# нельзя переносить на EN→RU. Повторный домен получает собственные пары и рубрику сохранения отрицания, ролей и фактов, с тем же matched-state принципом.

## Очередь остальных существенных гипотез: общая оболочка, отдельные вмешательства

Статус каждого пункта — **experimental pending**. Значения ниже — предложенные ограниченные budgets, не выполненные действия. Для новых наборов нужны отдельные фиксированные IDs/рубрики до исполнения; отсутствие готового fixture отмечено.

| ID / находка | Домен → consumer | Одно вмешательство и контроль | Малый бюджет / принятие и предел |
|---|---|---|---|
| Э-05 / Д-03, H-03 | intent routing → E51B `Compile/Nearest` | shared train root vocabulary против per-prompt vocabulary; те же teaching/queries/labels и exact cosine | Существующие 4 TE + 2 CH; 12 rank calls. Измерять subject/top-k, не oracle final text; улучшение ≥2/6 — local signal, без generalization claim. |
| Э-06 / Г-04, Г-06 | C# predicates → E51A `RunTrace` | memory enabled vs пустой/перемешанный label memory при одном classifier и features | Первоначальный дизайн: 13 прежних V плюс 6 новых source contexts; 38 predictions. Позднее помощники добавили `15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json` с иной постановкой. Его тело здесь ещё не верифицировано; числа и готовность не переносятся между версиями. Важен выигрыш на новых contexts. |
| Э-07 / Г-06 | flow routing → E51B `RunHeldOutTrace` | реальные output fields против oracle fields, **один** output supplier; отдельная CH-ID permutation | 4 TE outputs×2 +2 CH×2. Если реального supplier нет, честный `MISSING_CONSUMER`, не fabricate generation. ID permutation должна сохранять содержательное решение; это проверка controller, не LoRA. |
| Э-08 / Г-04, Г-07 | EN→RU → `ExecutorUnit.Execute` и Student | при одном frozen Student включить/выключить один trained-memory bridge; одна память, те же 8 law prompts | 16 итоговых ответов; semantic rubric negation/roles/entities, +2/8 и ноль новых critical. Это системная польза памяти; вклад LoRA проверяется отдельно при фиксированной памяти. Fixture/bridge ещё нужны. |
| Э-09 / Д-04, H-05 | каждый owner отдельно → его обычный consumer | live trained state vs cold import **того же** state; tokenizer/index/units/weights identity | 8 paired outputs на owner, 0 updates. Equality доказывает replay, не обучение. Нельзя тестировать TranslationMachine metadata API и объявлять RP01 LoRA restore сломанным. |
| Э-10 / Д-01 | C# doc policy, затем отдельно EN→RU | fixed base: q/v recipient vs расширенный recipient, всё остальное равно | По 4 update; точный allowed map до запуска. Только semantic gain ≥2/8, без new critical; negative с random base не исключает пользу capacity на хорошей базе. Новая recipient реализация нужна; не первый опыт. |
| Э-11 / Д-01 | C# doc policy | одна и та же DPO процедура на original base vs уже проверенном обученном base | По 4 update, 8 held-out. Прежде нужен настоящий donor checkpoint и его provenance; «v309 high accuracy» не удовлетворяет. Разные tokenizer/models делают contrast invalid. |
| Э-12 / Д-05 | конкретный закон C# либо EN→RU → DPO recipient | исходные 8 пар vs 8 пар с одной содержательной коррекцией rejected; одинаковая длина/семейства насколько возможно, judged до run | По 4 update; та же 8-case family holdout. Если меняются одновременно judge/model/base, эффект пары не выделен. Исправленные пары ещё не подготовлены. |
| Э-13 / Д-02, H-06 | тот же текст → tokenizer/materializer/decoder | exact roundtrip и masks; затем один stop-token contract при одном checkpoint и greedy decode | Сначала 8 token/material checks, 0 model calls; затем 16 generate calls. Изменение stop reason без смыслового gain — только decoder effect. EOS primary source ещё требует адресного чтения. |
| Э-14 / Д-04, retention | C# после EN→RU → тот же consumer | без replay vs фиксированная доля replay, одинаковый общий update count | Только когда old-domain baseline >0: по 4 update и по 8 old/new answers. Сохранение 0/3 не доказывает retention навыка. |
| Э-15 / Г-03 | C# AST → StructuralKnnPredictor | то же расстояние/кандидаты, exact vs ANN index | 256 matched queries, 0 neural updates; Recall@k и next-node accuracy отдельно. Успех retrieval не равен code generation. |
| Э-16 / И-01 | tiny neural preference → исторический trainer | один флаг вызова backward; одинаковые мини-модель/policy/ref/material/seed | 1 update×2 arms, 2 pairs и fixed scoring; compare weights/gradient and same-input margin. Проверяет механический эффект старого дефекта, не восстанавливает точный v309 run и не обещает смысловую генерацию. |

Существенные выводы о git-присутствии или числах сохранённого JSON не требуют переучивания для проверки факта чтения. Их **причинное толкование** покрывается карточками выше. Точная историческая matrix→kNN родословная остаётся provenance-вопросом; эксперимент может проверить полезность механизма, но не заменить отсутствующий commit.

## Общие правила результатов

Для каждого отдельного домена сохранять source/fixture/tokenizer/checkpoint hashes, единственное вмешательство, train/tune/test IDs, allowed state, реальный consumer, цены в calls/updates и измеренное время. Полезное изменение проверяется по смысловому закону задачи; loss, margin, hit@1, next-token accuracy и learned-state update не складываются в одну «точность».

Нельзя объявлять fixed extractor обученным: нужен изменяемый state и зависимость от учебных меток. Нельзя объявлять выбранный alpha LoRA-обучением. Нельзя считать replay сохранённого recipe доказательством импорта бинарного trained state. Если различение не удалось, сохраняется `INCONCLUSIVE` с конкретной причиной; гипотеза не закрывается голосованием авторов.

Здесь сохранены проекты возможных будущих проверок. Само их описание не разрешает изготовление harness или исполнение. До ошибочного Э-01 существовали только документы; затем были созданы runner и runtime artifacts, что отдельно отмечено в [07](07-study-log-2026-10-16.md). Текущая работа снова ограничена чтением и Markdown; существующие исходники и artifacts не дорабатываются.
