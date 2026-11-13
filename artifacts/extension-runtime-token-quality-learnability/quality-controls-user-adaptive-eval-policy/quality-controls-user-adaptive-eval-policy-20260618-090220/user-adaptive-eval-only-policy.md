# User-adaptive eval-only policy

Run: `quality-controls-user-adaptive-eval-policy-20260618-090220`

Статус: policy/evidence design only. Сейчас не запускается eval, не создается training data, не меняются model/checkpoint/target/locator и не открывается common route.

## Что разрешено

- Создать policy, matrix, gates и next-phase selection artifacts для 7 `USER_ADAPTIVE_ONLY` сценариев и 21 pair row.
- Разрешить только будущий user-local/private/evidence-only eval-only runner, если все gate условия из `eval-only-gates.json` пройдут.
- В будущем хранить raw private inputs только в approved user root, а в repo класть только redacted non-training evidence refs.
- Готовить scenario/rubric wording без score outputs и без claims.

## Что запрещено

- External web/AI enrichment.
- Training, continue-training, DPO training, common dataset creation.
- Любая мутация model/checkpoint/target/manifest/locator.
- Копирование private/user/session/project-specific rows в common training.
- Использование 5 `EVIDENCE_ONLY` сценариев как eval/training inputs сейчас.
- Любые claims про token savings, quality improvement, semantic readiness, learnability, live-host readiness, common readiness, strict `QUALITY-03`, target promotion или locator update.
- Любое касание старого `cursor-user-data`.

## Что это доказывает

- Есть локальная/private/evidence-only policy route для будущего eval-only runner по 7 user-adaptive сценариям.
- Route остается non-common и не может обновлять common sets/checkpoints.

## Что это не доказывает

- Не доказаны token savings, quality improvement, semantic quality, learnability, live host readiness, common readiness, training readiness, strict `QUALITY-03`, target promotion или locator update.

## Следующий независимый трек

- `QUALITY-01` как artifact-only scoring rubric / eval prompt pack preparation: без model mutation, без eval execution, без readiness claims.
