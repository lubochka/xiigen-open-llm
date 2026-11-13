# Кандидаты в исправления: измеренное, ещё не измеренное и документные конфликты

Этот отчёт добавлен как документный кандидат. **Полезное улучшение отдельной метрики сохраняется как успех этой метрики и темы; ухудшения других метрик показаны рядом.** Перенос в продукт, повтор старых опытов и полная цепочка исправления тестов не выполнялись и этим документом не назначаются.

## Поздний измеренный кандидат LC-REF-CACHE

Core0cf3634 добавил A/B raw: allocated bytes7 331 104 632→6 010 567 464 (−18.01%), train wall23 096.5536→19 375.8694ms (−16.11% в одном запуске). В обоих плечах сохранены46/46 и44/44 teacher-forced TRAIN tokens и2/2 полных teacher-forced responses; фактических DPO updates2 на arm. Это полезный ресурсный результат на реальном обучении. SFT moments/RNG общего старта не захвачены; для этой пары использованы weights-only и fresh DPO epoch. Phase timing содержит противоречивые Started/Completed/Incomplete counts, поэтому его детализацию нельзя выдавать за надёжное измерение чистых policy/reference стадий. NLL после DPO немного вырос одинаковоA/B; все дельты и исходные ссылки — [в дополнении09](09-late-core-0cf3634-update.md).

Кандидат уже записан в поздний защищённый required-changes другим участником: scoped reuse frozen-reference scores в `11.DPO/DpoTrainer.cs`; перенос не выполнялся этим аудитом. Пара consumed:0 новых Train/Forward/Generate/recovery.

## 1. Уже измеренные полезные эффекты

Следующие результаты уже внесены в защищённый [required-changes.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/required-changes.md). Здесь их согласованное прочтение и точные границы; это не шесть новых запусков. Raw keys, таблицы и provenance — в [аудите §5/§10](02-current-card-audit.md).

| Кандидат / место будущей правки | Полезный измеренный показатель | Ухудшения и открытые цели |
|---|---|---|
| **N63-CORRECTION-COLLISION** — `14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs`, подготовка domainExamples до provenance/material binding: при том же domain/prompt и точном совпадении SFT.Response с DPO.Rejected передать исправленный источник, сохранить оба SourceId | В завершённом N63C-PROBE после одинакового DPO и последующего SFT corrected B сохраняет21/23 правильных TRAIN tokens против20/23 у stale A; margin B+1.1151558 против A−0.119724035 | Общий before22/23: обе руки потеряли токены. Whole-response exact=false везде; EXAM0, NLL уB хужеA. Historical binary/current source и полная state custody не аттестованы; это ограниченный частичный результат, не доказательство готового общего исправления |
| **N60-E1** — `27.TranslationDomain/TranslationStructuredBatchBuilder.cs`, verifier projection128–141: не добавлять отрицательный verifier lesson, когда rejectedCandidate ordinal-equal chosen | False reject1→0; состояние6695→6544байта; нет роста accuracy,3/4 в обоих плечах | False accept0→1. Отсутствующий корректный negative lesson не восполняется фильтром; до широкого переноса нужен назначенный контраст с действительным negative того же маркера |
| **WIKI-TAXONOMY-01** — `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs`: добавить2labels после15старых, правильный NotProjectWiki fallback index и2decoder ветви | Правильный выбор2/6→4/6; wrong4/6→1/6; старый CodeTrace сохранён | Переводческий negative control стал неверным, появился1defer;570→935 параметров,9224→14885bytes snapshot,+7024allocbytes. Старый positional snapshot и full product router не проверены |
| **WIKI-MARGIN-01** — тот же model `Train`: выбирать конкурента вне expected и делать update при margin<0.10 | На целевой router_training группе0/2→2/2; реальный update появился: A0/B1 | B выбирает один класс везде, ровно constant baseline2/4; отрицательные2/2 стали wrong. Различение маршрутов OPEN. Разные WT/WM datasets не дают причинного объяснения регрессии |
| **N-KU-01** — `12.SemanticCompiler/Prediction/FileActionKnn.cs::ActionLabelGenerator.Label`, hasLoop: добавить `SyntaxKind.DoStatement` | Ложный совет добавить уже существующий цикл3/3→0/3; correct1/4→3/4 | Нужный loop потерян1/1→0/1; outputs всего majority-class shift приk3. FeatureVector без do-while не исправлялся, структурное обобщение не доказано |
| **BPE-NO-MATCH-COPY-01** — `01.Tokenizer/BpeTokenizer.cs::MergeChunk`, возврат исходного chunk при отсутствии merge match вместо лишней копии внутри настоящего Train | Allocated code-doc3904664→1126744bytes (−71.14%); ENRU2892824→893656 (−69.11%). Dictionary256→320/64learned merges, A/B hashes равны | Code-doc CPU31.25→46.875ms хуже; ENRU CPU не изменился. Single ordered run не доказывает устойчивый speedup. Roundtrip4/4 — lossless контроль, не качество перевода |

