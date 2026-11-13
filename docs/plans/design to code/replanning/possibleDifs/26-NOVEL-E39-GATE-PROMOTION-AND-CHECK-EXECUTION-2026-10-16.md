# N26-A — E39: исторический PASS при нарушенном обязательном gate; исполнитель проверяет только 2 условия

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED для конкретных путей; **влияние на RP01 — НЕ ДОКАЗАНО**, причинный RUN и независимый CODEX/Astra 6 UltraCode review не выполнены. Read snapshot: `llm_mvp_core@dfd13067ecebdf7b3c3da5000ddbe8604b94ce27`; исторический архив 2026-05-01. Это новая адресная находка поверх [существующего каталога](README.md), не ещё одна версия общего «не хватает DPO».

## Короткий вывод

В эксперименте E39 улучшилась **F1 выбора набора проверок** (0.7801 → 0.9378, +20.22%), но его же контракт требовал **recall >= 0.95**. Фактический recall в сохранённом результате **0.911**; тем не менее сериализованы `passed:true`, `should_scale_up:true` и `next_action:PROMOTE_TO_NEXT_TIER`. Общий `ExperimentV2Result.Passed` проверяет лишь `ImprovementPct >= 5`, а `NextAction` отдаёт этому условию приоритет перед `ShouldScaleUp`. Поэтому такие исторические строки нельзя считать успешно прошедшим экспериментом по всем объявленным критериям.

В `15.OrchestratorModel.QualityOrchestrator` объявлены массивы обязательных и типовых проверок, но `Check` реально оценивает только минимальную длину текста и наличие substring `return` для `CODE_SERVICE`. Это другой дефект: **измерить, какие проверки выбраны** != **выполнить сами проверки**. `OrchestratorFacade.ProcessAsync` вызывает интерфейс качества перед `Store`; доказательство того, что именно этот concrete class был вызван в конкретном RP01 Student examination, ОТСУТСТВУЕТ.

## Первичные доказательства и границы

| ID | Наблюдение | Прямой источник | Статус |
|---|---|---|---|
| N26-E39-01 | Внутри E39 норматив `recall>=.95`, а `ShouldScaleUp = n<15 || enhRecall<.95` | [E39QualityOrchestrator.cs L7–13, L43–78](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E39QualityOrchestrator.cs#L7-L78) | SOURCE |
| N26-E39-02 | Архив `enhF1=.9378`, `scaling_signal=.911`, `passed=true`, `should_scale_up=true`, `next_action=PROMOTE_TO_NEXT_TIER` | [run-2026-05-01-001.json L1–18](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/results/E39-QUALITY-ORCHESTRATOR/results/run-2026-05-01-001.json#L1-L18) | ARCHIVE |
| N26-E39-03 | `Passed` определяется **только** primary improvement ≥ 5%; `NextAction` проверяет его раньше `ShouldScaleUp` | [ResultTypes.cs L50–69](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Core/ResultTypes.cs#L50-L69) | SOURCE |
| N26-E39-04 | Runner при `ShouldScaleUp` идёт на следующий tier, но одноразовый вывод и writer печатают статус `PASSED`, а archive уже промаркирован `passed` | [ExperimentRunner.cs L40–52, L82–101](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/ExperimentRunner.cs#L40-L101), [ResultWriter.cs L53–61](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Infrastructure/ResultWriter.cs#L53-L61) | SOURCE |
| N26-E39-05 | Concrete production `QualityOrchestrator.Check` не вызывает свои `AlwaysChecks` / `ByOutputType`, а содержит только два условий поверх строки | [QualityOrchestrator.cs L10–51](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/15.OrchestratorModel/QualityOrchestrator.cs#L10-L51) | SOURCE |
| N26-E39-06 | Шаг quality присутствует в facade; выбор реализации интерфейса и реальная трасса исполнения отдельно не доказаны | [OrchestratorFacade.cs L101–117](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/15.OrchestratorModel/OrchestratorFacade.cs#L101-L117) | SOURCE + BOUNDARY |

## Почему это может снижать качество

1. Историческая матрица могла продвигать feature по суммарной F1, хотя критическая полнота запретительных проверок ниже ограничения. **Это подтверждённый дефект интерпретации E39; масштаб влияния на другие строки матрицы пока не подсчитан.**
2. Runtime может возвращать `Passed=true` для содержательно неверного кода/текста, если он длиннее 10 символов и содержит `return` в случае `CODE_SERVICE`. Отсюда риск непроверенного feedback/admission **только если** этот concrete checker участвует в соответствующем пути.
3. E39 первично проверяет **правильность выбора перечня проверок**, не качество их исполнения. Нельзя переносить +20.22% на semantic output quality и тем более на RP01 DPO Student.

**Отдельная новая гипотеза, НЕ доказанное RP01-следствие:** [OrchestratorFacade.cs L83–100](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/15.OrchestratorModel/OrchestratorFacade.cs#L83-L100) передаёт следующему level только `allResults.Last().Result`, а не совокупность всех параллельных результатов. Это может терять контекст multi-branch task; проверять отдельным направленным counterexample, не смешивать с E39.

## Быстрая проверка, без тренировки модели

**E39-GATE-01, 2 arms:** A — архивный `Passed`; B — `Passed && !ShouldScaleUp && Recall>=.95` при сохранённых E39 числах. Фиксировать статус `PASS_PRIMARY_ONLY` vs `BLOCKED_RECALL`; ни один E39 исторический результат не переписывать. Negative control: искусственный результат recall=.97 при той же F1, должен проходить gate B. Источник значения recall — поле `scaling_signal` именно для E39 (не универсально для остальных экспериментов). Затем отдельно разметить E39 `testCases` по фактически выполненным check IDs и продемонстрировать `required-but-not-executed`.

**Acceptance:** на сохранённом E39 B не должен выдавать full PASS. В случае `recall=.97` B разрешает PASS. На `OrchestratorModel.QualityOrchestrator` не выдавать «16 checks executed», если не представлены результаты 16 evaluator'ов. Никакого вывода о Student gain.

## Роли и границы

- Внешняя открытая китайская модель — **создаёт** chosen/rejected DPO по выданному контракту. CODEX с **Astra 6 UltraCode** — **выбирает** человечески-значимый ответ и независимо **оценивает качество DPO** (обязательны VALID/TIE/BOTH_BAD/UNJUDGEABLE), затем оценивает качество обученного Student отдельно. Production `QualityOrchestrator` не заменяет CODEX judge.
- Этот файл — **DRAFT_REQUIRES_INDEPENDENT_CODEX_ASTRA6_ULTRACODE_REVIEW**; не было новых training runs, consumer calls, code fixes, branch merges или model review.
- **Запрещённый для записи каталог:** `llm_mvp_core/docs/plans/replanning-possible-diffs`. Он использован только как контекст; в нём ничего не меняется.
