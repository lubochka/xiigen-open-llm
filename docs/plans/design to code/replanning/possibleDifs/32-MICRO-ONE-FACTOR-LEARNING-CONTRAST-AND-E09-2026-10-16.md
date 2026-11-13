# MICRO-N32 — быстрые причинные проверки: RP01 multi-negative и E09 переходы AST через границы файлов

<!-- POST117-CURRENT-2026-11-03:START -->
## Текущий допуск после P117

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) не назначает новый M32-A или M32-B запуск. P116 подтверждает file-boundary механизм E43; native E09 и независимый multi-negative Student остаются другими получателями. До запуска любого шага ниже сначала проверить нынешнюю карточку/очередь, сохранённые попытки и разрешённый объём; дефект parser, отчёта или проверяющего исправлять по saved evidence.

Для M32-B данные обоих концов связываются с trusted project+file+position+actual content hash. Контроль с изменением только project, сохранёнными tokens/count/file labels должен быть отвергнут. Нельзя превращать совпавший source proxy в Roslyn PASS. Итог должен отказать при missing/false provenance или budget control. Все диагностические Train и неудачные попытки входят в общий расход; проектные дозы ниже не расширены и не исполнялись этим аудитом.
<!-- POST117-CURRENT-2026-11-03:END -->


**2026-10-16. Статус:** протокол native test + качественного A/B, **НЕ реализованное обучение**. Фактически выполнен только синтетический source-aligned proxy в Node 22: [исходник](32-MICRO-ADMISSION-E09-SOURCE-PROXY.mjs), [JSON receipt](32-MICRO-ADMISSION-E09-SOURCE-PROXY-RESULT.json). `dotnet` в среде текущего исследования недоступен; native C# = `NOT_RUN`; независимый CODEX/Astra judgement = `NOT_RUN`; actual improvement = `UNPROVEN`.

## На каком вопросе сфокусироваться первым

**Первым действием — сверка нынешнего задания, сохранённых результатов и actual caller.** Исторический приоритет16.10 предлагал RP01 admission, затем E09; он не является текущим разрешением запуска или перехода между ними. Только отдельно назначенный срез получает native fixtures/дозу; **не смешивать M32-A и M32-B как один A/B**, они работают с разными моделями и objectives.

## M32-A — RP01: сначала доказываем потерю выразимости контрастов, затем влияние на качество

### L0: source/proxy (выполнено ограниченно)

- В [`CodeDocumentationSimulationDpoExperiment.Inspect` L51–75](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L51-L75) есть `HashSet<string> prompts` и `!prompts.Add`. `id` и `triplet` отдельно различимы. [Отдельная гипотеза N32-A](32-NEW-RP01-SAME-PROMPT-CONTRAST-ADMISSION-2026-10-16.md).
- Node proxy с двумя разными `rejected` для одного `prompt` показывает отказ именно этого guard и допуск при различных prompt. **Это не вызов `Inspect`, не полный алгоритм validation.** Отдельный протокол подтверждения ниже.

### L1: 3 native fixtures в отдельном локальном test-only проекте, без model loading

Использовать **публичный actual method**
`CodeDocumentationSimulationDpoExperiment.Inspect(path, sha256OfFile, expectedCount:2, SimulationDpoDomain.CodeDocumentation)`. Создать три временных JSONL с **строго валидными UTF-8, schema, distinct id, provenance="simulation", domain="csharp", pair_kind="CODE_TO_DOC", chosen!=rejected**. Для каждого вычислить новый собственный SHA256 и передать его (никаких изменённых/поддельных checksum существующего RP01; не читать приватные teacher data).

| Fixture | Вход | Ожидается только из исходного guard |
|---|---|---|
| A | row1:P,chosen:X,rejected:Y1; row2:P,chosen:X,rejected:Y2; Y1≠Y2 | **Fail** по duplicate prompt, несмотря на уникальные id и triplet |
| B | те же, но row2 prompt=Q≠P | **Pass guard**; полное `Inspect` должно пройти при валидной остальной schema |
| C | row2 same id/triplet и same prompt | Fail — отрицательный контроль duplicate-пары |

Записать literal stdin JSONL SHA, source code SHA, expectedCount, returned code, duration и exception. **Не исправлять тест под фактический результат**. Если A не отвергается и B не допускается, сверить фактический import caller, контракт, roles и version; гипотеза об этом способе admission фальсифицирована.

### L2: только если L1 подтверждён — **действительная проверка качества обучения**

**H32-A:** два различающихся, независимо проверенных rejected **для одного source input** дают больший законно правильный перенос на **новых** задачах, чем повторение одного и того же rejected. Не меняем исходные 500, не пытаемся обходить permission: отдельная test-only authorised producer/composition, допускающая оба valid exact DPO material; external teacher и human judge нужны по реальному контракту.

Два новых изолированных Student/Reference/LoRA checkpoints из идентичной B0 и tokenizer SHA:

- **Control A:** один chosen ответ X, prompt P, два шага с одинаковой правильной предпочтительной парой `(P, X, Y1)`.
- **Treatment B:** тот же P и X, первый шаг `(P, X, Y1)`, второй шаг `(P, X, Y2)`, где Y2 — **иной реально ошибочный закон/причина**. Парное сравнение имеет одинаковые 2 обновления, beta, LR, LoRA rank/dropout, seed, optimizer state, context, maxNewTokens и BPE. Учесть длины Y1/Y2, измерять токены/градиенты; если длина сильно разнится, подобрать контролируемые по длине альтернативы **до** раскрытия eval.
- **No-learning baseline:** frozen B0 без двух updates. **Shuffled/irrelevant rejected** как отдельный negative control только если разрешён training-data admission и проверен law; нельзя маркировать искусственно неверную пару как legal chosen.