Успех BPE по allocations не зависит от роста accuracy. Успех N60 по false reject сохраняется при неизменной общей точности. Успех N63C по удержанию одного дополнительного токена не превращается в доказательство полного усвоенного ответа. Это разные решения пользователя о полезности переноса, а не один универсальный PASS/FAIL.

Исторические E09 Tier1, E40V2 и E36 также сохраняют положительные показатели. Их факторы/метрики и ограничения переноса приведены [отдельно](03-history-and-domain-transfer.md); ранний FAILED не служит основанием удалить эти receipts. Они не объявлены новыми current product candidates без соответствующего consumer/state trace.

## 2. Новые гипотезы — пока без измеренного улучшения

| ID | Точное место и предлагаемый фактор | Доказательство участия в обучении | Статус |
|---|---|---|---|
| [FILE-STATIC-RECEIVER-01](cards/FILE-STATIC-RECEIVER-01.md) | FileActionKnn.cs:32–38, исключить resolved static field из hasMemberAccess | Автоматический label → native Train(ordered memory) → native Predict; opt-in facade consumer показан | Новый подслучай N48; code fact подтверждён, эффект неизвестен |
| [RMS-MEAN-BACKWARD-01](cards/RMS-MEAN-BACKWARD-01.md) | TensorOps.BroadcastGradAlongAxis:1049–1060, адрес srcFlat/axisSize только для last-axis keepDims=true | Native SFT/LoRA → norm2/finalNorm → Mean backward → AdamW → learned readout | Ресурсная гипотеза; аналитический счёт — 5 376 int[3]/update, measured bytes/time отсутствуют |
| [ATTN-SOFTMAX-COPY-01](cards/ATTN-SOFTMAX-COPY-01.md) | TensorOps.Softmax:793–848, удалить первый copy-pass, читать x в max/exp, сохранить арифметику | Native attention forward внутри настоящего LoRA update | Ниже по приоритету; массивы результата остаются, ускорение не измерено |

Эти записи не имеют статуса «удачное исправление». Они являются конкретными кандидатами **на эксперимент**, со source evidence и фиксированным рецептом. Включение в этот файл не подменяет отсутствие A/B числами по коду. Доказанный новый дефект предметного качества LoRA/DPO/restore здесь не заявляется.

## 3. Исправления исполнительского и документного контракта

**EA-01, сначала:** в `experiments-v2/RP01-MICRO/N61-REF-CACHE/Run-With-Learning-Signal.ps1:9–19` для будущих ещё не выданных запусков нужен внешний guard existing attempt/claim **до build и перенаправлений журналов**. Нынешнюю выданную попытку не перезапускать и не изменять. Это прежний статус43e2337. В позднем0cf3634 common1000 и A/B raw уже сохранены; независимый terminal exit отдельно не подтверждён. Source-level риск wrapper не исправлен новым результатом; повторять consumed попытку нельзя. [Доказательства и точный override](02-current-card-audit.md).

**EA-02:** проектный конфликт закрыт core0cf3634/33 для MS01→MC01, TS01→NH01, AGC1→FBM1. Применять canonical families и gates из [каталога](05-reusable-trained-states.md); реальные полные bundles пока не materialized. Это исправление плана бюджета/восстановления, не экспериментальное улучшение само по себе.

**EA-03:** поздний core исправил index topics WIKI-FEATURE-KEYS-01 и WIKI-FEATURES-ONCE-01. В теле KN-ROUTER-LEARNED-KEY-01 остаётся строка «Домены: программирование C#/.NET и работа с документацией»; здесь явно различить topic=Оркестрация/D07 и consumer context=Wiki/C#/.NET. TRAIN и NEXT не меняются. Для этого не нужен искусственный «обучающий эксперимент».

**EA-04:** считать current N63C-PROBE законченным сохранённым partial result; не повторять старый consumed ID из-за цитаты раннего отчёта «чисел нет». Разделить фактические updates, declared budget, whole-answer goal и resource observations. Невалидный ранний0→0 опыт не становится валидным задним числом благодаря отдельной probe; полезная поздняя probe тоже не исчезает из-за раннего отказа.

## Решение после будущего опыта

При полезном измеренном эффекте исполнитель добавляет candidate с оригинальным code path/symbol, единственным diff, topic/metric, A0/A1/B0/B1, регрессиями и raw refs. Если factor не применён, donor не квалифицирован или процесс не завершён, записывает точный blocked/partial статус. Старые файлы не удаляет, не расширяет budget, не запускает production tests и не внедряет code до отдельного решения пользователя.
