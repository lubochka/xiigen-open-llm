# Проверка четырёх новых карточек по 13 правилам пользователя

Проверка 20 октября 2026: документация, literal fixtures, исходный donor Program и адресные source-copy specifications. **Новые host не реализованы, не собраны, обучение не запускалось.** Не заявляется измеренный выигрыш или готовность четырёх команд к немедленному исполнению. Core и прежние possibleDifs не редактировались; замечания направлены авторам новых документов.

## Область и конечные статусы

| Новый опыт | Настоящий учебный участок и конечный бюджет | Итог документной проверки |
|---|---|---|
| [DPO-TOKENIZER-SNAPSHOT-01](../neural-bpe/DPO-TOKENIZER-SNAPSHOT-01.md), [prompt](../neural-bpe/prompts/DPO-TOKENIZER-SNAPSHOT-01.md), [design](../neural-bpe/dpo-source-copy-design.md) | Prepare двух exact pairs реально потребляется одним StepVector на arm; два native updates всего. До/после — material/weights/optimizer/RNG, полезная стоимость Prepare и всего update; DPO-returned loss честно pre-update. | Документный дизайн согласован; 108 исходных C# и три library identities назначены. Реализация, ABI/DLL/B0 availability и runtime не проверены. |
| [BPE-WEIGHTED-CHUNKS-01](../neural-bpe/BPE-WEIGHTED-CHUNKS-01.md), [prompt](../neural-bpe/prompts/BPE-WEIGHTED-CHUNKS-01.md), [design](../neural-bpe/bpe-source-copy-design.md) | Четыре непустых native Train: A/B × C#/EN→RU. Ещё четыре объявленные инициализации byte-vocabulary; 32 EXAM Encode/Decode. | Один фактор: weighted representatives, без no-match optimization. Сохранены tie order, overlap semantics и multiplicity. Compile/runtime/эквивалентность пока не доказаны. |
| [WIKI-FEATURES-ONCE-01](../wiki/03-features-once-experiment.md), встроенный prompt, [work order](../wiki/04-source-copy-work-order.md) | Две непустые Train, две пустые инициализации, 288 учебных предъявлений, 16 native EXAM calls. Подготовка cache входит в измеряемый Train B. | Native perceptron назван правильно, все Labels прежние. Возможная цена живой памяти cache раскрыта; это локальный cost-дизайн, не свидетельство восстановленного RP01. |
| [WIKI-FEATURE-KEYS-01](../wiki/02-feature-keys-experiment.md), встроенный prompt, [work order](../wiki/04-source-copy-work-order.md) | Две непустые Train, две пустые инициализации, 144 предъявления, 24 EXAM calls. Разделение unigram/bigram меняет обе стороны одной Featurize. | Осмысленные route labels и 4/6 literal fixture; synthetic collision не выдается за полезное обучение. Гипотеза условная, NO_GAIN допустим; старые snapshots новым codec не читаются. |

Все четыре — **DESIGN_REVIEWED_NOT_IMPLEMENTED_NOT_RUN**. Это законченная проверка постановки, не сертификат исполнения. Найденные ниже конкретные документные конфликты устранены в новых карточках/спецификациях; оставшиеся неизвестные относятся к ещё не выполненной реализации и запуску.

## Выявленные конкретные замечания

| Замечание | Почему существенно | Статус |
|---|---|---|
| PowerShell переходил бы к следующей native-команде после ненулевого exit | Native dotnet exit code сам по себе не останавливает весь скопированный PowerShell-блок. Неуспешный build не должен переходить в training. | **Исправлено:** в обеих neural/BPE карточках и обоих prompts стоят проверки `$LASTEXITCODE` после каждого build/run; в обеих Wiki карточках — после SDK/restore/build/A/B. Никаких запусков для этой проверки не делалось. |
| Буквальное «никакого tool call» при STOP могло запретить завершить уже работающий процесс обучения | Продолжающийся собственный процесс нарушил бы саму команду остановить всё. | **Исправлено:** допускается только необходимое прекращение своего уже работающего процесса; cleanup, новые проверки, report polish и новый run по-прежнему запрещены. Правило находится в карточках и prompts. |
| Donor Wiki Program создаёт output только после Train/Evaluate; исходный PS не резервировал parent attempt и не сохранял process logs | Crash после начала обучения мог не оставить output. Отсутствие output ошибочно выглядело бы новой попыткой; фраза «сохраняем failed-run» ещё не реализует запись следа. | **Исправлено в документах:** обе команды создают parent attempt/intent до SDK/build и сохраняют stdout/stderr/issued/returned. Work-order §5 требует arm intent и неизменяемые phase-issued/completed до/после настоящих Train, failed/UNKNOWN при прерывании. File I/O вынесен из TRAIN cost. Arm directory создаёт host после guard; старый scaffold остаётся неизменным. |
| «Existing host/output → BLOCKED» смешивал чужой занятый каталог и собственный только что подготовленный host | После подготовки собственный host обязан существовать, иначе выполнить назначенную команду невозможно. | **Исправлено:** чужой/неустановленный host — конфликт до подготовки; свой подготовленный host является входом запуска; любой существующий attempt запрещает повтор. |
| JSONL содержал лишнюю пустую строку после двух records | Native Inspect не должен получить неожиданную пустую запись. | **Исправлено автором и проверено read-only:** ровно две JSON records, один конечный LF, SHA256 `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`, 848 bytes. Карточка, prompt и manifest синхронизированы. |
| Явный OutputPath уже содержал net9.0, а SDK мог добавить его повторно | Конечная команда могла не найти фактически построенную DLL; общие intermediates смешивали бы варианты. | **Исправлено авторами и прочитано в обоих designs:** абсолютные per-Variant/per-project пути, `AppendTargetFrameworkToOutputPath=false`, `AppendRuntimeIdentifierToOutputPath=false`, отдельные BaseIntermediateOutputPath/MSBuildProjectExtensionsPath до imports. Compile этим не подтверждён. |

