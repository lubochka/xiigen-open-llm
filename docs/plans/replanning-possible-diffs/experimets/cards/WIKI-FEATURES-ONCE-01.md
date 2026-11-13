# WIKI-FEATURES-ONCE-01 — стоимость повторного построения признаков во время обучения

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница ROUTE:** На названном маршрутизаторе/каталоге проверить writer→обученная память→ordinary route/payload и собственные positive/negative строки; label без выбранного payload не закрывает маршрут. Exact recall, простой baseline и предметная тема раздельно. Дополнительный dynamic same-functions фактор допустим только если назначен паспортом и проверяет скорость native Train.

**Исходный адрес из паспорта:** `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/report.ru.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



**Предметная тема:** оркестрация (D07), выбор маршрутов Wiki; learning_topic=orchestration, task_scope=Wiki request routing. CodeTraceTraining и WikiQuestionAnswer — классы одного маршрутизатора. Fixture metadata и русский report header используют эти поля; TRAIN/EXAM, метки, budget и downstream Units не изменяются.


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

**Единственное назначение результата WIKI-FEATURES-ONCE-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [WIKI-FEATURES-ONCE-01](WIKI-FEATURES-ONCE-01.md) → [N-HEAD-01](N-HEAD-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** На назначенной EXAM панели показать native маршруты/TargetUnitId A0/A1/B0/B1. Один features-pass и экономия allocations считаются отдельно от правильных выученных маршрутов. Численное равенство A/B при нулевом correct не подтверждает освоение.

## Действующее локальное назначение — 2026-10-20

Это активная карточка, перенесённая из [входящего исследования](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/03-features-once-experiment.md>). Гипотеза уже существовала в этом источнике; новая находка текущего агента не заявляется. Текущая фаза — **READ_ONLY_RESEARCH и правка Markdown**. Host, код эксперимента и результаты сейчас не создавались; **DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED**, сборка и обучение не проверены.

По прямому указанию пользователя внешние ссылки заменены локальными корнями. Назначенный будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01`; назначенный результат: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001`. Эти пути заменяют прежние короткие внешние корни источника; состав native файлов, фактор, Student и бюджет не меняются. Это документационный рецепт будущей реализации, не готовый исполняемый host.

Исполнитель и последовательный проверяющий: **модель текущей сессии по умолчанию, без provider/model/reasoning override**. Исследовательская роль Astra ultra не становится исполнителем runtime; отдельное поручение на исполнение выполняется назначенной сессией по её умолчанию. Student/tokenizer/perceptron описан ниже отдельно. Действительное разрешение на конкретный опыт сохраняется при смене инструмента: повторно просить то же разрешение не нужно. Этот документ сам по себе не разрешает запуск.

Конфликт путей: если назначенный host занят чужими/неизвестными файлами или назначенный result/attempt уже существует, **BLOCKED_PATH_EXISTS; не удалять, не переносить, не выбирать другой путь и не повторять запуск**. Новый attempt допускается только в отдельном прямом поручении с конкретным номером. Рецепт использует локальные Git-объекты указанных pin без fetch/checkout; недоступный объект, несовпавший blob или отсутствующая зависимость — PREPARATION_FAILED до обучения, без замены ветки/модели/проекта. Текущее разрешение записи карточек не разрешает менять production, fixtures, source-copy или runtime. При будущем исполнении исходники и сами карточки читаются; результаты пишутся только в назначенный result.

Навыки будущего исполнителя, точные локальные файлы: [architect-behavior-classifier-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md>), [planning-session-startup-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md>), [phase-planning-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md>), [plan-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md>), [code-examination-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/code-examination-SKILL.md>), [generated-code-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md>), [prompt-patch-authoring-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md>), [output-readability-gate-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md>), [human-readable-architecture-audit-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md>), [human-communication-GLOSSARY.md](<C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md>). Русский отчёт и конечный шаблон указаны ниже; искать другие skills/формат не требуется.

**STOP/NEXT:** при «следующий эксперимент» немедленно оставить прежний опыт и перейти только к названному следующему, без дописывания старого отчёта, повторного hash/build/CI/commit. При «стоп» прекратить действия; допустимо лишь необходимое прекращение собственного уже работающего процесса, чтобы он не продолжал обучение. Старые опыты и failed attempts сохраняются.


**Статус: SOURCE_CONFIRMED / NEW_CANDIDATE / DESIGNED_NOT_RUN.** Дизайн и буквальные данные подготовлены; новый host ещё не реализован, не скомпилирован и не запускался. Сейчас только документируем. Карточка не разрешает изменить production; текущая запись этой карточки отдельно разрешена пользователем.

## 1. Проверяемая гипотеза и место в настоящем Train

При неизменных строках повторное построение лексических признаков в каждой эпохе может расходовать лишние выделенные байты и время. Проверяется один фактор: вычислить `Featurize` каждой строки один раз на вызов `Train`, использовать результат в тех же 18 эпохах. Полезный результат может быть уменьшением allocations/CPU/time при той же точности. Дополнительная живая память cache во время эпох — возможная цена, которую нельзя скрывать.

Core pin: `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. [TrainingRunner L43–53][runner] вызывает native `Train`, затем native `RouteForEvaluation`. В [Train L58–83][train] признаки прямо входят в `PredictIndex`, затем в запись `_tokenWeights[token][expected/predicted]` и `_biases`. [Featurize L341–378][features] детерминирован и не читает обучаемые параметры. Это измерение процесса настоящего обучения, не внешнего отчётного валидатора.

