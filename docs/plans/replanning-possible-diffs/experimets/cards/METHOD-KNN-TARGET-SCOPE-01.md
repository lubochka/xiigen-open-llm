# METHOD-KNN-TARGET-SCOPE-01 — область целевой категории обучаемой памяти методов

<!-- CODEX-POST131-CURRENT:START -->
## Действующее уточнение после P131 — 3 ноября 2026

**Проверка `POST131-METHOD-NATIVE-TARGET-BINDING`. WRONG:** тот же SourceRowId, правильный reported target и длина native ответа доказывают, что Predict использовал нужный обученный payload. **CORRECT:** связать pinned source/symbol/span и extracted target с фактическим `_pairs.Next.KindBag`, затем с уже назначенным native Predict; итог пересчитать из действительного output. **Путь приёмки:** `ExtractWithTargetScope`→`TrainWithTargetScope`→actual target memory→native Predict→independent scope/answer comparator. На сохранённой копии заменить reported actual/KindBag при прежних IDs/counts/length и неизменной native записи: общий integrity verdict обязан измениться. Неверный, но согласованно записанный ответ learner остаётся научной ошибкой панели. Не выдумывать недоступные neighbor trace, checkpoint API или дополнительную Compile/facade ветвь; исходные наборы и бюджет сохраняются.

**Область:** связь raw с native проверяется только по реально существующим либо явно назначенным записям. Если такого источника нет, записать точный предел подтверждения. Этот блок не назначает новый Train/Flow/Load, persistence API, повтор PRIMARY или новую научную очередь. Integrity refusal и честный отрицательный scientific result различаются.

**Передаваемая исполнителю дельта:**

> Свяжи source/symbol/span и extracted target с actual _pairs.Next.KindBag, затем с уже назначенным native Predict. На копии подмени reported actual/KindBag при прежних IDs/counts/length: несовпадение с имеющимся источником должно изменить integrity verdict. Не выдумывай neighbor trace, checkpoint API или Compile/facade. Честный неверный результат learner учитывай отдельно от целостности записи. Применяй [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md); исходный паспорт, gold, factor и дозы сохраняются.

Основание: [аудит 149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) · [план 150](../../fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md) · [покрытие 151](../../fixes-based-on-experiments/codex-151-post131-card-coverage-2026-11-03.md) · [договор 49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md). Ниже сохранена прежняя постановка; текущий блок уточняет только указанную границу.
<!-- CODEX-POST131-CURRENT:END -->


