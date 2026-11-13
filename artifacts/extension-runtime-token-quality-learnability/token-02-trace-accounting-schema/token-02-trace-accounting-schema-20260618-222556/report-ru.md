# TOKEN-02 trace/accounting schema report

## Что сделано

Создан статический контракт для будущих token capture packets. Он фиксирует, какие JSONL-строки и JSON-отчёты должны появиться позже для baseline и adaptive прогонов: идентификаторы пары, host/surface, prompt hash, метод, evidence class, target locator, common/adaptive refs, tokenizer/accounting version, counted spans, redaction refs, quality prerequisite и claim locks.

Этот пакет не запускал runtime host, не делал external AI/model calls, не считал eval, не запускал training/DPO и не мутировал checkpoint/locator. Это static schema evidence only.

## Как будут сравниваться токены

Сравнение допустимо только внутри одного `pair_id` из TOKEN-01. Baseline строка должна быть `common_baseline`, `adaptive_enabled=false`, без user-local checkpoint и без чтения adaptive artifacts. Adaptive строка должна иметь `adaptive_enabled=true` и точные ссылки на user-local/target checkpoint, adaptive manifest, target locator и fresh-load evidence.

До token delta идут три обязательных шага: сначала pair integrity, затем quality non-regression для того же `pair_id`, и только потом token delta. Pair integrity требует одинаковые host/runtime surface, prompt hash, method, evidence class/live scope, target locator mode кроме явной baseline/adaptive переменной, tokenizer/accounting version, counted spans policy и redaction policy.

Текущий accounting freeze использует `xiigen-token-accounting.v1`, tokenizer `xiigen-heuristic-word-punctuation` version `v1`, type `heuristic`, counted spans policy `xiigen-counted-spans-v1`. Если позже появится exact tokenizer, его нельзя смешивать с heuristic внутри одной пары: обе стороны должны перейти на один и тот же новый accounting contract.

## Что не является достаточным evidence

Token count decrease сам по себе не доказывает quality, runtime readiness, target promotion, learnability или token savings. Если quality non-regression для того же `pair_id` не PASS, savings claim остаётся закрыт даже при меньшем числе токенов.

Dashboard/API/helper/handler/package-smoke traces не доказывают live Claude/Cursor/Codex token savings. Они могут быть только scoped diagnostic evidence внутри своего evidence class. Historical traces тоже не подходят, пока они не привязаны к текущим TOKEN-01 pair ids, prompt hashes, evidence class, tokenizer/accounting и redaction policy.

Отсутствие adaptive model refs, target locator refs, fresh-load refs, raw/private refs, redacted refs или token accounting report делает пару invalid. Любой mismatch host/prompt/method/evidence/tokenizer/redaction/adaptive ref/quality prereq должен закрывать claim lock.

## Текущий статус narrow/blocked runtime evidence

TOKEN-01 оставил Claude и Codex пригодными только для narrow/core-boundary accounting preparation, без live full-host token savings claim. Cursor остаётся deferred/blocked до safe launch contract. Поэтому следующий work order должен быть `TOKEN-03-NARROW-TRACE-CAPTURE-OR-DEFER-01`: он обязан либо честно отложить capture для неподготовленных поверхностей, либо подготовить только scoped same-evidence-class packet без live savings claim.

## Claim locks

Все claim locks в этом пакете остаются false: token savings, quality improvement, semantic readiness, learnability, live host readiness, target promotion, locator update, final readiness и product readiness не заявлены и не разрешены.
