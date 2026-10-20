"""Gera os sprites pixel art do Neon Survivor em Assets/Resources/Sprites.

Uso: python3 Tools/gen_sprites.py
"""
import math
import os
import random

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Sprites")

PAL = {
    "k": (10, 6, 20, 255),       # contorno
    "d": (38, 26, 66, 255),      # roxo escuro
    "p": (255, 42, 109, 255),    # rosa neon
    "P": (255, 140, 180, 255),   # rosa claro
    "c": (5, 217, 232, 255),     # ciano neon
    "b": (0, 86, 120, 255),      # ciano escuro
    "y": (249, 248, 113, 255),   # amarelo
    "w": (255, 255, 255, 255),
    "g": (74, 79, 110, 255),     # metal
    "G": (130, 138, 170, 255),   # metal claro
    "s": (242, 198, 160, 255),   # pele
    "o": (255, 140, 0, 255),     # laranja
    "r": (230, 20, 50, 255),     # vermelho
    "v": (123, 44, 191, 255),    # violeta
    "h": (43, 45, 66, 255),      # roupa escura
}


def from_rows(rows, mirror_h=False, mirror_v=False, outline=True):
    if mirror_h:
        rows = [r + r[::-1] for r in rows]
    if mirror_v:
        rows = rows + rows[::-1]
    w = len(rows[0])
    assert all(len(r) == w for r in rows), [len(r) for r in rows]
    img = Image.new("RGBA", (w + 2, len(rows) + 2), (0, 0, 0, 0))
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch != ".":
                img.putpixel((x + 1, y + 1), PAL[ch])
    return add_outline(img) if outline else img


def add_outline(img):
    out = img.copy()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            if img.getpixel((x, y))[3] != 0:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h and img.getpixel((nx, ny))[3] != 0:
                    out.putpixel((x, y), PAL["k"])
                    break
    return out


def save(img, name):
    img.save(os.path.join(OUT, name + ".png"))


# ---------------------------------------------------------------- personagens

PLAYER_TOP = [
    "........",
    "......pp",
    ".....ppp",
    "....pphh",
    "....phss",
    "....hccc",
    "....hcwc",
    ".....hss",
    "....vvvv",
    "...vvpvv",
    "...svvvc",
    "....vvvv",
]
PLAYER_LEGS = [
    ["....hh....hh....", "....hh....hh....", "....gg....gg...."],
    [".....hh..hh.....", ".....hh..hh.....", ".....gg..gg....."],
]


def player():
    top = [r + r[::-1] for r in PLAYER_TOP]
    for i, legs in enumerate(PLAYER_LEGS):
        save(from_rows(top + legs), f"player_{i}")


def drone():
    body = [
        "...gGGG",
        "...gGgg",
        "..ggrrp",
        "...gggg",
        "....ggg",
        ".....cc",
        "......c",
    ]
    props = [["GG.....", ".Gg....", "..gg..."], [".......", "GGg....", "..gg..."]]
    for i, p in enumerate(props):
        save(from_rows(p + body, mirror_h=True), f"drone_{i}")


def biker():
    rows = [
        "......hhhh..........",
        ".....hppppph........",
        "rr.gggppcpppgggGGGyy",
        "rrggggppcppggggGGGyy",
        ".....hppppph........",
        "......hhhh..........",
    ]
    save(from_rows(rows), "biker")


def turret():
    base = [
        ".GGGGGG",
        "Ggggddd",
        "Ggddddd",
        "Gddvvvv",
        "Gddvrrr",
        "Gddvrpp",
        "Gddvrpp",
    ]
    save(from_rows(base, mirror_h=True, mirror_v=True), "turret_base")
    barrel = [
        "............GGGGGGGGgg..",
        "............ggggggggggrr",
        "............ggggggggggrr",
        "............GGGGGGGGgg..",
    ]
    save(from_rows(barrel), "turret_barrel")


