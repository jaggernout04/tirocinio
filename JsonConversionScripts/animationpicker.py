import os
import random

def generate_animation_file(folder_path=None, num_lines=None, num_cols=None):

    # 2. Check if folder exists
    if not os.path.isdir(folder_path):
        print(f"❌ Error: Folder '{folder_path}' does not exist.")
        return

    # 3. Retrieve all files in the folder (excluding directories)
    files = [
        f for f in os.listdir(folder_path) 
        if os.path.isfile(os.path.join(folder_path, f))
    ]

    if not files:
        print(f"❌ Error: No files found in '{folder_path}'.")
        return

    # 4. Generate random grid of filenames
    output_lines = []
    for _ in range(num_lines):
        row = [random.choice(files) for _ in range(num_cols)]
        output_lines.append(" ".join(row))

    # 5. Write to animation.txt
    output_path = os.path.join(folder_path, "animations.txt")
    with open(output_path, "w", encoding="utf-8") as f:
        f.write("\n".join(output_lines))

    print(f" success! Created '{output_path}' with {num_lines}x{num_cols} matrix.")

if __name__ == "__main__":
    FOLDER_PATH = "./converted_jsons"  # Change this to your folder path
    NUM_LINES = 5  # Number of rows in the output file
    NUM_COLS = 5   # Number of columns in the output file
    generate_animation_file(FOLDER_PATH, NUM_LINES, NUM_COLS)