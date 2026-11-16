# 바닥 가장자리 바깥의 낮은 이끼 둔덕(apron) 3청크 — 봉우리 발치를 받치고 허공을 가린다. 높이 1~2.3 m.
import numpy as np, os, sys, json
from scipy.ndimage import gaussian_filter, binary_opening
from scipy.spatial import cKDTree
import gen_spires as G
S = G.S; VPM = G.VPM; Q = 6; HB = 3; CB = 3
X0, Z0, X1, Z1 = 0, 48, 62, 92
NX, NZ = (X1 - X0) * VPM, (Z1 - Z0) * VPM
xs = (np.arange(NX) + .5) * S + X0; zs = (np.arange(NZ) + .5) * S + Z0
XX, ZZ = np.meshgrid(xs, zs, indexing='ij')
E = np.load(os.path.join(G.HERE, 'edges.npz')); e1 = gaussian_filter(E['e1'].astype(float), 12, mode='nearest') * S; e2 = gaussian_filter(E['e2'].astype(float), 12, mode='nearest')[::-1] * S
poly = [(0, 80), (16, 80), (16, 64), (48, 64), (48, 48)]; hcap = 4.16
P = []; HE = []
def seg(a, b, h0, h1):
    n = int(np.hypot(b[0]-a[0], b[1]-a[1]) / 0.05); t = np.linspace(0, 1, n, endpoint=False)
    P.extend(zip(a[0]+(b[0]-a[0])*t, a[1]+(b[1]-a[1])*t)); HE.extend(h0+(h1-h0)*t)
seg(poly[0], poly[1], hcap, hcap); seg(poly[1], poly[2], hcap, e1[16*VPM])
for x in range(16*VPM, 48*VPM, 3): P.append((x*S, 64)); HE.append(e1[x])
for k in range(0, len(e2)-1, 3): P.append((48, 64-k*S)); HE.append(e2[k])
P = np.array(P); HE = np.array(HE); tree = cKDTree(P)
def base_height(x, z):
    d, i = tree.query(np.stack([np.atleast_1d(x), np.atleast_1d(z)], 1)); return HE[i]
dq, idx = tree.query(np.stack([XX.ravel(), ZZ.ravel()], 1), workers=2); dq = dq.reshape(XX.shape)
B = gaussian_filter(HE[idx].reshape(XX.shape).astype(np.float32), 8, mode='nearest')
outside = ((ZZ >= 80) & (XX < 16)) | ((XX >= 16) & (ZZ >= 64) & (XX < 48)) | ((XX >= 48) & (ZZ >= 48))
MASK = outside & (dq <= 12)
n1 = G.gn(30.0, 71, XX.shape); n2 = G.gn(5.0, 72, XX.shape)
R = (1.1 + 0.6 * n1 + 0.35 * n2) * (1 - G.smooth(dq, 9.5, 12)) 
R = np.clip(R, 0.5, 2.4) * MASK
def blockify(a, b):
    c = a[b//2::b, b//2::b]; o = np.repeat(np.repeat(c, b, 0), b, 1)
    return np.pad(o, ((0, a.shape[0]-o.shape[0]), (0, a.shape[1]-o.shape[1])), mode='edge')
Hv = blockify((np.round((B + R) / S / Q) * Q).astype(np.int32), HB); Bv = np.round(B / S).astype(np.int32)
n3 = G.gn(5.0, 73, XX.shape)
mc = np.full(Hv.shape, G.PIDX['moss'], np.uint8); mc[n3 < -0.8] = G.PIDX['moss_dark']; mc[n3 > 0.9] = G.PIDX['moss_light']
mc[(n2 > 1.45)] = G.PIDX['rock']
T = blockify(mc, CB)
CHUNKS = {'AP1': (0, 80, 16, 92), 'AP2': (16, 64, 48, 92), 'AP3': (48, 48, 62, 92)}
def facec(axis, gi, gj, ks):
    return np.where(n3[gi, gj] < -0.3, G.PIDX['moss_dark'], G.PIDX['moss']).astype(np.uint8) * np.ones(len(ks), np.uint8)
if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else 'out2'; st = []
    for name, (a, b, c, d) in CHUNKS.items():
        box = (int((a-X0)*VPM), int((c-X0)*VPM), int((b-Z0)*VPM), int((d-Z0)*VPM))
        # 마스크 밖 이웃은 기준 높이 -12복셀까지 단면
        V, N, C, nq = G.mesh(Hv, MASK, T, facec, Bv - 12, box)
        V = V + np.array([0, 0, 0], np.float32)
        np.savez_compressed(f'{out}/{name}.npz', V=V, N=N, C=C, palette_n=len(G.PAL_ORDER)); st.append(dict(name=name, tris=nq*2, box=(a, b, c, d))); print(st[-1])
    json.dump(st, open(f'{out}/stats_apron.json', 'w'))
