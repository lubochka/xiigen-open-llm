# HELDOUT-ADMISSION-ATOMICITY-01 — ошибка оценки после уже применённого урока

<a id="current-executor-task"></a>
## Текущее задание после P131 — границы допуска и сохранённой приёмки

<!-- CODEX-POST131-CURRENT:START -->
**Владелец: `HELDOUT-ADMISSION-ATOMICITY-01 / Q129` · RESULT_RECORDED; transfer HOLD.** На срезе `b27783593745ffac8e74e81f917539717cd33615` единственный PRIMARY `P12901/results-01` остаётся записанным. [Отчёт](../../../../../experiments-v2/RP01-MICRO/P12901/results-01/report.md) и пересчёт20 ответов подтверждают cold0/2 → S0 1/2 → corrected2/2 у A/B и2/2 после загрузки другим процессом. Обе живые модели после отказа сохранили S0. Наблюдённая польза раннего отказа B — один предотвращённый native call и одна неисполненная отброшенная учебная итерация. Действующий staging уже обеспечивает наблюдённую атомарность; preflight её здесь не исправляет. Corrected allocations у B выше на5,53%; N=1 и фиксированный A→B не доказывают стабильный speedup. Q130 и Q131 уже записаны; дальнейшее назначение определяется текущим индексом, а не прежним NEXT этой карточки.

**Существующий ремонт, а не новые находки.** `Run-Experiment.ps1` уже передаёт producer/child/verifier в Get-CombinedExit и завершает оболочку через exit. По [дополнению48](../codex-48-post128-executor-verification-2026-11-03.md) остаются реальный отрицательный verifier-процесс через outer entrypoint, включая0/0/9, read-only input/new output для saved reanalysis и достижение численной ветви finite-проверки. Историческая confidence="NaN" доказала String→Double отказ, не IsFinite. Эти известные ограничения не выдавать за новое обнаружение после P131 и не чинить уже исправленный Say-only путь повторно.

**Именованная граница `HELDOUT_SYNTAX_TYPED_FINITE`.** Неверный вывод: успешный JsonDocument.Parse и отказ на строковом "NaN" доказывают законность любого payload всех trainers и работу численного guard. Корректный вывод: выполненный фактор проверяет только синтаксис InputJson/ChosenOutputJson назначенного executor-en_ru до native Train. Typed-unit проверка требует реальных DTO соответствующей единицы и законного входа; численная проверка отдельно достигает finite/range предиката типизированным числом, включая NaN/Infinity при допустимом входе тестового API. Если JSON-десериализация отвергает значение раньше, это отдельный parser/type контроль. Нельзя заменять unit-контракт произвольной общей схемой или объявлять поддержку других UnitId по одному EN_RU. Такой будущий продуктовый ремонт не расширяет фактор и дозу выполненного Q129.

**Именованная граница `IMMEDIATE_FAILURE_FULL_STATE`.** Неверная приёмка: после отказа сначала Load/Import/restore S0 или corrected batch, затем сравнить counts и заявить отсутствие побочных эффектов. Корректная: на том же живом holder/runtime немедленно сохранить exception/rejection и полный pre/failure payload, exact/phrase/KNN targets, identities, versions, history/readiness и обычные ответы — до любого восстановления/исправления. Сравнение исключает только заранее названный timestamp экспорта; resource/history поля не исчезают ради равенства. Сохранённые pre/failure A и B совпали; запрет восстанавливать до снимка не требует нового повторения. В saved-copy контроле подмена target при прежних counts либо отсутствие immediate post-state должны блокировать общий итог. Candidate checkpoint, если он заявлен, требует настоящих bytes/hash/locator; отброшенная staged copy A не является сохранённым candidate.

**Профилактика по P131, без неподтверждённого обвинения P129.** Для каждого выводимого actual/route/verdict/confidence проследить уже имеющийся независимый native/process record и его потребителя. Равенство нескольких производных копий не подменяет эту связь. В P129 аналогичный согласованный обход не воспроизводился и не объявляется доказанным. Если исторический пакет не содержит отдельного native payload, отметить NOT_OBSERVED для этой дополнительной связи; не создавать его повторным Flow/Train и не делать исторический научный результат ошибкой только из-за нового требования. Полный подлинный неверный ответ остаётся допустимым отрицательным наблюдением.

