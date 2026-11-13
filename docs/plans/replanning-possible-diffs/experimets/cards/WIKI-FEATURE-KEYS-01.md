# WIKI-FEATURE-KEYS-01 — раздельные unigram/bigram признаки wiki learner

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница ROUTE:** На названном маршрутизаторе/каталоге проверить writer→обученная память→ordinary route/payload и собственные positive/negative строки; label без выбранного payload не закрывает маршрут. Exact recall, простой baseline и предметная тема раздельно. Дополнительный dynamic same-functions фактор допустим только если назначен паспортом и проверяет скорость native Train.

**Исходный адрес из паспорта:** `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/report.ru.md>).
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

**Единственное назначение результата WIKI-FEATURE-KEYS-01:** R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001; report.md и candidate.md в R (WIKI-FEATURES использует report.ru.md, AST — candidate-fix.md). Все старые output-only CLI/bridge/guard/DTO/checkpoint-output ссылки на собственный прежний results-root заменены тем же R; read-only исходный B0/tokenizer/source не переносить. Host-owned execution.claim/study/train/source/dependency/build provenance остаются в назначенном host, до R. R резервирует только один штатный execution/prepare owner. При старом claim/result/partial сначала сохранить его и записать EXISTS_OR_UNKNOWN; новый путь не обход ранее issued попытки. Модель по умолчанию, STOP/NEXT выше оформления.


**Маршрут по ID:** [WIKI-FEATURE-KEYS-01](WIKI-FEATURE-KEYS-01.md) → [WIKI-FEATURES-ONCE-01](WIKI-FEATURES-ONCE-01.md). Числовые позиции и текущие статусы — в [едином индексе](00-execution-index.md). Переход следует [правилу перехода](00-execution-index.md#next-contract); одиночный ID заканчивается END. Consumed означает чтение сохранённого статуса без повторного entry, а OPEN не становится completed от перехода. Исторические source labels не определяют очередь.


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Обязательно для этой карточки и каждого её промпта.** Модель исполнителя — модель сессии по умолчанию, без override. В назначенной изолированной копии допустимы throw/try-catch/void, простые DTO и минимальный host; переделка в продуктовый OperationResult, комментарии к каждой строке, product tests/CI/Git не требуются. Runtime scope и один назначенный фактор сохраняются. При обычном завершении: один русский report.md в указанном ниже result-root; при полезном измеренном выигрыше — одна запись до 120 слов в [required-changes.md](../../required-changes.md) с точным source fix, ухудшениями и ссылкой. Это обязательно без отдельного поручения; немедленный переход/stop выше оформления. Ручные checksums и новые запуски ради отчёта запрещены.

**Достоверность осей.** При одном процессе на плечо и фиксированном порядке время/CPU/process peak сохраняются как измеренные наблюдения с неустановленной причинностью; полезное измеренное изменение сохраняется как успех своей оси, причинный speedup не установлен. Для эквивалентной оптимизации сравнить уже предусмотренные конечные learned/optimizer state, loss и logits/readouts; различие нельзя скрыть ресурсным выигрышем. При изменении качества равенство конечных ответов не требуется. Числа 0→0 не доказывают сохранность навыка. Эта вставка не добавляет training/evaluation calls к указанному ниже бюджету.

**Учебный сигнал и предел вывода.** На назначенных EXAM промптах показать native маршрут/TargetUnitId и correct counts до/после Train; коллизии feature keys — механизм. Положительный сигнал — выученный правильный маршрут, а не только разные ключи. Product orchestration readiness этим локальным learner не доказывается.

## Действующее локальное назначение — 2026-10-20

Это активная карточка, перенесённая из [входящего исследования](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/02-feature-keys-experiment.md>). Гипотеза уже существовала в этом источнике; новая находка текущего агента не заявляется. Текущая фаза — **READ_ONLY_RESEARCH и правка Markdown**. Host, код эксперимента и результаты сейчас не создавались; **DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED**, сборка и обучение не проверены.

По прямому указанию пользователя внешние ссылки заменены локальными корнями. Назначенный будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01`; назначенный результат: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001`. Эти пути заменяют прежние короткие внешние корни источника; состав native файлов, фактор, Student и бюджет не меняются. Это документационный рецепт будущей реализации, не готовый исполняемый host.

Исполнитель и последовательный проверяющий: **модель текущей сессии по умолчанию, без provider/model/reasoning override**. Исследовательская роль Astra ultra не становится исполнителем runtime; отдельное поручение на исполнение выполняется назначенной сессией по её умолчанию. Student/tokenizer/perceptron описан ниже отдельно. Действительное разрешение на конкретный опыт сохраняется при смене инструмента: повторно просить то же разрешение не нужно. Этот документ сам по себе не разрешает запуск.

Конфликт путей: если назначенный host занят чужими/неизвестными файлами или назначенный result/attempt уже существует, **BLOCKED_PATH_EXISTS; не удалять, не переносить, не выбирать другой путь и не повторять запуск**. Новый attempt допускается только в отдельном прямом поручении с конкретным номером. Рецепт использует локальные Git-объекты указанных pin без fetch/checkout; недоступный объект, несовпавший blob или отсутствующая зависимость — PREPARATION_FAILED до обучения, без замены ветки/модели/проекта. Текущее разрешение записи карточек не разрешает менять production, fixtures, source-copy или runtime. При будущем исполнении исходники и сами карточки читаются; результаты пишутся только в назначенный result.

Навыки будущего исполнителя, точные локальные файлы: [architect-behavior-classifier-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md>), [planning-session-startup-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md>), [phase-planning-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/phase-planning-SKILL.md>), [plan-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md>), [code-examination-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/code-examination-SKILL.md>), [generated-code-review-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md>), [prompt-patch-authoring-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md>), [output-readability-gate-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md>), [human-readable-architecture-audit-SKILL.md](<C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md>), [human-communication-GLOSSARY.md](<C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md>). Русский отчёт и конечный шаблон указаны ниже; искать другие skills/формат не требуется.

**STOP/NEXT:** при «следующий эксперимент» немедленно оставить прежний опыт и перейти только к названному следующему, без дописывания старого отчёта, повторного hash/build/CI/commit. При «стоп» прекратить действия; допустимо лишь необходимое прекращение собственного уже работающего процесса, чтобы он не продолжал обучение. Старые опыты и failed attempts сохраняются.


**SOURCE_CONFIRMED_COLLISION / CONDITIONAL_LEARNING_HYPOTHESIS / DESIGNED_NOT_RUN.** Подтверждено исходное смешивание разных признаков; потеря полезного качества не измерена. Буквальный fixture и задание подготовки есть, новый host не реализован и не собран. Сейчас только документируем. Production не меняется; текущая запись этой карточки отдельно разрешена пользователем.

## 1. Учебная гипотеза и её границы

Core pin `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. [RegexLikeWords L359–378][features] оставляет `_` внутри токена; [Featurize L341–356][features] записывает unigram под самим токеном и bigram под `left + _ + right`. Поэтому identifier `load_customer` и соседние слова `load customer` обновляют одну координату; bigrams `[a_b,c]` и `[a,b_c]` тоже имеют общий ключ `a_b_c`.

