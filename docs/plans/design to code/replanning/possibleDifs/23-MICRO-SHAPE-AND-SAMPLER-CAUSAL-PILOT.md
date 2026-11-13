# 23 MICRO — причинная проверка невидимого обучения в KNN и расхождений decoder

**2026-10-16 | EXECUTION WORK ORDER | ANALYTICAL PROXY RUN | NATIVE .NET NOT RUN | INDEPENDENT CODEX/Astra REVIEW NOT RUN.**

Родитель: [23-source-audit](23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md). Не заменяет приоритетный эксперимент [RP01 cold-start versus SFT/DPO](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) и [EXP01 Unit memory vs isolated Student](11-EXP01-UNIT-MEMORY-IMPACT.md). Цель узкой N23-A: проверить, может ли корректирующий ранее обученный KNN-урок исчезать при mismatched query dimension, **при неизменной памяти и весах**, а не предположить, что всё сломано из-за LoRA.

## Гипотеза и строгая граница

[Distances.CosineSimilarity](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/06.VectorSearch/Distances.cs#L10-L21) без проверки a.Length==b.Length использует только префикс stored vector, если query короче. Т.о. confidence может быть ложной. **Conditional**: один известный KNN owner всегда передаёт одинаковый _vectorDimensions. Пока не найдено подтверждения, что реальные RP01 вызовы нарушают размерность. Если такого caller нет, N23-A закрывается как объяснение RP01, даже если API уязвим.

## Gate 0: есть ли реальный путь?

В pinned core@0eccbdc3c4efb90adf1d699806cbfa9f5610998d проследить вызовы HnswIndex.Add/Search/Load и BruteForceIndex.Add/Search из конкретных Unit/consumer и соседних веток. Для каждой пары build/query залогировать: producer, train/query vector length, encoder/tokenizer/LoRA fingerprint, domain, index identifier, saved/reopened state, lesson output actually consumed. Если все реальные query/index размеры корректны и schema совпадает: verdict **FALSIFIED_FOR_RP01** для N23-A; не начинать бессмысленные 500-pair training reruns.

## Gate 1: маленький воспроизводимый native C# test

Создать вне production один BruteForceIndex с **двумя** TRAIN-мечеными уроками в порядке:
- old_wrong: vector [1,100], output WRONG
- learned_correct: vector [0.8,0], output CORRECT.

Оценить тот же фиксированный закон query_full=[1,0] и malformed query_short=[1]. Предсказание по точной формуле: query_full возвращает learned_correct с cos=1; old_wrong имеет ~0.0099995. Query_short обеим даёт cos=1, stable LINQ OrderByDescending выбирает old_wrong первым. Тот же batch при shape=2 ещё раз после нового сохранения индекса не должен измениться. Записать все соседние IDs и float scores. Отдельно вызвать strict shape guard: должен явно **отказать**, а не выдать RIGHT; abstain != learned correctness.

Для **проверки влияния на качество обучения** надо не ограничиваться этой игрушкой. Сформировать 8 учебных примеров из четырёх независимых законов, и 12 независимых heldout inputs (3/law), disjoint по source-family и исходным ID. Gold фиксирует независимый CODEX/Astra evaluator ДО построения index. Не допускать law label, gold, dataset split или synthetic signal в query features.

Четыре paired arms при одних trained examples, exact hidden questions, том же consumer и замороженных decoder/settings:
- A: current actual query encoder → current learned index
- B: re-encode **тот же** query из того же исходного текста, единственным исправлением делая canonical same-dimensional + same encoder fingerprint
- C: A + strict shape/fingerprint guard, считающий отказ отдельно
- D: B после save/reopen **того же** index.

Вывод per-case: actual source IDs, train/query vector shape/hash, retrieved neighbor IDs, learned output used, output text, gold-correct law, domain-correct, confidence, STOP/abstain, latency, exact commit/seed/data/checkpoint index hashes. Метрики: correct@1 из 12, wrong-high-confidence (≥0.9), abstain, wrong→right/right→wrong paired case transitions, total. Никакой «улучшенной accuracy» по одним train examples. На 12 кейсах это exploratory evidence, а не доказательство переносимого population gain.

**Фальсификация:** нет mismatch в реальном RP01 consumer / после shape fix paired right не растёт / изменения объясняются encoder-algorithm rewrite, метками или cached index, а не размерностью. При успехе (есть native cases wrong→right при равных остальных факторах) можно писать CONFIRMED_LOCAL_QUALITY_IMPACT, но не объявлять исправленной качество whole RP01 Student без отдельного Student-generation exam.

## Gate 2: декодер — read-only, не ре-тренировка

[Sampler](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/09.Inference/Sampler.cs#L19-L76) использует topK как альтернативу topP. На logits [5,4,0], temp1, topK2, topP.6 измерить base sampler и комбинированный topK∩topP в тестовой сборке; предсказание P(token1)=.26894 vs 0. **Это уже независимо замечено в [23 other agent 23.04](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), не новая находка.** На frozen model 12 source-disjoint prompts × 5 seeds сравнить semantic correctness vs exact same decoder constraints. Seeds не превращают 12 families в 60 независимых задач.

[BpeTokenizer.Decode](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/01.Tokenizer/BpeTokenizer.cs#L85-L92) silently drops ID missing from vocab; [TextGenerator](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/09.Inference/TextGenerator.cs#L173-L249) checks nextToken bounds only in detailed mode. Только тестовый несовместимый vocab/model fixture: принудительно выбрать ID, допустимый model, но отсутствующий в tokenizer; сравнить plain Generate and GenerateDetailed. Не менять реальные checkpoint. Если текущий RP01 вызов только greedy temperature0 topK0 topP1, с совместимыми vocab и detailed strict, оба decoder кандидата исключаются как объяснение результатов RP01.

## Выполненное и невыполненное

Исполнен source-equivalent Node toy proxy, см. [script](23-MICRO-SHAPE-SAMPLER-PROXY.mjs) и [receipt](23-MICRO-SHAPE-SAMPLER-PROXY-RESULT.json). Предсказанные/полученные при повторении формулы: correct full=true, correct short=false, false confidence 1, sampler second-token p~.26894 вместо 0. **Никакой native .NET training, actual RP01 blind law quality, Chinese-model teaching либо CODEX Astra 6 UltraCode review не выполнялись.**

## Исполнительская governance

Только CODEX с Astra 6 UltraCode определяет задачи и выполняет human selection. Внешняя лицензированная/открытая Chinese модель создаёт ОБЕ DPO стороны chosen/rejected. Другой CODEX/Astra 6 UltraCode независимо оценивает exact качество пар, фиксирует tie / unjudgeable / wrong chosen. Экзамен Student отделён. Нельзя выдавать offline deterministic RP01 simulation за эти роли.

Архитектор проверяет causal isolation; reviewer плана — leakage, confounds и критерий фальсификации; code review — diff/source identity, tensor/list shapes, gated refusal; prompt engineering — версия и текст exact teaching/evaluation framing; human communications — статус C/M/H/U понятным языком. Ограниченный dotnet invocation не должен ждать Windows CI. При отсутствии runnable DLL отметить NATIVE_BLOCKED и не подменять proxy реальным измерением обучения.
