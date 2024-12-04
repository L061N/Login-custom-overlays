# This script copies every overlay we want to publish from the SimHub folder.
# We use robocopy so it only copies changed files, and removes deleted files.

from subprocess import call

simhub_folder = "C:\\Program Files (x86)\\SimHub"
overlays_folder = "..\\Overlays"
overlays_to_copy = [
    "benofficial2 - iRacing Dash", 
    "benofficial2 - iRacing Delta", 
    "benofficial2 - iRacing Inputs", 
    "benofficial2 - iRacing Relative", 
    "benofficial2 - iRacing Standings", 
    "benofficial2 - iRacing Track Map"]

for overlay_name in overlays_to_copy:
    call(["robocopy", simhub_folder + "\\DashTemplates\\" + overlay_name, overlays_folder + "\\" + overlay_name, "/MIR"])
