# 48. Читать выученное тело только внутри уже выбранного маршрута

<!-- ADDRESSED-TRANSFER-QA-20261112 -->
**Назначенный ремонт R05: что проверять и как продолжать.** Нужен обычный вызов с собственным подтверждающим примером A, чужим B, обратным порядком и загрузкой в новом экземпляре. Ограничение тела по FlowRef не закрывает confidence: глобальный поиск подтверждения и поиск тела в выбранном маршруте — разные границы. Сборка, обнаружение и запуск тестов, обоснованный ремонт подготовки входят в назначенную QA. При занятой общей сборке [дождаться её владельца и продолжить](audits/2026-11-12/executor-context-and-index-repair.md#resource-wait-20261112), без второго исполнителя и общего запрета. Исходники и DLL фиксируются до сравнения; новая цель или дополнительные учебные вызовы требуют отдельного решения. Будущий бюджет 2 партии / 17 Generate / 2 Save / 5 Load остаётся предложением, а не выполненным опытом. Замена глобального поиска поиском в выбранном маршруте должна различаться отрицательным случаем обычного пути; положительные A/B и прежняя приёмка тела сохраняются.

<!-- EXECUTOR-CONTEXT-20261112-BB1 -->
**Исполнителю, 12 ноября — [CTX12 и действующий R05](audits/2026-11-12/executor-context-and-index-repair.md).** Старую приёмку фильтра тела сохранить. Исправляемый шаг — происхождение confidence witness: чужой FLOW-02 не подтверждает выбранный FLOW-01. Проверять actual candidate/witness/body через GeneratePlanAsync, а не только helper; 29/30 — формула, не вероятность правильности. Baseline S-A/S-AB записать до ожидаемого red assertion; после Load Train=0. Same-flow не закрывает active revision. Бюджет2 batches/17 Generate/2 Save/5 Load — предложение R05; дополнительные классы считаются отдельно.

<!-- XIIGEN-POST-ADMISSION-20261112-WITNESS:START -->
## Свежая сверка 12 ноября: тело уже ограничено маршрутом, свидетельство ещё нет

На core `f089a7d55d359ff103d0cdd9ddfb26eee346a16b` принятый FIX-IM остаётся подключён: `FindLearnedPair` передаёт `grounding.FlowRef` в поиск тела. Незавершённый B12-CW — другое место: `TryGround` всё ещё вызывает двухаргументный `FindNearestWithScore` без выбранного FlowRef. Поздняя приёмка фразового поиска A07 этот исходник не исправляет и не закрывает B12-CW.

Независимый пересчёт на свежих blob подтверждает уже известный контрпример: `beta FLOW-01` при уроке A/FLOW-01/alpha имеет своё сходство 0; добавление B/FLOW-02/beta сохраняет выбранный A, но чужой сосед даёт Jaccard 1/3 и confidence 29/30. Обратный порядок двух уроков результата не меняет. Оба собственных положительных запроса сохраняются. Это арифметическое подтверждение пути, **native UNRUN**, а не измеренная точность. [Свежие источники и расчёт](audits/2026-11-12/post-admission/codex-product-review.md).

При реализации сохранить три отдельных наблюдения: выбранный FlowRef; PairId/FlowRef/score подтверждающего соседа; PairId/FlowRef/полный body. Верный body A сам по себе не доказывает законность допуска A. Отказ при нулевом собственном свидетеле должен достигать ordinary `GeneratePlanAsync` и прежнего fallback; нельзя заменять его одной проверкой отфильтрованного store. После fresh load те же три наблюдения относятся к загруженному состоянию: статическое чтение `ExportCheckpoint→ImportCheckpoint→ImportState` указывает на сохранение пар/весов, но не считается выполненной проверкой восстановления. Ранее назначенные 2 batches / 17 Generate / 2 Save / 5 Load не расширяются этой записью; диагностические наблюдения входят в уже назначенные стадии.

Отдельный договор активной редакции остаётся в карточке15. Same FlowRef не доказывает принадлежность witness/body активной revision; текущий узкий ремонт нельзя закрывать за счёт удаления исторических пар или подмены требования общей уверенностью. Обратная мутация scoped→global должна краснеть на прежнем отрицательном запросе при сохранённых положительных контролях и неизменном oracle.
<!-- XIIGEN-POST-ADMISSION-20261112-WITNESS:END -->


<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-BODY48:START -->
## Активная редакция внутри выбранного маршрута — уточнение 11 ноября 2026

Прежний FIX-IM фильтр по FlowRef существует и сохраняется. Остаток F08/B11: один FlowRef может иметь несовместимые редакции chosen. Current FindLearnedPair не ограничивает body активной редакцией; ordinal PairId не задаёт порядок принятия целей.

Будущий ремонт карточки15 вводит **предлагаемую** active revision. Приёмка проверяет A→B1→B2: body именно B, B1/B2 удержаны, историческая A сохранена, но не выбирает исполняемую цель. Отдельные случаи: старый PairId раньше нового; переименование IDs без смены содержания; wrong revision после restore; missing active body. Контрольный C неизменен. Expected полного body не получается через candidate SerializeChosen.

PairId обозначает immutable canonical payload. Идентичный повтор имеет явный идемпотентный исход; тот же ID с другим содержанием отвергается **до** weights/history/revision mutation. Приёмка повторного ID A с телом B закрывает опасность связать новые weights со старой KNN-парой. Это граница проектируемого ремонта; новый native дефект ordinary caller этим не объявлен.

Import сначала проверяет весь candidate: уникальные IDs, существование members, совместимость FlowRef/mode/target каждого member с revision, веса и schema version. Лишь затем атомарно заменяет состояние. Повреждённый/неоднозначный snapshot оставляет прежние weights/pairs/revisions целыми; latest не угадывается по JSON order/PairId/filemtime.

Пустой/нулевой сосед — отдельный B12, не повод ослаблять confidence или снять FlowRef filter. Mode2 exact-body guard FIX-P118 сохраняется. [План §3.2](audits/2026-11-11/repair-plan.md) задаёт будущую дозу; прежние 32 Generate + 8 lookups не покрывают автоматически новые Train и мутации.
<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-BODY48:END -->


**Будущее покрытие данными, 4 ноября:** [158](codex-158-learning-sets-transfer-coverage-2026-11-04.md) использует контраст «объясни C#»/«напиши C#» и атомарный M004 только после подготовки отдельной панели. Метка правильного маршрута с чужим телом остаётся FAIL; native FindLearnedPair уже ограничен FlowRef, его заново не переписывать. Текущий остаток 48 — полное тело/заголовок и restore; исходный опыт не повторять. M004 содержит labels, поэтому сам по себе не доказывает полный body/dispatch; это проверяется интеграцией 20.

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски FLOW/STATE:** Правильная строка Unit/FlowRef не заменяет исполненный результат: typed refs→вызванные nodes, learned edge→Retry/Stop, graph order→sink. Положительный контроль использует обученный payload, отрицательный выявляет его потерю/чужое тело; trivial baseline и exact recall показаны отдельно. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Личность выбранного маршрута должна совпасть с источником его тела.** Наблюдать logical lesson ID, rank, route ID, ordered steps, payload source и реально выполненные шаги. В P104 E2/E4 body брался из trained_corpus_row: это полезный экспериментальный переходник, но ordinary API непервого соседа им не доказан.

Приёмка требует второго законного кандидата, который отличается телом от первого. Верный ID с подставленным rank1 payload должен упасть; два неправильных шага при прежней длине также. Дедуп по lesson identity и фильтрация по схеме идут до соответствующего cap, каждый имеет собственный достижимый witness. Порча меняет данные, evaluator неизменен.

Для first-hit/majority сохранить disagreement count. P1070/12 означает отсутствие различающего случая на панели, а не доказанную пользу majority. Синтетическая фикстура dedup проверяет одну функцию, не acquisition. После save restore доказать новым объектом и ordinary flow; same-live read не повышает статус.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Проверять один фактически исполненный путь: prompt → обученный выбор → выбранные model/profile → actual результат получателя → адрес записи нового урока → чтение после restore. Подстановка сохранённых logits/ответа вместо результата выбранной модели не доказывает этот путь: именно так ограничен новый P08 product test. Исторический oracle читается только для сравнения, а не подаётся как ответ следующему звену.

Для FIX-IM исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Доказывать вызов реального получателя после выбора, а не наличие созданного constructor/bridge. Контрпример P08: A/B TextGenerator создан, но sampled ответы получены вне него. Для этой карточки записать выбранную модель/профиль, реально вызванный метод потребителя, данные ответа и получателя нового урока; альтернативный маршрут и restore проверяются отдельно. Подмена ответа ближайшей строкой/таблицей gold не проходит E2E даже при правильном выборе домена.

Применимость: FIX-IM. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять фактическую конфигурацию модели после маршрутизации: domain, checkpoint, tokenizer, функции KNN, trainable recipient и параметры обучения. Контрпример P12: direct Trainer Warmup4 отказывает, но фабрика IncrementalTrainingSession уже задаёт Warmup0; дефект одного вызова не доказывает отказ всего маршрута. После урока проверять сохранение выбранной и неизменность чужих моделей; hash без полного snapshot не является restore.

Применимость: FIX-IM. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-IM

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** FLOW/STATE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**K — владение допущенным состоянием и восстановление.** Показать, какой объект хранит допущенные параметры, примеры, конфигурацию или ключ кэша; интерфейс IReadOnlyList сам по себе не доказывает неизменность. Контроль после создания/допуска: изменение исходного caller-owned массива или внешнего представления не меняет захваченную запись незаметно. Законное обновление/смена model identity вызывает предусмотренное обновление или инвалидирование кэша, а не запрет обучения. Если reader сохраняется/восстанавливается, сравнить поведение обычного consumer по сохранённым данным и настройкам: reconstruction с default не является эквивалентным restore opt-in режима. Ошибка из KDP: catalogue был проверен, затем изменён через возвращённый массив; schema не содержала catalogue. Для этой карточки проверять её собственный carrier, не добавлять чужой KNN-механизм ради теста. Если изменяемого carrier/сохранения нет, отметить это явно в evidence и не выдумывать лишний runtime.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Приоритетная поправка после R4:** [FU-03/67](67-transfer-repair-followup-audit-2026-10-27.md), [U3/68](68-transfer-repair-followup-plan-2026-10-27.md). After-restore Fact проверяет FlowRef/чужие IDs, но не actual chosen.PlanText. Composer печатает FlowRef независимо от body. Поэтому правильный ID+пустое тело, правильный ID+чужое тело и правильный ID+старое тело того же маршрута — три обязательных ошибочных outputs для scorer. Проверить полные literal тела10-a/00-b из §4 после Generate/restore/orchestrator; expected не получать candidate SerializeChosen. Snapshot/metadata реального recipient сравнивать отдельно, не стирать provenance при нормализации. Исходный budget32Generate+8lookups/0новых Train относится к переносу уже сохранённого обучения, а не разрешает новый необученный диагностический опыт. Правило обязательного обучения новой экспериментальной программы задаётся её карточкой, этот regression donor не переобучать ради assertions.

**Уточнение исполнения, 27 октября 2026.** Код внесён в `e2b844f5068` и вызывается OrchestrationPlanMachine. Direct FindLearnedPair проверяет фильтр, но не весь выдаваемый план. Достроить назначенный restored-state тест через GeneratePlanAsync и wrapper: выбранный FlowRef, PairId и полный текст относятся к одной паре; отсутствующий route не возвращает чужую пару как Mode1. Состояния брать из исторического payload с hashes, не создавать новую память перед каждым assertion. Внешнее выполнение специалиста этим всё ещё не доказано.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

ID: **FIX-IM**. Эксперимент: **IDENTITY-M1-01**. Дата: 22 октября 2026, UTC. Статус: **кандидат интеграции измеренного улучшения**; текущая работа документная. Следующий ID определяется только [реестром01](01-registry.md).

Это новый пробел после выбора маршрута. [FIX-RET15](15-prompt-flow-retention.md) сохраняет признаки при следующем уроке; FIX-IM не меняет обучение. [FIX-E2E20](20-prompt-domain-e2e.md) проверяет typed handoff и получателей; здесь исправляется собственный native поиск тела до handoff. Существующие опыты и планы сохранены.

## 1. Измеренный учебный результат

Тема — **оркестрация**, задача — выдать содержание выученного плана. LAB-A/LAB-B — идентификаторы двух учебных маршрутов, а не руки A/B или разные предметные темы. Память строится настоящими двумя TrainOrchestrationBatchAsync; нейронных весов/LoRA/optimizer здесь нет.

Первичное основание: [отчёт](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/report.md), [A/raw-answers.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/A/raw-answers.json), [B/raw-answers.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/B/raw-answers.json). Дата каталога20.10 не является датой измерения: внутри состояния от22.10UTC, результат опубликован23.10по местному времени. Все ссылки закреплены на проверенном core ebaf7760a36d43172bd88317ff469edfd09a615b.

| Показатель | До уроков A/B | После A | После B |
|---|---|---|---|
| Все ответы | 1/3 — верен только отрицательный контроль | 2/3 | **3/3** |
| Два положительных запроса | 0/2 | 1/2 | **2/2** |
| IQ1 выбранный маршрут / тело | Нет обученного блока | LAB-A / **LAB-B** | LAB-A / **LAB-A** |
| IQ2 прежний маршрут | Нет обученного блока | LAB-B / LAB-B | LAB-B / LAB-B |
| IQ3 отрицательный запрос | Нет утечки | Нет утечки | Нет утечки |
| Выученных пар / маршрутов | 0/0 | 2/2 | 2/2 |
| Allocations двух Train | N/A | 45 624 | 45 624 |
| Allocations IQ2 после, байт | N/A | 68 928 | **46 720** |

Одинаковый semantic hash после уроков — 5b3d68a4adb9d5b73308fcc906e80fc5ac0bf32950acac61cea5fa33de9d2a1f; raw checkpoints отличаются только ExportedAtUtc. Состояние3801байт в обеих руках. Проверены [A/states.json](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/A/states.json) с полными raw, а не одними counters, и опубликованные training receipts.

Ухудшения: постоянные+8байт на вызовах без работы фильтра; первое обучение19,6354→27,8554мс. Последний сдвиг не приписывается reader: холодные процессы/JIT, один порядок A→B. Экономия22 208байт относится IQ2 с одинаковым текстом; IQ1 экономит3120байт при разных текстах, поэтому его стоимость не столь чиста. Опыт не проверял restore, wrapper, полный host и большие corpora. IQ2 совпадает с TRAIN, IQ1 — короткая комбинация длинного урока; это не широкий независимый benchmark.

Любое измеренное улучшение полезной оси полезно именно в этом профиле. Нельзя стереть успех из-за малой панели или назвать его универсальной готовностью.

## 2. Причина, получатель и выбранная реализация

Настоящая цепочка: [OrchestrationPlanMachine.GeneratePlanAsync:99–117](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L99-L117) → PromptFlowGroundingModel.TryGround → FindLearnedPair → TrainedRouteComposerUnit.RenderLearnedFlowPlan. Последний метод печатает grounding.FlowRef в заголовке, а тело из learned.Chosen; проверять один заголовок недостаточно.

| Точный файл/метод | Сейчас | Будущая дельта |
|---|---|---|
| [OrchestrationKnnStore.FindNearest / FindNearestWithScore:34–48](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/42.ProjectWikiDomain/OrchestrationKnnStore.cs#L34-L48) | Ранжирует все пары подходящего Mode по Jaccard | Необязательная по смыслу restriction FlowRef до tokenization candidate/ranking |
| [PromptFlowGroundingModel.FindLearnedPair:69–79](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L69-L79) | Повторно ищет по всем Mode1 парам | Передать grounding.FlowRef только в последний поиск |
| TryGround того же файла | Выбирает route по выученным weights, проверяет глобальный neighbor и confidence | **Сохранить глобальный поиск без flow restriction** |
| [TrainedRouteComposerUnit.RenderLearnedFlowPlan](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L11-L38) | Формирует текст из выбранных данных | Не переписывать заголовок под чужое тело; код не меняется |
| [OrchestrationPlanOrchestrator.RunAsync](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs#L22-L86) | Вызывает machine, передаёт полный PlanText и predicted_flow_ref | Реальный consumer для проверки ниже, не новый router |

Для source и binary совместимости сохранить обе существующие двухаргументные публичные сигнатуры с прежним optional mode. Добавить отдельные трёхаргументные overload, у которых mode и flowRef обязательны при вызове:

~~~csharp
FindNearest(string prompt, PairMode? mode = null)
FindNearest(string prompt, PairMode? mode, string? flowRef)
FindNearestWithScore(string prompt, PairMode? mode = null)
FindNearestWithScore(string prompt, PairMode? mode, string? flowRef)
~~~

Старые overload делегируют с flowRef:null; новый FindNearest делегирует новому FindNearestWithScore. Единственная содержательная дельта его Where: сохранить mode-предикат и добавить
flowRef is null || string.Equals(pair.FlowRef, flowRef, StringComparison.Ordinal).
Не добавлять target к query, не менять Tokenize/Jaccard/ThenBy PairId. null означает глобальный legacy; пустая или неизвестная непустая строка — точное ограничение, не разрешение произвольного fallback. Если пары нужного route нет, вернуть null; GeneratePlanAsync сам использует прежний Mode2 fallback.

Сохранение старого overload — защитная интеграционная деталь, не измеренный отдельный фактор. Raw B имел optional3parameter; методика будущего native сравнения проверит предлагаемую ABI-compatible форму и не назовёт её уже исполненной.

Не расширять изменение до RET/max-накопления, classifier, уверенности или dedup PairId. Если RET уже принят, он присутствует одинаково A/B; snapshot этого опыта импортируется с его исходными weights.

## 3. Математика: условное извлечение и точный контрпример

Пусть $f(q)$ — route, выбранный обученными weights. Текущее извлечение $p^*=\arg\max_{p\in M}J(q,p)$ может выбрать пару с другим route. Чтобы тело соответствовало уже принятому решению, требуется
$p_f^*=\arg\max_{p\in M:\,route(p)=f(q)}J(q,p)$.
Если множество пусто, результат отсутствует. Это условие согласованности **после** обучаемого выбора, а не ручная классификация темы.

Контрпример вычислен из опубликованных двух пар и текущего BuildRuntimeEvidenceFeatures, не из нового обучения. На IQ1 входные scores LAB-A=4, LAB-B=3. Первые восемь evidence features выбранного LAB-A в Ordinal-порядке: a, account, approval, archive, audit, authorization, backup, balance. После объединения с четырьмя словами IQ1 query содержит11разных токенов. С короткой парой00-b пересечение3, объединение12, $J=3/12=0.25$. С длинной10-a пересечение10, объединение46, $J=10/46\approx0.217391$. Поэтому глобальный argmax возвращает LAB-B, хотя learned selector выбрал LAB-A. Фильтр оставляет десять-a внутри уже выбранного route; веса и confidence не меняются.

Это собственный source-derived расчёт: для токенизации использованы только ASCII-слова исходного snapshot и тот же Ordinal порядок. Он объясняет результат конкретного опыта, не доказывает оптимальность Jaccard для всех доменов.

Инвариант предполагает согласованность pair.FlowRef и pair.Chosen.FlowRef в обученных данных; обе source-пары её соблюдают. Повреждённый checkpoint с несовпадающими полями этим узким фильтром не исцеляется. В E2E20 уже предусмотрен отказ несовместимому typed handoff до вызова получателя. Не превращать patch в произвольное редактирование чужого chosen или обход допуска checkpoint.

## 4. Переиспользовать реально обученный снимок без нового fit

A/states.json содержит полный raw checkpoint для phase=empty и phase=after-lessons. SchemaVersion3, MachineId orchestration-plan-machine; source-pairs, weights, promoted/trained IDs, версия0.1.2 и progress0.102 сохранены. Он может быть восстановлен существующим публичным ImportCheckpoint/LoadCheckpointAsync. Это отличает его от старых неполных observations, для которых нужен producer.

Будущий common fixture:
Tests/Fixtures/ExperimentFixes/v1/IdentityRoute01/{empty.checkpoint.json,learned.checkpoint.json,exam.json,gold.json,manifest.json}.
Извлечь именно raw двух phases с pin выше; не достраивать недостающие поля по памяти, не переписать веса или время. Записать SHA исходного states.json и точные JSON-pointer /0/raw, /1/raw, затем SHA extracted files. Десериализовать OrchestrationPlanMachineCheckpoint через существующий OrchestrationPlanJsonOptions.Standard; все identity fields сохранить. Новый bootstrap запрещён: основная проверка имеет **0Train**.

Два исходных урока — provenance, не новое назначение:

| PairId | FlowRef | TRAIN prompt | Chosen.PlanText |
|---|---|---|---|
| 00-b | LAB-B | validate payment ledger release | Validate payment, persist the ledger, then release the receipt. |
| 10-a | LAB-A | Полные45слов из /1/raw/flow_grounding_knn[1]/prompt | Validate approval before writing the ledger; preserve the approval record. |

| EXAM | Prompt | Независимый gold |
|---|---|---|
| IQ1 | validate payment approval ledger | LAB-A + полное тело10-a |
| IQ2 | validate payment ledger release | LAB-B + полное тело00-b |
| IQ3 | schedule backups policy cron | Mode2Generalization без выученного блока |

Не сокращать45слов в сохранённой fixture: их длина является частью причины. Body/title/FlowRef никогда не подаются в UserRequest как expected values. Scorer сравнивает нормализованные whitespace тела, RouteMode и оба идентификатора; исходные outputs сохраняются до оценки.

## 5. Native regression, restore и consumer budget

Будущие файлы Tests/ExperimentFixes/IdentityRouteLearningTests.cs и IdentityRouteContractTests.cs, namespace LlmFromScratch.Tests.ExperimentFixes. Learning Fact **RestoredLearning_DeliversSelectedBodyAndConsumer** читает native assembly ProjectWikiDomain и существующие DTO, не скомпилированную экспериментальную копию.

На каждую руку A/B один основной процесс последовательно:

1. Import empty checkpoint; GeneratePlanAsync по IQ1–IQ3:3чтения.
2. Import learned checkpoint в свежую machine; та же панель:3чтения.
3. Один SaveCheckpointAsync в собственный stage output, затем существующий LoadCheckpointAsync; та же панель на восстановленном экземпляре:3чтения.
4. Получить GetOrchestratorAsync у этого экземпляра; RunAsync по тем же3prompts с ScenarioId=OrchestrationPlanOrchestrator.ScenarioId, без golden metadata. Это ещё3Generate внутри consumer.
5. На отдельной копии исходного learned DTO заменить только FlowGroundingWeights на пустой словарь, импортировать; IQ1/IQ2:2чтения. Без learned weights не должно быть LearnedFlowGrounding.

Итого14Generate на arm, **28 на A+B, 0Train**, плюс2Save/2Load. Это fresh-instance disk restore в том же процессе, не cold OS/JIT. Snapshot до и после readout сравнивается по semantic projection исключающей только ExportedAtUtc; сравнение provenance last-evaluation не маскируется удалением произвольных полей. Состояние не мутирует от чтения.

Один contract Fact **Restriction_LeavesGlobalQueryAndEmptyFallbackUnchanged** использует public ImportState/ImportPairs того же learned snapshot, без Train, и делает ровно4direct lookups на arm:

- store.FindNearestWithScore(IQ2, Mode1) прежним двухаргументным overload —00-b, тот же global score;
- model.FindLearnedPair("", HumanIntentContract.Empty, grounding c LAB-A и пустыми EvidenceFeatures) —null;
- model.FindLearnedPair(IQ1, Empty, grounding c LAB-MISSING и пустыми EvidenceFeatures) —A может дать чужую пару, B обязан null;
- model.FindLearnedPair(IQ1, Empty, grounding c LAB-A и пустыми EvidenceFeatures) —B обязан10-a.

Конструктор FlowGroundingResult во всех случаях задаёт confidence0.95 и пустые списки phase/artifact/gate/evidence. Это ограниченные contract fixtures, не доказательство нового выбора LAB-A. Никакой дополнительный вызов TryGround или подстановка predicted FlowRef в query не нужен.

Общие A assertions проверяют данные/терминальное выполнение, сохранность global/empty semantics и baseline observation; Compare после B проверяет исправленные constraints. Отдельный contract finding не получает собственного “успешного обучения”.

### Существующие тесты и ожидаемые изменения

В Tests/OrchestrationPlanTests.cs сохраняются:

- OrchestrationPlan_FreshMachine_WorksWithoutCorpusDir;
- FreshUntrainedMachine_DoesNotGroundFlow33.

Оба вызывают по1Generate без Train. Запуск ровно этих двух до A и после B добавляет **4Generate**. Полный расход назначения: **32Generate +8direct lookups, 0Train**. Никакие helpers TrainedMachineAsync/Loop не запускаются широким фильтром.

PromptFlowGroundingModel_DoesNotUsePredictedFlowRefAsKnnQueryFeature, DoesNotGroundFromChosenTitleOnly и ConfidenceUsesInputOnlyFeatures не переписываются; новый contract и основной readout адресно защищают изменённую границу. Их прежний corpus-based fit не добавляется скрыто в budget этого назначения.

На исходном A расхождение IQ1 с целевым телом ожидаемо по raw: оно сохраняется как known mismatch в terminal receipt и не прерывает capture остальных строк. Проверка исправленного B выполняется Compare после готовых A/B receipts. Если B сохраняет расхождение, исправлять last retrieval, не менять expected body на LAB-B и не выравнивать только печатный заголовок. Known mismatch не превращается в PASS семантического критерия или Skip теста. Старые mode/refusal/contract failures сначала разобрать по source trace. Эта документная сессия не запускала тестов и не утверждает наличие свежего red suite.

## 6. Команды и владение одним запуском

Применяется [03§9](03-test-and-execution-contract.md#native-ab-transfer): common harness+fixtures commit A, затем единственный product diff, потом B в C:/Projects/llm_mvp_core, codex/dpo-implementation. Разрешённые product paths только42.ProjectWikiDomain/OrchestrationKnnStore.cs и PromptFlowGroundingModel.cs. API добавляется после A; common tests вызывают только существующие методы и поэтому компилируются до factor.

Будущий PlanId=FIX-IM у Run-NativeAB.ps1:
existing filter — две полные FQN из§5 с prefix FullyQualifiedName=LlmFromScratch.Tests.OrchestrationPlanTests.;
contract filter — FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.IdentityRouteContractTests.Restriction_LeavesGlobalQueryAndEmptyFallbackUnchanged;
learning filter — FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.IdentityRouteLearningTests.RestoredLearning_DeliversSelectedBodyAndConsumer.
Discovery каждой стадии —4tests. Existing→contracts→learning:3процесса на руку, всего6,2Rebuild/2discovery. API/runtime/native DLL identities фиксируются без нового чтения модели.

~~~powershell
Set-Location C:/Projects/llm_mvp_core
$baselineCommit = (git rev-parse HEAD).Trim()
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-IM -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Только после terminal baseline-stage, назначения product factor и его review/commit в том же checkout выполняется стадия B. До вызова runner проверить, что A — предок B; сам runner дополнительно сверяет HEAD, разрешённый product diff, неизменность harness/fixtures и identity terminal A:

~~~powershell
$candidateCommit = (git rev-parse HEAD).Trim()
if ($candidateCommit -eq $baselineCommit) { throw "CANDIDATE_EQUALS_BASELINE" }
git merge-base --is-ancestor $baselineCommit $candidateCommit
if ($LASTEXITCODE -ne 0) { throw "BASELINE_IS_NOT_CANDIDATE_ANCESTOR" }
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-IM -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Переменные означают два реально записанных SHA common harness и будущего factor; нельзя передавать candidate на Baseline или догадаться по имени папки. Output:
artifacts/fbe/FIX-IM/attempt-0001/{learning/A,learning/B,contracts/A,contracts/B,baseline/A,after/B,comparison.json,report.md,status.json}.
Сохранённый native S+ каждой руки — learning/<arm>/learned.checkpoint.json; generated receipts не переносят snapshot автоматически в production registry. В дальнейшем эти exact bytes разрешено использовать read-only при совместимом receiver-контракте20; обучения ради отчёта нет.

Deadline отдельного тестового процесса5мин, Rebuild15мин. Ждать supported wait того же handle≤60сек за вызов в том же ходе; долгий запуск не означает отказ. Terminal status содержит actual counters и not_reached; уже завершённые данные не удалять и не повторять ради форматирования. Ошибка comparer исправляется чтением тех же raw. Общая build предпосылка02§10 сохраняется одинаково A/B.

## 7. Включение в prompt→domain E2E без подмены выбора

FIX-IM покрывает два уровня: learned prompt→route и delivery полного тела через реальный OrchestrationPlanOrchestrator. Это не внешний междоменный selector. В [20](20-prompt-domain-e2e.md) его тот же принцип используется дважды:

- после outer learned topic проверить совпадение grounding.FlowRef, retrieved pair.FlowRef и typed binding до dispatch;
- после вызова orchestration receiver сравнить полный выбранный PlanText, не лишь route label или MlGenerated.

В20 уже задан отказ неверному pair/grounding handoff. Новый48 исправляет origin несогласованности; он не даёт разрешение менять classifier на if/else по ключевым словам, менять domain tags или открывать Wiki gate. Budget20 не растёт от добавления48; receipt48 — входной reference для этого invariant, не его новый hidden producer. IDENTITY состояние не заменяет RET receiver автоматически: это разные корпуса/задачи и manifest IDs.

## 8. Промпт исполнителю

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/48-learned-route-body-identity.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Назначен только FIX-IM после решения о переносе. Работай по48,01,02,03.
Модель — текущая default без override. Язык отчёта — русский.
Skills в docs/skills: trainable-architecture-decision-SKILL.md,
phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md,
prompt-patch-authoring-SKILL.md, human-communication-GLOSSARY.md.
Сначала восстанови полные raw states из точного источника; новый fit запрещён.
Одна A/B пара, два native recipients, exact filters/budgets заданы.
Не объявляй untrained fallback обученным ответом и не подменяй полное тело меткой.
Любое полезное измеренное улучшение — успех по своей теме/оси; ухудшения рядом.
Отчёт: artifacts/fbe/FIX-IM/attempt-0001/report.md, шаблон03§8.
DPO заново не генерировать. При отдельном задании новые chosen/rejected создаёт
внешняя открытая китайская модель; роль человека при выборе и оценку качества
выполняет CODEX с astra 6 ultracode. Фактическую модель фиксировать честно.
STOP/NEXT немедленно прекращает старую карточку: не дописывать старые отчёты,
тесты, commit или cleanup. NEXT переходит к указанному ID, не к старому номеру.
Старые эксперименты не удалять. Ждать исходный живой процесс до terminal receipt
штатным wait в том же ходе, без blind polling/relaunch или финального «жду».
Не добавлять fullCI, чужой factor или новый budget для получения PASS.
~~~



## Свидетельство уверенности того же маршрута — уточнение 12 ноября 2026

**Приоритет этого уточнения:** требование «сохранить глобальный поиск» в строке TryGround таблицы выше было границей прежнего body-only FIX-IM. Принятый FIX-IM сохраняется. Для нового отдельного ремонта B12-CW это ограничение уточняется: глобальная форма store API остаётся доступной, но подтверждающий сосед внутри TryGround должен принадлежать уже выбранному FlowRef.

[Контрпример и полный план](audits/2026-11-12/codex-flow-confidence-review.md) проверены на source dcb9317 и арифметическим пересчётом; native UNRUN. Урок A: FLOW-01/alpha/ALPHA_BODY_ONLY. Урок B: FLOW-02/beta/BETA_BODY_ONLY. Обычный запрос `beta FLOW-01` настоящим классификатором получает AllowedFlowScope=[FLOW-01]. С A собственный сосед имеет score0 и получается fallback. После добавления B выбор остаётся A (input+scope=2, при равенстве преимущество собственного scope), однако глобальный сосед B имеет Jaccard1/3 и поднимает confidence A до29/30. Затем уже правильный flow-scoped body lookup возвращает A. Это не чужое тело; это чужое подтверждение допуска.

**Инвариант нового ремонта:** candidate.FlowRef, confidenceWitness.FlowRef и body.FlowRef совпадают; чужой положительный сосед не превращает нулевое собственное свидетельство в допуск. Сам score0.966667 не является вероятностью правильности. Исторический B12 уже предусматривал foreign negative; здесь задан конкретный достижимый механизм, не объявлен новый универсальный класс ошибок.

Минимальная форма в PromptFlowGroundingModel.TryGround — существующий вызов `FindNearestWithScore(queryText, PairMode.Mode1LearnedFlowGrounding, best.FlowRef)`. Сохранить ranking, query, finite/positive guards, thresholds и фильтр body. При отсутствии своего свидетеля не откатываться на чужой глобальный. Прямой двухаргументный store API и его исторические contract tests не требуется удалять или менять. Пороги не снижать, FlowRef/gold-body в исходный query искусственно не добавлять.

Предложенная приёмка содержит2 одноэлементных учебных batches,17 GeneratePlanAsync,2 Save и5 Load, разложенных по стадиям в указанном плане. Учебные S-A/S-AB подготавливаются один раз; subsequent loads не обучают. Положительные alpha/FLOW-01 и beta/FLOW-02 сохраняют полный собственный body; отрицательный beta/FLOW-01 возвращает прежний fallback. Обратная мутация только scoped→global обязана уронить этот отрицательный oracle. У текущего аудита native расход0;17 — будущий адресный бюджет, существующие регрессионные классы учитываются отдельно.

Это не закрывает B11/активную редакцию цели: same FlowRef может содержать несколько revisions. При последующем revision-ремонте тот же инвариант расширяется явной membership активной редакции для weights, witness и body. Не объединять два ремонта сменой общего порога или отключением learned пути.
