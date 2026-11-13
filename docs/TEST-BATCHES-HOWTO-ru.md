# Батчевый прогон тестов: как читать canonical-доказательство

Canonical-прогон запускает строки из `tools/test-batches/batches.tsv` строго
последовательно и пишет доказательства в `TestResults/batches-<slice>/run-<UTC>Z`.
Цель — не сделать красное дерево зелёным, а не потерять результаты остальных
групп, когда одна группа падает или зависает.

Папки результатов тестов не являются кодом. Исторические `Tests/TestResults`
сняты с отслеживания, а новые прогоны должны писать только в исключённые
каталоги вида `TestResults/...`. Если доказательство нужно сохранить для
обсуждения, в отчёте указывают путь и хеш, но не добавляют сам TRX, XML-журнал
или dump в git.

## Состояние корня

`state.txt = FINISHED` означает, что очередь дошла до конца опубликованного
манифеста. Это не означает full-suite green. Зелёный suite можно заявлять только
если все строки журнала доказаны, нет product-падений, а все not-executed строки
законно названы.

Если повтор той же команды с тем же `RunRoot` завершил ранее неполный прогон,
runner удаляет старый `incomplete-reason.txt` при записи `FINISHED`. Иначе
вчерашняя причина обрыва выглядела бы как текущий статус уже закрытого корня.

`state.txt = INCOMPLETE` оставлен для настоящего незавершённого evidence:
plan-only режима, ручных ограничителей `--max-leaves` / `--max-elapsed-minutes`,
ошибки fingerprint/discovery или незакрытого `STARTED` / `RUNNING` leaf.

## Timeout leaf

Если отдельный leaf оборван внешним `timeout`, runner пишет `LEAF_TIMED_OUT`.
Такой leaf:

- не продвигает `result.trx` в корень leaf-папки;
- учитывает все ожидаемые строки leaf как `not_executed`;
- делает родительский batch красным (`COMPLETED_RED`);
- не останавливает остальные строки canonical-очереди.

Verifier проверяет это как красное/blocked evidence, а не как зелёный TRX.

Короткая самопроверка защиты от зависшего процесса не запускает продуктовые
тесты. Она подменяет `dotnet vstest` синтетическим процессом: discovery отдаёт
один тест, сам запуск пишет частичный `result.trx`, затем спит дольше короткого
внешнего лимита. Runner должен завершить попытку внешним `timeout`, записать
`LEAF_TIMED_OUT`, оставить родительскую группу красной, сохранить stdout,
stderr, команду и `blame-hang.runsettings`, а заранее положенный старый зелёный
`result.trx` не должен стать доказательством успеха текущей попытки.

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-hang-guard \
  --run-root "TestResults/hang-guard-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

Эта проверка доказывает только одно: защита запуска от зависшего тестового
процесса работает и пишет читаемое evidence. Она не доказывает, что полный suite
зелёный, что конкретная продуктовая группа починена, или что Windows-only тесты
можно считать исполненными в Linux/WSL.

В leaf-mode корневая batch-строка берёт `started_utc`, `finished_utc` и
`seconds` из leaf-журнала. Если нужно проверить старый RunRoot, где parent
`seconds` ещё равен нулю, считайте время по `leaf-journal.tsv`, не по
batch-строке.

## Resume и journal projection

Возобновление делается той же командой и тем же `RunRoot`: новый каталог не
создаётся, закрытые batch/leaf пропускаются, а незакрытые выполняются дальше.
`journal.tsv` остаётся append-only event log. Поэтому при resume один batch
может иметь две строки: старую `LEAF_INCOMPLETE` и новую terminal-строку.

Runner summary и verifier считают текущий итог только по последней строке
каждого batch в порядке manifest. Raw event count печатается отдельно:
`run_root_rows` — projected current rows, `run_root_raw_journal_rows` — все
строки журнала без заголовка.

Хороший пример: `LEAF_INCOMPLETE` затем `COMPLETED_GREEN` для того же batch —
projected rows = 1, raw rows = 2, итог берёт terminal counts. Плохой пример:
две разные terminal-строки одного batch с разными counts или result path —
verifier обязан вернуть проблему evidence, а не спрятать конфликт последней
строкой.

## Theory/data rows

`leaf-plan.tsv` хранит оценку строк для упаковки leaf-паков. Если VSTest
возвращает больше строк для того же запланированного метода, verifier принимает
фактические counters из TRX: это обычные theory/data rows. Если TRX содержит
чужой метод или строк меньше плана, leaf остаётся недействительным.

Хороший пример: manifest запланировал один `[Theory]`-метод как одну строку,
а TRX вернул шесть data rows того же fully-qualified method. Это больше
плана, но метод не чужой — leaf доказан фактическими counters.

Плохой пример: TRX содержит метод, которого нет в `leaf-methods.tsv`, или
возвращает ноль строк для запланированного метода. Это уже не theory expansion,
а недействительное evidence.

## Нижний порог строк результата

Минимальное число результатов (`min_rows`) из `tools/test-batches/batches.tsv`
проверяется по физическим строкам `UnitTestResult`, которые реально прочитаны
из TRX. Число найденных методов, оценка из `leaf-plan.tsv`, ожидаемые строки
таймаута и итоговая строка журнала не могут закрыть недобор. Если группа из
небольших порций (`leaf-pack`) или обычная группа (`batch`) завершилась зелёным
или красным состоянием, но дала меньше физических строк, чем обещает
`min_rows`, запускатель (`runner`) пишет `INVALID_TOO_FEW_ROWS`, а проверяющий
скрипт (`verifier`) возвращает ошибку. Флаг `-AllowIncomplete` разрешает только
честный незакрытый каталог результатов (`RunRoot`) после
`MaxLeaves`/`MaxElapsedMinutes`; он не превращает старый зелёный результат ниже
порога в принятый результат.

Явные `NotExecuted` остаются строками результата и входят в `total`, но не
прибавляются к `passed`. Лишние строки того же `[Theory]`-метода принимаются как
фактическое расширение данных, если они принадлежат тому же методу; чужие или
пропавшие строки остаются недействительными результатами проверки (`evidence`).

Самопроверка нового порога не требует сборки .NET и запускается из WSL в новый
каталог `TestResults`:

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-min-rows \
  --run-root "TestResults/min-rows-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

## Выбор тестов по классу перед точным запуском методов

Лестница групп назначает тесты по имени класса, а не по словам внутри имени
метода. Запускатель (`run-test-batches.sh`) сначала снимает полный список
тестов выбранной сборки через `dotnet vstest ... /ListTests`, для каждого метода
находит первого владельца по классу, а затем запускает уже точный список
методов. Это защищает от случаев, когда слово из более ранней группы встречается
только в имени метода чужого класса.