Учебная часть: 1–2 исходных P и две настоящие корректно допущенные ошибки. Экзамен: **8–12 новых source-family-disjoint** C# либо ENRU задач, минимум половина на исключение/альтернативный тип ошибки, одинаковый greedy generation, max token, cold reopen и independent law-specific oracle. Primary = `paired blind law-correct count` и `critical error count`; secondary = `chosen absolute logP`, DPO margin, actual updated adapter tensor SHA, cost, abstain, old-skill retention. Сначала проверять, что Student хоть способен изменить train preference и logits; иначе `INCONCLUSIVE_BASE_CAPACITY`, не выводить «multi-negative бесполезен». Не требовать статистической значимости от 8–12: это **пилотный причинный сигнал**.

**Ожидаемая интерпретация:** B>A на независимой панели без роста критических ошибок → направление на расширенное исследование; B=A → пока нет evidence качества, guard может оставаться осознанным; B<A → альтернативный negative может мешать, нужна semantic audit, не ослаблять gates.

## M32-B — E09: one-factor file-boundary intervention

### L0 выполнено (не neural quality)

Source: [E09 `BuildTokenVectors` и `train.Zip`](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md). Synthetic fixture 3×4 nodes при старом 80/20 split: 9 train nodes, 8 old pairs, **2 cross-file, 6 valid intra-file**. Proxy assertions passed. Эти фикстуры — абстрактные ID, не Roslyn AST; к историческому +10.94pp они не относятся.

### L1 native E09 / test-only harness

На 6–10 небольших реальных C# файлах, в каждом несколько AST nodes: записать `fileId`, `SyntaxKind`, `SourceSpan`, `vector`, label, split owner. Оставить **одни и те же исходные files, 80/20 split (как старый код), AST extractor, per-file local kindMap, k=5, approximate/exact method**. Не смешивать это с починкой known `kindMap` или unmatched panels. Две версии *training pairs*:
- **A** `train.Zip(train.Skip(1))` независимо от `fileId`.
- **B** те же пары, отфильтрованные по `from.fileId==to.fileId`; никаких иных перестановок.

На **одних и тех же intra-file val transitions** оценить predictions: `paired top1(correct, wrong)`, per-file denominators, number removed, tie behavior, random seed, memory/time. При очень малой val панель дополнить файлы **до теста**, не менять после просмотра результата. Отдельный negative control: изменить только порядок файлов; оценить invariance per-case predictions при правильном `fileId`. Если B>A → старое межфайловое соседство причинно ухудшало *именно E09 top1* при этих условиях; если нет — не выдавать source-level дефект за measured quality regression.

### L1b — ignored `csharpSource` falsifier

Два вызова actual `E09StructuralKnn.Run("nonempty version A", files, tier)` и `Run("different nonempty version B", same files, tier)`; сравнить **все quality поля**, игнорировать wall time. Source argument должен быть инвариантен; если нет, исследовать невидимый caller path. Если API задуман так, что `files` — единственный истинный код, документировать это вместо исправления функции.

### L2 bridge to actual product quality (не автоматическое требование к E09)

Только если B улучшил paired E09 top1, изолированно перенести именно **boundary-safe transition feature** как test-only opt-in C# структурного KNN с тем же corpus/labels/caller, сравнить `law-correct` объяснения кода и поиск контекста на новых файлах + контроль unrelated-domain. Нельзя приписывать такой эффект LoRA или RP01 exact-v2. Исполнение сложного product bridge НЕ входит в L0/L1; без него «улучшилось пользовательское обучение XIIGEN» не доказано.

## Приоритет, отчёт, стопы

1. До любых native fixtures прочитать полную карточку,120§1/40§2/42/44, current pin и существующие raw/receipts/ledger; дефект измерителя или документа не назначает повтор PRIMARY. M32-A L1 —3 fixtures только в назначенном срезе; M32-A L2 дополнительно требует валидных teaching pairs и своего допуска. Это исследование *coverage policy*, не автоматическое разрешение дублировать prompt.
2. M32-B L1 в назначенном срезе — actual Roslyn AST и same-panel comparisons, trusted project/file/position/hash для обоих концов; same-count relabelled-project должен отказать. L2 не запускается автоматически после выигрыша. Missing/false/unknown provenance/budget контроль меняет итог, все диагностические Train и физические попытки учитываются.
3. Report: source SHA, path→method→line→counterexample, scorecards A/B, independent judge, original historical result **не трогать**. Остановиться при отсутствии meaningful semantic gain, а не запускать новую 500×Windows CI.
4. Роли: architect определяет владельца модели и side effects; planning сохраняет одновременную смену только одной переменной; planning/code-review проверяют исходник/законность/admission/consumer; prompt engineering формулирует разные содержательные rejected и непротекающие eval задания; human communication публикует различия **C / H / M / NOT_RUN** понятным языком. Полноценные multi-agent CODEX/Astra review и external Chinese teacher в этом исследовании **NOT_INVOKED**.

**Success gate:** L1 механический PASS ≠ L2 downstream quality PASS. Не менять `llm_mvp_core/docs/plans/replanning-possible-diffs`, не переписывать old E09 archive или RP01 results, не делать production commits.
