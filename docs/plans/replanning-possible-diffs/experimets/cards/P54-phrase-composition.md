# P54. Сборка фразы и потерянные позиции

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-P54:START -->
## Выбрать лучший допустимый план — уточнение 11 ноября 2026

Уточнение F01/B01 и F02 к [карточке109](../../fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md) и [плану §3.1](../../fixes-based-on-experiments/audits/2026-11-11/repair-plan.md). Исторический PRIMARY ниже не переписан. Current planner максимизирует coverage, затем минимизирует spans; caller допускает coverage≥0,85 **либо** spans≥2 и coverage≥0,50. Один suffix winner может уничтожить допустимую альтернативу.

| Нормализованный рисунок | Current winner | Допустимая альтернатива |
|---|---|---|
| 8 tokens; a1 a2 / a5 a6 / a2…a7 | 6/8, 1 span: отказ | 4/8, 2 spans: допуск |
| 6 tokens; a1 a2 / a3 a4 / a2…a5 | 4/6, 1 span: отказ | 4/6, 2 spans: допуск |

Это контрпримеры coverage/span predicate, не native качество русского перевода. Выбранный проект ремонта сохраняет лучшие suffix по категориям числа spans 0/1/≥2, сохраняя actual count для tie-break, и выбирает лучший допустимый результат общим admission predicate. Один winner плюс поздняя проверка недостаточен. Пороги, learned targets и область EN_RU/≤8/no-word-mapping сохраняются.

[Независимый exhaustive oracle](../../fixes-based-on-experiments/audits/2026-11-11/followup/recompute_product_measurements.py) совпал с предложенной моделью на 33 866 графах длин 1–5. Это не C# PASS, не все возможные ключи/длины/targets и не замер цены. Native приёмка фиксирует actual normalized tokens, selected keys/spans, coverage, admission, target compatibility, final route и ответ обычного reader. Reverse mutation к одному suffix различает оба случая; lawful control и scope boundaries сохраняются.

Ресурсы сравниваются в одинаковом ordinary caller A/B окне. Смешанная дробь 3,3% не доказывает product overhead<5%; медианный +10% guard сохраняет только свою область. Каждая парная строка и редкое ухудшение видны отдельно.

Дозировка плана — будущая приёмка, не уже исполненные 9 уроков/15 чтений. Нового Train этим дополнением не назначено.

**Сверка движущейся ветки 05caa77 перед публикацией.** Параллельный DRAFT 6a6628c уже добавил PlanAdmissible с отдельным лучшим suffix для каждого точного числа spans; ordinary caller ещё использует прежний Plan. Приведённые выше три категории — допустимый оптимизированный проект для действующего predicate, а не приказ заменить работающий exact-count вариант. Следующий шаг — закончить и принять wiring с единственным owner admission, полным target/fallback результатом и актуальной resource-проверкой.

Current allocation-панель нового helper включает 9-token строку и не является whole-product A/B. Самостоятельные новые absolute caps 10 400/14 300 не закрывают старый product resource договор. Принять ресурсы можно только в названной области: planner cost, product delta и future regression нового helper раздельно. [Поздняя сверка](../../fixes-based-on-experiments/audits/2026-11-11/followup/05-late-b01-delta.md).

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-P54:END -->


<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница TRANSLATION:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold.

**Исходный адрес из паспорта:** `27.TranslationDomain/TranslationTrainedUnits.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST88-AUDIT-2026-11-01:START -->
## Уточнение после опыта 88 — Экспериментальный перебор и продуктовый планировщик — разные реализации

**Основание и ошибка, которую нельзя повторить.** b46d1af4 использует динамическое программирование, а P5401 измерял другой B-прототип. Его +14536 выделенных байт нельзя переносить как цену нового кода.

**Проверяемое требование.** Сохранить exact learned phrases, выбранные границы, покрытые source tokens и итог через ExecutorUnit. Контрпример close user / user session / close → close user session должен сохранять session; complete known phrase и отказ без покрывающих уроков — положительные границы. Для новой ресурсной оценки привязать измерение именно к TranslationPhraseCompositionPlanner.AlgorithmIdentity и достигнутому caller; 8/9 слов и иное направление проверяют dispatch, не только Plan.

**Граница вывода и исполнения.** Нативный source-linked Plan в аудите проходит 4 проверки, но это не полный Flow. Максимальное покрытие не доказывает роли слов или семантическую истинность перевода.

Это дополнение имеет приоритет только в названной области. Оно не увеличивает учебный бюджет, не меняет PRIMARY/NEXT и не назначает повтор завершённого опыта. Общий учёт всех вызовов, подпись evaluated project instances и исправляющие квитанции — [договор39, §11](../39-transfer-evidence-contract-2026-10-30.md#11-дополнение-после-p88--фактический-бюджет-вход-и-чувствительность). Доказательства и точный остаток: [113](../../fixes-based-on-experiments/113-post88-transfer-and-experiment-audit-2026-11-01.md), [114](../../fixes-based-on-experiments/114-post88-addressed-repair-plan-2026-11-01.md).
<!-- POST88-AUDIT-2026-11-01:END -->


<!-- AUDIT-2026-11-01:START -->
## Дополнение аудита 01.11.2026 — Композиция должна сохранить закон

**Основание:** [A06/RF06; аудит107](../../fixes-based-on-experiments/107-transfer-and-experiment-audit-2026-11-01.md), core `790228c256168a336b01abae4387e28391048c0c`. Этот блок уточняет текущую границу доказанного; исходный рецепт и его историческая дата ниже сохранены.

Успешный подбор фразы не доказывает, что сохранены обязательные слова и отрицание во всём composed answer.

**Адресная приёмка:** `COMPOSITION_REQUIRED_SPANS`: сохранить покрытые source spans и остаток; negative пример со знаком/обязательным объектом после первой найденной фразы должен провалить greedy drop. Positive lawful composition и literal exact route учитываются отдельно. Нельзя объявить component segmentation gain end-to-end gain, если ordinary exact lookup обходит изменённый выбор.

Дополнение не назначает повтор Train, не увеличивает прежний бюджет и не меняет scientific NEXT. Saved observations проверяются до любого нового вмешательства; ordinary product caller обязателен для claim переноса, а experimental host сохраняет свою объявленную границу.
<!-- AUDIT-2026-11-01:END -->

<!-- BEGIN audit-hardening-2026-10-30:P54 -->
## Приёмка после аудита 30 октября 2026

Этот блок уточняет действующие требования по [общему договору39](../39-transfer-evidence-contract-2026-10-30.md) и имеет приоритет над расходящимися обещаниями ниже. Прежние DESIGNED / SHALL_IMPLEMENT сохраняются как история проектирования на указанную там дату.

**Текущий статус.** Статус исполнения P54 данным дополнением не устанавливается.

### Проверка P54-PHRASE-SPAN-COVERAGE

**Где проверять:** ExecutorUnit learned PhraseMappings → phrase composition/span selector.

**Требование:** Coverage считается по непересекающимся позициям source, targets берутся из обученных mappings; cutoff≤8 и fallback>8 явны.

**WRONG:** Сложить lengths перекрывающихся close-user/user-session и назвать полным покрытием либо собрать gold target вне learned mappings.

**CORRECT:** Сохранить выбранные интервалы, uncovered позиции, tie order и route; «close» + «user session» различается с greedy по заданной цели.

**Положительная приёмка:** Три известные фразы остаются читаемыми, разрешённая сегментация не перекрывает source positions.

**Отрицательная приёмка:** Overlap запрещён; восьми- и девятитокенный boundary различает enumerator и прежний fallback без выдуманного quality gold для длинного edge.

Блок назначает критерии приёмки. Он не назначает новый Train и не меняет научный NEXT; уже оплаченный receipt и его исходные данные сохраняются.
<!-- END audit-hardening-2026-10-30:P54 -->

<!-- BEGIN primary-experiment-2026-10-27 -->
## Действующий эксперимент P54-PRIMARY-01

**Статус: DESIGNED_NOT_RUN. Host: SHALL_IMPLEMENT.** Дата: 27 октября 2026; исходный snapshot `6e58d068db4de36b82afff49fd8a8f98eba8eb9c`. Этот паспорт превращает выбранный ниже подслучай в конкретное задание. Старые описания «справочная / нельзя запускать без рецепта» ниже относятся к прежнему состоянию; остальные подслучаи сохранены как отдельные будущие сравнения, не считаются выполненными этим PRIMARY.

- **Гипотеза / граница вывода:** P54.1: выбор сегментации читает выученные phrase mappings.
- **Исходный код и потребитель:** 27.TranslationDomain/TranslationTrainedUnits.cs — ExecutorUnit.TrainOnExample и phrase composition.
- **TRAIN, EXAM, gold:** TRAIN EN_RU: 'close user'→'закрой пользователя' с PhraseMapping close user→закрой пользователя; 'user session'→'сеанс пользователя' с PhraseMapping user session→сеанс пользователя; 'close'→'закрой' с PhraseMapping close→закрой. EXAM 'close user session'→'закрой сеанс пользователя'; controls три TRAIN-строки; edge 'close user session now please again today safely' (8 content words) и тот же +'tomorrow' (>8), gold не выдумывать: coverage/route отдельно.. Точные общие строки — [наборы 35](../35-P-fixture-packs.md); локальные уточнения ниже имеют приоритет. TEST не передаётся learner.
- **Модель, tokenizer, S0:** Native TranslationMachine и её Unit trainers, semantic tokenizer без BPE/LlmModel. Точный JSON ingress и S0 — раздел Translation DTO в наборе35. Направление/тема/число lessons заданы сравнением. Reader-only A/B клонируются из одного ExportCheckpoint; producer A/B обучаются из равной пустой памяти. P59 использует закреплённый существующий S0.

### Назначенное сравнение

Один native S0,3 lessons. A greedy leftmost. B для ≤8 content tokens перечисляет непересекающиеся phrase spans, максимизирует число покрытых source tokens, затем минимизирует число spans, затем ordinal phrase IDs в source order. Candidate translation собирается из actual learned targets. Для >8 неизменный A fallback.

**Бюджет:** 3 урока;4 cold+4 A+4 B=12 чтений основной панели;2 edge×2reader=4, всего16. Enumeration cap2^8=256 subsets на запрос; один S0, без повторного Train.

**Контроли и критерии:** Положительные exact-controls3. Перевод композиции и coverage измерять отдельно. Третий single-word phrase позволяет B покрыть все3 слова; это не смысловой parser. Сохранить дополнительные CPU/allocations. Правильные TRAIN-controls, независимый EXAM, отказы, gained/lost и ресурсы записывать отдельно. Любое измеренное улучшение полезного показателя — успех этого показателя и домена, с ухудшениями рядом. 0→0 без приобретённого навыка не является учебным результатом. При retention assertion выполняется сразу после вмешательства, до любого восстановления.

### Файлы, запуск и завершение

H: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401`. Будущий исполнитель создаёт только H/Experiment.csproj, H/Program.cs и H/Run-Experiment.ps1 по [контракту 34](../34-P-experiment-execution-contract.md). Это явно назначенная реализация экспериментального host, не готовый проверенный runner и не правка production.

