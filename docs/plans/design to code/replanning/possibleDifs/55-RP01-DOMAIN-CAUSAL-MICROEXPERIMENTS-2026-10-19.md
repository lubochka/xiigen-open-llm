# N55 — семь быстрых причинных экспериментов по темам обучения и реальным получателям

**19.10.2026 · экспериментальный протокол, native runs NOT_RUN в этой сессии.** Новые проверяемые hypotheses — [N55 evaluation-direction, policy drift, Unicode](55-NEW-SOURCE-SEAMS-EVALUATION-AND-POLICY-2026-10-19.md); восстановление исторического GOLD прежде всего — [Глава 0](00-01-PREPARETOGPL-GOLDEN-RESTORE-2026-10-19.md). Инструмент source-equivalent policy proxy уже можно запускать отдельно; его **не** считать обучением модели.

**Единый закон эксперимента:** одна гипотеза / одно изменение / один получатель / одинаковые данные и frozen scorer; минимум 1 положительный и 1 отрицательный контроль, before+after и fresh restore. Материализованные Unit examples не путать с независимыми уроками: из одной DPO-пары могут возникнуть 3 KNN signatures, но это **один** source law. Никаких GitHub Windows workflow/full CI ради этих семи проб. Учебные данные и gold не перезаписываются после просмотра ответа. Отчёт разделяет correct/total, accepted/total, incorrect-accepted/total, abstention, latency, memory, route, identities, target model state. **Успех синтетического proxy ≠ успех native .NET, и успех KNN memory ≠ успех LoRA.**

## EXP-GOLD-01 / Перевод / сохранить выученные знания из PrepareToGPL

- **Получатель:** TranslationMachine, 4 old direction Executors, shared KNN и phrase maps. **Consumer:** обычный TranslationRoutingFlow, не generic neural scorer.
- **Данные:** одна реально сохранённая старая checkpoint identity из PrepareToGPL; seed/hash из [gold forensic](00-PREPARETOGPL-RECOVERY-FIRST-2026-10-18.md), frozen 12 запросов (4 exact, 4 near/composition, 4 contrast: negation/tense/question/unknown).
- **Baseline A:** GOLD runtime + GOLD checkpoint. **B:** current loader + неизменённый checkpoint. **Enhanced C:** current runtime + *явная* миграция 4→6 (два новых пустых executors, четыре существующих без изменения).
- **Изолировать:** разрешить в staging лишь importer/migration; не обучать, не менять tokenizer/phrase/semantic checker. Если B отказывает по EXECUTOR_DIRECTIONS_INCOMPLETE — честный B FAIL_LOAD, не 0/12 «плохих ответов».
- **Метрики:** load success, exact retained entries/signatures, correct/12, false accepted, output/adapter identity на совпадающем subset, byte hashes except declared migration metadata.
- **Falsifier:** C загружается, но старые maps/Unit IDs изменены или правильные ответы потеряны; значит миграция не восстановила способность. **Минимальный native опыт**: 1 checkpoint × 12 requests × 3 branches, без DPO runs.
- **Ограничение:** доказательство переноса translation memory, не neural language capacity; формат GOLD vs current уже исследован другими агентами, здесь проверяется причинное качество.

## EXP-GOLD-02 / Перевод / не потерять обученную композицию частей

- **Получатель:** executor-en_ru (phrase/exact/KNN maps). **Consumer:** normal Flow.
- **Замороженное native свидетельство:** [P125](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md): 8 уроков → 19/24, 3 incorrect accepts. [Ingress A/B](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md): потерянные phrase_mappings → 11/24, с ними → 19/24. **Это уже работающий код, не новое изменение.**
- **Baseline A:** materialize same 8 source/chosen без phrase mappings (только учебный диагностический harness, не production mutation). **B:** те же 8 source/chosen с 16 phrase mappings как текущий working ingress. **C:** те же 8, но rejected/target переставлены как отрицательный контроль смысла.
- **Панель:** P125 24 идентичных вопроса с 8 exact, 3 near, 8 novel recombinations, 5 edge; сравнить без изменения gold или denominator. Output trace id+source lesson id where available; не выдумывать ID из количества троек.
- **Pass только в границах донора:** B ≥ A по correct, новые комбинации 8/8 на известном материале, no regression exact, incorrect accepted не растёт без явной регистрации. Эксперимент должен **показать** историческую цену false accepted и не скрывать 3 edge failures.
- **Domain guard:** синтаксис английских команд/русский target. Не применять эти phrase maps автоматически к SQL AST, где важны join/operator edge.

## EXP-N55-01 / Перевод / один learned EN→RU Unit, два пути оценки

