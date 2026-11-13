# Промпт исполнителю: DPO-TOKENIZER-SNAPSHOT-01

**DESIGNED_NOT_RUN / READ_ONLY_RESEARCH.** Сейчас создаются документы, runner ещё требует реализации. Одно чтение prompt не разрешает build или training. Ниже конечное задание для будущего отдельно назначенного выполнения именно этого опыта; не запускай существующий main500/calibration вместо него.

Модель исполнителя и последовательного reviewer — **модель текущей сессии по умолчанию**, без поиска другой модели и самовольного изменения режима. Student отдельно: исходный `rp01-csharp-small/v1`, сохранённая B0, q/v-LoRA rank8/alpha16/dropout0 и исходный AdamW. В обоих arms одинаковые независимые policy/reference copies и actual optimizer/RNG states; исполнитель не является этой Student.

Прочитай [карточку](../DPO-TOKENIZER-SNAPSHOT-01.md), [native source-copy design](../dpo-source-copy-design.md) и [2 literal JSONL строки](../fixtures/dpo-tokenizer-snapshot.jsonl). SHA fixture: `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`. Исходный commit `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`. Не ищи другой эксперимент, модель или B0 path.

Навыки из `C:/Projects/llm_mvp_core/docs/skills/`: architect-behavior-classifier-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, prompt-patch-authoring-SKILL.md, human-readable-architecture-audit-SKILL.md, human-communication-GLOSSARY.md. Уже прочитанное неизменившееся не перечитывай ради отчёта. Старые production CI/подписи каждого SHA не добавляй к этому ограниченному опыту; экспериментальное разрешение не разрешает prod/submitting.

Рабочий пакет **C:/xg/ts01**, host **C:/xg/ts01/Runner.csproj**. Складывай полные копии DPO14 C#, CapabilityTraining13 C#, ContinuousLearning81 C# по manifest —108 файлов. Native library identities/friends сохраняются; bridge внутри ContinuousLearning. Binary boundary ровно `C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/bin/Release/net9.0/` и список design. Исходные три заменяемые DLL не должны проникать через transitive references. При несовместимости фиксируй конкретную ошибку подготовки; никаких reflection/AssemblyResolve обходов, full product rebuild или fallback на другой bin.

Один treatment: A делает tokenizer Save→Load→hash для каждой пары; B reuse одного **private lazy frozen snapshot на один PrepareExecution** после admission и CheckText первой допустимой exact pair. При активном internal tokenization observer исходный per-pair путь и сброс invocation snapshot. В эксперименте tokenizer owner-exclusive и неизменяемый. Не снимай freeze/validation и не заявляй equivalence для чужой concurrent mutation. Только BpeSnapshot helper в копии DpoExactPairMaterial.cs и ref-carrier через копию LlmIngredientLogicalUnits.cs. Не трогай legacy objective, data/vector/masks, LoRA, reference cache или optimizer exports.

B0 загружай только ReadConfiguration/LoadModels из Program.cs:307–332. Конфигурация `config/code-documentation/generation-v1.json`; config hash `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; B0 weights `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`; manifest `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`. Step1/no-parent/InitialBaselineNoUpdate/55 tensors39base16adapter checks сохраняются. Physical path только из verified ParentManifest. Нет B0 — ошибка подготовки, без random fallback, C50 или уменьшения модели.

Всего **две StepVector**: один на A, один на B; в каждом одновременно обе строки. Для arm ровно одно `Prepare(preferencesOfTwo, tokenizer)` и одно `StepVector(allTwo,1)`. Реальные Validate/Project → authority → lease → mutation capability → CreateForProjectLora → transactional publication и native readback из design сохраняются. Не зови большой CodeDocumentationSimulationDpoExperiment.Run. Student Generate/Evaluate/teacher/scoring/warmup/calibration/retries — ноль.

Primary metric — фактические allocations/CPU/elapsed одного Prepare, чей результат реально потреблён завершённым optimizer update. Отдельно полная стоимость update/load/publication, GC/peak. До/после A0/A1/B0/B1: точные material digests/IDs/masks, optimizer0→1, actual LoRA delta (включая возможный0), unchanged base/reference, RNG и saved/live readback. Loss/margin, возвращённые StepVector, подпиши pre-update; не выдавай их за качество A1/B1. Отказ до optimizer — диагностическая подготовка, не улучшение обучения. Число исключённых capture — объяснение, не самостоятельный useful metric.

**Любое измеренное улучшение полезного показателя — успех по этой метрике и теме C# documentation; рост accuracy не обязателен.** Все ухудшения покажи рядом. Не объявляй выигрыш для EN→RU, legacy либо single-pair подготовки, которые не проверялись. Не добавляй генерации и повторы ради уверенного speedup.

После будущей реализации команды ровно:

```powershell
$ErrorActionPreference = 'Stop'
dotnet build C:/xg/ts01/Runner.csproj -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/xg/ts01/Runner.csproj -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/xg/ts01/bin/A/Release/net9.0/TokenizerSnapshotRunner.dll run-pair --peer C:/xg/ts01/bin/B/Release/net9.0/TokenizerSnapshotRunner.dll --fixture C:/xg/ts01/in/train.jsonl --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

Это **спецификация ещё не существующего CLI**, не готовый проверенный запуск. Результаты **C:/xg/ts01/r/0001**, A/B отдельно, intent/issued/completed create-new. Существующий attempt не удаляй, не переписывай и не обходи новым RunId; uncertain effect оставь UNKNOWN до анализа имеющихся записей, без нового learner call. Новая attempt identity — только отдельное прямое поручение.

Отчёт на **русском Markdown**, **C:/xg/ts01/r/0001/report.md**. До350 слов пояснений плюс raw таблицы; «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». Укажи A0/A1/B0/B1, A/B/B−A и единицы, actual issued/completed, «не измерено» вместо0. При useful gain — **candidate.md** рядом, до120 слов с source method и raw; только Люба решает перенос. Не меняй protected `docs/plans/replanning-possible-diffs`, оригинальную production logic, tests, full CI, commit/push или promotion.

**STOP/NEXT обязателен.** При прямом STOP немедленно останови действия, не делай следующий tool call, запись памяти или «завершение» прошлого report. Допустимо только необходимое прекращение уже работающего собственного процесса, чтобы он не продолжал обучение; это не cleanup/readback или новый запуск. При NEXT прекрати предыдущий эксперимент и перейди только к назначенному следующему; не продолжай старые отчёты, hashes, тесты, candidate registration и повторные запуски. Если следующий ID не назван, сообщи известный статус и спроси ID, не возвращаясь к старому. При обычном завершении один readback для фактической ошибки/битой ссылки; дополнительные циклы оформления запрещены.

