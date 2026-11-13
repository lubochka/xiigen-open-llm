# MICRO-N23 — короткая проверка регистровых коллизий и частично применённого Unit batch

<!-- POST113-CURRENT-2026-11-02:START -->
## Сверка после P113 — старый partial-update oracle больше не текущий

Старый протокол ниже описывает механизм до a28bc355. На нынешнем TrainStructuredBatchAsync уроки идут в staged copy; ошибка поздней строки/held-out до PublishVersion оставляет живое S0. Требование увидеть новый урок после отказа больше не является корректной приёмкой текущего кода. Исторический C# набросок не запускать как актуальный oracle без согласования.

Действующая Q129 карточка в core исправлена: malformedA обрабатывает урок в закрытой копии, но не публикует; после corrected ожидаются 6 entries у A/B, а не A9/B6. Доза 4batchcalls/4 processed lessons/20 flow+2internal сохранена; processing/published различаются. Отдельный 135/R01 проверяет real writer gate и принадлежность metric версии; это не новый фактор данного исторического опыта. Case-collision гипотеза этой карточки здесь заново не аттестована.

Основание: [сводка этого аудита](codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core 134](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md), [план 135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Core `21007e25ec3f6f6634dd8733fc8631ae267e518c`, planning `5fc34f3c19a12d46b7e48707aea89c28b6ec32a0`, 2 ноября 2026. Нижний текст сохраняет свой исторический срез; он не получает новую нативную приёмку и не переопределяет нынешний контракт. Новые Train/inference/PowerShell/.NET этим аудитом не исполнялись; данные, дозы и очередь сохранены.
<!-- POST113-CURRENT-2026-11-02:END -->

**Статус: протокол и готовый C# набросок, НЕ ИСПОЛНЕНО** (16.10.2026). Владелец: CODEX / Astra 6 UltraCode. Контекст: [N23 source audit](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md), core SHA `0eccbdc3c4efb90adf1d699806cbfa9f5610998d`. Исследуются реальные `SemanticTokenizerUnit`/`TranslationMachine`, **не Student RP01 DPO**. Нельзя переносить выводы между этими recipients без caller trace.

## Что за один короткий опыт мы узнаем

- `CASE-01`: может ли порядок добавления двух законных учебных строк, отличающихся регистром, поменять правильный ответ на **первую** строку? Условный механизм — `StringComparer.OrdinalIgnoreCase` на correction map.
- `ATOMIC-01`: может ли машинная память научиться на первой строке batch, который целиком сорвался на следующей, не отразив это в истории успешных batch? Условный механизм — mutation до полной валидации.
- `QUALITY-01` (только после механического подтверждения): улучшает ли защита этих границ качество по blind оценке CODEX при **том же** корпусе, времени/памяти, семантических законах и ordinary consumer?

## Native C# минимальные reproduction tests

Достаточно одного временного `Tests/N23TranslationUnitMicroTests.cs` в существующем проекте `Tests/LlmFromScratch.Tests.csproj`. Пример ниже — **иллюстрация входов**, а не отчёт о запущенном тесте. Напрямую использовать реальные классы из `27.TranslationDomain` и сохранение `ExportCheckpoint()`. Контрольные input не должны содержать приватных данных.

```csharp
using System.Text.Json;
using TranslationDomain;
using Xunit;

public class N23TranslationUnitMicroTests
{
    static TranslationLearningExample Row(string id, string text, SentencePattern pattern)
        => new(
            ExampleId: id,
            UnitId: TranslationUnitIds.SemanticTokenizer,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(new TokenizerInput(text, "en")),
            ChosenOutputJson: JsonSerializer.Serialize(new TokenizerOutput(pattern, "N23|" + id)),
            RejectedOutputJson: "{}",
            RejectedType: "N23_mechanism_probe",
            SemanticSignature: "N23|" + id,
            SourceId: "synthetic-only-N23",
            HeldOut: false);

    [Fact]
    public void Case_Fold_Can_Overwrite_A_Distinct_Training_Lesson()
    {
        var t = new SemanticTokenizerUnit();
        t.TrainOnExample(Row("A", "US", SentencePattern.SVO));
        var aBefore = t.Tokenize("US", "en").Pattern;
        t.TrainOnExample(Row("B", "us", SentencePattern.IMPERATIVE_V));
        var aAfter = t.Tokenize("US", "en").Pattern;
        var bAfter = t.Tokenize("us", "en").Pattern;

        Assert.Equal(SentencePattern.SVO, aBefore);
        Assert.Equal(SentencePattern.IMPERATIVE_V, aAfter); // EXISTING code prediction
        Assert.Equal(SentencePattern.IMPERATIVE_V, bAfter);
        // Reverse A/B order in second run; both map to SVO.
        // Independent semantic oracle needed before calling this a quality defect.
    }

    [Fact]
    public async Task Failed_Batch_Can_Leave_An_Uncommitted_Lesson_In_Memory()
    {
        var m = new TranslationMachine();
        var original = m.ExportCheckpoint().Value!;
        var good = Row("OK", "an exact first lesson", SentencePattern.SOV);
        var bad = Row("BROKEN", "second lesson", SentencePattern.SVO)
            with { ChosenOutputJson = "{bad-json" };
        var batch = new TranslationLearningBatch(
            "n23-private-run", TranslationMachine.DomainTag,
            new[] { good, bad }, new[] { "synthetic-only-N23" });

        await Assert.ThrowsAsync<JsonException>(
            () => m.TrainStructuredBatchAsync(batch));
        var after = m.ExportCheckpoint().Value!;

        Assert.Empty(original.SemanticTokenizer.SignatureCorrections);
        Assert.Single(after.SemanticTokenizer.SignatureCorrections); // current path expectation
        Assert.Empty(after.TrainingHistory); // batch never finished
        Assert.False(after.HasStructuredTraining); // yet a Unit already changed
        // Compare learned exact query behavior; do not compare ExportedAtUtc hashes.
    }
}
```

**Проверить перед запуском:** пакет Xunit фактически в `Tests`, namespace классов и public constants текущего HEAD, `OperationResult.Value`, исключение `JsonException`; если реальная сигнатура изменилась — обновить черновик по source, не подгонять expected. В тесте `CASE-01` две `SentencePattern` нарочно назначены различными: это доказывает *структурную* возможность стирания; **не** доказывает языковую правильность искусственных меток.

Запуск только адресный: `dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~N23TranslationUnitMicroTests`. Не запускать Windows CI кампанию, 500 DPO и полную suite. Сохранить вывод консоли, актуальные commit SHA, test source SHA и checkpoint state до/после.

## Controlled causal experiment — улучшение реального качества

**CASE quality:** CODEX/Astra 6 UltraCode заранее выбирает 4–8 подтверждённо различимых по значению case-sensitive source pairs для одного языка/задачи (actor, code-identifiers, proper-noun versus verb). Предварительная проверка: для каждой пары существует независимый корректный `TokenizerOutput`, поддержанный источником и нужный ordinary consumer; у case-insensitive контроля oracle совпадает. Сравнить две arms: A — исходный correction key, B — shadow `(SourceLang, Ordinal normalized text)`, не меняя ни правило, ни KNN, ни данные. В обоих arms обучить обе противоположные case lessons, затем оценить **обе исходные строки** (retention) и 8 новых source-disjoint строки (generalization), pre/post, в одинаковом порядке. Измерять exact case confusion, wrong actor/polarity, abstain, per-law quality, consumer invocation и latency. **Нельзя** назвать удержание exact-source памяти обобщением.

**Atomic quality:** подготовить нормальный A→B и A→broken-B corpus с одним встроенным malformed JSON только для изолированного synthetic test. A — текущий способ; B — shadow preflight/transactional state swap. Требование: после отказа B поведение и fingerprint всех Unit state должны быть идентичны состоянию до batch; после нормального успешного batch оба алгоритма должны давать одинаковые ответы. Любая разница при успешном batch — отдельный regression. Только если существуют реальные partial-failure, оценить на фактических query, где dirty state влияло на ответы.

## Фальсификаторы, критерии и границы

| ID | Проверка | Механизм подтверждён, если | Гипотеза опровергнута / частично |
| --- | --- | --- | --- |
| CASE-01 | 2+2 training order swap | один raw correction slot и изменение A из-за B | сохраняются два независимых outputs |
| ATOMIC-01 | valid A → invalid JSON B | exception и изменение checkpoint Unit без успешной истории | preflight отказывает до любого изменения |
| QUALITY-01 | A/B одна выборка, один consumer | blind paired semantic improvements на source-disjoint без law regressions | только exact memorization, 0 gain, invalid provenance |
| NEG-01 | identical case-equivalent + same oracle | нет ложного claimed improvement | любые отличия означают проблему контроля |

**Early exit:** если CASE-01/ATOMIC-01 не воспроизведены native C#, не переписывать код. Если какой-либо consumer никогда не использует конкретный Unit, закрыть `NOT_CONNECTED_TO_CONSUMER` и не объявлять продуктовый gain. Протокол не объясняет автоматически RP01 0/3 и 0/4; для него сначала холодный B0 SFT→DPO ablation и актуальные DPO pair/gradient checks.

**Выход:** `N23-raw-native-results.json` с `head_sha`, `fixture_sha`, `method`, `before_after_snapshot_fingerprint`, `exception`, `train_exact_retention`, `blind_source_disjoint_quality`, `consumer_trace`, `cpu_ms`, `peak_ram`, `review_status`, `claim_scope`. Если не измерялось — `NOT_MEASURED`. Получив raw evidence, отдельный CODEX/Astra 6 UltraCode проверяет результат, не принимает собственную оценку автора испытания на веру.

**Роли при DPO:** если позже понадобятся новые пары `chosen`/`rejected`, оба ответа производит разрешённая **внешняя открытая китайская модель**; человеческий отбор учебных задач/пар и их независимая оценка выполняются **CODEX с Astra 6 UltraCode**. Не подменять модель-автора CODEX и не маркировать synthetic/test-only как production-model-generated.
