# Точное задание будущей подготовки двух wiki micro-host

**Текущий статус: DOC_DESIGN_ONLY / NOT_COMPILED / NOT_RUN.** Этот документ задаёт подготовку; исходники новых host здесь не созданы. Текущее поручение — документировать, поэтому команды ниже в этой сессии не выполняются. После отдельного поручения на конкретный опыт исполнитель получает готовые пути, источник, единственный фактор и бюджет; искать другой runner или модель не требуется.

## 1. Исполнитель, язык, навыки и приоритет указаний

Исполнитель использует **ту же модель, которая установлена по умолчанию в его текущей сессии**, без выбора нового провайдера, иной модели или скрытого увеличения thinking budget. Обучаемая модель опыта отдельно фиксирована: native `ProjectWikiRequestRoutingTrainableModel`, 15 классов, CPU, 18 эпох, learning rate 0.14. Neural layers/LoRA/VRAM — `N/A`. Сетевые вызовы Teacher/LLM и генерация новых TRAIN/EXAM в бюджет не входят и не требуются.

Пояснения и `report.ru.md` пишутся по-русски. Имена свойств JSON, route enums, path и дословные английские fixture prompts сохраняются как есть. Использовать только относящиеся к текущему этапу правила из core `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-readable-architecture-audit-SKILL.md`. Исходники навыков зафиксированы [в core docs/skills][skills]. Требование полной production-цепочки не переносить на эксперимент. Современное явное указание пользователя имеет приоритет над архивной карточкой, модельным preset и историческим протоколом.

**Обязательный запрет продолжения предыдущей задачи:** команда пользователя «следующий эксперимент» немедленно завершает работу над прежним опытом: без дополнительного исправления отчёта, commit, hash, build, чистки или «последней проверки» старой задачи. Команда «стоп/закончить немедленно» прекращает действия; нельзя запускать ещё один tool call ради красивого завершения. Единственное допустимое действие после STOP — необходимое прекращение собственного уже работающего процесса этого опыта; не останавливать чужие процессы. Сообщить кратко только уже известный статус. Старые файлы и упавшие attempts не удалять и не переписывать. Не создавать новый запуск автоматически. Решение о production-правке принимает пользователь после отчёта.

Любое фактически измеренное улучшение полезного показателя — успех по этому показателю и домену; роста accuracy не требуется. Все изменения остальных показателей, включая ухудшения, показываются рядом. Уменьшение diagnostic counter само по себе не является полезным эффектом. Если выполнено только чтение исходника, статус остаётся `NOT_RUN`, а численные результаты — «не измерено».

## 2. Источник для копии уже выбран

Planning commit: `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`.

Базовый каталог уже существующего scaffold:

`llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/`

Взять по pinned commit ровно четыре файла: [BaselineModel.cs][baseline], [NativeRoutingContracts.cs][contracts], [Program.cs][program], [source-provenance.json][provenance]. **CandidateModel.cs из WIKI-MARGIN-01 не брать:** это другой экспериментальный фактор. Нельзя «для удобства» брать B от taxonomy.

Provenance донора указывает старый core commit `62bcc8…`, но исходные blob identities равны текущему pin `ab583032…`:

| Исходный core-файл | Git blob |
|---|---|
| `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs` | `3672bd2c9dd813eb2a31835c2e6b06071de37869` |
| `42.ProjectWikiDomain/ProjectWikiDomainModels.cs` | `9044c096665f29442de3dd01f7d7aa6bc1291d39` |

Все 15 файлов string constants из donor `constant_inlining` также имеют те же blob SHA в current core tree. Это подтверждает происхождение и равенство исходного кода, не является результатом компиляции. Сохраняются license headers. Допустимые общие edits донора уже перечислены в provenance: изолированный namespace, отсутствие ненужных AgentInfrastructure usings и соседнего ILogicalUnit wrapper, public-доступ к тому же `RouteForEvaluation`, одинаковые counters presentations/updates, inline точных compile-time Unit/Flow IDs. Не менять модельную арифметику A.

## 3. Буквальные короткие каталоги

