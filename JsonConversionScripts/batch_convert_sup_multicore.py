import os
from pathlib import Path
from tqdm import tqdm
from concurrent.futures import ProcessPoolExecutor
from AmassData import AMASSDataConverter

# --- CONFIGURATION ---
INPUT_ROOT = './BatchInputFolder'
OUTPUT_ROOT = './BatchOutputFolder'
# Leave 1 core free so your OS and mouse don't lag
MAX_WORKERS = max(1, os.cpu_count() - 1) 

def convert_single_file(file_info):
    """Worker function to process one file."""
    full_input_path, full_output_path = file_info
    try:
        # Create folder structure
        os.makedirs(os.path.dirname(full_output_path), exist_ok=True)
        
        # Initialize and write (Ensure AmassData.py is in the same folder)
        converter = AMASSDataConverter(full_input_path, show_messages=False)
        converter.write_to_json(full_output_path)
        return True
    except Exception as e:
        return f"Error on {full_input_path}: {e}"

def main():
    # 1. Collect all tasks
    tasks = []
    print(f"Scanning {INPUT_ROOT} for .npz files...")
    for root, _, files in os.walk(INPUT_ROOT):
        for file in files:
            if file.endswith('.npz'):
                full_input_path = os.path.join(root, file)
                rel_path = os.path.relpath(root, INPUT_ROOT)
                output_file_path = os.path.join(OUTPUT_ROOT, rel_path, Path(file).stem + ".json")
                tasks.append((full_input_path, output_file_path))

    if not tasks:
        print("No files found. Check your INPUT_ROOT path.")
        return

    print(f"Found {len(tasks)} files.")
    print(f"Turbo-charging with {MAX_WORKERS} workers and chunksize=100...")

    # 2. Execute tasks in parallel (Only once!)
    with ProcessPoolExecutor(max_workers=MAX_WORKERS) as executor:
        # Using chunksize=100 reduces the overhead of the Multiprocessing communication
        results = list(tqdm(
            executor.map(convert_single_file, tasks, chunksize=64), 
            total=len(tasks)
        ))

    # 3. Report errors
    errors = [r for r in results if isinstance(r, str)]
    if errors:
        print(f"\nCompleted with {len(errors)} errors.")
        # Optional: Print first 5 errors to see what's wrong
        for err in errors[:5]:
            print(f"  > {err}")
    else:
        print("\n*** ALL FILES CONVERTED SUCCESSFULLY ***")

if __name__ == "__main__":
    main()