Для больших групп точный список методов может быть длиннее безопасного размера
аргументов Linux. Поэтому запускатель кладёт фильтр в файл настроек VSTest
(`.runsettings`), а в командную строку передаёт только путь к этому файлу.
Проверяющий скрипт (`verify-test-batches.ps1`) сверяет закрытые результаты с
полным сохранённым списком `ListTests`, а не только с теми методами, которые уже
попали в производный план.

При проверке точных человеческих групп проверяющий скрипт сначала один раз
строит карту «полное имя класса + сборка → первая группа» для текущего основного
списка групп. Эту карту используют только в рамках одного вызова, чтобы не
повторять один и тот же обход списка для каждого защитного правила. Если строка
списка изменилась или в карте нет нужного класса, владелец класса пересчитывается
заново по текущим строкам; это сохраняет защиту от потерянного класса, чужой
сборки, похожего имени класса и такого же короткого имени в другом пространстве
имён: их нельзя ошибочно включить в целевую группу.

Внутри этого же вызова проверяющий скрипт один раз разбирает фильтры строк
списка на положительные и отрицательные условия. Это не долговременный кэш:
подготовленный разбор используется только пока исходная строка фильтра совпадает
с ним байт-в-байт. Если фильтр изменился, даже только регистром имени оператора,
скрипт возвращается к свежей проверке старым путём. Неподдержанная ветка фильтра
ведёт себя лениво, как раньше: если до неё не дошло вычисление, она не ломает
совпавшую строку; если дошло — проверка завершается ошибкой.

Короткая самопроверка ускоренного разбора не запускает тесты .NET:

```powershell
pwsh -NoProfile -File .\tools\test-batches\verify-test-batches.ps1 -SelfCheckPreparedFilterParsing
```

Подготовительный полный список тестов тоже ограничен по времени: запускатель
использует тот же предел, что и для маленьких порций (`--leaf-timeout`), пишет
отдельные `list-tests.command.txt`, `list-tests.started-utc.txt`,
`list-tests.finished-utc.txt`, `list-tests.seconds.txt`, stdout/stderr и
`list-tests.exit.txt`. Если discovery завис или завершился ненулевым кодом,
старый кэш и старый план не принимаются.

Самопроверка этого выбора не требует сборки .NET и запускается из корня
репозитория в WSL в новый каталог результатов:

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-class-first-selection \
  --run-root "TestResults/class-first-selection-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

## Подтверждённо пустая остаточная группа

Последняя остаточная строка основного списка групп, где фильтр записан
звёздочкой (`*`), может оказаться действительно пустой после того, как все
методы уже забрали более ранние именные группы. В этом единственном случае
запускатель (`run-test-batches.sh`) пишет отдельное состояние `EMPTY_VERIFIED`:
он сохраняет полный список методов этой сборки, производное назначение методов
по классам, привязку к текущему коммиту, контрольные суммы сборки, списка групп,
запускателя и проверяющего скрипта. Затем он не создаёт попытку запуска тестов
через VSTest, не пишет файл результата (`TRX`) и не запускает пустой фильтр или
`*`.

Такая строка не означает «ноль прошедших тестов». В журнале у неё `total=0`,
`passed=0`, `failed=0`, `not_executed=0`, а сводка считает её в отдельной строке
«Подтверждённо пустых остаточных групп». Нижний порог результатов (`min_rows`)
по-прежнему обязателен для всех непустых запусков; пустота разрешена только для
последней остаточной строки основного списка групп, когда полный список тестов,
полученный командой discovery (`ListTests`), непустой и независимо показывает,
что остаточных методов нет.

Проверяющий скрипт (`verify-test-batches.ps1`) не доверяет одной строке
журнала: он заново распределяет сохранённый полный список тестов по текущей
лестнице групп и отвергает именованную пустую группу, произвольный список групп
или выбор отдельных методов (`custom`/`focused`), устаревшую контрольную сумму,
пустой или неуспешно полученный список тестов, изменение сохранённого списка и
появившийся новый метод в остатке.

Если повтор запуска в уже существующем каталоге результатов обнаружил, что
раньше эта группа была непустой и в папке остались leaf-журнал, попытка или TRX,
запускатель останавливается до перезаписи файлов и просит указать новый
`--run-root`. Это сохраняет старые доказательства byte-for-byte: пустая
квитанция должна появляться в свежей папке либо поверх собственной прежней
`EMPTY_VERIFIED` квитанции, но не поверх непустого запуска.

Быстрая самопроверка этой ветви не требует сборки .NET и запускается из корня
репозитория в WSL:

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-empty-residual \
  --run-root "TestResults/empty-residual-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

У проверяющего скрипта есть отдельная короткая самопроверка той же квитанции:

```powershell
C:\Users\lubao\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell\pwsh.exe `
  -NoLogo -NoProfile `
  -File tools\test-batches\verify-test-batches.ps1 `
  -SelfCheckEmptyResidual