**Действие и предел.** Только назначенный saved-only ремонт по48/49: сначала разделить read-only `input_root` и новый `verification_output_root`, потому что прежний `SavedEvidence.Run` записывает validation.json в Evidence.Root. Старую verify-команду на originals не повторять. Честный полный пакет и lawful corrected урок остаются положительными контролями; targeted corruption, false/missing обязательного поля и ненулевой настоящий verifier меняют общий status/exit. Поле связывается с названным потребителем и причиной отказа. Сохранённая доза —4 native batch/4 completed-training-iterations,20 ordinary Flow и2 source-derived internal Execute; новый ремонт добавляет0 Train/Flow/Load. Полная конкурентность, поздняя отмена Evaluate и store failure требуют отдельного продуктового задания, не пятого batch в PRIMARY.

**Текст для исполнителя — передать целиком:**

> Работай по ID HELDOUT-ADMISSION-ATOMICITY-01 / Q129 только в назначенном saved-only ремонте [48](../codex-48-post128-executor-verification-2026-11-03.md)/[49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md); сохрани RESULT_RECORDED, исходный паспорт/pin, fixtures/gold и бюджет. Прочитай действующие AGENTS/CLAUDE и применимые архитектурные, planning/plan-review, code-examination, test-integrity, QA/automation/debug, prompt-patch-authoring и human-communication навыки; запиши фактически прочитанное. Не повторяй PRIMARY/Train/Flow/Load. Результат: corrected и child2/2 у обоих, immediate pre/failure одинаков; атомарность уже обеспечивает staging, полезный эффект B — один предотвращённый native call и одна отброшенная итерация, corrected allocations+5,53% и N=1 рядом. Снимок полного живого состояния обязан предшествовать restore/Load/Import/corrected; counts не заменяют targets/identities/version/history. Проверяй это на имеющихся данных и новых искусственных копиях. Старые NaN, writable saved-output и настоящий третий verifier exit уже учтены в48, не называй их новыми. Syntax-only JsonDocument.Parse не доказывает typed-unit или finite/range валидность: typed вход привяжи к реальному DTO/unit, NaN/Infinity доведи до численного предиката отдельно от parser/type отказа, не расширяя исторический фактор. Сначала разведи read-only input_root/new verification_output_root; прежний SavedEvidence.Run на originals не запускай. На новых копиях покажи честный PASS, адресную порчу с целевой причиной и реальный nonzero verifier через outer entrypoint, включая0/0/9; source/DLL и terminal сохрани. Source-derived/Python проверки не называй native PASS, при отсутствии runtime укажи NATIVE_UNRUN. Связь actual с native record проверить профилактически лишь по уже имеющимся свидетельствам: обход P129 не доказан, отсутствующий record пометь NOT_OBSERVED и не добывай новым Flow. Полный подлинный неверный ответ сохраняется как наблюдение. Ни полезный частный результат, ни4 batch/4 completed iterations/20Flow/2internal Execute не превращай в общий продуктовый перенос. Поздняя отмена/store/concurrency вне этого PRIMARY; STOP/NEXT пользователя имеет приоритет.

Основание: [аудит149](../../fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md), [договор49](../codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) и ранее назначенный [ремонт48](../codex-48-post128-executor-verification-2026-11-03.md). Исторические блоки ниже сохраняют данные, gold, factor, дозы и команды; их незапущенные статусы/NEXT относятся к прежним срезам и не являются новым назначением.
<!-- CODEX-POST131-CURRENT:END -->

## История постановки и передачи — не текущая команда запуска

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — staging уже есть, поздняя оценка отдельно

M04 теперь создаёт закрытую копию и атомарно публикует целый граф; прежнюю формулировку «такого holder нет» не использовать. Q129 всё ещё отдельный незапущенный опыт preflight held-out, не новая приёмка всей конкурентности. Positive законная назначенная партия после отказа работает. Negative поздняя held-out ошибка не должна изменить живой payload/history/identity; проверять сразу, до внешнего restore. На текущем staged A после отказа тоже остаётся S0: hoa-new обработан в закрытой копии, но не опубликован. После исправленной партии ожидаются 6 entries у A и B. Нулевая дельта live-state допустима; проверяемая польза preflight — ранний отказ и отсутствие лишнего staging/обработки, если это реально измерено.

У result есть measured state identity, старая оценка не маркирует новую версию. Межпоточный R01 проверяется отдельным dummy/product gate, не добавляет пятую batch попытку этому паспорту. Сохраняются 4 native batch calls/4 фактических урока/20 flow+2internal Execute; failure/exception и общий budget называются отдельно.

