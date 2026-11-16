# 뒷산 바닥 높이장(heightfield) 생성기 — 설계 미리보기와 이후 .vox 생성이 같은 데이터를 쓰도록 한 파일로 둔다.
import numpy as np
from scipy.ndimage import gaussian_filter
from scipy.spatial import cKDTree

S = 1.0 / 15.0          # 1복셀 = 0.0667 m (치비 1복셀과 동일)
VPM = 15                # 1 m = 15복셀
SEED = 7
rng = np.random.default_rng(SEED)

# 청크 (x0, z0, x1, z1) 미터. 월드 로컬 좌표(Unity에서는 이 값 그대로 배치)
CHUNKS = {
    'T':  (5, -5, 11, 0),
    'A1': (0, 0, 16, 16), 'A2': (0, 16, 16, 32), 'A3': (0, 32, 16, 48),
    'B':  (0, 48, 16, 64),
    'C1': (16, 48, 32, 64), 'C2': (32, 48, 48, 64), 'C3': (48, 48, 64, 64), 'D': (64, 48, 80, 64),
}
X0, Z0, X1, Z1 = 0, -5, 80, 64
NX, NZ = (X1 - X0) * VPM, (Z1 - Z0) * VPM

xs = (np.arange(NX) + 0.5) * S + X0
zs = (np.arange(NZ) + 0.5) * S + Z0
XX, ZZ = np.meshgrid(xs, zs, indexing='ij')   # [i(x), j(z)]

