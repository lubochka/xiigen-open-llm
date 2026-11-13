---
title: "Chapter 16: rank-deflation proof and the count–rank invariant"
source_pdf_pages: [651, 654]
book_pages: [615, 618]
batch: "014"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: rank-deflation proof and the count–rank invariant

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: Lemma 16.57 begins](0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md#lemma-16-57) · [Next: rank prescription](0655-0657-ch16-rank-prescription-and-identifiability-introduction.md)

> Editorial note: Full transcription of PDF 651–654 (printed pages 615–618). This file continues Lemma 16.57 from PDF 650 without creating a second lemma entry. Its equality-condition wording, the operator-norm remark, the cluster-threshold limits, “product” next to a sum, and placeholder references are preserved literally. Editorial mathematical checks appear separately in notes 260–263.

<a id="pdf-0651"></a>
## PDF 651 · Book page 615

<a id="continuation-lemma-16-57"></a>
### Continuation of Lemma 16.57

**(i) Energy reduction.**

<a id="equation-16-82"></a>
$$
\|A_{\mathrm{res}}\|_F^2-\|A_{\mathrm{res}}^{(\mathrm{new})}\|_F^2
=\sum_{i=1}^r\lambda_i^2.
\tag{16.82}
$$

**(ii) Optimality among rank-$r$ orthogonal projections.** For every orthogonal projector $P\in\mathbb R^{d\times d}$ of rank $r$ (equivalently, $P=P^\top=P^2$ with $\operatorname{tr}(P)=r$), the projector deflation $\widetilde A:=A_{\mathrm{res}}-PA_{\mathrm{res}}P$ satisfies

<a id="equation-16-83"></a>
$$
\|A_{\mathrm{res}}\|_F^2-\|\widetilde A\|_F^2
\le\sum_{i=1}^r\lambda_i^2,
\tag{16.83}
$$

with equality if and only if $\operatorname{range}(P)$ is a leading $r$-dimensional invariant subspace of $A_{\mathrm{res}}$ (i.e. $\operatorname{range}(P)=\operatorname{span}(u_1,\ldots,u_r)$ when $\lambda_1,\ldots,\lambda_r$ are simple, or any $r$-dimensional subspace of the union of eigenspaces $\bigoplus_{i=1}^r\ker(A_{\mathrm{res}}-\lambda_i I)$ in the degenerate case).

**(iii) Residual structure.** $A_{\mathrm{res}}^{(\mathrm{new})}$ is symmetric positive semi-definite, and its top eigenvalue equals $\lambda_{r+1}$ (with the convention $\lambda_{d+1}=0$).

*Proof.* The spectral decomposition asserted in the hypothesis is the standard symmetric eigenvalue decomposition; its existence and the orthonormality of $\{u_i\}$ follow from the spectral theorem for symmetric matrices. The Frobenius-norm identity used throughout is

<a id="equation-16-84"></a>
$$
\|M\|_F^2=\operatorname{tr}(M^\top M)=\operatorname{tr}(M^2)
=\sum_i\mu_i^2
\qquad\text{for symmetric }M\text{ with eigenvalues }\mu_i,
\tag{16.84}
$$

which uses $M^\top=M$ in the second equality and the trace-of-eigenvalues identity in the third.

**Proof of (i).** By construction, $A_{\mathrm{res}}^{(\mathrm{new})}=\sum_{i=r+1}^d\lambda_i u_i u_i^\top$ is symmetric, with eigenvalues $\lambda_{r+1},\ldots,\lambda_d$ on the eigenvectors $u_{r+1},\ldots,u_d$ together with $r$ zero eigenvalues on $\operatorname{span}(u_1,\ldots,u_r)$. Identity (16.84) applied to $A_{\mathrm{res}}$ and to $A_{\mathrm{res}}^{(\mathrm{new})}$ gives

<a id="display-0651-original-and-deflated-energies"></a>
$$
\|A_{\mathrm{res}}\|_F^2=\sum_{i=1}^d\lambda_i^2,\qquad
\|A_{\mathrm{res}}^{(\mathrm{new})}\|_F^2=\sum_{i=r+1}^d\lambda_i^2,
$$

and subtraction yields (16.82).

**Proof of (ii).** The proof has two steps. First, we establish the identity

<a id="equation-16-85"></a>
$$
\|A_{\mathrm{res}}\|_F^2-\|\widetilde A\|_F^2
=\|PA_{\mathrm{res}}P\|_F^2,
\tag{16.85}
$$

valid for every orthogonal projector $P$. Then we maximise the right-hand side over $P$ of rank $r$.

*Step 1 (the identity (16.85)).* Expand $\widetilde A=A_{\mathrm{res}}-PA_{\mathrm{res}}P$ in the trace:

<a id="equation-16-86"></a>
$$
\begin{aligned}
\|\widetilde A\|_F^2
&=\operatorname{tr}\bigl((A_{\mathrm{res}}-PA_{\mathrm{res}}P)^2\bigr)\\
&=\operatorname{tr}(A_{\mathrm{res}}^2)
-2\operatorname{tr}(A_{\mathrm{res}}PA_{\mathrm{res}}P)
+\operatorname{tr}\bigl((PA_{\mathrm{res}}P)^2\bigr).
\end{aligned}
\tag{16.86}
$$

The middle trace simplifies by cyclicity and idempotency $P^2=P$. Inserting the resolution of identity $I=P+(I-P)$:

<a id="equation-16-87"></a>
$$
\begin{aligned}
\operatorname{tr}(A_{\mathrm{res}}PA_{\mathrm{res}}P)
&=\operatorname{tr}(PA_{\mathrm{res}}PA_{\mathrm{res}})\\
&=\operatorname{tr}\bigl(PA_{\mathrm{res}}PA_{\mathrm{res}}(P+(I-P))\bigr)\\
&=\operatorname{tr}(PA_{\mathrm{res}}PA_{\mathrm{res}}P)
+\operatorname{tr}\bigl(PA_{\mathrm{res}}PA_{\mathrm{res}}(I-P)\bigr)\\
&=\operatorname{tr}\bigl((PA_{\mathrm{res}}P)^2\bigr)
+\operatorname{tr}\bigl((I-P)PA_{\mathrm{res}}PA_{\mathrm{res}}\bigr)\\
&=\operatorname{tr}\bigl((PA_{\mathrm{res}}P)^2\bigr)+0
=\operatorname{tr}\bigl((PA_{\mathrm{res}}P)^2\bigr),
\end{aligned}
\tag{16.87}
$$

<a id="pdf-0652"></a>
## PDF 652 · Book page 616

where the third line uses cyclicity, the fourth uses cyclicity again to move $(I-P)$ to the front, and the cancellation in the fifth line uses $(I-P)P=P-P^2=P-P=0$ from idempotency. The final trace is $\|PA_{\mathrm{res}}P\|_F^2$ by (16.84) applied to the symmetric matrix $PA_{\mathrm{res}}P$. Substituting (16.87) into (16.86):

<a id="equation-16-88"></a>
$$
\|\widetilde A\|_F^2
=\|A_{\mathrm{res}}\|_F^2-2\|PA_{\mathrm{res}}P\|_F^2
+\|PA_{\mathrm{res}}P\|_F^2
=\|A_{\mathrm{res}}\|_F^2-\|PA_{\mathrm{res}}P\|_F^2,
\tag{16.88}
$$

giving (16.85).

*Step 2 (maximising $\|PA_{\mathrm{res}}P\|_F^2$).* Write $P=QQ^\top$ with $Q\in\mathbb R^{d\times r}$ having orthonormal columns ($Q^\top Q=I_r$). Then by cyclicity of the trace and $Q^\top Q=I_r$:

<a id="equation-16-89"></a>
$$
\begin{aligned}
\|PA_{\mathrm{res}}P\|_F^2
&=\operatorname{tr}\bigl((QQ^\top A_{\mathrm{res}}QQ^\top)^2\bigr)\\
&=\operatorname{tr}\bigl(QQ^\top A_{\mathrm{res}}QQ^\top
\cdot QQ^\top A_{\mathrm{res}}QQ^\top\bigr)\\
&=\operatorname{tr}\bigl(Q^\top A_{\mathrm{res}}Q\cdot Q^\top Q
\cdot Q^\top A_{\mathrm{res}}Q\cdot Q^\top Q\bigr)\\
&=\operatorname{tr}\bigl((Q^\top A_{\mathrm{res}}Q)^2\bigr)
=\|Q^\top A_{\mathrm{res}}Q\|_F^2.
\end{aligned}
\tag{16.89}
$$

The matrix $B:=Q^\top A_{\mathrm{res}}Q\in\mathbb R^{r\times r}$ is symmetric (because $A_{\mathrm{res}}$ is) and positive semi-definite (because $A_{\mathrm{res}}\succeq0$ implies $Q^\top A_{\mathrm{res}}Q\succeq0$). Its eigenvalues, denoted $\mu_1\ge\mu_2\ge\cdots\ge\mu_r\ge0$, satisfy by (16.84)

<a id="display-0652-compressed-matrix-energy"></a>
$$
\|B\|_F^2=\sum_{j=1}^r\mu_j^2.
$$

By the Cauchy interlacing theorem for symmetric matrices, applied to $B=Q^\top A_{\mathrm{res}}Q$ as a $(d-r)$-deficient compression of $A_{\mathrm{res}}$,

<a id="equation-16-90"></a>
$$
\lambda_j\ge\mu_j\ge\lambda_{d-r+j}
\qquad\text{for }j=1,\ldots,r.
\tag{16.90}
$$

Since all $\lambda_j$ and $\mu_j$ are non-negative and squaring preserves the order on $[0,\infty)$, $\mu_j\le\lambda_j$ gives $\mu_j^2\le\lambda_j^2$, and summation yields

<a id="equation-16-91"></a>
$$
\|PA_{\mathrm{res}}P\|_F^2=\|B\|_F^2
=\sum_{j=1}^r\mu_j^2\le\sum_{j=1}^r\lambda_j^2.
\tag{16.91}
$$

Combining (16.85) and (16.91) gives (16.83).

*Equality condition.* Equality in (16.83) forces $\sum_{j=1}^r\mu_j^2=\sum_{j=1}^r\lambda_j^2$, which by $\mu_j\in[0,\lambda_j]$ for each $j$ requires $\mu_j=\lambda_j$ for every $j=1,\ldots,r$.

The condition $\mu_1=\lambda_1$ is, by the variational characterisation $\mu_1=\max_{x\in\operatorname{range}(Q),\,\|x\|=1}x^\top A_{\mathrm{res}}x$, equivalent to the existence of a unit vector $x_1\in\operatorname{range}(Q)\cap\ker(A_{\mathrm{res}}-\lambda_1 I)$. Iterating: requiring $\mu_j=\lambda_j$ for all $j$ forces $\operatorname{range}(Q)$ to contain $r$ orthonormal vectors, the $j$-th of which lies in $\ker(A_{\mathrm{res}}-\lambda_j I)$. Equivalently, $\operatorname{range}(Q)$ is contained in $\bigoplus_{i=1}^r\ker(A_{\mathrm{res}}-\lambda_i I)$, and dimensional matching ($\dim\operatorname{range}(Q)=r$) forces $\operatorname{range}(Q)$ to equal an $r$-dimensional subspace of this direct sum.

When $\lambda_1,\ldots,\lambda_r$ are simple, the kernels $\ker(A_{\mathrm{res}}-\lambda_i I)$ are each one-dimensional, and the unique $r$-dimensional subspace of their direct sum is $\operatorname{span}(u_1,\ldots,u_r)$. In the degenerate case (some eigenvalues repeated), any $r$-dimensional subspace of the eigenspace direct sum gives equality. This characterises $\operatorname{range}(P)$ as a leading $r$-dimensional invariant subspace of $A_{\mathrm{res}}$.

**Proof of (iii).** $A_{\mathrm{res}}^{(\mathrm{new})}=\sum_{i=r+1}^d\lambda_i u_i u_i^\top$ is a non-negative linear combination of rank-1 symmetric positive semi-definite matrices ($\lambda_i\ge0$ for all $i$ since $A_{\mathrm{res}}\succeq0$, and each

<a id="pdf-0653"></a>
## PDF 653 · Book page 617

$u_i u_i^\top\succeq0$). The cone of symmetric positive semi-definite matrices is closed under non-negative linear combinations, so $A_{\mathrm{res}}^{(\mathrm{new})}\succeq0$. Its eigenvalues are $\lambda_{r+1},\ldots,\lambda_d$ on the eigenvectors $u_{r+1},\ldots,u_d$ and zero on $\operatorname{span}(u_1,\ldots,u_r)$, so its largest eigenvalue is $\max(\lambda_{r+1},0)=\lambda_{r+1}$ (using $\lambda_{r+1}\ge0$). $\square$

<a id="remark-16-58"></a>
**Remark 16.58 (Recovery of rank-1 deflation).** Setting $r=1$ recovers the rank-1 deflation lemma of INCRT I: the deflation $A_{\mathrm{res}}^{(\mathrm{new})}=A_{\mathrm{res}}-\lambda_1u_1u_1^\top$ reduces the Frobenius energy by exactly $\lambda_1^2$, optimally among all rank-1 orthogonal-projector deflations.

<a id="remark-16-59"></a>
**Remark 16.59 (Relation to Eckart–Young–Mirsky).** Lemma 16.57 is closely related to the Eckart–Young–Mirsky theorem: the rank-$r$ matrix $A_r:=\sum_{i=1}^r\lambda_i u_i u_i^\top$ is the best rank-$r$ Frobenius approximation to $A_{\mathrm{res}}$, and the deflation $A_{\mathrm{res}}^{(\mathrm{new})}=A_{\mathrm{res}}-A_r$ is the relevant residual. The novelty here is the *optimality within the class of orthogonal-projector deflations* $\widetilde A=A_{\mathrm{res}}-PA_{\mathrm{res}}P$, which is a class strictly larger than rank-$r$ approximations of the form $A_{\mathrm{res}}-QQ^\top$. The identity (16.85), $\|A_{\mathrm{res}}\|_F^2-\|\widetilde A\|_F^2=\|PA_{\mathrm{res}}P\|_F^2$, appears not to have been recorded elsewhere in this clean form.

<a id="remark-16-60"></a>
**Remark 16.60 (Why operator energy, not operator norm).** The optimality (16.83) is in terms of the squared Frobenius norm of the residual operator, not the operator norm. A different deflation may reduce $\|A_{\mathrm{res}}\|_F^2$ by less but yield a residual with smaller operator norm if $\lambda_2\ll\lambda_1$. The framework's growth criterion (the INCRT I growth criterion) is based on the residual energy $\Gamma_{\mathrm{res}}=\|A_{\mathrm{res}}\|_F^2/\Gamma_0$, so the Frobenius optimality is the operationally relevant notion.

The lemma generalises the lemma stated above of INCRT I (rank-1 deflation) directly: the rank-1 case is recovered by setting $r=1$. The energy reduction is no longer linear in the eigenvalues but quadratic, reflecting the squared-Frobenius norm of the deflation.

The natural choice of $r$ for a given growth event is the size of the dominant spectral cluster of $A_{\mathrm{res}}$.

<a id="definition-16-61"></a>
**Definition 16.61 (Spectral cluster size).** Given an eigenvalue threshold ratio $\theta_{\mathrm{clust}}\in(0,1)$, the *spectral cluster size* of $A_{\mathrm{res}}$ is

<a id="equation-16-92"></a>
$$
r^*(A_{\mathrm{res}};\theta_{\mathrm{clust}})
=\max\{r:\lambda_r(A_{\mathrm{res}})
\ge\theta_{\mathrm{clust}}\lambda_1(A_{\mathrm{res}})\}.
\tag{16.92}
$$

The cluster size is the number of eigenvalues within a factor $\theta_{\mathrm{clust}}$ of the dominant one.

In the framework so far, $\theta_{\mathrm{clust}}=0$ corresponds to the rank-1 deflation (only the dominant eigenvector). A choice $\theta_{\mathrm{clust}}\in(0,1)$ produces a rank-$r^*$ deflation whose size adapts to the local spectrum. Tasks with isolated eigenvalues (clear spectral gaps) yield small $r^*$; tasks with degenerate spectra (clusters of eigenvalues at the same scale) yield large $r^*$.

<a id="section-16-14-2"></a>
### 16.14.2 The count–rank invariant

The static head count $K^*$ of the theorem stated in this part of INCRT I is derived under rank-1 deflation: each head captures one spectral direction, and the count satisfies $K^*=\Theta(\kappa_A^2\log(\Gamma_0/\theta_w))$. Under rank-$r$ deflation, each head captures $r$ directions, and the count is reduced accordingly.

<a id="theorem-16-62"></a>
**Theorem 16.62 (Count–rank invariant).** Let $A_{\mathrm{res}}$ be the residual operator with spectral structure satisfying the assumptions of the theorem stated in this part of INCRT I. Let $r_h\in\{1,\ldots,d\}$ be the rank of the deflation performed at head $h$, with $r_h$ chosen by the

<a id="pdf-0654"></a>
## PDF 654 · Book page 618

cluster-size rule of Definition the preceding result. The total rank product satisfies

<a id="equation-16-93"></a>
$$
\sum_{h=1}^K r_h
=\Theta\left(\kappa_A^2\log\frac{\Gamma_0}{\theta_w}\right),
\tag{16.93}
$$

independently of the individual choices of $r_h$ along the trajectory. The number of heads $K$ adjusts inversely: $K=\Theta(K^*/\bar r)$, where $\bar r$ is the average rank per head.

*Proof sketch.* The compressed-sensing argument of the theorem stated in this part of INCRT I bounds the total spectral “volume” that the architecture must cover, given by $\kappa_A^2\log(\Gamma_0/\theta_w)$. Each rank-$r_h$ head captures volume proportional to $r_h$. The total volume captured equals the product $\sum_h r_h$ multiplied by an average eigenvalue scale that is constant under spectral cluster homogeneity within each head. Equating the two volumes gives the stated count–rank invariant. The proof is in the technical appendix. $\square$

The theorem makes the count–rank trade-off explicit. The architectural choice is no longer “how many heads?” but “how to split the spectral budget between count and rank?”. A rank-1 deflation (the standard INCRT choice) gives $K=K^*$ and $\bar r=1$. A high-rank deflation (each head covers a wide eigenvalue band) gives $K\ll K^*$ and $\bar r\gg1$. The product is invariant.

**Recovery of the standard scaling.** The transformer convention $d_k=d/H$ is recovered when the spectral cluster size happens to equal $d/H$ on average, i.e. when the task spectrum has approximately uniform clustering at that scale. The convention is therefore a heuristic that is exact when the task spectrum is uniform and approximate otherwise.

**Recovery of the Bhojanapalli proposal.** The proposal of [citation], $d_k=N$ independent of $H$, corresponds to the limit $\theta_{\mathrm{clust}}\to1$ in the cluster-size rule: each head captures only the perfectly dominant eigenvector and any others that match it within an arbitrarily tight ratio. For tasks where the leading spectrum is isolated, this gives $\bar r=1$ and $K=K^*$, recovering the standard rank-1 INCRT. For tasks where the leading eigenvalues cluster, the choice forces $H=K^*$ heads of rank 1 each, which under the constraint $H\cdot d_k=d$ yields $d_k=d/K^*$. The Bhojanapalli proposal $d_k=N$ corresponds to the regime $K^*\approx d/N$, i.e. to tasks for which the compressed-sensing budget is saturated by sequence-length-many heads.

<a id="section-16-14-3"></a>
### 16.14.3 The depth–rank–width trade-off

Combined with the depth–width split of Corollary INCRT I (regime dynamics), the count–rank invariant gives a three-way trade-off.

<a id="corollary-16-63"></a>
**Corollary 16.63 (Three-way architectural trade-off).** At convergence of the three-level INCRT framework with rank-adaptive deflation, the architecture satisfies

<a id="equation-16-94"></a>
$$
L^*\cdot K_w\cdot\bar r
=\Theta\left(\kappa_A^2\log\frac{\Gamma_0}{\theta_w}\right),
\tag{16.94}
$$

where $L^*$ is the depth, $K_w$ the width per layer, and $\bar r$ the average head rank. The product is invariant; individual factors are determined by the spectral structure of the task at the relevant level.

*Proof.* Combining the theorem stated in this part of INCRT I (regime dynamics, $L/N=1/K_w$) with the theorem stated in this part (count–rank invariant, $\sum_h r_h=K^*$) gives the product $L\cdot K_w\cdot\bar r=K^*$ at convergence. $\square$
