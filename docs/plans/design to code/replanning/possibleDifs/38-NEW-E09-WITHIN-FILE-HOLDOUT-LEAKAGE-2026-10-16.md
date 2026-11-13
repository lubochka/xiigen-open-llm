# N38-B — Новая условная гипотеза о положительном E09: обучающие и проверочные AST-узлы могут принадлежать одному C# файлу

**16.10.2026 · CODE-VERIFIED SPLIT RULE / LEAKAGE OCCURRENCE NOT_MEASURED / SEMANTIC IMPACT NOT_RUN.** Не смешивать с N32-B (ложные переходы между **разными** файлами), N26 E09T (смена кодировки меток) или известным unequal validation denominator. Все сохранённые исторические отчёты E09 остаются неизменными.

## Причина, сформулированная человеческим языком

В E09 не выбирают целые файлы для обучения и проверки. Сначала все Roslyn AST-узлы из файлов складываются подряд, потом отсекаются первые 80% как train. Если граница попала **внутрь одного файла**, часть его AST окажется в train, часть — в validation. Для задачи «предсказывать следующий AST тип» соседние узлы одного метода разделяют контекст, повторяющиеся шаблоны и иногда значительный объём одинаковой структуры. Проверка тогда может оценивать узнавание уже виденного контекста, а не переносимое знание на новый файл.

Это не утверждение, что сохранённый E09 PASS был ложным: нужны количество узлов каждого исторического файла, положения split и независимая повторная оценка. **Если split попал точно между файлами, этот механизм не сработал.**

## Доказательства в исходниках и архиве

1. [E09StructuralKnn.BuildTokenVectors, L102–125](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L102-L125) последовательно перебирает файлы, для каждого делает CSharpSyntaxTree.ParseText → DescendantNodes и добавляет каждую запись в единый result, без file-level split.
2. [E09StructuralKnn.Run, L25–36](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L25-L36) вычисляет split=(int)(tokenVectors.Count*.8), train=Take(split), val=Skip(split). Это split **по узлам**, не GroupKFold/leave-one-file-out.
3. [E09.Run, L53–78](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L53-L78) обучает KNN на следующих node IDs и проверяет следующие IDs validation, используя структурный контекст. Если узлы одного файла присутствуют по обе стороны, независимость по исходному документу нарушена.
4. [Исторический Tier2 receipt](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) сообщает baseline .2413 и enhanced .3507, но **сравнивает разные длины val** — [это ранее установленная проблема](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md). Настоящая гипотеза дополнительно проверяет **file independence**, а не повторяет mismatch denominator.
5. [N32 E09 cross-file](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md) описывает фиктивные переходы fileA:last→fileB:first **внутри** train/val. Нынешняя находка другая: **один и тот же исходный файл** представлен в обеих выборках, даже если все межфайловые переходы удалить.

## Фальсифицируемое утверждение

**H38-B:** в использованном для Tier2 корпусе 80%-я точка попала в середину файла, и E09 показал завышенную либо нестабильную generalization-accuracy из-за близости контекста. **H0:** граница пришлась ровно между файлами либо после source-heldout сравнения итог не ухудшился; тогда проблема есть лишь теоретически, а не в этом receipt.

Проиллюстрируем split без утверждения о реальном корпусе: 3 файла по 4, 3 и 6 AST узлов ⇒ N=13, split=floor(10.4)=10; второй и третий разделены так, что первые 3 узла третьего файла попали в train, последние 3 — в val. Из 6 узлов третьего файла **обе части присутствуют** в эксперименте. Арифметика объясняет условие, но сама по себе не измеряет качество.

**Ограничение:** для N файлов, прочитанных последовательным циклом, straddle обычно может затронуть лишь файл, в который попал порог; это не автоматическая утечка всех тестов. Даже при straddle нельзя утверждать неверную метку: AST типы остаются настоящими. Это потенциально оптимистичная оценка переносимости, а не прямая порча gradient LoRA.

## MICRO-38-B / C# structural KNN — дешёвая проверка за два запуска

**L0, только диагностика источника, без обучения:** взять сохранённый exact Tier2 input list и commit. На **тех же** Roslyn парсерах получить per-file node counts, cumulative boundaries, split index, для каждого node fileId, source path/commit, AST node path, kind, structuralVector SHA. Записать в отдельный receipt: node_count, split_offset, split_file, train_file_set, val_file_set, file_overlap_count, boundary-crossing pair count. Если original input list недоступен — NOT_RECONSTRUCTABLE, нельзя заменять произвольными файлами и объявлять исторический PASS опровергнутым.

**L1, paired eval on same case IDs:** сначала заморозить reference index file-disjoint validation набор из 8–16 C# файлов иных source families и внутрифайловые (не cross-file) node transitions. При том же label semantic mapping, k, StructuralVectorExtractor, memory budget и одинаковой actual test panel:
- A: обучить E09 KNN на исходном node-prefix train, сохранить train file IDs.
- B: обучить KNN на таком же объёме node transitions, выбранных целыми **TRAIN-only файлами**, без утечки test family; если exact match размеров невозможен, сообщить count difference и интерпретировать осторожно.
- C (негативный контроль): исходный train при fixed file-disjoint panel, но с удалением ложных межфайловых переходов **в обоих** A и B (или отдельной факторной колонкой, не приписывать эффект file split другому seam).
- Вместе сравнить исторический within-file eval и frozen unseen-file eval, **на одной и той же** маске valid neighbor targets и одинаковых метках. Primary — top1(next semantic SyntaxKind) per file (macro), critical wrong action/law on 8–12 independently reviewed code-to-action prompts **лишь если есть настоящий end-to-end recipient**. Отдельно неизвестные SyntaxKind, file-level coverage, count overlap, label alias и ties.

**Ветка продукта:** [production AST predictor](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/12.SemanticCompiler/Prediction/StructuralKnn.cs#L106-L170) — другой контракт обучения/меток, поэтому провести distinct consumer trace; модель RP01 random B0 + LoRA не обязана вызывать E09. Ни сохранённая цифра Tier2, ни её исправленная оценка не доказывают причинность RP01 free 0/3.

**Область:** C# / DesignToCode AST, условно иные языки с правильным синтаксическим деревом. **Не** переносить этот риск на EN→RU, HE, семантический Unit/KNN без явной аналогичной document-family границы. Никаких GitHub Windows runners для L0; короткий локальный Roslyn console и single file JSON receipt достаточны.

## Новизна и статус

Сверено с [N32](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md), [E09 transfer](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md), [N35 sibling distance](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md), [matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md). **Новая проверяемая ось** — происхождение train/heldout по **одному и тому же файлу при node-level split**; не выдаём её за подтверждённый ущерб. Никаких нативных/семантических результатов для N38-B ещё нет.