```

## Source-only проверка verifier'а

Verifier сканирует исходники `Tests` и `Tests.ApiDashboardWpf`, чтобы проверить
полноту canonical-манифеста. Он намеренно не входит в `bin` и `obj`: эти папки
принадлежат сборке и VSTest, внутри них бывают временные `TestResults`, которые
могут исчезнуть или стать неполными независимо от опубликованного RunRoot.

Хороший пример: verifier читает `Tests/FooTests.cs` и распределяет найденный
public test class по batch-манифесту.

Плохой пример: verifier рекурсивно заходит в
`Tests/bin/Release/net9.0/TestResults/...` и падает на исчезнувшем временном
каталоге. Это инфраструктурная ошибка обхода, а не красный product-test.

## Платформенный пропуск

Windows-only batch в Linux/WSL пишется как `SKIPPED_PLATFORM` с причиной и
количеством строк. Это честная граница среды, не passed-строки и не скрытое
исключение из манифеста. Даже в focused manifest такой skip остаётся batch-only
evidence без leaf/TRX: verifier принимает его только если `total` равен
manifest `min_rows`, `passed=0`, `failed=0`, а все строки учтены как
`not_executed`.

Qwen quality canary имеет ещё одну точную границу: без Windows или без
`XIIGEN_RUN_LOCAL_QWEN_JUDGE_CANARY=1` он должен быть discovery-time
`Skip`/`NotExecuted`. При явном Windows opt-in отсутствие production-пинов,
ошибка модели или неверный выбор остаются `Failed`. Старый
`$XunitDynamicSkip$WINDOWS_ONLY_RUNTIME:` verifier нормализует только для
точного canary FQN как историческое evidence; сегодняшняя env verifier-процесса
не должна менять старые counts. Другой dynamic-skip prefix, другой FQN и
реальная ошибка качества остаются красными, а свежий Windows opt-in canary
обязан дать настоящий 1/1 pass либо настоящий fail.

## Inventory guard лестницы

`TestSuiteBatchInventoryGuardTests` — это не ещё один способ запустить suite.
Он проверяет карту до прогона: читает `tools/test-batches/batches.tsv`,
находит текущие тестовые классы в трёх тестовых сборках, учитывает linked
source-файлы и проверяет, что каждый класс получает ровно одно first-match
назначение. Windows-only `Tests.ApiDashboardWpf` остаётся в описи даже при
запуске из Linux/WSL: среда может честно пропустить исполнение, но не имеет
права стереть сборку из манифеста.

Основной список групп сейчас состоит из 32 групп: 31 именованной человеческой
групп и одна остаточная страховочная строка. Историческая отметка раннего
среза — 25 человеческих групп — больше не является текущим счётом. Группа
`training-entrypoints-and-admission` забирает шесть входных классов обучения:
legacy-проверку допуска DPO-пары, карантин старых DPO-артефактов, конфиг
модели, базовый тренер, предобучение и планировщик. Раньше эти классы доходили
до wildcard-остатка и терялись среди десятков несвязанных тестов, а теперь
падение сразу указывает на конкретный вход обучения. Группа
`compression-codecs` забирает восемь тестовых классов алгоритмов сжатия,
которые раньше попадали в страховочную корзину только из-за отсутствия
namespace. Оба шага сжимают остаточный batch
`core-m-z-and-remainder`: входы обучения нужны для ближайших правок DPO и
предобучения, а кодеки остаются быстрым алгоритмическим маршрутом без серверов,
обучения и внешних процессов.
Группа `model-core-math-tokenizer` забирает пятнадцать тестовых классов
математики модели, тензоров, sampling, tokenizer и transformer. Это следующий
маленький диагностический адрес после кодеков: если ломается вычислительное
ядро модели, оно больше не прячется среди десятков несвязанных M-Z классов.
Неизменяемый baseline слабых классов при этом не расширяется: из активного
snapshot удалены только те строки, которые получили сильный `FullyQualifiedName~`
маршрут в `batches.tsv`. Для шести входов обучения используется полный class FQN,
поэтому маршрут остаётся точным для класса без focused-синтаксиса `FullyQualifiedName=`.
Тело baseline дополнительно пришито к коду через
SHA-256 `bf61271266a0ba29510763c634fffcc3fa41706b21bdd5d644bcfe97df5fe9bc`,
поэтому нельзя тихо добавить новый слабый класс в оба snapshot-файла и
пересчитать их внутренние header-хеши.

Группа `model-activation` больше не является wildcard-строкой для отдельной
сборки включения модели конверсии дизайна. В canonical TSV она перечисляет
ровно шесть существующих классов через class-boundary термы вида
`FullyQualifiedName~DesignConversionHumanApprovalTests.`: точка после имени
класса нужна runtime-фильтру VSTest, который видит методы как `Class.Method`.
Поэтому будущий `DesignConversionHumanApprovalTestsLookalike` не попадёт в
этот batch и не изменит время targeted-прогона; inventory guard/verifier
потребуют отдельный человеческий маршрут. После одного этого сжатия active weak
snapshot содержал девять строк; WPF-срез ниже сжимает его ещё раз.

Группа `api-dashboard-wpf-windows` тоже больше не является wildcard-строкой для
отдельной Windows-сборки настольной панели наблюдения. Сейчас в
`Tests.ApiDashboardWpf` есть один public test-класс
`ApiDashboardWindowInteractiveTests` и девять `[Fact]`-проверок, поэтому TSV
задаёт class-boundary фильтр `FullyQualifiedName~ApiDashboardWindowInteractiveTests.`
и оставляет `min_rows=9`. Точка после имени класса важна: runtime-фильтр
совпадает с самим классом и его методами `ApiDashboardWindowInteractiveTests.*`,
но будущий `ApiDashboardWindowInteractiveTestsLookalike` не расширит этот
Windows-batch молча. Такой новый класс должен получить отдельный человеческий
маршрут или осознанное включение в существующую группу. После WPF-среза
canonical manifest содержал 24 группы, а active weak snapshot — восемь строк;
следующий server-owned replay/passport срез сжимает остаток ещё раз.
Этот source-only split не доказывает Windows/WPF runtime: в Linux/WSL
`api-dashboard-wpf-windows` по-прежнему должен читаться как честный
`SKIPPED_PLATFORM` на девять not-executed строк, а не как зелёный запуск окна.

Группа `server-owned-replay-manifest-attestation` забирает ровно один класс
`LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests`.
В нём три source-метода и пять физических xUnit-строк: одна `[Theory]`
разворачивается в три случая конфигурации HMAC, а два `[Fact]` проверяют
replay-attestation и passport исполняемого manifest. TSV использует
class-boundary фильтр
`FullyQualifiedName~LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests.`:
точка нужна, чтобы VSTest совпал с `Class.Method`, но будущий
`ServerOwnedLogicalCapabilityReplayVerificationCompositionTestsLookalike` не
попал в эту короткую группу. После этого сжатия canonical manifest содержал
25 групп: 24 строки с явным `FullyQualifiedName~...` сигналом и один
wildcard-остаток `core-m-z-and-remainder`; следующий Phase 08 lifecycle срез
сжимает остаток ещё раз.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-server-owned-replay-manifest-attestation"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "server-owned-replay-manifest-attestation" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой targeted-запуск доказывает только server-owned replay/passport группу и
не является full-suite green.

Группа `phase08-common-user-two-leg-lifecycle` забирает ровно один класс
`LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests`. В нём три
`[Fact]`-метода, поэтому `batches.tsv` ставит `min_rows=3`. Фильтр
`FullyQualifiedName~LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests.`
является class-boundary: он совпадает с методами `Class.Method`, но не захватит
будущий `Phase08CommonUserTwoLegLifecycleTestsLookalike`. После предыдущего
Phase 08-сжатия исторически было 26 групп и шесть слабых строк. После
singleton-среза исторически стало 27 групп: 26 именованных и одна остаточная.
Срез структуры потоков и иерархии исправления результата исторически довёл
лестницу до 28 групп: 27 именованных и одна остаточная. Текущие три группы
записи пар, выполнения графа и защищённого каталога сохраняемых моделей
довели лестницу до 31 группы: 30 именованных и одной остаточной. Следующее
выделение медленного локального сервера довело текущий список до 32 групп: 31
именованной и одной остаточной, а перечень классов без собственного точного
маршрута — до пустого активного снимка.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-phase08-common-user-two-leg-lifecycle"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "phase08-common-user-two-leg-lifecycle" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой targeted-запуск отвечает только за переносимый профиль, одобренный обмен,
продолжение обучения и доказательство отката Phase 08; он не является
full-suite green.

Группа `singleton-flow-node-identity` забирает ровно один класс
`LlmFromScratch.Tests.SingletonIdentityTests` из `UniversalFlowSingletonTests.cs`.
Четыре проверки подтверждают, что повторное создание схемы работы возвращает те
же объекты потоков и узлов и не добавляет повторные регистрации. Их отдельный
запуск не проверяет весь проект. Соседние классы
`ConcurrentExecutionIsolationTests` и `DeduplicationTests` из того же файла
остаются в своих группах. Фильтр
`FullyQualifiedName~LlmFromScratch.Tests.SingletonIdentityTests.` указан как
технический пример после человеческого описания: завершающая точка отделяет имя
нужного класса от похожих имён вроде `SingletonIdentityTestsLookalike`. На
момент singleton-среза было 27 групп: 26 именованных и одна остаточная; это
исторический счёт после следующего выделения. Срез структуры потоков и
иерархии исправления результата исторически довёл лестницу до 28 групп: 27
именованных и одна остаточная. Текущие три группы записи пар, выполнения
графа и защищённого каталога сохраняемых моделей довели лестницу до 31
группы: 30 именованных и одной остаточной. Следующее выделение медленного
локального сервера довело текущий список до 32 групп: 31 именованной и одной
остаточной. Перечень классов без собственного точного маршрута теперь пуст;
это не удаляет страховочную остаточную строку.
Исходный перечень классов без точного маршрута остаётся неизменным: его нельзя
расширять новыми классами.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-singleton-flow-node-identity"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "singleton-flow-node-identity" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой отдельный запуск доказывает только общие объекты потоков и узлов и
отсутствие повторной регистрации; состояние всего проекта он не закрывает.

Группа `logical-topology-and-result-gap` отделяет два оставшихся лёгких класса
проверки логической структуры: `LlmFromScratch.Tests.TopologyMutationFlowTests`
и `ResultGapLogicalHierarchyTests`. Первый класс содержит шестнадцать `[Fact]`
о мутациях структуры потоков, второй — четыре `[Fact]` о связи узла, потока и
оркестратора при исправлении результата. Поэтому `min_rows=20` — это
двадцать физических проверок, а не два класса. TSV использует фильтр с
границей полного имени класса
`FullyQualifiedName~LlmFromScratch.Tests.TopologyMutationFlowTests.|FullyQualifiedName~ResultGapLogicalHierarchyTests.`:
завершающая точка нужна фильтру запуска VSTest, который видит методы как
`Class.Method`, и не захватывает будущие
`TopologyMutationFlowTestsLookalike` или `ResultGapLogicalHierarchyTestsLookalike`.
На момент этого сжатия исторически было 28 групп: 27 именованных и одна
остаточная. Перечень классов без собственного точного маршрута содержал три
старые строки; неизменяемый исходный перечень слабых классов не меняется.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-logical-topology-and-result-gap"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "logical-topology-and-result-gap" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот отдельный запуск подтверждает только структуру потоков и иерархию
исправления результата; он не подтверждает прохождение всех тестов проекта.

Группа `result-gap-training-pair-writer` отделяет класс
`ResultGapPhase07DpoWriterTests`, который записывает обезличенные обучающие
пары и строки исправлений. В классе шесть обычных тестов, поэтому `min_rows=6`
означает шесть выполнений, а не качество обучения. Фильтр
`FullyQualifiedName~ResultGapPhase07DpoWriterTests.` заканчивается точкой:
она удерживает запуск на полном имени класса и не захватывает похожее имя.
В TSV эта точная строка стоит ранним порядком 11, сразу после
`design-conversion-remainder-n-z`, то есть до широких `dynamic-graph`,
`teaching-translation-dpo` и `orchestration-skills-wiki`: запускалка проверяет
группы по первому совпадению и не успевает забрать её в широкий маршрут по
слову из имени метода.

Короткий запуск записи пар:

```powershell
$runRoot = ".\TestResults\batches-result-gap-training-pair-writer"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "result-gap-training-pair-writer" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот отдельный запуск подтверждает только маршрут записи обезличенных пар и
исправлений; он не запускает обучение модели и не доказывает качество обучения.