Полная source-derived коллизия: `trace trace_trace` и `trace_trace trace` имеют одно отображение `{trace:1/√3, trace_trace:1/√3, trace_trace_trace:1/√3}`. Это иллюстрация из кода, не выполненное обучение. **Этим искусственным строкам не назначать разные route labels:** два различных настоящих wiki-намерения для них не установлены. Не запускать на них отдельный toy classifier; это подменило бы задачу придуманной семантикой.

Основная гипотеза: раздельный codec может снизить перенос обновлений между различными wiki-намерениями, если identifier и естественная фраза несут полезный различный сигнал. Остальные слова могут разрешить задачу уже в A, поэтому честный `NO_GAIN` допустим. Отсутствие collision после изменения само по себе не доказывает полезного качества.

Реальная цепочка: [TrainingRunner L43–53][runner] → [Train L58–83][train] → `Featurize(Prompt)` → `PredictIndex/Score` → запись `_tokenWeights/_biases` → [RouteForEvaluation L95–99, L155–220][reader] → та же Featurize → route/target/status. В experiment действительно обновляются веса, а EXAM проверяет их выход. Это не внешний validator, не участвующий в обучении.

Модель — native lexical perceptron, Labels15,18epochs,LR0.14,CPU. DTO называется DPO pair, но этот `Train` использует только Prompt/ExpectedRouteKind, без chosen/rejected likelihood. Neural DPO/LoRA/layers/VRAM/Teacher — N/A; участие этого learner в конкретном failed RP01 не установлено.

