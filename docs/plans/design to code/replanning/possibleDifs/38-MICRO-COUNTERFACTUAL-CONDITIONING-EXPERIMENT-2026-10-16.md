# MICRO-38-A — проверить за один короткий read-only запуск, научился ли RP01 зависеть от изменённого условия

**Experiment ID:** RP01-MICRO-CF-COND-01. **16.10.2026.** Статус READY_TO_IMPLEMENT / CODE_READ / NATIVE_NOT_RUN / SEMANTIC_BLIND_REVIEW_NOT_RUN. Это протокол причинного опыта, **не** receipt выполнения. Основа: [N38-A](38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md). Не меняет core, frozen C#500/ENRU500, sealed exams или защищённый docs/plans.

## 0. Проверяемая теория и опровергающий результат

**H1:** C50 после 50 DPO шагов научился предпочитать общие ответы, но различия конкретного кода недостаточно меняют вероятность законного ответа. Поэтому два исходника с противоположными correct output получают одинаковые знаки preference.

**H0 / фальсификатор:** у C50 conditional signed margin устойчиво **меняет знак** на противоположных q0/q1 и делает это лучше B0. Тогда семантическое условное знание частично возникло; искать потери в autoregressive first-token/stop/framing/adoption.

Отдельная альтернатива: всё остаётся случайным у B0 и C50 — тогда ни affirmative, ни negative preference prior не объясняют поражение; DPO не сформировал обнаружимого law-discrimination.

## 1. Данные: шесть OPEN контрастов C#; не sealed exam

**Замороженный machine-readable диагностический набор:** [38-MICRO-COUNTERFACTUAL-CODE-PAIRS-OPEN-v1.json](38-MICRO-COUNTERFACTUAL-CODE-PAIRS-OPEN-v1.json) — шесть пар полных prompt0/prompt1, candidate answer0/answer1 и expected stdout. Поле semantic_stdout_review=PENDING_NATIVE_OR_INDEPENDENT_CHECK. Набор не используется как train pairs, не объявлен source-disjoint и не заменяет независимый gold. Писать scores только в новый receipt, не редактировать fixture при неудаче.

Это **синтетические диагностические** пары, созданные только как входы для read-only evaluation, не для генерации DPO или обучения. До native запуска независимый C#12/.NET8 исполнитель или эксперт должен подтвердить stdout; при другой семантике запрос исключается **до** осмотра результатов модели. Слова y0/y1 составить строго в едином стиле «It writes VALUE because LAW.» на основе *OPEN* law oracle и запечатать до просмотра inference.

| Case family | q0: программа и expected output | q1: смысловой контрфакт и expected output |
|---|---|---|
| C01 precedence | Console.Write(2+3*4); → 14 | Console.Write((2+3)*4); → 20 |
| C02 increment | int x=4; int y=x++; Console.Write(y); → 4 | int x=4; int y=++x; Console.Write(y); → 5 |
| C03 short-circuit | bool b=false; int x=1; bool r=b&&(++x>0); Console.Write(x); → 1 | тот же текст с b\|\|(++x>0) → 2 |
| C04 concatenation | Console.Write(2+3+"4"); → 54 | Console.Write("2"+3+4); → 234 |
| C05 sign division | Console.Write(-9/4); → -2 | Console.Write(-9%4); → -1 |
| C06 value vs reference | int[] a={1,5}; int[] b=a; b[0]=9; Console.Write(a[0]); → 9 | int a=1; int b=a; b=9; Console.Write(a); → 1 |

Псевдоним Console означает System.Console при фактическом исполнении. Полные q0/q1 должны иметь **строго один и тот же instruction framing** текущего RP01 «C# 12 / .NET 8. Explain this code's observable behavior in one sentence.», после перевода всех Console в System.Console; никакого обучающего примера в prompt. Эти six cases могут пересекаться по *закону* с train500, поэтому это **механическая панель**, не независимое доказательство generalization. Добавить потом **8–12 source-family-disjoint OPEN** C# случаев по отдельным семействам и независимым oracles до утверждения результата.

## 2. Четыре вида read-only наблюдений над ровно теми же inputs

**Модели:** сохранённый B0 и реально reopened C50, identical tokenizer SHA, architecture, seed, reference identity; получить из существующих RP01 receipts/checkpoints (не создавать новый random B0). Снять hash каждого checkpoint, direct admitted source, LoRA keys. Если recovered C50 не побитово соответствует snapshot — NO_RUN / STATE_INVALID; нельзя сравнивать со свежесозданным «похожим» Student.

**M1 — условные log probabilities.** Для каждой q и двух y использовать *один* read-only policy forward с canonical segmented BPE-v2: model input = encode(q) + encode(y), mask = 0 для prompt и 1 для response, next-token target shift по [DpoTrainer.EvaluateAverageNextTokenLogProbability](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L956-L990) или через канонический read-only exact-material evaluator. Для сравнения с DPO response-sum умножить returned mean на действительное число masked response target IDs. Показать **и сумму, и среднее**, не смешивать в одном primary.

