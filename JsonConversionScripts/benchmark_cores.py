import os
import shutil
import subprocess
import time
import csv

# --- CONFIGURATION ---
# Paths to the scripts you want to benchmark
SCRIPT_SINGLE = "./batch_convert_sup.py"
SCRIPT_MULTI = "./batch_convert_sup_multicore.py"

# Folders to handle cleanup for accurate timing
INPUT_ROOT = './BatchInputFolder'
OUTPUT_ROOT = './BatchOutputFolder'

# Core counts you want to test for the multicore script
TOTAL_CORES = os.cpu_count()
CORE_CONFIGS = sorted(list(set([2, 4, 8, 12, 15, TOTAL_CORES])))
CORE_CONFIGS = [c for c in CORE_CONFIGS if c <= TOTAL_CORES]

# Number of trials per configuration to average out OS scheduling fluctuations
NUM_TRIALS = 3

def clear_output_directory():
    """Wipes the output directory to ensure a cold-run scenario for every test."""
    if os.path.exists(OUTPUT_ROOT):
        shutil.rmtree(OUTPUT_ROOT)
    os.makedirs(OUTPUT_ROOT, exist_ok=True)

def run_trial(script_path, core_count=None):
    """Executes the script as a subprocess."""
    clear_output_directory()
    
    env = os.environ.copy()
    if core_count is not None:
        env["BENCHMARK_MAX_WORKERS"] = str(core_count)
    
    start_time = time.perf_counter()
    try:
        result = subprocess.run(
            ["python", script_path],
            env=env,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            check=True
        )
        elapsed = time.perf_counter() - start_time
        return elapsed
    except subprocess.CalledProcessError as e:
        script_name = os.path.basename(script_path)
        print(f"\n[ERROR] Crash detected in {script_name} (Cores: {core_count}).")
        print(e.stderr)
        return None

def main():
    # Verify scripts exist
    if not os.path.exists(SCRIPT_SINGLE) or not os.path.exists(SCRIPT_MULTI):
        print("Error: One or both conversion scripts could not be found.")
        print(f"Expecting: {SCRIPT_SINGLE} and {SCRIPT_MULTI}")
        return

    print("=" * 70)
    print("      THESIS BENCHMARK: FULL SCALABILITY ANALYSIS     ")
    print("=" * 70)
    print(f"Detected System Cores: {TOTAL_CORES}")
    print(f"Running {NUM_TRIALS} trials per configuration to compute stable averages.\n")

    results_data = []
    
    # --- 1. Test the Original Sequential Script ---
    print(f"Testing Original Script (Sequential) | ", end="", flush=True)
    trial_times_single = []
    for trial in range(1, NUM_TRIALS + 1):
        print(f"T{trial}...", end="", flush=True)
        elapsed = run_trial(SCRIPT_SINGLE)
        if elapsed is not None:
            trial_times_single.append(elapsed)
        time.sleep(1)
        
    if trial_times_single:
        avg_single = sum(trial_times_single) / len(trial_times_single)
        min_single = min(trial_times_single)
        baseline_time = avg_single # Use this as the true baseline for speedup
        print(f" Done! Avg: {avg_single:.2f}s")
        
        results_data.append({
            "Configuration": "Sequential",
            "Avg_Time_Sec": round(avg_single, 4),
            "Min_Time_Sec": round(min_single, 4),
            "Speedup": 1.0,
            "Efficiency_Pct": "-"
        })
    else:
        print(" Failed.")
        baseline_time = None

    # --- 2. Test the Multicore Script ---
    if baseline_time is None:
         print("\nBaseline failed. Cannot calculate relative metrics.")
         return

    print("-" * 70)
    print(f"Testing Multicore Configurations: {CORE_CONFIGS}")
    print("-" * 70)

    for cores in CORE_CONFIGS:
        print(f"Testing Cores: {cores:<2} | ", end="", flush=True)
        trial_times = []
        
        for trial in range(1, NUM_TRIALS + 1):
            print(f"T{trial}...", end="", flush=True)
            elapsed = run_trial(SCRIPT_MULTI, core_count=cores)
            if elapsed is not None:
                trial_times.append(elapsed)
            time.sleep(1)
            
        if not trial_times:
            print(" Failed entirely.")
            continue
            
        avg_time = sum(trial_times) / len(trial_times)
        min_time = min(trial_times)
            
        speedup = baseline_time / avg_time
        efficiency = (speedup / cores) * 100
        
        print(f" Done! Avg: {avg_time:.2f}s | Speedup: {speedup:.2f}x | Efficiency: {efficiency:.1f}%")
        
        results_data.append({
            "Configuration": f"Multicore ({cores})",
            "Avg_Time_Sec": round(avg_time, 4),
            "Min_Time_Sec": round(min_time, 4),
            "Speedup": round(speedup, 2),
            "Efficiency_Pct": round(efficiency, 1)
        })

    # --- SAVE TO DATASET FILE ---
    csv_file = "full_performance_metrics.csv"
    with open(csv_file, mode="w", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=["Configuration", "Avg_Time_Sec", "Min_Time_Sec", "Speedup", "Efficiency_Pct"])
        writer.writeheader()
        writer.writerows(results_data)
        
    print("\n" + "=" * 70)
    print(f"BENCHMARK COMPLETE! Metrics exported seamlessly to: {csv_file}")
    print("=" * 70)

if __name__ == "__main__":
    main()