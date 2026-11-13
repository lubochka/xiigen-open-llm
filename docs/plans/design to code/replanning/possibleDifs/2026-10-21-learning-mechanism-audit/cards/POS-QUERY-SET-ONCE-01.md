# POS-QUERY-SET-ONCE-01 — один набор биграмм запроса на чтение обученной POS-памяти

**DESIGNED_NOT_RUN / HOST_NOT_MATERIALIZED. Приоритет: после разблокирования обучения, конфликтов и прямых учебных опытов.** Это новая конкретная гипотеза снижения allocations при использовании выученного результата. Она не меняет алгоритм обучения, Jaccard, признаки, k или голосование и не доказывает исправление neural RP01. Текущая фаза — только документация. Этот паспорт задаёт будущую реализацию после поручения исполнить ID; сейчас не создавать host и не запускать команды.

## Исполнитель, домен и ожидаемый результат

Модель исполнителя и проверяющего — модель текущей сессии по умолчанию, без override. Обучаемый механизм — `SemanticCompiler.PatternClassifier` проекта 12, не модель агента и не H512 Student. Домен — перевод и языковые формулировки, узкая задача определения порядка POS; синтетическая панель не доказывает качество естественного перевода. Отчёт — русский Markdown, до 350 слов пояснений плюс таблицы и ссылки на raw; кандидат — до 120 слов с адресом изменения, измеренной осью, ухудшениями и пределом.

Применить адресно skills из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Указать реально прочитанные разделы; не писать FULL_LOADED для частичного чтения. Старые автоматические Tier escalation/повторы и model overrides не отменяют пользовательский конечный бюджет и текущую модель.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Изменения остальных показателей, включая ухудшения, показываются рядом.** Успех не разрешает автоматически переносить правку в продукт.

**Запрет продолжать прошлую задачу:** при «следующий эксперимент» немедленно перейти к назначенному NEXT, не дописывая прежний отчёт, hash, review, Git или повторный запуск. При STOP / «остановить всё/закончить немедленно» прекратить работу. Незавершённые артефакты сохраняются; запрещено продолжать под видом завершения. У одиночного ID NEXT=END; в данном дополнении это последний ID, NEXT=END.

## Причина и реальное участие в обучении

[Train:39–43][native] записывает POS-последовательности и метки в `_train`; [Predict:46–54][native] читает эту память, ранжирует соседей, берёт k3 и голосует. [SequenceDistance:78–86][distance] строит `bigramsA` для **одного и того же запроса заново на каждой обученной строке**, а также `bigramsB` для строки памяти. В E25 TRAIN=128; один Predict повторяет подготовку query128 раз. Это source count, ещё не измеренная цена.

Путь: native GenerateExamples → native Train128 → изменённая `_train` → native Predict → class output → сравнение с frozen label. Фактор участвует непосредственно в чтении выученной таблицы. Поэтому это допустимый опыт зависимости результата от обученной памяти по правилу пользователя 4, но он идёт последним. Не заменять его отдельными вызовами private distance без Train. Текущий production fusion использует другой rule-classifier проекта 29; данный опыт не переключает fusion и не измеряет его качество. [Точный consumer][consumer].

## Единственное различие A/B

A — неизменённый `12.SemanticCompiler/Translation/PatternClassifier.cs` на core `d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19`, Git blob SHA1 `4d7a4c6ba2c9bb84bc915adebf7bd818135ebeba`.

B — копия того же файла. После `if (_train.Count == 0) return PredictByRules(posSequence);` один раз построить `HashSet<(int,int)> queryBigrams` тем же последовательным циклом `i=0..Length-2`. В сортировке передавать этот набор вместо исходного `posSequence`. У private `SequenceDistance` первый аргумент — подготовленный query set; удалить только повторное создание/заполнение bigramsA. Создание bigramsB, intersection/union, `union==0 ? 0`, порядок LINQ, стабильные ties, k3 и vote неизменны. Не кешировать train sets, не добавлять persistent cache, не изменять Train или fallback. Набор query локален одному Predict и не изменяется внутри comparator.

Следствие по исходнику: после Train на 32 запросах число query-set построений 4096→32; число training-set построений 4096→4096. Это контроль реализации, не измеренная экономия памяти и не 32768 искусственных повторов ради timing. Самую первую холодную ветку без `_train` B оставляет без query set.

Адрес будущего переноса, только после решения Любы: тот же `PatternClassifier.cs::Predict/SequenceDistance`. Предположение ownership: в течение синхронного Predict caller не меняет `posSequence` и TRAIN arrays; fixture неизменяема. Расширение на concurrent mutation не проверяется и не обещается.