Группа `single-combination-graph-runner` отделяет класс
`SingleCombinationGraphCandidateRunnerTests`, который проверяет выполнение
одного варианта графа, отмену и превышение времени. Четырнадцать обычных
тестов и два теста с двумя наборами входных данных дают 18 выполнений, поэтому
`min_rows=18`. Фильтр
`FullyQualifiedName~SingleCombinationGraphCandidateRunnerTests.` тоже
заканчивается точкой и удерживает запуск на полном имени класса.
Эта точная строка стоит ранним порядком 12, до широких графовых, обучающих и
групп оркестрации. Поэтому все 18 строк выполнения тестов остаются в своей
группе,
включая метод
`PublicSurface_IsOneAsyncCandidateMethodWithoutMultiPlanOrchestrationOrTrainingAuthority`,
где слово `Orchestration` находится только в имени метода, а не в имени класса.

Короткий запуск выполнения графа:

```powershell
$runRoot = ".\TestResults\batches-single-combination-graph-runner"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "single-combination-graph-runner" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Группа `retained-domain-model-registry` отделяет класс
`RetainedDomainModelRegistryTests`, который проверяет защищённый каталог
сохраняемых моделей. В Linux короткий запуск даёт 42 строки результата: три
переносимые строки выполняются, а 39 Windows-only строк честно учитываются как
NotExecuted. Это не зелёная подмена и не пропуск всей группы. Фильтр
`FullyQualifiedName~RetainedDomainModelRegistryTests.` заканчивается точкой,
поэтому будущий `RetainedDomainModelRegistryTestsLookalike` не расширит группу
молча.

Короткий запуск защищённого каталога вместе с остатком:

```powershell
$runRoot = ".\TestResults\batches-retained-domain-model-registry"
pwsh -NoProfile -File .\tools\test-batches\run-test-batches.ps1 -Only "retained-domain-model-registry,core-m-z-and-remainder" -RunRoot $runRoot -LeafMode pack -NoBuild
pwsh -NoProfile -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

После выделения медленного локального сервера всего 32 группы: 31 именованная
и одна остаточная. Активный
перечень классов без собственного точного маршрута пуст; остаточная строка
остаётся страховкой для будущих новых классов и должна подтверждаться как
пустая отдельным состоянием, а не пустым запуском тестов.

Пятнадцать классов группы `model-core-math-tokenizer` перечислены в TSV полными
именами классов: `MachineLearningTests`, `MatrixAlgorithmsTests`,
`MeanPoolingTests`, `NpyReaderTests`, `NpyWriterTests`,
`RotaryEmbeddingPositionOffsetTests`, `SamplerTests`, `SimdMatMulTests`,
`TensorCoreTests`, `TensorOpsCausalMaskWithPastTests`,
`TextGeneratorSeededSamplingTests`, `TokenizerEdgeCaseTests`, `TokenizerTests`,
`TransformerKnnTokenizerE15Tests` и `TransformerModelTests`. Короткие термы
вида `Tokenizer`, `Transformer`, `Tensor` и `Npy` здесь запрещены: они могут
зацепить чужой метод по имени, даже если класс относится к другой группе.

