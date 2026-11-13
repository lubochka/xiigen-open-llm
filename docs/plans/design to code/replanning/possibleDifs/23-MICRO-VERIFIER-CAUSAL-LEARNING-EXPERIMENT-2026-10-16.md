# EXP23-VERIFIER-LESSON-CONFLICT · Мини-эксперимент

**16.10.2026, DRAFT / NOT RUN NATIVE.** Источник: [N23-VERIFIER](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md). Цель — за один ограниченный запуск отделить алгоритм неверного накопления learned rules от реальной деградации качества. Без 500 DPO, Windows CI, большого покрытия.

## Паспорт и гипотезы

- ExperimentId: EXP23-VERIFIER-LESSON-CONFLICT; Tier 1: 2–4 train-only урока + 8–16 source-disjoint blind cases.
- Baseline A: верный урок A, fresh native QualityVerifierUnit, fixed candidate output.
- Enhanced/contrast B: тот же Unit, урок A + независимый правильный урок B с **той же реальной** SemanticSignature, но другим допустимым обязательным target term.
- Control C: A+B с **разными** Signature; другие условия не менять.
- Shadow D: та же A+B с test-only rule selection по source/domain/law (правила выбирают без знания sealed gold).
- Shadow E: одна общая QualityVerifier по направлениям против раздельных QualityVerifier для каждого target language; не смешивать с C/D.
- Primary metric: LAW_CORRECT_AND_ACCEPTED / N на тех же 8–16 frozen source-disjoint cases. Supporting: lawful false rejects, unlawful false accepts, critical violations, fallback rate, paired wins/losses, latency, state SHA.

H1: после нового законного урока B старый верный ответ A может стать FAIL, а scoped D исправляет именно этот отказ, не пропуская неправильные ответы. H2: без ключа Direction learned requirements могут мешать другой паре языков. Нулевая гипотеза: реальных конфликтующих сигнатур в панели нет; либо они не достигают consumer; либо shadow-rule не повышает semantic correctness. Даже positive H не доказывает root cause RP01 Student-only.

## Исполнимый минимальный порядок

1. Загрузить exact HEAD и исходники: [contracts](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationUnitContracts.cs), [tokenizer](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationSemanticTokenizer.cs), [Verifier Train/Verify](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L1040-L1145), [machine CreateRuntime](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationMachine.cs#L717-L788) и [adapter fallback](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L143).
2. Из реальных сохранённых structured translation examples найти 2 законных source с равной Signature через **живой TranslationSemanticTokenizer.Tokenize**, но разными legal terms. Записать exact source bytes, язык, domain, pair ID, signature, needed term и approved source semantics. Если таких нет — synthetic fixture пометить MECHANISM_ONLY, не объявлять measured user quality.
3. В одном коротком native harness выполнить обучение QualityVerifierUnit.TrainOnExample(A), проверить frozen lawful candidate в Verify; выполнить TrainOnExample(B), повторить **тот же** Verify для A/B, записать issue list и Pass, learned rule state, checkpoint. Сохранить A/B order reversed отдельно. Не трогать LoRA/KNN/generator.
4. Прогнать controls C/D/E на тех же candidate text и exact source signature; для actual consumer trace посмотреть TrainedTranslationAdapter: для обычного non-phrase результата FAIL может направить на Bootstrap, а phrase-map confidence≥0.85 имеет отдельное исключение. Не смешивать.
5. CODEX/Astra 6 UltraCode независимо фиксирует semantics rubric до просмотра результатов arms, оценивает одни и те же blind 8–16 ответов и возможные смысловые нарушения. Разделить true learning, rule guard, abstain и fallback. При 8–16 не утверждать статистическую значимость.
6. Только при реальном effect повторить SaveCheckpointAsync/LoadCheckpointAsync cold readback, сопоставить learned-state hash, output и ordinary consumer, не использовать metadata-only ExportAsync.

## Локальный proxy — не native и не quality experiment

В этом исследовании выполнена JS-реплика операции requiredTerms.Add плюс foreach candidate.Contains: после required=["thread"] строка "start thread" PASS; после required=["thread","process"] та же строка FAIL (missing process), строка "start process" FAIL (missing thread), "start thread process" PASS. **Это подтверждает формулу, но не качество реального перевода, не фактическое совпадение signature и не .NET run.**

## Предохранители, фальсификация, исполнитель

- Негативные контроли: повторить идентичный requirement; другой Signature; поменять порядок A/B; заморозить candidate; проверить отдельно substring-vs-whole-word (уже известная N21, здесь не основной фактор); негативный false-accept обязательно считать.
- Outcomes: MEASURED_QUALITY_GAIN только при native + blind + actual consumer; MECHANISM_ONLY при native flip без consumer/semantic эффекта; NO_CONFLICT_IN_PANEL при отсутствии реальных конфликтов; INVALID при смешении осей/данных.
- STOP без native same-signature conflict или без changes in law correctness: никаких больших DPO кампаний.

**Исполнительский prompt:** CURRENT TASK — один bounded native EXP23, с отчетом raw source→signature→learned rule state→candidate→verifier→adapter→independent law grade. NOT THE TASK — не менять RP01/results, не запускать Windows/GitHub full gates, не писать production fix до зафиксированного contrast. Required skills: architect-behavior-classifier, learning-path-audit, code-examination, human-readable-architecture-audit, phase-planning, plan-review, claim-verification, experiment-design, prompt-patch-authoring, conversation-intent-and-self-reflection. Allowed: test-only harness + new result artifacts. Forbidden: frozen C#500/ENRU500, hidden rubric edits, claimed DPO improvement without actual Student outputs. Good: paired A_PASS→B_FAIL→D_PASS lawful nonphrase with unchanged candidate, independent law grade and no false accepts. Bad: JavaScript proxy назван полноценным обучением, не подтверждены реальные signatures.

Return packet: source SHA, exact learning IDs and raw input, Signature/Direction/TopicDomain, rule state hash A/B, A/B/C/D/E table, native output and issues, paired wins/losses on N=8–16, critical errors, resource/time, cold consumer trace, limitations, decision. **Никакого самостоятельного reviewer PASS:** DRAFT до независимого judge. При новых DPO обе стороны создаются только разрешённой открытой китайской моделью; для этого механического эксперимента DPO не нужны.
