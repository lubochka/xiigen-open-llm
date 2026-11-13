# 113. Перенос P56: зависимости коммита и незакрытая учебная приёмка

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TRANSLATION/RUN:** Сверить builder→learned memory→обычный tokenizer/executor/verifier. Истинный negative и marker-only, exact topic/direction и нормализованные RuleTags не смешиваются; отрицательный смысловой контраст и сохранённый правильный перевод оцениваются раздельно, без подмены gold. Привязать source/config/inputs к фактически потреблённым child bytes и effective MSBuild instances; late mutation против unchanged — разные controls. Parent terminal/exit не заменяется child Passed. Такой структурный guard не требует выдуманного model Train.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


Сверка1ноября2026, после локального HEAD `2004e81030c`; предыдущий `7199ca1dcd3` относится к P66. Фаза — исследование и Markdown. В этой сессии продукт, тесты, runner и данные не менялись; сборки, тесты, обучение и inference не запускались. Удалённая публикация коммитов не проверялась. Этот документ дополняет [111](111-post-p55-transfer-plan-2026-11-01.md), а не назначает повтор P5601.

## Что найдено в фактическом переносе

Целые слова в signature-ветви verifier уже реализованы: `ContainsRequiredTermAsWholeWords` ищет смежные максимальные последовательности `char.IsLetterOrDigit` с `OrdinalIgnoreCase`. Marker/concept ветви сохраняют прежний Contains. Основание для этой узкой правки остаётся исходным положительным P56: main3/4→4/4, edges2/4→4/4 после обучения памяти. Native перенос и обычный flow не наследуют эти числа автоматически.

### Приоритет1: коммит содержит обращения к отсутствующей зависимости

`git show --name-only 2004e81030c` содержит `TranslationTrainedUnits.cs`, новый learning-test, Python-оракул и его вывод, `docs/ARCHITECTURE.md`. Однако в том же diff помимо verifier оказались изменения P51: `_exactSourceMap` заменена на `_exactTopicSourceMap`, ключ `TranslationExactSourceKey`, новый checkpoint entry и поля topic/version. Это другой механизм, владелец которого — [105](105-late-learning-transfer-plan-2026-10-30.md) и [101](101-persistent-state-bundle-contract-2026-10-30.md).

Read-only `git grep -n TranslationExactSourceKey HEAD -- '*.cs'` нашёл только пять употреблений в `TranslationTrainedUnits.cs` (:255,:273,:428,:537,:567), без определения. `git ls-tree -r --name-only HEAD -- 27.TranslationDomain/TranslationExactSourceKey.cs` не вернул файл. В рабочем дереве `git status --short` показывает этот файл как `??`, а `TranslationCheckpointContracts.cs` как modified. Следовательно, HEAD и локальная рабочая сборка имеют разные доступные зависимости. Это установленный разрыв состава исходников, **не результат выполненной сборки**.

В `HEAD:27.TranslationDomain/TranslationCheckpointContracts.cs` конструктор `ExecutorUnitCheckpoint` заканчивается старым `ExactSourceMap`; новых `ExactSourceKeyVersion`/`TopicSourceMap` и record entry нет. Новый exporter из того же HEAD уже вызывает эти именованные параметры. Текущий `TranslationMachine.cs` содержит `ValidateExecutorExactSourceMemory` (:626) и вызов до импорта (:554), но `git show HEAD:...TranslationMachine.cs` этого version/context guard не содержит. Таким образом, добавление только отсутствующего key-файла недостаточно: нужна согласованная цепочка key→checkpoint→validation→import.

### Приоритет1a: выбранный адресный ремонт полноты P51/P56

Сохранить уже реализованное whole-word правило P56. Зависимость P51 оформляется явно по105, не переписывается второй раз под именем P56. Будущий исправляющий набор включает ровно согласованные изменения следующих владельцев:

| Путь | Что обязан согласовать исполнитель |
|---|---|
| `27.TranslationDomain/TranslationExactSourceKey.cs` | Действительный тип, канонизация пары topic/source и версии; включить в состав исходников, не stub |
| `27.TranslationDomain/TranslationCheckpointContracts.cs` | Record пары и два trailing поля вложенной версии согласно105 |
| `27.TranslationDomain/TranslationTrainedUnits.cs` | Writer/reader/export/import используют один адрес; сохранить signature-only P56 helper |
| `27.TranslationDomain/TranslationMachine.cs` | Проверить version/schema/duplicates/context **до** очистки живых maps; нельзя переносить importer отдельно от validation |
| `Tests/ExperimentFixes/TranslationExactTopicMemoryTests.cs` | Существующий учебный набор105 и проверки version1/version2; заново не проектировать корпус |
| `Tests/ExperimentFixes/TranslationRequiredWholeTermLearningTests.cs` | Недостающая приёмка ниже; наличие этого файла не закрывает тесты105 |
| `docs/projects/27-translation-domain.md`, `docs/ARCHITECTURE.md` | Указать две зависимости, действующую политику старых снимков и предел P56 |

