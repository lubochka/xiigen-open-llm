# N58-06 — новая узкая source-гипотеза: известные противоположные состояния проходят SourceLaw как UNKNOWN

**19.10.2026. Статус: NEW_CANDIDATE_N58-K3 / CODE_PREMISES_VERIFIED / FORMULA_DEDUCTION / NATIVE_NOT_RUN / FINAL_QUALITY_NOT_MEASURED / PRIOR_ART_GLOBAL_SEARCH_INCOMPLETE / PENDING_LUBA_REVIEW.** Это не повтор N27/N37 `not/no` stopword и не доказательство neural RP01. Новизна здесь **внутри уже распознаваемых исходным SourceLaw слов** (`safe/unsafe, valid/invalid, enabled/disabled`), а не в попытке расширить исходный parser на все формы.

## Причинное доказательство из source

[TranslationMemorySourceLaw.cs L12–43 @eb05219](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMemorySourceLaw.cs#L12-L43) содержит:
1. `SimpleClause` с явным набором предикатов `empty|full|open|closed|ready|available|valid|invalid|enabled|disabled|safe|unsafe|null`. Следовательно, `the file is safe` и `the file is unsafe` **оба проходят один и тот же regex**.
2. `Compare` проверяет `subject`, `copula`, **`predicate`** на буквальное равенство. Если предикаты `safe` и `unsafe` отличаются, немедленно возвращает `Unknown`, **НЕ `Contradiction`**. Вызов сравнения `negative` происходит только ПОСЛЕ равенства всех трёх полей.
3. [TranslationUnitContracts.cs L268–293](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationUnitContracts.cs#L268-L293) подаёт SourceLaw verdict на retrieval admission, в котором `UNKNOWN` не равен доказанному противоречию. Но дополнительный source-score/signature может независимо отвергнуть candidate. Поэтому утверждение ограничено **L0 результатом SourceLaw**, а не final user result.

### Различающие пары — это не общий тест «не теряет ли not»

| Query | Candidate lesson source | Текущее L0 `Compare` (из кода) | Semantic relation in closed grammar |
|---|---|---|---|
| `the file is unsafe` | `the file is safe` | `Unknown` | Contradiction |
| `the account is disabled` | `the account is enabled` | `Unknown` | Contradiction |
| `the file is not safe` | `the file is unsafe` | `Unknown` | Compatible by explicit state-equivalence |
| `the file is safe` | `the file is not safe` | `Contradiction` | Contradiction — existing control |
| `the file is safe` | `the file is safe` | `Compatible` | Compatible — existing control |

**Новое условие:** защищённый механизм уже включает обе стороны в **свой собственный closed vocabulary**, но не определяет инверсию смысла для противоположных предикатов; для `not safe`/ `unsafe` не распознаёт эквивалентность. Нельзя считать любые две разные прилагательные противоположными: `ready` и `available` не всегда антонимы; `open` и `closed` могут зависеть от задачи. Предлагаемый L1 допускает только заранее подтверждённые пары `safe↔unsafe`, `valid↔invalid`, `enabled↔disabled`; остальные UNKNOWN. Это правило для source-law admission, не доказанная языковая способность Unit.

### Prior-art boundary

Обычная потеря отрицания и односторонний overlap уже найдены в [N27-04](../27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) и [N37-C](../37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md). Protected [P55.4](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P55-translation-laws.md) добавляет отдельный `can/cannot` law outside current copular predicate equality. **K3 — более узкое, независимое утверждение: противоположные слова уже находятся ВНУТРИ списка SourceLaw, но сравниваются как unrelated predicates.** В просмотренных P55/P56/P117/N27/N37/N56 это самостоятельное направление A/B не найдено; полного exhaustiveness / всем 499 entry новизны НЕ удостоверено.

## Мини-эксперимент N58-EXP-K3 (не запускать до личного APPROVE)

**Изначальный запрос — дословный фрагмент:** «Это может быть качество построения ДПО, Лора механизм, неправильные параметры векторной токенизации ... скрытые детерминистические алгоритмы ... КНН ... Все возможные различия надо собрать в документы с доказательствами». «... оформить небольшой эксперимент — как проверить гипотезу — который можно быстро реализовать ... под домейны». **Source-law — именно детерминистический ограничитель learned memory, а не новая обучаемая матрица.**

**Тема:** перевод; профиль EN→RU, английские простые copular states (не императивы K1/K2). **Actual learner under test:** `TranslationMachine` + `executor-en_ru` memory, no neural weights or DPO; `TranslationMemorySourceLaw.Compare` — zero-update source-law unit under A/B. **Запрошенная модель исполнения/рецензирования:** `gpt-6-astra / ultra` из исторического профиля; фактический session model ID, review identity and authorization **PENDING**. По внешней модели после запуска обязательно сохранить runtime attestation, а не заменить его названием из промпта.

**Навыки и подготовка:** exact current `docs/skills/intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `experiment-design-SKILL.md`, `code-examination-SKILL.md`, `root-cause-tracing-SKILL.md`, `learning-path-audit-SKILL.md`, `claim-verification-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md` и `human-communication-GLOSSARY.md`; [пиннутый корень навыков](https://github.com/lubochka/llm_mvp_core/tree/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/skills). Snapshot IDs of executed skills, AGENTS and reviewer before RUN. No protected core writes.

**L0 — быстрый zero-update native comparator** (требует APPROVE, не был исполнен):
- A: неизменённый `TranslationMemorySourceLaw.Compare`.
- B: та же функция с **ОДНИМ изменением** — замена буквального equality predicate на canonical `{pair_id, sign}` из закрытого **TRAIN-defined** набора (`safe/unsafe, valid/invalid, enabled/disabled`) плюс старый `negative` XOR. Все остальные source regex, `subject`, `copula`, tokenizer and stopword data неизменны.
- Frozen paired inputs в таблице выше, 5 запросов ×2 версии =10 native `Compare` calls; 0 lesson rows, 0 optimizer updates, 0 learned state change. Expected reason codes for A shown above. B expected `Contradiction`, `Contradiction`, `Compatible`, `Contradiction`, `Compatible`. Это **контрактная корректность policy**, а не измеренное улучшение переводчика. Если actual callable/internal access запрещён — `NOT_REACHABLE`; никогда не моделировать `Compare` моковым oracle и называть native.

**L1 — two-lesson, four-exam actual-flow conditional** (другая стадия, сама по себе не разрешена L0):
- Initial A/B: две fresh `TranslationMachine` с пустой ENRU memory, same tokenizer, index/k5, params, signatures and verifier; hashes captured before; никакого `GenerationStudent` или DPO.
- TRAIN T1 `the file is safe` → `файл безопасен`; TRAIN T2 `the account is enabled` → `учётная запись включена`, as typed `executor-en_ru` examples. Exactly two lessons per arm in one batch, identical state except policy; no EXAM rows in TRAIN.
- EXAM E1 `the file is unsafe` → «файл небезопасен»; E2 `the account is disabled` → «учётная запись отключена»; E3 `a file is safe` → «файл безопасен»; E4 `an account is enabled` → «учётная запись включена». Russian synonyms with same state accepted per frozen rubric. **E3/E4 are same-law near controls, not independent family transfer.**
- B changes **only K3 predicate semantic relation inside SourceLaw**; K1 query score, K2 imperative parser disabled. Do not add EXAM answers to learned map or parse gold inside policy.
- For each arm: A0/A1/B0/B1 on same 4 cases; logical16 native ordinary `GetOrchestratorAsync().RunAsync` calls (12 physical only with initial identity proven), 4 example training rows over two arms, 2 TrainStructuredBatchAsync calls, 0 neural updates. Save candidate source, exact/prefix/structure tier, SourceLaw verdict, target compatibility, verifier and bootstrap evidence. If L0 changes but actual KNN never returns T1/T2 for E1/E2: `L1=NOT_REACHED`; only comparator correctness can be claimed.
- Before/after metrics: actual `correct/all`, `wrongaccepted/all`, `answered/all`, `correct/answered`, `no-answer/all`, critical inversions, learned memory triples/checkpoint bytes, cold restore parity, elapsed time by stage; real process CPU and peak RAM if instrumented; model parameters/layers N/A, Unit/Flow executed depth logged. Numeric expected: **no promised gain**, B may decrease wrongacceptance by rejecting mismatched neighbor and leave correct0/4; any observed recall loss shown explicitly. Gold and SEM scorer never passed into native learner.

**Stopping criteria:** approval/model/initial SHA missing; source caller does not reach requested code; actual labels ambiguous; any protected edit; intermingled initial state; incompatible driver/consumer; TEST material appears in TRAIN; more than one algorithm factor differs; no independent review. All -> STOP and report, not default success.

### Будущий точный промпт исполнителю (НЕ АКТИВЕН)

> Только после письменного APPROVE(N58-EXP-K3, SHA карточки, model/session ID, TRAIN/EXAM freeze) прочитай перечисленные навыки. Сверь `TranslationMemorySourceLaw.cs` и `TranslationUnitContracts.cs` с core eb05219. Без изменения защищённой библиотеки отдельно проведи L0: A literal predicate, B closed complementary-predicate canonicalization, пять paired source comparisons; сохрани фактические значения. Если по согласованию отдельно разрешён L1, создай два fresh `TranslationMachine`, обучи одним batch на T1/T2 каждую, получи через обычный Flow все E1..E4 A0/A1/B0/B1, запиши выбранные KNN source и verifier, сохрани сырой output до rubric. Сообщай native report и независимому reviewer только после сохранения artifacts; не заявляй, что компаратор улучшил сам нейронный RP01 или перенёсся на иной язык. При первом STOP-condition не выполняй дальнейшие действия.

**Human gate:** Люба лично проверяет именно это тело, все исходные строки, список complement pairs, skills, модель исполнителя, freeze и expected outcomes. Сейчас **approval=PENDING**, запусков 0. Для нейронного Student нужна отдельная карточка и отдельная пара состояний (B0/DPO etc), не присваивать этой.
