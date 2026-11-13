# Карта исправленных копируемых промптов — дополнение к129

Дата: 2 ноября 2026. Область: только передача контекста в существующих Markdown; это не аудит качества185 экспериментов и не результат выполнения заданий.

185 различных файлов:147 экспериментальных карточек и38 файлов переноса/общих договоров. Первые три этапа добавили268 входов контекста:41 первого этапа,135 второго,92 строчных абзаца третьего. Это число вставок, не число самостоятельных копируемых промптов. Дополнительная независимая проверка обнаружила143 actual-промпта, отделённых от входа другим блоком; в них вход добавлен непосредственно внутрь. Все143 находятся в тех же185 файлах. Этапы пересекаются по файлам и не складываются как независимые аудиты.

Ссылки120§1,40§2,42 и точный путь самой карточки помещены внутрь передаваемого блока/абзаца. Итоговая проверка различает общий вводный blockquote и следующий самостоятельный prompt. Независимое извлечение подтвердило268 actual-фрагментов (143 исправленных,92 строчных,33 ранее защищённых), а не вывело их число из268 первых вставок. Проверены также граница авторизации и отрицательные документные копии с отсутствующим/подменённым полем. PRIMARY, данные, команды и бюджеты этой операцией не редактировались. Полное новое чтение требуется перед новым заданием, а не перед каждым плечом того же опыта.

Проверка сохранности: этап1 —41/41 обратных SHA; этап2 —135/135 после отдельного учёта согласованной параллельной поправки P59 о NOT_DISCRIMINATING; этап3 —83/83 файлов с92 абзацами. SHA ниже фиксирует bytes непосредственно перед соответствующим этапом. Он не является подписью исторического эксперимента. Проверка этапа2 отменяет позднейшие строчные вставки; P59 дополнительно исключает только явно указанную поправку, не удаляя её из live-файла.

Точная поправка P59: «Для подмены ответа выбрать сохранённый A, реально отличающийся от expected; иначе это NOT_DISCRIMINATING, не отрицательный контроль.»

SHA ниже относятся к первым трём этапам до последних143 исправлений границы копирования. Их отдельная обратная проверка и итоговый exact-copy разбор приведены в [пакете исследователя](reviews/2026-11-02-executor-context-followup.md). Это документная проверка обратимости, не185 независимых проверок и не доказательство соблюдения инструкций будущим исполнителем. [Основной отчёт129](codex-129-executor-context-followup-2026-11-02.md).

