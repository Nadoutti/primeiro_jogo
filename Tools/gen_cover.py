"""Gera a capa do itch.io (630x500) com os sprites do jogo em itchio/cover.png.

Uso: python3 Tools/gen_cover.py
"""
import os
import random

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.join(os.path.dirname(__file__), "..")
SPR = os.path.join(ROOT, "Assets", "Resources", "Sprites")
OUT = os.path.join(ROOT, "itchio")
W, H = 630, 500
FONT_BLACK = "/usr/share/fonts/noto/NotoSans-Black.ttf"
FONT_BOLD = "/usr/share/fonts/noto/NotoSans-Bold.ttf"
PINK = (255, 42, 109)
CYAN = (5, 217, 232)
YELLOW = (249, 248, 113)


def sprite(name, scale):
    im = Image.open(os.path.join(SPR, name + ".png")).convert("RGBA")
    return im.resize((im.width * scale, im.height * scale), Image.NEAREST)


def paste(base, im, cx, cy):
    base.alpha_composite(im, (int(cx - im.width / 2), int(cy - im.height / 2)))


def glow_layer(size, draw_fn, radius):
    layer = Image.new("RGBA", size, (0, 0, 0, 0))
    draw_fn(ImageDraw.Draw(layer))
    return layer.filter(ImageFilter.GaussianBlur(radius))


def main():
    rnd = random.Random(42)
    img = Image.new("RGBA", (W, H), (10, 6, 20, 255))

    # chão da arena
    floor = sprite("floor", 4)
    for y in range(-20, H, floor.height):
        for x in range(-30, W, floor.width):
            img.alpha_composite(floor, (x, y))
    d = ImageDraw.Draw(img)
    for x in range(0, W, 48):  # faixa amarela tracejada
        d.rectangle([x, 330, x + 24, 335], fill=(249, 200, 60, 255))

    # poça elétrica e carros no fundo
    paste(img, sprite("puddle_0", 4), 95, 410)
    paste(img, sprite("car_cyan", 4), 560, 440)

    # boss no canto superior direito
    paste(img, sprite("boss", 6), 525, 250)

    # anel do EMP ao redor do jogador
    ring = sprite("emp_ring", 2).copy()
    tint = Image.new("RGBA", ring.size, CYAN + (255,))
    ring = Image.composite(tint, Image.new("RGBA", ring.size), ring.split()[3])
    img.alpha_composite(ring.filter(ImageFilter.GaussianBlur(3)), (315 - ring.width // 2, 320 - ring.height // 2))
    img.alpha_composite(ring, (315 - ring.width // 2, 320 - ring.height // 2))

    # horda de drones
    for _ in range(26):
        x, y = rnd.randrange(-10, W + 10), rnd.randrange(215, H)
        if abs(x - 315) < 110 and abs(y - 320) < 110:
            continue
        paste(img, sprite(rnd.choice(["drone_0", "drone_1"]), 4), x, y)
    paste(img, sprite("turret_base", 4), 80, 250)

    # tiros de plasma e inimigos, chips de XP
    for (x, y) in [(400, 260), (440, 235), (230, 270), (190, 300), (370, 395)]:
        paste(img, sprite("bullet_plasma", 4), x, y)
    for (x, y) in [(470, 300), (500, 330), (130, 220)]:
        paste(img, sprite("bullet_enemy", 4), x, y)
    for _ in range(14):
        paste(img, sprite(rnd.choice(["xp_small", "xp_small", "xp_big"]), 3), rnd.randrange(150, 500), rnd.randrange(240, 480))

    # lâminas e jogador no centro
    for (x, y) in [(315 - 85, 320), (315 + 85, 320), (315, 320 - 85)]:
        paste(img, sprite("blade", 4), x, y)
    paste(img, sprite("player_0", 7), 315, 320)

    # bloom simples: borra os pixels claros e soma por cima
    rgb = img.convert("RGB")
    bright = rgb.point(lambda v: int((v - 150) * 1.6) if v > 150 else 0).filter(ImageFilter.GaussianBlur(6))
    img = ImageChops.add(rgb, bright).convert("RGBA")

    # escurece o topo para o título ler bem + vinheta
    shade = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    sd = ImageDraw.Draw(shade)
    for y in range(H):
        a = int(max(0, 215 - y * 1.25))
        sd.line([(0, y), (W, y)], fill=(10, 4, 24, a))
    img = Image.alpha_composite(img, shade)
    vig = Image.new("L", (W, H), 0)
    ImageDraw.Draw(vig).ellipse([-120, -100, W + 120, H + 100], fill=255)
    vig = vig.filter(ImageFilter.GaussianBlur(60))
    dark = Image.new("RGBA", (W, H), (12, 0, 25, 255))
    img = Image.composite(img, dark, vig)

    # título com brilho neon
    title_font = ImageFont.truetype(FONT_BLACK, 74)
    sub_font = ImageFont.truetype(FONT_BOLD, 19)

    def title(dr, fill, offset=(0, 0)):
        for i, word in enumerate(["NEON", "SURVIVOR"]):
            box = dr.textbbox((0, 0), word, font=title_font)
            x = (W - (box[2] - box[0])) / 2 + offset[0]
            dr.text((x, 18 + i * 72 + offset[1]), word, font=title_font, fill=fill)

    for _ in range(2):
        img = Image.alpha_composite(img, glow_layer((W, H), lambda dr: title(dr, PINK + (255,)), 14))
    img = Image.alpha_composite(img, glow_layer((W, H), lambda dr: title(dr, PINK + (255,)), 4))
    d = ImageDraw.Draw(img)
    title(d, CYAN + (200,), (3, 3))  # sombra ciano deslocada (aberração cromática)
    title(d, (255, 235, 245, 255))

    sub = "SOBREVIVA 3 MINUTOS EM NEO-SÃO PAULO"
    box = d.textbbox((0, 0), sub, font=sub_font)
    sx = (W - (box[2] - box[0])) / 2
    img = Image.alpha_composite(img, glow_layer((W, H), lambda dr: dr.text((sx, 176), sub, font=sub_font, fill=CYAN + (255,)), 5))
    ImageDraw.Draw(img).text((sx, 176), sub, font=sub_font, fill=(220, 255, 255, 255))

    # scanlines leves
    lines = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    ld = ImageDraw.Draw(lines)
    for y in range(0, H, 3):
        ld.line([(0, y), (W, y)], fill=(0, 0, 0, 40))
    img = Image.alpha_composite(img, lines)

    os.makedirs(OUT, exist_ok=True)
    img.convert("RGB").save(os.path.join(OUT, "cover.png"))
    print("capa:", os.path.join(OUT, "cover.png"))


if __name__ == "__main__":
    main()
