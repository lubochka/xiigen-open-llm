# NG01 — числовой pattern не должен скрывать учебную потребность в null guard

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница COMPILER:** Сверить назначенный уровень compiler: входные features, target packing и ordinary consumer не взаимозаменяемы. Отрицательный/положительный witness берётся из собственного корпуса этой карточки; copied distances/accuracy не обещать native пути без parity. Если вызывается Compile, учесть его реальные вложенные Grammar/Ngram Train.

**Исходный адрес из паспорта:** `12.SemanticCompiler/Prediction/FileActionKnn.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/report.md>).
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

**Единственное назначение результата NG01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [NG01](NG01-null-guard-label.md) → [BPE-REALIZED-MERGE-COUNT-01](BPE-REALIZED-MERGE-COUNT-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


Внутренняя версия источника planning commit 6bc4b6fba8f14870f5dc4966cf5a344754962d66. Локальные ссылки ведут на рабочие файлы; source pin ниже остаётся основанием source-copy. В конфликтах с внешней версией применяются явно исправленные здесь пути, следующий [BPE-REALIZED-MERGE-COUNT-01](BPE-REALIZED-MERGE-COUNT-01.md), единая команда с ранним claim и регистрация полезного кандидата в required-changes.md. Рецепт не собирался и не запускался.

**Статус: DESIGNED_NOT_RUN.** Сейчас создан только документ. Host, source copies, build и Train/Predict не выполнялись. Это узкий проверяемый подслучай известного N48-01; самостоятельная новая root-cause family не заявляется. Промпт — [NG01-executor.md](../prompts/NG01-executor.md).

**Фаза и очередь.** Будущий runtime выполняется по поручению выполнить NG01 либо выполнять/продолжать программу по реестру пакета, когда очередь дошла до NG01. Уже данное поручение на программу не требует повторного допуска каждого ID. Текущая документная сессия runtime не разрешает. Все consumed attempts из [индекса](00-execution-index.md) сохраняются без повтора; активный вход — [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md). NG01 не вставляется перед ним. Следующий опыт определяется реестром/прямым назначением, не поиском похожего ID. Production и старые результаты не меняются. Новая карточка и очередь применены в разрешённой пользователем внутренней папке.

**Маршрут в [едином внутреннем индексе](00-execution-index.md): [NG01](NG01-null-guard-label.md) → [BPE-REALIZED-MERGE-COUNT-01](BPE-REALIZED-MERGE-COUNT-01.md).** Состав и числовые позиции берутся из текущего индекса. Прямо назначенный ID имеет приоритет. При одиночном поручении NG01 следующий опыт самостоятельно не начинать.

## Обязательные правила карточки и любого её промпта

> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

**Модель:** модель текущей сессии по умолчанию, без override; фактическое имя в отчёте. Ученик — native FileActionKnnPredictor, k=3. Внешняя LLM, LoRA, neural optimizer и GPU не участвуют.

**Успех:** любое фактически измеренное полезное улучшение считается успехом по соответствующему показателю и домену; рост accuracy не обязателен. Все изменения остальных показателей, включая ухудшения, показать рядом. Нулевой эффект допустим. Одиночное фиксированное A→B время/CPU — наблюдение с неустановленной причинностью.

**Послабления:** в назначенной экспериментальной копии допустимы throw, try/catch, void, простые DTO и минимальный host. Product OperationResult, полный CI, комментарии к каждой строке и требования выпуска не навязываются. Ошибку сохранить с ненулевым exit code. Native learner не подменять proxy.

**Только отчёт и кандидат:** после обычного завершения — русский report.md, raw JSON и candidate.md с source fix, полезным результатом, регрессиями и границей доказательства. Перенос в production, product tests, commit/push не выполняются. При фактическом полезном результате действуют общие правила: одна запись до 120 слов в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md со ссылкой на raw и candidate.md; это регистрация кандидата, не перенос кода. Решение о переносе принимает пользователь. Самопроверка не называется независимым review.

**STOP/NEXT:** по «заканчивай и переходи к следующему» немедленно перейти к уже назначенному следующему заданию в разрешённой фазе. Запрещено продолжать прежний отчёт, реестр, паспорт, hashes, review, retry, исследование Git или закрывающие проверки. Нельзя объявлять завершение и снова возвращаться к опыту. По «остановить всё» немедленно прекратить всю работу; допустима лишь необходимая остановка собственного живого процесса. Незавершённые файлы и raw сохранить. Эти команды имеют приоритет над оформлением. Историческая цитата об остановке не является текущим приказом остановить документный аудит.

## 1. Учебный вопрос, домен и источник

**Домен:** программирование C#, распознавание необходимой защиты nullable receiver. Контракт панели: nullable параметр может быть null; операция должна обработать это до dereference. Это заданный контракт, не утверждение, что любой API обязан иметь guard.

[ActionLabelGenerator.Label32–38](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:32>) считает любой IsPatternExpression внутри if признаком null condition:

```csharp
int F(string? x, int n) { if (n is > 0) return x.Length; return 0; }
```

Условие относится к non-nullable int n и не защищает x. По коду Label подавляет ADD_NULL_CHECK. Это source inference, не измеренный native результат. Удалять все relational patterns нельзя: `int F(int? x){if(x is >0)return x.Value;return 0;}` законно исключает null перед x.Value; такой контроль обязателен.

**Реальный учебный путь:** [Facade.TrainFileActionKnn194–203](<C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:194>) → FeatureVector(source) + Label(source) → [native Train122–126](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:122>) → таблица _train → [Predict129–141](<C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:129>).

[Reader facade327–333](<C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:327>) требует включённый механизм, C# и минимум 4 записи. [UseFileActionKnn default false](<C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerOptions.cs:20>). Host фиксирует C# и включение одинаково, портирует только guard минимум 4. Полный Compile и downstream RP01 не вызываются.

**Новизна ограничена:** [N48-01:17–95](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md:17>) уже описывает false suppression из-за чужого/nested guard и отсутствие receiver/scope/dominance. Здесь отдельный ablation — не-nullable int relational pattern. N-KU проверял DoStatement, METHOD — Next scope; они не повторяются.

Дополнительный prior — [core65:45](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md:45>) и [N53 dedup:109](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md:109>): общий тезис «любой IsPatternExpression не обязательно защищает нужный объект» уже известен. Точный int-parameter factor с nullable-int/type/null controls данным prior не задаётся. Этот рецепт уточняет известную проблему, не присваивает ей новую историю происхождения.

## 2. Единственный фактор

A — весь исходный FileActionKnn.cs, только namespace Ng01.SourceA. B — тот же файл с namespace Ng01.SourceB; в hasNullCondition исключён только direct relational pattern на параметре, явно записанном как int. Точная замена и helper заданы в Prepare.ps1 ниже.

Nullable int, type patterns, null literal и остальные patterns сохраняются. Это узкий фильтр заведомо нерелевантного null evidence, не общий null analyzer. Он не решает receiver binding, dominance, called local functions, compound/parenthesized patterns, aliases System.Int32 и другие типы. Не расширять его в ходе опыта.

FeatureVector/TargetKinds, 30 координат, k=3, CosineDistance, сортировка, label priorities, данные и порядок одинаковы. Label и FeatureVector по-прежнему отдельно парсят source: KH-COST оптимизация здесь была бы вторым фактором.

## 3. Точное место и состав будущего host

**Состав и контроль данных.** Intended gold12TRAIN:3ADD_NULL_CHECK+9ADD_METHOD; EXAM4:1ADD_NULL_CHECK+3ADD_METHOD. Фактические labels A/B сохранять отдельно: их исправление является фактором, intended gold не подменяется ошибочной A-разметкой. Постоянный ADD_METHOD даёт3/4 на EXAM и0/1 на целевом null-guard случае. Поэтому показывать Q1 отдельно и3контроля отдельно; одна суммарная accuracy скрывает провал нужного класса. Все4структурных семейства представлены и в TRAIN, и в EXAM; переименование переменных и замена чисел не делают SyntaxKind-гистограммы независимыми. Это проверка исправленной метки и чтения внутри этих семейств, не family-disjoint обобщение. Счёт строк12 не равен12независимым семействам; ADD_LOOP/REFACTOR_EXTRACT не объявлять освоенными. Данные и число model calls ниже сохраняются.

Короткий корень **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01**, project **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/Runner.csproj**, results **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001**. Это фиксированный внутренний host; не переносить build в docs и не выбирать другой корень. Типовой output DLL находится здесь же в bin/Release/net9.0/Ng01Runner.dll. Прежний внешний C:/xg не используется. Длина будущих созданных файлов пока сборкой не проверялась. Если корень занят другой работой или attempt существует — сохранить его, не удалять/не перезаписывать и не выбирать новый attempt самовольно.

Read-only source checkout: **C:/Projects/llm_mvp_core**; pin **0b76ab950d82534f5904005dda096545505ae307**; путь **12.SemanticCompiler/Prediction/FileActionKnn.cs**; Git blob **ef7a88a3c4c2540569b9ce6f204d8e4750875501**. Git нужен только для чтения этого объекта, не истории/публикации.

Создать только четыре файла ниже через файловый инструмент. Prepare.ps1 затем создаёт SourceA.cs/SourceB.cs. Никакого ProjectReference, Core stubs, reflection, общего experiment dispatcher или whole-repo globs. net9.0, Roslyn4.12.0 и такой source-copy состав взяты из существующего [N-KU паспорта 198–245](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md:198>). Это назначение нового host, не утверждение, что NG01 уже собран.

**Материализация:** только в будущей явно порученной фазе NG01 создать этот корень файловым инструментом и сохранить четыре literal файла ниже. Другие папки, общий dispatcher и новый attempt не искать. Перед записью проверить: при execution.claim, готовых SourceA/SourceB, source-origin.json, bin/obj или result attempt — ничего не перезаписывать; сохранить partial и записать EXISTS_OR_UNKNOWN. Сначала прочитать существующие claim/стадию/exit/PID+start; живой принадлежащий этой попытке процесс ждать тем же handle, не выдавая повторный entry. UNKNOWN сам не назначает NEXT. Переход возможен по прямому указанию Любы или после установленного статуса в уже порученной программе по [правилу NEXT](00-execution-index.md#next-contract); незавершённая учебная цель остаётся OPEN. Если присутствуют только четыре побайтно одинаковых незапущенных файла из этой карточки, допускается единственный запуск Run.ps1. Это правило не разрешает материализацию сейчас.

### Runner.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>Ng01Runner</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="SourceA.cs" />
    <Compile Include="SourceB.cs" />
  </ItemGroup>
</Project>
```