| Файл | Этапы / число строчных абзацев | SHA256 до этапа1 | SHA256 до этапа2 | SHA256 до этапа3 |
|---|---|---|---|---|
| [experimets/cards/AST-SIB-CACHE-01.md](../experimets/cards/AST-SIB-CACHE-01.md) | 1 | 77c0baa81add6e2564152fb7c6315304e543c07b14e270724d75ceda0f23a5cf | — | — |
| [experimets/cards/BPE-NO-MATCH-COPY-01.md](../experimets/cards/BPE-NO-MATCH-COPY-01.md) | 2 | — | 6e1e52ae0514b73972d66eff4dc8f15c833d22a98b9661fb7bf2338e0c561496 | — |
| [experimets/cards/E-DPO-SFT-ORDER.md](../experimets/cards/E-DPO-SFT-ORDER.md) | 1 | ef467c6bbef8301264b96c4adb7ef0da50adc55daba4c77edc3698e3fe56bb91 | — | — |
| [experimets/cards/E-DPO-VALID-LENGTH.md](../experimets/cards/E-DPO-VALID-LENGTH.md) | 1 | c522bc0a16d53b1e38e853e85100657af3f026d65d2bb28404f8f375c1af1fdf | — | — |
| [experimets/cards/E-SFT-MOMENT-CONTINUITY.md](../experimets/cards/E-SFT-MOMENT-CONTINUITY.md) | 1 | 659e265e332c9eec53be124d62cab8a48356e914a6130ee228d4383008f0d100 | — | — |
| [experimets/cards/E-SFT-RESPONSE-MASK.md](../experimets/cards/E-SFT-RESPONSE-MASK.md) | 1 | 70bae349f992f66513c103b13ec6ca5f3870e4cfa8efbf9d0933e3c738f14400 | — | — |
| [experimets/cards/FROZEN-BROADCAST-MAP-01.md](../experimets/cards/FROZEN-BROADCAST-MAP-01.md) | 1 | f800567c246a8c1f927919c68dca9d57196ebbefd1d349af7079690920a131a2 | — | — |
| [experimets/cards/GRAD-ACCUM-COPY-01.md](../experimets/cards/GRAD-ACCUM-COPY-01.md) | 1 | 3730b6f0cff24e1b5565ebbfc1cca7906af8eda60e499d82318a03fbaa9037c5 | — | — |
| [experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md](../experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md) | 2 | — | a572ae61f5c5abd56a0633b4c640ec6c75547cd111cdd67b8598aceec9689b4b | — |
| [experimets/cards/N-HEAD-01.md](../experimets/cards/N-HEAD-01.md) | 3 (1) | — | — | 501af4b1587d73127b9a8835ff45e9914470fd885442c97efe2574a6162e4e08 |
| [experimets/cards/N-KU-01-do-loop-label.md](../experimets/cards/N-KU-01-do-loop-label.md) | 1 | be0ffa9abdb5eb557bc16e643cda30011f5c4531772481f91ce3fc45082854fa | — | — |
| [experimets/cards/N60-e1-verifier-admission.md](../experimets/cards/N60-e1-verifier-admission.md) | 1 | d370d5bb6fe511d83deae5b87b078e39b6a682062eee03e25f076db6d772c092 | — | — |
| [experimets/cards/N61-dpo-reference-reuse.md](../experimets/cards/N61-dpo-reference-reuse.md) | 2 | — | 482ac8bf53bb20c71c92123c76e60d82560b3d6ba379a54f63da45e6d97657c6 | — |
| [experimets/cards/N61-mode1-retention-and-identity.md](../experimets/cards/N61-mode1-retention-and-identity.md) | 3 (2) | — | — | feb51bed43df6f90cdf95211c3809d3c7530ebf794a24eda64032bc6520011bb |
| [experimets/cards/N63-correction-collision.md](../experimets/cards/N63-correction-collision.md) | 2 | — | 7f7440c40597ca9717f847009d3f9827ac8176ac3aa15e5e6cca2f2faf3aa43a | — |
| [experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md](../experimets/cards/NEURAL-FROZEN-TRANSPOSE-01.md) | 2 | — | 5c6f015fc9f2b81ed15cf7d5e96ae976e966073e1adeae575e5b4e5eb72a8927 | — |
| [experimets/cards/P01-trainable-recipient.md](../experimets/cards/P01-trainable-recipient.md) | 2 | — | f589d42d8155594cc4c7d94e62068a5ecf4b205828693c7d816858746b8deb25 | — |
| [experimets/cards/P02-warm-base.md](../experimets/cards/P02-warm-base.md) | 2 | — | efde400b6cf6e4af5ac31b0d96619041ac8a069810b853b5c91228993dab1fc1 | — |
| [experimets/cards/P03-historical-score-lineage.md](../experimets/cards/P03-historical-score-lineage.md) | 2 | — | 9cef3fcf970c1dcfd39d3ed840144a6d34ab883461237ffb1ab42c20d65cf715 | — |
| [experimets/cards/P04-preference-contrast.md](../experimets/cards/P04-preference-contrast.md) | 2 | — | 9ea455fd8d8d39d4c3c04c9150b35ab24f0cf98978eef6badbc58df7dc169603 | — |
| [experimets/cards/P05-response-length-objective.md](../experimets/cards/P05-response-length-objective.md) | 2 | — | 9f324d17e2fd0147d77785d589e076f2d4b5a6a5f6e3cf9b8393b41832fc973d | — |
| [experimets/cards/P06-teaching-vector-adamw.md](../experimets/cards/P06-teaching-vector-adamw.md) | 2 | — | f8e2d6f59f6afa96d2f10002aaf11be9594e2b4fc46b2386aaf62d500e71e792 | — |
| [experimets/cards/P07-materialized-token-contrast.md](../experimets/cards/P07-materialized-token-contrast.md) | 2 | — | 3ba59bc21006269a534c564fadd3f152b833c8eab71906a1b9286960e4b6580b | — |
| [experimets/cards/P08-answer-boundary.md](../experimets/cards/P08-answer-boundary.md) | 2 | — | ab3f3856db3bd03dbe305bc4997b56b446e9170c3e16dab69be4e3e0d984ebce | — |
| [experimets/cards/P09-cached-forward.md](../experimets/cards/P09-cached-forward.md) | 2 | — | c1da9a3011da3381c32581032c19dde89a647f765540972cdde1ddb669124322 | — |
| [experimets/cards/P10-composed-gradient.md](../experimets/cards/P10-composed-gradient.md) | 2 | — | 58f4b39c219706fb6eb1732fdbc9cca52d79854df6a82642d1ad17d677cc1dd1 | — |
| [experimets/cards/P100-topk-ties.md](../experimets/cards/P100-topk-ties.md) | 2 | — | 09cc86233a67c10a1374892bd0838e2c2e23664cc8499de62c739d96d5361923 | — |
| [experimets/cards/P101-learning-budget.md](../experimets/cards/P101-learning-budget.md) | 2 | — | 5da4e0cca8d2c02448356eed870bedb367038db3684107e16ddc093a477dbb1b | — |
| [experimets/cards/P102-teacher-forced-decoding.md](../experimets/cards/P102-teacher-forced-decoding.md) | 2 | — | 31a73c5345f505bb56815b242d2962a86b193bf13f7ec5957d2fbea10a7484fd | — |
| [experimets/cards/P103-stack-before-normalization.md](../experimets/cards/P103-stack-before-normalization.md) | 2, 3 (1) | — | 3d04f74f1482e86e7a3f8db4b197c884e6e2df8b2afe29e66dd044a32c349bbf | 2ba7cf22d24913d958232e2dd8078bb3a3536958d220650946a7a0ae543a40af |
| [experimets/cards/P104-law-eligibility.md](../experimets/cards/P104-law-eligibility.md) | 2, 3 (1) | — | 8f7f3ace20bfb58eec55fe12257aa7c99c36c2197aaade24eec2fb529654b833 | a49f9b404022f26d388edd67c88a3f3459aef692e33ec0cceef24fdb6dd09b5b |
| [experimets/cards/P105-frozen-reference.md](../experimets/cards/P105-frozen-reference.md) | 2 | — | edf017523dea670bd98bbb99ee0fcb4f7b458ee15d4f3e5d14ca4166c459a958 | — |
| [experimets/cards/P106-corrected-semantic-target.md](../experimets/cards/P106-corrected-semantic-target.md) | 2, 3 (1) | — | 8e8ca2195b5c2b34bdc728dcb4ca8c24aa95ab697b205bc8af5045d360f6d057 | 3512518c79036342af387e93ed9b953672bf2272ff231b2de3fa8fccc10afbbf |
| [experimets/cards/P107-lawful-neighbor-aggregation.md](../experimets/cards/P107-lawful-neighbor-aggregation.md) | 2, 3 (1) | — | d8bf3ee1a9817fe17a968d4335dabced416b2b5cbda0f781c521f09e829f1fb4 | 0ac656bc427181b408c4ca09d2f940ea055ef6c254ce0440d9c9db2f9e8f24ff |
| [experimets/cards/P108-metadata-carrier.md](../experimets/cards/P108-metadata-carrier.md) | 2, 3 (1) | — | 935f466485cbb47e74b9ce553e6c8f2227a8aa08c511c99ec70f18387b0e80b4 | 4997cb63cf644ed589a55045d4e60cd076a13d075be762fa93940a4898fe8ce8 |
| [experimets/cards/P109-gold-validity.md](../experimets/cards/P109-gold-validity.md) | 2 | — | cc4d0fff1347b6ffd0c87d212566a7b629cc09fd43f6e0365b0ac3000669d8c9 | — |
| [experimets/cards/P11-autograd-reachability.md](../experimets/cards/P11-autograd-reachability.md) | 2 | — | cef81e9420a9430ee378be609aee95bdf968cd1c6fd0593fbd4f2d194b230dca | — |
| [experimets/cards/P110-numeric-matrix-cost.md](../experimets/cards/P110-numeric-matrix-cost.md) | 2, 3 (1) | — | 99e3505e1ba295a6e29c7cf78702937be4d8ff4d4e37a35d0d3ce2efc488701d | 8ce3640bd15bae7924ddb718cab5cbd80e7a407cdb6666209a25d0f57c924207 |
| [experimets/cards/P111-reward-credit.md](../experimets/cards/P111-reward-credit.md) | 2, 3 (1) | — | a70fa2fcc0252aec71bf3035f6c2a9b39a335819831aeea69a6d18d8071db616 | 1b1a12d23fd86c18145c0e8481cfe6f88bdb95c81263300a77b66e183462f44a |
| [experimets/cards/P112-family-disjoint-evaluation.md](../experimets/cards/P112-family-disjoint-evaluation.md) | 2 | — | 3b096bb1a6fb87da3dd2d5000b5f59bbf2cd753d9c47fdd40dda0de241d550cb | — |
| [experimets/cards/P113-feature-energy.md](../experimets/cards/P113-feature-energy.md) | 2, 3 (1) | — | 6e7d22ee7b253291637e8e11c0dd408cca82abc76a389df797b47a2c1ea20985 | 391ced44ae41e01893da1012f68b700417c383d8c99d2cc13a365f84128b3321 |
| [experimets/cards/P114-zero-weight-pair.md](../experimets/cards/P114-zero-weight-pair.md) | 2 | — | c01743eaf38c284b8ace60dc58a028faf6859b72c72b15fb93cfcfa8f35206f1 | — |
| [experimets/cards/P115-bm25-statistics-cache.md](../experimets/cards/P115-bm25-statistics-cache.md) | 2, 3 (1) | — | 08ac5a6155fec79687f6bcc9848e9d886acdda0089f5915a5477bd22a3da3b29 | fee2bbb59ecc8c7d75e27c5b38b4604a02d2a513cd38129f2012b22d3591ddaa |
| [experimets/cards/P116-e43-file-boundary.md](../experimets/cards/P116-e43-file-boundary.md) | 2, 3 (1) | — | 122846fb841140a872cda5261605af1eba4a7701a2ed421d6ea140a0d4ee75f9 | 761058030182d1d7d091e2f207b6229f27aeb1fe08c1cf42c03e167faa5f09e7 |
| [experimets/cards/P117-translation-direction-memory.md](../experimets/cards/P117-translation-direction-memory.md) | 2, 3 (1) | — | 150eebacbb5097b33ce8fcda30816f9bc038637b882ad351a6dcd38cdde78ae7 | 195c17f9f62f7423921b96cdf3a4a21d6dca9417db733548667ea818c098887b |
| [experimets/cards/P118-projectwiki-content.md](../experimets/cards/P118-projectwiki-content.md) | 2, 3 (1) | — | 36751550689f459406252bbc71ce51ffc44912b772c3554e46ef3040b68300dd | 101bd73bdddc500fb465ba120540e55c460a5681c447ec2d1041a4a57bf54340 |
| [experimets/cards/P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md) | 2 | — | efb3c298899577ac8e5b4e5296f3ed6081fe2e3e609d965e0d42bce736bbe25e | — |
| [experimets/cards/P12-optimizer-boundaries.md](../experimets/cards/P12-optimizer-boundaries.md) | 2 | — | b7cf05525ac3d7115778f6e4388770eb2d6f9224443b2d0e925390b97f841401 | — |
| [experimets/cards/P120-semantic-compression-laws.md](../experimets/cards/P120-semantic-compression-laws.md) | 2, 3 (1) | — | 43808247e244c1ae47cf2bd4ed4c31a1db28bc87e127b5b7a0f2a7c5a836bc72 | 622a6951b5611591c35dc9877c2756053f4c6183ec41990ef66d767ff5dfb95a |
| [experimets/cards/P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md) | 2 | — | 0441d65c27ac9dc34e7d42a634653b30b3076a7e72aefb01377d3ff8a2a4a59e | — |
| [experimets/cards/P122-root-channel-unicode-digits-operators.md](../experimets/cards/P122-root-channel-unicode-digits-operators.md) | 2, 3 (1) | — | b0293f8f611f96d2f40755254e0c8b00d2f9d78ca7b5f7cec1f85eb4fba72a57 | 7009b3acb3fa78738cb044a69fad1ec265a4f4daaebcb704bff1ce4d4229da3c |
| [experimets/cards/P123-design-conversion-critic-evidence.md](../experimets/cards/P123-design-conversion-critic-evidence.md) | 2 | — | 96a8744178e40beed70a4ba1ac01aae20803856f0d24e1a77db51c1641b9580a | — |
| [experimets/cards/P124-p3-fixed-discriminator-contract.md](../experimets/cards/P124-p3-fixed-discriminator-contract.md) | 2 | — | 3a15fde2bca1bb54cf99ca218e44b1475467c927d7b85843fdcd26970d2f8eb9 | — |
| [experimets/cards/P125-executor-memory-learning.md](../experimets/cards/P125-executor-memory-learning.md) | 2, 3 (1) | — | fd483ba0f88b89ec90c01abc3ef4cc1cdf26a34b4d066ee2c689b4156177d75e | 82301865aff2a48b2dc089e7f0ba9dc6b118e498c9ddd53233e078d65e0bdc7f |
| [experimets/cards/P13-finite-ingress.md](../experimets/cards/P13-finite-ingress.md) | 2 | — | ac32fa512c09b9e260c0f31a3628af3bd109f25e16a458472d3e6f5c6b49e6ea | — |
| [experimets/cards/P14-teacher-provenance.md](../experimets/cards/P14-teacher-provenance.md) | 2 | — | 39ee9179609781bfbd3628aa6b8fab2df21eea39bc935658a03e7ccb912497c1 | — |
| [experimets/cards/P15-retention-replay.md](../experimets/cards/P15-retention-replay.md) | 2 | — | 586555c9950ee73b07687b86117cec06a2d058742747929a053ec21b3721e6d3 | — |
| [experimets/cards/P16-semantic-order-roles.md](../experimets/cards/P16-semantic-order-roles.md) | 2, 3 (1) | — | e942f5135da4f8835cfab98868f3360562f7230b5a3b4c173d442503d6ecc314 | bbb45c49d70b82cf0abebe73a20a229da3e0c7d53f1785417990e8a340c6c5ac |
| [experimets/cards/P17-tokenizer-refit.md](../experimets/cards/P17-tokenizer-refit.md) | 2, 3 (1) | — | 49169b05302e6aa76a36f36b63a0c654957f6b9f26691c8101a5d1e3cc65c7a4 | 15404df506ab8cbc66b8ea98bd1ca3b13ded7a42914e219039483e2ce6c703bd |
| [experimets/cards/P18-state-identity.md](../experimets/cards/P18-state-identity.md) | 2, 3 (1) | — | f081dddb3a1b2cb38516a1ec54e1668d9bdc9183776d07c79c367087a2d4b587 | 266bd8e33295ca34dc203bdaaca32d2bb9d6b49fc789019e81bd93276a022853 |
| [experimets/cards/P19-mutable-output.md](../experimets/cards/P19-mutable-output.md) | 2, 3 (1) | — | b5f0cb67db7ca61df910fab5682ca99ae4c82df0fd7a7c06a9b3816b91231ce4 | 66a21ef66a8efab5003dfc068613bf1dce210e1dd4f03788205727e657b35633 |
| [experimets/cards/P20-hnsw-reachability.md](../experimets/cards/P20-hnsw-reachability.md) | 2, 3 (1) | — | 1a6ee50a4556e8cb15980f8f4edea6a2b1bd40640c3e723d2890e828a39ed986 | 682719ff46044120dc799b3485c4dfc77aa1fcfa42a16381f405a1393630ffb4 |
| [experimets/cards/P21-hnsw-ingress-atomicity.md](../experimets/cards/P21-hnsw-ingress-atomicity.md) | 2, 3 (1) | — | 752c092e86f005176dcb967fa4cc591f8417d37d68cf599341720e66c6f42faa | 8cde3cd103d5f0dafee96cc1ef7baa43ce79d543eb4d0d5be2d63498891701c1 |
| [experimets/cards/P22-hnsw-rng-continuation.md](../experimets/cards/P22-hnsw-rng-continuation.md) | 2, 3 (1) | — | 30ff246bef4f22f65caf17a42b34eb6101f87a929fd4a8db54f199787abe2d92 | 34f72964c323fa28a12df0323cb523c64ae0b2ab9c84beb140ec3ce6b4a968f9 |
| [experimets/cards/P23-retrieval-fusion.md](../experimets/cards/P23-retrieval-fusion.md) | 2, 3 (1) | — | 6b457842e1f3e9b835a7499feef9f84d35fe30e825ac8ea20fb4f195f3e54801 | f0e5650ec01ae9fd2489718e67d89c132fcf6ddc1a64f4b2330735141738d0db |
| [experimets/cards/P24-replay-selection.md](../experimets/cards/P24-replay-selection.md) | 2 | — | 2a12b3176b361cf2c1de322a574afef2b6a8e49420a8925d561d8877655a8f2c | — |
| [experimets/cards/P25-cold-restore.md](../experimets/cards/P25-cold-restore.md) | 2 | — | c69d8c57d47b5922addd01d71800e207c6f2c6b8a6f67cae957b52dea09b7179 | — |
| [experimets/cards/P26-structural-features.md](../experimets/cards/P26-structural-features.md) | 2, 3 (1) | — | c6a731ae6ff79dff9e3aaa8a48225c7169ceacbdfa941219707c6d36aacde29a | 65964fe6786c11a1c971c392708860205bee52ba8f9793256aafcf082605316c |
| [experimets/cards/P27-structural-codec.md](../experimets/cards/P27-structural-codec.md) | 2, 3 (1) | — | 53ffc4aeaf3ec840f951b4b2951ac2cf6d3327380f979ba2ccb9fd8ce6963b69 | 44586fc77567bd88a17f0bc7471041f3f636e170623959f32d09b8c4a31c9a52 |
| [experimets/cards/P28-morphology-family.md](../experimets/cards/P28-morphology-family.md) | 2, 3 (1) | — | 053f33086e7a616ff9c377c17d893b8f36d552156cd26fcca88084d905b95ef9 | 7159ed94057651ca64825c666aa64a9ee23053385436f5e157ae18b57907dc11 |
| [experimets/cards/P29-formula-memory.md](../experimets/cards/P29-formula-memory.md) | 2, 3 (1) | — | 83e516890b2c5590cb0c20077db23033dcf1b5aba5d708be067b68d1841ee0d1 | cfd01bf44f55d0df68f25e741283f54d22c7b27208803e03a6763151f352a5dd |
| [experimets/cards/P30-learned-fusion-alpha.md](../experimets/cards/P30-learned-fusion-alpha.md) | 2, 3 (1) | — | 989fc8cdc8d98825612ef421d4196640d570297c32d46a0b91a83741ce29bd1f | c55a5c697192f9d0c16c2d1e746af13d887b1d43c946c65f6aa43137817f5009 |
| [experimets/cards/P31-source-trust.md](../experimets/cards/P31-source-trust.md) | 2, 3 (1) | — | 8b9d947caa775113f2d3b50e027ec86b39408caa6e973d1fc9535785b362fcce | 1fc06f105dd60188164620a817c1c2b0b5b021c8757f94e85a426abb44897e0c |
| [experimets/cards/P32-skill-selection.md](../experimets/cards/P32-skill-selection.md) | 2, 3 (1) | — | fbc4b274b8fdc69a6ecc4fb4f83eb76724b5ae4994e96d3275fdb9d6e4169e6a | 201e096d63cc86bd192af22fd75359050823542fd05239c43218c143827e4f46 |
| [experimets/cards/P33-domain-classifier.md](../experimets/cards/P33-domain-classifier.md) | 2, 3 (1) | — | 588105b859f71ecb0814fa18fb8ee645db3209f727f37fdfb88556be8050a326 | fd1653eebee64ee40a863028c12404e864f7a01e3fea56ab277b78c9ec4e0896 |
| [experimets/cards/P34-router-labels.md](../experimets/cards/P34-router-labels.md) | 2, 3 (1) | — | 966d6899945b53f623d3c8d4485430ad48265598dbbe7a75d851479bc05494aa | 45926f7ab926d18906e8d0e8cfb2c1334b03c0a660a1e1b84b59d0af695bfb72 |
| [experimets/cards/P35-actual-neighbor-votes.md](../experimets/cards/P35-actual-neighbor-votes.md) | 2, 3 (1) | — | 4adbfae7773be676eb1d2d77a2fd384e8044af767d742e5c47d64bec8a93df6d | 4a3ab09bc5427b6ca28e05dd8b7c570011a8ae0d456aa9ebfd5948fb5f195f3b |
| [experimets/cards/P36-incremental-router.md](../experimets/cards/P36-incremental-router.md) | 2, 3 (1) | — | c2218190ccac7e82efe86dddecf472957d4a25bcf7c2061e9543a6ba4082d423 | 408ec14bf353468893c435eb9b2e864c330a6b39e0e5770df6b57e27337e9f19 |
| [experimets/cards/P37-empty-router-query.md](../experimets/cards/P37-empty-router-query.md) | 2, 3 (1) | — | 675d83e630b58d658ceeda24348f266ecb925f3c7588cce37327b5c1ec8f43bd | 27f6b3533dfb53a9dd43eb4a7f76b2692bece2390b67e1a90b8036122edd1caf |
| [experimets/cards/P38-learned-segmentation.md](../experimets/cards/P38-learned-segmentation.md) | 2, 3 (1) | — | 5dfbb94bc14524a6795c54d767a303339b965b150b02e17d6501dc51c3f39bc6 | 8b5ec4fc45cb2db887f9c48e4d633bd443b47912bbed4e92ea1910013f949390 |
| [experimets/cards/P39-oracle-output-separation.md](../experimets/cards/P39-oracle-output-separation.md) | 2, 3 (1) | — | 257805386e0cca7d301565225d1a0dd6aa08dd4d98fdc10ce3de16e47c1a7398 | 9c1bb765742d3087155225e36e150413814a35eb6ccdb31457cd7bb76502deb5 |
| [experimets/cards/P40-router-feedback.md](../experimets/cards/P40-router-feedback.md) | 2, 3 (1) | — | 416ef7cd15d0c6304bc88dee92557a37a583cdc067e2bc1af23d418f11090c63 | a40e1e2e7c141cce66c5dd083acd5c66743972bfa9afdc731287e6be6b28cbd7 |
| [experimets/cards/P41-stop-and-split.md](../experimets/cards/P41-stop-and-split.md) | 2, 3 (1) | — | 610a13bbe5f480f6c5bcdcd34d4609cf48ce6e8aa937809efa08fbf331513edd | bf3610153f7c590b2ca1c9d7b7de62da3713f32f9b25b0c78d729c223ce410a7 |
| [experimets/cards/P42-checker-arbiter.md](../experimets/cards/P42-checker-arbiter.md) | 2, 3 (1) | — | e0f0eaa64d16fa82fb37e0c3a65a8d09f3df4cb51aeb177496964f1b7c702d61 | b9ce45dd7e017a4eb0d7742702270c549273938a0b6464d8a938f0a777046119 |
| [experimets/cards/P43-contextual-pos.md](../experimets/cards/P43-contextual-pos.md) | 2, 3 (1) | — | 8f890cdf105ec966cfcae6798144e571de8d0d80c86312b3296bc054058a4ea9 | 26a186cfa462b1f9d4b8eff7b34877b842d0d081af80a441c61b788535aa405c |
| [experimets/cards/P44-template-projection.md](../experimets/cards/P44-template-projection.md) | 2, 3 (1) | — | c11b4de08187d566da2d9bc60c3456840d18b7efffd77ca9b658bd8f7dd49c7b | b3661b30e061efcb3655be15572e6a091bda70cc7819fae8fc8894debb2002fb |
| [experimets/cards/P45-learned-reordering.md](../experimets/cards/P45-learned-reordering.md) | 2, 3 (1) | — | 502eda93132605e6434bc8638436be451f7e4ebb26e69f77fa38a6cdae2b4f2e | 9fd8198da5eec62039d3b579e55e7f18a6c791f27c9977e3d75afdff4abc07b7 |
| [experimets/cards/P46-pair-value.md](../experimets/cards/P46-pair-value.md) | 2, 3 (1) | — | 9c191f64a2e7e680e07d3ebe8cb30020f162de7fefa57f09b078645aa281b450 | 93cbfee85eb2c4c44d3d09c80dd9e7d74ef3e56ce97de25ec7279d3afba1e536 |
| [experimets/cards/P47-file-vector-context.md](../experimets/cards/P47-file-vector-context.md) | 2, 3 (1) | — | 330bcded456852986e76676e74373b39c2beec64fafa8609a608001efb72906f | 22b2329fd8d5fe38677307f77ac96fc29e569a9c20ad895acbcf7eac04bcc4f1 |
| [experimets/cards/P48-weighting-curriculum.md](../experimets/cards/P48-weighting-curriculum.md) | 2, 3 (1) | — | eeec5415abe82adb0918be18c474973a3ca4c196cd36f1fd48e94ea32ca06cf4 | 0bc830b56c4a3970b98551b0018060bee3a8c3d450ea57df060470fc65f28cc1 |
| [experimets/cards/P49-probe-warm-start.md](../experimets/cards/P49-probe-warm-start.md) | 2, 3 (1) | — | 80628022214cc7b7f9e47eb6202b2fee0984d4a042e52dcb42ab6c1c868c92f7 | b6fc0872c7a8827e62a827ac3801d1d8ae054fbd380f43e010b25f34da2c94a1 |
| [experimets/cards/P50-graph-dependencies.md](../experimets/cards/P50-graph-dependencies.md) | 2, 3 (1) | — | f194add6e4acdbcfe9ee580991725ccbf6ac1b52e94f80da21aca13d64f41d0c | d83c87efd85475fd7f6fc45754432011e8c25b0a74f2a2e67218c98b4d210b3b |
| [experimets/cards/P51-translation-context.md](../experimets/cards/P51-translation-context.md) | 2, 3 (1) | — | 52f5c00452e4d7974f8fef7437368b46def4c11fabd1afd34893eccf448f9c9b | d314d113836ff67779c255e5b42725c9d508ca544006ca0a8c8c49450ba3493d |
| [experimets/cards/P52-semantic-annotation.md](../experimets/cards/P52-semantic-annotation.md) | 2, 3 (1) | — | 3a62854afe73309f07c3e4eef303536800d4a4011e8427644b386d41f0085151 | 69ecc892d34308a4d6f6b03d9ab8f6cd323bbf86fcae36c21af2473a24fb8bca |
| [experimets/cards/P53-batch-atomicity.md](../experimets/cards/P53-batch-atomicity.md) | 2, 3 (1) | — | a3aac0fa382041fd5729fa2583fd28e7ed6a1c0684c9e9dbe7b03240ba48d22f | 9113af454a4384fd326ebbcc41739b57ce7e43302ebc0c948725073aeacc7405 |
| [experimets/cards/P54-phrase-composition.md](../experimets/cards/P54-phrase-composition.md) | 2, 3 (1) | — | b291d194f5eb3f2ed6e11b4a8ecda3cf12f148519284a384acd7c94fae0dd625 | be4c727ba04ab71c23056ea8206c5329b5dedad058aa162cc587800eecf8e998 |
| [experimets/cards/P55-translation-laws.md](../experimets/cards/P55-translation-laws.md) | 2, 3 (1) | — | d92f4589181d5b3edef17eccf6d0843c59cd28a5500d0e4c02ba561dcac98056 | 8581df9f0de327e312b23b2605c132af2f3aed0fe36fbd1fc408d2278c7800b5 |
| [experimets/cards/P56-verifier-context.md](../experimets/cards/P56-verifier-context.md) | 2, 3 (1) | — | 25557e60ca929763d73e4f091bac640a8e95a2f956665c1a77d2564e92aa6132 | 7b85a877021e019d9157d1991fa00b933db997adb24588617d24562af9451a09 |
| [experimets/cards/P57-semantic-measurement.md](../experimets/cards/P57-semantic-measurement.md) | 2 | — | a9e037260b6611c043633149f2af3642d86ec7a4db461b86adef9ee133543790 | — |
| [experimets/cards/P58-feedback-learning.md](../experimets/cards/P58-feedback-learning.md) | 2, 3 (1) | — | 13339ed2e1affd828b9e1edd11fda3d483cd1198053603aec80aea79ceb2e7e9 | b507a72c259467f742ec291f166cc94c5c196f84606bc6344219ddb960adcbb2 |
| [experimets/cards/P59-correction-visibility.md](../experimets/cards/P59-correction-visibility.md) | 2, 3 (1) | — | 84ae9693793c41c9f4f0cdc182e4ed6557d97282a59d061a9093046442fdbf01 | 0fd73f4aa47cdccd436394777f635993ed220770ae32e74c42e3a21ec382c415 |
| [experimets/cards/P60-signature-duplicate.md](../experimets/cards/P60-signature-duplicate.md) | 2, 3 (1) | — | cee8424bfd1e305c20591f894eafdd6fbee9f5b6d588c395048fad7a05d80185 | 48a9c8e26f3002f9abc622bd588981c0fab257759f10e3441741b5a40a6fdc5f |
| [experimets/cards/P61-unknown-tail.md](../experimets/cards/P61-unknown-tail.md) | 2, 3 (1) | — | 85759313d12ef1db5efcdd96de8f510b65c35cd5bd551ed5f800bf50f4cd16d1 | 68f8af4f7d9f36a653101f498265bda29e60dc363d446f9d0d047011c0e705d3 |
| [experimets/cards/P62-signature-monotonicity.md](../experimets/cards/P62-signature-monotonicity.md) | 2, 3 (1) | — | 47f2f9d2e8d4484b365809ed716b1c18324537e0504a515b74f2afa8c0cc8932 | 9cff3a3168868823264fc0d8c3febbef170d1bd9a0fc15e42782eefb992ad068 |
| [experimets/cards/P63-source-containment.md](../experimets/cards/P63-source-containment.md) | 2, 3 (1) | — | 46268b51b29177500b274c68a6a70712a9a7678513cd86c7784d984799f17987 | d041afabd4deffb69464925eda5146733acd2f2fe21cf2bac9116f6302cb9464 |
| [experimets/cards/P64-tier-postfilter.md](../experimets/cards/P64-tier-postfilter.md) | 2, 3 (1) | — | cd2a882439a505fcc18948392361cab1c5e66b0712529ffae14d8150af3c82e4 | 18edb6057ad574789d6e84ceec7e7ecdde68e6571a71ea1a90174e5a8b33279c |
| [experimets/cards/P65-history-admission.md](../experimets/cards/P65-history-admission.md) | 2, 3 (1) | — | 1115785b4ada8ad1b5158f71084522e29cba3962ba4be73bed34bcb297903812 | e0325fe0d497a530029b9ea8319b3d2ef2a57f40251ce6a3ad2c2f5716887292 |
| [experimets/cards/P66-heldout-contrast.md](../experimets/cards/P66-heldout-contrast.md) | 2 | — | a729ce3e293789acc57eea890ffc83666b85664e4f8e0df1f91e9991455292a2 | — |
| [experimets/cards/P67-correction-identity.md](../experimets/cards/P67-correction-identity.md) | 2, 3 (1) | — | 693988ba459307becf279c71def3e52cf24a43d52912e4d207362fc331a4ed2c | 9660576408bdbcf5b8f291f65cc099969c6bde82cb35f2c03b8df067d51dc472 |
| [experimets/cards/P68-pretrain-pipeline.md](../experimets/cards/P68-pretrain-pipeline.md) | 2 | — | e9a6300cb1e552c671ecd73773edcdf8e9dcae062a6799f68fd71523a6dca8d5 | — |
| [experimets/cards/P69-graphrag-context.md](../experimets/cards/P69-graphrag-context.md) | 2, 3 (1) | — | ee12d465e7d59c067fc64535129eaaa605e4c82cca089a2e901d409614951965 | aca83f3e0bf8f95baf11ed1538e0f0513d5b57bb2cfdbeae333cc6024c0bc227 |
| [experimets/cards/P70-wikitrace-context.md](../experimets/cards/P70-wikitrace-context.md) | 2, 3 (1) | — | d85e2b834f8a4ff637ea1a66e8922c17073710eb648d04d96e0384c631fadd55 | 6f44101f06516a2138f346cb458eb208dfc5490a2d6b802e75b52d3597a01de3 |
| [experimets/cards/P71-perplexity-alphabet.md](../experimets/cards/P71-perplexity-alphabet.md) | 2 | — | 4dd8a202be72f235414ae79f3129e76652ebc5e95789ee99f369a07e4e41f393 | — |
| [experimets/cards/P72-project-conditioning.md](../experimets/cards/P72-project-conditioning.md) | 2, 3 (1) | — | 0e79bf9db177dc13bfc7d149b8ae7765b3dadecd09ea18c8df71ab9677b74704 | 1d1a6671d84add867a90c6615fc896bd8e5f8ea986c404df091fc4896b8280a5 |
| [experimets/cards/P73-domain-conditioning.md](../experimets/cards/P73-domain-conditioning.md) | 2, 3 (1) | — | e1670d9c01424afc935da91d267bbb6445c9e5dd70bb6b439c667a11c05824ea | e08a0619d79672b06a736272c305f149a311c926aa2d171bbe30a128344849ae |
| [experimets/cards/P74-memory-eviction.md](../experimets/cards/P74-memory-eviction.md) | 2, 3 (1) | — | 4c9e13f4f119ae8fd8e88a72ae3b1631577c54358a3f2a1136cdc189d67f1da5 | c7326ce82e85067ee083e7b69f34b1791aee33b80625c40004a42d1aa625d7f9 |
| [experimets/cards/P75-sampler-residual.md](../experimets/cards/P75-sampler-residual.md) | 2 | — | 51199b0b075106e08567ba1fd1f03bdab4aa7fbb203cc834e823243abe627b2f | — |
| [experimets/cards/P76-utf8-boundary.md](../experimets/cards/P76-utf8-boundary.md) | 2 | — | 19bd065f1d52713dbec0f02da46fed0fb480cf5c9d1d8ebb6f4a1b64dab32c0c | — |
| [experimets/cards/P77-rare-class-support.md](../experimets/cards/P77-rare-class-support.md) | 2 | — | 214c78d5033caf98cef3fddc5c8c0183fb2db7230713bad6b50521fb7c55e2f5 | — |
| [experimets/cards/P78-loss-plateau.md](../experimets/cards/P78-loss-plateau.md) | 2 | — | 27a5629f0be0798977f36934613924b7da3cf5d08a71286bbcc08eece77b129a | — |
| [experimets/cards/P79-grammar-leakage.md](../experimets/cards/P79-grammar-leakage.md) | 2 | — | 33e3b3d990a62b42b1cd61580f572f22fa99aaf84de8b015edd0847f8fa9b192 | — |
| [experimets/cards/P80-marker-denominator.md](../experimets/cards/P80-marker-denominator.md) | 2 | — | 59ea2550af6d30c8e5b8a5328ca7e7ef944b354f5ccf7c8f139b002c36eb3afa | — |
| [experimets/cards/P81-input-conditioning.md](../experimets/cards/P81-input-conditioning.md) | 2 | — | 434073857df1bb5bcc055c3f6373a042e5e7d9615c4b28c620331f9d1c0a2d40 | — |
| [experimets/cards/P82-file-split.md](../experimets/cards/P82-file-split.md) | 2 | — | 96a650d3ad9c394b730573f23e90bbad2cda22cc072e1d80cd1660fa5892fbdd | — |
| [experimets/cards/P83-expression-body.md](../experimets/cards/P83-expression-body.md) | 2, 3 (1) | — | d9c419a7724e15f8dad9bcd3b2b8d1ea6467c7671725c8ce88d6d3d586282b3e | 288e11c7daa27132a1de22e52bd9f03311bee304b4802c93fcf534f2e32f8103 |
| [experimets/cards/P84-lambda-family.md](../experimets/cards/P84-lambda-family.md) | 2, 3 (1) | — | 5daa1ff2a36b2abf344f90184b1fe7c260cd00d517ed104bcb73b412b789bafc | d38d492b818a73bc0661a883346986f528e5c708cb9705e1563af88da9b2fa83 |
| [experimets/cards/P85-composite-obligations.md](../experimets/cards/P85-composite-obligations.md) | 2 | — | d0b336a628e1c44aecc975ff9697540b6753b8bbd34d63a7b4fb99d7a36c544d | — |
| [experimets/cards/P86-fractional-response-mask.md](../experimets/cards/P86-fractional-response-mask.md) | 3 (9) | — | — | 07b7b29b72aa8fe4e9ec6c9915d204edf21e800f676c024de5e0768b97cbf834 |
| [experimets/cards/P87-mixed-objectives.md](../experimets/cards/P87-mixed-objectives.md) | 2 | — | 1b9970b9ec9f8670507e8be73c687793f1985145300bc5d13c78b1675a6f87c2 | — |
| [experimets/cards/P88-unit-context-consumption.md](../experimets/cards/P88-unit-context-consumption.md) | 2 | — | 761b7ce262068dc5804b2c31eb648f185f48c6e16cd94bebedba1bc56c83ca84 | — |
| [experimets/cards/P89-density-stop.md](../experimets/cards/P89-density-stop.md) | 2 | — | f3509f6664aec6a2f230772216a5adaf3125ebbaf703b5096ac147a98d04d88f | — |
| [experimets/cards/P90-tagged-search.md](../experimets/cards/P90-tagged-search.md) | 2, 3 (1) | — | 5e247adf29f3cec7230069c670406c93df915791fe6bfb6f742022e3c000ecac | ee12910e0b37228a764856da0f17e2745ee5af72feafcaa9da6d393c0bdb21bd |
| [experimets/cards/P91-method-pair-split.md](../experimets/cards/P91-method-pair-split.md) | 2 | — | 1da8f41105b5cfd60911a9c809bc51ebba3ec88e4af522be1ef6f8f7720d55bf | — |
| [experimets/cards/P92-dpo-memory-flow-bridge.md](../experimets/cards/P92-dpo-memory-flow-bridge.md) | 2, 3 (1) | — | 338e53301fb5a2761fa49a412cd2adec94bd28293a0ac51f7a7968d00325ddf0 | de349909e738f8d2cb4423a009e68e46a06fec9783666b37398b07c02bac347a |
| [experimets/cards/P93-confidence-abstention.md](../experimets/cards/P93-confidence-abstention.md) | 2 | — | 04722519027e634f81b2ed99213723a39a31814d3f9440f0d6b047ac836fe363 | — |
| [experimets/cards/P94-typed-many-results.md](../experimets/cards/P94-typed-many-results.md) | 2, 3 (1) | — | 98367252dd0153b1ba3a56f41d2424a96043eaedce6f0469be360b7270f4f4e6 | 389863923ad3aeb4422d3febff60927c4f448c3d13b0bdc889f259e96a45371c |
| [experimets/cards/P95-learned-flow-edge.md](../experimets/cards/P95-learned-flow-edge.md) | 2 | — | 30819d2091ed0adaf30c93a4c94095c932485fd72cfdde5d2c65acc6ca3b7bf0 | — |
| [experimets/cards/P96-async-orchestration.md](../experimets/cards/P96-async-orchestration.md) | 2, 3 (1) | — | 6c53c10539c29a2141e0b71cc151721366fead8b3ecf2f53731a4d0ce27da701 | 16f34dcdac74392110de28fa9266cc17a0d6f397b98ead4177fe19f71f31ae19 |
| [experimets/cards/P97-learned-graph-order.md](../experimets/cards/P97-learned-graph-order.md) | 2 | — | ed22fc3cc4762337a814beff9b57f11d3eddee28a9a78b2edce25a03b1a40920 | — |
| [experimets/cards/P98-tokenizer-capacity.md](../experimets/cards/P98-tokenizer-capacity.md) | 2 | — | 860793ab393009d7301a1a7cb2a3716f142aa65b9d6d299575b347558f234713 | — |
| [experimets/cards/P99-unknown-token-id.md](../experimets/cards/P99-unknown-token-id.md) | 2 | — | 3ef7a93e0ded7a7c7bc9983e05a94db0a0af2c5712f00d4a9a985ccfae0389e5 | — |
| [experimets/cards/ROPE-TRAIN-TABLE-01.md](../experimets/cards/ROPE-TRAIN-TABLE-01.md) | 1 | 97494d09a3e2239b2468aa6ce099e8d038513da95955862399389aad38ce450a | — | — |
| [experimets/cards/SILU-SIGMOID-REUSE-01.md](../experimets/cards/SILU-SIGMOID-REUSE-01.md) | 1 | f508b86628d905a71469be7e93a82cbfc2e18e4c815baa80a33b01be17fff062 | — | — |
| [experimets/cards/WIKI-FEATURE-KEYS-01.md](../experimets/cards/WIKI-FEATURE-KEYS-01.md) | 1 | dad4d46d7e92c9c4e14872a3a7b2fdb7f9b202d0a8354a6eab57af2e3ba9fd2a | — | — |
| [experimets/cards/WIKI-FEATURES-ONCE-01.md](../experimets/cards/WIKI-FEATURES-ONCE-01.md) | 1 | 0c3c23268322437829d7ea4a784135b63075e48db4a5d46773fbf5d659ff4f27 | — | — |
| [experimets/cards/WIKI-MARGIN-01.md](../experimets/cards/WIKI-MARGIN-01.md) | 2 | — | d33ad58f7a9619d7da7dbdfd1cc896247724027276520930d85bedc424edaf1a | — |
| [experimets/cards/WIKI-TAXONOMY-01.md](../experimets/cards/WIKI-TAXONOMY-01.md) | 2 | — | e6cc194cc7e5993ec324069044f9d6de8cee14aabe733333849b6d7e83dfe1e4 | — |
| [fixes-based-on-experiments/03-test-and-execution-contract.md](03-test-and-execution-contract.md) | 1 | bf2b5cdf029344d6a699fbd40e1d5abe5e5dd5e16fad2c19d3d81e6e87e9db13 | — | — |
| [fixes-based-on-experiments/114-post88-addressed-repair-plan-2026-11-01.md](114-post88-addressed-repair-plan-2026-11-01.md) | 1 | 7150e2fa1b891ff051fc06c3a42aa20318633576eac9b44fc4c1c98b4a4a02be | — | — |
| [fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md](118-post101-addressed-repair-plan-2026-11-02.md) | 1 | c754e1f053295441e3465d090ba9b3b13db9d2fb987c2682d4c87ada8a93c7f1 | — | — |
| [fixes-based-on-experiments/12-bpe-no-match-copy.md](12-bpe-no-match-copy.md) | 1 | 4717d80cb8b8a19872d880715b18f288469ef9a7001095d5ac1cbac482b15788 | — | — |
| [fixes-based-on-experiments/13-dpo-reference-cache.md](13-dpo-reference-cache.md) | 1 | 8d01abe9543cdd5d16964144ad998e4f5ad64f8331050930012cc13f0ebb71c0 | — | — |
| [fixes-based-on-experiments/16-replay-domain-retention.md](16-replay-domain-retention.md) | 1 | fdbeb3fe6d3ddc26c7466a7ae7090eedef3b45a6d82df1fed544449ab7f278d8 | — | — |
| [fixes-based-on-experiments/18-contextual-dictionary-learning-coverage.md](18-contextual-dictionary-learning-coverage.md) | 1 | d490485c12cc84129d23615998fbc5f08fb5f63cc5c8421e27006ad6cc5fcc7a | — | — |
| [fixes-based-on-experiments/21-sft-optimizer-continuity.md](21-sft-optimizer-continuity.md) | 3 (1) | — | — | d69e32c076597b10dbbca078b765a619629d398105f523e6df6adc9e0c1a3890 |
| [fixes-based-on-experiments/22-dpo-valid-length.md](22-dpo-valid-length.md) | 3 (1) | — | — | d57a3d3ff7689b38df245711c2b6ee1115d4cfe307084111e7960bf9272163ac |
| [fixes-based-on-experiments/23-sft-response-mask.md](23-sft-response-mask.md) | 3 (1) | — | — | e0db6bb7a06cd6fdde9e3bfc56ad9d9149afaa879b21949acfb697c4945040b3 |
| [fixes-based-on-experiments/27-predicate-dataset-router-learning-coverage.md](27-predicate-dataset-router-learning-coverage.md) | 1 | 5806d911409f8f9ebc6cf2fa2bd0bbf36d8ba29c00fe036bc191e3ffeff00763 | — | — |
| [fixes-based-on-experiments/28-dpo-tokenizer-snapshot.md](28-dpo-tokenizer-snapshot.md) | 2 | — | a8202b44e5efbca569369fbd2cd85ed930a6a6b223f20d476d0e0eaad1478a56 | — |
| [fixes-based-on-experiments/29-matmul-right-gradient-locality.md](29-matmul-right-gradient-locality.md) | 1 | f1b21bff48b515624c91ab2f93692b19ac2a32144db4082e1243f88a40f9e248 | — | — |
| [fixes-based-on-experiments/30-broadcast-coordinate-allocation.md](30-broadcast-coordinate-allocation.md) | 1 | 19a91cc7bcde4eff657925b5c5694adf47c2c797c46227b2b0756a17ce81b840 | — | — |
| [fixes-based-on-experiments/31-bpe-weighted-chunks.md](31-bpe-weighted-chunks.md) | 1 | 47d8a66351abeb171bc570f547f775a3f752328da1b2dc1ab332f31eb8b01791 | — | — |
| [fixes-based-on-experiments/34-pos-bigram-learning-coverage.md](34-pos-bigram-learning-coverage.md) | 1 | cfec667b34fa5c0f276450287f192b81ad478601ba5dede6511434e3dfb7d4bc | — | — |
| [fixes-based-on-experiments/36-dpo-scored-row-head.md](36-dpo-scored-row-head.md) | 1 | 26a5618b9fc6c1fc49e5834840e6965694d4cd86cf560ea203f25a6ff34041c6 | — | — |
| [fixes-based-on-experiments/37-structural-knn-learning-coverage.md](37-structural-knn-learning-coverage.md) | 1 | b7b3e619af803e08e4563ed3ffd1e217dfb9efe53eb55a763def45ffd2568ee4 | — | — |
| [fixes-based-on-experiments/38-structural-knn-sibling-cache.md](38-structural-knn-sibling-cache.md) | 1 | dbcd61f5ad47656594edbcfeab8fde18ca49fe20c7e4c719046487452b70707f | — | — |
| [fixes-based-on-experiments/39-null-guard-training-label.md](39-null-guard-training-label.md) | 1 | 3c6fdc5460359a7b454f96dd2c88025bccd8bf98813faec775c41079780a101b | — | — |
| [fixes-based-on-experiments/40-bpe-realized-merge-objective.md](40-bpe-realized-merge-objective.md) | 1 | e5501f93f8ad7eb8e749784aec4697b906e035e377adffe0032da7ca5978c274 | — | — |
| [fixes-based-on-experiments/41-grad-accum-copy.md](41-grad-accum-copy.md) | 2 | — | 9042e83508680ca8d2b24b6d62b6ff093371a0f9cb6e66201f42594cf96c7b63 | — |
| [fixes-based-on-experiments/42-frozen-broadcast-map.md](42-frozen-broadcast-map.md) | 1 | e374ed4ce110e689408190a93d1ea511881721dbad1784b9f80acd3aeacde0c3 | — | — |
| [fixes-based-on-experiments/44-rms-row-reduce.md](44-rms-row-reduce.md) | 1 | e342a34d2c23f53ccd00535d4d5b8a73c6e20ffaec205e939ec7551d99f2e8db | — | — |
| [fixes-based-on-experiments/45-attention-transpose-strides.md](45-attention-transpose-strides.md) | 2 | — | 9951a339022b8f8e06175d5753502d76c51e7c03efa0890503ffb761c5034123 | — |
| [fixes-based-on-experiments/46-training-json-options-reuse.md](46-training-json-options-reuse.md) | 1 | ae14d843a25b68bae76edb7e240d57e7f4bd8433775bbecf99be4796e5cb01ef | — | — |
| [fixes-based-on-experiments/47-silu-sigmoid-reuse.md](47-silu-sigmoid-reuse.md) | 1 | 4357b2a99b9cec828b1a1d27b4e388dd62fb4ca196246fa2805d488e1deda258 | — | — |
| [fixes-based-on-experiments/48-learned-route-body-identity.md](48-learned-route-body-identity.md) | 1 | e722a96aef9ce303ef1aa7b9e05061d8fb1d62b68c3db0f625381a7c0dfc20a2 | — | — |
| [fixes-based-on-experiments/49-router-learned-key-normalization.md](49-router-learned-key-normalization.md) | 1 | 2dc4d7beb265f692ddbe53b7f4b4c02c4eed39d81239cc7d3a01e8587dd23167 | — | — |
| [fixes-based-on-experiments/51-pos-query-bigram-reuse.md](51-pos-query-bigram-reuse.md) | 2 | — | f82c032a6cc11ce34f73cd38f1a2c41c6893aa43798c9c1724729553e89df603 | — |
| [fixes-based-on-experiments/52-translation-training-identity-index.md](52-translation-training-identity-index.md) | 1 | 8a2bdc03115bcf4620a937abd6f6a189ebc0de5aa21851fadd7c7bbe38891d74 | — | — |
| [fixes-based-on-experiments/53-hierarchical-local-history.md](53-hierarchical-local-history.md) | 1 | 6b5bec7d5af7584628657e5e10571e81c814da3991e9c9561dd255b1af527a4a | — | — |
| [fixes-based-on-experiments/54-frozen-base-check-scope.md](54-frozen-base-check-scope.md) | 3 (1) | — | — | 9602adf57f3086df4ad24aa05f881d8332487e0ad17027a33b1a09470ee192dd |
| [fixes-based-on-experiments/55-frozen-linear-transpose-cache.md](55-frozen-linear-transpose-cache.md) | 3 (1) | — | — | 2ce485686bb9fe68e2b7a1b6c2fb36b6e2e17df5e788a03f0acccd25dc0f7667 |
| [fixes-based-on-experiments/57-native-transfer-contract-gaps.md](57-native-transfer-contract-gaps.md) | 3 (1) | — | — | 02fab684daf8539473f6445378bfb6d9747608314777a2fb10293f07a6d300b6 |
| [fixes-based-on-experiments/62-supervised-framing-and-learning-readout.md](62-supervised-framing-and-learning-readout.md) | 1 | 3c945542d20512b85c56f9f4972fda3900049d4889afdbb9e1a03198a093ae94 | — | — |
| [fixes-based-on-experiments/83-learned-bpe-boundary-transfer-addendum.md](83-learned-bpe-boundary-transfer-addendum.md) | 1 | b8a6c9b36274dd4bb6736ec9397fea0738d639c4618568c52a2b8fade5b1e885 | — | — |
| [fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md](codex-127-postmerge-addressed-repair-plan-2026-11-02.md) | 1 | a2eec39f981d0728b0c9deb0fd4fbf94e1ea3dd773bd102fa1391784900aad77 | — | — |

