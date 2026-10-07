"""Open Graph image of the landing page, 1200x630 (D-69).

    python scripts/og-image.py

Writes TOP4HONEYCHAINS/TOP4HoneyChains.Web/wwwroot/img/landing/og-image.jpg with the stacked logo
(wwwroot/img/brand/top4-logo-stacked.svg, used as is). If wwwroot/img/landing/og-bg.jpg exists the logo sits
on the left half of that photo; otherwise on a cream background. The SVG is rendered by a local headless Chrome
(set CHROME to its path if it is not in the usual place); needs Pillow (pip install Pillow).
"""
import os
import pathlib
import subprocess
import sys
import tempfile

from PIL import Image

WEB = pathlib.Path(__file__).resolve().parent.parent / 'TOP4HONEYCHAINS' / 'TOP4HoneyChains.Web' / 'wwwroot'
LOGO = WEB / 'img' / 'brand' / 'top4-logo-stacked.svg'
BG = WEB / 'img' / 'landing' / 'og-bg.jpg'
OUT = WEB / 'img' / 'landing' / 'og-image.jpg'
W, H = 1200, 630
CREAM = '#FFF4D6'


def chrome():
    candidates = [os.environ.get('CHROME'),
                  r'C:\Program Files\Google\Chrome\Application\chrome.exe',
                  r'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe',
                  '/usr/bin/google-chrome', '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome']
    for c in candidates:
        if c and pathlib.Path(c).exists():
            return c
    sys.exit('Chrome not found; set CHROME to its path')


def main():
    if BG.exists():
        background = f"background: url('{BG.as_uri()}') center / cover no-repeat;"
        place = 'left: 0; width: 600px;'      # the left half of the photo
    else:
        background = f'background: {CREAM};'
        place = 'left: 0; width: 1200px;'
    html = f'''<!doctype html><html><head><meta charset="utf-8"><style>
html, body {{ margin: 0; width: {W}px; height: {H}px; overflow: hidden; {background} }}
.slot {{ position: absolute; top: 0; height: {H}px; {place} display: flex; align-items: center; justify-content: center; }}
.slot img {{ height: 470px; width: auto; }}
</style></head><body><div class="slot"><img src="{LOGO.as_uri()}" alt=""></div></body></html>'''
    with tempfile.TemporaryDirectory() as tmp:
        page = pathlib.Path(tmp) / 'og.html'
        page.write_text(html, encoding='utf-8')
        shot = pathlib.Path(tmp) / 'og.png'
        subprocess.run([chrome(), '--headless=new', '--disable-gpu', '--hide-scrollbars', '--allow-file-access-from-files',
                        f'--window-size={W},{H}', '--force-device-scale-factor=1', f'--screenshot={shot}', page.as_uri()],
                       check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=60)
        with Image.open(shot) as image:
            image = image.convert('RGB')
            if image.size != (W, H):
                image = image.crop((0, 0, W, H))
            image.save(OUT, 'JPEG', quality=88, optimize=True, progressive=True)
    print(f'wrote {OUT} ({"photo" if BG.exists() else "cream"} background)')


if __name__ == '__main__':
    main()
