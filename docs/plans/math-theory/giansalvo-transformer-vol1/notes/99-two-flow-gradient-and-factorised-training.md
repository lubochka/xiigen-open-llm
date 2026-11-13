---
title: "99. Два ортогональных потока и обучение факторов Q/K"
source_pdf_pages: [347, 350]
book_pages: [311, 314]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 99. Два ортогональных потока и обучение факторов Q/K

[Индекс](../README.md) · [Theorem 10.9](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#theorem-10-9) · [Remark 10.10](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#remark-10-10) · [Граница пакета](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#batch-007-boundary)

## Точный смысл теоремы о двух потоках

Пусть $G_M=\partial\mathcal L/\partial M$ и

$$
P_s(G)=\frac{G+G^\top}{2},\qquad
P_a(G)=\frac{G-G^\top}{2}.
$$

**Theorem 10.9** рассматривает шаг

$$
M^+=M-\eta G_M+O(\eta^2).
$$

Линейность проекций даёт

$$
M_s^+-M_s=-\eta P_s(G_M)+O(\eta^2),\qquad
M_a^+-M_a=-\eta P_a(G_M)+O(\eta^2).
$$

Для обычного точного gradient-descent step по свободной матрице $M$ эти равенства точны без остатка; $O(\eta^2)$ допускает более общую аппроксимацию шага. Ортогональность относится к матричным подпространствам при Frobenius inner product.

**Remark 10.10** правильно предупреждает, что это не независимые процессы всего Transformer: обе части участвуют в общей loss, softmax, residual stream и следующих слоях. Даже для свободного $M$ градиенты двух компонент могут зависеть друг от друга через loss.

## Обучение \(W_Q,W_K\) индуцирует другую скорость \(M\)

Для $M=W_QW_K^\top$ и loss, проходящей через этот продукт,

$$
G_Q=G_MW_K,\qquad G_K=G_M^\top W_Q.
$$

Одновременный SGD step факторов с learning rate $\eta$ даёт редакторское разложение

$$
M^+-M
=-\eta H+\eta^2G_QG_K^\top,
\qquad
H=G_MW_KW_K^\top+W_QW_Q^\top G_M.
$$

Следовательно, фактические первые скорости S/A-компонент равны $-P_s(H)$ и $-P_a(H)$, а **не в общем случае** $-P_s(G_M)$ и $-P_a(G_M)$.

Ортогонально разложить можно и $H$, но для этого надо сначала вычислить именно induced flow. Внешний Frobenius-градиент $G_M$ и индуцированная скорость — разные объекты. Последний абзац PDF 350 не устраняет это различие одним названием “effective canonical-pair flow”.

## Проверяемый контрпример

Возьмём $d=2,d_k=1$:

$$
W_Q=e_1,\qquad W_K=e_2,\qquad
G_M=\begin{pmatrix}1&-1\\-1&1\end{pmatrix}.
$$

Матрица $G_M$ симметрична, поэтому $G_a=0$. Однако

$$
H=\begin{pmatrix}1&-2\\0&1\end{pmatrix},
\qquad
P_a(H)=\begin{pmatrix}0&-1\\1&0\end{pmatrix}\ne0.
$$

Обучение факторов меняет $M_a$ уже в первом порядке, несмотря на нулевой $G_a$. При $X=I_2$ выбранный $G_M$ имеет нулевые суммы строк, так что он совместим с tangent constraint softmax pullback. Это не произвольный пример с запрещённым направлением score-gradient.

Также факторизация накладывает $\operatorname{rank}M\le d_k$. Свободный шаг по $M$ может выйти из этого класса; сохранение произведения факторов и произвольная оптимизация матрицы не являются одной архитектурной операцией.

## Как читать заявления о росте энергии

Для прямого потока $\dot M=-G_M$:

$$
\frac{d}{dt}\lVert M_a\rVert_F^2=-2\langle M_a,G_a\rangle_F.
$$

Знак изменения энергии определяется **согласованием** $M_a$ и $G_a$, а не просто наличием или «знаком» матричного $G_a$. Для факторного потока вместо $G_a$ в этой формуле стоит $P_a(H)$. Momentum, weight decay, адаптивные preconditioners и дополнительные loss terms требуют своих полных правил обновления.

Это уточнение локального механизма, а не результат об обучении конкретного checkpoint или гарантированном движении signature geography.

## Точная граница чтения

На PDF 350 полностью закончены доказательство Theorem 10.9 и Remark 10.10. Затем начинается **§10.9.1**: определён parameter-side balance

$$
\alpha_{\mathrm{param}}
=\frac{\lVert M_a\rVert_F^2}{\lVert M_s\rVert_F^2+\lVert M_a\rVert_F^2},\qquad M\ne0.
$$

Вступительный абзац закончен, но подраздел **не завершён**. Объявленный следующий corollary находится за границей пакета и здесь не пересказывается. Продолжение — **PDF 351 / книга 315, §10.9.1**; следующий диапазон — **PDF 351–400 / книга 315–364**.


## Продолжение в проходе 008

[Карточка 100](100-parameter-balance-evolution-and-induced-flow.md) разбирает Corollary 10.11; [110](110-skew-gradient-flow-and-symplectic-eigenplanes.md) проверяет skew-flow главы 11. Дополнительные ограничения residual amplification и корректный градиент по X — в [карточке 117](117-residual-amplification-and-alignment-conditions.md).