## 2. Один фактор A/B

**A:** исходный native source-copy. **B:** та же копия A, меняется только кодирование feature keys в `Featurize`, одновременно на стороне Train и reader:

```csharp
// Спецификация ключей для будущей лабораторной копии, не новый runner.
static string TokenLength(string token)
    => token.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
static string UniKey(string token)
    => "u:" + TokenLength(token) + ":" + token;
static string BiKey(string left, string right)
    => "b:" + TokenLength(left) + ":" + left + ":" + TokenLength(right) + ":" + right;
```

В unigram loop создать `key=UniKey(token)` и использовать его в обоих обращениях к dictionary; в bigram loop заменить только создание key на `BiKey(tokens[index], tokens[index+1])`. Пример: `u:13:load_customer` и `b:4:load:8:customer`. Частоты, dictionary comparer, порядок циклов, lowercasing, допуск `_`/`-`, L2-normalizer и вся арифметика остаются прежними. Tokenization не меняется.

Не совмещать с FEATURES-ONCE cache, новым margin update, averaging, label taxonomy, shuffle, другим reader или threshold. B создаётся только из A, не из чужого CandidateModel. Обе модели свежие; старый snapshot не загружается новым codec. Если появится полезный gain, original fix-кандидат — `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:341–356`. Versioning/migration накопленных весов — отдельное условие до production, не второй фактор этого опыта. Изменение codec не объявляет совместимость со старым snapshot version.

Текущие [Labels L25–42][labels] содержат CodeTraceTraining (index5) и WikiQuestionAnswer (index13), оба не первый класс. Missing WIKI-TAXONOMY classes не используются. Условие `predicted == expected` из WIKI-MARGIN сохраняется. KN-ROUTER-LEARNED-KEY-CANONICALIZATION относится к другому learner и разным C#/.NET normalize на writer/reader; здесь оба конца уже используют одну функцию, проблема внутри feature namespaces. [Дедуп/protected gap](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/01-findings.md>) выполнен отдельно.

## 3. Буквальный доменный fixture, labels не подгоняются

Использовать [fixtures/WIKI-FEATURE-KEYS-01.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/fixtures/WIKI-FEATURE-KEYS-01.json>) без генерации и изменения: **4 TRAIN / 6 EXAM**, `origin=simulation`. Это авторские осмысленные runtime requests, не production logs.

| TRAIN | Точный prompt | Expected route / target |
|---|---|---|
| K-T1 | `Train the code trace builder on calls inside load_customer.` | CodeTraceTraining / code-trace-builder-unit |
| K-T2 | `Answer the project wiki question about how services load customer records.` | WikiQuestionAnswer / project-local-wiki-orchestrator |
| K-T3 | `Train the code trace builder on calls inside save_order.` | CodeTraceTraining / code-trace-builder-unit |
| K-T4 | `Answer the project wiki question about how services save order records.` | WikiQuestionAnswer / project-local-wiki-orchestrator |

Gold следует явному intent — обучить code trace либо ответить по wiki; identifier является аргументом, а не произвольно придуманной label rule. Native targets подтверждены [L211/L219][targets]. Опыт учит выбирать route, не запускает downstream CodeTraceBuilder training. Содержательные возражения к gold разрешаются до исполнения новой версией карточки, не после A/B.

EXAM K-Q1…K-Q4 — новые буквальные формулировки этих двух намерений с `load_customer`/`load customer` и `save_order`/`save order`; K-Q5/K-Q6 — plain controls без identifiers. Все шесть полных prompts находятся в JSON. Не переводить их на русский: это изменит фактор. Русским должен быть отчёт. Reader получает только prompt, без ID/group/gold. EXAM не входит в Train. Это small panel на знакомых identifiers, не unseen-repository generalization.

