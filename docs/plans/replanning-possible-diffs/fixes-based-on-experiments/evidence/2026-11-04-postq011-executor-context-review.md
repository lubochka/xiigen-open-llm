# Адресные остатки после POSTQ011: Q02, SFT restore и смысл измерений

4 ноября 2026. Исполнитель исследования: `/root/postq011_card_gaps`. Фаза — **READ_ONLY_RESEARCH + назначенный Markdown**. Это дополнение к [158](../codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md), [159](../codex-159-postq011-addressed-repair-plan-2026-11-04.md) и [покрытию 160](../codex-160-postq011-card-coverage-2026-11-04.md), а не повторный аудит всех 22 усиленных карточек. Новый FIX, новый PRIMARY и новый научный результат не назначены.

В наряде был указан `a78e55c6617`; фактическое чтение Git показало HEAD `8109a63aa2af9292746959751948ca7dc07c012c` и пустой `git status --short` **до записи этого файла**. Поэтому выводы ниже привязаны к прочитанным bytes и строкам; более старые source-статусы карточек не объявляются текущими автоматически. Q01 принят по сохранённой приёмке 29/29; этот worker её заново не выполнял. Следующий перенос Q02→Q03→Q06, Q04 приостановлен, Q05 зависит от него. Научная QA — 11/131; Q012 не назначен.

## 1. Q02: новое нормализованное RULE и сохранённое legacy RULE — разные ожидания

**Существующий владелец:** `135-R09-PAIR`, [135 §R09](../codex-135-post113-addressed-repair-plan-2026-11-02.md), [127 §M03](../codex-127-postmerge-addressed-repair-plan-2026-11-02.md). Уже назначенные canonical key, collision, reached-candidate и loaded-target проверки сохраняются; они не являются новой находкой этого обзора.

**Нынешний конкретный разрыв.** В 135, строка 208, ожидается `RULE:TENSE_PAST`. В [TranslationPairTailSignatureLearningTests.cs](../../../../../Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs), строки 73–84, fixture подаёт `VerifiedRuleTags=["TENSE_PAST"]`; строки **158 и 593** требуют эту же форму уже после настоящего builder. Однако:

- [TranslationVerifiedRuleTags.cs](../../../../../27.TranslationDomain/TranslationVerifiedRuleTags.cs), строки 27–30, 142–145, задаёт единственную новую форму `TENSE=PAST/PRESENT`.
- [TranslationStructuredBatchBuilder.cs](../../../../../27.TranslationDomain/TranslationStructuredBatchBuilder.cs), строки 115–122 и 605–610, вызывает этот нормализатор и сохраняет знак `=` при добавлении `RULE:`.
- [TranslationVerifiedRuleTagTests.cs](../../../../../Tests/ExperimentFixes/TranslationVerifiedRuleTagTests.cs), строки 84–104, 145–160, 207–220, независимо закрепляет новый результат `RULE:TENSE=PAST`, сохранение verified annotation при изменённом rejected и сведение трёх входных написаний к одному.

Это **статически установленное несовместимое ожидание**, не свежий failed test. Не следует чинить продукт обратно к старой строке или подставлять observed actual в gold без договора. При назначенной Q02 QA исправление такого устаревшего assertion входит в задачу по [46](../../experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md); затем обязательны сборка, discovery и фактические результаты соответствующей сборки.

**Почему нельзя заменить все подчёркивания на `=`.** [TranslationSemanticKey.cs](../../../../../27.TranslationDomain/TranslationSemanticKey.cs), строки 53–76, удаляет только отдельные `PAIR:` segments и намеренно сохраняет остальные. Поэтому legacy helper-cases в PAIR tests, строки **165–170**, законно сохраняют исходное `RULE:TENSE_PAST`. Generated-new и legacy-preservation должны иметь разные независимые expected. Политику нормализации legacy RULE при реальном импорте нужно решить и проверить у Q02 owner; наличие нового tag normalizer само по себе не доказывает миграцию всех old maps.

**Готовая короткая правка в 135/R09 и адресный вход 127/M03:**

