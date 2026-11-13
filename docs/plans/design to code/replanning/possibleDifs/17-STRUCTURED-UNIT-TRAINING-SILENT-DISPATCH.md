# 17 — Новый Unit-seam: обучение молча пропускает получателя, но отчёт считает строку обученной

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: отказ поздней известной строки должен сохранить память

Unknown UnitId и известный UnitId с несовместимым JSON-типом — разные случаи. Новые `TranslationBatchTypeMismatchRollbackTests` предъявляют законную строку перед ошибочной; добавленный тест ещё не доказывает, что production-код откатывает уже обученные Unit.

Для следующего адресного ремонта снять S0 через тот же reader, предъявить назначенную смешанную партию, получить отказ с действительными batch/row/unit identity и причиной, затем **до внешнего restore** сравнить полный learned state, version/history и ответы прежнего и нового ordinary reader. Положительная партия обучает все назначенные строки; ошибочная не публикует частичное состояние. Контроль с намеренно выключенной отменой изменений должен обнаружить новую память первой строки, а не только код отказа.

Синтаксическая десериализация, допустимость enum/типа и успешный trainer callback фиксируются отдельно. Исполнитель не ослабляет test ожидание под текущий partial update и не меняет gold. API/политика отказа берутся из адресного core-плана; тестовый try/catch не является доказательством rollback машины. Никакого дополнительного Train для оформления отказа.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

