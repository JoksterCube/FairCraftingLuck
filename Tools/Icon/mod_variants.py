# Builds mod icons on top of the shared base (Thunderstore/icon_base.png).
# FairCraftingLuck uses clover_badge(base) (variant_b_clover.png). The other helpers are kept for future mods:
# gold_center recolors the green center, sparkles adds glints, clover_badge shows how to add a corner badge.
# Usage (needs Pillow + numpy): python mod_variants.py <icon_base.png> <output folder>
import sys, math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

base_path, out_dir = sys.argv[1], sys.argv[2]
W, SS = 256, 4
CX, CY = 128.0, 126.0
base = Image.open(base_path).convert("RGBA")

def rgb_to_hsv(a):
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx, mn = a.max(-1), a.min(-1); d = mx - mn
    h = np.zeros_like(mx)
    m = d > 1e-6
    rr = m & (mx == r); gg = m & (mx == g) & ~rr; bb = m & ~rr & ~gg
    h[rr] = ((g - b)[rr] / d[rr]) % 6
    h[gg] = (b - r)[gg] / d[gg] + 2
    h[bb] = (r - g)[bb] / d[bb] + 4
    return h * 60, np.where(mx > 0, d / np.maximum(mx, 1e-6), 0), mx

def hsv_to_rgb(h, s, v):
    c = v * s; x = c * (1 - np.abs((h / 60) % 2 - 1)); m = v - c
    z = np.zeros_like(h); i = (h // 60).astype(int) % 6
    r = np.select([i == 0, i == 1, i == 2, i == 3, i == 4, i == 5], [c, x, z, z, x, c])
    g = np.select([i == 0, i == 1, i == 2, i == 3, i == 4, i == 5], [x, c, c, x, z, z])
    b = np.select([i == 0, i == 1, i == 2, i == 3, i == 4, i == 5], [z, z, x, c, c, x])
    return np.stack([r + m, g + m, b + m], -1)

def gold_center(img):
    a = np.asarray(img).astype(np.float32)
    rgb = a[..., :3] / 255
    h, s, v = rgb_to_hsv(rgb)
    yy, xx = np.mgrid[0:W, 0:W]
    r = np.hypot(xx - CX, yy - CY)
    green = (h > 65) & (h < 170) & (s > 0.12)
    fade = np.clip((84 - r) / 4, 0, 1)
    w = green * fade
    nh = np.full_like(h, 42.0)
    ns = np.clip(s * 1.15, 0, 1)
    nv = np.clip(v * 1.08 + 0.03, 0, 1)
    new = hsv_to_rgb(nh, ns, nv)
    rgb = rgb * (1 - w[..., None]) + new * w[..., None]
    a[..., :3] = rgb * 255
    return Image.fromarray(np.clip(a, 0, 255).astype(np.uint8))

def sparkle(draw, x, y, size, color):
    s = size * SS; x *= SS; y *= SS; t = s * 0.22
    draw.polygon([(x, y - s), (x + t, y - t), (x + s, y), (x + t, y + t), (x, y + s), (x - t, y + t), (x - s, y), (x - t, y - t)], fill=color)

def sparkles(img):
    layer = Image.new("RGBA", (W * SS, W * SS), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    for x, y, sz in [(78, 72, 9), (182, 86, 7), (170, 170, 6), (86, 168, 5)]:
        sparkle(d, x, y, sz + 1.6, (40, 26, 8, 255))
        sparkle(d, x, y, sz, (255, 236, 160, 255))
    layer = layer.resize((W, W), Image.LANCZOS)
    glow = layer.filter(ImageFilter.GaussianBlur(3))
    out = Image.alpha_composite(img, glow.point(lambda v: v) if False else glow)
    return Image.alpha_composite(out, layer)

def clover_badge(img, cx=204, cy=204, r=40):
    layer = Image.new("RGBA", (W * SS, W * SS), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    X, Y, R = cx * SS, cy * SS, r * SS
    # shadow, dark rim, gold ring, cream face
    sh = Image.new("RGBA", layer.size, (0, 0, 0, 0))
    ImageDraw.Draw(sh).ellipse([X - R, Y - R + 3 * SS, X + R, Y + R + 3 * SS], fill=(0, 0, 0, 120))
    sh = sh.filter(ImageFilter.GaussianBlur(4 * SS))
    d.ellipse([X - R, Y - R, X + R, Y + R], fill=(24, 20, 16, 255))
    R2 = R - 3.5 * SS
    d.ellipse([X - R2, Y - R2, X + R2, Y + R2], fill=(176, 132, 62, 255))
    R3 = R2 - 5 * SS
    d.ellipse([X - R3, Y - R3, X + R3, Y + R3], fill=(24, 20, 16, 255))
    R4 = R3 - 2.5 * SS
    d.ellipse([X - R4, Y - R4, X + R4, Y + R4], fill=(238, 226, 190, 255))
    # four-leaf clover: each leaf is a heart of two circles + triangle, rotated
    leaf_r = 9.0 * SS
    dark, green, light = (24, 20, 16, 255), (92, 150, 62, 255), (128, 184, 86, 255)
    stem_w = int(3.2 * SS)
    d.line([(X, Y), (X + 10 * SS, Y + 22 * SS)], fill=dark, width=stem_w + int(2.4 * SS))
    d.line([(X, Y), (X + 10 * SS, Y + 22 * SS)], fill=green, width=stem_w)
    for pass_ in (0, 1):
        for k in range(4):
            ang = math.radians(45 + 90 * k)
            ux, uy = math.cos(ang), math.sin(ang)
            px, py = -uy, ux
            dist = 10.4 * SS
            pad = 1.4 * SS if pass_ == 0 else 0
            col = dark if pass_ == 0 else (green if k % 2 else light)
            for sgn in (-1, 1):
                ccx = X + ux * dist + px * sgn * leaf_r * 0.62
                ccy = Y + uy * dist + py * sgn * leaf_r * 0.62
                rr = leaf_r + pad
                d.ellipse([ccx - rr, ccy - rr, ccx + rr, ccy + rr], fill=col)
            tip = (X - ux * pad * 1.5, Y - uy * pad * 1.5)
            a1 = (X + ux * dist * 1.2 + px * (leaf_r * 1.45 + pad), Y + uy * dist * 1.2 + py * (leaf_r * 1.45 + pad))
            a2 = (X + ux * dist * 1.2 - px * (leaf_r * 1.45 + pad), Y + uy * dist * 1.2 - py * (leaf_r * 1.45 + pad))
            d.polygon([tip, a1, a2], fill=col)
    # leaf veins
    for k in range(4):
        ang = math.radians(45 + 90 * k)
        d.line([(X, Y), (X + math.cos(ang) * 19 * SS, Y + math.sin(ang) * 19 * SS)], fill=(60, 100, 40, 255), width=int(1.2 * SS))
    combined = Image.alpha_composite(sh, layer).resize((W, W), Image.LANCZOS)
    return Image.alpha_composite(img, combined)

a = sparkles(gold_center(base))
b = clover_badge(base)
c = clover_badge(sparkles(gold_center(base)))
a.save(f"{out_dir}/variant_a_gold.png"); b.save(f"{out_dir}/variant_b_clover.png"); c.save(f"{out_dir}/variant_c_both.png")

sheet = Image.new("RGBA", (W * 4, W * 2), (255, 255, 255, 255))
sheet.paste((28, 30, 34, 255), (0, W, W * 4, W * 2))
for row in range(2):
    for i, im in enumerate([base, a, b, c]):
        sheet.alpha_composite(im, (i * W, row * W))
sheet.convert("RGB").save(f"{out_dir}/variants.png")
