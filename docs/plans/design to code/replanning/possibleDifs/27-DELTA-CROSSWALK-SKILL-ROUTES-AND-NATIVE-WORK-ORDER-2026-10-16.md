# 27 — Общий маршрут аудита RP01 + новую N27-гипотезу проверять не тестами, а результатом обучения

**Задача:** не переписать сотню аудитов заново, а соединить успешные исторические функции, RP01 Student, структурные Unit/KNN, свежую N27 и один дешёвый истинно причинный опыт. **Версия:** 2026-10-16. **Статус:** document/Code-source analysis and JS proxy; не состоялся native .NET run, независимый CODEX/Astra review, обучение Student или измерение прироста качества.

## Как объяснять ситуацию без ложных обобщений

Исторические успешные «обучения» относились к разным механизмам: кто-то получал качество благодаря **памяти примеров и KNN**, кто-то — **локальным переведённым шаблонам**, кто-то — **контекстному словарю или replay**. В RP01 проверяли иное: изменение LoRA весов маленького случайного Student. Доказательство изменения весов **не является доказательством**, что Student научился свободно отвечать. И наоборот: существование 0/4 free answers не обнуляет успешные старые KNN эксперименты. N27 добавляет ещё одну независимую возможную потерю: новый правильный урок Unit способен породить несуществующий неверный урок, если неконтролируемая морфология подменяет грамматическую роль.

Схема фактических *раздельных* получателей:

~~~mermaid
flowchart TD
    A[Одобренный источник пары] --> B{Какой обучаемый получатель?}
    B -->|RP01 Student| C[Exact DPO segmentation]
    C --> D[q/v LoRA + frozen base]
    D --> E[Student checkpoint]
    E --> F[TextGenerator ordinary text]
    B -->|Translation Units| G[BuildBatch + UnitId + role/word marker]
    G --> H[SemanticTokenizer / ExecutorUnit]
    H --> I[Shared KNN + word/phrase/exact maps]
    I --> J[SaveCheckpoint / LoadCheckpoint]
    J --> K[TranslationRoutingFlow ordinary request]
    G -. N27 role not projected .-> L[Потеря признака POS]
    H -. N27 suffix derivation .-> M[Ложный alternate mapping]
    G -. N27 possible pair tie .-> N[Необоснованный rejected]
    F --> Q[Independent blind semantic gold]
    K --> Q
~~~

Это *архитектурная карта source call graphs*, не утверждение, будто между C и G сейчас существует DPO→Unit bridge.

## Consolidation без повторного присвоения находок