- **Ресурс:** native P125 B_trained.json 8 lessons/16 phrases/24 shared signatures; обучать заново **не нужно**, можно использовать fresh process.
- **A:** напрямую TranslationRoutingFlow, languages (en,ru), topic general. **B:** generic TranslationMachine.EvaluateAsync на **тех же** 6 заранее выбранных EN→RU запросах. **C:** временный caller сравнивает те же gold и вызывает ordinary Flow с явно (en,ru), не изменяя core.
- **Предметные запросы:** “validate the file”, “delete the cache”, “move the report”, “delete the file”, “validate the cache”, “do not delete the file”; gold заморожен до вызова и повторяет заранее утверждённые данные P125. Последний пример **негативный**: нынешняя память принимает неправильный русский imperative.
- **Проверить:** Log actual source_language,target_language, adapter_id, output, confidence, true_correct и Checkpoint AccuracyAfterLastBatch; исключить случай когда generic EvaluateAsync вообще не вызывается production.
- **Первичная метрика:** число cases, где B реально пошёл RU→EN, A/C EN→RU, и разница semantic-correct /6. Никакое изменение state/weights в этом опыте не допускается. Установленное source различие — уже существовало и в GOLD; это **consumer-integration** гипотеза, а не Git regression.
- **Порог/фальсификатор:** если B использует EN_RU по факту — hypothesis false для данного invocation; если B RU_EN — contract bug confirmed, а влияние на конкретный production call лишь если его traced caller там же.

## EXP-N55-02 / Перевод / одинаковая память, другой исполняемый KNN policy

- **A:** GOLD policy (нет candidate-conditioned source-law); **B:** current policy en-simple-copular-polarity-v1. **Общее:** тот же immutable checkpoint, тот же input/target, одинаковая frozen tokenizer и signatures, один состав memory, никаких новых уроков.
- **Memory fixture:** положительный источник “the box is empty” → утверждение-перевод «коробка пуста»; контрастный learned источник “the box is not empty” → «коробка не пуста». Оба должны действительно попасть в тот же candidate bucket **или опыт помечается INAPPLICABLE**, а не fake PASS. Train fixture можно сгенерировать временно отдельно от PROD checkpoint.
- **Case panel (8):** оба exact, вариант с артиклем, отрицание перед сказуемым, subject box→door, was→is, question “is the box empty?”, negative “the box is not empty.”; frozen semantic gold — независимые переводы, а не string equality к любому из stored target.
- **Поля:** query, candidate source/target, policy verdict Contradiction/Compatible/Unknown, candidate rank before/after law, final answer/route, semantic correctness, incorrect accepted, abstention, model+policy fingerprint, loader result.
- **Score:** incorrect-accepted polarity inversions /8, correct /8, retention of good answers /8; дополнительный KPI — одинаковые training state/hash и разный route при changed policy. Если B уменьшит false accepts, это **quality of consumer guard**, а не learning gain.
- **Causal falsifier:** если различия output нет, policy может просто не активироваться на этом KNN-signature bucket; нельзя объявлять отсутствие эффекта для всех KNN; нужно trace reachability.

## EXP-N55-03 / Перевод / Unicode metamorphic к policy

- **A:** raw “the box is not empty”. **B:** тот же смысл с tab, NBSP U+00A0, narrow NBSP U+202F, thin U+2009 и double ASCII space между “is” и “not”. Candidate source memory: “the box is empty”.
- **Первый бесплатный слой:** source-equivalent regex probe в Python (логика буквально соответствует закрытым словам/регулярному выражению) — **выполнен**. Получено Contradiction для ASCII, TAB и DOUBLEASCII; Unknown для NBSP, NNBSP, THINSPACE. Это лишь proof of branchability на source regex, **не native .NET и не измерение учебного качества**.
- **Второй обязательный слой:** native internal Compare (возможен InternalsVisibleTo/малый локальный adapter с visibility без product edit); затем ordinary Flow с одинаковым checkpoint и выбранным bucket.
- **Frozen metrics:** semantic-correct /6, policy contradictions /6, incorrect accepted /6, output invariant under semantic-equivalent spacing. Отдельно проверять quoted “not” и true multiple clauses, чтобы не расширить правило без scope.
- **Falsifier:** caller нормализует whitespace до policy, тогда вне native direct Compare разницы на обычном consumer нет; это не обучающая регрессия.

## EXP-KNN-04 / Программирование и язык / матрица vs KNN, условные AST/POS признаки

