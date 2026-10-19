"""Genera el logo de la aplicación.

Uso: python tools/make_logo.py <carpeta_salida>
Crea logo.png (512 px; va en docs/images/), app.ico (16-256 px; va en
src/MediaGrab/Assets/) y preview.png (solo para revisarlo).
Requiere Pillow.
"""
import sys
from PIL import Image, ImageDraw

N = 1024  # lienzo de trabajo; se reduce con supermuestreo

# Degradado azul-índigo: neutro, sin asociarse a ninguna plataforma concreta.
TOP = (64, 140, 255)
BOTTOM = (76, 56, 214)
WHITE = (255, 255, 255, 255)


def gradient(size, top, bottom):
    # Degradado diagonal (de arriba a la izquierda a abajo a la derecha).
    g = Image.new("RGBA", (size, size))
    px = g.load()
    for y in range(size):
        for x in range(0, size, 4):
            t = (x + y) / (2 * (size - 1))
            c = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)) + (255,)
            for dx in range(4):
                if x + dx < size:
                    px[x + dx, y] = c
    return g


def round_line(d, points, width):
    """Polilínea con extremos y uniones redondeados."""
    d.line(points, fill=WHITE, width=width, joint="curve")
    r = width // 2
    for x, y in points:
        d.ellipse((x - r, y - r, x + r, y + r), fill=WHITE)


def render(simple=False):
    img = Image.new("RGBA", (N, N), (0, 0, 0, 0))
    mask = Image.new("L", (N, N), 0)
    ImageDraw.Draw(mask).rounded_rectangle((32, 32, N - 32, N - 32), radius=230, fill=255)
    img.paste(gradient(N, TOP, BOTTOM), (0, 0), mask)

    d = ImageDraw.Draw(img)
    w = 96 if simple else 76  # trazos más gruesos para los tamaños pequeños

    # Flecha de descarga: astil y punta.
    cx = N // 2
    top_y, tip_y = (200, 640) if simple else (220, 620)
    head = 230 if simple else 200
    round_line(d, [(cx, top_y), (cx, tip_y)], w)
    round_line(d, [(cx - head, tip_y - head), (cx, tip_y), (cx + head, tip_y - head)], w)

    # Bandeja.
    left, right, tray_top, tray_bottom = (230, N - 230, 660, 820) if simple else (250, N - 250, 650, 800)
    round_line(d, [(left, tray_top), (left, tray_bottom), (right, tray_bottom), (right, tray_top)], w)
    return img


def main(out):
    detailed, simple = render(), render(simple=True)
    detailed.resize((512, 512), Image.LANCZOS).save(out + "/logo.png")

    frames = [(simple if s <= 32 else detailed).resize((s, s), Image.LANCZOS)
              for s in (16, 24, 32, 48, 64, 128, 256)]
    frames[-1].save(out + "/app.ico", sizes=[f.size for f in frames], append_images=frames[:-1])

    # Previsualización sobre fondo claro y oscuro.
    sheet = Image.new("RGBA", (560, 300), (245, 245, 245, 255))
    ImageDraw.Draw(sheet).rectangle((0, 150, 560, 300), fill=(32, 32, 32, 255))
    x = 10
    for f in frames:
        if f.size[0] > 128:
            continue
        offset = (128 - f.size[0]) // 2
        sheet.paste(f, (x, 10 + offset), f)
        sheet.paste(f, (x, 160 + offset), f)
        x += f.size[0] + 20
    sheet.save(out + "/preview.png")


if __name__ == "__main__":
    main(sys.argv[1])