Native модель — лексический perceptron с 15 классами. Тип входа называется DPO pair, но данный метод учится только по `Prompt/ExpectedRouteKind`. Нейронный DPO, LoRA, число neural layers и VRAM здесь `N/A`. Участие этого learner в конкретном неудачном RP01 не установлено; этот опыт не объявляет такую причинность.

<!-- BEGIN book-math-pass4-2026-10-22:wiki-features-once -->
<a id="book-math-wiki-features-once"></a>
### Почему те же учебные шаги стали дешевле

**Пример.** Восемь учебных карточек можно подготовить один раз и затем изучать 18 раз. Повторное изучение сохраняется; заново выписывать одни и те же слова перед каждым чтением не требуется. WC01 проверяет именно такую экономию подготовки признаков при прежних предъявлениях и обновлениях модели.

**Связь с книгой.** В §7.3, печатная с. 207 / [PDF с. 243](https://github.com/lubochka/xiigen-planning/blob/fabc8c0d9aeeb5fded87c458c963482dc9c929b0/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0239-0244-ch07-information-flow-and-residual-stream.md#pdf-0243), Cirrincione разделяет вход и обучаемую проекцию:

$$
V_h=XW_{V,h}.
$$

Это условная аналогия для лексического perceptron, не утверждение, что WC01 — Transformer: фиксированный $X$ можно использовать повторно, но при изменении $W_{V,h}$ нельзя автоматически переиспользовать прежнее $V_h$. Так же здесь сохраняются признаки, а оценки по текущим весам вычисляются заново.

**Наше доказательство эквивалентности.** Пусть $z_i=\Phi(x_i)$ — признаки строки, $s_t$ — обучаемое состояние, $U$ — один учебный шаг:

$$
s_{t+1}=U(s_t,z_{i_t},y_{i_t}).
$$

Если $\Phi$ детерминирована и не зависит от обучаемых параметров, входы и порядок предъявлений одинаковы, начальное состояние совпадает, признаки неизменяемы, а порядок операций с плавающей точкой сохранён, состояния A/B совпадают по индукции: одинаковые аргументы $U$ дают одинаковое следующее состояние. Это наше доказательство для этих условий, не теорема книги о времени обучения.

В [BaselineModel.cs](../../../../../experiments-v2/RP01-MICRO/WC01/BaselineModel.cs) `Featurize` строит слова, биграммы и нормированные частоты внутри эпох; [CandidateModel.cs](../../../../../experiments-v2/RP01-MICRO/WC01/CandidateModel.cs) делает это перед ними. Словари признаков затем не меняются; `PredictIndex/Score` используют текущие веса при каждом предъявлении. [Точный diff](../../../../../experiments-v2/RP01-MICRO/WC01/one-factor.diff) сохраняет обновление модели. Кэшировать предсказания или зависящие от весов признаки этот аргумент не разрешает.

**Откуда экономия.** Для $N$ строк, $E$ эпох, стоимости признаков $c_\Phi$, учебного шага $c_U$ и суммарных накладных расходов организации и доступа к кэшу $c_{\mathrm{cache}}$ наша модель стоимости даёт

$$
T_A\approx EN(c_\Phi+c_U),\qquad
T_B\approx Nc_\Phi+ENc_U+c_{\mathrm{cache}},
$$

$$
T_A-T_B\approx(E-1)Nc_\Phi-c_{\mathrm{cache}}.
$$

По коду при $N=8$, $E=18$ построений TRAIN-признаков становится $144\to8$: на **136 меньше**. Это вывод из циклов, не отдельный измеренный счётчик; сокращение этих вызовов в 18 раз не означает такого же ускорения всего обучения.

**Сохранённое измерение.** [Raw A](../../../../../experiments-v2/RP01-MICRO/WC01/results/attempt-001/A/report.json) и [raw B](../../../../../experiments-v2/RP01-MICRO/WC01/results/attempt-001/B/report.json), выполненные на source `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`: по 8 TRAIN, 4 EXAM, 144 предъявления и 7 обновлений. До обучения оба дают 0/4 правильных и 4 defer; после — 4/4 правильных, 0 wrong, 0 defer. Итоговый state hash одинаков: `e81f918fd69a627c474fd546b3c36e342ef63eadf1b611d674db6db90ef74c59`.

| Стоимость непустого Train, включая создание кэша | A | B | Наблюдаемое снижение |
|---|---:|---:|---:|
| Выделенные байты текущего потока | 1 321 440 | 150 656 | 88.6% |
| Время по часам, ms | 23.2227 | 10.539 | 54.6% |

Это один короткий свежий процесс на плечо, A затем B, с затратами холодного JIT/helpers; CPU-счётчик округлился до 0. Устойчивый коэффициент скорости не установлен. Кэш удерживает все признаки во время Train; allocated bytes не измеряют пиковую RAM, изолированный пик памяти обучения не измерен. Проверены native source-copy `Train/RouteForEvaluation` на фиксированной панели: `productionEligibility=NOT_EVALUATED_NOT_PUBLISHABLE`, `publishAllowed=false`. Это пояснение существующего измерения, не новый запуск или изменение статуса.
<!-- END book-math-pass4-2026-10-22:wiki-features-once -->

## 2. Зафиксированный A/B фактор

**A:** source-copy текущего native learner; `Featurize(row.Prompt)` остаётся внутри каждой эпохи.

**B:** копия A, отличающаяся только локальным cache признаков в `Train`. После создания `model` и перед циклом эпох сформировать `prepared = rows.Select(row => (Row: row, Features: Featurize(row.Prompt))).ToArray()`. Внутри epoch заменить `foreach (var row in rows)` на `foreach (var item in prepared)`, восстановить `var row = item.Row`, а строку вычисления features заменить на `var features = item.Features`. Сохранить порядок перечисления prepared, словарей признаков и все остальные операции.

`LabelIndex(row.ExpectedRouteKind)` остаётся внутри эпохи. `PredictIndex/Score` никогда не кешируются: они зависят от новых весов. `EpochCount=18`, `LearningRate=0.14`, Labels15, biases, `if (predicted == expected) continue`, размер обновления, confidence/margin/defer reader остаются прежними. Не добавлять averaging, shuffle, cache между разными Train вызовами, margin training либо WIKI-TAXONOMY.

Cache — только локальная переменная Train; не добавлять его в snapshot или поля модели. Существующие presentation/update counters одинаковы в A/B. По source при восьми строках ожидается 144 построения TRAIN features в A и 8 в B; это механизм, не уже измеренный результат и не полезная метрика успеха сам по себе.

Если после опыта полезный показатель улучшился, кандидат в original fix — только перенос `Featurize` в `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:58–83` по этой схеме. Сначала отчёт и решение пользователя; полная production-цепочка здесь не запускается.

## 3. Домен и буквальный учебный материал

Использовать без генерации и редактирования [fixtures/WIKI-FEATURES-ONCE-01.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/fixtures/WIKI-FEATURES-ONCE-01.json>): **8 TRAIN и 4 EXAM**. `origin=simulation`: это авторские правдоподобные runtime wiki prompts, а не добытый production корпус. TRAIN достаточно длинный, чтобы в измерение вошло реальное построение нескольких десятков признаков; увеличение batch после просмотра результата запрещено.

Четыре строки явно просят обучить code trace, четыре — ответить на вопрос по wiki. Ожидаемые labels уже есть в текущем [Labels L25–42][labels]: `CodeTraceTraining` (index5) и `WikiQuestionAnswer` (index13). Targets native [L211, L219][targets]: `code-trace-builder-unit` и `project-local-wiki-orchestrator`. Missing taxonomy classes не используются. Два EXAM на каждый intent — отдельные замороженные формулировки; это small side panel, не оценка широкой репрезентативности.

TRAIN действительно проходит native запись весов; нельзя заменить обучение чтением готового snapshot. EXAM ни прямо, ни через expected labels не подаётся в `Train`. Reader получает только prompt; gold/target хранятся в host. Нельзя вместо router обучения запускать нижележащий CodeTraceBuilder: task здесь — научиться выбирать его по запросу.

Применимость: данный lexical wiki learner и другие точно установленные callers того же Train. Для BPE, AST-kNN, морфологического tokenizer или изменяющихся online-признаков пригодность cache не следует из этого опыта.

## 4. Бюджет и измерения до/после

| Величина | Один arm | Всего A+B |
|---|---:|---:|
| Fresh `Train([])` для нулевого состояния | 1 | 2 |
| Непустой native `Train(8 rows)` | 1 | 2 |
| Raw Train calls с учётом пустого | 2 | 4 |
| Эпох | 18 | 18 на каждый независимый arm |
| Row presentations | 144 | 288 |
| Native EXAM reader до/после | 4+4 | 16 |
| LLM/Teacher calls, neural backward, GPU calls | 0 | 0 |

Оба arm начинают с нулевых весов и biases. До и после одной непустой Train сохранить snapshot и ответы на один и тот же замороженный EXAM. EXAM не меняет state hash. Actual update count и nonzero parameters фиксируются, не подгоняются под ожидаемое значение. Если state не изменился, честно указать это, не засчитывать построенные fixtures как обучение.

Primary useful metric: `nonemptyTrainAllocatedBytesCurrentThread` (меньше лучше). Дополнительно `nonemptyTrainWallMilliseconds`, CPU milliseconds, whole-process peak working set, managed heap before/after, persisted snapshot bytes. Показать все, включая ухудшения. Таймер и allocations окружают только **непустую Train**, включая создание cache B; построение DTO, EXAM, snapshot serialization, build и запись отчёта вне этого окна. Нельзя исключить подготовку cache из измерения B.

Для качества: `correct_selected` требует native `Selected` и совпадение route+target; `wrong_selected`, `deferred` показывать отдельно. Confidence/margin — диагностика, не самостоятельная полезная метрика. Уменьшение defer полезно только вместе с числом правильных и неправильных выбранных решений. Обязательны `A до / A после / B до / B после`, `ΔA`, `ΔB`, `ΔB−ΔA`. Cost сравнивается A vs B внутри Train, не с пустой инициализацией.

Поскольку B должен сохранять вычисляемую модель, сравнить canonical содержимое весов/biases/labels и native EXAM outcomes A после vs B после. При различии не объявлять успех эквивалентной оптимизации: записать `SEMANTIC_DRIFT`, сохранить всё наблюдённое и закончить этот attempt. Не добавлять второй алгоритмический фактор ради совпадения.

Один короткий process per arm не доказывает стабильную пропускную способность. JIT/startup и resolution CPU timer могут доминировать; report сохраняет raw ticks/frequency и ограничения. Whole-process peak включает host. Cache держит все feature dictionaries живыми в Train; heap-after может этого не показать, поэтому отсутствие его роста не доказывает нулевую цену live RAM.

**Критерий пользователя:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и wiki-domain; рост accuracy не обязателен. Честно назвать уровень доказательства: наблюдение одного micro-run, а не подтверждённый performance SLO. Все регрессии рядом. При отсутствии улучшений — `NO_GAIN`; при недостаточном разрешении измерения — `INCONCLUSIVE`, без автоматического расширения опыта.

## 5. Где готовить и как запускать после поручения на опыт

Подготовка строго по [04-source-copy-work-order.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/04-source-copy-work-order.md>), sections 2–5. Исходный scaffold выбран там; B строится из BaselineModel, не из CandidateModel другого опыта. Host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01`; literal ID: `WIKI-FEATURES-ONCE-01`; fixture counts `8/4`. Assembly: `WikiProbe`. Результат: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001`.

Команды ниже выполняются **только после будущей подготовки host и поручения выполнить этот опыт**. В текущей сессии они не выполнялись; готового executable не заявлено.

```powershell
$ErrorActionPreference = 'Stop'
$wikiAttempt = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001'
if (Test-Path -LiteralPath $wikiAttempt) { throw 'ATTEMPT_EXISTS; preserve it; no retry' }
[System.IO.Directory]::CreateDirectory($wikiAttempt) | Out-Null
$wikiIntent = [System.IO.File]::Open("$wikiAttempt/intent.json", [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$wikiWriter = [System.IO.StreamWriter]::new($wikiIntent, [System.Text.UTF8Encoding]::new($false))
try { $wikiWriter.Write('{"experimentId":"WIKI-FEATURES-ONCE-01","attempt":1,"noRetry":true}') } finally { $wikiWriter.Dispose() }
dotnet --list-sdks 1> "$wikiAttempt/sdk.stdout" 2> "$wikiAttempt/sdk.stderr"
if ($LASTEXITCODE -ne 0) { throw 'SDK check failed; no retry' }
dotnet restore C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/WikiProbe.csproj --configfile C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/NuGet.Config 1> "$wikiAttempt/restore.stdout" 2> "$wikiAttempt/restore.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed; training NOT_RUN; no retry' }
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/WikiProbe.csproj -c Release --no-restore -o C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/bin 1> "$wikiAttempt/build.stdout" 2> "$wikiAttempt/build.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; training NOT_RUN; no retry' }
'{"phase":"A","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/A-issued.json" -Encoding utf8
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/bin/WikiProbe.dll --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/A 1> "$wikiAttempt/A.stdout" 2> "$wikiAttempt/A.stderr"
$wikiExitA = $LASTEXITCODE
@{ phase='A'; state='returned'; exitCode=$wikiExitA } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/A-returned.json" -Encoding utf8
if ($wikiExitA -ne 0) { throw 'Arm A failed; no B and no retry' }
'{"phase":"B","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/B-issued.json" -Encoding utf8
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/bin/WikiProbe.dll --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/B 1> "$wikiAttempt/B.stdout" 2> "$wikiAttempt/B.stderr"
$wikiExitB = $LASTEXITCODE
@{ phase='B'; state='returned'; exitCode=$wikiExitB } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/B-returned.json" -Encoding utf8
if ($wikiExitB -ne 0) { throw 'Arm B failed; no retry' }
```

Исполнять команды последовательно, проверять exit status; после failure не переходить к следующей команде. Нужен установленный .NET9 SDK/reference packs. Если его нет — `BLOCKED_ENVIRONMENT`. Собственный подготовленный host ожидаемо существует; неизвестный/чужой host до подготовки или любой существующий attempt/output — конфликт по work order, никакого удаления. Никакого full solution build, production тестов, download другой модели или правки исходных карточек во время runtime.

Выход: в каждом arm `fixture.json`, `source-provenance.json`, `before-snapshot.json`, `after-snapshot.json`, `report.json`; общий **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/report.ru.md`** по шаблону ниже. Сохранённые ошибки/логи не затирать. Успех build не заменяет execution evidence.

## 6. Русский отчёт — фиксированная структура

1. `Статус и область`: фактический run/not-run, wiki lexical router; source pin, fixture hash, default executor model, counts из runtime.
2. `Полезное изменение`: allocation/CPU/time A→B и цена RAM; рядом все положительные, нулевые и отрицательные изменения, без требования accuracy↑.
3. Таблица: `показатель | A до | A после | B до | B после | ΔA | ΔB | ΔB−ΔA`. Для costs отдельная таблица `Train A | Train B | B−A`, поскольку пустая модель не является cost baseline.
4. `Действительно обучалось`: native writer/reader, changed params/update count, post-state equality, отдельные correct/wrong/defer outcomes.
5. `Пределы`: 8/4 simulated rows, один process, JIT, cache live RAM, no production/RP01 attribution.
6. `Кандидат`: точное original Train и только перенос Featurize, если есть полезное измерение. Иначе `NO_GAIN`/`INCONCLUSIVE` с сохранением receipt. Решение о production оставлено пользователю.

## 7. Готовый промпт исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURES-ONCE-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURES-ONCE-01.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Текущий ID — WIKI-FEATURES-ONCE-01. До отдельного поручения исполнить опыт — только документация, NOT_RUN. Когда поручено исполнение: используй модель по умолчанию своей текущей сессии, отчёт по-русски; навыки core docs/skills architect-behavior-classifier, planning-session-startup, plan-review, code-examination, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit из work order. Подготовь только C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01 по 04-source-copy-work-order.md: A — native source-copy ab583032, B — такой же с единственным локальным cache Featurize до 18 эпох. LabelIndex, Score, margin/taxonomy, обновления и reader не меняй. Возьми frozen fixture 8 TRAIN/4 EXAM без редактирования. Выполни ровно A/B по командам этой карточки, результат C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001; четыре raw Train calls, только два непустых. Измеряй настоящую Train вместе с созданием cache; сохрани до/после и все показатели. Любое измеренное улучшение полезной метрики — успех по ней и этому домену, accuracy↑ не требуется; ухудшения показывай рядом. Число feature calls — только диагностика. Если нет среды/runner/build — честно BLOCKED/NOT_RUN, не чинить production. Старые опыты/attempts не удалять, исходные карточки во время опыта не менять, production не продвигать. На «следующий эксперимент» немедленно переходи к указанному следующему без доработки старого отчёта/commit/hash/build; на «стоп/закончить немедленно» прекрати действия, без последнего tool call; исключение — только необходимое прекращение своего уже работающего процесса этого опыта. Нельзя продолжать предыдущую задачу под предлогом завершения. После одного отчёта пользователь решает, какие исправления вносить.

[runner]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs:43>
[train]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:58>
[features]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:341>
[labels]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:25>
[targets]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:211>


## Встроенный точный рецепт подготовки

Рецепт перенесён из [исходной спецификации](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/04-source-copy-work-order.md>); выше действует локальное назначение путей.

# Точное задание будущей подготовки WIKI-FEATURES-ONCE-01

**Текущий статус: DOC_DESIGN_ONLY / NOT_COMPILED / NOT_RUN.** Этот документ задаёт подготовку; исходники новых host здесь не созданы. Текущее поручение — документировать, поэтому команды ниже в этой сессии не выполняются. После отдельного поручения на конкретный опыт исполнитель получает готовые пути, источник, единственный фактор и бюджет; искать другой runner или модель не требуется.

## 1. Исполнитель, язык, навыки и приоритет указаний

Исполнитель использует **ту же модель, которая установлена по умолчанию в его текущей сессии**, без выбора нового провайдера, иной модели или скрытого увеличения thinking budget. Обучаемая модель опыта отдельно фиксирована: native `ProjectWikiRequestRoutingTrainableModel`, 15 классов, CPU, 18 эпох, learning rate 0.14. Neural layers/LoRA/VRAM — `N/A`. Сетевые вызовы Teacher/LLM и генерация новых TRAIN/EXAM в бюджет не входят и не требуются.

Пояснения и `report.ru.md` пишутся по-русски. Имена свойств JSON, route enums, path и дословные английские fixture prompts сохраняются как есть. Использовать только относящиеся к текущему этапу правила из core `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-readable-architecture-audit-SKILL.md`. Исходники навыков зафиксированы [в core docs/skills][skills]. Требование полной production-цепочки не переносить на эксперимент. Современное явное указание пользователя имеет приоритет над архивной карточкой, модельным preset и историческим протоколом.

**Обязательный запрет продолжения предыдущей задачи:** команда пользователя «следующий эксперимент» немедленно завершает работу над прежним опытом: без дополнительного исправления отчёта, commit, hash, build, чистки или «последней проверки» старой задачи. Команда «стоп/закончить немедленно» прекращает действия; нельзя запускать ещё один tool call ради красивого завершения. Единственное допустимое действие после STOP — необходимое прекращение собственного уже работающего процесса этого опыта; не останавливать чужие процессы. Сообщить кратко только уже известный статус. Старые файлы и упавшие attempts не удалять и не переписывать. Не создавать новый запуск автоматически. Решение о production-правке принимает пользователь после отчёта.

Любое фактически измеренное улучшение полезного показателя — успех по этому показателю и домену; роста accuracy не требуется. Все изменения остальных показателей, включая ухудшения, показываются рядом. Уменьшение diagnostic counter само по себе не является полезным эффектом. Если выполнено только чтение исходника, статус остаётся `NOT_RUN`, а численные результаты — «не измерено».

## 2. Источник для копии уже выбран

Planning commit: `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`.

Базовый каталог уже существующего scaffold:

`llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/`

Взять по pinned commit ровно четыре файла: [BaselineModel.cs][baseline], [NativeRoutingContracts.cs][contracts], [Program.cs][program], [source-provenance.json][provenance]. **CandidateModel.cs из WIKI-MARGIN-01 не брать:** это другой экспериментальный фактор. Нельзя «для удобства» брать B от taxonomy.

Provenance донора указывает старый core commit `62bcc8…`, но исходные blob identities равны текущему pin `ab583032…`:

| Исходный core-файл | Git blob |
|---|---|
| `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs` | `3672bd2c9dd813eb2a31835c2e6b06071de37869` |
| `42.ProjectWikiDomain/ProjectWikiDomainModels.cs` | `9044c096665f29442de3dd01f7d7aa6bc1291d39` |

Все 15 файлов string constants из donor `constant_inlining` также имеют те же blob SHA в current core tree. Это подтверждает происхождение и равенство исходного кода, не является результатом компиляции. Сохраняются license headers. Допустимые общие edits донора уже перечислены в provenance: изолированный namespace, отсутствие ненужных AgentInfrastructure usings и соседнего ILogicalUnit wrapper, public-доступ к тому же `RouteForEvaluation`, одинаковые counters presentations/updates, inline точных compile-time Unit/Flow IDs. Не менять модельную арифметику A.

## 3. Буквальные короткие каталоги

| Опыт | Host | Fixture назначения | Новые результаты |
|---|---|---|---|
| WIKI-FEATURES-ONCE-01 | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/fixture.json` из [WIKI-FEATURES-ONCE-01.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/fixtures/WIKI-FEATURES-ONCE-01.json>) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WC01/results/attempt-001/A`, `…/B`, общий `report.ru.md` в `attempt-001` |

Результаты не писать в исходные карточки и внутрь production project. До подготовки проверить отсутствие чужого/неустановленного host; такой конфликт — `BLOCKED_PATH_EXISTS`, ничего не удалять. Собственный уже подготовленный host с подтверждённым provenance ожидаемо существует перед build/run и сам по себе не является конфликтом. **Любой существующий parent attempt или arm output блокирует повтор независимо от полноты файлов.** До SDK/restore/build карточки резервируют новый parent attempt с create-new intent и сохраняют stdout/stderr. Arm каталоги shell заранее не создаёт: их создаёт host после guards и до первого Train. Для следующей отдельно порученной попытки её точный номер должен быть указан в поручении; самостоятельно следующий номер не выбирать. Прежний attempt не возобновлять. Наличие `build.stdout`/`status.json` после неудачи — сохранённый след, а не повод удалить папку. Не расширять Windows path limits системными правками.

## 4. Минимальный состав каждого host

В выбранном коротком каталоге должны быть `BaselineModel.cs`, `CandidateModel.cs`, `NativeRoutingContracts.cs`, `Program.cs`, `fixture.json`, `source-provenance.json`, `WikiProbe.csproj`, `NuGet.Config`. Это **спецификация будущей записи**, а не наличие файлов сейчас.

`CandidateModel.cs` создаётся копией `BaselineModel.cs`; namespace меняется с `PossibleDiffs.Baseline` на `PossibleDiffs.Candidate`, затем применяется только фактор выбранной карточки. Общее имя класса сохраняется, Program использует существующие alias AModel/BModel. A и B находятся в одной сборке, запускаются разными свежими процессами. Состояние из A никогда не передаётся B.

Минимальный проект повторяет standalone .NET9 подход [существующего csproj][csproj], но все source paths становятся локальными; так компиляция не тянет длинное дерево или production solution:

```xml
<Project>
  <PropertyGroup>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
  </PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>WikiProbe</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="NativeRoutingContracts.cs" />
    <Compile Include="BaselineModel.cs" />
    <Compile Include="CandidateModel.cs" />
    <Compile Include="Program.cs" />
    <None Include="fixture.json" CopyToOutputDirectory="PreserveNewest" />
    <None Include="source-provenance.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
```

`NuGet.Config` содержит только `<?xml version="1.0" encoding="utf-8"?><configuration><packageSources><clear /></packageSources></configuration>`. Нужны установленный .NET9 SDK и его reference packs. Их отсутствие даёт `BLOCKED_ENVIRONMENT`; не выбирать незаданный runtime и не скачивать новый model/framework автоматически.

## 5. Точные изменения общего host Program

Скопированный Program уже имеет разделённые fresh A/B, `Train([])` для начального состояния, одну настоящую `Train(rows)`, before/after native reader, отсутствие gold в reader, state hashes, raw per-row outcomes и resource counters. В новой копии изменить только:

1. Literal `ExperimentId` на ID выбранной карточки и проверку размеров fixture: `8/4` для FEATURES-ONCE или `4/6` для FEATURE-KEYS. Порядок строк и gold не менять.
2. Поле `coreCommit` на `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; `scope` — согласно карточке; убрать чужие упоминания margin experiment.
3. В `source-provenance.json` сохранить original source/blob/constant mapping; записать current pin, `ancestor_scaffold` pinned URL, новый `experiment`, точный `candidate_factor`, статус фактического приготовления. Старый provenance в доноре не редактируется.
4. Не менять body `Evaluate`, который различает `Selected`, правильный route/target, wrong-selected и deferred. Не объявлять невыбранный `NotProjectWiki` правильным только из-за совпадения enum. `RouteForEvaluation` пропускает только publication gate; trained signal, margin и selection-confidence gates остаются native.
5. Четыре raw `Train` вызова на весь A/B опыт: по одному пустому и одному непустому на arm. Пустой вызов создаёт нулевое состояние и исключён из измеренного nonempty Train cost. Не добавлять warmup с обучением или многократный benchmark без нового поручения.
6. **Исправить общий журнал host до любого Train.** Donor Program создаёт output только после Train/EXAM; для этих двух новых опытов этого недостаточно. После fixture guards, но до первого `Train([])`, создать новый arm directory и эксклюзивный `intent.json` через `FileMode.CreateNew`. **Перенести, а не продублировать** поздние записи frozen fixture/provenance из конца donor Main в эту раннюю фазу; удалить их прежний повтор из финального блока. Иначе завершающий File.Copy выбросит исключение после успешного обучения, а File.WriteAllBytes перепишет уже сохранённый fixture. Перед каждым из двух Train записать отдельный неизменяемый `phase-N-issued.json`, после возврата — `phase-N-completed.json` с raw counters. Перед непустой Train issued receipt пишется до запуска таймера, completed — после остановки таймера, чтобы file I/O не входил в cost. При исключении записать новый `failed.json` с exception/последней известной фазой/issued и completed counts, сохранить все имеющиеся артефакты и выйти nonzero, без retry. Неизвестное число предъявлений или эффект оборванного вызова обозначить `UNKNOWN`, не нулём. При hard kill issued без completed остаётся достаточным запретом на повтор. Ничего не перезаписывать, не изменять числители задним числом. Это одинаковое plumbing A/B, не дополнительный алгоритмический фактор.
7. `before-snapshot.json`, `after-snapshot.json`, `report.json` сохраняются в этом зарезервированном arm по достижении соответствующих фаз; существующие файлы не перезаписывать; если запись snapshot перенесена в раннюю соответствующую фазу, её старое позднее повторение также удалить. Метаданные snapshot missing-gate могут содержать `TrainingRowCount=0`; реальное количество хранить отдельно в `observedTrainingRows`, не «исправлять» gate ради красивого отчёта. Parent shell пишет отдельные `A-issued/A-returned`, `B-issued/B-returned`; arm-issued означает запуск процесса, а не доказательство дошедшего до Train. Последний подтверждается внутренними phase receipts.

Замер `GetAllocatedBytesForCurrentThread` применим к этому синхронному learner. Whole-process peak включает startup/JIT; `GetTotalMemory(false)` не равен точной пиковой живой памяти обучения. Эти ограничения переносятся в отчёт буквально по смыслу. Снимки и per-row native outcomes, а не учебная loss на surrogate, подтверждают фактическое изменение модели.

## 6. Последовательность будущей авторизованной работы

После подготовки выбранного host: один scoped restore, один scoped build, затем ровно A и B, если подготовка и build успешны. Команды для каждого опыта заданы в его карточке. Ошибка до обучения отмечается `BUILD_BLOCKED`/`NOT_RUN`, ошибка после старта — `FAILED_PARTIAL`; нет перехода к full solution build, ремонту production tests, повтору старого опыта или новой гипотезе без поручения. Сам факт build — не успех учебного эксперимента.

После A/B создать один `report.ru.md` с таблицей `A до / A после / B до / B после`, дельтами внутри каждого arm и разницей этих дельт. Для costs сравнивать nonempty Train A/B, не нулевую инициализацию с обучением. Указать source pin, fixtures hash, domain, фактические counts, все raw files, useful gains и regressions, `NO_GAIN`/`INCONCLUSIVE` где необходимо. Не удалять старые карточки. В случае выигрыша — только кандидат в исправления с точным original method/line; production promotion оставляется владельцу.

[skills]: <C:/Projects/llm_mvp_core/docs/skills>
[baseline]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/BaselineModel.cs>
[contracts]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/NativeRoutingContracts.cs>
[program]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/Program.cs>
[provenance]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/source-provenance.json>
[csproj]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/experiments-v2/RP01-MICRO/WIKI-MARGIN-01/WikiMarginLearning.csproj>


## Замороженные данные этого опыта

Существующий локальный файл: [fixture](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/fixtures/WIKI-FEATURES-ONCE-01.json>). SHA256 байтов: `e684017bcf44cb96c2951e2a49926d08cc9a869f38e5f229bedcda8cc23f8210`. Исходный файл read-only; будущая копия должна совпасть с этими байтами. Никакая генерация данных не требуется.

```json
{
  "experimentId": "WIKI-FEATURES-ONCE-01",
  "origin": "simulation",
  "scope": "cost of native lexical router Train; frozen four-query quality side panel",
  "train": [
    {
      "id": "C-T1",
      "prompt": "Train code trace on source calls in the payment service. Learn the edges from ParseInvoice to ValidateInvoice and from ValidateInvoice to SaveInvoice using the displayed CSharp source. Keep source locations with each call edge.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "native_router_training"
    },
    {
      "id": "C-T2",
      "prompt": "Improve code trace training on source calls in the inventory service. Learn the edges from LoadStock to CheckStock and from CheckStock to ReserveStock. Preserve the argument value flow across the methods and the location of each call.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "native_router_training"
    },
    {
      "id": "C-T3",
      "prompt": "Train the code trace builder from repository examples of async request handlers. Learn calls from ReadRequest to CheckPermission and from CheckPermission to WriteReply. Include returned values and cancellation arguments in the training target.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "native_router_training"
    },
    {
      "id": "C-T4",
      "prompt": "Add source call examples for code trace training in the customer module. Learn which methods call LoadCustomer and UpdateCustomer. Keep evidence references for the extracted call graph and record the values passed between methods.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "native_router_training"
    },
    {
      "id": "C-T5",
      "prompt": "Answer the project wiki question about the payment service. Which component owns invoice validation and which documented interface calls it? Use the existing project evidence and provide an answer grounded in those documents.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "native_router_training"
    },
    {
      "id": "C-T6",
      "prompt": "Answer a wiki question about inventory reservations in this repository. Explain which component owns stock updates and cite the already indexed project documents. The request is for an answer about the project architecture.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "native_router_training"
    },
    {
      "id": "C-T7",
      "prompt": "Use the project wiki to answer the question about async request handlers. Explain the documented responsibilities of authentication and response writing. Use the existing evidence records and show where the interface behavior is described.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "native_router_training"
    },
    {
      "id": "C-T8",
      "prompt": "Answer the project wiki question about the customer module. Explain which documented service owns customer updates and which application component consumes it. Ground the answer in existing repository documentation and the relevant interface descriptions.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "native_router_training"
    }
  ],
  "exam": [
    {
      "id": "C-Q1",
      "prompt": "Train the code trace builder on source calls from ParseInvoice to SaveInvoice.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "code_trace"
    },
    {
      "id": "C-Q2",
      "prompt": "Improve code trace training using the request handler source call edges.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "code_trace"
    },
    {
      "id": "C-Q3",
      "prompt": "Answer the project wiki question about invoice validation responsibilities.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "wiki_answer"
    },
    {
      "id": "C-Q4",
      "prompt": "Use project wiki evidence to answer the question about customer updates.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "wiki_answer"
    }
  ]
}
```


## Единственный выбранный wiki-опыт

Общий исходный work order описывал два варианта. Эта карточка назначает только `WIKI-FEATURES-ONCE-01`; создавать второй host, запускать соседний factor или брать его CandidateModel нельзя. В recipe выбора counts применяются только значения текущей карточки. Встроенные source/fixture здесь достаточны для подготовки без поиска другой модели, проекта или набора данных.


**Немедленный переход/stop, также часть промпта исполнителю.** Команда пользователя «заканчивай и переходи» немедленно завершает текущий опыт и переводит к названной следующей задаче в пределах разрешённой фазы. После перехода не возвращайся дописывать report, hashes, review, retry или другие остатки предыдущего опыта. Команда «остановить всё» немедленно прекращает всю работу; не завершай ради оформления ни отчёт, ни проверки, ни повтор. Сохрани уже существующие результаты; новая команда не разрешает повтор ранее завершённого или прерванного запуска.
