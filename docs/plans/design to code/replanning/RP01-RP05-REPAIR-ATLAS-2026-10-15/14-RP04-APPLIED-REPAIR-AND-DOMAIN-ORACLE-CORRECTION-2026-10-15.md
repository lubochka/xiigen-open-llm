# RP04: применённые исправления, пятое уточнение и границы проверки

**15 октября 2026 · PLANNING_DRAFT / AUTHOR_SOURCE_REVIEW.** Опубликованы изменения реальных инструкций RP04, а не только ещё один список рекомендаций. Это не выполненное обучение, не исправленный core, не измеренный прирост и не независимая приёмка Astra. Новых Rxx, task IDs, обязательных циклов или product tests не добавлено.

## 1. Что действительно изменено

| Носитель | Изменение | Commit / content blob |
|---|---|---|
| [13 — BD01–BD04](13-RP04-BOUNDED-DISPATCH-REVIEW-2026-10-15.md) | Четыре source-bound уточнения существующих F17–F30, с конкретными примерами, владельцами, опровержением и минимальной проверкой | commit 8f2d923c35166f7a005341529b456e3f45f26043 |
| [RP04.01A](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.01A.md) | Переписана существующая карточка: два фактически наблюдавшихся законных кандидата, ограничение пула до ранжирования, lineage и реальная граница cancellation | commit 71d774fa565fc5dd117eac2539f4ecf995b76847; blob 8f4d07bc7ad88713b86fafe60edf6a9f5283cd28 |
| [RP04.01B](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.01B.md) | Переписана существующая карточка: точный получатель одного update, численный пример градиента, once-only journal, stable identity versus dispatch ordinal, место фактического чтения theta | commit eb4f8bea866f8367077c55c62273748012fdd020; blob eda35e27bd84080ef6fdb8a8cb61973b8744cc58 |
| [RP04.03B](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.03B.md) | Переписана существующая карточка: отдельные rank/reach/dispatch experiments, восемь строк предметных сценариев GOOD/BAD/EDGE, BD05, явные знаменатели и стоимость всего пути | commit dd0592172eb5a6bc79b84fde101fca5ee454f4f3; blob 4472a2effc6784f0b3a6a4d37ce9cfdc19eb8068 |
| [INSTALL-AND-START](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/INSTALL-AND-START.md) | Переписан существующий вход: выбрать ближайшее действие по имеющимся результатам; не возвращаться автоматически к 00A/00B; один актуальный физический prompt | commit fd664162ad562db96f3dffaa141f10acb0c20aab; blob 32d922852e26371a94ff9f30adbcd36455e33b82 |

В актуальном прочитанном manifest — **12 task IDs, V6**. Из них заново прочитаны полностью и переписаны **три физические карточки**, а не все 12. Остальные девять не выдаются за свежую полную semantic review. Их владельцы и функциональность сохранены в startup; отдельного продукта или версии V7 не создавалось. Восемь строк предметных сценариев — дизайн экзамена, не восемь выполненных экспериментов.

## 2. Пять уточнений: что ново, а что не надо ремонтировать заново

| ID | Классификация | Что изменяется в поручении |
|---|---|---|
| BD01 | VERIFIED_LOCAL_SOURCE; уточнение F28/F30, качество NOT_MEASURED | AxisSweep сортирует node/edge IDs и обрезает первые комбинации. При cap 4 четыре flag profile могут принадлежать одному A/E1. Кандидат другого домена не появится от последующего reranking. Проверять allocation до cap, отдельно lawful reach и conditional rank. |
| BD02 | VERIFIED_INTEGRATION_CONSTRAINT; не наблюдавшийся runtime-дефект | Runner требует Index == position, но CombinationId — смысловая identity. При learned reorder перенумеровать dispatch ordinal, сохранив CombinationId и привязку feedback. Не учить по позиции массива. |
| BD03 | VERIFIED_LOCAL_STRUCTURE / CONDITIONAL_DELAY_RISK | Enumeration/validation предшествуют timeout scope; cancellation drain ожидает детей без отдельной границы. Не обещать end-to-end deadline или освобождение ресурсов только по TIMEOUT. Не приписывать этому прошлые Windows-зависания без наблюдения. |
| BD04 | VERIFIED_LOCAL_LOOP; уточнение F25/F30 | Runner исчерпывает поданный список при обычном завершении. Post-result rank может улучшить выбор, но не отменяет уже оплаченную работу. Обозначать RESULT_RANK отдельно от DISPATCH_SUBSET/STOP и измерять actual launches. |
| BD05 | DOMAIN_ORACLE_DESIGN_CORRECTION, не установленный дефект core | Не превращать пример NullGuard→Bind в универсальный закон, если Bind только готовит nullable SQL parameter. Фиксировать schema/business precondition и настоящий protected effect до создания preference label. |