def smooth(t, a, b):
    t = np.clip((t - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)

# ---------- 길 중심선 ----------
def wob(t, L1, p1, L2, p2, a1, a2):
    return a1 * np.sin(2 * np.pi * t / L1 + p1) + a2 * np.sin(2 * np.pi * t / L2 + p2)

pts = []
z = np.arange(-5, 52.0001, 0.04)
env = smooth(z, 0, 6) * (1 - smooth(z, 42, 52))
xw = 8 + env * wob(z, 26, 0.8, 11, 2.0, 2.0, 0.7)
pts += list(zip(xw, z))
th = np.linspace(np.pi, np.pi / 2, 200)
for t in th:
    pts.append((12 + 4 * np.cos(t), 52 + 4 * np.sin(t)))
x = np.arange(12.04, 76.0001, 0.04)
env2 = smooth(x, 12, 20) * (1 - smooth(x, 66, 76))
zw = 56 + env2 * wob(x - 12, 28, 0.3, 12, 1.1, 2.0, 0.7)
pts += list(zip(x, zw))
P = np.array(pts)
seg = np.diff(P, axis=0)
seglen = np.hypot(seg[:, 0], seg[:, 1])
sarr = np.concatenate([[0], np.cumsum(seglen)])
tang = np.vstack([seg, seg[-1:]])
tang /= np.hypot(tang[:, 0], tang[:, 1])[:, None]
PATH_LEN = sarr[-1]

# ---------- 길 중심선 높이 (구간 길이 m, 경사 %) ----------
GRADES = [
    (5, 0),
    (6, 3), (5, 13), (3, 3), (2, 0),            # A1
    (4, 6), (6, 16), (3, 0), (3, 8),            # A2
    (5, 10), (5, 18), (2, 0), (4, 6),           # A3
    (4, 4), (6.3, 0),                           # B (코너 + 평지)
    (4, 0), (6, 14), (5, 20), (3, 0), (2, 6),   # C1
    (4, -6), (6, 14), (6, 18),                  # C2
    (3, 0), (6, 15), (7, 12),                   # C3
    (6, 10), (5, 18), (3, 0), (2, 8),           # D
]
def build_profile():
    ss = [0.0]; hh = [0.0]
    for L, g in GRADES:
        ss.append(ss[-1] + L); hh.append(hh[-1] + L * g / 100.0)
    return np.array(ss), np.array(hh)
PS, PH = build_profile()
def Hc_of_s(s):
    return np.interp(s, PS, PH)

# ---------- 최근접 길 점 ----------
tree = cKDTree(P)
def nearest(Xa, Za):
    q = np.stack([Xa.ravel(), Za.ravel()], 1)
    d, idx = tree.query(q, workers=2)
    p = P[idx]; tg = tang[idx]
    dx = q[:, 0] - p[:, 0]; dz = q[:, 1] - p[:, 1]
    # 진행 방향의 왼쪽(= 산 쪽)이 +
    side = -tg[:, 1] * dx + tg[:, 0] * dz
    dd = np.sign(side) * d
    return dd.reshape(Xa.shape), sarr[idx].reshape(Xa.shape)

D, SS = nearest(XX, ZZ)
Hc = Hc_of_s(SS)
HW = 1.5 + 0.28 * np.sin(SS / 4.5) + 0.12 * np.sin(SS / 1.7 + 1)   # 길 반폭(약 3 m 폭)
U = np.abs(D) - HW        # 길 가장자리로부터 바깥 거리 (<0 = 길 위)
left = D > 0

# ---------- 단면 ----------
def cross_section():
    uo = np.clip(U, 0, None)
    bankL = 0.28 * smooth(uo, 0, 0.8)
    bankR = 0.10 * smooth(uo, 0, 0.6)
    riseL = 0.20 * np.clip(uo - 0.8, 0, None)
    fallR = 0.13 * np.clip(uo - 0.6, 0, None)
    cs = np.where(left, bankL + riseL, bankR - fallR)
    cs = np.clip(cs, -0.9, 2.0)
    return cs
CS = cross_section()

# 포획 구역이 붙을 평지(코너 북쪽) — 나중에 붙일 곳이라 평탄 + 코너 높이와 같게
Hcap = float(Hc_of_s(sarr[len(z) + 100]))   # 코너 호 중간 높이 = 코너 평지
wplat = (1 - smooth(XX, 14, 22)) * smooth(ZZ, 57, 62)

# ---------- 울퉁불퉁 ----------
def gnoise(sigma, seed):
    r = np.random.default_rng(seed).standard_normal((NX, NZ)).astype(np.float32)
    g = gaussian_filter(r, sigma, mode='wrap')
    return (g / g.std()).astype(np.float32)

n_s = gnoise(4.0, 11); n_m = gnoise(12.0, 12); n_f = gnoise(2.2, 13)
w_off = smooth(U, -0.2, 0.9)
bump_path = np.clip(0.38 * n_s + 0.45 * n_m + 0.15 * n_f, -1.0, 1.0)
bump_off = np.clip(0.8 * n_s + 1.1 * n_m + 0.35 * n_f, -2.2, 2.2)
bump_vox = bump_path * (1 - w_off) + bump_off * w_off

# 작은 둔덕·움푹 (길 밖에만)
mound = np.zeros_like(XX, dtype=np.float32)
for _ in range(900):
    cx = rng.uniform(X0, X1); cz = rng.uniform(Z0, Z1)
    r = rng.uniform(0.25, 0.6); hgt = rng.choice([1, 1, 2, -1, -1, -2])
    i0 = int((cx - r * 2 - X0) / S); i1 = int((cx + r * 2 - X0) / S) + 1
    j0 = int((cz - r * 2 - Z0) / S); j1 = int((cz + r * 2 - Z0) / S) + 1
    i0 = max(i0, 0); j0 = max(j0, 0); i1 = min(i1, NX); j1 = min(j1, NZ)
    if i1 <= i0 or j1 <= j0: continue
    gx = XX[i0:i1, j0:j1] - cx; gz = ZZ[i0:i1, j0:j1] - cz
    mound[i0:i1, j0:j1] += hgt * np.exp(-(gx * gx + gz * gz) / (2 * (r * 0.55) ** 2))
mound *= smooth(U, 0.7, 1.4)

H_m = Hc + CS
H_m = gaussian_filter(H_m.astype(np.float32), 10, mode='nearest')   # 최근접 점이 바뀌는 곳의 단차를 부드럽게
H_m = H_m * (1 - wplat) + Hcap * wplat
Hv = np.round(H_m / S + bump_vox * (1 - 0.7 * wplat) + mound).astype(np.int32)

# 청크 안쪽 마스크
MASK = np.zeros((NX, NZ), bool)
CHK = np.full((NX, NZ), '', dtype=object)
for k, (a, b, c, d_) in CHUNKS.items():
    i0 = int(round((a - X0) * VPM)); i1 = int(round((c - X0) * VPM))
    j0 = int(round((b - Z0) * VPM)); j1 = int(round((d_ - Z0) * VPM))
    MASK[i0:i1, j0:j1] = True

# ---------- 색 ----------
def hexrgb(h):
    h = h.lstrip('#'); return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))