Применимость: lexical wiki/code intents с пересечением underscore vocabulary и соседних естественных слов. Для corpus без таких пересечений возможен нейтральный эффект. E25 tuple POS bigrams и AST-kNN сюда не относятся; результат на них не переносить.

## 4. Бюджет и измерения

| Действие | На arm | Всего A+B |
|---|---:|---:|
| Fresh `Train([])` для нулевого состояния | 1 | 2 |
| Nonempty native `Train(4 rows)` | 1 | 2 |
| Raw Train calls с учётом пустых | 2 | 4 |
| 18 эпох ×4 строки | 72 presentations | 144 |
| EXAM before+after | 6+6 | 24 reader calls |
| Teacher/LLM/neural backward/GPU | 0 | 0 |

Пустая Train исключена из nonempty Train cost. Перед и после непустой Train сохранить snapshot и EXAM decisions. Native reader не должен менять state. Actual updates/nonzero parameters нужны для подтверждения записи весов; не являются useful gain сами по себе.

Полезные показатели: `correct_selected/6` (Selected + правильные route **и** target), `wrong_selected/6`, `deferred/6`; отдельно группы 4 identifier-vs-words и 2 plain controls. Не выдавать defer за правильный отрицательный route по одному совпадению enum. Также Train wall/CPU/allocated bytes, heap-before/after, whole-process peak и persisted snapshot bytes. Длинные ключи могут ухудшить стоимость — показать рядом с выигрышем качества.

Обязательная таблица `A до / A после / B до / B после / ΔA / ΔB / ΔB−ΔA`. Gain B относительно нулевого состояния может быть обычным эффектом обучения, доступным A; причинный эффект codec оценивается **между** arm. Cost A/B измеряется только вокруг непустой Train, включая создание ключей. EXAM, DTO creation, serialization, build и file I/O вне cost-window. Разные keys закономерно меняют snapshot hash A/B: post-state equality здесь не требуется; отсутствие mutation во время EXAM требуется.

Confidence/margin, unique feature keys и disappearance of collision — диагностика. **Любое фактически измеренное улучшение полезной метрики — успех по этой метрике и домену; accuracy↑ не требуется. Все регрессии рядом.** Нет полезного улучшения — `NO_GAIN`; недостаточно разрешения измерения — `INCONCLUSIVE`. Не менять gold/EXAM после результата и не запускать дополнительные batches ради positive verdict. Один короткий process per arm содержит JIT/noise; process peak не равен пику Train. Область вывода остаётся малой.

## 5. Подготовка и точные будущие команды

По [04-source-copy-work-order.md §§2–5](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/04-source-copy-work-order.md>) подготовить **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01`**, assembly `WikiProbe`, literal ID `WIKI-FEATURE-KEYS-01`, fixture counts4/6. B — из pinned BaselineModel, не margin/taxonomy B. Результат **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001`**. Готовый executable здесь не заявлен: команды — после будущего поручения подготовить и исполнить этот опыт.

```powershell
$ErrorActionPreference = 'Stop'
$wikiAttempt = 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001'
if (Test-Path -LiteralPath $wikiAttempt) { throw 'ATTEMPT_EXISTS; preserve it; no retry' }
[System.IO.Directory]::CreateDirectory($wikiAttempt) | Out-Null
$wikiIntent = [System.IO.File]::Open("$wikiAttempt/intent.json", [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
$wikiWriter = [System.IO.StreamWriter]::new($wikiIntent, [System.Text.UTF8Encoding]::new($false))
try { $wikiWriter.Write('{"experimentId":"WIKI-FEATURE-KEYS-01","attempt":1,"noRetry":true}') } finally { $wikiWriter.Dispose() }
dotnet --list-sdks 1> "$wikiAttempt/sdk.stdout" 2> "$wikiAttempt/sdk.stderr"
if ($LASTEXITCODE -ne 0) { throw 'SDK check failed; no retry' }
dotnet restore C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/WikiProbe.csproj --configfile C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/NuGet.Config 1> "$wikiAttempt/restore.stdout" 2> "$wikiAttempt/restore.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed; training NOT_RUN; no retry' }
dotnet build C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/WikiProbe.csproj -c Release --no-restore -o C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/bin 1> "$wikiAttempt/build.stdout" 2> "$wikiAttempt/build.stderr"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; training NOT_RUN; no retry' }
'{"phase":"A","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/A-issued.json" -Encoding utf8
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/bin/WikiProbe.dll --arm A --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/A 1> "$wikiAttempt/A.stdout" 2> "$wikiAttempt/A.stderr"
$wikiExitA = $LASTEXITCODE
@{ phase='A'; state='returned'; exitCode=$wikiExitA } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/A-returned.json" -Encoding utf8
if ($wikiExitA -ne 0) { throw 'Arm A failed; no B and no retry' }
'{"phase":"B","state":"arm_issued"}' | Set-Content -LiteralPath "$wikiAttempt/B-issued.json" -Encoding utf8
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/bin/WikiProbe.dll --arm B --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/B 1> "$wikiAttempt/B.stdout" 2> "$wikiAttempt/B.stderr"
$wikiExitB = $LASTEXITCODE
@{ phase='B'; state='returned'; exitCode=$wikiExitB } | ConvertTo-Json | Set-Content -LiteralPath "$wikiAttempt/B-returned.json" -Encoding utf8
if ($wikiExitB -ne 0) { throw 'Arm B failed; no retry' }
```