def boss():
    img = Image.new("RGBA", (36, 36), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = PAL
    # pernas
    d.rectangle([7, 24, 12, 33], fill=c["g"])
    d.rectangle([23, 24, 28, 33], fill=c["g"])
    d.rectangle([6, 31, 13, 33], fill=c["h"])
    d.rectangle([22, 31, 29, 33], fill=c["h"])
    # tronco
    d.rectangle([8, 10, 27, 26], fill=c["G"])
    d.rectangle([10, 12, 25, 24], fill=c["g"])
    d.rectangle([14, 16, 21, 21], fill=c["d"])
    d.rectangle([16, 17, 19, 20], fill=c["p"])
    # ombros + canhões
    for x0 in (2, 28):
        d.rectangle([x0, 8, x0 + 5, 20], fill=c["G"])
        d.rectangle([x0 + 1, 2, x0 + 4, 9], fill=c["g"])
        d.rectangle([x0 + 1, 1, x0 + 4, 2], fill=c["r"])
    # cabeça
    d.rectangle([12, 3, 23, 10], fill=c["G"])
    d.rectangle([13, 6, 22, 7], fill=c["r"])
    d.point([(15, 6), (20, 6)], fill=c["y"])
    # faixas neon
    d.line([10, 25, 25, 25], fill=c["c"])
    d.line([8, 10, 27, 10], fill=c["c"])
    save(add_outline(img), "boss")


# ---------------------------------------------------------------- projéteis / armas

def disc(size, core, ring, glow=None):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    if glow:
        d.ellipse([0, 0, size - 1, size - 1], fill=glow)
    d.ellipse([1, 1, size - 2, size - 2], fill=ring)
    m = size // 3
    d.ellipse([m, m, size - 1 - m, size - 1 - m], fill=core)
    return img


def projectiles():
    save(disc(8, PAL["w"], PAL["c"], (5, 217, 232, 90)), "bullet_plasma")
    save(disc(8, PAL["y"], PAL["r"], (255, 42, 109, 90)), "bullet_enemy")
    blade = [
        ".....w......",
        ".....cw.....",
        "....bcc.....",
        "....bcc.....",
        "...bcccc....",
        "wccccPPccccw",
        "....ccccb...",
        ".....ccb....",
        ".....ccb....",
        ".....wc.....",
        "......w.....",
    ]
    save(from_rows(blade), "blade")
    # anel do EMP (branco, colorido em código)
    size = 96
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([0, 0, size - 1, size - 1], outline=(255, 255, 255, 255), width=3)
    d.ellipse([4, 4, size - 5, size - 5], outline=(255, 255, 255, 90), width=2)
    save(img, "emp_ring")
    save(Image.new("RGBA", (2, 2), (255, 255, 255, 255)), "pixel")


def pickups():
    chip = ["..c..", ".cwc.", "cwccb", ".cbb.", "..b.."]
    save(from_rows(chip), "xp_small")
    big = ["...p...", "..pPp..", ".pPwPp.", "pPwwPpv", ".pPPpv.", "..ppv..", "...v..."]
    save(from_rows(big), "xp_big")
    health = [
        "wwwwwwwww",
        "wGGGpGGGw",
        "wGGppPGGw",
        "wGpppppGw",
        "wGGGpGGGw",
        "wGGGpGGGw",
        "wwwwwwwww",
    ]
    save(from_rows(health), "health_pack")


# ---------------------------------------------------------------- arena

def floor():
    rnd = random.Random(7)
    img = Image.new("RGBA", (32, 32), (22, 15, 41, 255))
    for _ in range(60):
        x, y = rnd.randrange(32), rnd.randrange(32)
        img.putpixel((x, y), (30, 21, 54, 255) if rnd.random() < 0.7 else (16, 11, 30, 255))
    d = ImageDraw.Draw(img)
    d.line([0, 0, 31, 0], fill=(46, 30, 88, 255))
    d.line([0, 0, 0, 31], fill=(46, 30, 88, 255))
    save(img, "floor")
    # faixa de rua (amarela tracejada), tile 32x4
    lane = Image.new("RGBA", (32, 2), (0, 0, 0, 0))
    ImageDraw.Draw(lane).rectangle([0, 0, 15, 1], fill=(249, 200, 60, 200))
    save(lane, "lane")


def wall():
    rnd = random.Random(3)
    img = Image.new("RGBA", (16, 16), (12, 8, 24, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 15, 15], outline=(30, 20, 55, 255))
    for wy in (3, 9):
        for wx in (2, 6, 10):
            col = rnd.choice([(5, 217, 232, 255), (255, 42, 109, 255), (60, 40, 90, 255), (60, 40, 90, 255), (249, 248, 113, 255)])
            d.rectangle([wx, wy, wx + 2, wy + 2], fill=col)
    save(img, "wall")


def car(name, body, glow):
    img = Image.new("RGBA", (36, 20), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([0, 0, 35, 19], radius=6, fill=glow[:3] + (70,))
    d.rounded_rectangle([2, 3, 33, 16], radius=4, fill=body)
    d.rectangle([11, 5, 23, 14], fill=(20, 20, 40, 255))      # teto/vidro
    d.rectangle([12, 6, 15, 13], fill=(5, 120, 150, 255))
    d.line([2, 4, 2, 15], fill=PAL["r"])                      # lanternas
    d.line([33, 5, 33, 6], fill=PAL["y"])
    d.line([33, 13, 33, 14], fill=PAL["y"])
    save(img, name)


def obstacles():
    car("car_pink", (180, 30, 90, 255), PAL["p"])
    car("car_cyan", (20, 110, 140, 255), PAL["c"])
    img = Image.new("RGBA", (18, 18), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([0, 0, 17, 17], fill=(255, 42, 109, 255))
    d.ellipse([2, 2, 15, 15], fill=(60, 60, 85, 255))
    d.ellipse([4, 4, 13, 13], fill=(90, 92, 120, 255))
    d.ellipse([7, 7, 10, 10], fill=(255, 140, 180, 255))
    save(img, "pillar")
    for i in range(2):
        rnd = random.Random(i + 11)
        img = Image.new("RGBA", (40, 24), (0, 0, 0, 0))
        d = ImageDraw.Draw(img)
        d.ellipse([0, 0, 39, 23], fill=(5, 217, 232, 70))
        d.ellipse([3, 3, 36, 20], fill=(5, 140, 180, 140))
        for _ in range(4):  # faíscas
            x, y = rnd.randrange(8, 32), rnd.randrange(6, 18)
            pts = [(x, y)]
            for _ in range(3):
                x += rnd.choice([-2, -1, 1, 2]); y += rnd.choice([-1, 1])
                pts.append((x, y))
            d.line(pts, fill=(220, 255, 255, 255))
        save(img, f"puddle_{i}")


# ---------------------------------------------------------------- ícones UI

def icons():
    pistol = [
        "..............",
        ".ccccccccccc..",
        ".cwwwwwwwwwcy.",
        ".ccccccccccc..",
        ".ggggbb.......",
        ".gggg.b.......",
        ".gggg.........",
        ".gggg.........",
    ]
    save(from_rows(pistol), "icon_pistol")
    blade = [
        "......w.....",
        ".....cc.....",
        "....bcc.....",
        "....bcc.....",
        "...bcccc....",
        "...cccccc...",
        "......hh....",
        ".....ppp....",
        "......hh....",
        "......hh....",
    ]
    save(from_rows(blade), "icon_blade")
    emp = [
        "....pppp....",
        "..pp....pp..",
        ".p..yyyy..p.",
        "p..y....y..p",
        "p.y..ww..y.p",
        "p.y..ww..y.p",
        "p..y....y..p",
        ".p..yyyy..p.",
        "..pp....pp..",
        "....pppp....",
    ]
    save(from_rows(emp), "icon_emp")
    heart = ["..pp..pp..", ".pPPppppp.", "pPwPpppppp", "pPPpppppp.", ".ppppppp..", "..ppppp...", "...ppp....", "....p....."]
    save(from_rows(heart), "icon_heart")
    boot = ["..cccc....", "..cbbc....", "..cbbc....", "..cbbc....", "..cbbcccc.", ".ccbbbbbcc", ".cwwwwwwwc", ".gggggggg."]
    save(from_rows(boot), "icon_speed")
    magnet = ["rrr..GGG..", "rrr..GGG..", "rrr..GGG..", "rrr..GGG..", "rrrr.GGG..", ".rrrrGGG..", "..rrrrGG..", "...rrrr..."]
    save(from_rows(magnet), "icon_magnet")
    dmg = ["....y....", "...yy....", "..yyy....", ".yyyyyyy.", "....yyy..", "....yy...", "....y....", "...y....."]
    save(from_rows(dmg), "icon_damage")


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    player(); drone(); biker(); turret(); boss()
    projectiles(); pickups(); floor(); wall(); obstacles(); icons()
    print("sprites ok:", sorted(os.listdir(OUT)))