| Опыт | Host | Fixture назначения | Новые результаты |
|---|---|---|---|
| WIKI-FEATURES-ONCE-01 | `C:/xg/wc1` | `C:/xg/wc1/fixture.json` из [WIKI-FEATURES-ONCE-01.json](fixtures/WIKI-FEATURES-ONCE-01.json) | `C:/xg/wc1-r/attempt-001/A`, `…/B`, общий `report.ru.md` в `attempt-001` |
| WIKI-FEATURE-KEYS-01 | `C:/xg/wk1` | `C:/xg/wk1/fixture.json` из [WIKI-FEATURE-KEYS-01.json](fixtures/WIKI-FEATURE-KEYS-01.json) | `C:/xg/wk1-r/attempt-001/A`, `…/B`, общий `report.ru.md` в `attempt-001` |

Результаты не писать в защищённую папку и внутрь production project. До подготовки проверить отсутствие чужого/неустановленного host; такой конфликт — `BLOCKED_PATH_EXISTS`, ничего не удалять. Собственный уже подготовленный host с подтверждённым provenance ожидаемо существует перед build/run и сам по себе не является конфликтом. **Любой существующий parent attempt или arm output блокирует повтор независимо от полноты файлов.** До SDK/restore/build карточки резервируют новый parent attempt с create-new intent и сохраняют stdout/stderr. Arm каталоги shell заранее не создаёт: их создаёт host после guards и до первого Train. После явного разрешения повторить опыт используется следующий численный `attempt-002`, затем `attempt-003`; прежний attempt не возобновлять. Наличие `build.stdout`/`status.json` после неудачи — сохранённый след, а не повод удалить папку. Не расширять Windows path limits системными правками.

## 4. Минимальный состав каждого host

В выбранном коротком каталоге должны быть `BaselineModel.cs`, `CandidateModel.cs`, `NativeRoutingContracts.cs`, `Program.cs`, `fixture.json`, `source-provenance.json`, `WikiProbe.csproj`, `NuGet.Config`. Это **спецификация будущей записи**, а не наличие файлов сейчас.

`CandidateModel.cs` создаётся копией `BaselineModel.cs`; namespace меняется с `PossibleDiffs.Baseline` на `PossibleDiffs.Candidate`, затем применяется только фактор выбранной карточки. Общее имя класса сохраняется, Program использует существующие alias AModel/BModel. A и B находятся в одной сборке, запускаются разными свежими процессами. Состояние из A никогда не передаётся B.

Минимальный проект повторяет standalone .NET9 подход [существующего csproj][csproj], но все source paths становятся локальными; так компиляция не тянет длинное дерево или production solution:

```xml
<Project>
  <PropertyGroup>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
  </PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>WikiProbe</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="NativeRoutingContracts.cs" />
    <Compile Include="BaselineModel.cs" />
    <Compile Include="CandidateModel.cs" />
    <Compile Include="Program.cs" />
    <None Include="fixture.json" CopyToOutputDirectory="PreserveNewest" />
    <None Include="source-provenance.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
```

`NuGet.Config` содержит только `<?xml version="1.0" encoding="utf-8"?><configuration><packageSources><clear /></packageSources></configuration>`. Нужны установленный .NET9 SDK и его reference packs. Их отсутствие даёт `BLOCKED_ENVIRONMENT`; не выбирать незаданный runtime и не скачивать новый model/framework автоматически.

## 5. Точные изменения общего host Program

Скопированный Program уже имеет разделённые fresh A/B, `Train([])` для начального состояния, одну настоящую `Train(rows)`, before/after native reader, отсутствие gold в reader, state hashes, raw per-row outcomes и resource counters. В новой копии изменить только:

