# QUALITY-SEMANTIC-GATES-22 — staged promotion/rollback/backup packet

QSG-22 собрал неразрушающий staged-пакет перед возможной production promotion. Никакой production locator/checkpoint не был изменен: все proposed changes записаны как dry-run manifest.

## До

- Активный locator: `artifacts/extension-runtime-token-quality-learnability/quality-semantic-gates-17-test-only-active-locator.json`, scope `test_only_candidate_scoped_xiigen_extension_api_service_user_adaptive_asset_root`.
- Production/common registry: `models/index.json`.
- Текущие production/common checkpoints остаются в `models/...` и не мутированы.
- QSG-16 target/rollback кандидаты остаются `STAGED_INACTIVE` / `STAGED_INACTIVE`.

## После QSG-22

- Создан snapshot текущих locator/checkpoint refs: `artifacts/extension-runtime-token-quality-learnability/qsg22-staged-promotion/current-locator-checkpoint-snapshot.json`.
- Создан dry-run manifest exact changes: `artifacts/extension-runtime-token-quality-learnability/qsg22-staged-promotion/dry-run-promotion-manifest.json`.
- Создан rollback manifest: `artifacts/extension-runtime-token-quality-learnability/qsg22-staged-promotion/rollback-manifest.json`.
- Выполнен staged backup/rollback JSON+hash probe: `artifacts/extension-runtime-token-quality-learnability/qsg22-staged-promotion/rollback-load-probe.json`.
- QSG-21 host target-bound rows записаны как основание для promotion consideration: `artifacts/extension-runtime-token-quality-learnability/qsg22-staged-promotion/qsg21-promotion-basis.json`.
- Local adaptive readiness записана честно: evidence есть, но full readiness не открыта из-за overlay-only ограничений PromptToTask и SkillSelection.

## Почему production mutation еще не сделана

QSG-21 разрешает рассматривать promotion, но не является approval на irreversible mutation. QSG-22 доказывает, какие файлы изменились бы при approval, какие backup/rollback refs будут использованы, и какие claims остаются закрытыми.

## Claims

Открыто: staged snapshot, dry-run promotion manifest, rollback manifest/probe, QSG-21 promotion basis linkage, limited local adaptive evidence linkage.

Закрыто: token savings, quality improvement, semantic quality, learnability, common readiness, production locator update, target promotion, production rollback closure, final readiness.

## Следующий QSG шаг

QUALITY-SEMANTIC-GATES-23: review QSG-22 staged packet. Затем либо получить явное approval Luba на exact dry-run production mutation и выполнить ее с immediate rollback verification, либо сначала закрыть PromptToTask/SkillSelection full-machine local adaptive readiness gaps без production mutation.