Доказательство замечания о Wiki — [исходный donor Program.cs](https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/Program.cs#L24): наличие output проверяется перед learner, `Directory.CreateDirectory(output)` и запись raw стоят после обучения и оценки. Исправляется только будущая экспериментальная оболочка; старый scaffold не менялся.

## Соответствие текущим требованиям

| Правило пользователя | Что проверено в четырёх пакетах |
|---|---|
| 1. Малый эксперимент, желательно по домену | Конечные fixed counts; одна A/B-переменная; literal TRAIN/EXAM; C# documentation, BPE C#/EN→RU и lexical Wiki обозначены отдельно. |
| 2. Любое полезное измеренное улучшение считается успехом | Явно присутствует во всех карточках и prompts; accuracy не обязательна, все ухудшения рядом. Diagnostic counters не подменяют полезный показатель. |
| 3. Измеряемое должно участвовать в обучении | BPE/Wiki меняют входы настоящего writer; DPO Prepare действительно передаётся StepVector. Непрошедший update не считается ускоренным обучением. |
| 4. Для зависимости обучения — конкретный original код и способ исправления | Названы CreateBatch/PrepareExecution, BpeTokenizer.Train/CountPairs/MergeAll, Wiki Train/Featurize; указан будущий способ переноса и его пределы. |
| 5. Сначала разблокировать обучение | Новый документационный комплект не запускает периферийные проверки; отсутствие host/ABI/B0 обозначено точно и не маскируется генератором или чужим runner. |
| 6. Исходный код копируется в опыт, результат раньше внедрения | Native copies и authority boundary заданы; нет product patch, full CI или исправления всего дерева тестов. |
| 7. Исполнитель не выбирает размещение/модель/команды/разрешение конфликтов | Короткие буквальные пути, fixture bytes/JSON, source pin, зависимости, CLI и failed-run поведение заданы. Доступность DLL/SDK/B0 не выдумана; остаток подготовки назван. |
| 8. Нельзя удалять старые эксперименты | Добавление новых карточек, append-only новые attempts, no retry/cleanup. Wiki замечание выше отделяет заявленное правило от механизма фиксации partial run. |
| 9. Приоритеты и конфликты | Устранены конкретные блоки исполнения; model default, read-only protected и STOP/NEXT главнее старой policy. Не добавлены абстрактные approval gates. |
| 10. Послабления эксперимента не разрешены prod/submitting | Source-copy и тонкие throw/CLI оболочки не становятся product изменениями. Native learner не заменяется toy вычислением. |
| 11. Сейчас только документация и кандидаты | У всех NOT_RUN, нет вымышленных measured gains; будущий выигрыш регистрируется candidate.md и решение оставляется Любе. |
| 12. Формат отчёта, язык и скилы заранее | Русский Markdown, точный report-path, fixed sections, краткие пояснения плюс raw; точные skills, default model отдельно от обучаемого алгоритма. |
| 13. Запрет продолжать прошлое после NEXT/STOP | В каждой карточке и каждом готовом prompt; после NEXT нельзя задерживать переход ради hashes/review/report/CI, после STOP только необходимая остановка своего процесса. |

## Какие ограничения остаются после document review

Список исходников или точная будущая команда не доказывают существование executable. У DPO остаются материальные предпосылки полного assembly boundary и подтверждённого B0; у BPE — реализация weighted алгоритма с исходными tie-break; у Wiki — реализация ровно одной source-copy правки и указанной оболочки. Никакая из них не подтверждалась новой сборкой или training.

Передача исполнителю должна сохранять те же literal данные, модель, budget и measurement window. Если будущее измерение покажет ресурсный выигрыш при неизменной точности, он остаётся полезным кандидатом. Если обнаружится divergence, неуспешная подготовка, невыполненный writer или неразрешимая ABI-граница, такой результат сохраняется с точным этапом; заменить его успешным нулём или новым незаданным опытом нельзя.