### Prepare.ps1

```powershell
$ErrorActionPreference = 'Stop'
$Root = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01'
$Repo = 'C:/Projects/llm_mvp_core'
$Commit = '0b76ab950d82534f5904005dda096545505ae307'
$NativePath = '12.SemanticCompiler/Prediction/FileActionKnn.cs'
if (!(Test-Path -LiteralPath $Root -PathType Container)) {
    throw 'NG01 root is missing; materialize only the four files from the card.'
}
foreach ($Name in @('SourceA.cs', 'SourceB.cs', 'source-origin.json')) {
    if (Test-Path -LiteralPath (Join-Path $Root $Name)) {
        throw "Existing NG01 artifact must be preserved: $Name"
    }
}
$Lines = & git -C $Repo show ($Commit + ':' + $NativePath)
if ($LASTEXITCODE -ne 0) {
    throw 'Pinned Git object unavailable; do not substitute HEAD or another branch.'
}
$Source = ($Lines -join [string][char]10) + [string][char]10
function Replace-ExactlyOnce([string]$Text, [string]$Old, [string]$New) {
    if ([regex]::Matches($Text, [regex]::Escape($Old)).Count -ne 1) {
        throw 'Pinned source anchor does not occur exactly once.'
    }
    return $Text.Replace($Old, $New)
}
$A = Replace-ExactlyOnce $Source 'namespace SemanticCompiler.Prediction;' 'namespace Ng01.SourceA;'
$B = Replace-ExactlyOnce $Source 'namespace SemanticCompiler.Prediction;' 'namespace Ng01.SourceB;'
$OldCondition = '.Any(node => node.Kind() == SyntaxKind.NullLiteralExpression || node.Kind() == SyntaxKind.IsPatternExpression));'
$NewCondition = @'
.Any(node => node.Kind() == SyntaxKind.NullLiteralExpression ||
                        (node is IsPatternExpressionSyntax pattern &&
                         !IsNonNullableIntRelationalPattern(method, pattern))));
'@
$B = Replace-ExactlyOnce $B $OldCondition ($NewCondition.Trim())
$Anchor = '    // Build the normalized SyntaxKind frequency vector used by FileActionKnnPredictor.'
$Helper = @'
    private static bool IsNonNullableIntRelationalPattern(
        MethodDeclarationSyntax method,
        IsPatternExpressionSyntax pattern)
    {
        if (pattern.Pattern is not RelationalPatternSyntax ||
            pattern.Expression is not IdentifierNameSyntax identifier)
            return false;
        return method.ParameterList.Parameters.Any(parameter =>
            parameter.Identifier.ValueText == identifier.Identifier.ValueText &&
            parameter.Type is PredefinedTypeSyntax predefined &&
            predefined.Keyword.IsKind(SyntaxKind.IntKeyword));
    }

'@
$B = Replace-ExactlyOnce $B $Anchor ($Helper + [Environment]::NewLine + $Anchor)
$Utf8 = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText((Join-Path $Root 'SourceA.cs'), $A, $Utf8)
[System.IO.File]::WriteAllText((Join-Path $Root 'SourceB.cs'), $B, $Utf8)
$Origin = @{
    repository = 'lubochka/llm_mvp_core'
    commit = $Commit
    path = $NativePath
    blob = 'ef7a88a3c4c2540569b9ce6f204d8e4750875501'
    note = 'git show pinned object; LF normalization; namespaces isolated'
} | ConvertTo-Json
[System.IO.File]::WriteAllText((Join-Path $Root 'source-origin.json'), $Origin, $Utf8)
```