R: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401/results/attempt-0001`. Сохранить state, raw до/после, metrics.json и русский report.md. Уже существующую попытку сначала прочитать; не перезаписывать её и не запускать повтор по одному факту наличия новой карточки.

```powershell
pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P5401/Run-Experiment.ps1"
```

Команда действует после реализации host; сейчас она не запускалась и её работоспособность не проверена.

**Промпт исполнителю:** Полный действующий контекст: карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md; свежее полное чтение AGENTS/CLAUDE/skills — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1); роли — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2); применимость и разбор падений — C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткий список ниже дополнительный. Выполняется только прямо назначенный ID/этап; документальное поручение не разрешает запуск. PRIMARY, собственные примеры, данные, команды и бюджет полной карточки сохраняются. исполни только этот PRIMARY и контракт 34, модель текущей сессии по умолчанию. Учебная цепочка writer→state→обычный consumer обязательна; gold не подставлять в ответ. До запуска найди средства launch/wait по схемам доступных tools. Дождись terminal того же handle, не заканчивай ход сообщением «жду» и не делай частый polling. STOP Любы останавливает работу сразу; NEXT переводит на указанный ею ID, без продолжения прошлого опыта. После отчёта следующий ID — **P55-PRIMARY-01**, по [индексу](00-execution-index.md); не возвращаться автоматически к исправлениям. Успех не разрешает product/tests/CI/Git. Текущая работа автора карточки — только документация, **запуск сейчас запрещён**.
<!-- END primary-experiment-2026-10-27 -->

**Согласование incoming8346, 21.10: доставка phrase_mappings уже реализована.** Не назначать её повторное исправление как новый опыт P54: [reader](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4465) → [row](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:5146) → [builder](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4687) → [TrainOnExample](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:112). Сохранённый [ingress report](../results/2026-10-18/translation-memory-ingress-01/report.md) фиксирует 11/24→19/24, composition0/8→8/8, acceptedWrong1→3. Это доставка учебного материала в память, не neural DPO и не готовность всех P54.1–P54.7: для их отдельных reader-факторов нужны перечисленные ниже конкретные learned maps. Старые raw не перезапускать; новые hosts/результаты здесь не заявлены.


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

**Дополнение eaed/d7 — предел guard перед чтением памяти.** Исторический N60-E2 OrderedPhraseSupport/v1 задавал проверку до всех exact/phrase/word/kNN routes. Текущий описанный SourceSpanFrames guard стоит только перед phrase return. Это разные охваты; phrase-only проверка не доказывает защиту остальных readers. Не подменять её более широкой гарантией. Для отдельного whole-router/phase-only сравнения автор должен назначить learned snapshot, точный caller, fixture и host/entry; это NEEDS_INPUT_FOR_RECIPE, не новый runnable ID и не причина исключить семь учебно-зависимых P54 подслучаев. Positive-verifier correction с chosen.Pass=true остаётся условным lead: реальный producer/caller не установлен. VT37 использует отдельный установленный native chosen.Pass=false путь и этим не закрывает conditional lead.


## Действующее уточнение P54: проверка читателя уже обученной памяти относится к результатам обучения

**Дата:** 20 октября 2026. **Статус:** документное исправление границы применимости; эксперименты не выполнялись, эффекты не измерены. Внутренняя P54 исправлена по прямому поручению пользователя; исходный код не менялся. Этот файл дополняет P54.1–P54.7 и не назначает новые запуски вместо действующей очереди.

## Что необходимо исправить в постановке

В прежней [P54 во входящем аудите](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-results-and-new-gaps/06-P54-learned-reader-addendum.md:7>) семь подслучаев запрещены как `OUT_OF_LEARNING_SCOPE` с нулём разрешённых вызовов, поскольку меняют получение ответа из готовых maps. При этом вступление той же карточки и пользовательское поручение прямо допускают загрузку, восстановление и использование результата обучения с конкретной зависимостью от него. Требование обязательно добавить новый writer/update для каждого такого опыта — конфликт инструкций.

Здесь зависимость подтверждается кодом. Это не произвольный scorer или внешняя диспетчерская арифметика:

1. [TranslationMachine.TrainStructuredBatchAsync, 107–126](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:107>) вызывает зарегистрированный `TrainOnExample`, затем измеряет held-out результат.
2. [ExecutorUnit.TrainOnExample, 403–455](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:403>) записывает выбранные переводы в kNN store, `_exactSourceMap`, `_wordMap`, `_surfaceWordMap` и `_phraseMap`. Это конкретные результаты обучения памяти.
3. [ExecutorUnit.Execute, 264–294](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:264>) читает exact map, затем phrase map, затем word map и только после этого kNN.
4. [ExecutorUnit.MeasureAccuracy, 460–481](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:460>) вызывает этот же `Execute` для held-out примеров. Его результат возвращается через [TranslationMachine.MeasureHeldOutAccuracy, 662–671](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:662>) в показатели учебного batch.
5. [ExecutorUnit.ExportCheckpoint/ImportCheckpoint, 486–535](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:486>) сохраняет и восстанавливает эти maps; [TranslationMachine.ImportCheckpoint, 344–358](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:344>) связывает их с восстановленным runtime.

**Точная заменяющая формулировка для всех семи подслучаев:** «Читатель участвует в использовании и оценке обученного состояния. Разрешён ограниченный опыт на одном подтверждённом learned checkpoint: одинаковые память, tokenizer, запросы и оценка; различается только названная правка читателя. Новые Train/optimizer updates не требуются. До запуска автор обязан назначить конкретный checkpoint с происхождением из обучения, состав проекта, короткий путь, literal входы, единственную правку, команды и бюджет. Отсутствующий артефакт или runtime contract указывается по имени. Старая метка OUT_OF_LEARNING_SCOPE только из-за отсутствия нового update не применяется».

**Назначение исполнителю:** все P54.1–P54.7 сейчас NEEDS_INPUT_FOR_RECIPE, а не готовая команда запуска. Конкретные отсутствующие inputs перечислены ниже для каждого ID. Host, project и команды для этих семи ещё НЕ назначены. Автор карточки обязан закрепить их до runtime work order; исполнитель не ищет snapshot, не строит новый writer, не выбирает другое направление/папку/fixture и не запускает старые 8/50/100-call панели. В общей уже порученной программе — одна запись NEEDS_INPUT_FOR_RECIPE с ID и названной зависимостью, затем действующий NEXT; при одиночном поручении — сообщить ровно эту зависимость без запуска. Модель по умолчанию. Это снятие ошибочного запрета на учебную применимость, не выдуманный runnable host.

**Граница tokenizer:** ExecutorUnit.MeasureAccuracy создаёт новый TranslationSemanticTokenizer. Поэтому участие Execute в batch metric подтверждено, но импортированные tokenizer corrections в этом scoring path и полный runtime restore/Flow этим чтением не доказаны. A/B сохраняют один заранее назначенный tokenizer contract; не объединять component reader, batch metric и полный TranslationMachine в один результат.

Снятие ошибочного запрета **не означает**, что уже существует готовый паспорт запуска или доказано улучшение. В прочитанных источниках не найден закреплённый checkpoint, проверенный на необходимые ниже содержимое maps и маршрут. Ссылка на один класс или вручную выдуманный словарь не заменяет такой артефакт. Честное состояние конкретного рецепта — «учебная связь подтверждена; указанные входы ещё нужно закрепить», а не «обучение запрещено».

## Общие условия для будущего сравнения

Домен — **перевод**. RU→EN и EN→RU — разные направления внутри этого домена; результаты по ним нельзя автоматически объединять или переносить на HE/ES. Для примеров с C# identifiers дополнительно фиксируется предмет перевода технического текста.

**До/после:** A читает неизменный learned checkpoint исходным `Execute`; B читает тот же checkpoint копией с одним исправлением. Это сравнение использования результата обучения, не новый рост навыка от A0 к A1. Состояние памяти после чтения должно остаться тем же. Для любого выбранного подслучая нужны реальные ответы, маршрут `AdapterId`, покрытие обязательного смысла, ошибки, отказы и стоимость именно этого reader; не добавлять новый Train для формального допуска.

Точный checkpoint должен содержать provenance учебных записей, направление, tokenizer/signature contract и требуемые maps. Для каждого запроса нужно заранее исключить перехват `_exactSourceMap`; иначе исследуемая phrase/word ветвь не достигается. Полученная неизменность ответа при таком перехвате — результат отсутствия контраста, не опровержение дефекта reader. Если host обращается к отдельному методу, его вывод называется компонентным; полный `TranslationMachine`/route не считается пройденным.

Не подставлять gold translation, TEST expected, готовую правильную роль или маску в изучаемый consumer скрыто. Если семантическая маска/отношение задано как разрешённый controlled input, обозначить это и ограничить вывод компонентом. Успех автоматического извлечения этих отношений этим не доказывается.

## P54.1 — выбор сегментации выученных фраз

**Исходник и состояние:** `ExecutorUnit.TryComposeFromPhraseMappings`, [798–857](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:798>). `_phraseMap` записывается через `TrainOnExample → AddPhraseMapping`, [453–455 и 756–760](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:744>). Текущий reader сортирует фразы по длине и выбирает первый подходящий span слева.

**Один предлагаемый фактор:** для заранее ограниченных запросов до 8 content tokens перебрать непересекающиеся сегментации и выбрать maximum mandatory coverage, затем минимум spans, затем заранее назначенный ordinal ключ фразы. Значения переводов, learned maps, нормализация, tokenizer и правила допуска target одинаковы. Более длинные запросы остаются на исходном пути и не смешиваются с основной панелью.

**До/после и граница:** сравнить выбранные spans, конечный текст, обязательное покрытие и стоимость reader; controls — одна допустимая сегментация и запрос без phrase hit. Повышение покрытия не доказывает сохранность ролей: семантический выбор вынесен в P54.6.

**Чего конкретно не хватает:** checkpoint с реально выученными пересекающимися `close user` / `user session` либо аналогичными фразами одного направления и контрольными word mappings, плюс фиксированный список запросов, не имеющих exact entry. Не назначать выдуманный словарь как якобы найденный saved result.

## P54.2 — исходный индекс после удаления пустых нормализованных tokens

**Исходник и состояние:** [804–807 и 837–842](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:804>). После фильтрации пустых normalized strings индекс `i` применяется к исходному `tokens.Tokens`; fallback затем читает обученные word maps через `TryGetAcceptedWordMapping`, [860–869](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:860>).

**Один предлагаемый фактор:** сохранить пары `(normalizedWord, originalIndex)` при существующей фильтрации. Для word fallback брать `tokens.Tokens[sourceTokens[i].OriginalIndex]`. Фильтрация, phrase match, пороги coverage и значения maps не меняются; дополнительное сохранение boundary — отдельная P54.4.

**До/после и граница:** один и тот же learned checkpoint, запросы с ведущим, внутренним и завершающим punctuation-only token, контроль без удаляемого token. Записать фактические token index → lemma/concept → map lookup, spans, текст и ошибки. Отличие индекса само по себе не считается улучшением перевода; оно должно доходить до нужного ответа или корректности использования выученной записи.

**Чего конкретно не хватает:** checkpoint с phrase entry и fallback word mappings, а также реальные token sequences данного tokenizer, в которых удаление действительно создаёт сдвиг и exact branch не срабатывает. Нельзя считать любой тире автоматически таким token.

## P54.3 — пропущенное обязательное слово в phrase composition

**Исходник и состояние:** [824–857](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:824>). Незнакомое слово может не добавить часть результата; допуск зависит от общего coverage или repeatedPhraseCoverage. Результат состоит из обученных phrase/word entries.

**Один предлагаемый фактор:** перед прежним успешным return проверить, что покрыты все tokens, объявленные обязательными разрешённым входным контрактом. При неполном покрытии вернуть `false` и продолжить прежний `Execute` fallback. Не писать «корректный отказ» по одному `false`: последующие ветви могут всё равно вернуть неверный ответ, поэтому нужен конечный результат.

**До/после и граница:** считать пропуск отрицания/actor/action, wrong accepted/all, answered/all и конечный output. Снижение ошибок с падением coverage показывается рядом. Это улучшение использования выученного материала, не освоение отсутствующего перевода.

**Чего конкретно не хватает:** checkpoint с частичным phrase покрытием различающей строки и независимый разрешённый источник mandatory mask. Если mask дан вручную, допустим только явно обозначенный controlled-input результат; автоматическая языковая польза остаётся неподтверждённой.

## P54.4 — исчезнувший разделитель создаёт ложную соседнюю фразу

**Исходник и состояние:** [804–807, 824–833](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:804>). Фильтрация может соединить слова по разные стороны удалённого token; `MatchesAt` читает уже сокращённую последовательность и выбирает обученную phrase entry.

**Один предлагаемый фактор:** сохранить boundary marker у заранее объявленного значимого разделителя и запрещать только multiword phrase span через него. Остальные punctuation правила, word fallback, индексы и maps одинаковы. Не чинить одновременно P54.2.

**До/после и граница:** сравнить `close -> session` и `close session` только при подтверждённой токенизации и контракте, где стрелка разделяет операции. Записать ложные phrase hits, конечный перевод и стоимость. Обычная запятая не становится универсальным семантическим запретом.

**Чего конкретно не хватает:** learned checkpoint с соответствующей multiword phrase, actual tokenization данного разделителя и документированный смысл boundary в выбранном профиле.

## P54.5 — неполное покрытие в самостоятельной word-map ветви

**Исходник и состояние:** `TryComposeFromWordMappings`, [366–400](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:366>). Незнакомые слова не добавляются; достаточно двух mapped tokens и доли 0.50. `Execute` возвращает результат до kNN на [273–277](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:273>).

**Один предлагаемый фактор:** при пропуске обязательного token вернуть `false` из этого составителя и продолжить прежний fallback. Не изменять обучение maps, `k`, similarities, phrase branch или target script gate.

**До/после и граница:** checkpoint знает переводы `open` и `files`, query `do not open files` не имеет exact/phrase entry. Сравнить raw word composition и конечный output, сохранность отрицания, wrong accepted/all и coverage. Контроли — полностью покрытая строка и all-unknown. Снижение wrong answers может сопровождаться отказами; не называть его знанием нового слова.

**Чего конкретно не хватает:** подтверждённые word-map entries и отсутствие exact/phrase перехвата в выбранном checkpoint; разрешённая mandatory mask и ожидаемое поведение оставшихся ветвей. Маску из TEST gold не использовать.

## P54.6 — семантический выбор среди альтернатив

**Исходник и состояние:** тот же [TryComposeFromPhraseMappings, 798–857](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:798>). Текущий метод выдаёт одну жадную композицию. Подслучай зависит от заранее подготовленного общего для A и B bounded composer P54.1; иначе одновременно менялись бы генерация и выбор.

**Один предлагаемый фактор:** для одного и того же списка кандидатов A выбирает coverage/spans/ordinal, B сначала исключает противоречие доступному runtime action–object relation, затем применяет тот же rank. Для `close user session` контекст должен независимо задавать object=session и possessor=user. Gold перевод не является источником такого контекста.

**До/после и граница:** список candidates одинаков, learned values одинаковы; измерить сохранность отношения, false reject и стоимость reader. Контроли — единственный законный кандидат и две допустимые сегментации. Это отдельная проверка выбора результата выученных записей, не доказанный универсальный parser.

**Чего конкретно не хватает:** сохранённый learned checkpoint, закреплённая общая версия composer с кандидатами и фактический разрешённый supplier action–object relation. Без последнего возможен только controlled-input вариант с явно ограниченным выводом. Эта зависимость не даёт права исполнителю самостоятельно объединять P54.1 и P54.6.

## P54.7 — ранний word-map return скрывает подходящую запись kNN

**Исходник и состояние:** ранний return [273–277](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:273>), последующий kNN route [279–294](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:279>), `TrySelectKnnCandidate` [333–363](<C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:333>). Обе памяти заполняются одним `TrainOnExample`; это настоящий consumer их результатов.

**Один предлагаемый фактор:** перед возвратом word-map candidate проверить противоречие разрешённому runtime relation/polarity contract. Только `CONTRADICTION` продолжает неизменный kNN путь; `COMPATIBLE` и `UNKNOWN` сохраняют исходный return. Не менять kNN order, thresholds, stored pairs или обучения ради выигрыша.

**До/после и граница:** одна память содержит неверную для данного контекста word composition и подходящую kNN запись. Считать фактический маршрут, selected example, конечные actor/action/recipient/polarity, false reject, coverage и стоимость. Контроли — правильная word-map composition и случай, где оба пути неверны. Перечисление сохранённых кандидатов без изменения output — trace, не полезный результат само по себе.

**Чего конкретно не хватает:** checkpoint и запрос, на которых обе записи реально достижимы; source contract/checker, получающий смысл из разрешённых входов; trace подтверждённого kNN eligibility. Наличие красивой ручной пары `Anna gave Mary` / `Mary gave Anna` этих условий не доказывает.

## Общие правила карточки и любого следующего промпта

> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

- **Модель:** модель текущей сессии по умолчанию, без override. Фактическое имя в отчёте. Не искать отдельную «более сильную» модель, другой host или другой опыт.
- **Успех:** любое фактически измеренное полезное улучшение считается успехом по соответствующему показателю и домену; рост accuracy не обязателен. Все ухудшения, дополнительная стоимость и пределы причинного вывода указываются рядом. Одно фиксированное A→B время не доказывает устойчивое ускорение.
- **Изоляция:** при будущем поручении на эксперимент допустимы `throw`, `try/catch`, `void`, простые DTO и минимальный host в назначенной копии. Не менять продукт, его тесты или CI; не выполнять полную цепочку переноса до решения пользователя.
- **Отчёт:** русский Markdown, один `report.md` с source→state→consumer, точным сохранённым артефактом, A/B, реальными ответами, метриками и регрессиями; успешный результат оформить кандидатом исправления. Сейчас регистрируется только документный кандидат.
- **STOP/NEXT:** по «заканчивай и переходи к следующему» немедленно перейти к уже назначенной следующей строке действующего индекса в разрешённой фазе. Запрещено возвращаться к прежнему отчёту, hashes, review, retry или закрывающим проверкам. По «остановить всё» немедленно прекратить работу; допустима лишь необходимая остановка собственного процесса. Незавершённые файлы и результаты не удалять. Эти правила обязательны для каждого из семи подслучаев и каждого будущего промпта.
- **Порядок:** P54 — поздняя проверка читателя результата обучения. Старые attempts №01–09 сохраняются; учебная цель№08 OPEN. Активный вход — [PARENT-ADOPTION-01](PARENT-ADOPTION-01.md), затем [N63-INGRESS-LENGTH-LEARNED-01](N63-INGRESS-LENGTH-LEARNED-01.md) по [текущему индексу](00-execution-index.md#next-contract). Это уточнение не вставляет P54 в текущую очередь перед разблокировкой обучения.

## Граница проверки

Проверены текст семи подслучаев и названные участки двух файлов core на commit `0b76ab950d82534f5904005dda096545505ae307`: `TranslationTrainedUnits.cs` blob `1742abfc9fb617dfc66e4d618e0c169da01a6ccc`; `TranslationMachine.cs` blob `fd0e736de3739236b4dde2d6b9d000f5ebae8416`. Проверка — чтение кода и документации. Сборки, native calls, восстановления и обучения не было. Исторические успешные learned checkpoint здесь не объявляются отсутствующими во всех ветках: лишь нужный для данного рецепта закреплённый артефакт пока не подтверждён данным просмотром.



---


**Обязательное уточнение запуска и выводов (новый журнал 20.10).** Сначала полностью записать/закрыть script и входы, затем один последовательный запуск; не запускать одновременно с записью. До build/Train проверить существующий результат и раннюю защиту от повтора. Поздний FileMode.CreateNew после обучения защищает файл, а не бюджет. При timeout/неизвестном исходе наблюдать тот же process/session ID; новая копия, удаление partial/raw/claim или третий каталог запрещены. Существующий JSON и пустой log не доказывают успешный exit или единственный запуск. Равные первые loss не доказывают тождество states; 4 steps/full-sequence/frozen head остаются раздельными гипотезами. Это правило входит в каждый промпт ниже; текущая документная сессия runtime не запускает. [Подробности](../00-execution-policy-2026-10-20.md).

**Дополнение по новым конфликтам (20.10).** Это исходное семейство гипотез, не отдельный runtime work order. До передачи исполнителю автор конкретного рецепта назначает наблюдаемый положительный учебный сигнал; одинаковые 0→0, записанные веса или экономия не доказывают освоение задания. В изолированном опыте допустимы throw/try-catch/void и минимальный host, без переделки в product OperationResult/комментарии к каждой строке. Модель сессии по умолчанию сохраняется. При обычном завершении успешного назначенного опыта обязательны русский отчёт и одна краткая запись кандидата в [required-changes.md](../../required-changes.md); не нужны отдельное поручение регистрации, product tests/CI/Git или ручные checksums. Немедленные переход/stop имеют приоритет. Fixed-order время/CPU/process peak — наблюдения без установленной причинности. Подробности — [действующее правило](../00-execution-policy-2026-10-20.md); эти ограничения обязательны для каждого промпта ниже.

**Разрешение конфликтов из последнего аудита (20.10).** Старый запрет копий в `81-validation-program.md` не применяется: используется только назначенная A/B-копия; product/tests/CI не меняются. Ограничение «protected не менять» в отчёте помощника относилось к его поручению, а не отменяет прямое поручение Любы исправить карточки. `NOT_RUN` означает отсутствие результата данной версии, а не запрет; поздняя карточка не меняет протокол уже сохранённого опыта и не назначает повтор. Любое полезное измеренное улучшение — успех по метрике/домену, без порога5% и требования роста accuracy; несопоставимые затраты не приписываются фактору. Связанные подготовка, restore и чтение выученного состояния допустимы с точным source→state→consumer; посторонний scorer-пример не назначает опыт. Вместо отсутствующей84-ссылки — [реестр источников](../01-full-source-registry.md) и [карта покрытия](../2026-10-19-small-learning-probes/06-findings-map.md); следующая работа задаётся [индексом](00-execution-index.md). Назначенные данные, домен, host и команды нельзя заменять самостоятельным поиском другого опыта.

**Место в каталоге:** справочная карточка. Порядок маленьких запусков и точные рецепты — в [едином индексе](00-execution-index.md); номер P не является номером запуска.

## Немедленное завершение по указанию Любы

**Обязательно для каждого подслучая и каждого промпта исполнителю этой карточки.** Прямое текущее указание Любы имеет приоритет над нижними требованиями завершить отчёт, review или весь цикл.

- **«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу.
- Если следующее задание уже указано, не выбирать другое и не обходить каталог в поисках более готового. Если сказано «следующий» или назван порядковый номер — взять его из единого индекса `cards/00-execution-index.md`; не искать N-KU-02 по аналогии с N-KU-01 и не задавать вопрос о выборе. После конца назначенной очереди кратко сообщить об этом, не возобновляя прошлый опыт. Поручение редактировать карточки или описывать опыт само по себе не разрешает запуск обучения.
- **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Команду перехода не подменять остановкой всей программы; команду полной остановки не подменять следующим запуском.
- Незавершённые файлы и уже записанные данные оставить как есть. Их наличие не разрешает продолжать вопреки указанию. Не удалять raw/receipts и не устраивать закрывающую проверку; краткий статус — только по уже известным фактам, без новых инструментов.
- Исполнитель опыта не берёт на себя исправление карточек: это отдельное поручение документной сессии. Остановка одного исполнителя не отменяет отдельное поручение другой сессии, если Люба не остановила и его.

## Как выполнять этот опыт: разрешение конфликтов инструкций

## Готовый формат отчёта и навыки — без поиска образца

**Когда оформление закончено.** Достаточно одного русского report.md по этому каркасу: до 350 слов пояснений плюс таблица фактических метрик/ответов и ссылки на raw. При обычном завершении полезного измеренного опыта обязательно добавить или уточнить одну запись до 120 слов в ../../required-changes.md со ссылкой на отчёт; отдельное поручение не требуется. Не выполнять оформление после команды немедленно перейти или остановиться. Один раз прочитать записанное, исправить только фактическую ошибку, сломанную ссылку или кодировку. Не делать циклы стилистического улучшения, Mermaid/SVG/render, исследования Git или новую проверку алгоритма. Не вычислять и не сверять hash ради отчёта. Уже существующие технические receipts оставить как есть; после правки текста достаточно фразы «уточнён отчёт, данные и опыт не менялись». Не удалять старые raw или receipts ради упрощения отчёта. Состояние «независимый review не выполнен» записать честно, оно не требует искать другую модель или удерживать оформление открытым. После этих файлов задача оформления завершена; следующая работа — только уже назначенный следующий опыт, без нового поиска формата.

**Язык — русский. Формат — обычный Markdown.** Итоговый файл — `report.md` в точном каталоге результатов выбранного подслучая, указанном в этой карточке; рядом остаются исходные JSON/логи. Не искать другие отчёты, новый реестр или шаблон. Перед каждым новым аудитом/заданием свежо и полностью прочитать governance и весь основной/ролевой стек договора120 из входного блока этой карточки; старые пять навыков не являются полным перечнем. Внутри того же задания отчёт/readback/wait не требуют повторять уже выполненное полное чтение. Исторические команды навыков не расширяют текущую фазу, модель, данные или бюджет.

Скопировать следующий каркас и заполнить только фактическими данными. Пустые измерения писать «не измерено», сбой — «этап не завершён», а не 0.

```markdown
# <ID>: <что изменилось в обучении, простыми словами>
Домен: <предметная тема>. Исполнитель: <фактическая модель и режим>.
Проверяющий: <фактическая модель или «не выполнялась»>.
execution_status: <NOT_STARTED / PARTIAL / COMPLETED / FAILED / OUTCOME_UNKNOWN>.
learning_validity: <VALID_FOR_NAMED_UNIT / INVALID_NO_TRAINING / INVALID_ZERO_TO_ZERO / INVALID_UNQUALIFIED_BEFORE / INVALID_INCOMPLETE_COMPARISON / NOT_EVALUATED / UNKNOWN>; единица и raw before/after: <точно>.
hypothesis_status: <OPEN / SUPPORTED_FOR_NAMED_METRIC / NOT_SUPPORTED_IN_VALID_COMPARISON>; invalid всегда оставляет OPEN.
Источники: <паспорт, raw, команды>; завершённые updates отдельно от ошибки сохранения, неизвестные counts=null, невыполненное плечо — не измерено.