1. Literal `ExperimentId` на ID выбранной карточки и проверку размеров fixture: `8/4` для FEATURES-ONCE или `4/6` для FEATURE-KEYS. Порядок строк и gold не менять.
2. Поле `coreCommit` на `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; `scope` — согласно карточке; убрать чужие упоминания margin experiment.
3. В `source-provenance.json` сохранить original source/blob/constant mapping; записать current pin, `ancestor_scaffold` pinned URL, новый `experiment`, точный `candidate_factor`, статус фактического приготовления. Старый provenance в доноре не редактируется.
4. Не менять body `Evaluate`, который различает `Selected`, правильный route/target, wrong-selected и deferred. Не объявлять невыбранный `NotProjectWiki` правильным только из-за совпадения enum. `RouteForEvaluation` пропускает только publication gate; trained signal, margin и selection-confidence gates остаются native.
5. Четыре raw `Train` вызова на весь A/B опыт: по одному пустому и одному непустому на arm. Пустой вызов создаёт нулевое состояние и исключён из измеренного nonempty Train cost. Не добавлять warmup с обучением или многократный benchmark без нового поручения.
6. **Исправить общий журнал host до любого Train.** Donor Program создаёт output только после Train/EXAM; для этих двух новых опытов этого недостаточно. После fixture guards, но до первого `Train([])`, создать новый arm directory и эксклюзивный `intent.json` через `FileMode.CreateNew`. **Перенести, а не продублировать** поздние записи frozen fixture/provenance из конца donor Main в эту раннюю фазу; удалить их прежний повтор из финального блока. Иначе завершающий File.Copy выбросит исключение после успешного обучения, а File.WriteAllBytes перепишет уже сохранённый fixture. Перед каждым из двух Train записать отдельный неизменяемый `phase-N-issued.json`, после возврата — `phase-N-completed.json` с raw counters. Перед непустой Train issued receipt пишется до запуска таймера, completed — после остановки таймера, чтобы file I/O не входил в cost. При исключении записать новый `failed.json` с exception/последней известной фазой/issued и completed counts, сохранить все имеющиеся артефакты и выйти nonzero, без retry. Неизвестное число предъявлений или эффект оборванного вызова обозначить `UNKNOWN`, не нулём. При hard kill issued без completed остаётся достаточным запретом на повтор. Ничего не перезаписывать, не изменять числители задним числом. Это одинаковое plumbing A/B, не дополнительный алгоритмический фактор.
7. `before-snapshot.json`, `after-snapshot.json`, `report.json` сохраняются в этом зарезервированном arm по достижении соответствующих фаз; существующие файлы не перезаписывать; если запись snapshot перенесена в раннюю соответствующую фазу, её старое позднее повторение также удалить. Метаданные snapshot missing-gate могут содержать `TrainingRowCount=0`; реальное количество хранить отдельно в `observedTrainingRows`, не «исправлять» gate ради красивого отчёта. Parent shell пишет отдельные `A-issued/A-returned`, `B-issued/B-returned`; arm-issued означает запуск процесса, а не доказательство дошедшего до Train. Последний подтверждается внутренними phase receipts.

Замер `GetAllocatedBytesForCurrentThread` применим к этому синхронному learner. Whole-process peak включает startup/JIT; `GetTotalMemory(false)` не равен точной пиковой живой памяти обучения. Эти ограничения переносятся в отчёт буквально по смыслу. Снимки и per-row native outcomes, а не учебная loss на surrogate, подтверждают фактическое изменение модели.

## 6. Последовательность будущей авторизованной работы

После подготовки выбранного host: один scoped restore, один scoped build, затем ровно A и B, если подготовка и build успешны. Команды для каждого опыта заданы в его карточке. Ошибка до обучения отмечается `BUILD_BLOCKED`/`NOT_RUN`, ошибка после старта — `FAILED_PARTIAL`; нет перехода к full solution build, ремонту production tests, повтору старого опыта или новой гипотезе без поручения. Сам факт build — не успех учебного эксперимента.

После A/B создать один `report.ru.md` с таблицей `A до / A после / B до / B после`, дельтами внутри каждого arm и разницей этих дельт. Для costs сравнивать nonempty Train A/B, не нулевую инициализацию с обучением. Указать source pin, fixtures hash, domain, фактические counts, все raw files, useful gains и regressions, `NO_GAIN`/`INCONCLUSIVE` где необходимо. Не удалять старые карточки. В случае выигрыша — только кандидат в исправления с точным original method/line; production promotion оставляется владельцу.

[skills]: https://github.com/lubochka/llm_mvp_core/tree/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/skills
[baseline]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/BaselineModel.cs
[contracts]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/NativeRoutingContracts.cs
[program]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/Program.cs
[provenance]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/WIKI-MARGIN-01/source-provenance.json
[csproj]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/wiki-margin/scaffold/experiments-v2/RP01-MICRO/WIKI-MARGIN-01/WikiMarginLearning.csproj