### Program.cs — literal TRAIN/EXAM и bounded native lifecycle

TRAIN — 12 source: по три разных идентификатора/числа в четырёх структурных семействах. EXAM — четыре новых текста тех же семейств. Это влияние teacher на выученные ответы; histogram повторяются, **обобщение на независимые семейства не заявляется**. Gold не передаётся Label, FeatureVector, Train или Predict.

```csharp
using System.Diagnostics;
using System.Text.Json;
using ALabel = Ng01.SourceA.ActionLabelGenerator;
using BLabel = Ng01.SourceB.ActionLabelGenerator;

internal sealed record Sample(string Id, string Source, string Expected, string Group);
internal sealed record Answer(string Id, string Expected, string? Actual, bool Correct);

internal static class Program
{
    private static readonly string[] Names =
        ["ADD_NULL_CHECK", "ADD_LOOP", "REFACTOR_EXTRACT", "ADD_METHOD"];
    private static readonly Sample[] Lessons =
    [
        new("T01", "class L1{int F(string? x,int n){if(n is >0)return x.Length;return 0;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("T02", "class L2{int G(string? a,int count){if(count is >1)return a.Length;return 1;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("T03", "class L3{int H(string? text,int size){if(size is >2)return text.Length;return 2;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("T04", "class L4{int F(string? x){if(x==null)return 0;return x.Length;}}", "ADD_METHOD", "null-literal"),
        new("T05", "class L5{int G(string? a){if(a==null)return 1;return a.Length;}}", "ADD_METHOD", "null-literal"),
        new("T06", "class L6{int H(string? text){if(text==null)return 2;return text.Length;}}", "ADD_METHOD", "null-literal"),
        new("T07", "class L7{int F(object? x){if(x is string s)return s.Length;return 0;}}", "ADD_METHOD", "type-pattern"),
        new("T08", "class L8{int G(object? a){if(a is string b)return b.Length;return 1;}}", "ADD_METHOD", "type-pattern"),
        new("T09", "class L9{int H(object? value){if(value is string text)return text.Length;return 2;}}", "ADD_METHOD", "type-pattern"),
        new("T10", "class L10{int F(int? x){if(x is >0)return x.Value;return 0;}}", "ADD_METHOD", "nullable-int"),
        new("T11", "class L11{int G(int? a){if(a is >1)return a.Value;return 1;}}", "ADD_METHOD", "nullable-int"),
        new("T12", "class L12{int H(int? value){if(value is >2)return value.Value;return 2;}}", "ADD_METHOD", "nullable-int")
    ];
    private static readonly Sample[] Queries =
    [
        new("Q1", "class Q1{int Read(string? item,int limit){if(limit is >3)return item.Length;return 3;}}", "ADD_NULL_CHECK", "unrelated-int"),
        new("Q2", "class Q2{int Read(string? item){if(item==null)return 3;return item.Length;}}", "ADD_METHOD", "null-literal"),
        new("Q3", "class Q3{int Read(object? item){if(item is string content)return content.Length;return 3;}}", "ADD_METHOD", "type-pattern"),
        new("Q4", "class Q4{int Read(int? item){if(item is >3)return item.Value;return 3;}}", "ADD_METHOD", "nullable-int")
    ];

    private static int Main(string[] args)
    {
        if (args.Length != 2 || (args[0] != "A" && args[0] != "B"))
        {
            Console.Error.WriteLine("Usage: Ng01Runner A|B C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001");
            return 2;
        }
        var arm = args[0];
        var output = Path.GetFullPath(args[1]);
        if (!Directory.Exists(output))
        {
            Console.Error.WriteLine("The assigned result directory must already exist.");
            return 2;
        }
        var rawPath = Path.Combine(output, "arm-" + arm + ".json");
        var failurePath = Path.Combine(output, "failure-" + arm + ".json");
        if (File.Exists(rawPath) || File.Exists(failurePath))
        {
            Console.Error.WriteLine("Existing arm artifacts must not be overwritten.");
            return 2;
        }

        var stage = "initialize";
        var featureIssued = 0; var featureCompleted = 0;
        var labelIssued = 0; var labelCompleted = 0;
        var trainIssued = 0; var trainCompleted = 0;
        var predictIssued = 0; var predictCompleted = 0;
        var logicalQueries = 0; var guardBlocked = 0;
        try
        {
            using var trainingClaim = new FileStream(Path.Combine(output, "training-" + arm + ".claim"), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var a = arm == "A" ? new Ng01.SourceA.FileActionKnnPredictor(3) : null;
            var b = arm == "B" ? new Ng01.SourceB.FileActionKnnPredictor(3) : null;
            Func<string, float[]> feature = arm == "A" ? ALabel.FeatureVector : BLabel.FeatureVector;
            Func<string, int> label = arm == "A" ? ALabel.Label : BLabel.Label;
            Func<int> count = arm == "A" ? () => a!.TrainingSize : () => b!.TrainingSize;
            Action<IReadOnlyList<(float[] Features, int Label)>> train =
                arm == "A" ? a!.Train : b!.Train;
            Func<float[], int> predict = arm == "A" ? a!.Predict : b!.Predict;

            float[] Features(string source)
            {
                featureIssued++;
                var value = feature(source);
                featureCompleted++;
                return value;
            }
            string? Read(float[] vector)
            {
                logicalQueries++;
                if (count() < 4) { guardBlocked++; return null; }
                predictIssued++;
                var value = predict(vector);
                predictCompleted++;
                return Names[value];
            }

            stage = "exam-features";
            var queryVectors = Queries.Select(query => Features(query.Source)).ToArray();
            stage = "before";
            var rowsBefore = count();
            var before = new List<Answer>();
            for (var i = 0; i < Queries.Length; i++)
            {
                var actual = Read(queryVectors[i]);
                before.Add(new(Queries[i].Id, Queries[i].Expected, actual,
                    actual == Queries[i].Expected));
            }

            stage = "training";
            using var process = Process.GetCurrentProcess();
            var stopwatch = new Stopwatch();
            process.Refresh();
            var cpuBefore = process.TotalProcessorTime.TotalMilliseconds;
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var collectionsBefore = GC.CollectionCount(0);
            stopwatch.Start();
            var rows = new (float[] Features, int Label)[Lessons.Length];
            for (var i = 0; i < Lessons.Length; i++)
            {
                var vector = Features(Lessons[i].Source);
                labelIssued++;
                var target = label(Lessons[i].Source);
                labelCompleted++;
                rows[i] = (vector, target);
            }
            trainIssued++;
            train(rows);
            trainCompleted++;
            stopwatch.Stop();
            var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            var gen0Collections = GC.CollectionCount(0) - collectionsBefore;
            process.Refresh();
            var cpuMilliseconds = process.TotalProcessorTime.TotalMilliseconds - cpuBefore;
            var rowsAfter = count();

            stage = "after";
            var after = new List<Answer>();
            for (var i = 0; i < Queries.Length; i++)
            {
                var actual = Read(queryVectors[i]);
                after.Add(new(Queries[i].Id, Queries[i].Expected, actual,
                    actual == Queries[i].Expected));
            }

            stage = "record";
            var result = new
            {
                experiment = "NG01", arm,
                sourceCommit = "0b76ab950d82534f5904005dda096545505ae307",
                domain = "C# programming; nullable receiver guard",
                learner = "native source-copy FileActionKnnPredictor",
                model = new { k = 3, expectedFeatureDimensions = 30,
                    neuralLayers = 0, trainableNeuralLayers = 0, optimizerSteps = 0 },
                rowsBefore, rowsAfter,
                training = Lessons.Select((sample, i) => new
                {
                    sample.Id, sample.Source, sample.Group, sample.Expected,
                    actualLabel = Names[rows[i].Label],
                    labelCorrect = Names[rows[i].Label] == sample.Expected,
                    features = rows[i].Features
                }).ToArray(),
                queryData = Queries, queryVectors, before, after,
                correctBefore = before.Count(value => value.Correct),
                correctAfter = after.Count(value => value.Correct),
                cost = new
                {
                    measuredRegion = "12 native FeatureVector + 12 native Label + 1 native Train",
                    wallMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                    cpuMilliseconds, allocatedBytes, gen0Collections,
                    storedFeatureAndLabelPayloadBytes =
                        rows.Sum(row => (long)row.Features.Length * sizeof(float) + sizeof(int)),
                    retainedHeapBytes = (long?)null,
                    processPeakBytes = (long?)null,
                    vramBytes = (long?)null
                },
                calls = new
                {
                    featureIssued, featureCompleted, labelIssued, labelCompleted,
                    trainIssued, trainCompleted, predictIssued, predictCompleted,
                    logicalQueries, guardBlocked, generateCalls = 0,
                    compileFacadeCalls = 0, warmupCalls = 0, retryCalls = 0
                },
                scope = "family recall, not unseen-family generalization; full facade not run"
            };
            WriteNew(rawPath, result);
            return 0;
        }
        catch (Exception error)
        {
            WriteNew(failurePath, new
            {
                experiment = "NG01", arm, stage, error = error.ToString(),
                featureIssued, featureCompleted, labelIssued, labelCompleted,
                trainIssued, trainCompleted, predictIssued, predictCompleted,
                logicalQueries, guardBlocked
            });
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void WriteNew(string path, object value)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, value, new JsonSerializerOptions { WriteIndented = true });
    }
}
```