> Новая verified annotation проходит TranslationVerifiedRuleTags и записывается как `RULE:TENSE=PAST/PRESENT`; rejected не является источником этой аннотации. Отдельно проверить сохранность допустимого legacy RULE при удалении PAIR: простой Canonicalize не обязан переписывать старые tag bytes. В TranslationPairTailSignatureLearningTests разделить expected после настоящего builder (158/593) и сохранение legacy segments (165–170). Не ослаблять RULE до отсутствия тега или любого исключения. После обоснованной адаптации fixture выполнить весь назначенный native gate на свежей сборке.

**Положительный/отрицательный случаи.** Положительный: verified past и verified present сохраняют собственные разные факты; canonicalized provenance не теряет legacy RULE. Отрицательный: смена rejected не меняет verified past; конфликт past+present даёт `VERIFIED_RULE_TAG_TENSE_CONFLICT`; missing annotation остаётся законно пустой, а не восстанавливается из rejected. Это уже заданные source-cases в `TranslationVerifiedRuleTagTests`, строки 121–181 и 249–258. Прежний полезный перевод, source-floor и sensitivity загруженной карты из 127/135 остаются обязательными.

Сохранённый [P106 report](../../../../../experiments-v2/RP01-MICRO/P10601/results/attempt-0001/report.md), строки 17–26 и 44–61: исторически обучался только semantic-tokenizer; точные строки стали 2/2, варианты с точкой остались 0/2, после reload 2/2. Это не доказательство ответа всей translation machine и не разрешение тихо обучить executor/verifier заново. Исторический [before-run-blocked](m03-pair-key-migration/before-run-blocked-2026-11-02.md) сохраняется как свидетельство запрета прежней среды, а не текущий запрет назначенной QA.

## 2. Q02 и TR07/M001: отдельная предметная панель, а не готовый native DPO-вход

**Пробел связи:** [learning-sets 158](../codex-158-learning-sets-transfer-coverage-2026-11-04.md), строки 11, 48 и 63–77, связывает наборы с 13/17/20/48/49/62/74, но не уточняет ближайший Q02. Имя `dpo.train.jsonl` не выбирает получателя обучения.

**Лично прочитанные данные:** [accepted.jsonl](../../../../../learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl) содержит 10 строк TRAIN; первая — `TR07-OMISSION.ru-en.M001.F0000001.P01`: полный prompt задаёт кухонный контекст и восстановление одной пропущенной буквы, `source_raw="Молко стоит в холодильнике."`, chosen=`The milk is in the fridge.`, rejected=`The juice is in the fridge.`. `canonical_source` и `canonical_token` — метаданные эталона, не новый learner input.

[accepted.acceptance.json](../../../../../learning-sets/TR07-OMISSION/ru-en/M001/accepted.acceptance.json), строки 3–5 и 16–18: `DATA_REVIEWED_NOT_NATIVE_TRAIN_READY`, `DATA_ONLY`, native admission `NOT_CHECKED`, training `NOT_RUN`. [export.json](../../../../../learning-sets/TR07-OMISSION/ru-en/M001/export/export.json), строки 2–9: `INTERCHANGE_FORMAT_ONLY`, native import false, 10 TRAIN. Файлы `dpo.dev.jsonl` и `dpo.exam.jsonl` фактически пустые. [Отчёт подготовки](../../../../../learning-sets/TR07-OMISSION/ru-en/M001/report.md) отделяет review 10/10 от результата модели. Новый review этой сессией не выполнялся.

**Настоящий Q02 recipient:** `TranslationStructuredBatchBuilder.BuildRuEnGeneralBatch` → `TranslationMachine.TrainStructuredBatchAsync` → назначенные semantic/executor/verifier maps → `ExportCheckpoint` → новый `TranslationMachine.ImportCheckpoint` → `GetOrchestratorAsync`/обычный `RunAsync`. Уже существующий PAIR test, строки 625–682, читает именно target и проверяет `FinalHumanResult`, `TRANSLATED`, adapter identity и неизменность checkpoint после чтения. Это не маршрут `DpoTrainer.StepVector`, не AdamW и не LoRA.

**Конкретная граница формата.** `TranslationTrainReadyPair`, builder строки 12–28, требует `id/source/direction/pair_family/a_side/validation_outcome/validation_errors`, необязательные verified rule tags. `BuildGeneralBatch`, строки 88–100, требует direction и `train_ready`, затем токенизирует **ASide**. Строки 130–155 строят три typed unit lessons. Обменный export содержит лишь prompt/chosen/rejected, а accepted имеет другую схему и иной смысл review-status. Ни один из них нельзя объявить подходящим native DTO путём изменения названия файла. `Prompt` и контекст задачи нельзя молча потерять: непосредственный native input в этой ветке — ASide.