- **Получатель:** StructuralKnn E09 для кода **отдельно** от PatternClassifier E25 для перевода. **Не смешивать домены в одной числовой accuracy.**
- **C# AST arm (8–16 cases):** базовый retrieval на текущих структурных features; enhanced с sibling/child adjacency distance; negative same syntax different next AST; expected next AST kind, source-disjoint file/group split. Показать top1 + false neighbors; не считать число одинаковых templates независимостью.
- **Sentence order arm (8–16 cases):** E25 POS-sequence KNN with/without order-aware features; train 2+ POS clauses и hard negatives (imperative vs question, VSO vs SVO); отдельно one-token edge где исторический bigram-Jaccard даёт empty/empty distance=0. **Historical E25 34.38%→87.5% top1** на 160 synthetic и 32-val доказывает scoped learner, но не ordinary TranslationExecutor.
- **A/B + carrier × feature:** 2×2 baseline/feature on matrix, baseline/feature on KNN, same seeds/gold; тогда прошлый matrix FAIL не маскирует полезный KNN feature. Для C# syntax-tree feature может помогать; для бытовой команды и SQL literal syntax не обязательно.
- **Pass:** source-disjoint correctness gain with no excessive false accepts; затем реальная подача в Unit/Flow. Только изменение index score без влияния product answer считается PROXY_ONLY.

## EXP-RP01-05 / C# документация и перевод / восстановить нейронную генерацию, а не смешать с памятью

- **Получатель:** native Student/TextGenerator с trainable B0+LoRA, а не translation KNN; отдельный ordinary generation consumer. **Historical**: [RP01 recovered diagnostics](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) — C#0/3→0/3, EN→RU0/4→0/4 despite weight updates.
- **Frozen tiny dataset:** 8–12 uniquely authored same-prompt chosen/rejected with contrastive semantic errors, independent heldout 8–12 source families; include 2 unchanged previous-domain retention tasks. No need to produce new 500.
- **Four arms:** A random B0 no training; B B0+DPO-only (current); C chosen-only **tiny supervised warmup** then DPO with same authorized trainer; D GOLD trained **Unit memory** on same domain-shaped questions, **separately labelled** MEMORY not neural. Keep new params only where code intentionally permits; LoRA rank/alpha etc matched.
- **Must instrument:** tokenizer ID/masks, response-only logp, ref policy, LoRA q/v and embedding/bias gradient, 1–4 optimizer steps, gradient finite checks, checkpoint + fresh-load, stop reason, free answer, final semantic scorer. Report DPO margin vs free answer separately.
- **Falsifier:** if C doesn't alter free semantics, do not conclude DPO mathematical bug; route, tokenizer, base capacity, generation EOS and consumer should be separately investigated. D always separate from neural.
- **Stop rule:** first inspect 1 case after 1–4 updates; if recipient wiring is broken, stop before large datasets. No GitHub Windows run, no CI gate substituting for subject learning.

## Short executor protocol / prompt (copy-safe)

**CURRENT TASK:** execute one named EXP above and publish one evidence packet only. **NOT TASK:** rewrite core algorithms, all 500 records, run full CI, claim semantic improvements from hashes, edit protected core docs or other authors' claims.

**Skill load:** architectural decision: architect-behavior-classifier-SKILL.md + trainable-architecture-decision-SKILL.md; experiment-design-SKILL, learning-path-audit-SKILL, code-examination-SKILL, phase-planning-SKILL, plan-review-SKILL, claim-verification-SKILL, human-communication-GLOSSARY, prompt-patch-authoring-SKILL; verify source and recipient before the first step.

**Inputs:** named immutable checkpoint SHA; source/expected panels; trained Unit/model hash; exact branch/commit; test harness source; baseline/enhanced single factor; pre-registered scorer and metrics. **Good:** “same lesson, same model, same consumer, only retrieval policy varied, 2/8 incorrect accepts→0/8 with 8 retained correct.” **Bad:** “weights changed, DPO test green, model learned”; or “E25 synthetic top1 grew, therefore RP01 free response recovered.”

**Output fields:** experiment_id; domain/topic; profile/direction; recipient; ordinary consumer; source snapshot; train/heldout group IDs and leakage audit; A/B toggled factor; exact source/hash; predeclared quality rubric; raw outputs; route; unit/model version; runtime environment; correct/total; coverage; incorrect accepted; latency RAM/VRAM; post-restore correctness; effect and uncertainty; verdict(PASS/FAILED/SCALE_UP/ARCH/DATA/TOOL); what was **not** proved; next smallest discriminating step. Reviewer verdict source-only vs native observed distinct.

**Not executed now:** complete EXP-GOLD-01/02, EXP-N55-01/02/03 native, EXP-KNN-04 native, EXP-RP01-05 neural. Only source-equivalent, no training, regex microprobe has a reproducible receipt. Existing native P125/P25 are historical data and remain unchanged.