**Дата:** 2026-10-16. **Статус:** `SOURCE_VERIFIED_CONDITIONAL_BUG / UNIT_ADOPTION_NOT_MEASURED / NATIVE_TEST_NOT_RUN`. **Pinned core:** `codex/dpo-implementation@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. Не приписывать дефект автоматически RP01, потому что непосредственный RP01 Student — отдельный поток Transformer/DPO/LoRA.

## Проблема для человека

У XIIGEN несколько обучаемых Unit. Недостаточно сказать, что DPO «принят»: полезный учебный пример должен попасть в **конкретный именованный получатель**, изменить его состояние и затем использоваться реальным Flow. Обнаружен маршрут, в котором строка с неизвестным ID получателя пропускается, но общее поле отчёта `RowsTrained` и номер версии могут увеличиваться. Это создаёт ложную видимость обучения без изменения соответствующего Unit.

## N17-A — Source-confirmed counted-but-not-trained

В [TranslationMachine.TrainStructuredBatchAsync, строки 89–145](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L89-L145):

1. Для каждой не-heldout записи выполняется `if (!_unitTrainers.TryGetValue(example.UnitId, out var trainer)) continue;` (строки 107–112). Неизвестное имя — **пропуск без ошибки**.
2. `trainedUnits.Add(example.UnitId)` происходит только после успешного вызова `trainer.TrainOnExample(example)` (113). Это более честный индикатор, чем RowCount.
3. `RowsTrained = batch.Examples.Count(example => !example.HeldOut)` (135) считает **все** не-heldout записи, включая пропущенные. `IncrementPatchVersion()` (131) вызывается вне зависимости от того, найден ли хотя бы один trainer; `_hasStructuredTraining = trainedUnits.Count > 0` частично защищает оценку (129), но не исправляет счётчик.

**Условный воспроизводимый пример:** подать одну строку с `UnitId="executor-ru_EN-UNKNOWN"` и `HeldOut=false`. `TryGetValue` возвращает false: actual trained recipient = 0, `RowsTrained=1`, `PromotedUnitIds=[]`, patch version incremented. Это логически следует из показанного кода независимо от обучающих весов. **Факт столкновения именно такого ID в production/corpora не установлен**.

## N17-B — test/injected constructor создаёт нулевой trainer map

[TranslationMachine, строки 31–49](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L31-L49): обычный конструктор `new TranslationMachine()` строит `CreateRuntime()`; конструктор `TranslationMachine(TranslationRoutingFlow flow)` задаёт пустой словарь `_unitTrainers`. При использовании последнего для structured training каждая запись будет пропущена тем же маршрутом, тогда как поля отчёта могут выглядеть как работающий batch. Исходные комментарии называют такой constructor тестовым; НЕ заявляем, что именно он используется при ordinary user request.

[CreateRuntime, строки 714–770](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L714-L770) создаёт SemanticTokenizerUnit, ExecutorUnits по направлениям и QualityVerifierUnit и регистрирует их в словаре с `StringComparer.Ordinal`. Значит case, формат направления, версия схемы Unit ID являются частью фактического train/serve contract, а не косметической меткой.

## Важная положительная улика: не все Unit являются имитацией

[ExecutorUnit.TrainOnExample, строки 403–456](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L456) записывает выбранную сторону в `_exactSourceMap`, `TranslationKnnStore`, word/phrase maps. [ExecutorUnit.Execute, строки 262–280](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L262-L280) читает эти же структуры. Этот **настоящий discrete memory learning** нельзя смешивать с LoRA-gradient. Проверять нужно сохранение данных *и* их достижимость через обычный Flow.

## Три разных причины одного внешнего симптома

| Симптом | Возможная причина | Различающий наблюдаемый факт |
| --- | --- | --- |
| `RowsTrained>0`, ответы прежние | неизвестный UnitId или пустой registry | `actualTrainerInvocations=0`, `PromotedUnitIds=[]` |
| Unit вызван, ответы прежние | память изменилась, но consumer не читает её/выбран иной path | новый checkpoint hash и direct Unit ответ меняется, ordinary Flow — нет |
| Direct Unit и Flow меняются, score не растёт | качество/покрытие/метрика, а не loss of training | gold-heldout показывает ошибки/регрессию, require new-law contrasts |

## Ограниченный causal probe N17

**Baseline:** один frozen machine с валидным UnitId и один с намеренно неизвестным UnitId при одинаковом семантическом примере; в обоих `HeldOut=false`. Записывать `RowsTrained`, `PromotedUnitIds`, old/new state digest, direct Unit result, Flow result. **Enhanced:** reject unknown Unit ID до mutation/version increment, а `RowsTrained` вычислять по реально вызванным trainers. В обоих режимах *валидная* строка должна обучать, invalid — давать явную ошибку. Отдельным тестом использовать test-only injected constructor.

**Что подтверждает механизм:** invalid ID больше не засчитывается как обучение; валидная строка действительно меняет обучаемую memory. **Что доказывает влияние на качество:** при неизменном heldout и обычном consumer потеря/восстановление именно корректной маршрутизации изменяет независимый перевод/семантический law verdict; фиксированная таблица score заранее. Изменение одного `RowsTrained` без улучшения реального ответа — только исправление evidence, не learning gain.

**Фальсификатор RP01-гипотезы:** если actual RP01 route не использует `TranslationMachine.TrainStructuredBatchAsync` или все нужные UnitIds корректны, дефект не объясняет деградацию RP01; оставить в реестре как отдельный продуктовый риск, не менять причинный verdict.

## Оформление и исполнение

В work order CODEX: загрузить `code-examination-SKILL.md`, `learning-path-audit-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `experiment-design-SKILL.md`, `plan-review-SKILL.md`, `claim-verification-SKILL.md` и `human-readable-architecture-audit-SKILL.md`; самостоятельно проверить путь from `TranslationStructuredBatchBuilder` / `TranslationMachine` construction до actual ordinary Flow. Не запускать весь Windows GitHub gate. Сохранить исходные frozen DPO/RP01 артефакты; read-only baseline, затем test-only isolated fixture, независимый reviewer для результата.

**Связанные уже найденные различия:** [исходные 44 DIF](01-PRIORITIZED-DIFFERENCES.md), особенно `DIF-26` (DPO→Unit) и `DIF-43` (feedback→adoption). **Новизна** N17 — конкретная рассинхронизация `TryGetValue → continue` и `RowsTrained`, с прямым source-level counterexample, а не общий вопрос о Unit wiring.

## Сверка с независимым audit (дедупликация)

Этот seam **совпадает** с `NEW-16.03` в [16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md). Нельзя учитывать N17 unknown-unit `TryGetValue→continue` и `RowsTrained` как отдельные дополнительные дефекты. Новые независимые элементы соседнего разбора: known-unit JSON-null no-op, `_hasStructuredTraining` reset, wrong-direction evaluator, macro-vs-micro scoring. Здесь оставлены краткий отдельный контрпример test-only injected constructor и train→consumer diagnostic work order; фраза «новизна» в более раннем тексте означает concrete source seam относительно **старого** общего DIF-26, а не уникальное первенство относительно параллельного отчёта.
