# RP02 V6A — независимые law-family экзамены C# / EN→RU / SQL
**План и открытые примеры, НЕ реальные SEALED и НЕ уже измеренные результаты.** Независимый составитель готовит TRAIN/OP_DEV/SEALED из разных проектов/первоисточников, фиксирует версии законов, права, hash семьи и не допускает leakage. Перефразировки одного источника — одна семья. Заморозить `P,K,Student,decoder,vector_schema,candidate_ids,rights,CPU/GPU/PC,law_version`.
## C# — AST и nullable
* **GOOD** `string? s; var n = s?.Length ?? 0;`: `?.` условный доступ, `??` fallback; возвращаемое выражение принципиально отличается от `s?.Length`. **BAD** «?. само заменяет null на 0» — неправильный AST/тип. **EDGE** другая версия языка / Nullable выключен / dynamic dispatch: доказанный version-specific oracle или UNKNOWN, не косинус.
* **GOOD** отнесение `What does ?. do in this C# snippet?` к subject=C#, intent=CODE_Q. **BAD** subject=CODE_Q; ось вида задачи ошибочно подменяет предметную область. **EDGE** SQL snippet с просьбой перевести ответ на русский: MIXED subject=SQL, intent=TRANSLATE, EN→RU.
* Измерить: AST operator error, type/nullable law error, источник версии, финальная корректность документации и лучший из заранее перечисленных допустимых решений. Использовать parser oracle для независимого adjudication, не подставлять его в runtime.
## EN→RU — референты, роли, модальность, кириллица
* **GOOD** “Do not send until approved.” → «Не отправляйте до подтверждения.» сохраняет отрицание+условие. **BAD** «Отправьте до подтверждения.» — противоположный приказ. **EDGE** разные правильные стили речи/лицо, судить смысл и scope, не exact string.
* **GOOD** «Книга лежала на столе; он был тяжёлым» допускает `он`=стол, хотя книга feminine. **BAD** жёсткий запрет мужского после feminine существительного без сопоставления референта. **EDGE** неоднозначный referent, сохранять вероятностные альтернативы и abstain.
* **GOOD** «Книга оказалась интересной» женский род согласован при same referent. **BAD** мужской без переопределения того же объекта. **EDGE** цитата, новый субъект, смешанный EN/RU — пересчёт синтаксических связей.
* **CODE RISK** `TrainingVectorBuilder.NormalizeRoots` = `[^a-z]+`: на этом пути кириллица теряется; **не утверждать, что таков весь tokenizer** без actual caller. Выход: Unicode-aware versioned φ, index rebuild, independent output exam; Unicode/ё/punctuation edge cases.
* Измерить: negation-scope and conditional-preservation, POS distribution только при actual POS consumer, grammatical dependency and referent accuracy, false law veto, semantic alternative admissibility, ordinary translation quality; учесть Unicode.
## SQL — третья независимая группа
* **GOOD** `WHERE col IS NULL` vs `WHERE col = NULL`: трехзначная логика. **BAD** считать эквивалентом по lexical NULL. **EDGE** неизвестный диалект/DDL — qualify/UNKNOWN.
* **GOOD** `L LEFT JOIN R ON L.id=R.id AND R.active=1` vs перенос `R.active=1` в WHERE: возможна потеря unmatched left rows. **BAD** утверждать эквивалентность, потому что набор слов одинаков. **EDGE** дубликаты справа/NULL columns/версии — oracle based on schema.
* Измерить: dialect/schema mapping, 3VL / join law compliance, correct query/answer path vs lawful alternatives, time and abstention.
## RP01 DPO и RP02 retrieval-preference: разные цели, качество chosen/rejected
**Student response-DPO (RP01):** берём одну и ту же версию prompt x, закона домена, контекста и разрешённого source family. Утверждённая китайская Teacher-модель создаёт ОБЕ финальные стороны chosen y+ / rejected y−; отдельная реально запущенная CODEX Astra UltraCode-экспертиза проверяет содержательную разницу, если такой вызов состоялся. Пара обязана объяснять, почему chosen существенно лучше при **том же** вопросе. Реальная DPO objective учитывает замороженную reference policy, например `−log σ(β[(log πθ(y+|x)−log πref(y+|x))−(log πθ(y−|x)−log πref(y−|x))])`; требуется настоящее обновление Student/LoRA параметра. Не называйте ручные метки/индексный rerank/неисполненный Teacher-вызов «DPO-обучением». `chosen.model != rejected.model` в отдельном историческом формате метаданных не заменяет проверку одинакового x и действительного авторства обеих финальных сторон.

**Domain ranker (RP02):** допустимая предпочтительная пара `(q,z+,z−)` должна иметь **одинаковый query, кандидатный тип, права, source-law version и пригодность по hard arbiter**. Для обновления w_d допустима другая, парная ranking objective, например `softplus(−w_d·[φ_d(q,z+)−φ_d(q,z−)])`. Она НЕ является RP01 response-DPO. Запрещённый tenant/source результат вообще исключается из selectable pool; научно полезный bad example или hard-law violation не становится обслуживаемым negative neighbor и не легализуется cosine score.

