# Порядок кратких причинных экспериментов — без GitHub Windows-марафона
Каждый test должен иметь ① один механизм ② одинаковый matched control ③ raw+semantic + independent family rubric ④ stop/rollback ⑤ видимый вычислительный бюджет. Это **предлагаемые работы**, не выполненные модельные вызовы. Основа уже есть в [RP01.REC01-EVIDENCE.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC01-EVIDENCE.md) … [RP01.REC05-LEARNING-LOOP.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC05-LEARNING-LOOP.md); не создавать вторую конкурирующую очередь задач.

| Очередь / владелец | Вопрос → A/B вмешательство | До каких затрат и когда остановить | Сигнал |
| --- | --- | --- | --- |
| 0 / REC01 | frozen scientific JSON, pin SHAs, exact dataset families, teacher provenance; **no training** | doc-only, 0 model calls | трасса для каждого DIF и реальная подмена/отсутствие |
| 1 / REC02 | numerical directional gradient DPO + masks vs finite difference on tiny model | 1–2 коротких isolated passes; stop on mismatch | gradient sign/scale/one-hot mask correctness |
| 2 / REC02 | 8–32 train pairs: before→after same training cases, forced likelihood & free completion | bounded few updates, early stop when no overfit | обучаем ли выбранный recipient вообще |
| 3 / REC03 | B0 DPO-only vs B0+short lawful SFT→DPO, equal token budget | one new tiny heldout family, no 500 | learnability precondition; hypothesis cold start |
| 4 / REC03 | tokenizer ENRU fragment efficiency / prefix / EOS / greedy first decision | tokenizer-only + ≤2 generation arms | model can decode intended text / stop correctness |
| 5 / REC04 | two input semantic laws: raw/normalized/local60D/shared60D, same downstream | no trainer; one tagged pair | meaning/coordinate information lost? |
| 6 / REC04 | exact lawful kNN vs generic first-hit vs BM25+semantic rerank, with arbiter | same memory, tiny 10–20 examples, no 500 | precision, coverage, veto, negative retrieval |
| 7 / REC04/RP03 | typed Unit two-input/two-output + Flow correction + OutputRefs; old/new law provenance | one complete consumer trace | correspondence, no deterministic override |
| 8 / REC03/REC05 | q/v rank8 vs larger/alternative modules **only if** tiny train overfit indicates capacity bottleneck | equal compute/tokens, one variable per comparison | learning per wall-time, not just weight count |
| 9 / REC05 | warmed old-domain competence then ENRU no-replay vs replay, fixed trained parent | only once positive C# baseline | old retention and new gain |
| 10 / REC05 | selected checkpoint SHA → normal user request consumer | one cold re-open + actual generation | no stale/incorrect serving version |
| 11 / REC05 | transfer to unseen domain/law after first gain, deterministic-only vs learned-only vs hybrid | tight wall/RAM, no blanket Windows CI | heldout semantic gain minus abstention cost |

## На каких данных сравнивать
Пары не равны независимым families; C#25×20 и ENRU20×25 => family-disjoint split. Сохранённые historical 3/4-case blind panels **не** использовать для нового «независимого подтверждения», они уже раскрыты. Для C# нужны AST/compiler/runtime oracle для небольшого проверяемого поднабора; для ENRU — сохранение negation, actor/recipient, modality, number, referents + non-ambiguous source meaning. Для SQL NULL/LEFT JOIN, для dynamic graph unseen composition, DTC rendered layout. Точные размеры на каждом новом pilot определяются бюджетом и дисперсией, а не красивыми 95%/PASS обещаниями.

## В каждой строке нового scientific receipt
`runID, HEAD, actual student checkpoint sha, family IDs, train/dev/heldout identity, teacher provenance, law ids, chosen/rejected hashes, exact masks, base/adapter frozen keys, tokenizer sha, optimizer/rng/cursor, reference identity, pre-post response odds, free generation, independent scorer, raw invalid/abstain, domain coverage, latency phase, peak own process RAM + GPU if measured, rollback, learned vs deterministic attribution`.

**STOP-полезность:** если grad test не сходится — чинить exact math seam; если сходится, но tiny train не overfits — focus B0/LoRA/tokenize/LR; если tiny overfits, но free still invalid — decode/prefix/capacity/generalization; если trained Student works, но ordinary consumer не меняется — integration/adoption/routing. Смена причины меняет next task, а не заставляет автоматически расходовать ещё 500. Результат NEGATIVE допустим. Не добиваться «позеленевшего CI» подгонкой assertions.

## Следующая конкретная мини-проверка вместо неопределённого «поэкспериментируем»

- [DIF-01: контролируемое tiny A/B random B0 + DPO-only vs SFT→DPO](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) — тот же B0, две новые блок-группы, один фактор, SFT-only промежуточная точка, masked objective, fresh-loader, независимые semantic families, stop criterion; это **документ плана**, не исполненный эксперимент.
- [DIF-01…44: точный R0/R1/T1/T2 протокол на каждый пробел](12-DIF-01-44-CHEAP-CAUSAL-PROBES.md) — не 44 запуска, а 44 проверяемых способа отличить кодовое различие от реального падения качества обучения.

Если улучшился поиск/decoder при замороженном Student — это `CONSUMER_GAIN`, **не** `STUDENT_LEARNING_GAIN`. Последнее требует actual admitted tiny training and blind new-family free-output correctness. Нет существующей команды `train 8` в RP01 CLI; сначала разрешённый research mode или точный существующий owner.