F17–F30 не объявлены заново найденными. На свежем K подтверждены указанные ниже конкретные участки DynamicGraphMachine/AxisSweep/runner; остальные прежние findings использованы с их опубликованными ограничениями, а не как якобы полностью перепроверенный runtime. Например, отсутствие inner law guard в одном фрагменте не доказывает отсутствие downstream enforcement.

## 3. BD05 — подробный новый пункт для атласа

**Наблюдение плана:** в [F28–F30, раздел Domain examples](11-RP04-F28-F30-NEW-DIRECT-SOURCE-FINDINGS-2026-10-15.md) используется сокращение NullGuard→ParameterizedBind как GOOD и Bind before Guard как BAD. Без указания операции и предусловий это позволяет менее сильному исполнителю сделать неверное универсальное правило и испортить DPO/graph preferences.

**Первичный API-источник:** [Microsoft SqlParameter.IsNullable](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqlparameter.isnullable?view=sqlclient-dotnet-core-7.0) объясняет, что IsNullable не проверяет значение параметра, а null обрабатывается через DBNull. Это подтверждает API-семантику, но не доказывает, что любая бизнес-задача разрешает null.

**Точная замена смысла:** закон задаётся как `when precondition Q applies, establish Q before protected operation E`. Подготовка параметра, проверка бизнес-условия, dereference nullable value и выполнение SQL — разные операции. Для задачи с обязательным ненулевым id нельзя выполнять защищённую операцию до проверки; для допускающего NULL поля подготовка DBNull-параметра не становится отрицательным примером только из-за порядка подготовки.

**GOOD:** schema/task явно требует id; проверка проведена до Execute/опасного dereference, typed output корректен. Другой GOOD: schema разрешает NULL, создан безопасный параметризованный read-only proposal с DBNull; нет ложного запрета на его подготовку.

**BAD:** пометить любую Bind-before-Guard последовательность как rejected; либо выставить IsNullable=false и считать, что runtime сам проверил бизнес-условие. **Почему:** ошибочный oracle заставит систему учить неверную границу между законом и предпочтением.

**EDGE:** неизвестны dialect/schema или Bind означает сразу execution. Не угадывать: записать LAW_UNRESOLVED и уточнить конкретный источник/операцию. **Опровержение:** если исходный конкретный сценарий явно определяет Bind как защищённый effect и требует Q, исходный порядок корректен именно для этого сценария; уточнение остаётся запретом на необоснованное обобщение, а не отменой guard.

**Применено:** условность примера прямо записана в 01A, 01B, 03B и startup. Исторические F28–F30 и прочие старые носители не переписаны целиком; их сокращённый пример нельзя считать проверкой общего закона. Продуктовые SQL/graph тесты и код этим действием не менялись.

## 4. Как быстрее получить обучение, не потеряв требования

Ближайший шаг выбирается по готовности артефактов, не по обязательному повтору всей нумерации. Если реальные 00A/00B inputs и два законных observed outcomes уже есть — **сразу 01B**. Если update уже записан — **01C**, без повторного градиента. Если cold receipt одной ветви готов — **03B**, не ждать вторую ветвь. Только отсутствующий источник/owner/пара возвращает ровно к соответствующей части 00A/00B/01A.

E3 и E4 остаются независимыми. Первый честный CONFIG_PREF update не закрывает обязательство структурного выбора путей или higher-order graph: эти результаты продолжают иметь своих владельцев и отдельный экзамен. Первый урок можно проводить на одном реально поддержанном домене; двухдоменный пилот следует после update; широкая панель — следующий уровень доказательства, а не предварительный стопор.

**kNN и предметные векторы не объявлены второстепенными.** Для выбранного домена нужны достаточные законные role/token/vector/kNN inputs. Их фиксируют в отдельном RP04 причинном сравнении, чтобы не перепутать источники прироста. Если representation не выражает критическое правило, affected case не считается корректным учебным входом. RP02 исправляет этот механизм; RP03 отвечает за typed many→many и Flow corrections. LoRA/projection, continual learning и обычная DTC/IDE поставка не исчезают от минимизации test coverage.

Прогресс различает работающий код, реальное изменение состояния, чтение состояния обычным потребителем, изменение результата и независимый прирост. Документ или изменённый hash не заменяет ни один из последних четырёх фактов.

## 5. Что проверено по источникам

