# N60-E1: точные уроки, которые достигают обучаемого правила Verifier

> **Атрибуция финальной дельты:** та же недостижимость быстро/тихо независимо описана как LF-04 в [соседнем пакете](https://github.com/lubochka/xiigen-planning/blob/3b625a83ee878906d42544516741849a16c04689/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/01-new-findings-and-small-experiments.md). Здесь материализация альтернативных допустимых входов и bounded source-copy подготовки; новая общая гипотеза не заявляется. Не требуется запускать оба варианта.

## Статус и цель

Это добавление к уже существующему N60-E1, **не новая гипотеза**. Оно закрывает конкретный пробел материала protected-карточки, не меняя защищённую папку. Домен — перевод RU→EN. Изменяемый механизм — допуск ошибочного отрицательного примера при создании урока для `QualityVerifierUnit`. Модель-исполнитель и обучающийся получатель различны: получатель — собственная дискретная таблица правил `QualityVerifierUnit`, нейронные параметры отсутствуют.

**Любое фактически измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен. Остальные изменения, включая ухудшения, должны быть показаны рядом.** Исправленный набор ещё не исполнялся. Достижимость его ветви установлена чтением кода; ожидаемые результаты ниже не являются измерениями.

Точный источник карточки: [N60-E1, строки 213–243](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L213-L243). Известное смежное условие N57 — повреждённые manner endings; оно не исправляется одновременно с admission.

## Почему прежние «быстро/тихо» не подходят

1. `TranslationSemanticTokenizer.ClassifyPos` не присваивает им ADV: их нет в `RuAdverbAllowList`; «быстро» заканчивается на `ро`, «тихо» на `хо`, тогда как текущая ветвь русских наречий перечисляет другие окончания.
2. В этих фразах есть subject/past-verb, поэтому `RuleTags.Count` ненулевой.
3. `QualityVerifierUnit.ExtractSourceMarker` сначала ищет ADV. При его отсутствии и непустых RuleTags запасная проверка требует `LooksLikeMannerMarker`, содержащий mojibake-окончания и `ly`. Обычные UTF-8 слова из прежнего опыта не подходят.
4. Generated issue содержит `translated quality marker`; writer намеренно не добавляет его в signature-list. При пустом marker не меняется intended таблица `_requiredTermsBySourceMarker`.

Это объясняет, почему просто вызвать Train недостаточно: нужно доставить различающий урок в исследуемую запись. Не следует чинить tokenizer вместе с admission — это разрушило бы однофакторность сравнения.

Источники: [tokenizer 89–97, 265–292](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationSemanticTokenizer.cs#L89-L97), [suffixes 574–585](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationSemanticTokenizer.cs#L574-L585), [TrainOnExample 1119–1144](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationTrainedUnits.cs#L1119-L1144), [ExtractSourceMarker 1265–1299](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1299).

## Одно изменение и настоящий учебный путь

Путь: `TranslationStructuredBatchBuilder.BuildGeneralBatch` → `TranslationLearningExample` с `UnitId=quality-verifier` → `TranslationMachine.TrainStructuredBatchAsync` → `QualityVerifierUnit.TrainOnExample` → `_requiredTermsBySourceMarker` → `QualityVerifierUnit.Verify`.

Оригинальный адрес будущего исправления: `27.TranslationDomain/TranslationStructuredBatchBuilder.cs`, `BuildGeneralBatch`, блок создания `:verifier` на строках 128–141. Кандидат исправления: **не добавлять fail-урок Verifier, когда вычисленный rejectedCandidate точно совпал с chosen**; отдельно записать причину `NO_DISTINCT_NEGATIVE`. Остальные semantic/executor-строки не менять. Реализуется сначала только в экспериментальной копии. Результаты и diff передаются пользователю; даже положительный опыт не разрешает автоматически переносить изменение в основной код.

Доказательство исходной ошибки: [BuildGeneralBatch 80–141](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L80-L141) создаёт verifier fail; [BuildRejectedCandidate 269–288](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L269-L288) при `[WRONG:tense]` без explicit negative возвращает chosen.

## Замороженные TRAIN и EXAM

`N60-E1-TRAIN.jsonl` содержит точные поля `TranslationTrainReadyPair` (id/source/direction/pair_family/prompt/a_side/chosen/rejected/validation_outcome/validation_errors). Значения — исследовательские simulation-данные, не вымышленная генерация внешнего teacher.

| ID | Source | Chosen | Rejected |
|---|---|---|---|
| V1 | `Он спокойно открыл дверь.` | `He calmly opened the door.` | `[WRONG:tense]` |
| V2 | `Она громко закрыла окно.` | `She loudly closed the window.` | `She closed the window.` |

Общий `Direction=RU_EN`, `heldOut=false`, `signatureMode=Structural`; Source-поля различны. `prompt` совпадает с `a_side`, `pair_family=manner-admission`, `validation_outcome=train_ready`, `validation_errors=[]`. В обоих плечах tokenizer и EnglishAdverbRegex одинаковы. «Спокойно» и «громко» распознаются существующими ADV-окончаниями `но` и `ко`; required target words — `calmly`, `loudly`. Никакие mappings из EXAM не добавляются.

| EXAM ID | Source | CandidateText | Gold для собственного Verify |
|---|---|---|---|
| Q1 | `Он спокойно закрыл окно.` | `He closed the window in a composed manner.` | Pass: законная парафраза «спокойно» |
| Q2 | `Он спокойно закрыл окно.` | `He closed the window.` | Fail: пропущен способ действия |
| Q3 | `Она громко открыла дверь.` | `She loudly opened the door.` | Pass |
| Q4 | `Она громко открыла дверь.` | `She opened the door.` | Fail |

Это четыре наблюдения двух marker-семейств на новых рекомбинациях, не четыре независимых языковых закона. CandidateText задан как вход именно проверяющему; его не называют собственным переводом Executor. Gold используется только после Verify; не попадает в learned state.

## Состояния и бюджет

A0/B0 — два независимых пустых состояния всех участвующих Units; одинаковый tokenizer и отсутствие learned rules. A получает прежние 6 projected rows. B получает 5: удалена только V1:verifier, остальные соответствующие строки равны A. Физическое различие количества принятой учебной работы — следствие admission-фактора; его цена измеряется, а не скрывается принудительным выравниванием.

| Вызов | A | B | Всего |
|---|---:|---:|---:|
| `BuildGeneralBatch` | 1 | 1 | 2 |
| `TrainStructuredBatchAsync` | 1 | 1 | 2 |
| Внутренние `Unit.TrainOnExample` | 6 | 5 | 11 |
| Внутри предыдущей строки: `QualityVerifierUnit.TrainOnExample` | 2 | 1 | 3 |
| Verify до обучения | 4 | 4 | 8 |
| Verify после обучения | 4 | 4 | 8 |
| Всего Verify | 8 | 8 | 16 |
| Ordinary Translate/Flow, LoRA, neural optimizer | 0 | 0 | 0 |

11 включает 3 verifier calls: их нельзя складывать как 14 уроков. Источник обучения состоит из двух пар на плечо. Промежуточные snapshot и source-marker trace получают из тех же вызовов; не вводят новую панель, warmup или дополнительные уроки. Данная карточка выбирает только двухbatch-путь извлечённого native метода с настоящими Units. Direct Unit-only host в этот паспорт не входит и самостоятельно вместо него не выбирается.

## Ожидаемое различие — не результат запуска

По исходнику A обучит требование `спокойно → calmly`; B его не запишет. Оба обучат `громко → loudly`. Возможное наблюдение: B перестанет отвергать Q1, но начнёт пропускать omission Q2; Q3/Q4 останутся одинаковыми. Это одновременно полезное снижение false reject и ухудшение false accept, даже если общий correct count не вырастет.

Отсутствие различия после действительно достигнутого writer/reader — валидный нулевой результат. Пустой marker и отсутствие записи intended state означают, что исходная гипотеза этим запуском не проверена. Не подменять native trace вычисленной в документе таблицей ожиданий.

Метрики: Q1–Q4 Pass/Issues/Score; false reject и false accept отдельно; фактически записанные required terms; число допущенных fail-уроков; bytes learned state; wall/CPU подготовки и обучения; managed allocation и process memory с правильными единицами и областью. Слои neural: N/A; уровни: source pair → structured Unit lesson → learned marker table → Verify. Полный перевод не измеряется. Возвращаемый `LearningBatchResult.AccuracyAfter` при `heldOut=false` вычисляется через `MeasureUnitCoverage` по затронутым UnitId; это доля типов Unit, а не точность на Q1–Q4. `RowsTrained` считает projected rows6/5, а не исходные пары2/2. Собственная предметная метрика берётся только из16 реальных Verify и заранее заданного gold.

## Размещение и команды будущего выбранного опыта

Окончательные фиксированные адреса для этого варианта:

- Экспериментальная копия механизма: `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission/`.
- Малый host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/Runner.csproj` и `Program.cs`.
- Материал: файлы `N60-E1-TRAIN.jsonl`, `N60-E1-EXAM.json` рядом с этим документом; их bytes не меняются между A/B.
- Результат этой программы: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/results/N60-E1-VERIFIER-ADMISSION-01/`.

**Техническая граница готовности:** данный overlay закрывает ошибку входного материала и точно назначает размещение; полного two-batch native host в нём нет. Точный объём переноса задан в [N60-E1-native-host-preparation.md](N60-E1-native-host-preparation.md):11 source-файлов, literal extraction blocks, одинаковые namespaces A/B, typed DTO, csproj/Program contract и [N60-E1-prepare-sources.ps1](N60-E1-prepare-sources.ps1). Переносится нативный batch method с настоящими Units; полный ordinary Flow не включается. Это отдельная ограниченная подготовка, разрешённая поручением на реализацию опыта, не запрос к пользователю о выборе пути. До её завершения запрещено выдавать приведённые ниже будущие команды за существующую готовую CLI или фактическое измерение. Выбор другого consumer, числа вызовов или одного unit-only host вместо указанного batch-пути требует новой версии дизайна, не молчаливой замены.

Source manifest закреплён на62bcc8f; late7c5718 меняет только документы, исходный механизм тот же. RNG/seed=N/A: deterministic Unit-memory, никаких neural weights/optimizer. После материализации **именно этого host** контракт команд фиксирован:

```powershell
$e1Root = 'C:/Projects/llm_mvp_core'
$e1Project = "$e1Root/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/Runner.csproj"
$e1Dll = "$e1Root/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/bin/Release/net9.0/N60E1VerifierAdmission.dll"
$e1Out = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/results/N60-E1-VERIFIER-ADMISSION-01'
dotnet build $e1Project -c Release -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet $e1Dll --arm A --output "$e1Out/A"
dotnet $e1Dll --arm B --output "$e1Out/B"
```

Runner должен принимать ровно эти аргументы, читать замороженные inputs, создавать только новые output directories и завершать выбранную панель. Это заданный **будущий интерфейс**, а не обнаруженный существующий. Если каталог результатов уже существует, сохранить его и остановить этот фиксированный запуск; не перезаписывать чужой опыт и не выбирать новый ID автоматически. .NET source, product tests, общий csproj/dispatcher и защищённый `docs/plans/replanning-possible-diffs` не меняются.

## Полный промпт исполнителю

> Подготовь и выполни только уточнённый N60-E1, если тебе поручена реализация и выполнение этого опыта. Сохрани exact bytes/hash данного overlay, TRAIN/EXAM, перечисленных исходных методов и фактического host. Используй назначенные здесь пути. Сначала материализуй source-faithful two-batch host; существующей готовой CLI этот документ не обещает. Основной код и защищённый каталог не редактируй. Оригинальный будущий target — BuildGeneralBatch, блок verifier 128–141; только admission rejectedCandidate==chosen различается между A/B. Tokenizer и source-marker helper одинаковы и не исправляются.
>
> Используй ровно «спокойно/calmly» и «громко/loudly», две пары и Q1–Q4; gold не учи. Оба плеча действительно вызывают TrainStructuredBatchAsync. Сохрани A0/A1/B0/B1, два batch calls, 11 внутренних Unit calls, из них 3 verifier calls, и 16 Verify. Покажи actual marker/state и собственные решения Verify; не называй их свободным переводом. Пустой intended marker — недостигнутый механизм, а честный нулевой результат достигнутого обучения допустим.
>
> Любое измеренное полезное улучшение — успех по этому показателю и RU→EN; рост точности не обязателен. Рядом покажи false rejects, false accepts, цену обучения и хранения, а неизмеренное обозначь явно. Не добавляй warmup, новые уроки, повторения, другой домен, LoRA, product tests или CI. После результата дай пользователю raw outputs, объяснение и точный diff кандидата переноса. Перенос в основной код выбирает пользователь; успешный эксперимент сам его не разрешает. Это уточнение существующей N60-E1, не новая гипотеза.