<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница COMPILER/STATE:** Сверить назначенный уровень compiler: входные features, target packing и ordinary consumer не взаимозаменяемы. Отрицательный/положительный witness берётся из собственного корпуса этой карточки; copied distances/accuracy не обещать native пути без parity. Если вызывается Compile, учесть его реальные вложенные Grammar/Ngram Train. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Исходный адрес из паспорта:** `12.SemanticCompiler/Prediction/MethodKnn.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](../31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Не завершать ход на ожидании и не придумывать работу.** ISSUED/WAITING → штатный блокирующий/event wait исходного handle → terminal/exit → чтение назначенных raw → отчёт. Финальное «жду уведомления / доложу потом» до результата запрещено; обещание auto-notification не доказывает возобновление. Не создавать фоновый waiter/monitor/watchdog, циклы polling, progress-события каждые N шагов, summary scripts, manifests или другую работу «пока ждём»; не запускать агентов или следующую карточку. Обычное ожидание не требует заполнения паузы работой. Соблюдать provider no-poll/no-sleep. Проверить настоящий wait/continuation в каталоге до launch; при отсутствии доказать конкретный блокер, сохранить handle и INCOMPLETE, без выдуманного API или relaunch. STOP/NEXT Любы немедленны. Полный порядок и исключения — [контракт31](../31-tool-discovery-and-wait.md).

**Без действительного обучения задача не выполнена.** Для каждой новой экспериментальной программы, включая diagnostic/reader/restore, обязателен назначенный реальный учебный участок: original файл/метод, learner→изменённое состояние→обычный consumer, точные TRAIN/EXAM, before/after и конечный бюджет. Сохранённый обученный S0 является стартом, но не заменяет этот участок. Для retention/restore навык проверяется до вмешательства; для acquisition допустим cold start с приобретением поведения. Ноль обновлений допустим у отдельного контрольного плеча или reader после общего Train, но не у всей новой программы. Unit/BPE действительно обучают память/правила; neural updates=0 у них не означает отсутствие native Train. Updates, loss, hash и случайный верный токен сами по себе не доказывают навык; 0→0 не является учебным результатом, частичные токены не объявляются целым ответом. Исторический безучебный receipt сохранить; его повтор получает отдельный назначенный рецепт по оригиналу и новый output, а не считается завершённым новым опытом. Уже выполненный учебный участок этой же попытки не повторять ради счётчика после сбоя отчёта; продолжить его сохранённые stages. Отдельная unit-проверка готового receipt не является новым экспериментом обучения. Полный охват карточек и адресные рецепты — [37](../37-full-catalogue-learning-coverage.md).

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](../30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
**Состав данных ограничивает вывод.** До запуска автор задаёт точные TRAIN/EXAM/CONTROL, покрытие классов, роли отрицательных примеров, семьи/пересечения и конкретный контроль постоянного ответа. Одноклассовый TRAIN и выигрыш на его единственной метке не доказывают различение маршрутов; updates/confidence не заменяют этот контроль. Разные опыты не устанавливают причину регрессии состава данных. Измерять любой исход, не предписывать ухудшение; набор и budget исполнитель не меняет сам. Полное обязательное правило — [состав данных и допустимый вывод](../32-training-set-and-inference-contract.md).

**Сохранённый учебный результат используется без повторного выбора.** В уже разрешённой фазе исполнения пригодные сохранённые веса/обученное состояние имеют приоритет: восстановить их и продолжить назначенный опыт, не спрашивая «A: использовать результат или B: обучить заново». Поздняя ошибка проверки, сериализации или отчёта сама по себе не основание повторять Train. Сначала прочитать актуальные receipts и установить, не выполнены ли recovery/плечи уже; состоявшуюся работу не переиздавать. Проверять только необходимую для следующего шага совместимость и учебную квалификацию, используя имеющиеся данные; недостающие показания получать лишь по явно ограниченному recovery-рецепту, без optimizer updates и без притворного восстановления отсутствующих moments/RNG. Если пригодность доказана, выбрать reuse автоматически. Если объективно отсутствует необходимое состояние, назвать конкретный технический пробел; не предлагать повторное обучение «для чистоты», не просить пользователя снять guard и не выдавать размер файла/падение loss за доказательство выученного навыка. Текущая документная фаза запусков не разрешает.

**Реализация не отменяет контракт.** «Код не читает RNG/optimizer/проверку» означает отклонение, а не разрешение его игнорировать. До следующего model call исправить реализацию в уже разрешённом scope либо применить только явно назначенный в карточке совместимый режим восстановления; не придумывать исключение, не просить снять guard и не переносить weights-only DPO-исключение на SFT continuation.

<!-- END mandatory-learning-and-recovery-2026-10-21 -->

**Единственное назначение результата METHOD-KNN-TARGET-SCOPE-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [METHOD-KNN-TARGET-SCOPE-01](METHOD-KNN-TARGET-SCOPE-01.md) → [ROPE-TRAIN-TABLE-01](ROPE-TRAIN-TABLE-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** Показать native категории четырёх назначенных запросов A0/A1/B0/B1, correct count и контрольные случаи. Уменьшение nested target contamination должно доходить через Train до ответа. Статическая разница KindBag без learned Predict не является результатом обучения.

20.10.2026. **DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED / CONDITIONAL_CONTRACT**. Написан только Markdown со встроенными данными. Source-copy, Program.cs, fixtures, build, Train/Predict не создавались/не выполнялись. Этот документ не разрешает runtime. Это новый ограниченный подслучай данного source-поиска, не заявление о мировой новизне или причине всего RP01.

Исполнитель и последовательный независимый проверяющий — **модель текущей сессии по умолчанию без override**. Astra ultra остаётся исследователем. При прямом разрешении ограниченного исполнения не требовать нового личного подтверждения ID/SHA. Единственный порядок — [индекс](00-execution-index.md), не имя файла.

**Немедленный переход/stop — часть карточки и промпта.** По «заканчивай и переходи» немедленно перейти в разрешённой фазе; не возвращаться дописывать report, hashes, review, retry или закрывающие проверки прошлого опыта. По «остановить всё» немедленно остановить всю работу. Существующие результаты сохраняются. Оформление не является исключением.

## Контракт и первичный causal path

Домен C#, native adjacent-method kNN. Автор заранее назначает условный контракт: категория Next относится к управляющим операциям **внешнего метода**, исключая невызванные вложенные local function/lambda. В literal наборе они не вызываются и не возвращаются наружу. Если consumer намеренно классифицирует любую вложенную syntax, A соответствует иному контракту и универсальная ошибка не заявляется. Called-local interprocedural semantics здесь не оценивается.

[Extract28–40](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:28) → [ExtractKindBag62–75](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:62) → [Train120–130](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:120) → настоящая память _pairs → [Predict139–146](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:139) → [CanonicalType172–184](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:172). DescendantNodes включает вложенные тела; Try/Catch имеет приоритет ErrorHandling, If+Ret даёт Guard. Метка вычисляется при чтении сохранённого Next.KindBag, не записана как отдельный teacher label.

[PackedMethod12–16](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs:12) не хранит AST/raw source, PackedString ещё ограничен Take(12). Поэтому reader не может восстановить scope из старого bag/string. B должен подготовить scope из того же raw TRAIN AST и записать отдельную Next-проекцию в Train. Один и тот же PackedMethod может быть Next и следующим Current; shared bag нельзя мутировать.

Public writer [facade179–190](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:179), ordinary consumer [314–323](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:314) связывают learner с PredictedNextMethodType. Наш bounded host вызывает native learner **k1**, не facade k5/Compile; это механизм target-learning, не доказанный эффект всего product consumer. k1 одинаков A/B; с четырьмя парами k5 свёл бы все запросы к одному majority vote. Нейронных весов, DPO, LoRA/optimizer нет.

[Исторический E13:58–77](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E13MethodLevelKnn.cs:58) использует ту же CanonicalType для baseline/validation gold; его self-consistency не проверяет внешний outer-body rubric.

## Дедуп и граница новизны

Known prior [N48-01:17–81](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md:17>) — nested null guard **FileActionKnn.Label**. Здесь другой writer/state/reader: target projection Next в MethodKnn. [P84](P84-lambda-family.md:140) объединяет lambda tokens; [P85](P85-composite-obligations.md:129) касается настоящих совместных обязанностей; [While/Do N-K3](../2026-10-19-small-learning-probes/16-additions-typed-state-and-restore.md:116) известен и не переоткрывается. Старый N-KU result не тронут.

В planning possibleDifs и canonical docs queries MethodKnn/ExtractKindBag/KindBag + nested/LocalFunction/scope/лямбд/вложен и обратный порядок дали0matches до новых документов; общий local-function поиск дал12matches включая N48. Это bounded dedup, не отсутствие любого перефразированного аналога. Snapshot HEAD момента чтения e5fbd6050bbdada4c2def5392ac8e2a7f3bb0e99 и DesignToCode-clean e346b88a3cc1fcce48aefa626bfc7b00509eefd6 имеют одинаковый MethodKnn blob. language_translate f6032aff81f1f83de7c09cf9b9efe6b058dd12b9 / origin/DesignToCode8f20ceb8a9bfbadab2ad309226c47dd76e714548 содержат тот же механизм со сдвигом−5строк. Это inherited property, не доказанная недавняя регрессия.

## Единственный фактор: точный writer/state рецепт

A хранит прежние Current/Next.KindBag. B scope-фильтрует только **Next target projection при Train**. Raw lessons, Current/query features, Jaccard kernel, k1, pair adjacency/order и CanonicalType неизменны. Изменяется один training-state factor; текущая product implementation не правится.

Будущие additions только в copied MethodKnn.cs:

1. В MethodExtractor добавить public experimental `ExtractWithTargetScope(string source, short fileId)`, возвращающий native OperationResult нового `MethodScopeMaterial` с `IReadOnlyList<PackedMethod> Methods` и `IReadOnlyDictionary<(short FileId,int IndexInFile),HashSet<string>> TargetScopes`. Новый DTO в том же file/namespace. Тело использует тот же один CSharpSyntaxTree.ParseText и тот же root.DescendantNodes().OfType<MethodDeclarationSyntax>() order. Прежние PackMethod/ExtractKindBag создают полностью исходные PackedMethod. Параллельно на **этом AST** создаётся scope map; повторный parse PackedString запрещён. Existing Extract может делегировать common extraction, но возвращаемые original Methods/KindBags не меняются.
2. private `ExtractTargetScopeKindBag(MethodDeclarationSyntax method)`: Body null→пустой Ordinal HashSet. Обойти method.Body.DescendantNodes с descendIntoChildren predicate `node is not LocalFunctionStatementSyntax && node is not AnonymousFunctionExpressionSyntax`. Также исключить сами boundary nodes из выходных элементов (Roslyn predicate ограничивает детей, но не обязательно сам node). На прочие nodes тот же AbbreviateKind, !="?" и Ordinal HashSet. Original ExtractKindBag/PackMethod/AbbreviateKind/ExpressionBody поведение не менять. Ordinary outer if/try/loops проходят. Никаких literal gold labels внутри producer.
3. Native MethodKnnPredictor constructor расширить optional `IReadOnlyDictionary<(short,int),HashSet<string>>? targetScopes=null` и `bool useTargetScope=false`; поля defensive-copy каждого scope set. A/B получают один и тот же material scope map, k1, Afalse/Btrue. Это новый experiment API, сейчас его нет. До Clear в **обоих** arms проверить unique method keys, полный map для всех methods, non-null sets; ошибка не даёт partial training. Эти shared checks не factor и их цена общая.
4. В native Train сохранить Clear/adjacency predicate/sequence. A пишет прежнее (Current,Next). Только B на строке128 кладёт `(methods[index], methods[index+1] with { KindBag = new HashSet<string>(scopeMap[(next.FileId,next.IndexInFile)], StringComparer.Ordinal) })`. Current original, Next новый record с новым set. Original methods/KindBag не мутировать; это предотвращает изменение Current в следующей паре. Map keys задаются самим extractor, не exam IDs.
5. Predict/JaccardDistance/CanonicalType исходные. Одинаковый observer внутри существующего Predict может сохранить уже вычисленные nearest IDs/distances/votes; повторные Predict/CanonicalType/Jaccard ради report запрещены. Snapshot trained _pairs делает только копию bags после Train. Common scope producer работает в обеих arms, его время отдельно и в total; delta не выдаётся за полное ускорение неинструментированного original A.

## Точные literal данные

Один ordered TRAIN input T1→T2→T3→T4: четыре файла, fileIds0–3, по2methods/1pair. Native C# не компилируется: parser извлекает структуру. Метод Inner — LocalFunction, не внешний MethodDeclaration. Actual counts обязан проверить future host, не переписывая данные при mismatch. Эти JSON сейчас только внутри Markdown.

```json
[
  {
    "id": "T1",
    "fileId": 0,
    "source": "class LessonOne { void BeforeFor(){for(int i=0;i<2;i++){}} int Target(){int Inner(int x){if(x<0)return 0;return x;}return 7;} }"
  },
  {
    "id": "T2",
    "fileId": 1,
    "source": "class LessonTwo { void BeforeWhile(){while(false){}} int Target(){System.Func<int,int> f=x=>{try{return x;}catch{return 0;}};return 8;} }"
  },
  {
    "id": "T3",
    "fileId": 2,
    "source": "class LessonThree { void BeforeIf(){if(true)return;} int Target(int x){if(x<0)return 0;return x;} }"
  },
  {
    "id": "T4",
    "fileId": 3,
    "source": "class LessonFour { void BeforeTry(){try{}catch{}} int Target(){try{return 1;}catch{return 0;}} }"
  }
]
```

Четыре unique query. Q3/Q4 уже negative controls; id/expected/purpose не входят в model source. query.KindBag извлекается один раз и переиспользуется до/после.

```json
[
  {
    "id": "Q1",
    "source": "class QueryOne { void Query(){for(int q=0;q<3;q++){}} }",
    "expected": "Mixed",
    "purpose": "uncalled local guard excluded"
  },
  {
    "id": "Q2",
    "source": "class QueryTwo { void Query(){while(false){}} }",
    "expected": "Mixed",
    "purpose": "uncalled lambda handler excluded"
  },
  {
    "id": "Q3",
    "source": "class QueryThree { void Query(){if(true)return;} }",
    "expected": "Guard",
    "purpose": "negative control: outer guard retained"
  },
  {
    "id": "Q4",
    "source": "class QueryFour { void Query(){try{}catch{}} }",
    "expected": "ErrorHandling",
    "purpose": "negative control: outer handler retained"
  }
]
```

Условный gold Mixed/Mixed/Guard/ErrorHandling задан до запуска. Current signatures знакомы, только identifiers/числа изменены; generalization между независимыми source families не заявляется. По source ожидается A0/B0=Mixed×4; A1=Guard/ErrorHandling/Guard/ErrorHandling; B1=Mixed/Mixed/Guard/ErrorHandling. Это **не native результат**. Before2/4 — default fallback, не приобретённое знание. Настоящие outer controlsQ3/Q4 должны сохраниться; missed/extra outer obligations записываются отдельно.

## Конечная source/build граница

**Проверка состава без новых learner calls.** На уже сохранённых TRAIN/query KindBag показать exact совпадения и принадлежность исходным4семействам: разные идентификаторы не создают новый feature input. Gold EXAM2Mixed:1Guard:1ErrorHandling; постоянный Mixed даёт2/4. Это же число до Train является fallback, не приобретённым знанием. Показать Q1/Q2 target и Q3/Q4 controls отдельно, а также per-class correct/total; доли нельзя смешать в одно «выучено». Набор проверяет scope correction на знакомых структурах, не выбор из всех возможных Next-категорий C#. Дополнительные извлечения/Train/Predict для отчёта не назначаются.

Host: **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01**. Result: **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001**. Project Runner.csproj, Program.cs; три Common files ниже; ничего вне host для source/build. Рецепт использует [KH-EXT83–111](KH-EXT-01.md:83) с другим factor/данными, без его facade wrapper и admission experiment.

Pin **e5fbd6050bbdada4c2def5392ac8e2a7f3bb0e99**, три native file с прочитанными blob IDs:

| Original absolute path | Copy внутри host | Git blob |
|---|---|---|
| C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/MethodKnn.cs | Common/MethodKnn.cs | e0ecebb6d513259a0930e8706737254f926197cd |
| C:/Projects/llm_mvp_core/12.SemanticCompiler/Core/Core.cs | Common/Core12.cs | 7e68b2ce1526402be24d3a286a7edf31b395d920 |
| C:/Projects/llm_mvp_core/29.SemanticCompiler/Core/Core.cs | Common/Core29.cs | 0dc09344f15816c11f23e3166bfe8e24ff9fada9 |

Core12/29 прочитаны полностью: OperationResult/CompilationResult→SemanticCompiler29.Domain замкнуты этими files. Не toy replacement native result; namespaces/copyright сохраняются. Source mismatch не разрешает silent latest: зафиксировать exact mismatch автору. Ни source copying, ни ABI/build сейчас не проверены.

Runner.csproj: SDK Microsoft.NET.Sdk, OutputType Exe, net9.0, Nullable enable, ImplicitUsings enable, EnableDefaultCompileItems=false, AssemblyName=MkScopeRunner; PackageReference Microsoft.CodeAnalysis.CSharp **4.12.0**. Compile ровно Program.cs/Common/MethodKnn.cs/Common/Core12.cs/Common/Core29.cs. Нет ProjectReference, whole-repo globs или package upgrade. ImportDirectoryBuildProps/Targets=false в command; local default bin/Release/net9.0/obj. Одна сборка содержит A/B, runtime flag фиксирован до calls; нет незаметной второй реализации расстояния.

## Lifecycle, точные команды и предел бюджета

Будущая CLI пока **не существует**: `--prepare <exact-result-root>` (create-new study.json/inputs.json,0Extract/Train/Predict); `--arm A|B <exact-arm-root>` (3args); `--summarize <exact-result-root>` (только existing receipts,0modelcalls). Prepare фиксирует ID/arm paths/factor/k1/data/source pins/noRetry/A→B. Existing result/issued marker/чужой host — BLOCKED_PATH_EXISTS; не remove/overwrite/new attempt. Разработчик реализует именно этот контракт, не ищет иной проект.

На каждом arm сначала четыре TRAIN ExtractWithTargetScope дают8methods и полный map; четыре query Extract даютпо1method (shared path может заполнить неиспользуемый scope тоже). Все prepared values сохраняются. Только после этого создать один fresh predictor(k:1,targetScopes:этот map,useTargetScope:arm==B), затем **Q0(4Predict) → один Train whole ordered input → Q1(4Predict)**. После size4. Negative controls внутри4, не отдельная панель. Не делать additional calls ради feature/gold/report/review; observer использует данные текущих calls.

| Native operation | A | B | Total |
|---|---:|---:|---:|
| Extract источников/queries | 8 | 8 | 16 |
| Train whole input | 1 | 1 | 2 |
| Predict before | 4 | 4 | 8 |
| Predict after | 4 | 4 | 8 |
| Все Predict | 8 | 8 | 16 |
| Neural updates/Generate/Compile/warmup/retry/teacher | 0 | 0 | 0 |

Пути фиксированы; команды выполняются только после отдельного прямого поручения исполнения. Они не заявляются уже работающими.

```powershell
dotnet build 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/Runner.csproj' -c Release --nologo -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'Build failed; no learner calls' }
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll' --prepare 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001'
if ($LASTEXITCODE -ne 0) { throw 'Preparation failed; preserve evidence' }
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll' --arm A 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/A'
if ($LASTEXITCODE -ne 0) { throw 'A failed; no B or retry' }
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll' --arm B 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/B'
if ($LASTEXITCODE -ne 0) { throw 'B failed; no retry' }
dotnet 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/bin/Release/net9.0/MkScopeRunner.dll' --summarize 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001'
```

## Метрики и вывод

Для A0/A1/B0/B1 own4raw predictions/correct4, false Guard/ErrorHandlingQ1/Q2, сохранностьQ3/Q4, size/pair snapshots. Same Current/query feature sets/nearestIDs/k/order обязательны. Extract+scope producer/Train/query costs wall/CPU/allocated bytes отдельно и combined, process peak отдельно; nested intervals не суммировать. Scope projection memory overhead/все регрессии рядом. Любой полезный измеренный эффект — успех своей оси/домена, accuracy расти не обязана. Нулевой результат честен; fixture после результата не менять.

Error/cancellation сохраняет original ErrorCode/exception и issued/completed/partial; exit nonzero, дальнейший arm не стартует автоматически. Русский report.md≤350слов плюс таблицы/raw, один readback фактов. Измеренный кандидат≤120слов; product перенос отдельно. Полный facade k5 meaningful fixture и called-local semantic summary — следующие отдельные leads, не текущий бюджет и не автоматическое расширение.

## Самодостаточный промпт

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

«Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Полная карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md. Единственный R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001; next_id — [ROPE-TRAIN-TABLE-01](ROPE-TRAIN-TABLE-01.md) только в порученной программе, одиночный ID — END. Модель сессии по умолчанию. Точные навыки в C:/Projects/llm_mvp_core/docs/skills/: architect-behavior-classifier-SKILL.md, planning-session-startup-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, generated-code-review-SKILL.md, prompt-patch-authoring-SKILL.md, output-readability-gate-SKILL.md, human-communication-GLOSSARY.md. Перед новым аудитом или заданием заново и полностью прочитать стек договора120 и навыки применимой роли. Внутри того же задания ожидание и оформление отчёта сами по себе не требуют повторного полного чтения. По прямому поручению на METHOD-KNN-TARGET-SCOPE-01 реализуй только MKSC01 по трём pinned source files и этому паспорту, default session model без override. Общий AST producer создаёт scope-map, B записывает отдельный Next record/set в настоящем Train, Current/query/Jaccard/k1 неизменны. Один Train на arm,4query includingcontrols вbefore/after:2Train/16Predict всего. Literal данные не менять, gold не передавать learner. Покажи actual learned pairs/own answers/полезные показатели и регрессии; accuracy рост необязателен для успеха другой оси. Не вызывай Compile/нейросеть/N-KU/retry. Missing input/compiler refusal сохраняй без обхода. По “заканчивай и переходи” немедленно переходи без отчёта/hashes/review прошлого; по “остановить всё” прекрати всё. Research карточка сама runtime не разрешает».

## Навыки входят в полный промпт

> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

Точные файлы: C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/code-examination-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md; C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md. Не искать другой workflow; читать только нужные ещё не прочитанные части. Продуктовые CI/promotion gates не расширяют эксперимент. Полный источник, Student, inputs и один factor сохраняются; default модель исполнителя не Student. Русский report по R до350 слов плюс таблицы, candidate до120; полезный gain с регрессиями. STOP/NEXT немедленно. Окончательные номер/NEXT/R — верхний действующий маршрут этой карточки.
