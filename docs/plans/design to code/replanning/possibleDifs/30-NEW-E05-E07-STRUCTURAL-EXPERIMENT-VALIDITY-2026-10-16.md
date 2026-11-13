# N30-A/B/C — почему нельзя переносить выводы E05/E07 как доказанное качество структурного обучения

**16.10.2026 · независимая повторная source-проверка.** Core snapshot: `codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8`; planning: `llm_plan3`. **SOURCE-CONFIRMED CONTROL/FEATURE ERRORS**, causal impact on native heldout **NOT_RUN**, RP01 neural failure **NOT_ATTRIBUTED**. Этот файл добавляет новые узкие механизмы к существующим [общей карте](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [историческим оценкам](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [архитектурной карте](28-DELTA-MASTER-AUDIT-ALL-AXES-HISTORY-AND-NOVEL-CAUSAL-GATES-2026-10-16.md). В прочитанных ключевых сводках эти точные E05/E07 механизмы не найдены; **полная дедупликация всех 203 файлов не утверждается**.

## В чём риск — объяснение для архитектуры

В RP01 ухудшение свободной генерации действительно наблюдалось, но ошибка — объявлять все прежние матричные идеи «провалившимися» либо все старые PASS доказанной переносимой способностью. Есть минимум два различных случая:

1. В **E05** правило допустимого следующего синтаксического элемента подсматривало в проверочный текст; выросшая точность могла измерять знание test sequence, а не способность выучить ограничение.
2. В **E07** идея контекстного AST-признака могла получать неверную привязку BPE к узлу дерева. Дополнительно обещанные более богатые признаки вообще не попадали в ключ модели.

Считать E05 честной верхней оценкой или E07 честным отрицанием AST-context **нельзя без causal parity rerun**. Отдельный факт: ранние KNN структурные идеи были реально перенесены в `12.SemanticCompiler/Prediction/StructuralKnn.cs`, но эта история уже разобрана в [E09T](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md); новые N30 механизмы от E09T отличаются.

## N30-A — E05: grammar mask читает holdout

**SOURCE evidence:**
- [E05SyntaxFilter.cs:20–33](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E05SyntaxFilter.cs#L20-L33): `BuildVocabulary([csharpSource])` на полном корпусе, затем split 80/20 по AST token position.
- [E05SyntaxFilter.cs:35–75](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E05SyntaxFilter.cs#L35-L75): trigram baseline обучается на `trainTokens`; `grammarSources = allSources ?? [csharpSource]`, затем `grammarFilter.Train(trainTokens,grammarSources,...)`; enhanced измеряется на `validationTokens`.
- [GrammarMaskFilter.cs:18–48](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L18-L48) и [90–130](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L90-L130): `BuildGrammarTable(allSources,kindToIdx)` сохраняет **наблюдаемые тройки** `(prev2,prev1)→next` из всех источников, включая holdout при default.
- [E05 сохранённый run 01.05](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E05-SYNTAX-FILTER/results/run-2026-05-01-001.json): baseline `0.5079`, enhanced `0.6597`, +29.9% **relative**, `passed=true`.

**Доказано по коду:** часть будущих ответов попадает в грамматическую таблицу ещё до проверки; контроль и enhanced получают различную видимость holdout. **Не доказано:** сколько именно ошибок исправлено утечкой, и станет ли честный E05 хуже baseline; настоящий C# AST может по законам языка обобщаться и с train-only грамматикой.

**Мини-опыт E05-LK (1–10 настоящих C# исходников, однофакторный):** заморозить tokenizer, единый `kindToIdx` от train-only (или явно измерить OOV), модель trigram, split, порядок, seed, `prev2,prev1` и gold labels. A: `grammarSources=[sourceWithHoldout]` (архивный путь); B: `grammarSources=[trainOnlySource]` (новая честная таблица, syntax span должен соответствовать исходному коду). Мерить `holdout_only_triples`, `mask_contains_gold`, `top1_accuracy`, `mask_coverage`, `false_allowed/false_blocked` на **одних и тех же позициях**, с отчётом empty-mask fallback. Сделать также вторую, более строгую проверку file-/project-disjoint без смешивания с первой. **Фальсификатор влияния:** если качество и решения совпадают на повторном наборе, leak пока не дал measured gain (но сам breach контракта остаётся). Без одинаковой панели не публиковать gain.

## N30-B — E07: символный индекс принят за индекс BPE

**SOURCE evidence:**
- [E07StructuralContextToken.cs:39–52](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L39-L52): BPE `Train(csharpSource,512)`, `Encode(csharpSource)`, split AFTER tokenization. Это отдельный риск fit on full holdout distribution (не утверждается leakage of labels).
- [E07StructuralContextToken.cs:130–240](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L130-L240): `BuildStructuralTokens` берёт `posMap[i]` для `i`-го BPE token, но `BuildPositionAstMap` записывает `posMap[s]` по `charPos=(int)(s*step)`, `slots=min(textLen,50000)`. Индексы принадлежат **разным пространствам**; при multi-character merge привязка сдвигается, а fallback/default может маскировать дефект. `new AstMeta(...,0,scope)` жёстко ставит FileId=0.
- [E07 archived T1](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-001.json): baseline 0.1531 vs enhanced 0.1462. [T2](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json): 0.1502 vs 0.1310. Архивное FAIL **не опровергает** вариант с корректным token-to-AST alignment.

**Мини-опыт E07-ALIGN:** на 4–8 C# source files и замороженном BPE artifact построить реальный `tokenID→byteSpan→UTF16Span` (учесть Unicode, special handling для byte-level merges) и AST `SyntaxNode.Span`, получить `AstMeta` по действительному диапазону токена, не путем `posMap[tokenIndex]`. Измерить `alignment_mismatch_rate` между legacy и corrected, `default_meta_rate`, `cross_scope_errors`, затем сравнить frozen train/heldout `top1` при **одном** изменении метаданных; с одинаковыми fallback. Для file-disjoint проверки нельзя получать gold next из теста до infer. **Фальсификатор:** mismatch низок/качество не меняется → `FEATURE_ALIAS_ONLY`. Не смешивать с изменением гиперпараметров LoRA.

## N30-C — E07: заявленные tier2–tier5 измеряют не те признаки

В [E07:13–29](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L13-L29) Tier 2=parent, 3=file/depth, 4=scope, 5=structure batching. Однако [Run:83–107](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L83-L107) вызывает один и тот же `mdNgram.Train(train)` и `Predict(bpe,AstNodeType,AstDepth)` независимо от tier. В [MultidimensionalNGram:267–325](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L267-L325) ключ только `(bpe,ast,depth)`; `AstParentType`, `FileId`, `ScopeDepth` не участвуют в нём. `tier switch` меняет текст `dimensionNote`, не обучаемую политику. Это source-confirmed absence of actual tier ablation; различия архивных T1/T2 могут происходить из данных/масштаба, а не «добавленного parent».

**Мини-опыт E07-TIER:** freeze один dataset+splits+BPE+alignment, прогнать текущий `Run` на всех tier (метки predictor/accuracy должны совпасть, если прочие входы те же); затем последовательно, отдельно, добавить `parent`, `scope`, `file` в train AND predict identity, хранить версия ключа и compare accuracy, latency, unseen-key-rate. **Ни одна из старых отрицательных метрик не экстраполируется на настоящие parent/scope features до этого опыта.**

## Scope и статус

- **C# AST/token prediction:** E05/E07 могут быть релевантны. **EN→RU, русский морфологический KNN, general DPO/LoRA:** не переносить автоматически.
- **SOURCE доказательства выше ≠ native A/B ≠ обучение Student.** Python fixture `30-historical-seams-microproxy.py` прилагается отдельно только как логическая демонстрация, не Roslyn; исходные historical JSON сохранены без изменения.
- Сначала тот же обычный consumer и active feature flag: даже хороший AST predictor, нигде не вызванный в RP01, не может объяснять его `0/3`.
- [Короткий проверочный план](30-MICRO-EXPERIMENT-HISTORICAL-QUALITY-RECHECK-2026-10-16.md) и [сквозная карта](30-LIVE-CROSSWALK-BACKLOG-AND-CODEX-WORK-ORDER-2026-10-16.md). Protected core `docs/plans/replanning-possible-diffs` **READ_ONLY**.