### Run.ps1 — одна сборка, один процесс A, один процесс B

```powershell
$ErrorActionPreference = 'Stop'
$Root = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01'
$Result = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001'
if (Test-Path -LiteralPath $Result) {
    throw 'Attempt already exists; preserve all partial results and do not retry.'
}
$Claim = [System.IO.File]::Open((Join-Path $Root 'execution.claim'), [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$Claim.Dispose()
# Claim is retained on every outcome; never remove it to retry.
[System.IO.Directory]::CreateDirectory($Result) | Out-Null
$Utf8 = [System.Text.UTF8Encoding]::new($false)
function Log-Stage([string]$Stage, [string]$State, [int]$Code) {
    $Event = @{ stage = $Stage; state = $State; exitCode = $Code; utc = [DateTime]::UtcNow.ToString('O') } | ConvertTo-Json -Compress
    [System.IO.File]::AppendAllText((Join-Path $Result 'events.jsonl'), $Event + [Environment]::NewLine, $Utf8)
}
function Finish([string]$Stage, [int]$Code) {
    $Value = @{ experiment = 'NG01'; stage = $Stage; exitCode = $Code } | ConvertTo-Json
    [System.IO.File]::WriteAllText((Join-Path $Result 'status.json'), $Value, $Utf8)
}
Log-Stage 'prepare' 'issued' -1
& pwsh -NoProfile -File (Join-Path $Root 'Prepare.ps1') 1> (Join-Path $Result 'prepare.stdout') 2> (Join-Path $Result 'prepare.stderr')
$Code = $LASTEXITCODE
Log-Stage 'prepare' 'completed' $Code
if ($Code -ne 0) { Finish 'prepare-failed' $Code; exit $Code }
Log-Stage 'build' 'issued' -1
& dotnet build (Join-Path $Root 'Runner.csproj') -c Release -v minimal -m:1 -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false 1> (Join-Path $Result 'build.stdout') 2> (Join-Path $Result 'build.stderr')
$Code = $LASTEXITCODE
Log-Stage 'build' 'completed' $Code
if ($Code -ne 0) { Finish 'build-failed' $Code; exit $Code }
$Dll = Join-Path $Root 'bin/Release/net9.0/Ng01Runner.dll'
Log-Stage 'A' 'issued' -1
& dotnet $Dll A $Result 1> (Join-Path $Result 'A.stdout') 2> (Join-Path $Result 'A.stderr')
$Code = $LASTEXITCODE
Log-Stage 'A' 'completed' $Code
if ($Code -ne 0) { Finish 'A-failed' $Code; exit $Code }
Log-Stage 'B' 'issued' -1
& dotnet $Dll B $Result 1> (Join-Path $Result 'B.stdout') 2> (Join-Path $Result 'B.stderr')
$Code = $LASTEXITCODE
Log-Stage 'B' 'completed' $Code
if ($Code -ne 0) { Finish 'B-failed' $Code; exit $Code }
Finish 'A-and-B-completed' 0
```

