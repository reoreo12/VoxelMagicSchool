# 뒷산 배경: 얇고 긴 바위 기둥(봉우리) 4종 × 방향 2종(길이축 X / Z) + 낮은 이끼 둔덕(apron) 3청크.
# 각 봉우리는 로컬 좌표(왼쪽 아래 모서리 = 0, y=0이 바닥)로 만들고 씬에서 이동·균등/비균등 확대만 한다.
# 카메라(남서→북동)에서 보이는 면(윗면, -X, -Z)만 생성. 방향 전환은 높이장 단계에서 X↔Z 전치 후 다시 메싱.
import numpy as np, sys, os, json
from scipy.ndimage import gaussian_filter, binary_opening
from scipy.spatial import cKDTree
FULL = os.environ.get('FULL', '1') == '1'     # 1: 보이지 않는 +X/+Z 면까지 채움 → 회전해 재사용 가능
S = 1.0 / 15.0; VPM = 15
OUT = sys.argv[1] if len(sys.argv) > 1 else 'out2'
HERE = os.path.dirname(os.path.abspath(__file__))
PAL_ORDER = ['path','path_dark','path_light','outline','soil','soil_dark','moss','moss_dark','moss_light','skirt1','skirt2','rock_light','rock','rock_dark','crack']
PIDX = {n: i + 1 for i, n in enumerate(PAL_ORDER)}
PALHEX = {'path':'#8A5A36','path_dark':'#6B4428','path_light':'#A06C44','outline':'#4C321E','soil':'#6A4A33','soil_dark':'#5B3F2B','moss':'#507848','moss_dark':'#386038','moss_light':'#5F8A52','skirt1':'#4A3322','skirt2':'#3A281C','rock_light':'#B5AA9A','rock':'#9A8F82','rock_dark':'#7C7268','crack':'#5E564E'}
def smooth(t, a, b):
    t = np.clip((t - a) / (b - a), 0, 1); return t * t * (3 - 2 * t)
def gn(sigma, seed, shape):
    r = np.random.default_rng(seed).standard_normal(shape).astype(np.float32)
    g = gaussian_filter(r, sigma, mode='wrap'); return g / g.std()

# ---------------- 공통 메서(greedy, 윗면 + -X + -Z) ----------------
def greedy(g):
    rects = []
    if not g.any(): return rects
    used = np.zeros(g.shape, bool); nu, nv = g.shape
    for u, v in zip(*np.nonzero(g)):
        if used[u, v]: continue
        c = g[u, v]; v2 = v + 1
        while v2 < nv and g[u, v2] == c and not used[u, v2]: v2 += 1
        u2 = u + 1
        while u2 < nu and (g[u2, v:v2] == c).all() and not used[u2, v:v2].any(): u2 += 1
        used[u:u2, v:v2] = True; rects.append((u, v, u2, v2, int(c)))
    return rects

def mesh(Hg, Mg, Tg, facec, lowg, box, sides=((-1, 0), (0, -1))):
    """Hg 높이(복셀 정수) Mg 마스크 Tg 윗면색 lowg 마스크 밖 이웃일 때 단면 아래 끝(복셀). box=(i0,i1,j0,j1) 그리드 인덱스. 좌표는 box 모서리 기준 미터."""
    i0, i1, j0, j1 = box; NXg, NZg = Hg.shape
    hs = Hg[i0:i1, j0:j1]; ms = Mg[i0:i1, j0:j1]; quads = []
    kmin = int(lowg[i0:i1, j0:j1][ms].min()) - 1 if ms.any() else 0; kmax = int(hs[ms].max()); 
    for lvl in np.unique(hs[ms]):
        g = np.where(ms & (hs == lvl), Tg[i0:i1, j0:j1], 0).astype(np.uint8)
        for u, v, u2, v2, c in greedy(g):
            y = lvl * S; quads.append(((u*S, y, v*S), (u*S, y, v2*S), (u2*S, y, v2*S), (u2*S, y, v*S), 0, 1, 0, c))
    for di, dj in sides:
        planes = {}
        ii = np.arange(i0, i1)[:, None]; jj = np.arange(j0, j1)[None, :]
        ni = ii + di; nj = jj + dj; ins = (ni >= 0) & (nj >= 0)
        nm = Mg[np.clip(ni, 0, NXg-1), np.clip(nj, 0, NZg-1)] & ins
        nh = np.where(nm, Hg[np.clip(ni, 0, NXg-1), np.clip(nj, 0, NZg-1)], lowg[i0:i1, j0:j1])
        pa, pb = np.nonzero(ms & (hs > nh))
        for a, b in zip(pa, pb):
            gi = i0 + a; gj = j0 + b; ks = np.arange(nh[a, b] + 1, hs[a, b] + 1)
            cols = facec('x' if di else 'z', gi, gj, ks)
            key = (gi + (1 if di > 0 else 0)) if di else (gj + (1 if dj > 0 else 0)); along = (gj - j0) if di else (gi - i0)
            planes.setdefault(key, []).append((along, ks, cols))
        for key, items in planes.items():
            n_al = (j1 - j0) if di else (i1 - i0)
            g = np.zeros((n_al, kmax - kmin + 2), np.uint8)
            for along, ks, cols in items: g[along, ks - kmin] = cols
            for u, v, u2, v2, c in greedy(g):
                ya, yb = (v + kmin - 1) * S, (v2 + kmin - 1) * S
                if di:
                    x = (key + (1 if di > 0 else 0) - i0) * S if False else (key - i0) * S
                    quads.append(((x, ya, u*S), (x, yb, u*S), (x, yb, u2*S), (x, ya, u2*S), int(di), 0, 0, c))
                else:
                    z = (key - j0) * S
                    quads.append(((u*S, ya, z), (u2*S, ya, z), (u2*S, yb, z), (u*S, yb, z), 0, 0, int(dj), c))
    Vq = np.array([q[:4] for q in quads], np.float32); Nq = np.array([q[4:7] for q in quads], np.float32)
    cr = np.cross(Vq[:, 1] - Vq[:, 0], Vq[:, 2] - Vq[:, 0]); bad = (cr * Nq).sum(1) < 0; Vq[bad] = Vq[bad][:, [0, 3, 2, 1]]
    return Vq.reshape(-1, 3), np.repeat(Nq, 4, 0), np.repeat(np.array([q[7] for q in quads], np.int32), 4), len(quads)