## Модель, данные, before/after и бюджет

Данные — **native** `PatternClassifier.GenerateExamples(seed:42, perPattern:20)` из того же pinned файла. Получить ровно 160items; первые 128TRAIN, последние 32EXAM. Выполнить генератор один раз в общем prepare, сохранить все ordered POS arrays/labels в `H/in/fixture.json`; A/B читают одни bytes. Сохранить тип/версию .NET и hash fixture. Никаких внешних POS taggers, corpus, перебора seeds либо изменения split. Повторы шаблонов сохраняются и ограничивают вывод синтетической задачей; это не family-disjoint natural-language экзамен.

В каждом отдельном процессе A и B:

1. Создать пустой classifier(k3). Снять 32 Predict для EXAM — A0/B0. Это native rule fallback до обучения, а не обученное состояние.
2. Ровно один `Train(TRAIN128)`. Сохранить ordered TRAIN hash, число строк/labels и время/allocated/CPU этого вызова отдельно. Train действительно заполняет память; никаких LoRA/optimizer updates нет.
3. Ровно 32 Predict на тех же EXAM — A1/B1. Сохранить каждый ответ, label, правильность и стоимость всей 32-строчной панели. Hard subset считать по **уже полученным** ответам; полный `E25.Run` запрещён, потому что он повторно вызывает Predict на hardCases.

Итого: **1 GenerateExamples в общей подготовке, 2 Train, 256 предъявлений строк (128 уникальных общих), 128 Predict, 0 нейронных updates, 0 нейронного Generate, 0 warmup, 0 retry, 0 дополнительных hard-panel вызовов**. Исторический E25 показал 11/32→28/32 на этом seed/tier; это основание достаточности набора, не гарантированный будущий результат. Ненулевой полный class output непосредственно измеряется. Если both before/after correct=0 либо фактический Train не меняет состояние — `LEARNING_COMPARISON_INVALID`, без добавления шагов/примеров. Исходные 0/32 не заменять margin; исторические числа в новые raw не переносить.

A0/B0 и A1/B1 должны совпасть по каждому ответу и native distance/vote поведению. State хранится в памяти; persistence не исследуется. Не вызывать дополнительный classifier ради проверки parity: исходная source-copy и raw двух назначенных процессов достаточны для данного бюджета.

## Точные будущие host, файлы и команды