**Готовая короткая правка в learning-sets158 и ссылочный вход Q02:**

> Для `135-R09-PAIR` TR07-OMISSION/ru-en/M001 — только будущая дополнительная предметная панель. До запуска закрепить отдельную projection с hashes исходных accepted bytes, pair/family IDs, выбранными строками, direction, ASide, политикой контекста, review→native admission и независимым expected полного перевода. Не присваивать `train_ready` только по accepted filename. Не подавать canonical_source, rationale, error_tag или gold-метаданные во вход. Если проекция оставляет только source_raw и тем самым убирает исходный контекст, назвать её отдельной проверкой exact-source memory/readback; не выдавать за исполнение исходной contextual typo-задачи. Сохранить chosen/rejected whole-answer contrast milk/juice и `translation_only`. Новая панель не заменяет обязательные RULE, legacy/import, source-floor и collision случаи Q02; 10 TRAIN без EXAM не доказывают перенос на новые случаи.

Положительный будущий контроль: exact разрешённый материал через действительный recipient, затем новый loaded target выдаёт целый правильный ответ и сохраняет state. Отрицательные: неправильный target `juice` при сохранённом correct ID; неверный/утраченный pair provenance; повреждённая нужная imported map. Наличие слова `milk` при неправильном остальном переводе не закрывает whole-answer contract. Эти проверки здесь **не запускались**, данные/проекция не создавались. Пригодность этого набора для Q02 не установлена до указанной проверки input contract.

**Q03 /135-R02-EXECUTION** получает отдельное указание: его consumer — lifecycle runner, а учебный набор **N/A для этой приёмки**. [P10801/Run-Experiment.ps1](../../../../../experiments-v2/RP01-MICRO/P10801/Run-Experiment.ps1), строки 68–93, использует общий terminal для build/run; этот уже известный остаток проверяется назначенным dummy child без модели. TR07 не должен стать скрытым уроком в проверке build/run/timeout. Его будущее предметное применение относится к Q02/своему learner-owner, не к Q03. Новая Q03 реализация здесь не выполнена.

## 3. SFT continuity: донести уже назначенный договор 159 до собственного consumer

**Существующий владелец:** Q04/FIX-MC, [21](../21-sft-optimizer-continuity.md), [E-SFT-MOMENT-CONTINUITY](../../experimets/cards/E-SFT-MOMENT-CONTINUITY.md). Это не новый numeric repair: решение уже есть в 159 §2. Остаток — точная применимость к собственным SFT states и тексту fixture.

**Ошибка недоговорённого входа.** Карточка E-SFT-MOMENT-CONTINUITY, строка 189, объединяет FromBytes, format/key/shape/finite validation и successful native import. [AdamW.TryImportPortableState](../../../../../04.TrainingLoop/Optimizers/AdamW.cs), строки 234–290, по текущему исходнику проверяет format/keys/shapes, копирует массивы и swaps state; finite m/v и v≥0 здесь не проверяются. [Сохранённый native-result-002](../../../../../experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/native-result-002.json), строки 81–96, показывает для empty/nan `NativeAccepted=true`, `FullStateAccepted=false`, изменённый native digest. Это свидетельство Q007, не новый запуск и не свидетельство испорченного MC01 input.

**Свой законный контраст MC нельзя заменить чужим.** [MC01 report](../../../../../experiments-v2/RP01-MICRO/MC01/results/attempt-0001/report.md), строки 28–30 и 44–47: A перед вторым уроком Step0/0 entries, B Step1001/16 entries. Saved `pair-summary.json` и A/B `metrics.json` сохраняют этот factor, bytes и step. Q007 Step401/4 entries не является MC contract. [AdamWOptimizerStateRecoveryTests](../../../../../Tests/AdamWOptimizerStateRecoveryTests.cs), строки 286–309, дополнительно сохраняет общий законный fresh Step0 и sparse одного parameter при двух bindings. Глобальный запрет empty или moments для всех bindings поломает положительные договоры.