# ---------------- 봉우리 4종 ----------------
KINDS = {   # L×W(m)
    'A': dict(L=4.2, W=3.4, H=9.0),    # 가는 기둥 + 작은 곁바위
    'B': dict(L=6.6, W=3.2, H=7.2),    # 쌍기둥
    'C': dict(L=10.4, W=3.2, H=5.8),   # 길쭉한 능선 바위(봉우리 3개)
    'D': dict(L=6.0, W=4.6, H=3.8),    # 둥근 화강암 돔 더미
}
def blob(U, V, c, r, H, p, e, rib_seed, rib=0.06):
    ang = np.arctan2(V - c[1], U - c[0]); n = 360
    g = gaussian_filter(np.random.default_rng(rib_seed).standard_normal(n), 2.2, mode='wrap'); g /= g.std()
    k = g[((ang + np.pi) / (2 * np.pi) * (n - 1)).astype(int)]
    d = (np.abs((U - c[0]) / r[0]) ** p + np.abs((V - c[1]) / r[1]) ** p) ** (1 / p) * (1 + rib * k)   # 세로 결(바위 홈)
    return H * np.clip(1 - d ** 2.2, 0, 1) ** e, d
def piece_height(kind, swap):
    sp = KINDS[kind]; L, W, H = sp['L'], sp['W'], sp['H']; m = 0.5
    nu, nv = int((L + 2 * m) * VPM), int((W + 2 * m) * VPM)
    U = (np.arange(nu)[:, None] + 0.5) * S - m; V = (np.arange(nv)[None, :] + 0.5) * S - m
    U = U + 0 * V; V = V + 0 * U
    f = np.zeros_like(U); foot = np.zeros_like(U)
    def add(c, r, h, p=2.8, e=0.42, seed=1):
        nonlocal f, foot
        b, d = blob(U, V, c, r, h, p, e, seed); f = np.maximum(f, b); foot = np.maximum(foot, 1 - smooth(d, 0.9, 1.25))
    if kind == 'A':
        add((L*.5, W*.5), (1.5, 1.2), H, seed=1); add((0.8, 2.6), (0.7, 0.65), H*.5, seed=2)
    elif kind == 'B':
        add((1.7, 1.55), (1.45, 1.2), H, seed=3); add((4.5, 1.65), (1.35, 1.1), H*.72, seed=4)
        add((3.1, 1.6), (1.4, 0.9), H*.35, p=2.2, e=0.7, seed=5)
    elif kind == 'C':
        for c, h, s in (((2.0, 1.5), .85, 6), ((5.3, 1.6), 1.0, 7), ((8.5, 1.5), .72, 8)): add(c, (1.7, 1.15), H*h, seed=s)
        add((5.2, 1.6), (5.0, 1.0), H*.42, p=2.4, e=0.6, seed=9)
    else:
        add((1.9, 1.9), (1.75, 1.6), H*.78, p=2.2, e=0.8, seed=10); add((4.0, 2.2), (1.9, 1.7), H, p=2.2, e=0.8, seed=11)
        add((3.0, 3.5), (1.5, 1.0), H*.55, p=2.2, e=0.8, seed=12)
    body = np.where(foot > 0.02, np.maximum(f, 0.3 + 0.4 * foot), 0)       # 가장자리는 이끼 발치(0.3~0.7 m)
    Hm = np.where(foot > 0.02, np.maximum(body, 0.30), 0)
    if swap: Hm = Hm.T
    return Hm
