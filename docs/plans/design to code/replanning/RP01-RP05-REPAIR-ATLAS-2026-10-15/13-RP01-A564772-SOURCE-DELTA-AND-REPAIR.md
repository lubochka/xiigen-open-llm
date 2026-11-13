# 13 · RP01: восстановление C0, смысл повторов и цена первого урока

15.10.2026 · AUTHOR_SOURCE_AND_PROMPT_REVIEW / DOCUMENTARY_DRAFT. Продолжение ранее опубликованного промежуточного finding, не новый R01–R25 и не дополнительные обязательные раунды. Рабочие dispatcher и карточки исправлены непосредственно. Продукт, модельные вызовы, обучение, private datasets, PC settings и independent Astra review этой публикацией не выполнялись/не изменялись.

## Что проверено и что изменено

Начальная версия этого файла опубликована в `d507493cb54a60bfff9e48e9bab5435e9c3d85bf`; она содержала уже найденный, но ещё не применённый ремонт RP01-A564-01. Этот пункт не выдаётся за новое открытие текущего продолжения. Core read snapshot K=`a564772ab713aae071b27c513a9bad5a1a775a5e`, parent=`025c42c74b1d30ffa4660f9824a52cf8fcaf7c88`.

Вместо ещё одного общего обзора согласованы [dispatcher](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-EXECUTE-NOW.md), [manifest 3.2](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/PACKAGE-MANIFEST.json), [00B](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/execution/RP01.00B.md), [03A](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/execution/RP01.03A.md), [T0](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/children/RP01.03A-T0-CONTROLS.md), [T1](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/children/RP01.03A-T1-UPDATE.md), [T2](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/children/RP01.03A-T2-EXAM.md) и [source map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md). Остальные карточки не объявляются заново полностью проверенными.

| Запись | Класс | Результат этого продолжения |
|---|---|---|
| RP01-A564-01 | Ранее подтверждённое противоречие plan/code | In-place ремонт: поддержанное pre-executor C0 recovery больше не скрывается за общим partial-intent catch-all |
| RP01-A564-Q1 | Проверенная и НЕ подтвердившаяся гипотеза дефекта | Full cached-verdict prevalidation существует и вызывается до новых calibration effects; REUSE_NO_CHANGE |
| RP01-A564-02 | Новое source-specific уточнение методики к прежним требованиям независимости | Одинаковый requested seed и repeat IDs не доказывают independent Student sampling; T2 получил точный порядок интерпретации existing raw |
| RP01-A564-03 | Распространение существующего core accounting contract в исполнимый план | Time-to-first-update и logical/new/reused/historical-UNKNOWN разделены; не новое открытие числа 120 и не новый runtime bug |
| RP01-A564-04 | Адресная оптимизация порядка работы карточки | 00B сначала читает состояние/переиспользует handoff, затем исследует нужный source edge, не весь trainer перед каждым T2 |

Таким образом, здесь НЕ заявлены пять новых дефектов продукта, пять выполненных опытов или пять новых prerequisites. Отрицательный результат source-проверки тоже сокращает ненужную работу.

## Неподменяемые роли

CODEX с запрошенной моделью `astra 6 ultracode` выполняет делегированную человеческую роль выбора учебной задачи. Допущенная внешняя открытая китайская модель создаёт ОБЕ окончательные DPO-стороны, chosen и rejected. Отдельный CODEX с `astra 6 ultracode` оценивает точную неизменённую пару. Независимый экзамен Student — отдельное назначение CODEX с `astra 6 ultracode`.

Native Qwen evaluator не заменяет J или независимый T2. Requested model label, actual runtime ID, genuine receipt и effect grant — разные evidence. Никакое поле документа не выдаёт права на модельный вызов. Валидная пара не обязана улучшить Student; отрицательный результат опыта не разрешает переписать judged pair под чужой provenance.