| Тип расхождения | Уже найденное, прямые ссылки | Открытый причинный вопрос |
| --- | --- | --- |
| Случайный base, мало шагов, Lora не создаёт знания из пустоты | [RP01 report](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md), [DIF 01–44](01-PRIORITIZED-DIFFERENCES.md) | Есть ли качественная warm-start SFT база? Обобщает ли после 8–32 малых примеров? |
| Pair quality, токенизация/маска/длина/gradient | [02-DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [16 pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) | Различаются ли chosen/rejected **по model-visible ответу** и корректна ли производная? |
| LoRA targets, rank, dropout и save/load | [03 LoRA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [RP01 pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md) | Изменяет ли LoRA реальный next-token ranking, а не только hash? |
| Детерминистический decoder и неверная stop/parsing семантика | [10 inference](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Потеря доказанных навыков на inference вместо training? |
| KNN содержит правильный урок, но первым выбирает старый/не тот | [16 FIFO, domain, direction](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md), [generic KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | KNN лучшим образом переносит исходные признаки и сохранённый порядок предпочтений? |
| Часть Unit обучается фиктивно или не выбирается | [17 silent dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), [N21 source admission](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md) | Есть ли actual trainer invocation и expected UnitId после admission? |
| Verifier переобучается противоречащим законам | [23 verifier](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) | Может ли новая запись отклонить старый правильный перевод? |
| Язык, POS, rule tags и предметная семантика меняют lookup identity | [23 language](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md), [26 language tags](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md), [E26 POS](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) | Одинаковы ли learned-train и ordinary infer query keys? |
| Попытка восстановить модель восстанавливает только metadata | [TranslationMachine Export/Import vs checkpoint](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md), [RP01 pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md) | Все ли persisted Unit mappings, KNN и optimizer state действительно восстанавливаются? |
| Ошибочная quality метрика и leakage в старых успехах | [E51A/B](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md), [E09/E14 and matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [historical successes](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) | Какие узкие успешные функции повторяют преимущество на одинаковой новой source-family-disjoint панели? |
| **N27 уникальное дополнение**: грамматическая роль выброшена, suffix creates false knowledge, quote marker/negative not validated | [N27 source evidence](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md) | Достигает ли poisoned word map настоящего Flow и ухудшает ли *retention* старой correct verb после нового noun урока? |

**Не считать отсутствие точного исторического donor commit доказательством, что соответствующего переноса не было.** [E07 matrix vs E09 structural KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) — пример, где ранний отрицательный эксперимент имел неверно сопоставленные структурные coordinates, а KNN с Roslyn AST показывал положительный сохранённый signal, но validation subsets несопоставимы. Найти source-level донорскую связь (commit→caller→same-panel) — отдельная открытая задача.

## Конкретный work order на один маленький вызов Kiro/CODEX

**CURRENT TASK:** Source-verify новый N27 и реализовать **ТОЛЬКО** MICRO-N27-L0, затем, если найден фактический train→consumer path, парный L1. **NOT THE TASK:** чинить все 44 DIF, строить 500 новых DPO, подбирать постфактум gold, запускать Windows GH workflow, менять чужой core docs каталог. **OWNED WRITES:** только этот planning/possibleDifs и изолированные локальные scratch / receipts; в [core docs/plans/replanning-possible-diffs](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs) не писать ни при каких обстоятельствах.

1. **Skills / architect**: реально открыть и прочесть [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md), [code-examination](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md), [learning-path-audit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/learning-path-audit-SKILL.md). Сохранить skill_load_status found/read/missing. Путь от source до ordinary call объяснить человеческими словами и file→method→quote. Не принимать вывод по одному grep.
2. **Planning**: использовать [plan-restructure](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-restructure-SKILL.md) для минимальной дельты к существующему RP01 recovery, с явно неизменными шагами. [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md) — freeze panel, isolated factor, stratified word roles, non-trivial baseline, controls и одна метрика **independent semantic final-answer correctness**.
3. **Implementation/QA**: сверить пин C# и historical DesignToCode, выполнить только [native L0 and L1](27-MICRO-ROLE-GENDER-RETENTION-CAUSAL-EXPERIMENT.md). Если точечного хоста нет, оформить minimal code snippet/fixture как NOT_RUN; не писать фиктивный PASS. Регистрировать WordMap before/after, exact/phrase/KNN bypass conditions, final ordinary result, save/load.
4. **Independent review**: применить [plan-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [claim-verification](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/claim-verification-SKILL.md). Не повышать verdict с CODE_CONFIRMED на LEARNING_GAIN без observed blind delta. Никакое утверждение «проверено CODEX/Astra» не писать без реального независимого вызова.
5. **Prompt/human communication**: [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md) — patch только **одного bottleneck**, с WRONG/CORRECT и не ухудшать уже рабочие RU verb/gender cases; [conversation-intent-and-self-reflection](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/conversation-intent-and-self-reflection-SKILL.md) — итог на русском: «что в коде доказано / что показала реальная система / что остаётся гипотезой». Не отвлекаться на рестарт всей архитектуры.

## Предписанный шаблон результата

- **READ_SCOPE:** exact GitHub commit SHA core/planning + files/line ranges; skills status; touched read/write paths.
- **TRAIN_SIGNAL:** frozen cases, teacher/author/source provenance, chosen/rejected semantic evidence, actual recipient, no hallucinated optimizer calls.
- **STATE_EFFECT:** N27 before/after maps, direct Unit and after cold restore; first selected route versus fallback.
- **QUALITY_DELTA:** blind gold and source-family split; per-case A/B answers/verdict, wrong accepted, abstains, retention; teacher/adjudicator separate from Student.
- **VERDICT:** SOURCE_BUG_CONFIRMED / CONSUMER_REACHABLE / CAUSAL_QUALITY_GAIN_MEASURED / INCONCLUSIVE / HYPOTHESIS_REFUTED, **не** одно общее PASS. Для всякого INCONCLUSIVE — одна конкретная следующая короткая проверка.
- **HANDOFF:** короткая русская сводка плюс ссылки на actual committed evidence, без приписывания невыполненного.

**Правило безопасности вывода:** выполнение Node proxy [code](27-ROLE-GENDER-QUOTE-MICRO-PROXY.mjs) и [raw output](27-ROLE-GENDER-QUOTE-MICRO-PROXY-RESULT.json) показывает *формулу*, а не настоящую .NET загрузку, обучение, semantic quality или связь с RP01 main500.
