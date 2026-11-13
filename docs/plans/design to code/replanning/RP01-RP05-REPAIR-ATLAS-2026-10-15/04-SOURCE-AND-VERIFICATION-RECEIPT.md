# 04 · Источники, доказательства и границы авторской проверки

**15.10.2026 · SOURCE-BOUND AUTHOR AUDIT, не независимая Astra-приёмка.** Этот документ фиксирует именно изученные материалы и их границы. Количество файлов, коммитов, содержащихся в них карточек или исходных requirements не выдаётся за количество полностью выполненных проверок.

## 1. Зафиксированные версии и публикация

- P repository: `lubochka/xiigen-planning`, branch `llm_plan3`.
- Исследованный planning HEAD: **`7278c9c24ff3bd3d2f8176914e717b5897409a01`**. Ветка была повторно прочитана непосредственно перед первой записью атласа; HEAD не изменился.
- Parent для фактического сравнения RP02 manifest: **`51298617c26a6b3187bf69fbdc96c6f2f718dbca`**.
- K repository: `lubochka/llm_mvp_core`, branch `codex/dpo-implementation`, исследованный HEAD **`97e4af3eadd3981320f698d104134a01984a2542`**.
- Более старые K `979256ad682e76925286bd6a7365f9fa773959cc` и `405a8a2596dc49dcd9ad6edbb16e59668b73ff0d` в RP01/RP05 являются наблюдениями тех планов, не текущим HEAD. Для проверенных ниже code facts сделано чтение на K97e4.

Директория этого атласа — отдельный reference/audit пакет. Публикация через подключённый GitHub уже создала README (`003ba0b39ad312ad8e0c1081d0025be70616082e`), ordered repairs (`6243861db8b360b79c8730a8406954be64d5d4f5`), work orders (`7952ec98347f7a228f7bcc96f1ca8855e33b44cb`) и domain exams (`2ad0de2853ccf0e4fb64ec07e70ca6d518cbca21`). Собственный commit этого файла виден в Git history; его SHA не выдумывается до записи. Remote readback выполняется по созданным commit/ref, не по локальной копии. Сообщение API об успешном create не является само по себе semantic review.

## 2. Дополнительный P0 остаток, найденный при чтении AGENTS

**Этот finding дополняет ремонт №3 и таблицу остатков в 01; он не означает, что AGENTS уже исправлен.**

На том же P7278:

- [Корневой replanning/AGENTS.md](../AGENTS.md) начинает с **`RP02 ACTIVE V6A · 14.10.2026`**, сообщает **17 actual task cards** и ссылается на V6A binding/workbook.
- [Текущий RP02 PACKAGE-MANIFEST.json](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/PACKAGE-MANIFEST.json) задаёт **V7.2, 18 ID, tasks-v7**, первый θ learner 02B1, 00C только при missing ordinary bridge.
- Root AGENTS также сохраняет RP01 CURRENT V4 ниже RP01 V6 секции и общие original `tasks/<ID>.md` указания.

**Классификация:** `VERIFIED_REMAINING_DISPATCH_CONFLICT`, а не предположение по названию коммита. RP02 runbook даёт текущий локальный приоритет, однако общий inherited agent context всё ещё предлагает другой маршрут. Поэтому утверждение «последний commit окончательно согласовал все входы» не подтверждается.

**Адресный ремонт для planning integrator:** перечитать полный текущий AGENTS и только затронутые dependents; заменить active RP02 navigation на current V7.2/tasks-v7/18 (исторический V6A сохранить reference-only); согласовать RP01 root forwarder с V6/execution; не менять чужие source requirements, права, параллельное владение Rxx или продуктовый код. Проверить root AGENTS→pack README→manifest→actual selected task и обратно. Не добавлять ещё одну противоречащую верхнюю шапку вместо устранения active collision. **Новых product tests: 0.** Для слабой модели пока выдавать проверенный локальный current packet с явным narrow precedence, не весь архив current banners.

Этот ремонт в существующих carriers **не выполнен атласом**: текущее поручение исполнено как ordered repair list и reusable procedure, а не как blanket rewrite RP01–RP05. Не выдавать отсутствие этого изменения за отсутствие write-доступа: создание атласа через GitHub подтверждено реальными commit receipts.

## 3. Изученные исходные требования, RAG и исследовательские результаты

Все относительные ссылки ниже разрешаются из этой директории; immutable audit snapshot — P7278. Для будущего исполнения перепроверять изменившиеся source blobs и смысловые dependents.

