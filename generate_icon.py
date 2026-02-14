"""
Gera o icone do CodeGymCraft em alta definicao maxima.
Renderiza a 2048px com supersampling 4x e faz downscale com LANCZOS.
O ICO usa compressao PNG interna para tamanhos >= 48px (maxima qualidade).
"""
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import struct
import io
import os

SUPERSAMPLE = 2048  # Renderizar em resolucao muito alta


def create_master():
    """Cria o icone master em 2048x2048 com anti-aliasing perfeito."""
    s = SUPERSAMPLE
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Fundo arredondado roxo/violeta
    bg_color = (124, 58, 237)
    radius = int(s * 0.22)
    draw.rounded_rectangle([(0, 0), (s - 1, s - 1)], radius=radius, fill=bg_color)

    # Gradiente sutil na parte inferior para profundidade
    overlay = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    od = ImageDraw.Draw(overlay)
    for y in range(s // 2, s):
        alpha = int(40 * (y - s // 2) / (s // 2))
        od.line([(0, y), (s, y)], fill=(0, 0, 0, alpha))
    img = Image.alpha_composite(img, overlay)
    draw = ImageDraw.Draw(img)

    # Texto "CG" — usar fonte grande e Bold
    font_size = int(s * 0.48)
    font = None
    # Tentar fontes em ordem de preferencia
    font_candidates = [
        "C:/Windows/Fonts/segoeuib.ttf",   # Segoe UI Bold
        "C:/Windows/Fonts/segoeui.ttf",     # Segoe UI Regular
        "C:/Windows/Fonts/arialbd.ttf",     # Arial Bold
        "C:/Windows/Fonts/calibrib.ttf",    # Calibri Bold
    ]
    for name in font_candidates:
        try:
            font = ImageFont.truetype(name, font_size)
            break
        except (OSError, IOError):
            continue
    if font is None:
        font = ImageFont.load_default()

    text = "CG"
    bbox = draw.textbbox((0, 0), text, font=font)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    tx = (s - tw) // 2 - bbox[0]
    ty = (s - th) // 2 - bbox[1] - int(s * 0.025)

    # Sombra suave (blur)
    shadow_layer = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    sd = ImageDraw.Draw(shadow_layer)
    sd.text((tx + 4, ty + 6), text, fill=(0, 0, 0, 60), font=font)
    shadow_layer = shadow_layer.filter(ImageFilter.GaussianBlur(radius=8))
    img = Image.alpha_composite(img, shadow_layer)
    draw = ImageDraw.Draw(img)

    # Texto branco principal
    draw.text((tx, ty), text, fill=(255, 255, 255, 255), font=font)

    # Checkmark verde no canto inferior direito
    cs = int(s * 0.28)  # tamanho do circulo
    cx = s - cs - int(s * 0.04)
    cy = s - cs - int(s * 0.04)

    # Sombra do circulo
    circle_shadow = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    csd = ImageDraw.Draw(circle_shadow)
    csd.ellipse([(cx - 2, cy + 4), (cx + cs + 2, cy + cs + 8)], fill=(0, 0, 0, 40))
    circle_shadow = circle_shadow.filter(ImageFilter.GaussianBlur(radius=6))
    img = Image.alpha_composite(img, circle_shadow)
    draw = ImageDraw.Draw(img)

    # Borda branca do circulo (ligeiramente maior)
    bw = int(s * 0.022)
    draw.ellipse([(cx - bw, cy - bw), (cx + cs + bw, cy + cs + bw)], fill=(255, 255, 255))
    # Circulo verde
    draw.ellipse([(cx, cy), (cx + cs, cy + cs)], fill=(34, 197, 94))

    # Checkmark branco dentro do circulo (mais grosso e definido)
    ccx = cx + cs // 2
    ccy = cy + cs // 2
    scale = cs * 0.24

    p1 = (ccx - scale * 1.0, ccy + scale * 0.05)
    p2 = (ccx - scale * 0.2, ccy + scale * 0.75)
    p3 = (ccx + scale * 1.1, ccy - scale * 0.7)

    line_w = max(6, int(s * 0.032))
    draw.line([p1, p2], fill=(255, 255, 255), width=line_w, joint="curve")
    draw.line([p2, p3], fill=(255, 255, 255), width=line_w, joint="curve")

    # Circulos nas pontas para suavizar (line caps)
    r = line_w // 2
    for p in [p1, p2, p3]:
        draw.ellipse([(p[0] - r, p[1] - r), (p[0] + r, p[1] + r)], fill=(255, 255, 255))

    return img


def build_ico_with_png(images_dict, output_path):
    """
    Cria um ICO com compressao PNG interna para cada tamanho.
    Isso resulta em qualidade MUITO superior ao ICO padrao do Pillow
    que usa BMP sem compressao e perde qualidade.

    images_dict: {size: PIL.Image} ex: {16: img16, 32: img32, ...}
    """
    entries = []
    data_blocks = []
    offset = 6 + 16 * len(images_dict)  # header(6) + entries(16 each)

    for size in sorted(images_dict.keys()):
        img = images_dict[size]
        # Converter para RGBA
        img = img.convert("RGBA")

        # Salvar como PNG em memoria
        buf = io.BytesIO()
        img.save(buf, format="PNG", optimize=True)
        png_data = buf.getvalue()

        # ICO entry: width, height (0=256), planes, bpp, size, offset
        w = 0 if size >= 256 else size
        h = 0 if size >= 256 else size

        entry = struct.pack("<BBBBHHII",
                            w,          # width (0 = 256+)
                            h,          # height (0 = 256+)
                            0,          # color palette
                            0,          # reserved
                            1,          # color planes
                            32,         # bits per pixel
                            len(png_data),  # size of data
                            offset)     # offset to data

        entries.append(entry)
        data_blocks.append(png_data)
        offset += len(png_data)

    # Escrever arquivo ICO
    with open(output_path, "wb") as f:
        # Header: reserved(2) + type(2, 1=ICO) + count(2)
        f.write(struct.pack("<HHH", 0, 1, len(images_dict)))
        for entry in entries:
            f.write(entry)
        for block in data_blocks:
            f.write(block)


# === MAIN ===
print("Gerando icone master 2048x2048...")
master = create_master()

# Gerar cada tamanho com LANCZOS de alta qualidade
sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
images_dict = {}
for sz in sizes:
    resized = master.resize((sz, sz), Image.LANCZOS)
    # Aplicar leve sharpen nos tamanhos pequenos para manter nitidez
    if sz <= 48:
        resized = resized.filter(ImageFilter.SHARPEN)
    images_dict[sz] = resized
    print(f"  {sz}x{sz} OK")

# Salvar ICO com compressao PNG interna (qualidade maxima)
ico_path = os.path.join(os.path.dirname(__file__), "src", "CodeGym.UI", "Resources", "icon.ico")
build_ico_with_png(images_dict, ico_path)
print(f"ICO (PNG-compressed): {ico_path}")

# Salvar PNG 256px
png_path = os.path.join(os.path.dirname(__file__), "src", "CodeGym.UI", "Resources", "icon.png")
master.resize((256, 256), Image.LANCZOS).save(png_path, optimize=True)
print(f"PNG 256: {png_path}")

# Salvar PNG 1024px para referencia
png_hd = os.path.join(os.path.dirname(__file__), "src", "CodeGym.UI", "Resources", "icon_hd.png")
master.resize((1024, 1024), Image.LANCZOS).save(png_hd, optimize=True)
print(f"PNG 1024: {png_hd}")

# Verificar tamanho do ICO
ico_size = os.path.getsize(ico_path)
print(f"\nICO file size: {ico_size:,} bytes ({ico_size/1024:.1f} KB)")
print("Done!")