Команды последовательно; после failure не выполнять следующую. .NET9 SDK/reference packs missing → `BLOCKED_ENVIRONMENT`. Собственный подготовленный host ожидаемо существует; неизвестный/чужой host до подготовки или любой существующий attempt/output → `BLOCKED_PATH_EXISTS`, ничего не удалять. Новый attempt только по отдельному прямому поручению с точным номером. Не чинить Windows settings, production tests, full solution или соседние опыты. В каждом arm сохраняются fixture/provenance, before/after snapshot, report.json. Общий русский отчёт — **`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/report.ru.md`**. После crash старые build.stdout/status.json остаются; отсутствие начавшегося обучения не называется нулевой точностью модели.

## 6. Фиксированный русский отчёт

1. Статус, source pin, simulated domain, fixture hash, actual counts, модель исполнителя по умолчанию.
2. Таблица correct/wrong/defer с A/B before/after и дельтами; per-row outcomes всех шести EXAM, включая неудобные случаи.
3. Затраты непустой Train A/B и snapshot bytes; все ухудшения рядом; не измеренные значения не заполнять предположениями.
4. Evidence actual weights update и native consumer; ограничения маленькой панели/JIT/gates.
5. Verdict по каждой useful metric. Source collision отдельно от learning gain. При успехе — candidate изменить original Featurize; compatibility/versioning не решены. Production решение за пользователем.

## 7. Готовый промпт исполнителю

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURE-KEYS-01.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

> Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Полная карточка: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/WIKI-FEATURE-KEYS-01.md. Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. ID WIKI-FEATURE-KEYS-01; сейчас документация/NOT_RUN до отдельного поручения исполнить. При исполнении используй модель по умолчанию текущей сессии, отчёт по-русски. Навыки core docs/skills architect-behavior-classifier, planning-session-startup, plan-review, code-examination, generated-code-review, prompt-patch-authoring, output-readability-gate, human-readable-architecture-audit — точные пути в work order. Подготовь только C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01 из pinned native BaselineModel по 04-source-copy-work-order.md. A native; B меняет только unigram/bigram codec §2 в Train и reader. Labels15,18epochs,LR0.14,margin/defer,порядок данных прежние. Не добавляй feature cache и чужой CandidateModel. Frozen fixture4TRAIN/6EXAM — явные CodeTraceTraining/WikiQuestionAnswer requests, не разные искусственные labels для trace trace_trace. Ровно A/B по командам карточки, output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001; четыре raw Train calls, два непустых. Измеряй actual training+native EXAM до/после, обе дельты и их разницу. Любое measured improvement useful metric — успех по ней и домену, accuracy↑ не требуется; все regressions рядом. Collision removal само по себе не quality gain; NO_GAIN допустим. Gold/fixture не менять ради результата. Если нет среды/host/build — BLOCKED/NOT_RUN, не чинить production. исходные карточки во время опыта не менять, старые experiments/results/failed attempts не удалять, production не продвигать. На «следующий эксперимент» немедленно прекращай старую задачу, без ещё одного commit/hash/build/ремонта отчёта. На «стоп/закончить немедленно» прекрати действия без ещё одного tool call; исключение — только необходимое прекращение своего уже работающего процесса этого опыта. Запрещено продолжать прежнюю задачу под видом завершения. Один отчёт, затем пользователь решает, какие исправления внедрять.