| Источник | Что действительно изучалось | Граница |
|---|---|---|
| [original task.md](../../original%20task.md) | Начальная архитектура четырёх ролей, async combinations, принципы authorship/judge | Исторический model/license shortlist не проверялся как актуальная юридическая рекомендация |
| [task prompt.md](../../task%20prompt.md) | Исторический DPO_LAST; полный перечисленный в начале DTC server/HTML/CSS/tree/render/DB/consumer scope | Старые runtime/worktree команды не реактивированы текущим поручением |
| [goal of the round.md](../../goal%20of%20the%20round.md) | DPO и weights storage, cold continuation, reuse already done, человеческая отчётность | Старые local-disk операции не выполнялись |
| [Алгоритм создания ДПО.md](../../Алгоритм%20создания%20ДПО.md) | Начальные design roles, source breadth, алгоритм обучения/измерений, смысл негативной пары и historical process counts | Не полное чтение всех правовых/архивных приложений |
| [WorkLeft MASTER](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) и [usage guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md) | Назначение индекса, relative root resolution, актуальные obligation/status ссылки, восемь training/state commitments | Архивные многосоткилобайтные тела не прочитаны полностью; inventory не равен semantic audit |
| [WorkLeft MECH-01](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md) | Текущая explanatory section и mechanism table: storage/admission/teacher/state/eval/ordinary graph gaps | Исторические code claims не стали runtime evidence сегодня |
| [WorkLeft RESEARCH-03](../../09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md) | R16 conformance delta: exact pair, actors, objective, no replay, state, causal domains | Остальные архивные исследования не перепроверены целиком |
| [V/RAG-06](../../../visual%20studio%20%26%20visual%20studio%20code%20continue/RAG-06-llm-dynamic-flow.md) | Различия design/generated corpus/cached code, ссылки на первичный F, D13–D15 | Старые 156971-file counts не подтверждены как нынешние |
| [F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md](../../../llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md) | Decision, pre-retrieval signals, positive/negative/warning, post-generation arbiter authority, priority boundary | Исторический E51 proof не объявлен проведённым этой сессией |
| [previous rounds thinking results](../previous%20rounds%20thinking%20results.md) | Opening R07 correction и первая программа 25 runs | Не прочитана вся гигантская история; R07 не закрыт |
| [PARALLEL-R01-R03-JOIN](../PARALLEL-R01-R03-JOIN.md) | PARALLEL25 override, nine R10 inputs, scoped R01–R09, source namespaces, shard ownership, semantic receipts | Это чтение договора, не 25 новых вызовов |
| [52 requirements crosswalk](../R02-52-REQUIREMENTS-32-SLICE-CROSSWALK-2026-10-14.md) | Основные таблицы требований и дополнения DTC/IDE/Hebrew/task types/local/privacy/retention | Route map не доказательство всех 52 behaviors и не полное чтение первичных source files каждого ID |
| [32-slice index](../32-SLICE-LIVE-REPAIR-INDEX.json) | Inventory metadata и начальные owners RP01 onward, RP02-REF distinction/current18 | 32/34/33/590 — metadata источника; полный независимый recursive recount всех packs не выполнялся |
| [Root README](../README.md) и [AGENTS](../AGENTS.md) | Начальные управляющие banners и их active conflicts | Не полностью прочитаны все historical overlays этих больших файлов |

**NDJSON depth:** в этой ревизии не выполнен полный MD↔NDJSON↔primary reconciliation всех архивов. Отдельный V/RAG→F primary переход был прочитан; это не даёт права пометить все RAG rows как пройденные. В поручении A следующему выбранному owner требуется точная matching row, если его claim зависит от неё.

## 4. Текущие пакеты и конкретные проверенные execution участки

| Пакет | Прочитанные управляющие/критические материалы | Что подтверждено |
|---|---|---|
| RP01 | PACKAGE-MANIFEST начальные relevant fields; RP01-V6-FAST-LEARNING-CONTRACT-2026-10-14.md current K979256/405a/role/bootstrap/source/minimal-risk sections | 16 ID в manifest; declared direct execution route; conditional repairs; first Student vs bootstrap; роли и источники. Не все16 cards прочитаны |
| RP02 | PACKAGE-MANIFEST at parent512986 и P7278; V7.2-LEARN-FIRST-RUNBOOK; tasks-v7/RP02.02B1.md substantive phase/prompts | Фактическая delta17→18/tasks→tasks-v7; law-conditioned θ route, минимальные признаки, source/type/admission, C0/T, nonblocking02A; карточка сохраняет несколько поколений blocks |
| RP03 | RP03-V6-LEAN-START.md; tasks/v6/RP03.02B.md | V6.3/declared15, первый actual observed lawful Flow signal, отдельные typed/cold/exam lanes; не все15 cards |
| RP04 | README.md полностью в полученном ответе; tasks/RP04.01B.md | V4/declared12; single E3/E4 lane может дать первый exam; current startup INSTALL-AND-START; actor/effect separation; не все12 cards |
| RP05 | README.md; RP05-V4-CODE-PATH-AND-SCIENCE-REPAIR-2026-10-14.md relevant content | V4/declared14; reuse-first feedback, preparedstep/group исправления, scientific/PC/retention; old ACTIVE V3 residual. Не все14 task cards |