Если shell завершился до status, stdout/stderr и последняя issued/completed запись остаются evidence; не повторять процесс для «полного» журнала. Сбой build/restore не разрешает чинить production или искать другой проект.

### Единственная команда будущего исполнения

```powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/Run.ps1
```

Единственная внешняя команда Run.ps1 последовательно выполняет Prepare → build → A → B. До Prepare/build атомарно создаётся execution.claim, до native lifecycle каждого плеча — training-A/B.claim; они сохраняются при любой ошибке. Prepare отдельно не запускать. Неизвестный исход проверять чтением того же процесса/сессии и сохранённых событий; новый запуск не назначен. Никаких dotnet test, полного 35 build, warmup, повторов, B→A, «equivalence run» продукта или нового экзамена reviewer.

## 4. Бюджет и метрики

| Операция | На плечо | Всего A+B |
|---|---:|---:|
| Учебных source | 12 | 24 |
| Native Label | 12 | 24 |
| FeatureVector на TRAIN | 12 | 24 |
| FeatureVector на EXAM, переиспользуются до/после | 4 | 8 |
| Native Train | 1 | 2 |
| Logical queries до/после | 4+4 | 16 |
| Закрыто guard до Train | 4 | 8 |
| Native Predict после Train | 4 | 8 |
| Optimizer / Generate / Compile / warmup / retry | 0 | 0 |

