# 161. После Q01 и POSTQ011: точное задание исполнителю

**Текущий перенос — Q02 / `135-R09-PAIR`, затем Q03 и Q06. Q01 принят в своей ограниченной области.** Это исправление инструкций и карточек, а не новая очередь или новый эксперимент. Q04/FIX-MC остаётся приостановленным; Q05 зависит от него. Порядок всегда сверяется с [живым реестром](01-registry.md#executor-next).

Основания: присланные пользователем журналы исполнения Q01 и аудита POSTQ011; [аудит POSTQ011](codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md), [его план 159](codex-159-postq011-addressed-repair-plan-2026-11-04.md), [охват 160](codex-160-postq011-card-coverage-2026-11-04.md); исходники и сохранённые результаты core `8109a63aa2a`. Этот документ добавляет недостающую связь между уже написанными правилами и конкретным следующим действием. Продукт, тесты, учебные пары и исторические результаты здесь не менялись; свежие сборки и обучение не запускались.

## 1. Что уже исправлено, а что действительно нужно изменить

| Наблюдение | Подтверждение | Действие автора карточки |
|---|---|---|
| Q01 уже закрывает три назначенных остатка | [Отчёт](../../../../qa-results/q01-concurrency-2026-11-04-20261104091631764/codex-report.md), итоговый TRX: 29 Passed, 0 Failed; первый TRX: 25 Passed, 4 Failed | Убрать из действующего поручения повтор CAS/late cancel/store. Сохранить тесты как регрессию. Не объявлять успешным полный набор тестов или CLI-запуск |
| Первые четыре отказа Q01 возникли в подготовке пути | [TranslationOrdinaryCandidateStoreTests](../../../../Tests/ExperimentFixes/TranslationOrdinaryCandidateStoreTests.cs): `Path.GetFullPath` перед путями результата и кэша | Чинить подготовку по договору Windows-пути; не менять ожидаемый ответ, не пропускать cache/disk/fresh. Этот первый прогон не является RED прежнего продукта |
| В Q02 смешаны две разные нормы RULE | [TranslationVerifiedRuleTags](../../../../27.TranslationDomain/TranslationVerifiedRuleTags.cs), [builder](../../../../27.TranslationDomain/TranslationStructuredBatchBuilder.cs), [SemanticKey](../../../../27.TranslationDomain/TranslationSemanticKey.cs) | Новый builder выдаёт `RULE:TENSE=PAST`; удаление PAIR из legacy-ключа сохраняет его `RULE:TENSE_PAST`. Исправлять только неверное ожидание конкретного пути |
| Q007 внешний проверяющий защищает импорт, который сам ещё принимает повреждение | [159, §2](codex-159-postq011-addressed-repair-plan-2026-11-04.md#2-q007-безопасный-импорт-и-полнота-конкретного-continuation) | Передать это также в FIX-MC и E-SFT-MOMENT-CONTINUITY; не считать PASS внешнего reader исправлением AdamW. Общий перенос MC не разблокировать |
| Номер 158 неоднозначен | Существуют два разных файла с этим номером | В копируемом поручении давать полный путь/ссылку, а не «прочитай 158» |
| Разрешённая QA иногда смешивается с новым научным опытом | Договоры [120](120-executor-context-and-test-repair-contract-2026-11-02.md) и [46](../experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md) | В самом поручении назвать фазу. Назначенный перенос включает сборки, запуск и обоснованный ремонт тестов; описание будущего опыта не является его запуском |

Не путать **[158 — подбор учебных наборов](codex-158-learning-sets-transfer-coverage-2026-11-04.md)** и **[158 — аудит POSTQ011](codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md)**. Имена файлов сохраняются, ссылки однозначны. Так же различать договор исполнителя 120 и отдельные отчёты с номером 120.

## 2. Один приоритет и один следующий шаг

При назначенном исполнении действует пользовательский порядок: сначала переносы и их исправления. Переход к отдельной очереди опытов допустим только при **заранее ожидаемом ожидании переноса от 30 минут** либо после завершения всех переносов и исправлений. Основание ожидания — уже запущенная операция и её ожидаемая длительность, а не то, что чтение документов заняло полчаса. Наличие ожидания не разрешает новый научный фактор или повтор записанного PRIMARY.

В поручении зафиксировать: имя очереди, устойчивый ID, текущую фазу, живой handle ожидаемой операции, основание оценки длительности, разрешённую вторичную работу и условие возврата. Не запускать несколько опытов одновременно. Если короткая задача ожидания закончилась, вернуться к переносу; уже запущенный процесс не бросать и не перезапускать по пустому логу. Прямые STOP/NEXT пользователя исполняются немедленно. Полную обязательную перечитку не заменять поиском; чтение не считается отдельным исполненным переносом.

На этом срезе разбор опытов принят для Q001–Q011: 11/131; Q012 не назначен текущей документной работой. Все 131 исходных результата уже записаны. Это три разных факта, не разрешение обучать Q012 заново.

## 3. Q02: исправлять ожидание по реальному владельцу

Ниже — конкретизация существующего [135, R09](codex-135-post113-addressed-repair-plan-2026-11-02.md#r09--m03-закончить-приёмку-миграции-и-исправить-preflight), а не новый перенос.

1. `TranslationVerifiedRuleTags.Canonicalize` приводит проверенную грамматическую аннотацию к `TENSE=PAST`/`TENSE=PRESENT`; `TranslationStructuredBatchBuilder` сохраняет знак `=` при добавлении `RULE:`. Поэтому `VerifiedRuleTags_SurviveWhileTheProvenanceSegmentIsGone` и `ImportedCheckpoint_StoresTheLessonUnderACanonicalKeyWithoutProvenance` в [TranslationPairTailSignatureLearningTests](../../../../Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs) должны проверять форму действительного builder. В прочитанном срезе оба ещё ожидают `RULE:TENSE_PAST`.
2. В том же первом тесте отдельные примеры `TranslationSemanticKey.Canonicalize` намеренно предъявляют старые ключи с `RULE:TENSE_PAST`. Их ожидание сохраняется: эта функция удаляет только PAIR, не переписывает остальные сегменты. Замена всех подчёркиваний на `=` или удаление RULE из продукта — неправильный ремонт.
3. [TranslationVerifiedRuleTagTests](../../../../Tests/ExperimentFixes/TranslationVerifiedRuleTagTests.cs) уже задаёт положительные прошлое/настоящее время, отсутствие аннотации, конфликт двух времён и независимость от rejected. Этот класс входит в обязательный набор вместе с PAIR, а не остаётся за фильтром.
4. Остальные границы R09 остаются обязательными: legacy/v2 и неизвестная версия; отказ коллизии до публикации; достижимый кандидат до source-floor; законный положительный ответ при достаточном совпадении; отсутствие PAIR-обхода; обычный ответ именно загруженной машины; object/array/null у chosen. Наличие нового RULE-теста не закрывает их.

**Будущие команды назначенного ремонта**, cwd `C:/Projects/llm_mvp_core`. После изменений сборка должна завершиться успешно до исполнения тестов; ниже команды не запускались:

Минимальная карта обязательных случаев ниже. PAIR — `LlmFromScratch.Tests.TranslationPairTailSignatureLearningTests`; RULE — `LlmFromScratch.Tests.TranslationVerifiedRuleTagTests`; VALUE — `LlmFromScratch.Tests.ExperimentFixes.TranslationBatchChosenOutputValueKindTests`. Для FQN соединить полное имя соответствующего класса с именем метода через точку. Это карта прочитанного исходника, не результат discovery и не подтверждение прохождения.

| Граница R09 | Существующие методы, которые должны попасть в результат | Дополнительная приёмка, пока не подтверждённая этими методами |
|---|---|---|
| Новый RULE и сохранность старого сегмента | PAIR.`VerifiedRuleTags_SurviveWhileTheProvenanceSegmentIsGone`; RULE.`VerifiedPastTag_IsStoredWithTheLesson_NotThePresentTagOfTheRejectedVariant`, `VerifiedPresentTag_IsStoredWithTheLesson_NotThePastTagOfTheRejectedVariant`, `ChangedRejectedSide_DoesNotChangeTheVerifiedAnnotation` | Все прочие случаи RULE-класса сохраняются; helper legacy `_` не заменять на `=` |
| Schema/legacy | PAIR.`RowClaimingCanonicalFormWhileCarryingProvenance_IsRefusedWithBothValuesNamed`, `RowThatDoesNotStateItsKeyForm_HasTheFormDerivedFromTheKeyItself` | Отдельный отказ неизвестной версии и настоящий вход legacy checkpoint. Экспорт нового builder с последующим импортом не доказывает миграцию старого формата |
| Отказ коллизии | PAIR.`TwoRowsCollapsingToOneKeyWithIncompatiblePayload_AreRefusedBeforeAnyPublication` | Полное прежнее состояние до внешнего restore; проверить, что отказ дошёл до назначенного preflight |
| Source floor | PAIR.`ForeignProvenanceInTheKey_DoesNotLiftTheOrdinaryEnRuSourceFloor` | Явно наблюдать identity/count достигнутых кандидатов; отдельно сохранить lawful known-semantic floor0. Пустой список кандидатов не закрывает проверку порога |
| Загруженный получатель | PAIR.`SavedLesson_SurvivesCheckpointRoundTrip_AndIsAnsweredByTheLoadedMachineItself`, `ImportedCheckpointReExportsExactlyWhatWasSaved` | Fresh-machine negative уже есть; дополнительно нужна адресная порча именно использованной restored map. Хранение и обычный ответ оцениваются отдельно |
| Chosen ValueKind | Все три метода VALUE: `EmptyArrayAsChosenOutput_IsARefusalWithNamedValueKindAndNoPublication`, `NullAsChosenOutput_KeepsTheExistingTrainerSkipAndCountsTheAcceptedBatch`, `OrdinaryObjectChosenOutput_StillDetectsTheKeyCollision` | Bookkeeping/null и collision не заменяют положительный выученный ответ loaded-target |

Недостающие случаи относятся к уже назначенной приёмке R09. Исполнитель реализует их в существующем PAIR-классе, сохраняя остальные классы фильтра; тестовая адаптация для этого разрешена. **Предлагаемые имена новых методов, пока не существующие FQN:** `UnknownKeySchema_IsRejectedBeforeAnyPublication`, `LegacyCheckpoint_WithPairSegments_LoadsAndAnswersThroughTarget`, `KnownSemanticCandidate_RetainsLawfulZeroSourceFloor`, `CorruptingRestoredAnswerMap_BreaksLearnedAnswer`. Первые два используют независимый договор версии: неизвестная версия отвергается до публикации; legacy-вход действительно содержит старую форму ключей/отсутствующее поле версии. Его происхождение записать как конкретный архивный fixture либо явно искусственный DTO старой схемы; current export без такой смены формата не является legacy. Не называть искусственный DTO восстановленным историческим checkpoint. Для последних двух сохраняются уже назначенные input/gold и обученное начало PAIR; изменяется только проверяемое свойство, а не учебная доза или ожидаемый ответ. Положительный ordinary ответ должен существовать до порчи карты. Если независимого legacy-договора или пригодного начала нет, эскалировать именно этот вход автору; не объявлять четыре класса полным покрытием R09.

```powershell
$filter = 'FullyQualifiedName~TranslationPairTailSignatureLearningTests|FullyQualifiedName~TranslationVerifiedRuleTagTests|FullyQualifiedName~TranslationMachineContractSignatureTests|FullyQualifiedName~TranslationBatchChosenOutputValueKindTests'
$results = Join-Path 'C:/Projects/llm_mvp_core/qa-results/q02-pair' (Get-Date -Format 'yyyyMMdd-HHmmssfff')
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Q02 build failed; do not run stale DLL' }
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests --filter $filter
if ($LASTEXITCODE -ne 0) { throw 'Q02 discovery failed' }
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter $filter --results-directory $results --logger 'trx;LogFileName=q02.trx'
if ($LASTEXITCODE -ne 0) { throw 'Q02 tests failed; preserve TRX and classify failures' }
```

Фильтр охватывает перечисленные существующие классы, но не заменяет таблицу required FQN/data rows и сопоставление шести предметных границ R09. Если нужного случая пока нет, исполнитель добавляет его в назначенный тестовый класс, затем повторяет discovery. Перед запуском учитывает обучающие действия его setup и самих методов; не объявляет весь набор Train0. Бюджет исходного опыта не увеличивается: это назначенная продуктовая приёмка с отдельно записанным расходом. Непустой TRX, одни RULE-тесты или одно значение Failed=0 не закрывают недостающую границу. Полный набор тестов имеет собственный результат; адресный прогон не называется полным.

## 4. Как разбирать падение и чему учит Q01

| Случай | Правильное действие | Недопустимая подмена |
|---|---|---|
| Compiler/discovery/setup | Сохранить команду, exit, source/DLL, журнал и исправить техническую причину в разрешённом охвате | Запустить прежнюю DLL, удалить файл из проекта или объявить ноль найденных тестов успехом |
| Неверный expected | Сослаться на независимый договор и точный путь producer→consumer; сохранить различающий отрицательный случай | Заменить expected фактическим ответом без основания |
| Неверный ответ при корректном expected | Исправить продукт либо передать архитектору конкретный спорный договор | Удалить RULE, ослабить threshold, добавить Skip или принять любое исключение |
| Не хватает учебного предусловия | Назвать missing lesson/qualified state и отдельное необходимое назначение | Вшить ответ, скрыто переобучить при Load или записать 0→0 как полезный учебный опыт |
| Отрицательный контроль намеренно даёт неверный ответ | Проверить именно ожидаемый отказ/ноль при сохранённом положительном контроле | Требовать положительной accuracy от каждого отрицательного контроля |

Q01 различает измерение нуля и отмену: `Ordinary_incorrect_answer_is_a_successfully_measured_zero_not_cancellation` проверяет диагностику, а не приобретение нового умения. Это не исключение, разрешающее бессодержательный учебный опыт 0→0. Положительный контроль штатной оценки остаётся обязательным.

Для похожих проверок конкурентности использовать расписание **B победил → CAS A отказал → C опубликовался → оформить результат A**. Результат A обязан сохранить увиденную им B, а не прочитать текущую C позднее. Ранний отказ по версии не проверяет этот случай. Для отмены достигнуть последнего барьера и сравнить полное состояние, версию, точность и историю до внешнего восстановления.

Для похожих проверок сохранения использовать обычный путь создания, загрузки и кэша, затем отдельный процесс с реальным loader и обычным ответом. [Читатель Q01](../../../../Tests.Tools/TranslationLostCandidateReader/Program.cs) и [его тест](../../../../Tests/ExperimentFixes/TranslationOrdinaryCandidateStoreTests.cs) показывают существующий пример. Он принимает путь, hash, запрос и каталог вывода; правильный ответ остаётся у теста, а receipt содержит фактический ответ. Сравниваются PID, байты, состояние до/после и ответ. Отсутствие Train в прочитанном entry source не называть глобальным инструментированным счётчиком всех вызовов.

**Q03 /135-R02-EXECUTION** использует свой [135, R02](codex-135-post113-addressed-repair-plan-2026-11-02.md#3-r02--состояние-процесса-стадия-и-повторный-вход) и [159, §5](codex-159-postq011-addressed-repair-plan-2026-11-04.md#5-runner-реальный-deadline-без-повторного-научного-опыта): фактический child exit, timeout, сохранённые логи, освобождение только своей блокировки после завершения своего дерева процессов. Проверки искусственным child без модели валидны для этого технического договора. Они не являются новым опытом обучения и не требуют повторять BPE/SFT/DPO ради зелёного статуса.

## 5. Наборы данных и допуск переноса

Действует [полный план подбора наборов](codex-158-learning-sets-transfer-coverage-2026-11-04.md) с [проверенными источниками](evidence/2026-11-04-learning-sets-transfer-map.md). Существующие 5 729 строк черновиков — TRAIN; 67 повторяющихся ID имеют разные тройки. Статус accepted не подтверждает готовность нативного импорта или наличие независимого экзамена.

Кандидат для **будущей дополнительной панели Q02** — `learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl`: исправление опечатки и полный перевод. Это 10 TRAIN-строк, без готового DEV/EXAM, с `native_import_verified=false`. Получатель Q02 — `TranslationMachine` и `TranslationStructuredBatchBuilder`, а не нейронный DpoTrainer. Существующий R09 сначала закрывается на собственных fixtures. В новом профиле сохранять topic, направление, обычный полный ответ, PAIR-происхождение и проверенные RULE раздельно. Простой перенос prompt/chosen/rejected в DpoPair не создаёт typed translation lesson или checkpoint и не заполняет недостающую аннотацию.

Важная граница: builder токенизирует `ASide`; инструкция и контекст из `prompt` туда автоматически не попадают. В TR07 первый prompt задаёт кухонный контекст и восстановление пропущенной буквы. Подстановка только `metadata.source_raw` в ASide удалит контекст, а `canonical_source` подскажет исправленное слово. Перед будущим запуском нужен manifest проекции: полный исходный prompt, фактический вход ученика, политика контекста, IDs/hash, допустимые метаданные и native admission. Если назначен raw-only режим, называть его памятью исходной строки/восстановлением ответа, а не исходной задачей контекстного исправления опечатки. Автор обязан заранее разрешить это отдельное утверждение; исполнитель не меняет его молча.

Если позже назначено расширение набора, производные пары и независимые DEV/EXAM размещать в `learning-sets/simulations/transfer-coverage-2026-11-04/<profile>/v1/` по [инструкции 31](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/31-TRANSFER-VALIDATION-SIMULATIONS.md). Ничего из этого не создано здесь. Для Q03 предметный набор N/A: технический child не учится. Для Q06 остаётся отдельный DOC-CS-NULL/M002 и SFT-framing из плана подбора наборов; translation/RULE не подмешивать. Для MC данных недостаточно, чтобы снять установленную ресурсную паузу.

Пользу показывать по каждому показателю и домену: до, после, дельта, точность и ухудшения. Более 5% — пользовательский порог приоритета/допуска, а не доказательство статистической значимости; процессорное время имеет указанное пользователем исключение, остальные ресурсы не прятать за ним. Неизмеренная ось — NOT_MEASURED, не «без регрессии». Сохранение moments в MC улучшало NLL примерно на 49–50% и удержало одно экзаменационное предпочтение, но пик памяти вырос на 49,51%: это не общий допуск MC. У MC дополнительно исправлено толкование генерации: по 4 выхода в A0/A1/B0/B1, всего 16; наличие фразы «Returns the» — 0/4 в каждом состоянии, а качество полных ответов не оценено предметным исполнителем. Это не accuracy 0/16→0/16. Отдельный отказ повреждённому native state — узкий ремонт обещанного восстановления, а не способ перенести весь MC под словом bugfix. Математическое основание дополняет измеренный результат и не делает его универсальным.

## 6. Навыки и копируемое поручение

Перед новым заданием полностью и заново прочитать обязательный стек [договора 120](120-executor-context-and-test-repair-contract-2026-11-02.md), включая AGENTS/CLAUDE. Таблица ниже объясняет применение, не сокращает этот стек:

| Когда | Навык и проверяемое действие |
|---|---|
| До постановки | [architect-behavior-classifier](../../../skills/architect-behavior-classifier-SKILL.md), [planning-session-startup](../../../skills/planning-session-startup-SKILL.md), [plan-review](../../../skills/plan-review-SKILL.md): один owner, разрешённая фаза, достаточный положительный и отрицательный случай |
| При чтении кода | [code-examination](../../../skills/code-examination-SKILL.md), [code-execution](../../../skills/code-execution-SKILL.md), [generated-code-review](../../../skills/generated-code-review-SKILL.md): настоящий caller, fixture, фактическая DLL и распространение отказа |
| До теста и при падении | [qa-session-type](../../../skills/qa-session-type-SKILL.md), [test-integrity](../../../skills/test-integrity-SKILL.md), [test-failure-triage](../../../skills/test-failure-triage-SKILL.md), [systematic-debugging](../../../skills/systematic-debugging-SKILL.md), [debug-session](../../../skills/debug-session-SKILL.md): причина, независимый expected, сохранение первого отказа |
| При автоматизации и приёмке | [bug-to-tests](../../../skills/bug-to-tests-SKILL.md), [integration-testing](../../../skills/integration-testing-SKILL.md), [verification-before-completion](../../../skills/verification-before-completion-SKILL.md) и [QA46](../experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md): воспроизводимый запуск, discovery, настоящий получатель, свежий terminal |
| При данных и отчёте | [experiment-design](../../../skills/experiment-design-SKILL.md), [improvement-measurement](../../../skills/improvement-measurement-SKILL.md), [prompt-patch-authoring](../../../skills/prompt-patch-authoring-SKILL.md), [human-readable-architecture-audit](../../../skills/human-readable-architecture-audit-SKILL.md), [output-readability-gate](../../../skills/output-readability-gate-SKILL.md), [словарь коммуникации](../../../skills/human-communication-GLOSSARY.md): неизменная метрика, честный ресурсный допуск, понятный русский текст |

Готовый общий блок дополняется точным owner из §3 или §4:

> Выполни только назначенный перенос и оставшийся пункт его живой строки. Прочитай полные файлы, указанные в 120 и 161; короткие номера не заменяют имён. Сохрани принятую часть. Назначенная реализация включает сборки, discovery, реальные тесты и исправление технической подготовки/устаревшего expected по независимому договору. Не повторяй исторический PRIMARY и не меняй научный набор, модель или дозу для ремонта отчёта. Перед тестом прочитай его fixture и составь точные required FQN/data rows; после сравни их с TRX той же сборки. При проблеме покажи input, expected, actual, caller, source/DLL, команду, exit и спорное условие; исправь разрешённую техническую причину, спорный новый договор передай архитектору. Дождись terminal исходного handle; пустой лог не означает отказ. Отчёт по-русски положи в каталог результатов выбранного owner рядом с журналами: что изменилось, что реально проверено, чему равно до/после, что ухудшилось и какой остаток открыт. Прямой STOP/NEXT исполни сразу. Выполни git add --all; в публикации различай свои и чужие правки. Действующая модель исполнителя сохраняется; никаких самостоятельных замен модели. Ни коммит, ни отчёт, ни ожидание сами по себе не назначают новый опыт.

## 7. Граница этой проверки

Полный структурный поиск инструкций по карточкам не равен смысловой проверке каждой реализации. Прочитаны указанные исходники, сохранённый Q01/TRX и адресные материалы POSTQ011; отдельный исследователь проверил state/PAIR и подготовил [пакет свидетельств](evidence/2026-11-04-postq011-executor-context-review.md). Ни новые научные числа, ни новая native-приёмка здесь не получены. Большие инструкции обработаны целиком инструментом с адресным чтением; это не заявление о полном личном перечитывании всех их строк и не выполненная за исполнителя обязательная перечитка. Старые опыты и принятые результаты сохранены.
