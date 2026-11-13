# 04 — Dynamic kNN, domain laws, гибридный inference

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: попытка поиска и выбранный ответ

**Проверяемая ошибка:** P66 присваивает `KnnReached` по имени окончательного адаптера. В обычном `ExecutorUnit.Execute` поиск может быть вызван, отвергнуть всех соседей и перейти к запасному пути. Поэтому итоговый fallback не доказывает «KNN не достигнут».

Для переноса фиксировать отдельно вход в каскад, число найденных кандидатов, число допущенных, выбранный lesson ID и окончательный источник ответа. Положительный контроль: поиск вызван, все кандидаты отвергнуты законом/порогом, штатный fallback завершает ответ; expected `attempted=true`, `selected=false`. Другой контроль: точная память отвечает раньше; expected `attempted=false`. Подмена этих двух трасс должна нарушать неизменный оценщик. При отсутствии трассы старое поле означает только «KNN не выбран»; историческую попытку нельзя дорисовывать по конечному имени.

Это требование относится к обычному reader этой карточки и не меняет k, корпус, фильтр или назначенный бюджет.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Допуск проверяется до отсечения соседей; выбранный ответ возвращается со своим содержимым.** P104 сохраняет результат 2/5 → 4/5 законных ответов. Однако содержимое двух непервых кандидатов хозяин опыта берёт из обучающего корпуса. Для переноса обычный API должен вернуть тело выбранного урока, а обычный исполнитель — выполнить именно его шаги.

Шестой законный кандидат после пяти незаконных, полное отсутствие законных кандидатов и повторные записи одного урока — три разных контрольных случая. Их входы действительно портятся; оценщик не получает название порчи. Сохраняются исходные кандидаты, причины допуска, логические личности уроков, результат отсечения и источник выбранного тела.

В P107 первый сосед и большинство совпали на всех 12 входах: преимущества агрегации эта панель не показала. Синтетический набор из двух записей u1 и одной u5 различает правильный и сломанный учёт дубликатов, но не измеряет качество естественного поиска. Стоимость внепанельных запросов учитывается; восстановление требует загрузки новым объектом.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->

## DIF-401 — top1-only несмотря на k hits (CODE; P0 для kNN consumer)
[21.AgentInfrastructure/Logical/KnnLogicalUnit.cs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs) в ExecuteAsync: `hits=_index.Search(query,_k)`; `best=hits[0]`; prediction=`bestExample.Output`; остальные IDs — evidence. Следовательно _k не даёт голосования, learned reranking, law-aware admissibility в показанном пути. Это конкретная missing function, но НЕ доказанный root cause прямого DPO RP01 (не установлено, что этот consumer вызывался в free generation).

## DIF-402 — Независимая BPE+hashed predicate схема вместо структурных AST (CODE + CONTRACT_DIFF)
`Train` создаёт локальный BpeTokenizer `vocabSize:320`, HNSW `m8 efConstruction64 efSearch32`; Vectorize разделяет token/predicate tiers. Historical E09 строил структурный Roslyn SyntaxKind; там reported top1 .2413→.3507, но baseline и treatment имели разные validation subset! [Experiment reconciliation](01-EXPERIMENT-BASELINE-RECONCILIATION.md). Нельзя переносить E09 gain на нынешний KNN; сначала same-panel parity.

## DIF-403 — Хеш-признаки и ограниченная выразительность отношений (HYPOTHESIS)
BPE token hashing/predicate hashing могут схлопывать полярность, operator scope, entity identity, referent relation и SQL NULL semantics. Проверить актуальную `Vectorize` формулу, коллизии и долюlaw critical pairs с одинаковым вектором; не утверждать конкретные collision rates без расчёта. Typed roles и relations должны оставаться явными, vector schema version/axis contract известным; ANN соседи должны быть лишь кандидатами.

## DIF-404 — Confidence ≠ P(correct) (CODE + OPEN)
`confidence=Math.Clamp(best.score,0,1)`, rounded; threshold генерирует warning при floor. HNSW similarity score не является калиброванной вероятностью правильности. Проверить эмпирическую reliability/failure bins по домену и риск false acceptance; soft uncertainty/abstain на ambiguous context вместо выдачи уверенного одного класса.

## DIF-405 — Hard laws нужны перед ограничением соседей, но не все закономерности жёсткие (REQUIREMENT + OPEN)
Example GOOD ENRU: с большой уверенностью источник обозначает женский referent, мужской target pronoun требует проверку agreement; но `Книга на столе. Он деревянный` может описывать стол. BAD: глобальный gender-ban из-за пола предыдущего слова. C#: AST/type/scope/nullability; SQL: NULL three-valued and JOIN placement; unit of measure/order/negation. Закон имеет тип HARD/CONDITIONAL/SOFT/UNKNOWN, происхождение, версию и потребителя. Из source `4132bf71` translation polarity guard уже есть ограниченный deterministic contrast; это не learned metric.

## Минимальный causal 2×2
A frozen nearest-only/no law; B same retrieval+law; C learned rerank/adaptive-k/no law; D both. Для всех same examples/index, measured retrieval recall, top-1/top-k lawful answer accuracy, coverage/abstain, contradiction severity, wall latency, peak PC cost. Изменять только один компонент, не увеличивать k без обоснования. [RP02 owner](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack). Результат kNN и direct DPO записывать отдельно.