Это состав будущего согласованного изменения, не команда сейчас включить всё рабочее дерево. Не удалять чужие изменения и не присоединять несвязанные файлы. Не переписывать историю существующего коммита; подготовить отдельное исправление зависимости. До его приёмки статус P56 — `SOURCE_IMPLEMENTED_ACCEPTANCE_OPEN`, а commit2004 — `DEPENDENCY_CLOSURE_INCOMPLETE` по статической проверке. Если результат105 ещё не принят, зависимость остаётся открытой; нельзя обойти её добавлением типов без договорённости о состоянии.

Политика version1 уже назначена105: пустая legacy map может стать пустой contextual, непустая source-only map отвергается с `CHECKPOINT_EXACT_SOURCE_CONTEXT_REQUIRED` **до мутации**, исходный файл сохраняется. Тему не угадывать и уроки не replay. Это намеренная несовместимость без потери принятого runtime, а не обещание миграции старого обученного checkpoint. Новый импорт, который молча читает только `TopicSourceMap ?? []`, без machine guard потерял бы legacy exact память; здесь описан риск неполного переноса, не измеренная потеря в текущем запуске.

### Приоритет2: доказательство алгоритма не равно выполненной учебной приёмке

Сохранённый [вывод оракула](../../../../Tests/ExperimentFixes/oracle-whole-term-111-p56-output.txt) явно говорит: доказаны поведение алгоритма на панели и чувствительность к мутации; компиляция C# и прогон теста не доказаны. Оракул читает правило, а не обучает native `TranslationMachine`. Его положительные строки полезны как диагностика и не являются ещё одним выполненным учебным экспериментом.

Добавленный [TranslationRequiredWholeTermLearningTests](../../../../Tests/ExperimentFixes/TranslationRequiredWholeTermLearningTests.cs) содержит настоящий `QualityVerifierUnit.TrainOnExample` (:501). Два основных теста задают2 lessons и52 Verify вместе с sensitivity; это подготовленный код, а не выполненная доза. В файле нет TrainStructuredBatchAsync/GetOrchestratorAsync/RunAsync и запуска child process. Восстановление (:259–267) прямо обозначено как `new QualityVerifierUnit` + ImportCheckpoint в том же процессе. Называть его cold-process restore было бы неверно. Профиль111 назначал10 lessons,52 Verify,10 flow и2 fresh-process readers; обычный bridge и независимые процессы ещё отсутствуют в этом тесте.

Канонический каталог `experiments-v2/_runners/FIX-P56-WHOLE-TERM-attempt-0001` на момент проверки отсутствовал. Это не доказательство отсутствия любого запуска в ином месте; в предъявленном commit/оракуле нет terminal receipt C#-приёмки. Соседний `P56-WHOLE-TOKEN-REQUIRED-attempt-0001` запускает прежний P5601 и не заменяет native тест нового переноса.

### Приоритет2a: завершить назначенный профиль111, не выдумывать новый опыт

Сохранить ровно корпус и oracle111-P56. Direct часть: A1 урок/16 Verify, B1 урок/24 Verify +12 sensitivity. Восемь B readback заменить чтениями в отдельном child, а не добавить ещё8. Действующий code test остаётся owner прямых assertions; новый процесс загружает сериализованный learned checkpoint без Train. Для machine bridge использовать ровно `null готов→null already` и `ReadyFlag готов→ReadyFlag ready`, четыре unit lessons на A и на B,10 ordinary flow включая fresh B. Фразы не добавляются в phrase map; trace подтверждает прохождение verifier. Общая доза по-прежнему10 unit lessons/4 training entries/52 Verify/10 flow, не12 lessons из случайного повторного запуска класса.

Оснастка — уже назначенные111 `tools/translation-transfer-readback/Program.cs`, `tools/translation-transfer-readback/TranslationTransferReadback.csproj`, `tools/translation-transfer-readback/Run-TransferAcceptance.ps1`; не создавать ещё один конкурирующий launcher. Профиль P56 и команды Baseline/Candidate остаются прежними. Если запускается xUnit как operational host, использовать только BaselineArm в baseline-stage и CandidateArm в candidate-stage; запуск обеих сторон до и после правки требует другого бюджета и не выполняется автоматически. Третий `OldSubstringLogic_...` — дополнительный алгоритмический oracle, не третий Train; его вызовы явно учесть, если он отдельно назначен. Полный suite не заменяет привязку actual source/assembly к фазе.