**Не заявляется:** полный semantic review всех75 current RP cards, всех590 original tasks, всех user-listed commits, всехS00–S10, всех32 numbered slices или всех external product repositories. Сам список пользовательских commit messages использовался для выбора relevant published materials, не как доказательство их корректности. Полное before/after сравнение подтверждено для указанного RP02 manifest; остальные repairs классифицированы current-contract verified, не придуманными commit-level diff reviews.

## 5. Проверенные текущие code anchors

Все ссылки ниже pinned к K97e4. Перед реальным edit исполнитель читает **весь изменяемый файл**, interfaces, callers и required consumers; прочитанный здесь fragment не заменяет implementation review.

| K path / участок | Blob | Непосредственное наблюдение |
|---|---|---|
| [KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs), Train/ExecuteAsync/Vectorize, две последовательные выборки до конца файла | `2957816a88aa9cdcd3020bb81be56b5dfff165e0` | Train строит BPE/HNSW; ExecuteAsync Search(_k) возвращает bestExample.Output из hits[0]; другие hits используются в evidence IDs; Vectorize складывает token/predicate buckets без explicit order/referent structure |
| [CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs), строки740–800 | `fe0552d48e32be49e35604cc1648d7b9712e6e46` | ExecuteCapturedPreparedAsync содержит refusal для inputs null / DpoStepCount !=1 / QualityGate null; это actual prepared path constraint, не полный аудит trainer |
| [DynamicGraphMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/15.OrchestratorModel/Logical/DynamicGraphMachine.cs), строки1–125 | `8149717b04d442e6e89bb61ab34fb608201ef369` | RuntimeMode honest_untrained; TrainBatch/Evaluate/Export/Import return NOT_ACTIVATED; GetOrchestrator factory не доказывает active learning lifecycle |
| [FeedbackRouter.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/14.ContinuousLearning/Feedback/FeedbackRouter.cs), строки1–130 | `a676058b1ffcaa19dd6385608ec8f1ef242b3822` | Объявленные generic routing semantics: UserCorrection/ModelResponse и no-preferred sentinel; Route switch. В этом fragment не исполнены private RouteEdited/Rejected и не доказано governed Chinese authoring |
| [FileDpoPairStore.Transactions.cs](https://github.com/lubochka/llm_mvp_core/blob/97e4af3eadd3981320f698d104134a01984a2542/14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs), строки1580–1640 | `a46a003a986ab395b342f8fe33f91156b4f583c5` | SelectAtomicTrainingUnits требует двух ordered CriticUnit/ImproverUnit rows для committed group; целый5000-line store не проверен |

Для kNN не установлена полная цепочка всех обычных EN→RU/C# callers. Поэтому «у всей системы только top1» — недопустимое обобщение. Для prepared trainer не выполнялись runtime requests. Для Graph не утверждается отсутствие иных обучаемых policies во всём K; вывод относится к прочитанному Machine lifecycle.

## 6. Навыки, действительно прочитанные и использованные по разделам

Корень: [K/docs/skills @ проверенном SHA](https://github.com/lubochka/llm_mvp_core/tree/97e4af3eadd3981320f698d104134a01984a2542/docs/skills).

| Skill | Полученный участок / blob | Применённый принцип |
|---|---|---|
| trainable-architecture-decision-SKILL.md | Начальные140 строк; `81e96fb8981d4eeb4a3f25f57001e74c453ffdbd` | Named learned owner, signal/state/consumer, сохранение архитектурного смысла; source-scoped historical process requirements |
| phase-planning-SKILL.md | Начальные140 строк; `27ba212c829148158511c7a5a69bc418abfc276c` | Current unquoted task vs evidence, bounded effects, actual chain и зависимости |
| plan-review-SKILL.md | Начальные115 строк; `b99f4ce280f48d0242a35f2ba421a2dc1996399f` | Exact candidate, complete work-order schema, DRAFT is not ACCEPT, исторические pass ledgers не автоматический новый scope |
| generated-code-review-SKILL.md | Начальные100 строк; `eb27d6267115cc96b17a2cd77b601f84f2e6f0b6` | Code-first claims, final SHA, callchain, запрет README/status как runtime proof |
| prompt-patch-authoring-SKILL.md | Начальные115 строк; `063fe9ca26b2a7921279dd803db91ac4e2551c2c` | Один bottleneck, literal WRONG/CORRECT, не перегружать prompt; собственные score bands skill не universal learning acceptance |
| conversation-intent-and-self-reflection-SKILL.md | Начальные105 строк; `3cea7832d4a0e5d447f12f3f010d5fccd7f2759b` | Ответить на текущий запрос о ремонтах, не заменить его новой продуктовой кампанией; ясные границы фактов/планов |

Дополнительные skills `code-examination`, `experiment-design`, `learning-loop-closure`, `dpo-triple-format` включены в **будущие** адресные work orders там, где нужны. Их полное чтение в этой ревизии не заявляется. Следующий executor обязан реально открыть относящийся раздел. Эти шесть прочитанных начал не равны загрузке всей библиотеки skills.

## 7. Как воспроизвести проверку без product CI

1. Read current branch heads; получить содержимое changed sources по immutable SHA и relevant blobs. Сравнить только semantic dependents при changed HEAD.
2. Для подтверждённой delta RP02 сравнить два pinned manifests: current IDs/count/active_task_prompt/execution_contract и historical fields. Проверить 00C и actual tasks-v7 path.
3. Независимо открыть root AGENTS/README: поиск ACTIVE V6A/17 и RP01 V4 не замена чтению контекста, а указатель к выявленной коллизии.
4. Открыть code anchors выше и убедиться, что источник действительно содержит упомянутый механизм; не приписывать runtime наблюдение чтению кода.
5. Прочитать пять файлов атласа после записи по final candidate. Проверить Markdown links/разделы, пять именованных A–E поручений, 16 пунктов ремонта, три основных domain passports, обязательные roles и honest statuses. Это author-side document check; не независимый модельный запуск и не новый software test suite.

## 8. Что не делалось

Не изменялся core; не выполнялись dotnet build/test, Windows workflows, Student/Teacher training/inference, optimizer steps, сбор actual datasets, PC benchmarks, external product delivery. Не вызывался отдельный CODEX/Astra selector/judge/reviewer. Не проверялась юридическая пригодность конкретного внешнего провайдера для актуального запуска. Не доказывалась подгонка tests по истории — для этого нужен отдельный bounded test/requirement/code delta, а не подозрение.

**Статусы:** `PLANNING_ARTIFACT_WRITTEN; SOURCE_FRAGMENTS_REVIEWED; AUTHOR_AUDIT_ONLY; CORE_UNCHANGED; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED; RESIDUAL_SOURCE_DEPTH_OPEN`.

Материальные найденные остатки не скрываются: root RP02 AGENTS17/V6A, root RP01 V4, RP05 ACTIVE V3, многослойные task prompts, отсутствие фактических learning/runtime receipts. Их владельцы и smallest changes указаны в 01/02 и §2 этого файла. Новая публикация этих инструкций не переводит остатки в FIXED.


## 15.10.2026 · RP04 follow-up source receipt (author-side, NOT independent review)

Additional **K HEAD** directly fetched: `97e4af3eadd3981320f698d104134a01984a2542`. Verified `15.OrchestratorModel/Logical/DynamicGraphMachine.cs` (blob `8149717b04d442e6e89bb61ab34fb608201ef369`, lines 22–77); `DynamicGraphLogicalOrchestratorCompositionExecutorAdapter.cs` (blob `6f2702b1a10648de82d2f0e22327dfb243dbad0b`, lines 144–255); `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs` (blob `cca8e27b703559338aefa42f14e63d2b8c520d18`, lines 27–28 and 232–276); `00.Core/DynamicGraphContracts.cs` (blob `987c29396fab838db0eecd8060837d5e5e33bce4`), `LearningMachineContracts.cs` (blob `7f605d49762e3fc0c1b4d4ca490596bc9668cdd5`), plus source leads for L3 selected-target executor. Current RP04 12 task cards read before edit and modified in-place. [Exact five new findings, qualifications and falsifiers](05-RP04-F17-F21-NEW-VERIFIED-FINDINGS.md). This was bounded primary-source/code review, **not full-file reads of all WorkLeft and R01–R25, not executed K training, not actual CODEX/Chinese calls, not an independent Astra review or unseen-domain evaluation**.
