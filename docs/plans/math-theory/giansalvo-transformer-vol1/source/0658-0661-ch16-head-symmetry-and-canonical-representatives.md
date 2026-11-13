---
title: "Chapter 16: head symmetry, identifiability, and canonical representatives"
source_pdf_pages: [658, 661]
book_pages: [622, 625]
batch: "014"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: head symmetry, identifiability, and canonical representatives

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: identifiability introduction](0655-0657-ch16-rank-prescription-and-identifiability-introduction.md) · [Next: deployment constraints](0662-0664-ch16-minimum-budget-dual-and-quantisation-model.md)

> Editorial note: Full transcription of PDF 658–661 (printed pages 622–625). The direct-product group, left-multiplication action, eigenspace/channel identifications, numerical table, and symmetry-breaking claims are retained exactly as source claims. No cited method or empirical result has been independently substituted for the source's citation placeholders.

<a id="pdf-0658"></a>
## PDF 658 · Book page 622

sign gauge. The eigenvalue-gap structure of the residual operator determines the strength of identifiability: well-separated eigenvalues yield stronger identifiability than clustered ones.

<a id="section-16-15-1"></a>
### 16.15.1 The symmetry group of the head decomposition

The attention block of INCRT I computes, for each head $h$, a quadratic form $x^\top W_h^Q(W_h^K)^\top x$ followed by a softmax and a linear projection through $W_h^VW_h^O$. The output is invariant under several continuous transformations of the per-head weights.

<a id="definition-16-64"></a>
**Definition 16.64 (Head symmetry group).** The *head symmetry group* is the product

<a id="equation-16-95"></a>
$$
\mathcal G=S_K\times\bigl(O(d_k)\bigr)^K,
\tag{16.95}
$$

where $S_K$ is the symmetric group on $K$ symbols (permuting head indices) and $O(d_k)$ is the orthogonal group acting within each head's $d_k$-dimensional channel subspace. The group $\mathcal G$ acts on the head decomposition $(W_h^Q,W_h^K,W_h^V,W_h^O)_{h=1}^K$ by

<a id="equation-16-96"></a>
$$
W_h^{Q,K}\mapsto R_hW_{\pi^{-1}(h)}^{Q,K},
\qquad W_h^{V,O}\mapsto R_hW_{\pi^{-1}(h)}^{V,O},
\tag{16.96}
$$

for $\pi\in S_K$ and $R_h\in O(d_k)$.

The group action leaves the model's output invariant: the quadratic form within each head is unchanged under orthogonal rotation of its channel basis, and the sum over heads is unchanged under permutation of the head indices. The size of the symmetry group is $K!\prod_h\operatorname{vol}(O(d_k))$, which is large enough that two random initialisations of the same architecture lie in distinct $\mathcal G$-orbits with probability one.

**Connection to existing analyses.** The orthogonal-rotation factor $O(d_k)^K$ is the rotational degree of freedom identified by [citation] as the source of inefficiency in memory-light optimisers. The permutation factor $S_K$ is the head-relabelling symmetry identified empirically by [citation] as the mechanism behind spontaneous head specialisation in vision transformers. The framework's symmetry group $\mathcal G$ unifies the two under a single algebraic structure.

<a id="section-16-15-2"></a>
### 16.15.2 Identifiability up to symmetry

The head decomposition is unique modulo $\mathcal G$-action: the equivalence class $[\{u_h\}]_{\mathcal G}$ is an invariant of the task, even though the representative $\{u_h\}$ is not.