C# snippets парсятся, но не компилируются и не исполняются как программы. Один build host учитывается отдельно от runtime cost. Effort подготовки/build не прибавляется к Train скрыто. Лимит **120 секунд на Prepare, 120 секунд на build и 30 секунд на плечо** наблюдает исполнитель; при превышении останавливает только собственный процесс и сохраняет partial. Незавершённый этап не является accuracy 0. Второй параллельный процесс не запускать.

Основные оси: правильность фактически использованных TRAIN labels; ответы Q1–Q4 до/после; false negative ADD_NULL_CHECK на Q1; false positive на Q2–Q4; сохранность трёх контрольных guards. ADD_METHOD на controls означает оставшуюся категорию действующей policy и отсутствие ложного ADD_NULL_CHECK, не доказанную полезность добавления метода.

Allocated bytes — managed выделения в указанном training region. Feature/label payload — нижняя оценка полезного содержимого, не retained heap или peak RSS. Полный peak, retained heap, VRAM и end-to-end RP01 quality не измеряются. Один локальный learner и guard adapter; полный Unit/Flow не пройден. Нейронных слоёв и optimizer updates нет.

Ожидание по source — изменятся три labels unrelated-int, controls останутся прежними. Это не результат. При одинаковых ответах A/B записать отсутствие query gain, даже если teacher labels исправлены. Полезный measured label-quality gain относится к реально использованной supervision, не автоматически к качеству output.

