---
title: "356. Appendix A: rank, SVD, нормы и корректное поле eigenvectors"
source_pdf_pages: [792, 793]
book_pages: [756, 757]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 356. Appendix A: rank, SVD, нормы и корректное поле eigenvectors

## Вопрос

Какие базовые линейно-алгебраические определения нужны, чтобы правильно читать rank и residual diagnostics?

**Приложение A собирает точные инструменты, но их математические определения нужно отделять от численных approximations.** [§§A.1.1–A.1.2](../source/0792-0796-appendix-a-mathematical-background.md#section-a-1), [SVD, Schatten и pseudoinverse](../source/0792-0796-appendix-a-mathematical-background.md#pdf-0793).

## Три разных способа измерить матрицу

Для singular values $\sigma_i(M)$:

$$
\|M\|_{S_1}=\sum_i\sigma_i,\qquad
\|M\|_F=\Bigl(\sum_i\sigma_i^2\Bigr)^{1/2},\qquad
\|M\|_2=\max_i\sigma_i.
$$

Первая норма суммирует все amplitudes, вторая измеряет суммарную squared energy, третья — самый большой linear amplification. Порог в одной норме нельзя переносить в другую без размерностного коэффициента или дополнительного rank bound. Например, при rank $r$ имеем $\|M\|_2\le\|M\|_F\le\sqrt r\|M\|_2$ непосредственно из суммы квадратов.

Математический rank считает все ненулевые singular values; численный diagnostic с threshold — отдельное определение. Смена масштаба, относительный/абсолютный threshold и precision должны быть указаны.

## Pseudoinverse и проекторы

$MM^\dagger$ проецирует на column space, $M^\dagger M$ — на row space. Это разные пространства и обычно разные размеры. Утверждение $\operatorname{tr}\Pi=\dim\operatorname{im}\Pi$ относится к проектору; для ортогонального проектора дополнительно $\Pi=\Pi^\top$. В вычислениях pseudoinverse с truncated SVD может задавать другой, численно выбранный subspace.

## Уточнение eigenvalue convention

В §A.1.3 напечатано $v\in\mathbb R^d$, $\lambda\in\mathbb C$. Для вещественной матрицы и **невещественного** eigenvalue ненулевой eigenvector должен рассматриваться в $\mathbb C^d$: если $v$ вещественен, равенство $Mv=\lambda v$ заставляет imaginary part $\lambda$ быть нулевой. Симметричный случай ниже корректно даёт real eigenvalues и real orthonormal eigenbasis.

Для связи с билинейной формой надо также соблюдать порядок аргументов: при $M\in\mathbb R^{m\times n}$, $x\in\mathbb R^n$, $y\in\mathbb R^m$ скаляр можно записать как $y^\top Mx$. Размерности снимают неоднозначность словесного “bilinear form on $\mathbb R^n\times\mathbb R^m$”.

