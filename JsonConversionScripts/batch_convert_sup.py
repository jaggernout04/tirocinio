import os
from pathlib import Path
from tqdm import tqdm
from AmassData import AMASSDataConverter

# --- CONFIGURATION ---
input_root = './BatchInputFolder'
output_root = './BatchOutputFolder'

# Create output directory if it doesn't exist
os.makedirs(output_root, exist_ok=True)

# 1. Walk through the input directory
# This handles all subfolders (ACCAD, CMU, etc.) automatically
for root, dirs, files in os.walk(input_root):
    # Filter for .npz files
    npz_files = [f for f in files if f.endswith('.npz')]
    if not npz_files:
        continue

    # 2. Recreate the same folder structure in the output
    rel_path = os.path.relpath(root, input_root)
    dest_path = os.path.join(output_root, rel_path)
    os.makedirs(dest_path, exist_ok=True)

    print(f"\nProcessing Folder: {rel_path}")

    # 3. Batch Convert
    for file in tqdm(npz_files):
        full_input_path = os.path.join(root, file)
        # Change .npz extension to .json
        output_file_path = os.path.join(dest_path, Path(file).stem + ".json")

        try:
            # Initialize the converter (This runs the quaternion conversion automatically)
            # show_messages=False keeps the console from being flooded
            converter = AMASSDataConverter(full_input_path, show_messages=False)
            
            # Write the file
            converter.write_to_json(output_file_path)
            
        except Exception as e:
            # If a file is corrupted or missing keys, it will skip and keep going
            print(f"\n[Error] Failed to convert {file}: {e}")

print('\n*** BATCH CONVERSION COMPLETE ***')