## 5. Отчёт и кандидат без поиска формата

**Русский Markdown:** до 350 слов пояснений плюс таблицы фактов и ссылки на raw. Файлы **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/report.md** и **candidate.md**. Raw arm-A.json/arm-B.json, failure, logs, events и status не редактируются. Для отчёта не пересчитывать Label/FeatureVector/Predict, hashes и не повторять опыт.

Навыки из **C:/Projects/llm_mvp_core/docs/skills**:

| Навык | Что применяется |
|---|---|
| architect-behavior-classifier-SKILL.md | Teacher → learned state → reader |
| planning-session-startup-SKILL.md | Назначенная фаза/пути/бюджет |
| plan-review-SKILL.md | Один фактор и controls |
| generated-code-review-SKILL.md | Один просмотр source-copy diff до исполнения |
| prompt-patch-authoring-SKILL.md | Не менять другой bottleneck/данные |
| output-readability-gate-SKILL.md | Понятный русский отчёт |
| human-communication-GLOSSARY.md | Домен C#, механизм kNN |

Если нужные разделы уже прочитаны и не менялись, повторный полный обход навыков не нужен.

```markdown
# NG01: влияние числового pattern на обучение null-guard совета
execution_status: <NOT_STARTED / PARTIAL / COMPLETED / FAILED / OUTCOME_UNKNOWN>.
learning_validity: <VALID_FOR_NAMED_UNIT / INVALID_NO_TRAINING / INVALID_ZERO_TO_ZERO / INVALID_UNQUALIFIED_BEFORE / INVALID_INCOMPLETE_COMPARISON / NOT_EVALUATED / UNKNOWN>; единица и raw before/after: <точно>.
hypothesis_status: <OPEN / SUPPORTED_FOR_NAMED_METRIC / NOT_SUPPORTED_IN_VALID_COMPARISON>; invalid всегда оставляет OPEN.
Источники: <паспорт, raw, команды>; завершённые updates отдельно от ошибки сохранения, неизвестные counts=null, невыполненное плечо — не измерено.
Домен: программирование C#. Исполнитель: <фактическая default модель>.
Проверка: <отдельный reviewer или самопроверка; без новых запусков>.
Источник: commit 0b76ab9..., FileActionKnn.cs::ActionLabelGenerator.Label.
Raw: arm-A.json, arm-B.json, status.json; отсутствующее — явно.

## Что сравнивали
A — исходный label. B — исключён direct relational pattern параметра int.
Один Train на 12 source; те же 30 features/k3 и четыре family-recall запроса.

## Фактические ответы
| Query | A до | A после | B до | B после | Gold |
|---|---|---|---|---|---|
| Q1, null risk | | | | | ADD_NULL_CHECK |
| Q2, null literal | | | | | ADD_METHOD по текущей policy |
| Q3, type pattern | | | | | ADD_METHOD по текущей policy |
| Q4, nullable int | | | | | ADD_METHOD по текущей policy |

## Полезный эффект и ухудшения
<Correct TRAIN labels, actual output, allocated bytes, wall/CPU наблюдения.
Любое полезное измеренное улучшение — успех своей метрики/домена;
рост accuracy не обязателен. Все ухудшения рядом.
Неизмеренное — «не измерено», не ноль.>

## Граница и кандидат
<Не полный null analyzer, не unseen-family generalization, не доказательство RP01.
Точный helper/condition; production не менялся.
Calls issued/completed и лишние операции отдельно; no automatic transfer.>
```

candidate.md — до 120 слов: source method, один diff из раздела 2/Prepare, ссылка на report/raw, фактический useful gain, регрессии и статус «кандидат; решение о переносе за пользователем». Без результата это непроверенная гипотеза, успех не присваивается. Один раз прочитать отчёт и исправить только фактическую ошибку/ссылку/кодировку; не продолжать оформление по кругу.

## 6. Проверка документа и пределы

Source-copy API и mutation просмотрены статически. Конкретная версия NG01 не собиралась; parse/runtime, equality controls и качество не проверены. Текущий документный аудит запрещает такой запуск. Будущий исполнитель не меняет рецептуру при неожиданном результате: сохраняет его и завершает назначенный бюджет.

Этот паспорт сохраняет точный source, один фактор, настоящее обучение памяти, ответы до/после, short paths, immutable results и немедленный STOP/NEXT. Полная production цепочка не требуется для проверки гипотезы.