## Что меняли
<Один фактор, исходный файл/метод, куда урок записывает навык и кто его читает.
A — исходный код, B — конкретная правка только экспериментальной копии.>
## На чём проверяли
<Точные TRAIN/EXAM или ссылка на сохранённые данные; начальное состояние; реальные вызовы/предъявления.
Запланированный и фактический бюджет отдельно; лишние операции перечислить.>
## Что получилось
| Показатель, единица и область | A до | A после | B до | B после | Изменение и смысл |
|---|---:|---:|---:|---:|---|
| <точность/ошибки/состояние> | | | | | |
<Стоимость реального перехода обучения: A = …, B = …, B−A = ….
Время, CPU, allocated bytes, retained heap, RSS/peak — отдельные строки.
Слои модели, уровни Unit/Flow и усилия подготовки — значения или не измерено.
Для короткой панели привести каждый фактический ответ и gold.>
## Выигрыши и ухудшения
<Любое полезное измеренное улучшение — успех по этой оси и домену;
рост accuracy не обязателен. Все ухудшения рядом. Ожидание не результат.>
## Граница вывода и кандидат изменения
<Что измерение доказывает и чего не измеряло. Не объяснять шум/JIT без контроля.
Точный исходный метод и предлагаемый diff для решения Любы; основной код
автоматически не меняется. Никаких новых запусков ради составления отчёта.>
```

**Хорошо:** «За тот же учебный переход выделено A=12 МБ, B=9 МБ; ответы одинаковы. Выигрыш по выделениям — 3 МБ. Пик процесса вырос на 1 МБ. Один запуск; причина разницы wall-time не установлена». Числа здесь — только образец формы, не результат.
**Термины и предел редактирования.** Сначала простое русское название, затем точный ID/API в скобках; статус словами, машинный код — рядом в backticks. Не строить Mermaid, симуляцию ветвей или повторный расчёт алгоритма ради отчёта. Использовать уже записанные ответы и counters. «Не предложил лишний цикл» не означает «весь совет правильный»; «расходы не измерены/шумны» не означает «без дополнительных затрат». Проверка push, обход истории и перезапуск модели не являются частью написания отчёта.
**Плохо:** «Все тесты зелёные, обучение исправлено»; «accuracy не выросла, поэтому экономия памяти не считается»; «память стала 9 МБ» без указания, что измеряли.


**Послабление для экспериментального кода — прямое уточнение Любы.** Малый экспериментальный host может использовать `throw`, `try/catch`, `void`, простые DTO и собственный формат журнала. Не переделывать API скопированного алгоритма ради продуктового `OperationResult`, обязательных комментариев, полного code-slice workflow или требований к публикации. Для ошибки достаточно сохранить её текст, этап, фактические счётчики и ненулевой exit code; не превращать сбой в нулевую точность и не проглатывать его. Это послабление относится к оболочке опыта, не разрешает менять данные, фактор, бюджет или исследуемый алгоритм между A/B. Полные продуктовые требования применяются позже, если Люба поручит перенос результата.

**Прямое уточнение Любы, 20.10.2026:** сначала проверить изменение на экспериментальной копии того же кода и передать результаты пользователю; перенос в основной код — только по её отдельному решению. Поручение на проектирование означает правку карточки; прямое поручение на запуск означает выполнение опыта по порядку ниже.

1. Выполнить назначенный подслучай и предметную тему по конкретному паспорту ниже. Уроки, EXAM, начальное состояние, один фактор, бюджет, точные пути, зависимости и команды задаёт автор карточки; исполнитель только материализует приведённые данные и рецепт. Ему не поручается выбирать другой host, искать входы, придумывать исправление или разрешать конфликт правил. Общего `PENDING` и повторного личного разрешения на каждый ID/SHA нет. Если доступный технический рецепт ещё не заполнен, это `PREPARABLE` — адресный недочёт автора карточки, а не запрет обучения и не вопрос на новое разрешение Любы.

**После завершения двух плеч.** Продолжение по сохранённому снимку не разрешает операции, которые пользователь не поручал: дополнительные warmup, equivalence, cosine, B→A или reviewer rerun. Зафиксировать уже выполненные лишние вызовы отдельно и не выполнять оставшиеся самовольные добавления. Не обходить worktree, не исследовать `.claire`/`.clone`, не чинить чужие терминалы, навыки или память агента ради гигиены опыта. Редакционная правка report.md не требует нового A/B или вычисления hash: сохранить старые receipts, добавить одну фразу «уточнён текст, данные и опыт не менялись». Не создавать новые таблицы контрольных сумм ради отчёта. Для reviewer использовать доступную модель сессии по умолчанию; другой worker означает независимую проверку, самопроверка — нет. Не связывать сохранение результатов с разрешением на commit/push или перенос в продукт.
2. Эта P-карточка сохраняет исходные варианты, но не назначает проект, домен или запуск по шаблону <ID>. Единственный источник задания — конкретная строка [индекса](00-execution-index.md) и её исполнительская карточка: там назначены полный путь A/B-копии, host, входы, Student, команды и output. Не искать и не выбирать другой проект. Если для запрошенного подслучая такого рецепта ещё нет, это документная работа автора карточки, а не поручение исполнителю изобрести host; в разрешённой программе перейти к назначенному «Дальше». Модель исполнителя — модель его сессии по умолчанию без override; не путать её с обучаемым Student.
3. A — исходная реализация, B — её копия с одним изменением. Сохранить исходный commit, пути/методы, hash скопированных файлов и A/B diff. Общие зависимости и начальные состояния одинаковы; фактический learner исполняется в экспериментальном проекте. Не заменять его расчёт ожидаемым ответом или ручной имитацией. Ссылки ниже на оригинал — происхождение кода и будущий адрес переноса, а не разрешение редактировать production.
4. Выполнить одинаковые уроки и контрольные задачи до/после в A и B. Для зависимого от обучения участка допустим общий сохранённый обученный state. Собрать только точный host из паспорта, затем вызвать его DLL по указанной там команде; не загружать сборку через PowerShell reflection и не запускать общий dispatcher. Не добавлять warmup, повторы, проверку фасада или новые уроки сверх счётчиков карточки. Для совсем короткого измерения указать шум/предел разрешения, а не вводить 20+200 прогонов. Разница A/B не разрешает дополнительный порядок B→A, новый диагностический каталог или повтор от reviewer. Сохранить наблюдаемую разницу; без отдельного контроля не объявлять JIT, порядок запуска или «шум» доказанной причиной. Уже выполненные сверх бюджета операции сохранить и отдельно перечислить, не включать молча в основной результат. Не чинить продуктовые тесты, не запускать полный CI, старый Main500 или другой S-слайс ради этого опыта.
5. Записать реальные ответы, обучение, время, память, усилия, слои модели и уровни Unit/Flow; неизмеренное обозначить явно. Любое полезное измеренное улучшение — успех по показателю и домену, без обязательного роста точности; ухудшения указать рядом. Отчёт и raw outputs: `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/<дата>/<ID>/`. Показать Любе результат и точный кандидат переноса. Даже успешный опыт не разрешает менять основной код, его тесты, делать commit/push или разворачивать полную цепочку внедрения.

**Какие старые указания здесь не применяются.** `AGENTS.md`/`CLAUDE.md`, разделы `GOLDEN RULE`, `EXECUTION_INTEGRITY_PUBLICATION_GATE` и `G-route/G-loop`, описывают продуктовый цикл; они не превращают этот опыт в внедрение, обязательный push или бесконечный следующий слайс. `.kiro/skills/xiigen-code-slice/SKILL.md` отправляет в `.kiro/xiigen-slices/00-START.md` и чужой active brief: не использовать этот переход для данной карточки. Требования ниже изменить оригинал, запретить точную экспериментальную копию либо ждать личного допуска каждой версии заменены этим прямым уточнением пользователя. AGENTS, CLAUDE, навыки и steering в рамках карточки не редактировать.

**Навыки и роли.** Перед новым заданием свежо и полностью прочитать основной стек [договора120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md#1-свежее-полное-чтение-перед-новым-аудитом-или-заданием) и дополнительные навыки роли из [40](../40-effective-executor-context-2026-11-02.md); раздел «Готовый формат отчёта и навыки» задаёт формат, а не сокращённый перечень чтения. Исполнитель и последовательный независимый проверяющий используют модель своей текущей сессии по умолчанию, без model/reasoning override и поиска Astra. Это прямое уточнение Любы от 20.10.2026. Фактическое имя модели записать; если платформа его не сообщает — «не раскрыто платформой», без догадки. Не более одного активного worker, без вложенных агентов. Модель исполнителя не является обучаемым Student/Unit. Проверяющий читает исходники, diff и сохранённые raw/receipts. Независимость означает отдельного проверяющего, а не другую модель; самопроверку так и назвать. Никаких повторных Train/Predict или прогревов ради review. После compaction восстановить паспорт, stage и actual counts; summary не является новым поручением.

**Параллельная сессия улучшения карточек.** По прямому разделению работы Любы другая сессия читает рассуждения исполнителя и правит только карточки: она не меняет код, данные, веса или runtime и не запускает эксперименты. Поэтому её документные правки и новый HEAD с такими правками не создают конфликта запуска готового опыта. Исполнитель работает по зафиксированному паспорту; документная сессия не требует остановки, повторного личного допуска или нового обхода AGENTS/skills. Реальное пересечение возможно только при фактической работе с теми же исполняемыми файлами или состоянием; его нельзя предполагать из одной правки Markdown.

**Сохранение экспериментов — прямое правило Любы.** Можно добавлять новые экспериментальные файлы и результаты; старые эксперименты, исходники, результаты и каталоги сборки `bin/obj` не удалять, не переносить и не перезаписывать. Общие cleanup-инструкции этого не разрешают. Совпадение пути с уже начатым опытом не разрешает пересоздать его: продолжать его зафиксированную версию, если именно она поручена, сохраняя уже полученные результаты. Новая редакция карточки не означает второго запуска или автоматической миграции текущего опыта. В документной сессии разрешено менять только карточки.

**Версия без бесконечного перечитывания.** Один раз прочитать фактические bytes выбранной карточки, исходников и применяемых навыков и записать их hashes в паспорт. Изменение HEAD из-за чужих документов не требует повторного обхода всех AGENTS/skills и не меняет уже зафиксированные условия опыта. Если до запуска изменился сам используемый механизм или его входы — сравнить именно это изменение и закрепить согласованную версию. Не считать hash старых bytes hash текущего файла. Чужие правки не откатывать; результаты не перезаписывать.

**Обязательное дополнение к каждому промпту исполнителя в этой карточке:** «Работай на точной экспериментальной копии исходного механизма по порядку выше. Основной код и продуктовые тесты не меняй; после измерений передай результаты и предлагаемый перенос Любе. Не запускай продуктовый цикл и не переназначай модели самостоятельно».


## Исходные варианты для автора — не исполнительский паспорт

Это историческая карточка вариантов, **не готовый исполнительский паспорт**. Запрошенные исполнитель и reviewer: **модель текущей сессии по умолчанию**, последовательно, не более одного активного worker; фактическое назначение записывается при выполнении. Отдельное личное согласование каждой карточки отменено указанием пользователя от 20.10.2026. Student/Unit не равен модели исполнителя: точное имя, конфигурацию и исходное состояние назначает автор конкретного рецепта для одного подслучая и предметного профиля. Исполнитель не выбирает Student из справочника.

**Исходное задание и ограничения — дословно:**

«Ок, рассмотри пожалуйста последние обновления помошников и выстрои из них экперименты, как можно более простые и маленькие для каждого домейна. Задача ускорить процесс проверки и сузить обучающийся механизм до очень простых маленьких примеров, что бы проверить улучшаются показатели довольно быстро. Отчетность должна быть человеческим языком но точно показывающая какую часть механизма обучения меняем и как будем тестировать.»

«Разве эксперименты не были построены по формуле: Что  было - пробег по маленькому сету, изменение в качестве эксперимента, измеренеи как стало стем же сетом?»

«ну раз так то у меня нет особо выбора, ты должен внести изначальные промпт, скилы, начальные данные, название модели, ожидаемый  результат в тело эксперимента и мне придется тебя проверить перед началом работ.»

«Нельзя подменять задание пользователя - измерять изменения в процессе обучения - если то что измеряется никак не участвует в процессе обучения - это не валидный эксперимент! Не валидно тратить на это токены! Особенно когда, пользователь это запретил!»

Критерий пользователя: любой фактически измеренный полезный выигрыш в процессе настоящего обучения — успех по показателю и домену; рост точности не обязателен. Все другие изменения и потери рядом. Отдельный serving/report/cold контроль не становится учебным опытом из-за формальных уроков. Для изменения самого обучения оба плеча действительно учатся и фиксируются A0/A1/B0/B1. Для восстановления или использования его результата допустим общий доказанно обученный state и сравнение зависимого участка до/после одной правки; код этой связи и способ исправления обязательны. Существующие acquisition/no-update контроли не переименовываются в новое обучение.

**Данные и ожидаемый результат:** конкретные исходные строки, TRAIN/EXAM и gold находятся ниже в выбранном `Dxx — профиль проверки` и `Pxx.n — самостоятельное сравнение`. Они не образуют автоматически один общий запуск. Автор до передачи исполнительской карточки фиксирует в её теле один Pxx.n, один Dxx, точные уроки/экзамен/prompt/рубрику, native writer/update/reader, допустимые файлы, бюджет и ожидаемые различающие наблюдения. Если данные в профиле описаны только условием, это **NEEDS_INPUT**, а не разрешение исполнителю придумать их во время опыта.

**Незаполненные поля:** `selected_subcase`, `selected_domain_profile`, точные initial data/state/model/config/tokenizer/reference/optimizer/RNG bytes и применимые SHA256, actual native caller/command, одна разрешённая mutation, output/RunId, окончательные counts/cost bound и ссылка на действующее поручение пользователя. Для Unit-пути нейронные поля явно N/A с причиной, не вымышленные веса. Общий источник ниже не заполняет эти поля автоматически. Пока автор не назначил подслучай и полный рецепт, это справочник вариантов. Автор закрывает перечисленные исходные поля до передачи задания; исполнитель записывает только фактические наблюдения назначенного выполнения. Конкретная недоступная зависимость фиксируется точно, без поиска другого опыта. Фаза определяется последним поручением пользователя, а не датой карточки.

**Навыки внутри тела:** следующие hashes и адресные требования переиспользованы из проверенного паспорта04/18; это не утверждение нового полного чтения восьми файлов для каждой P-карточки.

| Навык, SHA256 и диапазон ранее прочитанного текста | Применяемое требование |
|---|---|
| [architect-behavior-classifier-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md), `e876e0ecae57aeda56cec054deedd8c2c6e8e40eb3bed6ed32e92ce86473f507`, 281–295 | «human clarity, readable architecture» — назвать реального learner и границу доказательства |
| [planning-session-startup-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/planning-session-startup-SKILL.md), `0aa3da46367b362f7bbc1a81254dbccc5e9b4ea91aed9f1cb4234ce0a573d802`, 1615–1636 | «Who executes the work and what is in scope?» — роли, разрешённые файлы и текущая фаза записаны до запуска |
| [plan-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/plan-review-SKILL.md), `792d4bb9621154dbfb7c7dd6220e7ebd64f206e01234f7b58f32d38b83925358`, 2115–2143 | «exact source/prompt/file lookup presented as generalization» — не выдавать lookup или совпадение учебного входа за новый ответ Student |
| [generated-code-review-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/generated-code-review-SKILL.md), `1b32d8a121fad463cdde6532cb86baa52974b6560f8a20b545f163b65e992a52`, 762–786 | «review must first verify the work-order boundary» — проверка source не расширяет поручение до реализации |
| [output-readability-gate-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/output-readability-gate-SKILL.md), `493e15347e7e28ad7780ad4aee088271107c64b403deb396db256846bd660a2d`, 197–218 | «Что ожидалось и что фактически проверено» — ожидаемые и actual поля раздельны |
| [human-readable-architecture-audit-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/human-readable-architecture-audit-SKILL.md), `a08e8f79995929fafa80f32c858329ffeb11a9080caa7b89b63fe50c4a5f6a7f`, 1–55 | «различать предметную тему обучения (домен), конкретную задачу и направление» — предметная тема берётся из выбранного D-профиля; механизм указан отдельно |
| [human-communication-GLOSSARY.md](C:/Projects/llm_mvp_core/docs/skills/human-communication-GLOSSARY.md), `39c3e64aa78733f542be3db562c0845d8c6e47ebe11b32127baa4fd4ca49d4eb`, 5–22 | «Домен — предметная тема, по которой обучается единица.» — не называть optimizer или hash предметным доменом |
| [prompt-patch-authoring-SKILL.md](C:/Projects/llm_mvp_core/docs/skills/prompt-patch-authoring-SKILL.md), `e96526474e658664d1594dd3d72a7a0fa87c006c045eeb498774bb3c2d475409`, 1–95 | «A patch targets exactly one bottleneck per cycle.» — один выбранный фактор; изменение всего pipeline не локализует причину |

**Видимое поручение до исполнения:** выполнять только конкретный подслучай назначенной строки [единого индекса](00-execution-index.md), с заранее заполненным автором рецептом: предмет, Student, исходное состояние, данные, host, команда и бюджет. Модель исполнителя — его текущая по умолчанию без override. Исполнитель не выбирает другой подслучай и не исследует, как заполнить пропуски; автор закрывает их до передачи задания. Исторические бюджеты и весь набор подслучаев автоматически не запускать. При отсутствии доказанной учебной связи автор уточняет source→state→consumer, а проверку совсем вне обучения не назначает. Дополнительные уроки для формального допуска не придумывать.

**Правило успеха каждого задания (Люба, 19.10.2026):** любое фактически измеренное полезное улучшение — успех по этому показателю и предметной теме: скорость обучения/ответа, CPU, RAM/VRAM, усилия, число уроков/обновлений, объём состояния, слои модели, Unit/Flow, точность и иные полезные величины. Рост точности не обязателен. Остальные изменения и ухудшения показываются рядом; возможны одновременно успех RAM и ухудшение точности. Локальный измеренный эффект признаётся локальным успехом с границами, без обязательной серии повторных запусков. Для сравнения нужны одинаковая работа и сопоставимый измеритель. Исправление измерителя — успех достоверности; смена подписи/единиц не равна физическому ускорению. Это не допуск к публикации и не утверждение об иной неизмеренной способности. Паспорт сохраняет условия опыта; разрешение на уже порученную программу действует без повторного личного согласования каждой версии. Конкретные недостающие входы указываются отдельно.

**Участие в обучении — обязательная граница задания:** перед выполнением названы настоящий обучаемый получатель, запись урока/изменённого состояния и чтение этого состояния в процессе обучения; сопоставимы исходное состояние, уроки и экзамен, кроме одного выбранного фактора. Измеряется реальное обучение нейронных весов либо обучаемой памяти Unit. Допустимы также участки до и после update, если от них зависит обучение или использование выученного состояния: загрузка, подготовка пар, восстановление, выдача результата. Для каждого указать файл/метод оригинального кода, цепочку зависимости, что сломано и как исправить. Отсутствие нового update в таком опыте само по себе не исключает его; лишние уроки ради галочки не нужны. OUT_OF_LEARNING_SCOPE относится только к проверкам, совсем не связанным с обучением и его результатом.

18 октября 2026. **READ_ONLY_RESEARCH; проект, не выполнено.**

## Механизм и общий контроль

**Получатель изменения:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen.

**Механизм:** конкретное вмешательство A/B описано по самостоятельным подслучаям ниже. Это поле отделено от предметной темы обучения.

Оба плеча используют один frozen corpus и независимые состояния из описанного старта, одну панель и scorer. На один A/B выбирается ровно один подслучай ниже. Все остальные изменения запрещены данным сравнением. Положительный результат на любой оси сохраняется вместе с tradeoffs; числа пилотов старых источников не являются квотой успеха.

<!-- BEGIN book-math-2026-10-22:p54-composition -->
<a id="book-math-p54-composition"></a>
### Почему композиция позволяет использовать урок в новой фразе

**Пример.** Из validate the file → проверь файл и delete the cache → удали кэш можно собрать delete the file → удали файл, когда переводы действия и объекта выделены верно и правило их соединения уже задано. Полезный механизм этой карточки — сохранение такой композиции при чтении обученной памяти.

**Связь с книгой.** У G. Cirrincione, *Mathematical Theory of the Transformer*, Vol. I, 15.05.2026, формулы (2.1)–(2.3), печатные с. 32–33 / PDF с. 68–69, вычисляют все парные оценки через общие компоненты:

$$
QK^\top=(X_{\mathrm{pos}}W_Q)(X_{\mathrm{pos}}W_K)^\top.
$$

[Источник книги](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/math-theory/giansalvo-transformer-vol1/SOURCE.md). Для нашей памяти аналог повторного использования частей записывается иначе:

$$
F(a,o)=C(A(a),O(o)).
$$

$A$ переводит действие, $O$ — объект, $C$ соединяет результат. При независимых переводах частей и известном $C$, $n$ действий и $m$ объектов требуют $n+m$ компонентов вместо до $nm$ независимых целых сочетаний. Это условный подсчёт компонентов и наша иллюстрация, не теорема книги об обучении phrase-map. Если нужны новые падежные формы, отрицание, вопрос или идиома, простая разделимость может не выполняться.

**Измеренный пример.** В [P125.1](P125-executor-memory-learning.md#book-math-p125-1-composition) восемь уроков с явными PhraseMappings дали 8/8 правильных новых сочетаний известных действий и объектов; вся панель — 19/24, края — 0/5, три ошибочных ответа приняты. Это подтверждает местное повторное использование частей обычным Flow и показывает его границу. Само разбиение уроков было задано; минимум уроков и сравнительное время до нужного качества не измерялись. Результат P125 не является исполнением сравнений P54.1–P54.7.
<!-- END book-math-2026-10-22:p54-composition -->

<!-- BEGIN book-math-pass2-2026-10-22:p54-md-source -->
<a id="book-math-p54-md-source"></a>
**Прямой MD-первоисточник для общей факторизации:** [формула (2.1)](https://github.com/lubochka/xiigen-planning/blob/6e062c9709fce91bc3065949b14ed87fc961779f/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0064-0075-ch02-single-head-and-scores.md#equation-2-1), [определение $M$, (2.2)](https://github.com/lubochka/xiigen-planning/blob/6e062c9709fce91bc3065949b14ed87fc961779f/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0064-0075-ch02-single-head-and-scores.md#equation-2-2), [масштабированная оценка, (2.3)](https://github.com/lubochka/xiigen-planning/blob/6e062c9709fce91bc3065949b14ed87fc961779f/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0064-0075-ch02-single-head-and-scores.md#equation-2-3), печатные с. 32–33 / PDF с. 68–69. Это источник формул attention; условная композиционная модель памяти выше остаётся отдельно обозначенной иллюстрацией.
<!-- END book-math-pass2-2026-10-22:p54-md-source -->

## Самостоятельные сравнения

<a id="s-1"></a>
### P54.1 — Greedy phrase segmentation

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

A — greedy leftmost phrase; B — для входа не длиннее 8 content tokens перебрать non-overlapping segmentations и выбрать max mandatory-token coverage, затем минимум spans, затем ordinal phrase ID. Это конкретный bounded proposed selector; длина>8 остаётся прежним путём и отдельной группой. close user / user session overlap, same map, exact-source контроль; считать дополнительную стоимость.

<a id="s-2"></a>
### P54.2 — Filtered ordinal против original index

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

A — индекс i от отфильтрованных nonpunctuation tokens используется в исходном списке; B — каждый token несёт originalIndex. Leading/interposed/trailing dash и no-punctuation контроль. Карта phrase spans должна ссылаться на те же позиции; сами переводы не меняются.

<a id="s-3"></a>
### P54.3 — Unknown обязательное слово не исчезает

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

A — неизвестный обязательный token пропускается; B — явный incomplete/abstain при неполном mandatory coverage. not, actor name, delete vs archive — критические различия; пунктуация optional. Correct/answered и answered/all вместе, отказ не называется новым переводом.

<a id="s-4"></a>
### P54.4 — Удалённый разделитель создаёт ложное соседство

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

A — punctuation-only token удаляется до phrase matching; B — сохраняется boundary marker, через который multiword phrase match запрещён. Controlled tokens [close, ->, session] не равны соседней фразе [close, session]; no-separator control должен остаться прежним. Semantics разделителя задаётся во входном контракте; обычная запятая не считается универсальной границей без языковой рубрики. Word fallback и ordinal .2 неизменны.

<a id="s-5"></a>
### P54.5 — Word-map omission отдельно от phrase composition

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

A — TryComposeFromWordMappings пропускает unknown при mapped>=2 и ratio>=.50; B — тот же составитель возвращает incomplete и продолжает прежний fallback, если пропущен mandatory token из заранее разрешённого входного task contract. Controlled do not open files: memory только open→открывать/files→файлы, not mandatory; никаких phrase/exact mappings. Сравнить raw composition, actual route и final output. Fully-covered input контроль; all-unknown край. Если mandatory mask supplied вручную, доказан controlled component, не automatic understanding; реальный parser/contract отдельно подтверждается. Abstain/error reduction вместе с coverage, не новый перевод.

<a id="s-6"></a>
### P54.6 — Полная композиция и независимый law rerank

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

При frozen candidates, перечисленных bounded composer .1, A выбирает coverage/spans/ordinal tie. B сначала исключает кандидаты, нарушающие доступный из runtime task contract typed action-object relation, затем использует тот же A rank. Для close user session контракт операции CloseSession(userId) требует object=session, possessor=user; close+user session конкурирует с close user+session при одинаковом покрытии 3/3. Нельзя читать TEST expected для построения этого relation. Если runtime binding отсутствует, только controlled-input компонент, ordinary gain CONDITIONAL. Контроль единственная законная сегментация; edge две законные оставляют tie прежним. Сравнение генерации альтернатив (.1) и их semantic выбора (.6) раздельно; reversed lesson order по supplied proxy не меняет leftmost результат.

<a id="s-7"></a>
### P54.7 — Word-map прерывает поиск до проверки известного противоречия

**Действующая учебная применимость:** learned reader связан с обучением; обязательный новый writer/update не требуется. Статус этого подслучая — NEEDS_INPUT_FOR_RECIPE по точному списку в действующем уточнении выше. Старый нулевой бюджет как OUT_OF_LEARNING_SCOPE отменён; старые панели ниже не являются разрешёнными командами. Исполнитель не заполняет missing snapshot/contract/host сам.

A — обычный Executor возвращает прошедшую word-map composition до kNN. B — только перед этим return проверяет candidate на противоречие с доступным runtime законом; при CONTRADICTION продолжает неизменный kNN путь, при COMPATIBLE или UNKNOWN сохраняет исходное поведение. Конкретный controlled контракт содержит actor/action/recipient и polarity текущего source; candidate checker получает их из текста/разрешённого task contract, не TEST gold. Пример Anna gave Mary the key / Mary gave Anna the key: ранний map переставил роли, а frozen kNN содержит допустимый перевод; память и все rankings одинаковы. Контроли: верный map не должен обходиться, оба кандидата неверны не должны превращаться в успех. Отдельный tracing-only режим перечисляет kNN candidates, не меняя output. Automatic parser/checker и достижимость такого обычного caller не установлены: controlled component возможен, автоматическая предметная польза CONDITIONAL. Это предложенная candidate-selection policy, не доказанная существующая функция и не neural update.

## Прямые источники

- [30-SOURCE-LOGIC-PROXY.mjs:65](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-SOURCE-LOGIC-PROXY.mjs:65>) — Прямой источник соответствующей причины; подробный тезис и предел фиксируются в ledger.
- [33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:7](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:7>) — Прямой источник соответствующей причины; подробный тезис и предел фиксируются в ledger.
- [21-MICRO-PHRASE-CLAUSE-SURVIVAL-EXPERIMENT-2026-10-16.md:22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-MICRO-PHRASE-CLAUSE-SURVIVAL-EXPERIMENT-2026-10-16.md:22>) — Прямой источник соответствующей причины; подробный тезис и предел фиксируются в ledger.
- [21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md:30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md:30>) — Word-map omission отдельно от phrase composition.
- [30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md:18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md:18>) — Полная композиция и независимый law rerank.
- [37-MICRO-MULTIDOMAIN-NEURAL-STRUCTURE-KNN-AND-HISTORY-EXAM-2026-10-16.md:27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-MULTIDOMAIN-NEURAL-STRUCTURE-KNN-AND-HISTORY-EXAM-2026-10-16.md:27>) — Word-map прерывает поиск до проверки известного противоречия.

Это утверждения предоставленных исследований, а не свежий code audit. Все связанные контекстные упоминания находятся в [реестре содержания](../04-source-claim-ledger.md); JSON/script — предоставленные fixtures/proxy, не наши runs. [Исходная постановка](../../83-check-catalog-and-prompts.md) не заменяет эти прямые основания.

## Обязательная запись M для каждого рецепта

Каждый D-рецепт создаёт отдельную **M(D, подслучай)** до/после A и B: sec/update, tokens/s, time-to-quality либо updates0/n/a + operation latency; предметная точность и critical errors; RAM всей группы/VRAM/index/checkpoint/parameters; **model layers total/trainable**, отдельно **Unit/Flow levels и executed depth** (unchanged/n/a с причиной). Считать также calls, retention/recovery, sample/manual effort по механизму. Правило пользы изменения структуры записать заранее. Единицы, формулы, fictional examples и полный формат — [03](../03-metrics-and-reporting.md).

<a id="d04"></a>
## D04 — профиль проверки: EN→RU

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** EN→RU.

**Вход/gold:** Run now→«Сейчас запускай»: run не соответствует первому сейчас. Дополнительно controlled token sequence с перекрывающимися phrase spans [close, user] и [user, session] (переведённые spans заранее заданы для этого направления); выбравший span не должен потерять обязательный session или not. Перенос этой fixture в natural source требует реального parser.

**Применимые подслучаи:** P54.1; P54.2; P54.3; P54.4; P54.5; P54.6; P54.7.

**Этот механизм:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen. Для EN→RU нужен обычный direction-specific caller именно этого owner; source code общих классов не доказывает одинаковое достижение каждой ветви. Caller trace отделяет этот memory путь от RP01 neural Student. Проверяемые поля/состояние и результат настоящие; controlled input разрешён только как обозначенный компонентный уровень.

**M(D04):** сохранённые роль, отрицание, контекст и направление; correct/all, correct/answered, answered/all, wrong accepted/all; отдельно выбранный lesson/rule ID; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевое изменение и прежний правильный урок — отрицательные контроли; unknown token/context, conflicting correction и непокрытый обязательный закон — края. Конкретный A/B edge каждого подслучая указан выше.

<a id="d05"></a>
## D05 — профиль проверки: RU→EN

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** RU→EN.

**Вход/gold:** «Сейчас запускай»→Run now: сейчас не соответствует первому Run. Дополнительно controlled token sequence с перекрывающимися phrase spans [close, user] и [user, session] (переведённые spans заранее заданы для этого направления); выбравший span не должен потерять обязательный session или not. Перенос этой fixture в natural source требует реального parser.

**Применимые подслучаи:** P54.1; P54.2; P54.3; P54.4; P54.5; P54.6; P54.7.

**Этот механизм:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen. Для RU→EN нужен обычный direction-specific caller именно этого owner; source code общих классов не доказывает одинаковое достижение каждой ветви. Caller trace отделяет этот memory путь от RP01 neural Student. Проверяемые поля/состояние и результат настоящие; controlled input разрешён только как обозначенный компонентный уровень.

**M(D05):** сохранённые роль, отрицание, контекст и направление; correct/all, correct/answered, answered/all, wrong accepted/all; отдельно выбранный lesson/rule ID; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевое изменение и прежний правильный урок — отрицательные контроли; unknown token/context, conflicting correction и непокрытый обязательный закон — края. Конкретный A/B edge каждого подслучая указан выше.

<a id="d11a"></a>
## D11a — профиль проверки: HE→EN, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** HE→EN, условный профиль.

**Вход/gold:** עכשיו הפעל→Run now: עכשיו означает now, а не Run. Дополнительно controlled token sequence с перекрывающимися phrase spans [close, user] и [user, session] (переведённые spans заранее заданы для этого направления); выбравший span не должен потерять обязательный session или not. Перенос этой fixture в natural source требует реального parser.

**Применимые подслучаи:** P54.1; P54.2; P54.3; P54.4; P54.5.

**Этот механизм:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen. Для HE→EN, условно нужен обычный direction-specific caller именно этого owner; source code общих классов не доказывает одинаковое достижение каждой ветви. Продуктовая стадия условна до подтверждения route, material и независимого языкового gold. Проверяемые поля/состояние и результат настоящие; controlled input разрешён только как обозначенный компонентный уровень.

**M(D11a):** сохранённые роль, отрицание, контекст и направление; correct/all, correct/answered, answered/all, wrong accepted/all; отдельно выбранный lesson/rule ID; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевое изменение и прежний правильный урок — отрицательные контроли; unknown token/context, conflicting correction и непокрытый обязательный закон — края. Конкретный A/B edge каждого подслучая указан выше.

<a id="d11b"></a>
## D11b — профиль проверки: EN→HE, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** EN→HE, условный профиль.

**Вход/gold:** Run now→עכשיו הפעל: Run соответствует הפעל, не עכשיו. Дополнительно controlled token sequence с перекрывающимися phrase spans [close, user] и [user, session] (переведённые spans заранее заданы для этого направления); выбравший span не должен потерять обязательный session или not. Перенос этой fixture в natural source требует реального parser.

**Применимые подслучаи:** P54.1; P54.2; P54.3; P54.4; P54.5.

**Этот механизм:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen. Для EN→HE, условно нужен обычный direction-specific caller именно этого owner; source code общих классов не доказывает одинаковое достижение каждой ветви. Продуктовая стадия условна до подтверждения route, material и независимого языкового gold. Проверяемые поля/состояние и результат настоящие; controlled input разрешён только как обозначенный компонентный уровень.

**M(D11b):** сохранённые роль, отрицание, контекст и направление; correct/all, correct/answered, answered/all, wrong accepted/all; отдельно выбранный lesson/rule ID; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевое изменение и прежний правильный урок — отрицательные контроли; unknown token/context, conflicting correction и непокрытый обязательный закон — края. Конкретный A/B edge каждого подслучая указан выше.

<a id="d12a"></a>
## D12a — профиль проверки: ES→EN, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** ES→EN, условный профиль.

**Вход/gold:** Ahora ejecuta→Run now: Ahora соответствует now, не Run. Дополнительно controlled token sequence с перекрывающимися phrase spans [close, user] и [user, session] (переведённые spans заранее заданы для этого направления); выбравший span не должен потерять обязательный session или not. Перенос этой fixture в natural source требует реального parser.

**Применимые подслучаи:** P54.1; P54.2; P54.3; P54.4; P54.5.

**Этот механизм:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen. Для ES→EN, условно нужен обычный direction-specific caller именно этого owner; source code общих классов не доказывает одинаковое достижение каждой ветви. Продуктовая стадия условна до подтверждения route, material и независимого языкового gold. Проверяемые поля/состояние и результат настоящие; controlled input разрешён только как обозначенный компонентный уровень.

**M(D12a):** сохранённые роль, отрицание, контекст и направление; correct/all, correct/answered, answered/all, wrong accepted/all; отдельно выбранный lesson/rule ID; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевое изменение и прежний правильный урок — отрицательные контроли; unknown token/context, conflicting correction и непокрытый обязательный закон — края. Конкретный A/B edge каждого подслучая указан выше.

<a id="d12b"></a>
## D12b — профиль проверки: EN→ES, условно

**Тема обучения (домен):** Перевод.

**Задача / язык / направление:** EN→ES, условный профиль.

**Вход/gold:** Run now→Ahora ejecuta: Run соответствует ejecuta, не Ahora. Дополнительно controlled token sequence с перекрывающимися phrase spans [close, user] и [user, session] (переведённые spans заранее заданы для этого направления); выбравший span не должен потерять обязательный session или not. Перенос этой fixture в natural source требует реального parser.

**Применимые подслучаи:** P54.1; P54.2; P54.3; P54.4; P54.5.

**Этот механизм:** ExecutorUnit phrase/word composition, source token indices и mandatory coverage; learned maps, tokenizer и verifier frozen. Для EN→ES, условно нужен обычный direction-specific caller именно этого owner; source code общих классов не доказывает одинаковое достижение каждой ветви. Продуктовая стадия условна до подтверждения route, material и независимого языкового gold. Проверяемые поля/состояние и результат настоящие; controlled input разрешён только как обозначенный компонентный уровень.

**M(D12b):** сохранённые роль, отрицание, контекст и направление; correct/all, correct/answered, answered/all, wrong accepted/all; отдельно выбранный lesson/rule ID; механический observable — выбранный выше state/vector/gradient и собственный output. Контроль/edge: Нулевое изменение и прежний правильный урок — отрицательные контроли; unknown token/context, conflicting correction и непокрытый обязательный закон — края. Конкретный A/B edge каждого подслучая указан выше.

## Не включённые ветви

D10, D13, D14, D01, D02, D03, D06, D07, D08, D09: Эти сравнения относятся к named TranslationMachine/Units и их представлению языкового материала. Другой neural trainer, C#/SQL AST или layout consumer не объявляется тем же получателем. Перенос через иной producer/owner — самостоятельная неподтверждённая зависимость, а не выполненная проверка этой темы.

## Исторический промпт и первоначально запланированные свидетельства

Этот раздел сохраняет первоначальную постановку. Его команды не назначают повтор записанного опыта. Для нынешнего задания использовать текущий вход карточки и собственную Q-строку [индекса](00-execution-index.md); научный результат, ремонт артефакта и приёмка переноса — разные виды работы.

> Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.
>
> Обязательная часть этого промпта: сам найди launch/wait/status в каталоге tools, прочти их схемы и skills/references контракта31; выдай entry один раз и сохраняй handle. Живую silent/running операцию жди штатным bounded wait/resume того же ID до terminal exit/deadline; два быстрых пустых status требуют перейти в wait, а не прекратить нормальное ожидание. Если wait неисправен, используй независимые H/R/claim/exit и PID+start. Yield/timeout вызова не равен exit процесса. Не спрашивай, продолжать ли ждать, не перекладывай запуск на Любу, не выдавай entry повторно и не выбирай results-NN. Отсутствие log не доказывает отсутствия запуска. Проверять реальное обучение назначенного механизма и пригодность before; 0→0, отсутствие обучения и неполное A/B оставляют учебную цель OPEN, один отчёт не завершает её. Поздний сбой не обнуляет updates; неизвестное — null. Исправленный рецепт назначает автор, без поиска исполнителем и без steps до PASS. Сохранить raw; любой полезный выигрыш считать успехом своей метрики только при валидном учебном сравнении, с регрессиями рядом. STOP/NEXT Любы немедленны. Полные правила: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/30-failure-recovery-and-learning-validity.md. Tool/wait reference: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md.
>
> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.

«Самодостаточный вход: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md (§1: AGENTS/CLAUDE и полный стек навыков); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md (§2: роли); C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md (применимость и падения). Только прямо назначенный пользователем ID/этап; документальное поручение не разрешает запуск. Полная карточка C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md обязательна. Модель сессии по умолчанию, без override. Только назначенная строка единого индекса и её готовые host/данные/команды; отсутствующее возвращай автору, не проектируй и не ищи замену. Документное поручение не разрешает runtime. По «заканчивай и переходи» немедленно NEXT без оформления/review/retry/Git прошлого; по «остановить всё» прекрати всю работу, сохрани raw/partial. Отчёт по назначенному пути на русском; полезный измеренный эффект — успех своей оси с ухудшениями рядом. Выполняй только прямо назначенную строку единого индекса и её конкретный рецепт; эта справочная P-карточка сама запуск не назначает. Перед выполнением зафиксируй P54, один указанный подслучай и один профиль проверки (D-секцию). Используй только названный механизм и его A/B; сохрани исходные state/data/tokenizer hashes, одинаковую панель и реальные сырые выходы. Сначала покажи различающее условие, отрицательный контроль и edge, затем предметный результат. Заполни M отдельно для этого профиля проверки, включая model total/trainable layers и Unit/Flow levels до/после. Покажи любой полезный эффект и цену. Если caller/schema/gold отсутствует, назови конкретную зависимость. Не подставляй эталон вместо результата и не расширяй разрешённую программу. Любое фактически измеренное полезное улучшение — успех по этому показателю и домену; рост точности не обязателен. Все остальные изменения и ухудшения покажи рядом; локальный эффект признаётся локальным успехом без обязательных повторных прогонов. Исправление измерителя не называй физическим ускорением. Перед исполнением покажи связь проверяемого участка с обучением или использованием его результата, включая подготовку входов и восстановление. Укажи точный файл/метод оригинального кода, цепочку вызовов, неисправность и способ её исправления. Для нового обучения измеряй реальные updates; для зависимого участка допустим один доказанно обученный state и одинаковые входы до/после исправления без лишнего Train. Исключается только проверка, совсем не связанная с этой цепью. Внеси фактические наблюдения в уже назначенный паспорт. Недостающие проект, входы, метод или команду возвращай автору карточки; не ищи замену и не меняй опыт. При поручении только на проектирование подготовь карточку; при поручении на выполнение продолжай без повторного вопроса об ID, пути или SHA. Неустранимый недостающий вход назови точно.».

Формула/модель алгоритма — **симуляция**, её время не является native cost. Искусственные входы допустимы для настоящего компонента, но его измеряемые state/output должны быть реальными. Обучение памяти и neural update отражаются раздельно. Будущий пакет: raw cases и gold, state deltas, actual caller trace, measurements M и список gained/lost cases; на момент первоначального проектирования эти артефакты ещё не были получены; текущий записанный результат указан в Q-строке индекса, а доступность и пригодность конкретных файлов проверяются отдельно.