def blockify(a, b):
    if b <= 1: return a
    c = a[b//2::b, b//2::b]; o = np.repeat(np.repeat(c, b, 0), b, 1)
    return np.pad(o, ((0, a.shape[0]-o.shape[0]), (0, a.shape[1]-o.shape[1])), mode='edge')
def build_piece(kind, swap, Q=int(os.environ.get('SQ', 8)), seed=0, HB=int(os.environ.get('SHB', 3)), CB=int(os.environ.get('SCB', 3))):
    Hm = piece_height(kind, swap); NU, NV = Hm.shape; M = Hm > 0
    Hv = (np.round(Hm / S / Q) * Q).astype(np.int32); Hv[M] = np.maximum(Hv[M], Q)
    Hv = blockify(Hv, HB); Hv[M] = np.maximum(Hv[M], Q); Hv[~M] = 0
    Hmax = Hv[M].max()
    n_a = gn(4.0, 90 + seed, (NU, NV)); n_b = gn(2.5, 91 + seed, (NU, NV))
    relf = Hv * S
    slope = np.zeros(Hv.shape, np.int32)
    for ax in (0, 1):
        for sh in (-1, 1): slope = np.maximum(slope, np.abs(Hv - np.roll(Hv, sh, ax)))
    shelf = slope <= Q
    low = relf < (1.1 + 0.5 * n_a)                                     # 낮은 부분 이끼
    summit = (Hv > 0.72 * Hmax) & (n_b > -0.3) & shelf                 # 꼭대기 풀 얹힘(사진 속 정상부)
    ledge = shelf & (relf > 1.2) & (n_a > 0.55)                        # 중간 선반 이끼
    moss = (low | summit | ledge) & M
    moss = binary_opening(moss, iterations=1) | (low & M)
    rock = np.full(Hv.shape, PIDX['rock'], np.uint8)
    rock[n_b > 0.9] = PIDX['rock_light']; rock[n_a < -0.9] = PIDX['rock_dark']; rock[shelf & (relf > 1.2)] = PIDX['rock_light']
    mc = np.full(Hv.shape, PIDX['moss'], np.uint8); mc[n_b < -0.9] = PIDX['moss_dark']; mc[n_b > 1.0] = PIDX['moss_light']
    T = blockify(np.where(moss, mc, rock).astype(np.uint8), CB)
    stripe_i = gn(3.5, 92 + seed, (NU, 1)).ravel(); stripe_j = gn(3.5, 93 + seed, (NV, 1)).ravel()
    lim = 1.3 + 0.7 * n_a
    def facec(axis, gi, gj, ks):
        st = stripe_j[gj] if axis == 'x' else stripe_i[gi]
        base = PIDX['rock_light'] if st > 0.9 else PIDX['crack'] if st < -1.6 else PIDX['rock_dark'] if st < -0.8 else PIDX['rock']
        mcol = PIDX['moss_dark'] if st < -0.3 else PIDX['moss']
        return np.where(ks * S < lim[gi, gj] + 0.35 * np.sin(ks * 0.11 + gj * 0.2), mcol, base).astype(np.uint8)
    low0 = np.zeros(Hv.shape, np.int32)
    V, N, C, nq = mesh(Hv, M, T, facec, low0, (0, NU, 0, NV), sides=((-1, 0), (1, 0), (0, -1), (0, 1)) if FULL else ((-1, 0), (0, -1)))
    return V, N, C, nq, dict(size=(NU * S, NV * S), margin=0.5, Hmax=float(Hmax * S))

if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True); st = []
    for k in KINDS:
        for sw in ((0,) if FULL else (0, 1)):
            V, N, C, nq, info = build_piece(k, sw, seed=ord(k))
            name = f'Spire_{k}' if FULL else f'Spire_{k}{"z" if sw else "x"}'
            np.savez_compressed(f'{OUT}/{name}.npz', V=V, N=N, C=C, palette_n=len(PAL_ORDER))
            st.append(dict(name=name, tris=nq * 2, **info)); print(st[-1], flush=True)
    json.dump(st, open(f'{OUT}/stats_spires.json', 'w'), indent=1)