Группа `semantic-compiler-search-vector` забирает десять классов семантической
компиляции, поиска и vector search: `OssEquivalentsTests`,
`OssEquivalentsV4Tests`, `PatternFusionTests`, `RoslynCodeAnalyzerTests`,
`SearchEngineTests`, `SemanticCompiler29Tests`, `SemanticCompilerTests`,
`SemanticNlpTests`, `VectorSearchEdgeCaseTests` и `VectorSearchTests`. Эти
классы раньше доходили до `core-m-z-and-remainder`, поэтому правка поисковой
или компиляторной логики требовала смотреть на большой остаточный batch. В TSV
они перечислены полными именами классов с namespace `LlmFromScratch.Tests`,
чтобы фильтр не сработал по похожему имени метода в чужой группе. Зелёный
результат этой группы означает только быстрый сигнал по поиску и семантическому
компилятору; он не является полным зелёным прогоном проекта.

Группа `orchestration-routing-product-boundaries` забирает семь классов
оркестрации, маршрутизации и границ подключения продукта:
`MultiSubPhaseRouterLearnedAuthorityTests`, `OrchestratorModelTests`,
`PluginBoundaryTests`, `PromptRoutingInputCompilerUnitTests`,
`ReflectionWiringTests`, `SolutionProductProjectRegistrationTests` и
`ThinClientApiContractTests`. Это путь “prompt -> compiled request -> route
choice -> orchestrator/model -> reflection wiring -> plugin/thin-client
contract”, вынесенный из общей корзины в короткий диагностический batch.
`ProjectTaskOrchestrationPipelineTests` сюда не добавляется: он уже получает
сильный маршрут в группе `orchestration-skills-wiki`, которая стоит выше по
first-match лестнице.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-orchestration-routing-product-boundaries"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "orchestration-routing-product-boundaries" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой короткий запуск нужен, чтобы быстро увидеть поломку маршрутизации,
связей и plugin-поверхности. Он не является зелёным полным прогоном: полный
статус проекта появляется только из сводки по всем группам актуального списка.

Группа `design-conversion-live-local-server-method-diagnostic` стоит первой в
основной лестнице и забирает только глобальный класс
`DesignConversionLiveLocalServerTests` из основной тестовой сборки. Фильтр
`FullyQualifiedName~DesignConversionLiveLocalServerTests.` намеренно содержит
точку после имени класса: он совпадает с методами `Class.Method`, но не
расширяется на `DesignConversionLiveLocalServerTestsExtended` и одноимённые
классы в другом namespace. Старая группа `design-conversion-local-host`
остаётся второй и сохраняет семейный терм `DesignConversionLiveLocalServer`,
чтобы похожие классы по-прежнему шли в обычную локальную порцию, а не
становились скрытым расширением новой группы.

У новой группы есть обязательная строка в
`tools/test-batches/leaf-mode-overrides.tsv`: каждый метод медленного класса
получает отдельную порцию и отдельный файл результата. Свежий plan-only для
текущей DLL показал 46 методов, 46 порций и 60 оценочных строк. Это не
доказательство прохождения всех серверных тестов; зелёным можно называть только
закрытый RunRoot с файлами результата. Если у класса или его xUnit-коллекции
появится настоящий общий ресурс тестов (`IClassFixture<...>`,
`ICollectionFixture<...>` или запись в `leaf-shared-collections.tsv`),
планирование этой группы должно остановиться fail-closed, а не разрезать общий
ресурс по методам. Это узкий консервативный отказ, а не универсальный C#-анализ:
любой список базовых классов или интерфейсов у точного класса, общий объект
текущей коллекции, полученный через наследование или псевдоним типа, такой же
список в любой partial-части definition-класса этой коллекции, другая коллекция
или отсутствующее описание текущей коллекции считаются небезопасными для
автоматического запуска по методам.

Группа `test-infrastructure-evidence-measurement` забирает пять классов,
которые проверяют не продуктовый сценарий, а честность самой проверочной
машины: `CrossStackFlowSeedHonestyTests`, `MeasurementNoisePolicyTests`,
`TestOwnedOutputRootTests`, `TestSuiteBatchInventoryGuardTests` и
`WorkLeftRagEvidenceIndexTests`. Раньше они жили в wildcard-остатке; теперь
поломка batch-инструментов, RAG-описи или политики шума измерений видна через
короткий targeted-запуск этой группы.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-test-infrastructure-evidence-measurement"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "test-infrastructure-evidence-measurement" -RunRoot $runRoot
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой короткий запуск отвечает только на вопрос: "живы ли инструменты
тестовой описи, изоляции временных файлов, RAG-индекса и политика измерительного
шума". Он не заменяет сводку по всем группам актуального списка.

Эта группа тоже имеет живой override в
`tools/test-batches/leaf-mode-overrides.tsv`: методы guard-а строятся в режиме
`method-diagnostic`, то есть один метод = одна порция с точным
`FullyQualifiedName=...` фильтром. Class-level назначение в `batches.tsv`
остаётся прежним и продолжает защищать от потерянных классов; меняется только
способ запуска внутри группы. Это нужно потому, что `TestSuiteBatchInventoryGuardTests`
сам проверяет тяжёлые мутации manifest/snapshot/verifier: если один такой метод
завис или покраснел, соседние RAG- и measurement-методы уже записаны отдельными
доказательствами и не сгорают вместе с ним. Fingerprint включает effective mode,
hash override-файла и строку override, поэтому старый pack-run не может быть
переиспользован как method-level evidence при resume.

Если один метод guard-а проверяет несколько плохих вариантов одного и того же
списка групп, он должен один раз построить снимок описи репозитория и
переиспользовать его для всех мутаций. Иначе каждая порча снова запускает
тяжёлый обход тестов: такой метод может зависнуть целиком и разрушить
возобновляемый прогон, потому что вместо одной диагностической порции получится
несколько скрытых полных сканирований.

Если нужна короткая передача работы между заходами, ограничивай запуск через `-MaxLeaves` или
`-MaxElapsedMinutes`: verifier с `-AllowIncomplete` честно покажет закрытые
leaf-и и оставшиеся открытые leaf-и. Такой ограниченный набор полезен для
диагностики, но всё равно не является зелёным полным прогоном и не закрывает всю
группу, если часть leaf-ов ещё не запускалась.

`-MaxLeaves` — это общий бюджет новых попыток выполнения порций (`leaf`) за один
вызов запускалки (`runner`) между всеми выбранными группами. Например
`-MaxLeaves 3` сначала может закрыть две порции первой группы и одну порцию
второй; следующая группа не начнёт лишний тест в том же заходе. Повтор той же
команды с тем же `RunRoot` начинает новый бюджет, пропускает уже закрытые
порции и продолжает со следующей открытой порции. Значение `0` означает "без
ограничения по числу порций".

