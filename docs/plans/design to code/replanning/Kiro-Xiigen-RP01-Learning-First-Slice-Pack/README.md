# RP01 · Learning-first: исправленный план после двух отрицательных 500-парных опытов

**16.10.2026 · CURRENT DISPATCH: REC01–REC05 · DRAFT_PENDING_INDEPENDENT_ASTRA_REVIEW.** Больше не начинать с повторной выдачи/восстановления C# Main500. Доказанная фактическая граница — [полный source-backed аудит DPO, LoRA, kNN и затрат](RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) → [действующий стартовый промпт](RP01-EXECUTE-NOW.md) → [5 полностью разработанных task cards](tasks/recovery/README.md).

## Почему RP01 изменён

С учётом опубликованных commit/evidence 16 октября: **реальный optimizer/LoRA действительно обновляет Student**, замороженные base tensors не меняются, checkpoint сохраняется и грузится. Однако C#500 дал `0/3→0/3` правильных свободных ответов, ENRU500 `0/4→0/4`. Увеличение preference margin было численно небольшим, без улучшения semantic correctness или выбранного токена в исследованной первой позиции. Значит, **полезное обучение локальной языковой способности пока NOT_DEMONSTRATED**, веса `RESEARCH_ONLY_NOT_ADOPTED`. Важный приоритет — проверить недостаточную исходную компетенцию randomly initialized B0 + vocab629, DPO masks/signs, sensitivity of LoRA/decoder и независимость доменных законов. Не делать ещё один дорогой500 до короткого различающего эксперимента.

Уже проведённый исторический [BATCH500](tasks/batch500/README.md) означает два завершённых научных опыта, а не положительный приёмочный результат. Frozen datasets/exams/receipts/grading неизменны; ранее упоминавшийся `Main500 issued` был состоянием 15 октября, не сегодняшним launch instruction. Не выдумывать нового runtime исхода из публикации плановых файлов.

## Последовательность, разрешающая параллельную работу

| Task | Что сделать | Приёмка именно возможности, не coverage |
| --- | --- | --- |
| [REC01](tasks/recovery/RP01.REC01-EVIDENCE.md) | Пройти source→results/provenance, отделить факты от hypothesis | Constrained failure tree with next falsifier; zero model calls |
| [REC02](tasks/recovery/RP01.REC02-DPO-GRADIENT.md) | Tiny8–32 exact DPO overfit + gradient/LoRA/decoder, measured phase costs | Train preference signal/reload & observed token/semantic behavior or justified STOP |
| [REC03](tasks/recovery/RP01.REC03-CAPABILITY.md) | B0 pretraining, actual tokenizer capacity; short SFT→DPO vs DPO-only | Matched real model/holdout, evidence whether basic ability prerequisite |
| [REC04](tasks/recovery/RP01.REC04-HYBRID.md) | Real C# and ENRU domain laws, semantic representation, source-conditioned kNN versus controls | Ordinary consumer path improves correctly with precision **and coverage** reported |
| [REC05](tasks/recovery/RP01.REC05-LEARNING-LOOP.md) | Only after signal: new prospective domain-law learning curve, feedback/on-PC costs | Paired meaningful improvement or honest negative, no mandatory threshold after 500 |

REC01 + REC02 + read-only REC03 + REC04 могут идти параллельно; **единственный writer** у конкретного Student/checkpoint. REC04 — bounded RP02 handoff, а не конкурирующая реализация RP02. RP03 = `ILearningUnit` many-to-many and `ILearningFlow`; RP04 = async `ILearningOrchestrator` and higher-order `ILearningDynamicGraph`; RP05+ = continual learning/feedback/PC. Не создавать жёсткий RP01→RP02 барьер или заново R01–R25.

## Научная, а не тестовая проверка

Имеет значение **domain mapping → law-specific token/vector representation → constraints/positive-negative candidates → adaptive kNN → final consumer output**, и в ней отдельно DPO objective/LoRA/Student. Пример для языкового домена: не одно ближайшее слово, а часть речи, позиция, роль, согласование референта, scope `not`; для C# — AST, типы, порядок, nullability и специфика версии. `KnnLogicalUnit.Search(query,_k)` с `hits[0]` и hashed bag representation — реальный ограниченный consumer, но не вся XIIGEN. Детерминированный `TranslationMemorySourceLaw` уже улучшил один узкий контраст, одновременно снизив покрытие; learned adaptive k не подтверждён.

Для read-only задач 0 новых продуктовых тестов; для новых deterministic seam — только минимальная адресная компиляция/check по изменённому риску, не Windows matrix, не coverage percentages. Для learning — отдельно обучающие пары, независимые semantic families, actual optimizer steps, free responses, paired correctness, bad/edge/unknown, latency per stage and peak PC-process resources, regression in each domain. `+2/12` после одной пары был необоснован; новый будущий gain заранее не гарантировать.

**Люди и модели.** CODEX с запрошенной `astra 6 ultracode` выполняет роль человеческого выбора задания; другой CODEX с той же моделью — независимый судья exact DPO chosen/rejected; отдельный examiner — quality of blinded Student outputs. Astra development author вправе симулировать обе стороны внешней китайской открытой модели только с честной source меткой `ASTRA_SYNTHETIC_DEV` и actual actor receipts — это не фактический Qwen output. Реальная внешняя китайская модель остаётся отдельным допустимым source с фактической лицензией/условиями и проверкой происхождения. Ранее опубликованные datasets не переименовывать.

## Источники, сохранённая история и граница этой правки

Исполнитель читает skills архитектуры, планирования, plan/code review, prompt engineering и human communication в `llm_mvp_core/docs/skills`, `original task.md`, WorkLeft research/DPO/RAG, `replanning/previous rounds thinking results.md`, `replanning/results/R01–R25` по индексам и [стратегию R01–R25→code slices](../R01-R25-TO-CODE-SLICES-STRATEGY.md). Исторические `RP01-OFFLINE-TEACHER-BATCH500.md`, B500 execution tasks, manifests, frozen 2026-10-15 admission/contracts остаются доступными, **но больше не текущий dispatch**. [Предыдущий README от 15 октября](https://github.com/lubochka/xiigen-planning/blob/f8450a0133abe78b80eaa91cbf7766bf5180c4c2/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md) сохранён в git истории.

В этой правке изменяются только planning Markdown в `llm_plan3`; новые Student experiments, релиз, ещё одно независимое actual Astra review и новые замеры не выполнялись. `DRAFT_PENDING_INDEPENDENT_ASTRA_REVIEW` означает незавершённую проверку нового плана, а не неуспех прежних опубликованных инженерных проверок.
