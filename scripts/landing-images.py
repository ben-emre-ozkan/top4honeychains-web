"""Responsive WebP variants of the landing images (D-65).

Put the photos in TOP4HONEYCHAINS/TOP4HoneyChains.Web/wwwroot/img/landing as
step-1-apiary.jpg ... step-5-shelf.jpg (and og-image.jpg, 1200x630, used as is for Open Graph), then run

    python scripts/landing-images.py

It writes <name>-480/-960/-1440.webp and the same widths as .avif next to each step image (never larger than the
original) and resizes a step JPEG wider than 1440 px down to 1440 px. The page picks the variants up by itself.
Needs Pillow (pip install Pillow). Re-running overwrites the variants.
"""
import pathlib
import sys

from PIL import Image, ImageOps

FOLDER = pathlib.Path(__file__).resolve().parent.parent / 'TOP4HONEYCHAINS' / 'TOP4HoneyChains.Web' / 'wwwroot' / 'img' / 'landing'
STEPS = ['step-1-apiary', 'step-2-delivery', 'step-3-lab', 'step-4-processing', 'step-5-shelf']
WIDTHS = [480, 960, 1440]


def main():
    found = 0
    for name in STEPS:
        source = FOLDER / f'{name}.jpg'
        if not source.exists():
            print(f'missing  {source.name} (the page shows a placeholder)')
            continue
        found += 1
        with Image.open(source) as original:
            image = ImageOps.exif_transpose(original).convert('RGB')
        if image.width > WIDTHS[-1]:
            image = image.resize((WIDTHS[-1], round(image.height * WIDTHS[-1] / image.width)), Image.LANCZOS)
            image.save(source, 'JPEG', quality=84, optimize=True, progressive=True)
        for width in WIDTHS:
            webp = FOLDER / f'{name}-{width}.webp'
            avif = FOLDER / f'{name}-{width}.avif'   # D-70: AVIF first in the srcset, WebP as the fallback
            if width > image.width:
                webp.unlink(missing_ok=True)
                avif.unlink(missing_ok=True)
                continue
            variant = image.resize((width, round(image.height * width / image.width)), Image.LANCZOS) if width < image.width else image
            variant.save(webp, 'WEBP', quality=80, method=6)
            variant.save(avif, 'AVIF', quality=60, speed=4)
            print(f'wrote    {webp.name} ({webp.stat().st_size // 1024} KB), {avif.name} ({avif.stat().st_size // 1024} KB)')
    og = FOLDER / 'og-image.jpg'
    print(('found    ' if og.exists() else 'missing  ') + og.name + ' (Open Graph image, 1200x630)')
    return 0 if found else 1


if __name__ == '__main__':
    sys.exit(main())
