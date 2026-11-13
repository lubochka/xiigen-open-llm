---
title: "359. Weyl, Wedin и compressed sensing: какие условия нельзя убрать при переносе на heads"
source_pdf_pages: [795, 796]
book_pages: [759, 760]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 359. Weyl, Wedin и compressed sensing: какие условия нельзя убрать при переносе на heads

## Вопрос

Дают ли perturbation bounds и RIP готовую гарантию выбора нужного числа голов?

**Weyl контролирует eigenvalues; устойчивость subspaces требует spectral separation. Compressed sensing гарантирует конкретную линейную sparse recovery задачу при RIP, а архитектурный перенос требует отдельного соответствия объектов.** [§§A.6–A.7](../source/0792-0796-appendix-a-mathematical-background.md#section-a-6).

## Eigenvalues против eigenvectors

Формула (A.13) сохраняет важный модуль:

$$
|\lambda_k(A+B)-\lambda_k(A)|\le\|B\|_2
$$

для вещественных симметричных матриц с согласованным порядком eigenvalues. Она не ограничивает поворот individual eigenvectors при почти кратных eigenvalues. В вырожденном eigenspace basis вообще не единственен.

Wedin-type estimates для углов singular subspaces требуют separation между интересующей spectral group и её дополнением. Поэтому малая абсолютная perturbation сама по себе не означает малый относительный поворот, если gap ещё меньше. Для controller нужно отдельно записывать величину perturbation, внешний gap, cluster policy и выбранную norm.

Это согласуется с осторожностью при deterministic spectral pre-pass: воспроизводимый subspace и воспроизводимый ordered basis — разные задачи.

## Что именно утверждает sparse recovery

В (A.14) наблюдение имеет вид $y=\Phi x_0$, $x_0$ — точно $s$-sparse, измерения noiseless, а $\delta_{2s}<\sqrt2-1$ — достаточная RIP-гипотеза для точного решения $\ell_1$ program. Если measurements шумные, sparsity approximate или sensing map обучается вместе с target, написанная задача уже не описывает весь процесс.

Чтобы связать её с head count, надо определить:

- какие coefficients играют роль $x_0$;
- что является linear sensing operator $\Phi$;
- какую dictionary и sparsity convention используют;
- почему выполняется RIP;
- как recovery error связан с модельным выходом и loss.

Слова “inputs play the role of measurements” и “head count plays the role of sparsity” обозначают аналогию, но не выполняют эти проверки. Нелинейные softmax/gates и изменение dictionary при growth особенно требуют явного обоснования. Утверждение Appendix A о переносе на INCRT остаётся ссылкой на условия основной главы, а не новым безусловным доказательством.

Авторские bibliographic notes на PDF 796 перенесены; никаких новых recovery experiments в этом проходе нет.