Чистый исторический A не восстанавливается откатом рабочего дерева. Whole-word фактор уже в HEAD; для сравнения нужен либо ранее сохранённый native baseline с достаточной квитанцией, либо специально назначенная изолированная historical-A копия с тем же learner и единственной заменой этой проверки. До отдельного назначения такой копии нельзя объявлять post-fix BaselineArm настоящим pre-fix A. Не создавать worktree/clone автоматически в этой исследовательской сессии. Если A не предъявлен, разрешённый будущий green прогон может подтвердить текущие инварианты, но не новое native A/B ускорение.

### Приоритет3: квитанции сейчас способны затереть предыдущую фазу

В тесте константа `AttemptId="attempt-0001"` (:80), `ReceiptDirectory` (:623–645) выбирает один каталог без stage, `WriteReceipt` (:649–660) вызывает `File.WriteAllText`. Повтор baseline/candidate перезаписывает прежний `arm-A/arm-B` receipt. Это тот же класс ошибки, который уже обнаружен у P53/P55 в110, теперь с новым конкретным writer. Ещё до нового запуска изменить writer так, чтобы он принимал явные attempt/stage/outputRoot от назначенной оснастки, отвергал неизвестную фазу до Train и создавал файлы без перезаписи. Порядок `baseline/` и `candidate/` принадлежит111; fixed attempt не должен превращаться в поиск latest или скрытый autosuffix retry.

У фаз отдельные source identity, assembly identity, корпус, cold/after/readback, фактические дозы и terminal outcome. Имена тестов A/B сами по себе фазу не определяют. Старые файлы сохраняются. Провал записи после обучения не отменяет уже выполненный урок; не переобучать модель лишь ради квитанции. Этот ремонт writer относится к достоверности результата обучения, а не является отдельным нетренирующим экспериментом.

## Область новых и уже известных материалов

Материалы P66–P81 уже перечислены в другом [аудите107](107-transfer-and-experiment-audit-2026-11-01.md), с владельцами ремонтов в [108](108-addressed-repair-plan-2026-11-01.md) и [109](109-card-hardening-and-coverage-2026-11-01.md). Новая сверка не выдаёт их за впервые найденные переносы. Отдельный [112](112-convergence-two-diseases-second-consumer-2026-11-01.md) уже описывает диагностику недостижимого окна convergence и нулевого порога. Новые первичные уточнения собраны в [review после P65](reviews/2026-11-01-post-p65-evidence-review.md).

### Что именно усилено в других карточках

В текущем дереве записаны шесть адресных верхних блоков P65/P66/P68/P69/P70/P74. Они сохраняют старые тела/результаты, запрещают повтор старого опыта ради документации и связывают новый статус с действующим владельцем. В109 поправлены четыре ложных указателя: P65→111-P65, P66 иP74 не получают несуществующий work order изRF08, P69 не получает GraphRAG encoder изRF04/E28. Это проверка шести карточек и четырёх ссылок, не новый аудит всех89 карточек, упомянутых прежним109 на другом срезе.

- P68:24 evaluation Forward +8 training Forward =32 публичных model Forward; старое `total24` имело более узкую область. Полезный gain охвата и малый loss gain остаются, расходы и отсутствие нового T2 навыка рядом.
- P69: для head32+tail32, F<32 и полной длины>64 доля tail-фактаF/64 не растёт только от замены prefix287 на72. Этот аналитический факт не предсказывает неизменность всего вектора или качества, но исключает предложенное объяснение «кратного» усиления факта.
- P70:confidence — rounded/clamped score;1.0000 не доказывает идентичность векторов. Сам успех2/4→4/4 сохранён; нынешняя доставка полей повторно не назначена.
- P66/P74: диагностическая видимость и marker coverage не превращаются в доказанный правильный ответ. Новое native продолжение требует своего точного caller, набора и учебного бюджета; общее названиеRF08 этого не предоставляет.

Проверка возможного нового обхода learned verifier через phrase-map нашла131 совпадение в планах/библиотеке; отдельная новая карточка на уже известный вопрос не создана. Это результат конкретного поиска, не исчерпывающая карта всех ранее обсуждавшихся гипотез.

Использованы актуальные границы AGENTS/CLAUDE и адресные разделы навыков architect-behavior-classifier, planning-session-startup, plan-review, generated-code-review, prompt-patch-authoring, output-readability-gate; human-communication-GLOSSARY прочитан полностью. Полный reload многотысячных skill-файлов не заявлен. Один исследователь `/root/late_positive_evidence_review` сверяет первичные новые пакеты; parent владеет этим аудитом. Большая общая выдача git status в Context Mode превысила буфер; её неполный счёт не используется. Вывод о зависимости основан на отдельных успешных git grep/ls-tree/status указанного пути.