PowerShell-обёртка с `-ChangedPath`/`-ChangedSince` делит положительный
`-MaxLeaves` между наборами обязательных проверок (`focused manifest`) и основными группами. После
каждого дочернего запуска она читает прирост `leaf-journal.tsv`: новые
leaf-порции уменьшают остаток, а уже закрытая работа при resume не тратит
лимит. Если остаток стал нулём, следующий дочерний процесс не стартует и
обёртка не передаёт `--max-leaves 0`, потому что для bash-запуска это
безлимитный режим.

При явном `RunRoot` короткие обязательные результаты пишутся рядом с ним:
`RunRoot-focused-obligations/<manifest>`. Это стабильная ручка продолжения для
повтора той же команды; основной `RunRoot` не содержит focused leaf-журналы и
не смешивает их в своей сводке. Если короткий focused-каталог завершился
`INCOMPLETE`, покраснел, потерял `state.txt` или имеет повреждённый
`journal.tsv`, PowerShell-обёртка не считает его зелёным и не запускает
основные группы следом.

Если положительный `-MaxLeaves` задан без явного `RunRoot`, PowerShell-обёртка
сама выбирает один основной каталог `TestResults/batches-<slice>/run-<UTC>Z`,
печатает его как путь для продолжения и передаёт его Linux-запуску. Иначе
обёртка не смогла бы честно измерить, сколько leaf-порций реально добавил
основной дочерний процесс.

Положительный `-MaxLeaves` несовместим с `-LeafMode off`: в этом режиме нет
`leaf-journal.tsv`, значит нечем честно списывать общий бюджет между дочерними
процессами. Такая комбинация отказывается до WSL; `-LeafMode off -MaxLeaves 0`
остаётся безлимитным запуском.

`-MaxElapsedMinutes` тоже относится к текущему заходу, но это не жёсткая
гарантия общего времени команды: запускалка проверяет время перед запуском
следующей порции и не убивает уже безопасно начатый VSTest посередине. Оба
лимита записываются в журнал в каталоге результатов для честного отчёта, но не
входят в контрольный отпечаток плана (`fingerprint`). Если же меняется override,
effective leaf mode, filter, source-map, test DLL или другой смысл плана,
продолжение тем же каталогом результатов должно отказать fail-closed через
несовпадение контрольного отпечатка.

При автоматическом запуске PowerShell-обёртки этот временной предел пока
отсчитывается отдельно в каждом дочернем процессе. Общее время всей команды
может быть больше; в данном исправлении общим стал только лимит количества
порций (`MaxLeaves`).

Самопроверка этого межгруппового бюджета не запускает продуктовые тесты: она
использует синтетический `dotnet`, который только имитирует перечисление тестов
(`ListTests`) и файлы результатов (`TRX`):

```bash
bash tools/test-batches/run-test-batches.sh --no-build --self-check-invocation-budget --run-root "TestResults/invocation-budget-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

PowerShell-обёртка имеет отдельную самопроверку с управляемой подменой. Она
запускает копию живой обёртки с подменой выбора групп и запуска Linux
(`synthetic selector`, `fake WSL`), пишет evidence под `TestResults` и
доказывает только управление вызовами, а не продуктовые .NET тесты:

```powershell
pwsh -NoProfile -File .\tools\test-batches\self-check-wrapper-budget.ps1 -EvidenceRoot .\TestResults\wrapper-budget-self-check-<date>
```

История таких заходов хранится отдельно в `RunRoot/run-invocations.tsv`. Это
append-only журнал: каждый вызов runner-а сначала проверяет уже записанную
историю, затем дописывает ровно одну новую строку с временем, выбранными
группами, запрошенным и фактическим leaf-режимом, `MaxLeaves` и
`MaxElapsedMinutes`. Строки связаны hash-цепочкой: у каждой есть hash всех
предыдущих строк и hash собственного содержимого. Поэтому второй resume-заход
не стирает бюджет первого, а verifier видит отсутствующий или испорченный
header, удалённую первую строку, нарушенную последовательность, дубликат
invocation-id, нечисловой бюджет и подмену старой строки как отдельные красные
evidence-факты. Этот журнал нужен для аудита вызовов; он намеренно не входит в
fingerprint leaf-плана.

RAG-проверка внутри этой группы не исключается в WSL. Если исторический путь
записан как `C:\...`, loader переводит его в `/mnt/c/...` до чтения, чтобы
Linux не склеивал Windows-диск с текущим каталогом как относительный хвост.
Источники WorkLeft сначала ищутся в живом каталоге сессии, а после архивации
25.08.2026 - в его `archive`; hash и публичный `workleft://...` остаются теми
же, поэтому проверяется тот же документ, а не новая копия.

Guard отличается от runner/verifier так: runner запускает выбранные строки и
пишет `RunRoot`; verifier проверяет, что `RunRoot`, журналы, fingerprints и TRX
не подменены; inventory guard смотрит на исходники и canonical TSV. Он ловит
новый тестовый класс, который молча упал бы в wildcard-остаток, удалённый или
переехавший класс, лишнюю тестовую сборку без batch-строки, ручной exact-method
фильтр в canonical manifest и разрыв неделимых групп.

`Tests/TestSuiteBatchLegacySnapshot.txt` теперь хранит только слабые классы:
те, которые дошли до wildcard-остатка без явного `FullyQualifiedName~...`
сигнала в `batches.tsv`. Рядом лежит
`Tests/TestSuiteBatchLegacyWeakBaseline.txt` — неизменяемый список старых
слабых классов, который разрешает только сжатие активного snapshot. После
выделения групп оркестрации, тестовой инфраструктуры, включения модели, WPF
панели, server-owned replay/passport, Phase 08 lifecycle, singleton-проверок
общих потоков/узлов, структуры потоков/иерархии исправления результата,
записи пар, выполнения графа и защищённого каталога сохраняемых моделей
перечень классов без собственного точного маршрута пуст. Проверка
сначала сверяет тело baseline с внешним якорем в `TestSuiteBatchManifest`, а
потом сравнивает active snapshot с текущей описью. Если класс получил сильное
правило, его удаляют из активного snapshot; если появился новый слабый класс,
его нельзя добавлять в snapshot, пока для него не выбран человеческий
batch-сигнал в `batches.tsv`.

Хэш считается по LF-канону тела snapshot: CRLF checkout не меняет принятую
опись. Авто-refresh запрещён: красный snapshot — это review-сигнал, а не
просьба тесту переписать самого себя. Сообщение
`new_test_class_without_strong_batch_signal` означает простую вещь: новый тест
пока держится только на общей корзине, поэтому надо добавить осмысленный
`FullyQualifiedName~...` префикс или явно решить, к какой группе он относится.