**Готовая короткая правка в адресный верх 21 и E-SFT-MOMENT-CONTINUITY:**

> Применять уже назначенный 159 §2 раздельно к intrinsic numerical validity и trusted completeness собственного continuation. Для MC сохранить A Step0/empty как фактор reset; B допускается по независимому контракту его исходного Step1001/16-entry bundle. Не копировать Q007 Step401/4 и не брать expected keys из предъявленного мутанта. Успех FromBytes/внешнего guard не равен отказу actual AdamW/actual SFT restore. В назначенном узком ремонте подтвердить native refusal NaN/Infinity/v<0 и missing-required-entry до смены старых moments/step/RNG; законные fresh/sparse positives сохранить. Admission/readback-контроли используют saved states/portable DTO, без H512 preparation и повторного второго урока. Общий FIX-MC остаётся приостановлен.

Положительный сохранённый опыт остаётся положительным по NLL/удержанию preference-sign. Отрицательные numerical controls — будущая приёмка существующего owner, не результат MC01 и не новая очередь. Формулы обновления, данные уроков и исходная доза не меняются.

## 4. FIX-MC: фразовый индикатор свободной генерации ошибочно получил вид полной accuracy

**Точный адрес:** 21, строка 16, сейчас говорит «Свободная генерация 0/16 → 0/16». В исходном MC01 report, строка 76, число относится к **наличию фразы `Returns the`**, а строка 58 — к teacher-forced target positions. Это разные измерения. Наличие фразы само по себе не является рубрикой correctness полного свободного ответа.

Лично разобраны сохранённые `generation-0000.json`…`generation-0003.json` и `evaluation-receipt.json` во всех четырёх папках под `experiments-v2/RP01-MICRO/MC01/results/attempt-0001/`:

| Состояние | Реальные RowIds | Generation files/completed | Contains `Returns the` | qualityVerdict |
|---|---|---:|---:|---|
| `A/evaluation/A0` | D1,D2,D3,D4 | 4/4 | 0/4 | NOT_GRADED_BY_CODE_EXECUTOR |
| `A/evaluation/A1` | D1,D2,D3,D4 | 4/4 | 0/4 | NOT_GRADED_BY_CODE_EXECUTOR |
| `B/evaluation/B0` | D1,D2,D3,D4 | 4/4 | 0/4 | NOT_GRADED_BY_CODE_EXECUTOR |
| `B/evaluation/B1` | D1,D2,D3,D4 | 4/4 | 0/4 | NOT_GRADED_BY_CODE_EXECUTOR |

Это выполненный **read-only пересчёт 16 существующих JSON**, а не генерация/оценка модели. В каждом raw stopReason=`LENGTH_LIMIT`. Пример [A1 generation-0000](../../../../../experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/evaluation/A1/generation-0000.json), строки 2–8, сохраняет настоящий D1 text, 64 tokens и stop; [A1 receipt](../../../../../experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/evaluation/A1/evaluation-receipt.json) отделяет scores/margins от `qualityVerdict`. Полной независимой semantic рубрики этот пересчёт не создаёт.

**Готовая короткая замена для21 и его текущей сводки:**

> В сохранённых 16 генерациях суммарно (A0/A1/B0/B1, по4) фраза `Returns the` отсутствует: по0/4 в каждом состоянии. Это фразовый индикатор, не новое измерение whole-answer accuracy. Teacher-forced TRAIN33/33 и preference-sign EXAM A3/4→2/4, B3/4→3/4 показываются отдельно. Нового правильного свободного ответа по этой метрике не доказано; full-answer evaluation не присваивается счётчику phrase occurrence.

## 5. Допуск и границы сделанного

Ресурсный договор остаётся [69](../69-transfer-admission-and-priority-2026-10-27.md), строки 35, 46–55: приоритет полезного выигрыша строго >5%, отсутствие ухудшений >5% вне узкого исключения CPU; wall time, RAM, allocations, disk/load/save не становятся CPU. Исправления явной диагностики/сохранения/восстановления идут отдельным адресным основанием и не дают разрешения на весь paused перенос.

Для MC01 сохранённый peak 838893568→1254211584 Б (report75, +49,51% в действующем21) не исчезает от новой формулировки метрики или от numeric guard. Q04 pause и Q05 dependency сохраняются. У Q02/03 логическое основание не превращается в заявление о новом приросте перевода или скорости; будущая TR07-панель не получает RESOURCE_PASS по отсутствию измерений.