**P baseline:** d507493cb54a60bfff9e48e9bab5435e9c3d85bf. **K baseline:** a564772ab713aae071b27c513a9bad5a1a775a5e. Все code claims ограничены прочитанными spans; для реального edit требуется полное чтение изменяемого файла и вызывающих/вызываемых компонентов.

| Источник | Реальная глубина и назначение |
|---|---|
| K/DynamicGraphMachine.cs | 1–145, целиком полученное тело; blob 8149717b04d442e6e89bb61ab34fb608201ef369. Learning lifecycle здесь остаётся NOT_ACTIVATED, не доказательство отсутствия всех других trainers. |
| K/AxisSweepCombinationStrategy.cs | 1–195; blob 9720de08246190fe9696115f78d9cba77178c94d. Ordinal axes, four profiles, early cap, canonical signature. |
| K/BoundedCombinationGraphCandidateRunner.cs | 1–330, 425–535, 600–760; blob 8099c2c68d3977f61ddf15ada77b89451f4a499a. Dispatch loop, index/identity validation, timeout and drain. Не полный файл. |
| P/RP04 source architecture, startup, 01A/01B/03B | Полные полученные тексты до правки; manifest 1–160; его task IDs подтверждают 12, хвост не переписывался. |
| P/Atlas F22–F27 и F28–F30 | Полные полученные приложения; README/02/04 — относящиеся части с границами, не полный semantic audit всех numbered additions. |
| D/original task.md | 1–70: четыре роли и несколько async combinations; исторический список провайдеров не верифицирован как актуальная лицензия/рекомендация. |
| D/WorkLeft MASTER | 1–75: RAG как указатель, восемь state/learning обязательств; не всё многосоткилобайтное архивное тело. |
| D/WorkLeft RESEARCH-03 | 1–65: R16 exact pair, actors, no-repeat, complete state, causal utility. |
| F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md | 1–95: pre-retrieval signals, positive/negative/warning memory и независимое final arbiter acceptance. Старые E51 claims не новые измерения. |
| R/previous rounds thinking results.md | 1–45: R07 repair limitation и исходные 25 назначений; не вся история. |
| R/PARALLEL-R01-R03-JOIN.md | Начальный полученный фрагмент, включая PARALLEL25 override, source namespaces, owner/receipt rules; не все хвостовые разделы. |

Фактически читались относящиеся разделы шести K/docs/skills: trainable-architecture-decision 1–110; phase-planning 100–175; plan-review 1–75; conversation-intent-and-self-reflection 1–85; prompt-patch-authoring 1–125; generated-code-review 1–90. Применены named owner/signal/state/consumer, bounded current scope, exact-candidate review, literal correction, evidence honesty и ясная передача. Другие skills названы в будущих task packets и не объявлены полностью прочитанными здесь. Исторические 30/50 passes не исполнены и не добавлены к текущему поручению.

**Не заявлено:** полное чтение всех RAG/NDJSON, WorkLeft MECH-01, task prompt/goal, всех R01–R25 результатов, S00–S10, 32 slices или всех карточек RP01–RP05. Эти источники/обязательства сохранены у владельцев; их существование не заменяет прямого чтения.

## 6. Авторская проверка публикации

Для каждого update использован известный SHA именно изменяемого файла; чужие файлы/ветки не переписывались. После четырёх правок выполнен remote readback на **fd664162ad562db96f3dffaa141f10acb0c20aab**: 01A 30–57, 01B 25–44, 03B 35–69, startup 1–31. Полученные content blobs совпали с четырьмя значениями в §1; в прочитанных участках видны BD-discriminators, конкретные примеры и state-aware dispatch. Это targeted author readback, не независимый полный code/plan PASS и не замена semantic review непрочитанных девяти карточек. Commit данного receipt определяется Git history после записи, не выдумывается внутри файла.

**Статусы:** PLANNING_CHANGES_PUBLISHED; THREE_TASK_CARDS_AND_START_REPAIRED; AUTHOR_SOURCE_REVIEW_ONLY; PRODUCT_CODE_UNCHANGED_BY_THIS_WORK; NEW_PRODUCT_TESTS=0; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED.

**Роли неизменны:** CODEX с **astra 6 ultracode** выполняет делегированную человеку задачу выбора. Допущенная внешняя открытая китайская модель пишет **обе** окончательные DPO стороны — chosen и rejected. Другой вызов CODEX с **astra 6 ultracode** оценивает качество exact pair. Отдельные post-update examiner и exact-SHA reviewer имеют собственные реальные receipts. Ни один такой вызов не выполнен публикацией этих инструкций.