Для каждой пары q0/q1 вычислить 2×2 score matrix:
- s00=logP(y0|q0), s01=logP(y1|q0)
- s10=logP(y0|q1), s11=logP(y1|q1)
- d0=s00−s01, d1=s10−s11, contrast=d0−d1.
- Сохранить выбранные raw token IDs обеих y, counts, masks, fingerprint, q hash. Неправильный response prefix, маска либо другой checkpoint = PRECONDITION_FAILED, а не score 0.

**M2 — prompt-free prior negative control.** При том же framing, но нейтральном test-only code (не пустая подсказка), оценить y0/y1; он не должен сам давать сильную правильную ориентацию для каждого противоположного закона. Для C# использовать нейтральное «System.Console.Write(0);» только как отрицательный контроль вне обучения, без объявления семантической правильности какого-либо y0/y1.

**M3 — unchanged ordinary free generation.** Для B0/C50 вызвать настоящий [CodeDocumentationStudentGeneration.Generate](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76) на том же q, temperature=0, same maxNewTokens, stop/null EOS, seed, tokenizer; сохранить text, stopReason, first8/16/32 decoded tokens и semantic law correctness. Не добавлять префикс y и не менять decoder в этой стадии.

**M4 — cold restore parity.** Reopen C50 во **втором** read-only instance, сравнить scores и greedy first-token побитово либо с объявленной float tolerance. Если различается — сначала investigate source/adapter/seed/cache; это не failure гипотезы о conditioning.

## 3. Принадлежность доменам и следующий отдельный опыт

**C# — primary.** Ожидаемую семантику устанавливает изолированный .NET toolchain или независимая проверка, не LLM Student. Инструкции кода учитывать по AST/порядку операторов, а не similarity bag-of-words. Использовать один сценарий для обоих C# arms.

**EN→RU — optional separate ID RP01-MICRO-CF-ENRU-02:** восстановить C50/C100 на реально ENRU обученном состоянии и freezing tokenizer; создать 4–8 пар «not», «never», субъект he/she, числовое отрицание, падеж/время с корректными target answers и независимым русскоязычным судьёй. Матрица score строится между двумя **русскими** y0/y1; не использовать C# frozen Student для русского языка и не подменять его обученным TranslationMachine/KNN.

**HE/SQL — не запускать** без фактических соответствующих checkpoints; такие идеи остаются переносимыми гипотезами.

## 4. Предрегистрация результатов (не подгонять)

- **Primary (механический):** доля противоположных пар, где C50 правильно меняет оба знака d(q0)>0 и d(q1)<0; отдельно число таких же пар у B0; absolute контрфактический interaction contrast и delta C50−B0. Сохранить все 6 строк, даже провалы.
- **Primary (качество):** paired correct_free_answer/12 queries по независимому C# law oracle + wrong accepted/abstain, отдельно на source-disjoint OPEN panel.
- **Secondary:** prompt-free prior scores; masked-length/first differing token/rank; cosine distance hidden states **только если доступен настоящий runtime diagnostic**; p50 latency; checkpoint SHA; output head/LoRA state IDs.
- **Сигнал для follow-up:** хотя бы **4/6** контрастов меняют signed preference правильно **только** в C50 и нет регрессии critical wrong на source-disjoint panel. Это exploratory triage, НЕ статистическое доказательство generalization. Если 0/6 у обоих — приоритет warm-start / gradient / language-base. Если sign правильный, но free 0/12 — приоритет output initiation/decoder/KV/stop.
- **Falsify H1:** matched/contrafactual signs и прирост free semantic quality достоверно отражают source law; тогда «не зависит от входа» не является объяснением.
- **No shortcuts:** не подставлять правильный y в prompt; не оценивать только train-500 предпочтения; не редактировать inputs после просмотра C50; не объявлять улучшение от изменения checkpoint SHA; не считать значение DPO margin предметной точностью.

## 5. Исполнимый порядок без Windows GitHub runners

1. **Architect/code examiner:** за 1 запуск подтвердить canonical score API и source SHA, открыть B0/C50 как read-only, подтвердить model-state binding и BPE segmentation.
2. **Executor:** предрегистрация fixture 6 пар, независимые stdout, SHA JSON; один локальный native .NET 8/9 console harness, максимум 24 response score на модель плюс 24 free generations и retry только если инфраструктурный сбой.
3. **QA/reviewer:** отдельным проходом проверить raw masks, source→score, cold restore и расчёт 2×2, открыть semantic gold лишь после вывода; выдать PASS / FAIL / INCONCLUSIVE с причинами и всеми случаями.
4. **Planner:** одно causal решение: (a) переносим способность через warm start, (b) decoder/recipient restore, (c) корректируем data/contrast, (d) no signal. Лишь затем расширять training; никакие CI/Windows/e2e не включать.

**Machine-readable receipt minimal schema:** experiment_id, source_sha, config_sha, tokenizer_sha, seed, pair_id, q_sha, y0_sha, y1_sha, checkpoint_sha, step, s00, s01, s10, s11, d0, d1, contrast, changed_sign, free_text_sha, free_correct, stop_reason, state_parity, independent_rubric_id, reviewed_by, verdict. Не считать описание исполнения фактически выполненной пробой.