**Предел чтения.** Заново машинно обработаны local/canonical AGENTS, CLAUDE и названные навыки; лично прочитаны применимые первые разделы/фрагменты architect-behavior-classifier, planning-session-startup, plan-review, generated-code-review, code-execution/examination, prompt-patch-authoring, human-readable-architecture-audit, output-readability, test-integrity, test-failure-triage. Experiment-design и human-communication-GLOSSARY прочитаны полностью. Большие выдачи частично индексировались/усекались; полный личный reload всех огромных файлов **не заявляется**. Лично прочитаны приведённые исходники/фрагменты и сохранённые reports; не все файлы семейства, не все131 карточки.

**Изменён только этот evidence Markdown.** Готовые правки выше ещё не применены worker'ом в live cards; их ограниченный apply owner — root. Product, tests, runner, datasets, accepted/export/raw/binaries не изменены; сборки, tests, model calls, training и native Load не выполнялись. Git mutations не выполнялись. Ошибочные read-only `rg` обращения к несуществующим `docs/plans/replanning-possible-diffs/learning-sets` и `TranslationLearningBatchBuilder.cs` дали exit2; затем использованы реальные `learning-sets/` и `TranslationStructuredBatchBuilder.cs`. Это ошибки поиска, не результаты научных отрицательных контролей.

Контрольные SHA-256 прочитанных bytes:

| Путь | SHA-256 |
|---|---|
| `27.TranslationDomain/TranslationVerifiedRuleTags.cs` | `02e0c437a0e0c2bcea5eb303108017554535d347fbadfe4ffe46dea111e0232b` |
| `27.TranslationDomain/TranslationStructuredBatchBuilder.cs` | `b8a346a6f8641d50a535f20e1e0d834464ade071090175d3321a5e034c276d8d` |
| `Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs` | `1b6fe0c31b4e2d676198b3986e7705cc3d3735e6762eafef5f4595009435483a` |
| `04.TrainingLoop/Optimizers/AdamW.cs` | `4ff288aef879a110dbdbc0da356d5839f1087e219d128a06cb987bf251abe3ea` |
| `learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl` | `f4fd0393161f0f65e060b053f8af51d1bf85f84af20bb1cc65cd01be1e2399e2` |

Honesty check: claim_scope — адресные document/source findings и saved JSON recount; evidence_scope — перечисленные bytes/строки; count_claimed — 4 предметных раздела, **не 4 passes/cycles**; independent_evidence_count — не заявлено число независимых reviews; subagent_id — `/root/postq011_card_gaps`, вложенных workers не было; downgrade_needed — никаких новых native/scientific PASS и никакого полного аудита семейства.

## 6. Применение и проверка root, 4 ноября 2026

Предложения выше применены родительской сессией в действующих карточках и индексах; это отдельный факт после исходного пакета worker. План [161](../codex-161-postq011-executor-context-repair-2026-11-04.md) связывает каждую правку с местом исполнения. Повторное адресное чтение `/root/postq011_card_gaps` нашло неверный общий namespace в новой таблице и неточное описание D4 в строке MS01. Root сверил namespace в трёх исходных классах и метрику в MS01/report, затем исправил оба места. PAIR/RULE принадлежат `LlmFromScratch.Tests`, VALUE — `LlmFromScratch.Tests.ExperimentFixes`; отрицательный D4 означает ошибочное предпочтение, а не измерение полной генерации.

Статические проверки пакета: 24 изменённых/новых Markdown в двух репозиториях; 1 480 различных локальных целей ссылок существуют, проверенные якоря разрешаются. Исправлен старый усечённый якорь главы N44 в planning README. Порядок и состав 121 `work-*` якоря реестра сохранены. Все строки исходной Q-таблицы сохранены: 132 строки таблицы содержат 131 уникальный ID, это не 132 опыта. `git diff --check` завершился с exit0 в обоих репозиториях. Это проверки документов и ссылок, не проверка исполнения всех перечисленных работ.

Новые сборки, тесты, обучение и inference не запускались. Сохранённые результаты Q01 и MC/MS прочитаны как историческое свидетельство; их числа не выданы за новый запуск. История научных результатов не изменена.
