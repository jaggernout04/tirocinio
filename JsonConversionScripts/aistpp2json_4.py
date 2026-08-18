import os
import glob
import pickle
import json
import numpy as np
from scipy.spatial.transform import Rotation as R

def convert_single_pkl_data(data, pitch_correction_deg=90):
    """Processes raw AIST++ dict data into bmlSUP JSON structure."""
    smpl_poses = np.array(data['smpl_poses'])  # Shape: (num_frames, 72)
    raw_trans = np.array(data['smpl_trans'])   # Shape: (num_frames, 3)
    
    # Scale translation to proper meters
    scaling_factor = float(np.array(data['smpl_scaling']).squeeze()) if 'smpl_scaling' in data else 1.0
    scaled_trans = raw_trans / scaling_factor

    # Root orientation correction (+90 deg pitch for upright posture)
    R_upright = R.from_euler('x', pitch_correction_deg, degrees=True)
    corrected_trans = R_upright.apply(scaled_trans)

    num_frames = smpl_poses.shape[0]
    poses_quat_list = []

    for i in range(num_frames):
        frame_72 = smpl_poses[i].flatten()
        aa_matrix = frame_72.reshape(24, 3)

        # 1. Correct Root Joint Orientation (Joint 0)
        root_rotation = R.from_rotvec(aa_matrix[0])
        corrected_root = R_upright * root_rotation
        aa_matrix[0] = corrected_root.as_rotvec()

        # 2. Convert 24 axis-angle joints to quaternions [x, y, z, w]
        quats = R.from_rotvec(aa_matrix).as_quat()

        # 3. Exact 52-joint remapping (SMPL -> SMPL-H / bmlSUP)
        body_joints = quats[:22]  # Joints 0-21 (Body)

        # Left hand (Joint 22 = Wrist, 14 finger pads)
        left_wrist = quats[22:23]
        left_fingers_pad = np.tile([0.0, 0.0, 0.0, 1.0], (14, 1))
        left_hand_full = np.vstack([left_wrist, left_fingers_pad])

        # Right hand (Joint 23 = Wrist, 14 finger pads)
        right_wrist = quats[23:24]
        right_fingers_pad = np.tile([0.0, 0.0, 0.0, 1.0], (14, 1))
        right_hand_full = np.vstack([right_wrist, right_fingers_pad])

        # Combine into full 52-joint skeleton (22 + 15 + 15)
        full_52_quats = np.vstack([body_joints, left_hand_full, right_hand_full])
        poses_quat_list.append(full_52_quats.tolist())

    return {
        "gender": data.get("gender", "female"),
        "trans": corrected_trans.tolist(),
        "poses": poses_quat_list,
        "betas": [0.0] * 16,
        "dmpls": [[0.0] * 8 for _ in range(num_frames)],
        "fps": 60
    }

def batch_convert_folder(input_folder, output_folder, pitch_correction_deg=90):
    """Finds all .pkl files in input_folder and converts them to bmlSUP JSON in output_folder."""
    os.makedirs(output_folder, exist_ok=True)
    pkl_files = glob.glob(os.path.join(input_folder, "*.pkl"))

    if not pkl_files:
        print(f"No .pkl files found in '{input_folder}'.")
        return

    print(f"Found {len(pkl_files)} .pkl files. Starting batch conversion...\n")

    for index, filepath in enumerate(pkl_files, start=1):
        filename = os.path.basename(filepath)
        output_filename = os.path.splitext(filename)[0] + ".json"
        output_filepath = os.path.join(output_folder, output_filename)

        print(f"[{index}/{len(pkl_files)}] Converting {filename} -> {output_filename}...")

        try:
            with open(filepath, 'rb') as f:
                data = pickle.load(f)

            bmlsup_json = convert_single_pkl_data(data, pitch_correction_deg=pitch_correction_deg)

            with open(output_filepath, 'w', encoding='utf-8') as f:
                json.dump(bmlsup_json, f)

        except Exception as e:
            print(f"  ❌ Error processing {filename}: {e}")

    print("\nBatch conversion complete!")

# --- RUN BATCH CONVERSION ---
INPUT_DIR = "./aist_plusplus_annotations"    # Folder containing your .pkl files
OUTPUT_DIR = "./converted_jsons" # Destination folder for converted .json files

batch_convert_folder(INPUT_DIR, OUTPUT_DIR)