[features]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:341>
[runner]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs:43>
[train]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:58>
[reader]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:95>
[labels]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:25>
[targets]: <C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:211>


## Встроенный точный рецепт подготовки

Рецепт перенесён из [исходной спецификации](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/04-source-copy-work-order.md>); выше действует локальное назначение путей.

# Точное задание будущей подготовки WIKI-FEATURE-KEYS-01

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
| WIKI-FEATURE-KEYS-01 | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/fixture.json` из [WIKI-FEATURE-KEYS-01.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/fixtures/WIKI-FEATURE-KEYS-01.json>) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/WK01/results/attempt-001/A`, `…/B`, общий `report.ru.md` в `attempt-001` |

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

Существующий локальный файл: [fixture](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/fixtures/WIKI-FEATURE-KEYS-01.json>). SHA256 байтов: `90d70b9c6998d921465720ac743e095aade7d3f5126b937ae7dd2ee6305ce63d`. Исходный файл read-only; будущая копия должна совпасть с этими байтами. Никакая генерация данных не требуется.

```json
{
  "experimentId": "WIKI-FEATURE-KEYS-01",
  "origin": "simulation",
  "scope": "native wiki route learning: explicit CodeTraceTraining requests versus explicit WikiQuestionAnswer requests",
  "examScope": "six frozen paraphrase/control prompts; small domain panel, not production representativeness",
  "labelRule": "An explicit request to train the code trace builder is CodeTraceTraining; an explicit request to answer from project wiki evidence is WikiQuestionAnswer. Identifiers are task arguments, not route labels.",
  "train": [
    {
      "id": "K-T1",
      "prompt": "Train the code trace builder on calls inside load_customer.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-T2",
      "prompt": "Answer the project wiki question about how services load customer records.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-T3",
      "prompt": "Train the code trace builder on calls inside save_order.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-T4",
      "prompt": "Answer the project wiki question about how services save order records.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "identifier_vs_words"
    }
  ],
  "exam": [
    {
      "id": "K-Q1",
      "prompt": "Train code trace using source calls from load_customer.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-Q2",
      "prompt": "Answer the wiki question about which components load customer records.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-Q3",
      "prompt": "Learn source call edges in save_order for code trace training.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-Q4",
      "prompt": "Use project wiki evidence to answer how components save order records.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "identifier_vs_words"
    },
    {
      "id": "K-Q5",
      "prompt": "Train code trace on source call edges.",
      "expectedRouteKind": "CodeTraceTraining",
      "expectedTargetUnitId": "code-trace-builder-unit",
      "group": "plain_control"
    },
    {
      "id": "K-Q6",
      "prompt": "Answer the project wiki question about service ownership.",
      "expectedRouteKind": "WikiQuestionAnswer",
      "expectedTargetUnitId": "project-local-wiki-orchestrator",
      "group": "plain_control"
    }
  ]
}
```


## Единственный выбранный wiki-опыт

Общий исходный work order описывал два варианта. Эта карточка назначает только `WIKI-FEATURE-KEYS-01`; создавать второй host, запускать соседний factor или брать его CandidateModel нельзя. В recipe выбора counts применяются только значения текущей карточки. Встроенные source/fixture здесь достаточны для подготовки без поиска другой модели, проекта или набора данных.


**Немедленный переход/stop, также часть промпта исполнителю.** Команда пользователя «заканчивай и переходи» немедленно завершает текущий опыт и переводит к названной следующей задаче в пределах разрешённой фазы. После перехода не возвращайся дописывать report, hashes, review, retry или другие остатки предыдущего опыта. Команда «остановить всё» немедленно прекращает всю работу; не завершай ради оформления ни отчёт, ни проверки, ни повтор. Сохрани уже существующие результаты; новая команда не разрешает повтор ранее завершённого или прерванного запуска.