## RP01-A564-01 — применён поддержанный recovery route, без повторного урока

**Evidence:** [scientific lifecycle, wrappers/admission/BeginScientificHold](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/26.TrainableRouter/Training/CodeDocumentationScientificTrainingLifecycle.cs#L27-L110); [coordinator](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs#L300-L660); [supported CLI](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/tools/Rp01RawDpoLesson/README.md).

Старый dispatcher и physical 00B/03A/T0/T1 сводили partial intent к общему recovery. Новая ветка различает:

| Состояние | Следующее действие |
|---|---|
| Intent чисто отсутствует, inputs/права готовы | Один fresh `train-and-hold` |
| Тот же intent/source/policy/parent/pair; исходный C0 частичен или полон; journal owner подтверждает pre-executor eligibility | Один explicit `resume-baseline-and-hold` с повторной server admission |
| Canonical attempt/claim/journal/Trial либо post-update partial state, drift/orphan/sibling conflict/UNKNOWN | Existing recovery owner; НЕ baseline-resume и НЕ fresh replay |
| Complete held | T2 над исходными raw, без нового T1 |
| Same Trial уже published | Только оставшийся cold-consumer этап, без второго publish/update |

**Точная команда** находится в T1: `dotnet run --no-build --project tools/Rp01RawDpoLesson -- resume-baseline-and-hold <same-repo-root> <same-intake.json> <same-intake-sha256> <same-loopback-uri> <same-training-run-id>`.

Это не read-only просмотр: после допустимого C0 команда может дойти до ПЕРВОГО optimizer и собрать Trial. Реальные training/evaluation/resource grants сохраняются; источник/code review не гарантирует прохождение native/baseline guards. Ни отсутствие canonical файла в GitHub, ни cancel сами по себе не доказывают, что optimizer на PC не запускался.

**GOOD:** пять exact C0 raw сохранены; новый explicit route подтверждает исходные условия, переиспользует raw/results, завершает только отсутствующие slots, затем достигает существующего executor при допуске. **BAD:** удалить intent/sidecar, сменить run ID или назвать resume безопасной диагностикой. **EDGE:** complete C0 без canonical attempt тоже допускается только через owner eligibility; canonical attempt без held уже не эта recovery ветка.

**Prompt/acceptance:** physical T1 содержит inputs, actual source/methods/skills, командные альтернативы, права/запреты, GOOD/BAD/EDGE, стоимость, failures и return schema. Проверять согласованность fresh/pre-executor/post-attempt/held/UNKNOWN между dispatcher, manifest и physical cards. Ноль новых product tests и ноль model calls для документного ремонта. Eligibility actual PC остаётся NOT_OBSERVED.

## RP01-A564-Q1 — suspected cached-validation gap НЕ подтверждён

**Вопрос:** может ли поздний invalid cached evaluator result быть обнаружен только после дорогих новых calls в ранних отсутствующих slots?

**Проверенная цепочка:** `BeginScientificHold → AdmitRetainedBaseline → coordinator.ValidateRetainedBaselineAsync → CalibrateBaselineAsync → executor`. В [assessment service](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs#L590-L710) retained validator обходит все исходные C0 slots и использует обычный `EvaluateCaseAsync` с non-null cached result, без выдачи permit. Проверяются profile/provenance/parser и повторённые request IDs по full C0 set. [Actual caller](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs#L535-L585) расположен до calibration. После complete baseline capture переключается на ordinary store для Trial.

**Вывод:** REUSE_NO_CHANGE для этой гипотезы на указанном reached route. Не создавать второй validator, дополнительные tests или новый план до урока. Это source-bound вывод, не runtime certification всех entry points, filesystem races или provider.

**GOOD:** invalid сохранённый verdict в позднем slot вызывает отказ full prevalidation до новых measurements. **BAD:** прочитать только storage deserializer, не найти там parser и объявить parser отсутствующим во всей цепочке. **EDGE:** другой route/code delta требует проверить именно его caller, а не автоматически переносить этот вывод на весь проект.

**Если фактический runtime противоречит source:** сохранить trace exact method/branch и последний подтверждённый effect; условный 01B получает узкое задание на реальный разрыв. Не запускать synthetic suite ради доказательства уже существующей функции.

## RP01-A564-02 — одинаковый requested seed не становится независимым Student sampling

**Evidence:** [calibration repeat loop](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentCalibrationService.cs#L190-L285) меняет только AssessmentRunId в record copy; [GenerateStudentAnswer](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs#L548-L590) передаёт прежний settings.Seed. Это новое точное source основание для сужения научного claim, а не доказанная runtime ошибка или факт одинаковых реальных ответов.

**Задание T2:** по existing raw hashes для каждой family/arm описать пять slots, число уникальных hashes и кратности. Сохранить blind scoring, original ordered references и frozen aggregation. После scores связать descriptive summary с исходными settings. При разрешённом доступе отдельно показать вариацию Student raw и native grader verdicts. Не использовать разные run IDs как доказательство разных seeds; не использовать zero observed variance как доказательство универсальной точности. Разные hashes не доказывают разные semantic solutions или независимые families.

**GOOD:** пять одинаковых raw отмечены как одна уникальная строка в пяти slots; полный frozen scorer сохранён, independent family N=12. **BAD:** N=60 или «5 independent Student seeds» только из five repeat labels. **EDGE:** один seed дал разные raw — это наблюдение с неизвестными возможными runtime источниками вариации, не повод исправить артефакты.

**Repair/acceptance:** обновлена physical T2, dispatcher, manifest и source map. Companion evidence не добавляется в strict scientific-verdict JSON. Не менять seed, не удалять дубликаты и не отменять существующие 5 повторов в начатом опыте. Новый seed-robustness/efficiency experiment — только prospective работа своего владельца; он не prerequisite первого урока. Фактическая повторяемость/вариативность остаётся NOT_MEASURED.

Методический внешний ориентир: [официальное руководство scikit-learn о randomness](https://scikit-learn.org/stable/common_pitfalls.html#controlling-randomness) разделяет воспроизводимость и исследование вариации RNG. Это не доказательство, что XIIGEN использует sklearn или имеет его поведение; вывод о текущем XIIGEN seed берётся только из приведённого K-кода.

## RP01-A564-03 — учитывать цену ПЕРВОГО update, а не только полного цикла

Core README уже различает 120 logical outcomes и physical calls. Новизна здесь — перенос этого контракта в реальные T1/T2/manifest instructions и разделение стоимости до optimizer и после него; не новое открытие числа 120.

**Source:** [PlanQualityGate/CalibrateBaseline](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs#L100-L220); [baseline перед executor](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs#L535-L660); [store dispatch/result accounting](https://github.com/lubochka/llm_mvp_core/blob/a564772ab713aae071b27c513a9bad5a1a775a5e/14.ContinuousLearning/Training/CodeDocumentationPrivateEvaluationStore.cs#L18-L175).

Fresh baseline имеет 12×5=60 logical results до первого optimizer, полный C0/Trial — 120. Legacy ObservedEvaluationCallCount включает observed request IDs полного набора, в том числе reused results; это не necessarily новые current-invocation issued calls. Дополнительный independent CODEX exam — другой actor/effect.

**Задание T1:** вернуть отдельно actual NewDispatchIntents, NewCallsIssued, ReusedRawAnswers, ReusedEvaluatorResults, CompletedLogicalResults, HistoricalEvaluatorCalls/UNKNOWN; Student generations только по их собственной evidence. Сохранить unreturned dispatch как UNKNOWN external effect, не zero. Из actual timings показать admission/load/reuse, remaining C0, first optimizer, Trial/held; later T2/T3 добавляют time-to-accepted. Missing metric NOT_MEASURED. Не запускать новые calls для заполнения счётчика.

**Публичные примеры при полном допущенном завершении без новых сбоев:**

| Сохранённый исходный C0 | Новые Student generations | Новые native evaluator calls | Исторические calls |
|---|---:|---:|---|
| 5 exact raw, 0 durable evaluator results | 55 C0 + 60 Trial | 120 | UNKNOWN, если нет receipts |
| 5 exact raw и 5 допустимых durable результатов | 55 C0 + 60 Trial | 115 | Отдельно по genuine evidence; не угадывать |

Это арифметические иллюстрации, не замер пользователя. Grants/guards могут отказать раньше; crash может добавить неизвестные effects. CODEX S/J/T2 в эти числа не включены.

**BAD:** назвать complete logical120 «120 вызовами lifetime», исключить failed attempts или выводить PC speed из timestamps commits. **EDGE:** сохранены все60 C0 results до canonical attempt — generation C0 не повторяется, но actual eligibility/native guards по-прежнему обязательны. Если actual metric отсутствует, не строить estimator «на глаз».

**Приоритет:** немедленно устраняется повтор совместимых результатов, не ослабляется fixed science. Изменение числа повторов/порядка дорогих checks возможно только отдельным prospective evidence-based решением владельца, а не скрытым обходом текущего native gate. Никаких новых Windows/coverage rounds.

## RP01-A564-04 — state-first 00B вместо повторного чтения всей цепочки

В прежней physical 00B подробная source-трассировка стояла раньше определения existing intent/held. Это могло превращать даже простой переход к T2 в повторный обход optimizer/DI/consumer. Класс — локальная prompt inefficiency, не measured execution cost.

**Ремонт:** 00B сначала проверяет разрешённую metadata и пригодный previous handoff, выбирает ближайшее состояние, затем открывает changed/missing source edge этой ветки. Полная карта `pair → mask/objective → learned state → Trial/hold → publisher → ordinary consumer` сохраняется как требование доказательства всего контура, но не пересоздаётся на каждом повторном входе. Unknown часть получает точного owner; частичная карта не называется full ROUTE_VERIFIED.

**GOOD:** complete held и прежняя пригодная code map есть → verify metadata и T2 entry, не новый trainer audit. **BAD:** перечитать весь optimizer до того, как увидели сохранённый complete held. **EDGE:** source delta меняет generation/scoring семантику → inspect эту delta; административный recovery commit не даёт автоматического утверждения полной semantic equivalence.

**Проверка:** одинаковый held/UNKNOWN/pre-executor/post-attempt input выбирает то же действие в 00B, 03A, T0/T1, dispatcher и manifest. Отсутствие PC-доступа не заменяется придуманным handoff. Проверка документа не выполняет модельного эффекта.

## Функциональный объём не сужен до первого C# урока

[03 — domain exams/functional crosswalk](03-DOMAIN-EXAMS-AND-FUNCTIONAL-CROSSWALK.md), [02 — owner-specific work orders](02-REVIEW-WORK-ORDERS.md), [11 — existing refinements 47–58](11-LEARNING-FIRST-REPAIR-DETAILS.md) сохраняют исходные requirements и owners. Они не превращены в новую serial gate queue.

| Способность | Что должно быть доказано её владельцем, а не объявлено результатом RP01 |
|---|---|
| DPO positive/negative | Exact selected input, обе genuine Chinese стороны, отдельный CODEX J, допустимый реальный update и independent family-level utility; loss/receipt не достаточно |
| Domain mapping, vectors, dynamic kNN | Правильный domain/context, typed features и applicability законов, positive/negative/warning memory, learned score/policy и реальный consumer; больше соседей не означает domain reasoning |
| C#, EN→RU, SQL | Разные независимые domain families и ошибки по предзаданным законам, а не один pooled score. Перенос mixed syntax/referent/NULL-like tasks оценивается своими frozen passports |
| LoRA и semantic projection | Раздельные recipients/objectives, реальные tensor/state deltas, versioned input/output representation и typed downstream consumption; same vector dimension не доказательство semantics |
| Unit/Flow | Пользовательский many→many контракт и обучаемая correction вход→выход; не collapse к одному первому candidate и не фиксированный rule под именем learning |
| Orchestrator/Graph | Async completion/cancellation/partial outcomes, higher-order patterns и actual learned path use; выбор только внутри fixed pool не доказывает создание новых путей |
| Continual/PC/DTC/IDE | Уникальные новые feedback lessons, retention, feedback chronology, time/accuracy bulk-to-bulk при сопоставимом workload, full PC cost и ordinary-user adoption |

**Публичные иллюстрации требований, не authored DPO/SEALED cases:** C# path должен учитывать syntax/type context, а не только близость tokens. В языковом контексте `Мария сказала, что Антон пришёл` смена рода относится к другому субъекту; общий запрет мужского рода после упоминания женщины был бы плохим rule. SQL-паспорт должен явно задавать dialect/schema и применимые законы, а не переносить валидность по похожим словам. Эти примеры направляют владельца RP02; новая готовность этих доменов здесь не заявлена.

В RP01 C0→Trial меняется только разрешённый Student state. RP02 domain-law/kNN может идти параллельно, не дожидаясь RP01 gain, но не подменяет часть уже начатого RP01 comparison. Для доказательства гибрида необходимы собственные matched-budget contrasts при сохранённых hard laws, actual exposure/consumer и разграничении state-change, improvement, promotion и adoption. Ни один из этих broader claims не закрывается checksum или green Windows run.

## Read scope, skills и воспроизводимая документная проверка

Непосредственно прочитаны relevant current source ranges из ссылок выше и полные тела изменённых physical 00B/03A/T0/T1/T2, dispatcher/manifest/source map и предыдущего этого файла. Перечитан WorkLeft RESEARCH-03 lines1–90, current R16 obligations отделены от исторического тела. Earlier previous-rounds/JOIN/RAG-06/ARBITER evidence из d507493 сохранена как earlier review, не приписана этому продолжению как полный новый аудит всех RAG/S00–S10/R01–R25 или всех 75 RP cards.

Skills на K a564772: trainable-architecture-decision 1–200, phase-planning120–255, plan-review1–160, code-examination1–160, prompt-patch-authoring1–180, human-readable-architecture-audit1–150. Применены bounded task, source/caller/consumer, exact-candidate evidence, good/bad/edge и readable communication. Legacy quotas/automatic 30–50 passes не добавлены.

Документная проверка: открыть remote branch и изменённые sections; manifest 3.2 должен иметь прежние16 tasks и9 core children, без новых serial rounds, no-new-Windows/coverage flags; fresh/recovery/post-attempt/held/UNKNOWN совпадают между manifest и physical cards; T2 companion fields не добавлены в strict verdict schema; role contract сохранён. Product tests/model calls для этой проверки — 0. Independent expert acceptance не выдается самим автором.

Основные уже выполненные in-place commits: `833274f` dispatcher, `c535e3e` T1, `58d2fde` parent03A, `8ea9d7d` T0, `f6f4118` 00B, `6746171` manifest, `862fd27` T2, `4c9a6af` source map. Финальная branch/readback проверяется после записи этой версии; эти references не доказывают обучения.

**Граница:** PLANNING_REPAIR_PUBLISHED, не PRODUCT_FIX_COMPLETE. Runtime eligibility, actual training/gain, independent CODEX `astra 6 ultracode` invocation, all-domain improvement, PC performance и cold adoption остаются NOT_OBSERVED/NOT_PERFORMED/NOT_MEASURED. Из реально проверенного достигнут короткий state-aware путь к существующему learning mechanism без выдуманных новых tests и без скрытого повторного урока.
