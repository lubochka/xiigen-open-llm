# MICRO-N26 — самый дешёвый запуск проверки новых механизмов, не ещё один GitHub Windows gate

**16.10.2026 | Work order для исполнителя, НЕ выполнен в .NET; независимое CODEX/Astra review НЕ выполнено.** Исходная ветка core codex/dpo-implementation@b0afc08dc0df110b10ed5ed38b518906665c16f8; изменять продукт не разрешено этим исследовательским work order. Предварительно изучить [N26-A](26-NEW-E27-STRUCTURAL-REORDER-COVERAGE-LOSS-2026-10-16.md), [N26-B](26-NEW-ADAMW-SCALE-CANCELLATION-2026-10-16.md), [RP01 learning recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

## Один быстрый native optimizer probe — минимальный исполнимый C#

Исполнитель создаёт **одноразовый проект во временном каталоге рядом с корнем core** (например tools/N26AdamWScaleProbe). Использовать существующий проект TrainingLoop, не копировать исходник AdamW и не менять сам оптимизатор. ProjectReference из tools/N26AdamWScaleProbe: ../../04.TrainingLoop/TrainingLoop.csproj; TargetFramework net9.0; ImplicitUsings=enable. В Program.cs:

~~~csharp
using TensorCore.Core;
using TensorCore.Layers;
using TrainingLoop.Optimizers;

static float OneFreshStep(float gradient)
{
    var parameter = new Parameter(
        new Tensor(new[] { 0f }, new TensorShape(1)));
    parameter.Value.Grad = new Tensor(new[] { gradient }, new TensorShape(1));
    var adam = new AdamW(beta1: 0.9f, beta2: 0.95f,
                         weightDecay: 0.01f, eps: 1e-8f);
    adam.Step(new[] { parameter }, lr: 1e-5f);
    return parameter.Value.Data[0];
}

float a = OneFreshStep(0.01f);
float b = OneFreshStep(0.02f);
float negative = OneFreshStep(-0.01f);
Console.WriteLine($"A={a:R}; B={b:R}; negative={negative:R}; ratio={MathF.Abs(b/a):R}");
~~~

Команда в рабочем каталоге одноразового проекта: **dotnet run -c Release**. До запуска убедиться, что SDK поддерживает net9.0 и ProjectReference резолвится; отсутствие SDK=ENVIRONMENT_BLOCKED, не выдавать за FAILED hypothesis. Расхождение <1% для ratio ожидается для g=.01/2g=.02; градиент −.01 должен инвертировать знак. Снять один JSON-like receipt с ref, commit, runtime, входами, числами и output, не организовывать полный CI, не commit продуктовый код по умолчанию.

**Ограничение:** этот probe доказывает numerical cancellation **одного AdamW шага**, но не полезный новый ответ; не писать «Student learned» даже если ratio=1.

## Отдельный короткий опыт влияния именно на обученную функцию E27

Для главного вопроса пользователя о **качестве обучения**, тест должен содержать обучение памяти, а не только перестановку правил.

1. **Pre:** фиксировать два экземпляра обычного Translation Executor/Flow с пустой обучаемой памятью. Не использовать исторический готовый exact-source key в heldout.
2. **Train (оба):** вне heldout дать одинаковые 6–12 корректных новых структурированных переводческих уроков: chosen lexical/word mappings для двух семейств SOV/VSO + TOPIC_OV. Зафиксировать digest maps/KNN/checkpoint, реальные changed entries. Для чистого эффекта порядка A и B должны получить **побитово одинаковую память**.
3. **A:** текущий ExecutorUnit fallback + ApplyE27Reorder. **B:** такая же frozen learned memory, но заменить *только* fallthrough reorder на историческую таблицу PatternReorderer.Reorder при SourcePattern, определённом обычным парсером. **C:** no-learn с той же таблицей — контроль реального вклада уроков. **D:** B с неправильным SourcePattern или без него — контроль скрытого oracle.
4. **Exam:** 6–8 **новых source-disjoint** предложений с теми же предметными закономерностями, но новыми лексическими комбинациями и по возможности новым контекстом; обязательный отдельный порядок предложений, NEGATION и preserved referent где применимо. Test до выполнения output никакого chosen/expected не видит. Исключить exact/phrase/KNN bypass; логировать вызов reorder, otherwise mark NOT_APPLICABLE.
5. **Primary:** прирост процента корректных source→target outputs на **том же** heldout: (B_post − B_pre) − (A_post − A_pre); отдельно показать B_post−C_post, per-case correct/critical error/abstention. Это позволит ответить именно «структурный перенос улучшает качество обучения словесных mappings?» и отделить выигрыш чистых правил.
6. **Решение:** если B качественнее A при одинаково выученном состоянии, B_post>C_post, вызов реально достиг reorder, а разница воспроизводима ещё на одном source-disjoint mini-panel, зафиксировать LIMITED_POSITIVE_CAUSAL_SIGNAL. Если выигрывает B, но не B_post−C_post, это улучшение **детерминистической композиции**, не доказательство обучения. Если у A/B route разный до reorder — эксперимент испорчен, не адаптировать labels.
7. **Stop:** не переходить к 500 DPO, пока не показана хотя бы маленькая воспроизводимая learning-dependent difference. Не менять frozen результаты и score rubric после запуска.

**Time/cost target:** один маленький native optimizer-only test и один bounded translation-memory mini-run; никакой полной сборки Windows matrix на GitHub. Число минут заранее не гарантировать. Skills для исполнителя: docs/skills/experiment-design-SKILL.md, code-examination-SKILL.md, plan-review-SKILL.md, root-cause-tracing-SKILL.md, human-readable-architecture-audit-SKILL.md, prompt-patch-authoring-SKILL.md.

## Приёмочный формат одного case

~~~text
case_id | source_family | source_text_sha | trained_state_sha
expected_law (sealed until scoring) | actual_pattern_from_parser
A_pre | A_post | B_pre | B_post | C_post | D_post
selected_route | reorder_reached | semantic_correct | critical_error
~~~

Сводка должна отдельно показывать **optimizer numerical finding**, **quality after training**, **learned map changed**, **real consumer used trained state**. Эти четыре сущности не взаимозаменяемы.