# FarmPond 구운 바닥 아틀라스에서 직접 읽은 색 (길 #8A5A36 · 외곽선 #4C321E · 풀 #6F8F4A) + 연못 이끼 (#507848, #386038)
PAL = {
    'path': hexrgb('#8A5A36'), 'path_dark': hexrgb('#6B4428'), 'path_light': hexrgb('#A06C44'), 'outline': hexrgb('#4C321E'),
    'soil': hexrgb('#6A4A33'), 'soil_dark': hexrgb('#5B3F2B'),
    'moss': hexrgb('#507848'), 'moss_dark': hexrgb('#386038'), 'moss_light': hexrgb('#5F8A52'),
    'skirt1': hexrgb('#4A3322'), 'skirt2': hexrgb('#3A281C'),
}
from scipy.ndimage import binary_erosion, binary_opening
m_noise = gnoise(8.0, 21)
rv = np.random.default_rng(5).random((NX, NZ))
hrank = gaussian_filter(Hv.astype(np.float32), 6)
rel = Hv - hrank       # 주변보다 높으면 이끼 잘 낌
moss_m = (m_noise + 0.35 * rel) > 1.05
moss_m = binary_opening(moss_m, iterations=2)       # 점 같은 잔 얼룩 제거 → 덩어리만
path_m = (U <= 0)
moss_m &= ~binary_opening(path_m, iterations=0) if False else ~path_m
ring_m = path_m & ~binary_erosion(path_m, iterations=1)   # 길 가장자리 1복셀 외곽선

# 클래스: 0 길 / 2 흙 / 4 이끼(풀)
CLS = np.full((NX, NZ), 2, np.int8)
CLS[moss_m] = 4
CLS[path_m] = 0

def top_color():
    C = np.zeros((NX, NZ, 3), np.uint8)
    r2 = np.random.default_rng(9).random((NX, NZ))
    C[CLS == 0] = PAL['path']
    C[(CLS == 0) & (r2 < 0.012)] = PAL['path_dark']; C[(CLS == 0) & (r2 > 0.992)] = PAL['path_light']
    C[CLS == 2] = PAL['soil']; C[(CLS == 2) & (r2 < 0.025)] = PAL['soil_dark']
    C[CLS == 4] = PAL['moss']; C[(CLS == 4) & (r2 < 0.03)] = PAL['moss_dark']; C[(CLS == 4) & (r2 > 0.99)] = PAL['moss_light']
    C[ring_m] = PAL['outline']
    return C
TOP = top_color()

if __name__ == '__main__':
    m = MASK
    dx = np.abs(np.diff(Hv, axis=0)); dz = np.abs(np.diff(Hv, axis=1))
    mx = np.maximum(dx[m[1:, :] & m[:-1, :]].max(), dz[m[:, 1:] & m[:, :-1]].max())
    print('PATH_LEN', PATH_LEN, 'rise', PH[-1], 'Hcap', Hcap)
    print('max neighbor step vox', mx, ' H range m', Hv[m].min() * S, Hv[m].max() * S)
    # 길 위 이웃 단차 분포
    pm = (U < 0) & m
    dxp = dx[pm[1:, :] & pm[:-1, :]]; print('path steps x', np.bincount(dxp))
    print('class %', {k: round(float(((CLS == k) & ((U > 0.35) if k >= 2 else True) & m).sum() / m.sum() * 100), 1) for k in range(5)})
    print('columns', int(m.sum()))