`DesignConversionLocalIndependentQwenRuntimeCanaryTests` больше не должен
лежать в слабом snapshot: его имя получает сильный сигнал через
`DesignConversionL` и остаётся в batch `design-conversion-remainder-a-m`. Это
не доказательство качества Qwen и не зелёный runtime-canary: контрактный слой
Qwen держит отдельную границу 24 из 24 без обращения к модели, а четыре
реальные production-попытки canary остаются 0 из 4 принятых, пока оператор не
включит Windows-маршрут с opt-in флагом и тремя производственными пинами
модели. В Linux/WSL такой real-model canary без opt-in обязан быть честным
Skip/NotExecuted; его нельзя засчитать зелёным или исключить из inventory,
чтобы спрятать недоказанный runtime.

## Куда попадают новые проверки жизненного цикла документации кода

Это не новое деление тестов и не вторая система батчей. Это маленькая
адаптация уже существующей лестницы первого совпадения: новые проверки того,
как документация кода создаёт DPO-наборы и публикует модельные checkpoints в
репозиторий, должны идти рядом с другими репозиторными обучающими артефактами,
а не падать в большую страховочную корзину остатка.

Правило `repository-owned-training-artifacts` теперь забирает оставшиеся классы,
чьё полное имя начинается с `CodeDocumentation`. Это покрывает новые проверки
жизненного цикла: командную строку батчевого запуска документации кода,
серверную аренду создания DPO-набора, исполнитель обучения на DPO-наборе,
старт репозиторного checkpoint, публикацию checkpoint и координатор полного
пути «сохранённый DPO-набор → дообучение → новая опубликованная модель».

Два более старых класса остаются в `core-a-f`: `CodeDocumentationRoutingFlowTests`
и `CodeDocumentationTrainingRunnerTests`. Их уже забирает более ранняя строка
лестницы, поэтому правило первого совпадения не отдаёт их поздней группе
`repository-owned-training-artifacts`. Это важно: мы не расширяем старый
core-батч и не создаём двойного владельца тестов; мы только не даём новым
жизненным проверкам улететь в последний общий остаток.

Чтобы посмотреть план только этой группы без запуска тестов:

```powershell
.\tools\test-batches\run-test-batches.ps1 -Only "repository-owned-training-artifacts" -PlanOnly -NoBuild -SkipFocusedObligations
```

Чтобы проверить саму таблицу распределения после изменения батчей:

```powershell
.\tools\test-batches\select-impacted-batches.ps1 -SelfCheck
```

## Быстрый выбор затронутых батчей по изменённым файлам

`tools/test-batches/select-impacted-batches.ps1` нужен для первого короткого
прогона после небольшой правки. Он читает изменённые пути, разбирает
C#-исходник тестов синтаксическим деревом Roslyn и назначает найденные классы
по первому совпадению текущего основного списка групп (`batches.tsv`). Активный
слабый снимок больше не является картой владельцев: он может быть пустым, но
остаётся проверяемым контрольным списком старых классов без собственного
точного маршрута. Параметр `SnapshotPath` сохранён для совместимости старых
команд, но не определяет выбранные группы. Это не заменяет полный прогон всех
основных групп перед заявлением full-suite green: selector только помогает не
тратить целый день на разведку очевидно незатронутых групп.

Хороший пример после правки одного тестового файла:

```powershell
.\tools\test-batches\select-impacted-batches.ps1 -ChangedPath Tests\DesignConversionLocalIndependentQwenRuntimeCanaryTests.cs
.\tools\test-batches\run-test-batches.ps1 -ChangedPath Tests\DesignConversionLocalIndependentQwenRuntimeCanaryTests.cs -NoBuild -MaxLeaves 2
```

Первый вызов показывает человеку, почему выбран батч
`design-conversion-remainder-a-m`. Второй вызов передаёт этот выбор запускалке
как `-Only "design-conversion-remainder-a-m"` и оставляет остальные группы
нетронутыми.

Если один C#-файл подключён в две тестовые сборки, selector обязан вернуть оба
маршрута. Например `Tests/DesignConversionModelActivationRegistryTests.cs`
попадает и в основную сборку, и в отдельную сборку включения модели, поэтому
первый короткий запуск должен выбрать оба batch id:
`design-conversion-remainder-a-m,model-activation`.

Для автоматического выбора нужен PowerShell 7 с встроенными сборками Roslyn
(`Microsoft.CodeAnalysis*.dll`) в каталоге самого PowerShell. Если такого
парсера нет, либо файл содержит C#-атрибут метода, который selector не может
надёжно связать с xUnit, скрипт отказывает и просит ручной `-Only`; частичный
зелёный запуск запрещён.

Если путей несколько, передавайте их списком через запятую или через
`-ChangedPathsFile`. Скрипты отключают позиционное сопоставление параметров:
лишний путь не сможет случайно стать `RepoRoot` или другим параметром запуска,
а остановится как явная ошибка вызова.

Пути нормализуются через полный путь внутри репозитория: дубликаты в разном
регистре схлопываются, а `../...` и абсолютные пути за пределами репозитория
останавливаются ошибкой. Это важно, чтобы selector не выбрал batch для файла,
который на самом деле не принадлежит проверяемой рабочей копии.

Для правки самой инфраструктуры батчей selector выбирает основную группу со
стражем описи и отдельно называет короткий набор обязательных проверок
(`focused manifest`) `tools/test-batches/test-suite-batch-inventory-guard-focused.tsv`. Сейчас этот
файл списка перечисляет 43 точных метода guard-а. Это быстрый первый сигнал о
том, что основной список групп, слабый снимок и guard не разошлись. Если
изменён только документ, production-файл без заранее безопасной карты, удалённый
файл или C#-файл с неоднозначным тестовым атрибутом, selector честно пишет, что
нужен ручной выбор `-Only` или отдельный focused manifest; он не выдумывает
покрытие и не возвращает частичный список.

PowerShell-обёртка запускает короткую focused-команду для инфраструктуры до
основной группы, если изменён сам запускатель, selector, слабый снимок или
guard. Это защищает от тихой потери focused manifest: маленький страж сначала
проверяет лестницу, снимок и guard, а только потом можно тратить время на более
тяжелую группу.
Если нужна только эта обязательная короткая проверка, используйте
`-FocusedObligationsOnly`.

Если PowerShell-обёртка запускается с `-ChangedPath`/`-ChangedSince` и
`-PlanOnly`, она останавливается после PowerShell-выбора объёма проверки и не открывает WSL.
Такой режим нужен для быстрой проверки самого выбора: он показывает выбранные
batch id и короткие обязательные проверки, но прямо пишет, что тесты не
выполнялись.

## Что именно чинит этот runner/verifier slice

Первый неполный RunRoot показал, что `timeout` отдельного leaf-пака ошибочно
останавливал всю очередь как `INCOMPLETE`. Теперь такой leaf записывается как
явный `LEAF_TIMED_OUT`, его ожидаемые строки идут в `not_executed`, batch
становится красным, а остальные 17 строк манифеста продолжают выполняться.

Второй неполный RunRoot показал, что `[Theory]`/data rows того же метода
ошибочно считались нарушением membership. Теперь лишние строки того же метода
разрешены, но чужие методы и недобор строк всё ещё запрещены.