H=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01`; R=`H/results/attempt-0001`. Пути намеренно короткие; заранее проверить фактическую длину каждого создаваемого пути <240 символов. Если H уже занят иной работой или имеется `execution.claim`/R — `EXISTS_OR_UNKNOWN`, ничего не удалять, не менять suffix, не искать другое место.

После runtime-поручения до внешнего entry создать только `H/run.py`, `H/host/Program.cs`, `H/A/Runner.csproj` и `H/B/Runner.csproj`. Native copies и fixture заранее не создавать: их единственный владелец — wrapper после exclusive root claim. Он создаёт `H/A/Native/PatternClassifier.cs` как неизменённую копию, `H/B/Native/PatternClassifier.cs` с единственным указанным B-patch; затем режим prepare создаёт `H/in/fixture.json` ровно один раз. Compile обоих проектов — ровно их own Native/PatternClassifier.cs и общий `../host/Program.cs`. AssemblyName=`PosQueryRunner`, OutputType=Exe, TargetFramework=net9.0, ImplicitUsings/Nullable=enable; default Compile items отключены. ImportDirectoryBuildProps/Targets=false задаются **до** явных `Sdk.props`/`Sdk.targets` imports. Ни ProjectReference на продукт, ни NuGet packages, ни ссылка на весь 35.ExperimentsV2 не нужны. Зависимости — только .NET9 BCL.

Program имеет `prepare-fixture --out <H/in/fixture.json>` и `run --arm A|B --fixture <...> --out <R/A|R/B> --owner <nonce>`. Первый режим выполняет ровно один native GenerateExamples и сериализует fixture; у него 0 Train и 0 Predict, он не создаёт arm claim. Его защищает уже созданный root claim. Runner не принимает произвольные k/seed/budget/данные. Только режим `run` атомарно создаёт `training.claim` своей руки **до** classifier construction и всех 32 before Predict, Train и 32 after Predict; сравнивается owner nonce общего claim. Ответы до обучения тоже входят в issued budget.

Внешний entry — будущий файл, существование/сборка сейчас не утверждаются:

```powershell
& 'C:/Python314/python.exe' 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/run.py'
```

Wrapper — один владелец: exclusive-create `H/execution.claim` → read-only чтение pinned source и Git-blob SHA check → CreateNew A-copy и B-copy с единственным patch → один buildA, один buildB → один fixture generation режимомA без arm claim → runA со своим arm claim → runB со своим arm claim → compare/report. После B-patch исходник A поверх B не копировать. SHA drift или отсутствующий исходник → `SOURCE_DRIFT`, не выбирать другой commit. Не нужен новый approval каждого hash. Полные build-команды:

```powershell
dotnet build 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/A/Runner.csproj' -c Release --nologo
dotnet build 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PQ01/B/Runner.csproj' -c Release --nologo
```

Запускать соответствующую `H/A|B/bin/Release/net9.0/PosQueryRunner.dll` через `dotnet`, с указанными режимами и абсолютными H/R. Сборка каждой руки≤120s, prepare≤30s, каждая измерительная рука≤60s, сводка≤30s; повторов нет. На build error сохранитьstdout/stderr/exit; не чинить production или product tests. Для output использовать `CreateNew`, не overwrite. Wrapper создаётRодин раз, children — только своиR/A,R/B; двойное reserve одним путём запрещено.

## Ожидание, метрики и отчёт

Сначала обнаружить реально доступные launch/wait/status и прочитать их схемы. Зафиксировать tool_wait_binding: provider, handle, ongoing/terminal markers, смысл yield/RPC timeout и первоначальный experiment/phase deadline. После выдачи команды ждать тот же handle настоящим bounded wait, не более 60 секунд на один вызов, сообщая прогресс не реже раза в минуту. Два быстрых неблокирующих snapshot без новой информации требуют перейти в настоящий wait. Нормальный silent/running wait продолжать до terminal state или исходного deadline; правила «только два ожидания» нет. Независимая диагностика назначенных файлов и своего PID/session нужна при неисправном wait или противоречии каналов; затем для живого процесса вернуться к wait. Отсутствие stdout не доказывает отказ. Ни диагностика, ни tool yield не продлевают бюджет и не разрешают второй launch. RPC/yield timeout не равен experiment/phase deadline. Только по истечении назначенного experiment/phase deadline сохранить PID/session/start/elapsed/exit/terminal state и завершить принадлежащий wrapper процесс, если он ещё жив. Неизвестное завершение остаётся UNKNOWN, не 0 updates. [Контракт discovery/wait](https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md); схему конкретного Kiro/Codex provider проверить в своей среде, не копировать отсутствующий tool по имени.

Снимать allocations (`GC.GetAllocatedBytesForCurrentThread`) вокруг Train и отдельной 32Predict панели, wall monotonic clock, CPU process delta, GC0/1/2 и process peak. Не делать `GC.Collect` или дополнительный прогрев. Сериализация/compare вне измеряемой панели; использовать массивы результатов, не filewrite внутри каждой distance. Общая instrumentation одинакова вA/B; нельзя добавить query-set расходы вA ради удобства refactor. Данные динамическогосчётчика не заменяют native calls.

В отчёте показать A0/A1/B0/B1correct/32, все изменённые ответы, ΔA/ΔB и difference-in-differences; ожидаемая разница качества 0 не является провалом cost-фактора. Снижение allocations trained-readout — полезная метрика; Train costs, output equality, время/CPU/peak и ухудшения рядом. Одна fixed-order A→B пара не доказывает устойчивого ускорения: `observed_metric_success` и `causal_attribution` раздельны. Не выдавать стоимость Predict за цену обучения или общего перевода.

ВR: `report.md`, `candidate.md` (только при measured gain), `arms.json`, `per-row.json`, `costs.json`, `verdict.json`, `source-manifest.json`, `fixture.json`/hash, `one-factor.diff`, build logs/exit receipts, process receipts и фактический бюджет. Только экспериментальныйH; защищённые docs не трогать. Исторические эксперименты/raw/bin/objне удалять и не переписывать. Пользователь решает дальнейший перенос; полного CI/solution build нет.

[Полный промпт исполнителю](../prompts/POS-QUERY-SET-ONCE-01.md). [Независимая source/design проверка](../reviews/independent-review.md). Число новых запусков в текущем аудите —0.

[native]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/12.SemanticCompiler/Translation/PatternClassifier.cs#L34-L54
[distance]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/12.SemanticCompiler/Translation/PatternClassifier.cs#L78-L86
[consumer]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L213-L245