| Домейн | GOOD chosen vs rejected и почему | BAD и почему | EDGE/действие |
| --- | --- | --- | --- |
| C# | Оба документа про те же `?.` и `??` в одной версии; chosen объясняет nullable и fallback, rejected корректен частично, но упускает scope | chosen и rejected из разных q/версий или проверяем только style/длину — перепутаны причинные факторы | Два равно корректных объяснения: TIE, без обновления |
| EN→RU | Обе допустимые версии сохраняют отрицание; chosen лучше сохраняет `until approved` | «Отправьте» как chosen для “Do not send” — инверсия предметного закона | Неоднозначный референт, несколько корректных русских переводов: independent adjudication/альтернативы |
| SQL | Обе цитаты из одной версии SQL/DDL; chosen дополнительно объясняет LEFT JOIN ON/WHERE и NULL | разные диалекты и схемы объявлены взаимоисключающей учебной парой — ложная preference | Неизвестный диалект или NULL edge: UNKNOWN/quarantine |

**Обязательные поля на каждый реальный урок:** pair ID и once journal; x/q, выбранная и отклонённая стороны, обоих авторов/реальные вызовы или NOT_PERFORMED; exact rights/consent/law/source-family/version; какой дефект rejected и почему chosen лучше; независимая оценка TIE/BOTH_BAD; frozen reference/Student или ranker before/after SHA; версии φ/index; actual ordinary consumer; blind multi-domain T−C0 и whole-PC. Повторённая или взятая из SEALED пара не является новым уроком. Показатель доли корректных пар важнее количества строк.

**LoRA→semantic token (01C) не происходит автоматически.** Только реальная цепь `LoRA A/B → trained projector/codebook → φ_d version → index → cold ordinary consumer` с независимой source-family абляцией позволяет говорить о переносе; иначе UNBOUND. Одна учебная пара/один update доказывают только механизм: для качества обучения нужны независимые source families B0→B1→B2, доменные ошибки, время и полная стоимость.

## Сравнивать ОДИН фактор, не проценты покрытия
**Первая E2 причинность:** P=старый код/стейт; C0=исправленный код без state update; T=байт-идентичный C0 с одним pairwise update, реальный changed parameter SHA/once journal и normal cold consumer. `T−C0` обучение; `C0−P` engineering. Если code/state/output не существует → NOT_WIRED/NOT_TRAINED/NOT_MEASURED.
**C01–C06 по отдельности:** C01 axes mapper, C02 versioned domain φ, C03 learned ranker at fixed k, C04 dynamic k vs fixed within equal PC, C05 exact vs ANN on same eligible pool/index schema, C06 optional typed second-neighbor answer. Factorial RP01 real LoRA×RP02 lawful kNN=4 cells только при двух настоящих arms, без смешивания source family.
**B0→B1→B2:** независимо новые уроки, перепроверять старые доменные семьи без подмены рубрик. Одно обновление = механический PILOT_ONLY, не доказательство переноса.
## Фальсификация и ремонт
* Неверная буква закона или false gender veto → проверить authority/referent, не перескладывать SEALED под победителя.
* T−C0 не лучше → STATE_CHANGED_NO_GAIN (возможно rollback), не считать тренировку законченной.
* Цена RAM/VRAM/index/teacher/judge/failed branches сильно выше при том же качестве → отвергнуть либо улучшить бюджетный k-policy и перепроверить на новых данных.
* Нет normal user consumer → NOT_WIRED; добавлять только разрешённый tiny adapter, не новый универсальный framework.
* Нельзя доказать права или независимость SEALED → INVALID_EVIDENCE, переотбор independent families.
* Короткие, случайные, повторно используемые trials → PILOT_ONLY / неопределённость; фиксировать семейства и независимого examiner.
```yaml
task_id: RP02.__
P_planning_sha: ""
K_core_sha: ""
ordinary_consumer_method: NOT_WIRED
candidate_type_and_law_version: ""
rights_and_tenant: UNKNOWN
vector_schema_hash: ""
train_family_ids: []
dev_family_ids: []
sealed_family_hashes: []
P_output: NOT_MEASURED
C0_output: NOT_MEASURED
T_output: NOT_MEASURED
before_after_trained_state_sha: NOT_TRAINED
once_journal_id: NOT_TRAINED
new_product_tests_for_readonly: 0
new_focused_changed_deterministic_risk_probes: []
per_law_accuracy_and_false_veto: NOT_MEASURED
best_lawful_path_abstain_p50_p95: NOT_MEASURED
whole_PC_RAM_VRAM_disk_teacher_judge_cost: NOT_MEASURED
B0_B1_B2_retention: NOT_MEASURED
independent_Astra_receipt: NOT_PERFORMED
verdict: DOCUMENT_ONLY
one_next_owner: ""
```