Финальная проверка показала третий дефект verifier'а: source-scan заходил в
runtime-папки `bin`/`obj`. Теперь verifier обходит только исходные `.cs`-файлы,
не смешивая опубликованное evidence с временными артефактами VSTest.

Четвёртая review-правка закрывает timing: parent batch row теперь считается
только после строгой проверки каждой leaf-журнальной строки. Timestamp обязан
быть ровно `yyyy-MM-ddTHH:mm:ssZ`, `seconds` — неотрицательным целым текстом,
а обязательные поля не могут быть пустыми. Плохой timestamp или `seconds=2x`
делают batch/root incomplete вместо ложного `FINISHED`.

## Текущее canonical evidence `batches-full-f91d6c36-20260829T193803Z`

RunRoot:
`TestResults/batches-full-f91d6c36-20260829T193803Z`.

Это evidence на срезе `f91d6c36` остаётся product RED, а не full-suite green:

- состояние корня: `FINISHED`;
- область проверки: `CANONICAL`, `focused_mode=false`;
- projected current rows: 18;
- raw journal rows: 19;
- всего test rows: 11571;
- passed: 11188;
- failed: 124;
- not executed: 259;
- Windows-only WPF batch: 9 строк, все 9 записаны как `SKIPPED_PLATFORM`;
- full-suite green не заявляется: есть реальные failed и not-executed строки.

Важно для чтения: лишняя raw-строка — это старый `LEAF_INCOMPLETE` для
`single-serial-groups`, после которого есть terminal `COMPLETED_GREEN`. Она не
удаляется из журнала, но не считается второй раз в projected totals.

Что сделано на том историческом срезе: batching довёл все 20 тогдашних
человеческих групп до учёта, timeout leaf не убил очередь, theory/data rows
того же exact method приняты, verifier отсёк corrupt timing и runtime
`bin`/`obj` noise. Тогда текущий `batches.tsv` вырос до 24 групп. Сейчас в
основном списке 30 групп: последующие срезы добавили новые точные маршруты, а
старые числа в этом абзаце остаются историческим описанием того запуска.

Что осталось: product-test failures и законные not-executed/platform строки.
Этот slice не чинит продуктовые падения и не превращает красный suite в зелёный.

## Как получить один честный итог из готовой папки батчей

Если батчи уже запускались, не надо перечитывать десятки `result.trx` руками и
не надо запускать весь suite заново только ради отчёта. Для этого есть
переносимый сводчик:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\summarize-test-batches.ps1 `
  -RunRoot .\TestResults\batches-<slice>\run-<utc> `
  -AllowIncomplete
```

Он создаёт рядом с `journal.tsv` два файла:

- `summary.md` — общий счёт по батчам: сколько строк результата найдено,
  сколько прошло, сколько упало, сколько не исполнялось и какие батчи остались
  незакрытыми;
- `notrun-report.md` — список неисполненных проверок и порций: Windows-only,
  platform skip, timeout без читаемого TRX и физические `NotExecuted` строки из
  TRX.

Главное правило: сводчик считает физические `UnitTestResult`, а не доверяет
шапке `Counters`. Это защищает от известной ловушки, где TRX пишет
`notExecuted=0` в шапке, хотя внутри есть реальные строки `NotExecuted`.
Одинаковые имена тестов из разных сборок тоже не схлопываются: пока нет
отдельного решения о дедупликации, это разные физические строки результата.

### Дополнение 12.11.2026: откуда взято каждое число и когда итог не является полнотой

Это дополнение к разделу выше; прежние формулировки остаются в силе.

Сводчик теперь называет **происхождение** каждого числа, потому что число из
журнала и число, прочитанное из `result.trx`, — разные доказательства, и после
сложения их было не различить. В `summary.md` появились строки:

- `Происхождение чисел: TRX | JOURNAL_ONLY | MIXED | NONE` — `TRX` значит
  «прочитано из файлов результатов», `JOURNAL_ONLY` — «заявлено журналом о себе»,
  `MIXED` — и то и другое вместе, `NONE` — в итоге нет ни одного числа. Последнее
  значение встречается не только на пустом прогоне: у испорченной подтверждённо
  пустой группы заявленный общий счёт в итог намеренно не импортируется, поэтому
  чисел в итоге нет, а заявление об исполненных проверках названо строкой ниже;
- `Из них заявлено журналом без сырого файла результатов: N`;
- `Заявлено ИСПОЛНЕННЫМИ без сырого подтверждения (NOT_PROVEN_RAW): N` —
  именно этот счёт отличает ложь от честного отсутствия файла. Приставки «из них»
  у него нет намеренно: заявление об исполненных проверках бывает и там, где в
  итог не попало ни одного журнального числа — например у подтверждённо пустой
  группы, чей заявленный общий счёт в итог намеренно не импортируется;
- строка `Подтверждённой полнотой этот итог НЕ является...` печатается только
  когда последний счёт больше нуля.

В таблице батчей добавлены колонки `Происхождение` и `Не доказано сырым`.
Счётчик батчей переименован в `Батчей с прочитанным и подтверждённым
результатом`: он требует не только читаемого состояния журнала, но и того, что
заявленные исполненными проверки подтверждены сырым файлом.

**Что изменилось в поведении.** Батч, у которого журнал объявил путь
`result.trx`, а файла по этому пути нет, больше **не** считается прочитанным и
переводит прогон в незакрытое состояние. Платформенный пропуск и подтверждённо
пустая группа при честной строке журнала этим не затронуты: они заявляют ноль
исполненных проверок, и это правда.

Но **любая** из четырёх ветвей с журнальными числами теперь читает заявленных
исполненными из журнальной строки. Поэтому строка платформенного пропуска или
подтверждённо пустой группы, заявляющая исполненные проверки, называется
предупреждением с именем батча и числом, не попадает в подтверждённые и даёт
код возврата `66`. Заявленный общий счёт при этом в итог не импортируется: ловит
ложь счёт заявленных исполненными, а не арифметика.

**Коды возврата.** `66` — в итоге есть проверки, заявленные исполненными без
сырого файла результатов. Этот код **не снимается** флагом `-AllowIncomplete`:
флаг означает «каталог честно не завершён» и никогда не означал «принять числа
без доказательства». `65` — остались незакрытые батчи; этот код, как и прежде,
`-AllowIncomplete` подавляет. Предупреждение о недоказанной полноте печатается
независимо от флага.

Во внешнем выводе сводчика появилась строка
`provenance=... journalOnly=... notProvenRaw=...` — те же сведения для
вызывающего, без чтения `summary.md`.

Быстрая проверка самого сводчика:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\summarize-test-batches.ps1 -SelfCheck
```

Эта проверка создаёт маленький синтетический пример под `TestResults/`, ловит
две ошибки чтения результата и удаляет только свой точный временный каталог.
