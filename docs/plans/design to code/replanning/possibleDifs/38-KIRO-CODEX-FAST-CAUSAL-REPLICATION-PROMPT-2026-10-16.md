# RP01 N38 — готовое задание Kiro/Codex на один научный локальный прогон, без перепланирования платформы

**Назначение:** исполнить только [MICRO-38-A](38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md) и собрать проверяемый native result. **Статус:** prompt READY / EXECUTION NOT_RUN. Не использовать этот файл как подтверждение результатов. Основа источников: [N38 hypothesis](38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md) и [frozen OPEN fixture](38-MICRO-COUNTERFACTUAL-CODE-PAIRS-OPEN-v1.json).

## System/manager brief агенту

Ты выполняешь **один** read-only научный эксперимент над уже сохранёнными моделями RP01. Цель: отделить **неусвоенную связь input→law** от **потери усвоенного law в свободном decoder**. Не демонстрируй зелёные unit tests как доказательство learning. Нельзя менять оригинальный frozen RP01 dataset, exam/rubric, saved result, trained checkpoints; нельзя править **никакие** файлы в lubochka/llm_mvp_core/docs/plans/replanning-possible-diffs. Отчёты и prompt-протокол публикуй только в xiigen-planning/llm_plan3/.../replanning/possibleDifs по окончании **действительно выполненного** запуска.

Прочитать сначала соответствующие skills (не копировать в контекст сразу всю библиотеку):
- docs/skills/architect-behavior-classifier-SKILL.md — граница neural Student против Unit/KNN.
- docs/skills/code-examination-SKILL.md — code path, exact method callsites, true recipient.
- docs/skills/experiment-design-SKILL.md — hypothesis, controls, false positives, pre-registration, metrics.
- docs/skills/claim-verification-SKILL.md и plan-review-SKILL.md — proof/status/negative examples.
- docs/skills/phase-planning-SKILL.md, prompt-patch-authoring-SKILL.md и conversation-intent-and-self-reflection-SKILL.md — короткая понятная коммуникация; не перезапускай многошаговое «планирование разработки».

### Роль 1: архитектор и code examiner — до кода

1. Открой RP01 run receipts и current core source на указанном SHA. Подтверди B0/C50 реальную архитектуру, правильный tokenizer state/shape, response target mask, identity all checkpoint and model adapters. Не переносить чужой ENRU C100 в эксперимент C#.
2. Проверь read-only scoring API: [DpoTrainer.EvaluateAverageNextTokenLogProbability](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L953-L985) возвращает mean logP по masked next-token positions. Нужна canonical BPE-v2 segmentation и mask с единицами только response. Сумма для DPO-style сравнения = mean×targetCount при единичной mask. Сверь с [ComputeTargetLogProbs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1028-L1068), не вводи самодельный off-by-one.
3. Сначала получить лишь provenance artifact hashes и составить скромный plan ≤1 executable native probe. Если корректного B0 или C50 checkpoint, tokenizer или code evaluator нет — **STOP: PRECONDITION_FAILED** с точными путями и кодом причины. НЕ компенсировать переобучением с другим seed.

### Роль 2: executor — один вход, одна изолированная интервенция

1. Загрузить JSON fixture из planning unchanged, hash; запретить использовать его как DPO train.
2. В локальном disposable .NET8/9 harness, использующем существующие project references (не новый переводчик/генератор), независимо подтвердить stdout всех 12 C# кодов. Отдельно сохранять native stdout vs fixture; при несовпадении — перечитать law с экспертом, статус INVALID_FIXTURE, не переписать fixture под результат C50.
3. Открыть B0 и C50 **в разных read-only моделях**. Для 6 пар посчитать [logP(y0|q0),logP(y1|q0),logP(y0|q1),logP(y1|q1)] при одинаковой инструкции и canonical encoding; взять response sum и mean, сохранить маски и BPE id digests.
4. Для тех же q получить ordinary Student free generation (temperature=0 и неизменный budget) без forced prefix, без teacher answer in prompt. Сохранить output hashes/text, stop reason, first token rank если runtime поддерживает его нативно.
5. Cold reopen C50 и повторить 6 пар scoring; доказать state identity и parity. Не писать checkpoint назад и не запускать github/windows/Playwright pipelines.

### Роль 3: независимый reviewer и human communication

1. Отдельно пересчитать d0=s00−s01, d1=s10−s11, contrast=d0−d1, effect=contrast_C50−contrast_B0; проверить, что masked response scores, а не все tokens. Не читать label verdict до raw score freeze.
2. Судить **двумя разными метриками**: (A) противоположные signs условных preferences; (B) правильность free full answer по независимому C# execution/semantics. Не объявлять DPO margin learning gain.
3. Реестр adverse: wrong numbers, wrong explanation, aborted output, false-sense confidence, no output. Показать все 6 пар и нули. Серия 6 = triage, not generalization. Для подтверждения качества запрашивается заранее зафиксированная 8–12 source-family-disjoint панель; не делать вывод out-of-domain по 6 train-like формулам.
4. Отчёт по-русски максимум 1 страница executive summary + машинный receipt: какой exact source SHA и checkpoint, что реально запускалось, формулы, таблица B0/C50, 3 варианта NEXT_ACTION, ограничения. Маркировать NOT_RUN если логов нет, а не «100% coverage». Предложенные решения: (i) weak conditioning→ tiny SFT/warm foundation, (ii) signs acquired/free bad→ decoder/prefix/adoption, (iii) good signs/free good→ расширить blind domain tests, (iv) tool invalid→ устранить только инструмент.

## Допустимая публикация результата

Создать **новый** файл 38-MICRO-COUNTERFACTUAL-CONDITIONING-RESULT-2026-10-16.json (если реальный executable run состоялся) с run_id, source/fixtures SHA, state hashes, raw rows, C# stdout evidence, original model outputs, scored metrics, peer verdict, timestamp и negative controls. Отдельно 38-MICRO-COUNTERFACTUAL-REVIEW-2026-10-16.md с human verdict. Не создавать receipt со значениями, если выполнялся лишь source audit; status=NOT_RUN у протокола в этом случае сохраняется.

## Stop-lines и review criteria

- Изменена более чем одна причинная переменная (model-state B0 vs C50, при неизменных q/y/tokenizer/decoder) → INVALID_COMPARISON.
- Fixture отредактирована после просмотра raw C50 output → DATA_LEAK.
- Нет настоящих restored states/real model/API → NO_NATIVE_PROBE, никаких выдуманных score.
- Вместо сравнения двух противоположных q использовали два разных y при одном q → это другой старый experiment, не N38-A.
- Поменяли heuristic KNN/Unit, а затем приписали результат нейронному Student → WRONG_RECIPIENT.
- Изменяли protected core/docs/plans/replanning-possible-diffs, frozen original datasets либо исходные receipts → PROHIBITED.
- Отправили десятки Windows CI ради проверки шести безвредных scientific inputs → MISDIRECTED_COST.

**Решение должно быть готово к просмотру человеком:** конкретные source evidence, 1 изменяемый фактор, 1 таблица нулей/успехов, однозначный следующий эксперимент. Никаких скрытых корректировок под pass.