Основание: [адресный план 135](../../fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- POSTMERGE-HARDENING-2026-11-02:START -->
## M03/M04 — failure-state проверяется до исправления, фактор остаётся preflight

Единственный factor Q129 — held-out syntax admission в wrapper: A вызывает текущий native batch без wrapper preflight и обрабатывает урок в staged copy до позднего отказа, B отказывает до native Train. Живая память обеих после отказа остаётсяS0; нулевая разница целостности допустима. Сразу после ошибки читать полные payload, identities/version/history и назначенную panel на тех же живых A/B до restore или corrected batch; сохранные counts при подменённом target не проходят content-checker. Corrected batch и чтение final checkpoints child без replay уже входят в рецепт. Сохраняются4 native batch calls,4 фактических lessons,20 ordinary Flow и2 internal Execute. Не возвращать исправленный partial-update дефект в historicalA ради положительной дельты. Для текущего staging требованиеS0 после отказа относится к обеим ветвям. Единый holder и проверки ранее выданного/нового orchestrator принадлежат отдельной product acceptance M04/124-S09, не меняют B на транзакционный алгоритм и не добавляют Flow к PRIMARY.

Основание и выбранный ремонт: [план127](../../fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Исходные результаты и учебные дозы сохраняются. Контроли реализации не добавляют скрытые Train/Read к научному паспорту; их стоимость и scope указываются отдельно.
<!-- POSTMERGE-HARDENING-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [120-executor-context-and-test-repair-contract](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница HELDOUT-SYNTAX:** WRONG: требовать whole-transaction/typed/evaluator/save/concurrent-version repair от wrapper-only syntax фактора. CORRECT: один shared S0; A/B различаются только preflight malformed hoa-eval до настоящего TrainStructuredBatchAsync. Сразу после отказа, до исправления/restore, фиксировать hoa-new identity и exact/phrase/KNN state, history/version/readiness; B сохраняет S0, назначенная corrected batch затем действительно учит новый ответ. Положительны keep и законный corrected open-file; отрицателен malformed heldout. Остаются4native batch/4lessons/20flow+2internal Execute. Случайная late error не разрешает повтор Train; полная атомарность остаётся отдельным RF01/R06.

**Исходный адрес из паспорта:** `27.TranslationDomain/TranslationMachine.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
**Действующее согласование:** [40-effective-executor-context](../40-effective-executor-context-2026-11-02.md) связывает роль, scorer и стадию попытки; [120-post102-transfer-and-executor-audit](https://github.com/lubochka/llm_mvp_core/blob/d18b716e4a855f6dd94415104f48999876108a51/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-post102-transfer-and-executor-audit-2026-11-02.md) — отдельный источник, не этот договор120 и не разрешение повторить recorded PRIMARY. Этот адресный абзац заменяет прежний общий пример только в своём факторе; данные/дозы/команды ниже сохранены.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-CODEX-AUDIT-2026-11-02:START -->
## Действующее уточнение POST107 — Отказавшая партия проверяется на том же состоянии до исправления

**Проверка `POST107-Q129`. WRONG:** после JsonException загрузить S0 либо отправить corrected batch и только затем объявить atomicity. **CORRECT:** на `TrainStructuredBatchAsync → TrainOnExample → MeasureHeldOutAccuracy` немедленно снять failing-state того же A/B: actual exception/rejection reason, exact/phrase targets, KNN tuples, identities, version/history/readiness и ordinary ответы. Count keys недостаточен: повтор new lesson может увеличить tuples при прежних словарных ключах.

**Честный и испорченный контроль.** Уже назначенная malformed heldout строка действительно содержит `{`, а B preflight отклоняет её до native Train; checker не знает имени arm. Сравнить failure-state B с его реальным pre-state, A — с его собственным, до corrected batch. В копии receipt сохранить counts, подменив target `keep`: неизменная проверка полного retained content/answer должна отказать. Corrected batch на тех же живых A/B — назначенная часть одной программы; она не заменяет отдельную запись стоимости отброшенного staged updateA; если вопреки текущему контракту возникла live мутация, это отдельный измеренный отказ. Child затем действительно загружает final checkpoints без replay.

**Предел и бюджет.** Остаются4 native batch calls/4 actual lessons,20 ordinary flow+2 internal Execute; wrapper-only отказ не native call, применённый в упавшей партии урок не0. Syntax preflight не доказывает rollback при позднем I/O/cancel/semantic failure. Дополнительные failure injections и reruns не добавляются к этому PRIMARY; статус запуска не меняется.

Основание и общий порядок проверки: [аудит POST107](../../fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md) и [инструкция41](../codex-41-post107-executor-checks-2026-11-02.md). Этот верхний блок уточняет только названные доказательства и будущую приёмку; исходный текст/результаты ниже сохранены, очередь и разрешение запуска не меняются.
<!-- POST107-CODEX-AUDIT-2026-11-02:END -->

<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Syntax preflight не доказывает полную транзакцию

**Проверка `POST102-Q129`. WRONG.** Отказ на malformed heldout до Train обеспечил atomicity для typed DTO/evaluator/save failures.

**CORRECT.** Назначенный единственный фактор — heldout syntax admission в wrapper на shared S0. Native update/evaluation путь настоящий, wrapper не выдаётся за полный продуктовый transaction repair. R06 отдельно покрывает поздние failures и весь state.

**Адресная приёмка.** Проверить до/после exact/phrase/KNN memory, logical identity, version/history/readiness и20ordinary flow+2internal Execute. Всего4batch calls/4actual lessons по паспорту, повтор corrected batch на A является назначенной частью опыта. Непредусмотренный late failure не даёт автоматического повторного Train; state/price остаются в receipt.

Применять [единый контекст40](../40-effective-executor-context-2026-11-02.md) и [адресный план121](../../fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md): актуальные роли навыков, один рабочий оценщик, ordinary consumer и реакция на test/report failure. Это ограничивает вывод и усиливает приёмку; исходные factor, доза, fixed panel и порядок сохраняются. Исправление saved evidence не является новой PRIMARY, а наличие этого блока не разрешает запуск. Исторические указания ниже действуют только в своей исходной области; при противоречии бюджета/фактора требуется адресное решение до исполнения.
<!-- POST102-AUDIT-2026-11-02:END -->


Q129. 01.11.2026. **DESIGNED_NOT_RUN / SHALL_IMPLEMENT только после прямого назначения.** В нынешней исследовательской сессии runner, данные и состояния не создавались; запуск не выполнялся. Тема обучения — перевод; профиль EN_RU/general. Получатель — штатный ExecutorUnit, состояние — exact/phrase/KNN память; нейронного обучения/DPO здесь нет.

## Зачем нужен опыт

Переводчик должен либо принять партию вместе с её оценкой и квитанцией, либо сохранить прежнее принятое состояние. Сейчас JSON-preflight проверяет только обучаемые строки, а отложенная строка `HeldOut=true` десериализуется после новых уроков в закрытой staged-копии. Если она испорчена, партия завершается отказом до публикации: живое состояние A и B остаётся S0. Ранний admission B может отдельно уменьшить напрасную работу подготовки. Повтор исправленной партии применяется к прежнему живому S0 и не должен накапливать урок из отвергнутой копии.

Это новая проверяемая граница P53, а не доказанный результат исправления. [Аудит110](../../fixes-based-on-experiments/110-transfer-state-and-new-evidence-audit-2026-11-01.md) фиксирует поиск дубликатов и текущий код. Ремонт нечитаемого **учебного** JSON из109 уже присутствует в рабочем дереве и не является фактором этого опыта.

Источник причинной цепочки:

- [TranslationMachine.TrainStructuredBatchAsync](../../../../../27.TranslationDomain/TranslationMachine.cs#L89): preflight → TrainOnExample → MeasureHeldOutAccuracy → version/history/result.
- [MeasureHeldOutAccuracy](../../../../../27.TranslationDomain/TranslationMachine.cs#L868) вызывает настоящий `trainer.MeasureAccuracy`.
- [ExecutorUnit.MeasureAccuracy](../../../../../27.TranslationDomain/TranslationTrainedUnits.cs#L489) читает InputJson/ChosenOutputJson без локального catch, затем вызывает Execute.
- [ExecutorUnit.TrainOnExample](../../../../../27.TranslationDomain/TranslationTrainedUnits.cs#L430) добавляет exact и три signature/prefix/structure KNN записи. Число добавленных записей проверяется по фактическому checkpoint; не путать его с числом различных dictionary keys.

## Один фактор и существующий код

A — общий экспериментальный wrapper с выключенной проверкой held-out; вызывает текущую штатную `TranslationMachine.TrainStructuredBatchAsync`. B — тот же wrapper с проверкой синтаксиса InputJson и ChosenOutputJson у отложенной строки **до** native вызова. После успешной проверки B вызывает тот же native метод. Скопировать в host чистую логику `DescribeUnreadableJson`; не копировать learner и не заменять его таблицей ответов.

Область wrapper намеренно одна: `TranslationUnitIds.ExecutorFor(TranslationDirection.EN_RU)`. Любой другой UnitId во входном fixture — ошибка подготовки, а не повод придумывать правило для unknown-unit. Новый gate возвращает `HELDOUT_ROW_UNREADABLE` и ID ошибочной строки. A и B одинаково фиксируют исключения/result, но не ловят ошибку так, чтобы объявить частичный Train успешной партией. Только B включает раннюю проверку отложенного JSON; trainable-row preflight одинаков у обоих и остаётся native.

Продукт, исходные P5301/109 и другие эксперименты не меняются. При подтверждении кандидат будущего ремонта — расширение `ValidateStructuredBatchRowsReadable` на реально оцениваемые held-out строки известных trainers. Типовая/семантическая валидность DTO, отмена, ошибки записи, неизвестные Unit и общая транзакционная архитектура этим не доказаны.

## Точные данные

DTO и signature строятся как [P5301.BuildExample](../../../../../experiments-v2/RP01-MICRO/P5301/Program.cs#L837): штатный `TranslationSemanticTokenizer`, `ExecutorInput(source,"en","general")`, `ExecutorOutput(target,0.95,null,null)`, `TranslationRoutingFlow.FlowId`, SourceId=`hoa01-v1`. WordMappings/PhraseMappings отсутствуют. RejectedOutputJson — `ExecutorOutput("[WRONG: missing translation]",0.1,null)`; RejectedType=`synthetic_missing_translation`. Этот rejected текст не участвует в обучении Executor и не выдаётся за DPO.

| ExampleId | Source | Chosen | HeldOut | Назначение |
|---|---|---|---|---|
| hoa-base | keep | сохрани | false | Общий реально обученный S0 |
| hoa-new | open file | открой файл | false | Единственный новый урок |
| hoa-eval | open file | строка ChosenOutputJson ровно `{` | true | Ошибка после урока в A |

Исправленная партия меняет **только** ChosenOutputJson `hoa-eval` на сериализованный `ExecutorOutput("открой файл",0.95,null,null)`. ExampleId, SourceId, UnitId и порядок `[hoa-new, hoa-eval]` сохраняются. BatchId общей базы `hoa-base-batch`; обе версии второй партии — `hoa-candidate-batch`. Это запланированный ошибочный вызов и затем исправленный вызов внутри одной программы, не скрытый перезапуск опыта. Не менять ID исправленного урока, чтобы скрыть повторное добавление.

Панель ordinary flow: `keep → сохрани`, `open file → открой файл`. Подавать реальный UserRequest через `GetOrchestratorAsync → RunAsync` с metadata source_language=en, target_language=ru, topic_domain=general, как существующий P5301. Не подменять flow вызовом внутреннего Execute. Проверять actual text, отказ и маршрут; gold используется только внешним scorer.

## Программа и полное обучение

1. Новый producer: cold panel2, затем настоящий batch с `hoa-base`, затем panel2. Требуется `keep` correct через принятую память, `open file` ещё не выучен. Если этого нет — INVALID_START; 0→0 не объявлять полезным результатом. Никакого подбора rate/seed/дополнительных уроков.
2. Один раз сохранить штатный S0 через `SaveCheckpointAsync` в `results/attempt-0001/shared/s0/checkpoint.json`. Это reusable learned start; receipt хранит корпус/профиль/версию/checkpoint identity. Нельзя подменять его `ExportAsync`, который не сохраняет trained state.
3. Две независимые машины A/B загрузить через `TranslationMachine.LoadCheckpointAsync` из этого S0, без Train. Перед каждой ветвью panel2 подтверждает исходные ответы.
4. Каждая ветвь получает ошибочную партию. На текущем staged A обработает `hoa-new` в закрытой копии и затем предположительно бросит JsonException на held-out **до публикации**. B должен отвергнуть её до native Train. В живой памяти обеих ветвей после отказа остаётся S0; отсутствие частичного update у A не является ошибкой опыта. Сразу сохранить result/exception, ExportCheckpoint и panel2, **до исправленной партии**. Не выполнять внешний rollback/reload до наблюдения: проверить реально опубликованное состояние A, а не содержимое отброшенной staged copy.
5. На тех же живых A/B передать исправленную партию один раз. Ожидаются принятые обе строки как input, один обучаемый урок и одна реальная held-out Execute-оценка. Panel2 —2/2 у обеих. B должен действительно приобрести `open file`; один только отказ не считается успехом.
6. Сохранить итоговые A/B в `a/final/checkpoint.json`, `b/final/checkpoint.json`. Один новый child process последовательно загружает оба checkpoint, читает panel2 у каждого и завершает работу без Train. Передать exact file paths, не искать latest и не replay корпус.

Все пути выше относительны новому host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HOA01/`. Порядок — producer → A полностью → B полностью → child; параллельного обучения нет.

| Стадия | Native batch calls | Фактические Unit lessons, ожидаемые по исходнику | Ordinary flow | Held-out internal Execute |
|---|---:|---:|---:|---:|
| Producer cold → Train base → panel | 1 успешный | 1 | 4 | 0 |
| A load/pre → malformed → panel → corrected → panel | 1 с ошибкой +1 успешный | 2, включая урок в упавшей партии | 6 | 1 |
| B load/pre → gate rejection → panel → corrected → panel | 1 успешный; отказ wrapper до native не считается batch call | 1 | 6 | 1 |
| Fresh child A и B | 0 | 0 | 4 | 0 |
| **Всего** | **4 native вызова:3 успешных +1 с ошибкой** | **4** | **20** | **2** |

Итого22 чтения обученной единицы на разных уровнях; ordinary20 и internal2 не смешивать. Пять вызовов внешнего admission API включают один wrapper-only отказ B. Всего9 предъявленных строк внешнему admission (base1 +четыре партии по2);7 доходят как строки native batch API. Планируемые4 TrainOnExample вызывают до12 учебных AddPair; это прогноз именно учебного пути, не измеренный общий счётчик staging/import/index rebuild. Работу копирования/восстановления индекса записать отдельно по реальным call sites; отсутствие такого показания не превращать в0. Для штатной последовательности приS0=3entries иfinalA/B=6: source-derived12учебных AddPair +9staging-copy (0+3+3+3) +18четырёх Load (3+3+6+6) =39физических AddPair. Источник: StageRuntimeCopy→ApplyCheckpointToRuntime→TranslationKnnStore.ImportCheckpoint, который восстанавливает каждую entry черезAddPair. Это не новый учебный урок и не инструментальное число; actual counter до запуска unknown. Сбой A не обнуляет стоимость обработки урока в staged copy; опубликованных новых уроков после этого отказа0. Нет warmup, дополнительных fits, teacher/model API, DPO/SFT или neural updates.

SaveCheckpointAsync3 раза (S0,A-final,B-final); LoadCheckpointAsync4 (A-S0,B-S0,child-A,child-B). Явные ExportCheckpoint7: producer S0 и по3 состояния каждой ветви (pre/failure/corrected); ещё3 внутренних Export выполняются штатными Save. Никакого дополнительного обучения для snapshots. Таймеры Export/Save/Load учитывать отдельно от Train.

## Что считать результатом

Главная проверка целостности — число незаконно опубликованных уроков/записей после **отказавшей** партии при сохранении старого правильного ответа. На текущем коде A иB должны дать0 и сохранить keep. После corrected ожидаются6 KNN entries у обоих: base3 +один new3. Старый прогнозA9/B6 относился к реализации доa28bc355 и больше не задаёт oracle. Нулевая дельта целостности допустима; возможная дополнительная польза B — отказ до staging и обработки урока, измеряется отдельными calls/elapsed, не объявляется автоматически. Это source-derived ожидания; actual tuple counts и changed keys публикуются независимо. Проверять exact/phrase maps, KNN tuples, structured identities, version/history, HasStructuredTraining; timestamp-only поля помечать отдельно.

Identity oracle задан отдельно: `hoa-base` есть в S0 обеих ветвей; сразу после failure **ни A, ни B не публикуют `hoa-new`**. A добавляет identity лишь в stagedIdentities, которые отбрасываются при ошибке оценки до PublishVersion. После corrected `hoa-new` присутствует по одному разу у обоих. У A обработка первого нового урока была оплачена, но его staged state не становится базой corrected; повторного live AddPair от ошибочной партии нет. `hoa-eval` не является обученной identity. Version/history после failure равны S0, после corrected отражают одну принятую новую партию; HasStructuredTraining уже true от base и не используется как доказательство отсутствия частичной мутации.

Если вместо ожидаемогоS0 после отказа всё же наблюдается частично приобретённый ответA, это регрессия атомарности, а не улучшение качества принятого обучения. Actual ответ и полный payload всё равно сохраняются в таблице, без подгонки oracle. Финальные2/2 A/B, retention keep и fresh-load2/2 обязательны; противопоставление «A может учиться, B только всё отвергает» не проходит.

Любое измеренное улучшение полезного показателя признаётся успехом по этому показателю и теме; рост общей accuracy не обязателен. Рядом указать ухудшения, wall/CPU, allocations, checkpoint bytes, время обучения/записи/загрузки. Для B rejected batch новый parsing входит во время admission; нельзя измерять только старый LearningBatchResult.CpuMs, который исключает ранний preflight. Финальная точность, состояние, ресурс и пригодность переноса — отдельные выводы. Положительный опыт ещё потребует решения пользователя о полном переносе. Успех B не доказывает атомарность при любых ошибках.

## Оснастка и исполнение после назначения

Новые будущие файлы: `HOA01/HOA01.csproj` (net9.0, ProjectReference **`../../../27.TranslationDomain/27.TranslationDomain.csproj`**), `Program.cs`, `ExperimentalHeldOutAdmission.cs`, `Run-Experiment.ps1`; данные входят в `fixtures.json`, отделённый `oracle.json`; результат в новом attempt. Ныне этих файлов нет. Классы native — из указанного проекта27; bootstrap-тексты текущего runtime не выдавать за уроки этой программы.

Перед этим экспериментом будущий исполнитель **заново и полностью** читает текущие AGENTS/CLAUDE и skills в `C:/Projects/llm_mvp_core/docs/skills/`: architect-behavior-classifier, planning-session-startup, plan-review, generated-code-review, prompt-patch-authoring, human-readable-architecture-audit, output-readability-gate, human-communication-GLOSSARY. При написании runner дополнительно code-execution/test-integrity и предметные translation-domain-mindset-reset/translation-domain-reality-check. Имена навыков означают соответствующие существующие `*-SKILL.md`, словарь — `human-communication-GLOSSARY.md`. Усечённая выдача не считается полным чтением. Модель исполнителя — его текущая по умолчанию; Astra исследователь не становится runtime-исполнителем.

Из корня репозитория, после создания назначенного host и проверки SDK9/PowerShell:

```powershell
pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HOA01/Run-Experiment.ps1 -AttemptId attempt-0001
```

Runner обязан restore/build Release один раз, вывести реальные команды/коды выхода, затем один раз запустить producer/A/B и child. Бюджет build≤600с, вся учебная программа≤180с, итог≤15мин. Эти пределы не являются разрешением лишних шагов. Если источник изменился, сначала сверить цепочку; уже исправленный баг не возвращать в продукт. В этой действующей постановке A — текущая native машина со staging; historical A без staging не подставляется ради возвращения положительной дельты. Отдельная историческая репликация потребовала бы собственного явного назначения и не входит в этот бюджет.

Занятый attempt: прочитать manifest/handle. Если это живой запуск — ждать его; если terminal — сохранить и использовать следующий свободный номер только для отдельно разрешённого повторения. Не стирать результат и не превращать autosuffix в самовольный retry. При смешении данных/изменении корпуса/неизвестном типе — остановка этой постановки с точной причиной, не поиск «более удобного» набора.

Ожидание через штатный handle того же процесса, bounded waits≤60с; пустой лог не является неудачей. Не запускать дубль, не уходить из хода словом «жду». Прямое «стоп» немедленно прекращает работу; прямое «следующий» запрещает ещё раз дописывать/исправлять прежний опыт перед переходом. Исторический пример нарушения этого правила не является текущей командой остановки.

Отчёт `report.md` **по-русски**: что выучено; точные ответы четырёх стадий; был ли отказ и что изменилось несмотря на него; acquired/lost; счётчики обучения/чтений; fresh-load; ресурсы; actual terminal всех процессов; ограничения и candidate conclusion. Рядом `metrics.json`, `observations.json`, `states/`, stdout/stderr и source identity. Никакие тесты/чексумы без реального обучения не заменяют программу. Старые P53 результаты и другие карточки не удаляются и не редактируются.

