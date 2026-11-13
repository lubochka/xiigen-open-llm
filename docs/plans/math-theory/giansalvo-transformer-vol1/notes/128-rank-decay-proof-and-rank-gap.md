---
title: "128. Effective rank после проекции: контрпример и пробелы layer-rank proof"
source_pdf_pages: [412, 413]
book_pages: [376, 377]
batch: "009"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 128. Effective rank после проекции: контрпример и пробелы layer-rank proof

[Индекс](../README.md) · [§11.15.3](../source/0408-0414-ch11-proofs-and-bibliographic-notes.md#section-11-15-3) · [Lemma 11.76](../source/0408-0414-ch11-proofs-and-bibliographic-notes.md#lemma-11-76) · [(11.45)](../source/0408-0414-ch11-proofs-and-bibliographic-notes.md#equation-11-45)

## Определение надо сохранить вместе с числом

На PDF 412 используется entropy effective rank

$$
r_{\mathrm{eff}}(W)=\exp\left(-\sum_kp_k\log p_k\right),
\qquad p_k=\frac{\sigma_k(W)}{\sum_j\sigma_j(W)}.
$$

Он определён для ненулевой матрицы и нормирует **singular values**, а не их квадраты. Это не обычный algebraic rank и не participation ratio. При $W=0$ нужна отдельная договорённость. Ранее использованные в книге показатели нельзя сравнивать, не фиксируя их формулу; см. [карточку 118](118-layerwise-correlation-and-effective-rank-claims.md).

## Lemma 11.76 опровергается диагональным примером

Книга утверждает $r_{\mathrm{eff}}(WP)\le r_{\mathrm{eff}}(W)$ для любого orthogonal projector $P$. Возьмём

$$
W=\operatorname{diag}(100,1,1),\qquad
P=\operatorname{diag}(0,1,1).
$$

До проекции нормированный спектр равен $(100,1,1)/102$, после проекции — $(0,1/2,1/2)$. Поэтому

$$
r_{\mathrm{eff}}(W)=1.1163897953\ldots<2=r_{\mathrm{eff}}(WP).
$$

Обычный ранг уменьшается с 3 до 2, но **равномерность оставшегося нормированного спектра возрастает**, поэтому entropy effective rank увеличивается. Это контрпример даже для проектора, согласованного с singular vectors. В общем положении singular values $WP$ к тому же не обязаны быть простым подмножеством значений $W$.

Ссылка §11.16 на data-processing inequality не исправляет лемму. Удаление dominant mode с последующей перенормировкой спектра — не тот же объект, что фиксированное отображение случайной величины без conditioning. Нельзя переносить информационное неравенство на новую спектральную нормировку без отдельного доказательства.

## Низкая attention entropy не означает низкий ранг

При uniform attention $A=\mathbf1\mathbf1^\top/n$ row entropy максимальна, но rank равен 1. При $A=I_n$ row entropy равна нулю, а rank равен $n$. Для $X=I_n$ и $W_V=I_n$ то же сравнение относится к $Y=AXW_V$. Можно взять непрерывное семейство

$$
A_t=(1-t)\mathbf1\mathbf1^\top/n+tI_n,\qquad0\le t\le1.
$$

По мере $t\uparrow1$ строки становятся более концентрированными, а разные строки выбирают разные позиции. Общий ранг не обязан падать. Значит, переход от уменьшения row entropy к [(11.44)](../source/0408-0414-ch11-proofs-and-bibliographic-notes.md#equation-11-44) неверен без условий о **совпадении выбранных подпространств между строками** и о value geometry.

## Градиент выходной проекции: размеры и история

Пусть $Y_{\mathrm{cat}}=[Y^{(1)}\ \cdots\ Y^{(H)}]\in\mathbb R^{n\times Hd_v}$, $W_O\in\mathbb R^{Hd_v\times d}$ и $Z=Y_{\mathrm{cat}}W_O$. Тогда

$$
\nabla_{W_O}\mathcal L=Y_{\mathrm{cat}}^\top\nabla_Z\mathcal L,
\qquad
\nabla_{W_O^{(h)}}\mathcal L=(Y^{(h)})^\top\nabla_Z\mathcal L.
$$

Headwise gradients образуют блоки полного gradient, а не сумму одинаковых $d_v\times d$ матриц для общего $Hd_v\times d$ параметра. [Исходная формула](../source/0408-0414-ch11-proofs-and-bibliographic-notes.md#display-0413-output-gradient) смешивает эти уровни; странная запись $\operatorname{span}\{Y^{(h,l)}\}^{\perp\top}$ сохранена буквально.

Для фиксированного $Y$ градиент ограничен соответствующим подпространством входов проекции. При меняющемся $Y_t$ сумма updates зависит от объединения подпространств во времени. Выходные векторы $Y^{(h)}W_O^{(h)}$ и строки $Y^{(h)}$ принадлежат различным пространствам; одну размерность нельзя автоматически подставлять вместо другой.

Без weight decay не обучаемая компонента инициализации сохраняется. Даже при decay надо задать его тип, коэффициент и режим сходимости. Масштаб **элементов** $O(1/\sqrt d)$ не означает такой же масштаб singular values всей случайной матрицы: большое число малых элементов может давать spectral values порядка единицы.

## Последняя цепочка обращает установленное неравенство

Текст перед (11.45) утверждает $r_{\mathrm{eff}}(W_O^{(l)})\lesssim\dim V_{\mathrm{sig}}^{(l)}$. Но для заключения цепочка использует в конце обратное направление:

$$
\dim V_{\mathrm{sig}}^{(l)}\lesssim r_{\mathrm{eff}}(W_O^{(l)}).
$$

Такой lower bound не доказан. Даже два корректных upper bounds $a\le A$, $b\le B$ и $A\le B$ не дают $a\le b$. Помимо этого, уменьшение числа голов $H_l$ в Step 3 вводится без вывода из предыдущих условий; число голов и attention entropy обозначают разные величины.

**Практический статус:** entropy effective rank полезен как измеряемая спектральная характеристика. Proposition 11.41 и Conjecture 11.65 нельзя использовать как универсальную гарантию монотонного сокращения обучаемой сложности. Для каждого слоя стоит измерять отдельно rank и спектр представлений, head outputs и $W_O$, сохраняя точное определение effective rank. Приведённый контрпример — редакторская математическая проверка, а не результат обучения модели.
