"""Background video of the landing trust section (D-70).

    python scripts/video-loop.py

Reads TOP4HONEYCHAINS/TOP4HoneyChains.Web/wwwroot/media/honey-loop.mp4 (the master, left as it is) and writes next
to it honey-loop-720.mp4 (H.264) and honey-loop-720.webm (VP9), 1280x720 without sound, plus honey-loop-poster.jpg.
The master is a camera move whose last frame does not match the first, so the last second is crossfaded into the
first: the result is one second shorter and loops without a jump. Each file must stay under 3 MB.
Needs ffmpeg and ffprobe on the PATH.
"""
import pathlib
import subprocess
import sys

MEDIA = pathlib.Path(__file__).resolve().parent.parent / 'TOP4HONEYCHAINS' / 'TOP4HoneyChains.Web' / 'wwwroot' / 'media'
SOURCE = MEDIA / 'honey-loop.mp4'
FADE = 1.0
LIMIT = 3 * 1024 * 1024


def duration(path):
    out = subprocess.run(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'default=nw=1:nk=1', str(path)],
                         check=True, capture_output=True, text=True).stdout
    return float(out.strip())


def run(*args):
    subprocess.run(['ffmpeg', '-v', 'error', '-y', *args], check=True)


def main():
    total = duration(SOURCE)
    body = total - FADE
    # A = source from FADE to the end, B = the first FADE seconds; B fades in over the last FADE seconds of A,
    # so the loop ends on the frame it starts with
    graph = (f'[0:v]trim=start={FADE},setpts=PTS-STARTPTS,scale=1280:720:flags=lanczos,fps=24,format=yuv420p[a];'
             f'[0:v]trim=end={FADE},setpts=PTS-STARTPTS,scale=1280:720:flags=lanczos,fps=24,format=yuv420p[b];'
             f'[a][b]xfade=transition=fade:duration={FADE}:offset={body - FADE}[v]')
    mp4, webm, poster = MEDIA / 'honey-loop-720.mp4', MEDIA / 'honey-loop-720.webm', MEDIA / 'honey-loop-poster.jpg'
    run('-i', str(SOURCE), '-filter_complex', graph, '-map', '[v]', '-an', '-c:v', 'libx264', '-preset', 'slow', '-crf', '28',
        '-profile:v', 'high', '-pix_fmt', 'yuv420p', '-movflags', '+faststart', str(mp4))
    run('-i', str(mp4), '-an', '-c:v', 'libvpx-vp9', '-b:v', '0', '-crf', '40', '-row-mt', '1', '-deadline', 'good', '-cpu-used', '2', str(webm))
    run('-i', str(mp4), '-frames:v', '1', '-q:v', '5', str(poster))
    for f in (mp4, webm, poster):
        print(f'wrote {f.name}: {f.stat().st_size // 1024} KB, {duration(f) if f.suffix != ".jpg" else 0:.2f} s')
    if mp4.stat().st_size > LIMIT or webm.stat().st_size > LIMIT:
        sys.exit('a video is over 3 MB; raise the crf')


if __name__ == '__main__':
    main()
