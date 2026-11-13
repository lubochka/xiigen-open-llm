# Видимый русский статус плана после review cycle 2

Текущая стадия: `PLAN-REVIEW-CYCLE-02_OF_10`.

Runtime/code/training/host execution сейчас не продолжается. Причина простая:
текущая инструкция Luba требует сначала 10 свежих bounded review/fix циклов по
execution plan. Старые review rows, старый state, QSG-отчёты и parent self-review
не считаются этими 10 циклами. После 10/10 parent должен показать этот
человекочитаемый русский packet, и только потом можно возвращаться к следующему
разрешённому work order. Даже после 10/10 runtime/model действия требуют свежий
approval packet из раздела 10 плана.

## Шаги плана

1. `RUNTIME/HOST-IMPL`: проверить или починить тонкие Claude, Cursor и Codex
   adapters, затем доказать реальный host runtime path с visible answer,
   debug/error path, raw/redacted input/output, correlation id и core-boundary
   evidence.
2. `TOKEN`: собрать одинаковый scenario pack и paired baseline/adaptive token
   traces с одним tokenizer/accounting method.
3. `QUALITY`: оценить outputs по human-readable rubric, positive/negative
   examples, source-disjoint split и regression guard.
4. `LEARN`: пройти AI output/correction -> DPO -> user-local training ->
   backup/export/import/share -> fresh-load -> continue-training -> held-out ->
   ablation -> rollback.
5. `DOC`: написать human docs, которые объясняют, как запускать и читать
   runtime/token/quality/learnability evidence.
6. `REVIEW`: после implementation провести 10 отдельных final readiness checks.

## Что уже сделано по QSG evidence

- `QSG-01..02`: в текущем target evidence set не найдены как named
  `quality-semantic-gates-01/02` report files. Их нельзя выдумывать или считать
  current proof без отдельного provenance packet.
- `QSG-03..13`: есть progressive repair/eval/token/quality/semantic-gate
  evidence. Оно помогло продвинуть диагностику, но не открыло full readiness.
- `QSG-14`: открыл только узкие bounded claims для evaluated scenario pack:
  bounded semantic graduation, bounded semantic-quality gate и bounded token
  savings claim. Это не common/product/final readiness.
- `QSG-15..18`: common readiness, product runtime semantic quality,
  learnability, target promotion, locator update, rollback closure, product
  readiness и final readiness оставались закрытыми; live target-bound host
  proof не был закрыт.
- `QSG-19`: проверил target-locator plumbing на handler/test-only уровне и
  rollback-load probe. Он явно не заявил live UI/chat readiness, production
  locator promotion, production rollback closure, runtime extension readiness
  или final readiness.

## Что остаётся с QSG-20+

Следующий runtime-facing шаг после 10/10 review gate: true live Claude/Cursor/Codex
UI/chat target-bound probes с QSG-19 TargetLocator path. Только если эти live
доказательства проходят, можно отдельно проверять production-like locator
promotion и rollback closure. Full readiness всё равно закрывается только после
10 final readiness checks.

## Claims

Открыто узко: только QSG-14 bounded semantic/token claims для evaluated scenario
pack.

Остаётся закрыто: token savings readiness, quality improvement readiness,
product runtime semantic quality, learnability, common readiness, target
promotion, locator update, rollback closure, promotion readiness, product
readiness и final readiness.

## Как план создаёт 3 full extensions

План создаёт три полных host-facing extensions: Claude, Cursor и Codex. В каждой
ветке adapter должен быть тонким: принять user prompt в реальном host path,
собрать `XiigenExtensionRequest`, вызвать `XiigenExtensionApiService` внутри
`llm_mvp_core`, получить `XiigenExtensionResponse`, показать visible answer,
debug trace и controlled error state. Вся логика prompt-to-task, orchestration,
skill selection, adaptive learning, DPO, backup/export/import/share,
continue-training, rollback и no-overclaim остаётся в `llm_mvp_core` evidence
path, а не прячется в host adapter.

Что этот packet не доказывает: live host readiness, token savings readiness,
quality improvement readiness, local learnability, common model readiness,
promotion, rollback closure и final readiness пока не доказаны.