<a id="theorem-16-65"></a>
**Theorem 16.65 (Identifiability up to symmetry).** Let two INCRT trajectories on the same task converge to head decompositions $\mathcal B=\sum_h\lambda_hu_hu_h^\top$ and $\mathcal B=\sum_h\lambda_h'(u_h')(u_h')^\top$. There exists a permutation $\pi\in S_K$ and orthogonal rotations $R_h\in O(d_k)$ such that

<a id="equation-16-97"></a>
$$
u_h'=R_hu_{\pi(h)},\qquad \lambda_h'=\lambda_{\pi(h)},
\tag{16.97}
$$

i.e. the two decompositions are related by a $\mathcal G$-action.

*Proof sketch.* Both decompositions reproduce the same task operator $\mathcal B$; their multisets of eigenvalues coincide, giving the permutation $\pi$. For each eigenvalue $\lambda$, the eigenspace $E_\lambda(\mathcal B)$ has a fixed dimension and admits an orthonormal basis defined up to $O(\dim E_\lambda)$; the rotations $R_h$ are precisely the orthogonal transformations between the two bases of each eigenspace. The proof is a direct application of the spectral theorem to $\mathcal B$ and the uniqueness of eigenspace decompositions. $\square$

The theorem establishes that the head decomposition is a quotient object: the equivalence class $[\{u_h\}]_{\mathcal G}$ is a unique invariant of the task, and the architecture is a representative.

<a id="pdf-0659"></a>
## PDF 659 · Book page 623

<a id="section-16-15-3"></a>
### 16.15.3 The spectral invariants

The quantities that drive the framework — $\Gamma_0,\delta_0,\kappa_A,K^*$, the count–rank invariant of this chapter — are $\mathcal G$-invariant: they depend only on the equivalence class $[\{u_h\}]_{\mathcal G}$, not on the representative.

<a id="lemma-16-66"></a>
**Lemma 16.66 ($\mathcal G$-invariant spectral quantities).** The following quantities are invariant under the action of $\mathcal G$ on the head decomposition:

- the directional energy $\Gamma_0=\|\mathcal B\|_F$;
- the smallest informative eigenvalue $\delta_0=\min_i\lambda_i(\mathcal B)$;
- the directional task complexity index $\kappa_A=\Gamma_0/\delta_0$;
- the optimal head count $K^*=\kappa_A^2\log(\Gamma_0/\theta_w)$;
- the count–rank product $K\bar r$ of this chapter;
- the residual energy $\Gamma_{\mathrm{res}}=\|A_{\mathrm{res}}\|_F^2/\Gamma_0$ at every step.

*Proof.* Each quantity is a function of the spectrum of $\mathcal B$ or the spectrum of $A_{\mathrm{res}}$, and the spectrum is preserved under the action of $\mathcal G$ (orthogonal conjugation does not change eigenvalues; permutation does not change the multiset). $\square$

The lemma is the structural justification for treating the framework's quantities as task properties rather than fitting properties. Two INCRT runs on the same task may produce different head representatives, but they predict the same $K^*$, the same $\bar r$, and the same convergence rate. The framework is well-defined on the quotient space.

<a id="section-16-15-4"></a>
### 16.15.4 Eigenvalue gaps and partial identifiability

The strength of identifiability depends on the eigenvalue structure of $\mathcal B$. When all eigenvalues are well-separated, the orthogonal-rotation factor of $\mathcal G$ degenerates to a discrete sign gauge $\mathbb Z_2^K$, and the head decomposition is identifiable up to permutation and sign. When eigenvalues cluster, the orthogonal factor remains non-trivial, and identifiability is correspondingly weaker.

<a id="theorem-16-67"></a>
**Theorem 16.67 (Eigenvalue-gap identifiability).** Let $g_h=\lambda_h-\lambda_{h+1}$ denote the eigenvalue gap between consecutive heads, with the convention $\lambda_K-\lambda_{K+1}=\lambda_K$ at the bottom. The identifiability strength of head $h$ is

<a id="equation-16-98"></a>
$$
s_h=\min(g_{h-1},g_h),
\tag{16.98}
$$

with $g_0=\infty$ at the top. Heads with $s_h>0$ are identifiable up to sign; heads in eigenvalue clusters with $s_h=0$ are identifiable only up to orthogonal rotation within the cluster.

*Proof sketch.* The orthogonal-rotation factor of $\mathcal G$ acts on each eigenspace $E_\lambda(\mathcal B)$ by $O(\dim E_\lambda)$. For simple eigenvalues ($\dim E_\lambda=1$), $O(1)=\mathbb Z_2$ gives the sign gauge. For repeated eigenvalues, the dimension exceeds one and a non-trivial rotation factor remains. Continuity of the spectrum gives that small gaps degrade identifiability smoothly: an eigenvalue cluster of width $O(\epsilon)$ admits residual rotations of magnitude $O(\epsilon)$ in the regime where the framework's matched-bound argument applies. $\square$

<a id="pdf-0660"></a>
## PDF 660 · Book page 624

**The clustering regime in practice.** Most realistic task operators have non-trivial spectral clusters. The CS bound of the theorem stated in this part of INCRT I sums logarithmically over eigenvalue magnitudes, so clusters of comparable eigenvalues contribute coherently to $K^*$ but do not distinguish individual heads within the cluster. The rank-adaptive extension of this chapter addresses this regime directly: eigenvalue clusters are captured by single rank-$r$ heads rather than $r$ rank-1 heads, restoring identifiability at the level of clusters rather than individual eigenvectors. The combination of rank-adaptive deflation and sign-gauge selection therefore yields a maximally identifiable head decomposition.

<a id="section-16-15-5"></a>
### 16.15.5 Symmetry breaking and canonical representatives

The equivalence-class identifiability of the theorem stated in this part is satisfactory at the theoretical level but inconvenient in practice: a canonical representative is desirable for interpretability, transfer, and deployment.

A canonical representative is selected by an explicit symmetry-breaking mechanism that fixes the gauge. Two such mechanisms have been proposed in the recent literature:

- **PG-OP (Position-Gated Output Projection)** of [citation] fixes the head ordering by position-dependent learned rotations applied to the output projection $W^O$, breaking the $S_K$ factor into a canonical choice while leaving the within-head rotation uncontrolled.
- **Bias-direction symmetry breaking** of [citation] inserts batchwise-sampled query and value biases that prefer a direction in the rotational space, breaking $O(d_k)$ to a finite stabiliser.

The two mechanisms are complementary: PG-OP fixes the permutation factor, bias-direction breaking fixes the rotation factor. Their combination breaks $\mathcal G$ to a discrete subgroup of sign flips, recovering full identifiability up to a finite gauge group.

<a id="corollary-16-68"></a>
**Corollary 16.68 (Canonical head decomposition).** Under the combined PG-OP and bias-direction symmetry-breaking mechanisms, the head decomposition is identifiable up to a discrete gauge group $\mathcal G_{\mathrm{br}}\subset\mathcal G$ of size $|\mathcal G_{\mathrm{br}}|=2^K$ (sign flips), and a canonical representative is selected by fixing the sign of the first nonzero entry of each $u_h$.

*Proof.* The combination of the two mechanisms breaks $\mathcal G$ to $\mathbb Z_2^K$ (sign flips per head); fixing the sign of the first nonzero entry of each $u_h$ produces a canonical form modulo the trivial group. $\square$

<a id="section-16-15-6"></a>
### 16.15.6 Operational consequences

The identifiability analysis has three operational consequences for the framework.

**Reproducibility.** Two INCRT runs on the same task produce different head representatives but the same equivalence class. Comparison metrics across runs should be $\mathcal G$-invariant: the directional task complexity index, the residual energy curve, and the head count $K^*$ are valid; per-head eigenvector comparisons are not. Existing reproducibility studies of attention heads [citation] are implicitly $\mathcal G$-invariant in the metrics they report.

**Transfer.** The transfer extension of this chapter computes a spectral-overlap measure between source and target tasks. The overlap is $\mathcal G$-invariant by construction: it depends only on the eigenspaces of the two task operators, not on the chosen representatives. The transfer bound therefore predicts the same architectural mismatch regardless of how the source representative was selected.

<a id="pdf-0661"></a>
## PDF 661 · Book page 625

**Interpretability.** Per-head interpretability claims — “head $h$ attends to syntactic structure”, “head $h$ tracks coreference” — are representative-dependent. A canonical-representative selection is required for the claims to be reproducible across training runs. The PG-OP and bias-direction mechanisms provide such a selection, and the canonical decomposition of the relevant corollary is the appropriate object on which to attach interpretability labels.

<a id="section-16-15-7"></a>
### 16.15.7 Empirical signature

The identifiability strength of the theorem stated in this part predicts that two INCRT runs on the same task produce head representatives whose maximum inner-product alignment, after permutation matching, scales with the eigenvalue gap. The prediction can be tested directly.

**Setup.** A controlled task with $\kappa_A=4$ and prescribed eigenvalue spectrum is run twice with different random initialisations. The eigenvalue spectrum is varied across runs to span four regimes: well-separated ($g_{\min}/\Gamma_0=0.1$), moderately separated ($0.03$), nearly clustered ($0.01$), and clustered ($0.003$). For each pair of runs, the maximum inner-product alignment $\max_\pi\min_h|\langle u_h,u_{\pi(h)}'\rangle|$ is computed after permutation matching.

**Result.** the table reports the alignment across the four regimes, with and without the PG-OP and bias-direction symmetry-breaking mechanisms.

<a id="table-16-11"></a>
| $g_{\min}/\Gamma_0$ | no SB | PG-OP only | PG-OP + bias |
|---:|---:|---:|---:|
| 0.100 | 0.94 | 0.97 | 0.99 |
| 0.030 | 0.81 | 0.89 | 0.96 |
| 0.010 | 0.52 | 0.74 | 0.91 |
| 0.003 | 0.21 | 0.45 | 0.78 |

**Table 16.11:** Maximum inner-product alignment of head eigenvectors between two INCRT runs on the same task, as a function of the minimum eigenvalue gap $g_{\min}$ relative to the total energy $\Gamma_0$.

*“no SB” is the bare framework; “PG-OP only” adds the position-gated output projection of [citation]; “PG-OP + bias” additionally adds the bias-direction symmetry breaking of [citation]. The bare framework's alignment degrades sharply as eigenvalues cluster, recovering the prediction of the theorem. The combined symmetry-breaking mechanisms restore alignment across the entire range, producing a canonical representative in agreement with the relevant corollary.*

The agreement validates the structural prediction: bare INCRT is identifiable up to $\mathcal G$ but not pointwise; the combined symmetry-breaking restores pointwise identifiability up to the discrete sign gauge.

**Summary.** The head decomposition of INCRT is unique modulo the symmetry group $\mathcal G=S_K\times O(d_k)^K$, and the spectral quantities of the framework are precisely the $\mathcal G$-invariants of the decomposition. Identifiability strength depends on the eigenvalue-gap structure of the task operator: well-separated eigenvalues admit a sign gauge, clustered eigenvalues admit only an orthogonal-rotation gauge within each cluster. The PG-OP mechanism of [citation] and the bias-direction mechanism of [citation] break the symmetry to a discrete subgroup, providing a canonical representative for the head decomposition. The framework therefore
