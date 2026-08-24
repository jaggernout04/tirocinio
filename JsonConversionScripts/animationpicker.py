import os
import random

def generate_animation_file(
    folder_path=None, 
    num_lines=None, 
    num_cols=None,
    x_bounds=(-10.0, 10.0), # (min_x, max_x)
    z_bounds=(-10.0, 10.0)  # (min_z, max_z)
):
    # 1. Check if folder exists
    if not os.path.isdir(folder_path):
        print(f"❌ Error: Folder '{folder_path}' does not exist.")
        return

    # 2. Retrieve all files in the folder (excluding animation.txt / animations.txt)
    files = [
        f for f in os.listdir(folder_path) 
        if os.path.isfile(os.path.join(folder_path, f)) and not f.endswith(".txt")
    ]

    if not files:
        print(f"❌ Error: No valid JSON files found in '{folder_path}'.")
        return

    # 3. Generate lines with custom transform tokens
    output_lines = []
    for _ in range(num_lines):
        # Generate random position on the plane (Y fixed to 0.0)
        px = round(random.uniform(x_bounds[0], x_bounds[1]), 2)
        py = 0.0
        pz = round(random.uniform(z_bounds[0], z_bounds[1]), 2)

        # Generate random rotation angles (X and Z at 0.0, Y random 0-360 deg)
        rx = 0.0
        ry = round(random.uniform(0.0, 360.0), 1)
        rz = 0.0

        # Construct transform string format: {[x,y,z][rx,ry,rz]}
        transform_token = f"{{[{px},{py},{pz}][{rx},{ry},{rz}]}}"

        # Pick random files for this row
        row_files = [random.choice(files) for _ in range(num_cols)]

        # Prepend the transform token to the row
        line_content = f"{transform_token} " + " ".join(row_files)
        output_lines.append(line_content)

    # 4. Write to animations.txt
    output_path = os.path.join(folder_path, "animations.txt")
    with open(output_path, "w", encoding="utf-8") as f:
        f.write("\n".join(output_lines))

    print(f" success! Created '{output_path}' with {num_lines} lines and {num_cols} columns per row.")

if __name__ == "__main__":
    FOLDER_PATH = "./converted_jsons"  # Change to your folder path
    NUM_LINES = 5                      # Number of animation rows/lines
    NUM_COLS = 5                       # Number of animations per line
    
    # Optional area boundaries on the ground plane (meters)
    X_BOUNDS = (-5.0, 5.0)
    Z_BOUNDS = (-5.0, 5.0)

    generate_animation_file(
        folder_path=FOLDER_PATH, 
        num_lines=NUM_LINES, 
        num_cols=NUM_COLS,
        x_bounds=X_BOUNDS,
        z_bounds=Z_BOUNDS
    )