# This script copies every overlay we want to publish from the SimHub folder.
# We use robocopy so it only copies changed files, and removes deleted files.

from subprocess import call
import os
import shutil

simhub_folder = "C:\\Program Files (x86)\\SimHub"
overlays_folder = "..\\Overlays"
overlays_to_copy = [
    "benofficial2 - iRacing Dash", 
    "benofficial2 - iRacing Delta", 
    "benofficial2 - iRacing Inputs", 
    "benofficial2 - iRacing Relative", 
    "benofficial2 - iRacing Standings", 
    "benofficial2 - iRacing Track Map",
    "benofficial2 - iRacing Setup Cover",
    "benofficial2 - Twitch Chat"]

def copy_overlays_from_simhub():
    for overlay_name in overlays_to_copy:
        call(["robocopy", simhub_folder + "\\DashTemplates\\" + overlay_name, overlays_folder + "\\" + overlay_name, "/MIR"])

def delete_backup_folders(start_path):
    for root, dirs, files in os.walk(start_path, topdown=False):
        for folder in dirs:
            if folder == "_Backups":
                folder_path = os.path.join(root, folder)
                try:
                    shutil.rmtree(folder_path)
                    print(f"Deleted: {folder_path}")
                except Exception as e:
                    print(f"Failed to delete {folder_path}: {e}")

copy_overlays_from_simhub()
delete_backup_folders(overlays_folder)
