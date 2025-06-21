# This script copies every image library we want to publish from the SimHub folder.
# We use robocopy so it only copies changed files, and removes deleted files.

from subprocess import call

simhub_folder = "C:\\Program Files (x86)\\SimHub"
images_folder = "..\\Images"
libraries_to_copy = ["CarLogos", "Flairs"]

def copy_libraries_from_simhub():
    for library_name in libraries_to_copy:
        call(["robocopy", simhub_folder + "\\ImageLibrary\\benofficial2\\" + library_name, images_folder + "\\" + library_name, "/MIR"])

copy_libraries_from_simhub()

