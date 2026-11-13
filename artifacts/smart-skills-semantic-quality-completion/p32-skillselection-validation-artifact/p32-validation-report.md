# P32 validation report - P31 candidate rows

Вердикт: P32 artifact-level validation прошел. Это доказывает целостность P31 candidate rows, но не доказывает качество модели.

## Что проверил runner

Runner `p32-validate-p31-candidates.js` прочитал P31 artifacts и проверил:

- есть 12 materialized DPO candidate rows;
- source families не дублируются;
- есть 6 dashboard QA candidate scenarios;
- split manifest не заявляет training/eval/checkpoint mutation;
- leakage design не заявляет execution against model;
- каждая строка сохраняет запрет на common training, training, eval и raw private text copying;
- каждая строка имеет context package, chosen decision, rejected decision, quarantine example и teaching point;
- каждый dashboard scenario показывает candidate-only scope и debug field `training_allowed_in_p31=false`.

## Что это не доказывает

Это не model eval. Это не semantic-quality gate. Это не host runtime. Это не checkpoint readiness. Это только artifact-level QA, чтобы следующая фаза не строилась на сломанной схеме.

## Результат

Runner вернул `passed=true`, `failures=[]`.

Следующая фаза может строить более сильную validation layer или dashboard integration вокруг этих сценариев, но training/eval/checkpoint mutation все еще требуют отдельного authorization.
