# N26-B — E32: 90 формулировок 10 задач ≠ 90 независимых задач; KNN заменён константой

**Дата:** 2026-10-16. **Evidence snapshot:** `llm_mvp_core@dfd13067ecebdf7b3c3da5000ddbe8604b94ce27`. **Статус:** SOURCE_CONFIRMED (расхождение алгоритмов и отсутствие group holdout); CAUSAL_IMPACT_UNMEASURED (RP01/обычный consumer не запускались). Новая конкретизация ранее известных retrieval-versus-student отличий, не основание переписывать исходные positive результаты.

## Найдено

1. Исторический E32 тестировал **KNN на FamilyCollapser tags** для предсказания `SubEpisodeCount`: из 90 записей `EpisodeCorpusLoader.Expand` (10 исходных эпизодов × 9 переформулировок) делал random **row** split 72/18, затем при нехватке типов переключался на `StratifiedSplit` **по Type**, не по идентификатору оригинального эпизода. **Ни один из путей не требует group-disjoint split по 10 исходным эпизодам.** Если в val есть пять разных типов при 18 строках, часть исходных эпизодов неизбежно присутствует в обоих наборах — все девять формулировок одного эпизода не могут целиком лежать в val для пяти типов. Это contamination по задаче/семейству, не обязательная дословная утечка.
2. Сохранённый +11.11% E32 — `count_accuracy 0.5000→0.5556`, то есть **10/18 вместо 9/18 правильных ответов** (один дополнительный пример). `±1_accuracy=0.9444` является вторичной tolerance-метрикой и не подтверждает устойчивый перенос на новую исходную задачу. Следует различать источник E32 и positive run другого алгоритма.
3. В `15.OrchestratorModel.EpisodeDecomposer` нет KNN, обучающего корпуса, параметров или `FamilyCollapser`. Используется `ExpectedSubEpisodes` по `TaskDomain` и `SplitByConjunctions`. Кроме того, экспериментальные `Type` = `BUILD/DESIGN/FIX/MIGRATE/RESEARCH`, runtime `TaskDomain` = `PLAN/ARCH/REFACTOR/BUG/CODE_Q`; даже метки прямого контракта различны.
4. Разрыв существовал как минимум в соседних ветках `DesignToCode@8f20ceb8...` и `language_translate@f6032aff...` — там также найден KNN E32 + фиксированная production-таблица. Поэтому это **унаследованный transfer gap**, НЕ установленная регрессия, внесённая RP01. RP01 Student/DPO — вообще отдельный path.

## Source → mechanism → граница утверждения

| Claim | Доказательство | Что именно следует |
|---|---|---|
| N26-E32-01 10 групп × 9 | [EpisodeCorpusLoader.cs L9–27, L180–194](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Infrastructure/EpisodeCorpusLoader.cs#L9-L194) | Отношение paraphrase → canonical episode определено в `LoadAll()`; `Expand()` **теряет GroupId** |
| N26-E32-02 Row/Type split | [E32EpisodeDecomposer.cs L15–44](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E32EpisodeDecomposer.cs#L15-L44) | Нет source episode/identity group isolation |
| N26-E32-03 KNN contract | [E32EpisodeDecomposer.cs L58–75](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/Experiments/E32EpisodeDecomposer.cs#L58-L75) | Теги FamilyCollapser, Jaccard distance, top-3 vote |
| N26-E32-04 Positive, small n | [run-2026-05-01-001.json L1–18](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/35.ExperimentsV2/results/E32-EPISODE-DECOMPOSER/results/run-2026-05-01-001.json#L1-L18) | 0.50→0.5556; historical only; 18 val, 10 canonical families |
| N26-E32-05 Current hardcoded | [EpisodeDecomposer.cs L10–66](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/15.OrchestratorModel/EpisodeDecomposer.cs#L10-L66) | `ExpectedSubEpisodes` и conjunctions, **не** KNN |
| N26-E32-06 Реальный вызов interface | [OrchestratorFacade.cs L53–64](https://github.com/lubochka/llm_mvp_core/blob/dfd13067ecebdf7b3c3da5000ddbe8604b94ce27/15.OrchestratorModel/OrchestratorFacade.cs#L53-L64) | Facade использует `_decomposer`; выбор конкретного DI binding требует дополнительной трассы |
| N26-E32-07 Sibling parity | [DesignToCode E32](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E32EpisodeDecomposer.cs#L14-L74) и [production decomposer](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/15.OrchestratorModel/EpisodeDecomposer.cs#L10-L66) | Разные механизмы и до RP01; нет права датировать разрыв сегодняшней веткой |

## Причинная гипотеза и опровержение

**H32-LEAK:** row split оценивает распознавание варианта уже виденного canonical episode вместо переноса на новую задачу. **Опровержение:** при group-disjoint контрасте KNN сохраняет или увеличивает advantage над медианой с сопоставимым числом TRAIN (72) и корректной оценкой 10 независимых групп.

**H32-TRANSFER:** фиксированное число по домену не реализует условное retrieval-предсказание `SubEpisodeCount`. **Опровержение:** для всех оценочных задач production rule+conjunctions даёт тот же count и корректность, что исходная KNN функция, и есть трасса соответствующего consumer, объясняющая эквивалентность. Пока нет — это только разность механизма, не регрессия измеренного качества.

**Важная тонкость:** production `EpisodeDecomposer` реально возвращает **число получившихся строковых частей, ограниченное сверху expected count**, а не ожидаемое число из таблицы. Поэтому неправильно сравнивать только числа из `ExpectedSubEpisodes` с ярлыком `SubEpisodeCount`; при обычном prompt без `and/then` он вернёт один `SubTask`.

## Прямо реализуемая проверка

См. [N26-MICRO-E32-GROUP-DISJOINT-2026-10-16.md](26-MICRO-E32-GROUP-DISJOINT-2026-10-16.md): A historical random-row split, B grouped same-size split; одинаковые data, `FamilyCollapser`, K=3 и scoring; плюс отдельный shadow-call production consumer. Решающий результат — разность `source-group heldout accuracy`, не общий pass компиляции. Не обновлять frozen E32 и RP01 archives.

## Вывод для проекта

Эпизодная функция — **другой learning recipient**, не LoRA DPO Student. Даже если восстановить KNN для `EpisodeDecomposer`, улучшение RP01 free C#/ENRU generation из этого автоматически не следует. Нужен paired ordinary-consumer trace и независимое заключение CODEX/Astra 6 UltraCode. Роли DPO в проекте не подменяются: внешняя открытая китайская модель **генерирует DPO**, а **CODEX на Astra 6 UltraCode выбирает и оценивает качество пары**. Реальная обученная policy оценивается отдельно.

**Review state:** DRAFT_REQUIRES_INDEPENDENT_CODEX_ASTRA6_ULTRACODE_REVIEW. Запрещённый core каталог `docs/plans/replanning-possible-diffs` не менялся.
