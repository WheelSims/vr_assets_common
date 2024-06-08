import os
import subprocess

for (dirpath, dirnames, filenames) in os.walk("."):
    for filename in filenames:
        file = os.path.join(dirpath, filename)
        if ".psd" in filename.lower() and ".meta" not in filename.lower():
            subprocess.call(["convert", file + "[0]", file[:-3]+"png"])
            os.rename(file+".meta", file[:-3]+"png.meta")
            os.remove(file)
            