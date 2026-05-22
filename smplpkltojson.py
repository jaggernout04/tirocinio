import os
import json
import joblib
import numpy as np
from scipy.spatial.transform import Rotation

def convert_aist_to_unity_json(input_folder, output_folder, manifest_path):
    """
    Converts AIST++ pkl files to an AMASS-compatible format with corrected
    Unity spatial orientation, rotation, and proper position scaling.
    """
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    pkl_files = [f for f in os.listdir(input_folder) if f.endswith('.pkl')]
    manifest_lines = []

    AIST_JOINTS = 24  
    AMASS_JOINTS = 52 
    ROTATION_VECTOR_DIMENSIONS = 3
    QUATERNION_DIMENSIONS = 4

    FEMALE_BETAS = [
        0.7485445418795964, -0.654436602910122, -0.07826481329844574, 
        -1.7522458951057311, -0.501135521082701, 1.9893865585425268, 
        -1.5665706780951434, 1.7043194430892066, -2.415117917424295, 
        -0.36359405720012405, -0.8305809003624811, -2.362723252831996, 
        -0.7000814696294306, 0.24171983253744453, 2.4352209678154675, 
        0.3089969850160291
    ]

    print(f"Found {len(pkl_files)} files. Starting conversion with Coordinate System Corrections...")

    for file_name in pkl_files:
        try:
            pkl_path = os.path.join(input_folder, file_name)
            data = joblib.load(pkl_path)
            
            poses_raw = data['smpl_poses']  
            trans_raw = data['smpl_trans']  
            frames = poses_raw.shape[0]

            # --- 1. FIX TRANSLATION SCALING & COORDINATES ---
            # DO NOT multiply by smpl_scaling here; your raw trans is already large.
            # Convert centimeters/pixels to typical Unity meters by dividing by 100.
            # Also map Right-Hand (X, Y, Z) to Left-Hand Unity space (X, Z, Y)
            trans_unity = np.zeros_like(trans_raw)
            trans_unity[:, 0] = trans_raw[:, 0] / 100.0   # Unity X is Python X
            trans_unity[:, 1] = trans_raw[:, 2] / 100.0   # Unity Y (Up) is Python Z
            trans_unity[:, 2] = trans_raw[:, 1] / 100.0   # Unity Z (Forward) is Python Y
            
            # Anchor translation relative to the first frame so they start at (0,0,0)
            trans_unity = trans_unity - trans_unity[0, :]
            trans_scaled = trans_unity.tolist()
            
            # --- 2. FIX POSE ROTATIONS (AXIS-ANGLE TO QUATERNION) ---
            reshaped_poses = np.reshape(poses_raw, [frames, AIST_JOINTS, ROTATION_VECTOR_DIMENSIONS])
            poses_as_quaternion = np.empty([frames, AMASS_JOINTS, QUATERNION_DIMENSIONS])
            identity_quat = np.array([0.0, 0.0, 0.0, 1.0])

            # Prepare a 90-degree correction matrix along the X-axis to stand the character up
            # adjusting from Right-Handed Z-up to Left-Handed Y-up.
            rotation_correction = Rotation.from_euler('x', -90, degrees=True)

            for frame_idx in range(frames):
                for joint_idx in range(AIST_JOINTS):
                    rot_vec = reshaped_poses[frame_idx][joint_idx]
                    r = Rotation.from_rotvec(rot_vec)
                    
                    if joint_idx == 0:
                        # Apply world orientation adjustment to the ROOT joint only
                        r = rotation_correction * r
                    
                    # Convert rotation matrix to left-handed quaternion values [x, y, z, w]
                    quat = r.as_quat()
                    
                    # Coordinate inversion adjustment for Left-Handed systems
                    poses_as_quaternion[frame_idx][joint_idx] = [-quat[0], quat[2], -quat[1], quat[3]]
                
                # Pad out fingers/hands
                for joint_idx in range(AIST_JOINTS, AMASS_JOINTS):
                    poses_as_quaternion[frame_idx][joint_idx] = identity_quat

            # --- 3. GENERATE JSON PAYLOAD ---
            unity_data = {
                "gender": "female",
                "trans": trans_scaled,
                "poses": poses_as_quaternion.tolist(),
                "betas": FEMALE_BETAS,
                "dmpls": [], 
                "fps": 60     
            }

            json_file_name = file_name.replace('.pkl', '.json')
            output_path = os.path.join(output_folder, json_file_name)
            
            with open(output_path, 'w') as f:
                json.dump(unity_data, f, separators=(',', ':'))
            
            manifest_lines.append(json_file_name)
            #print(f"Successfully converted: {json_file_name}")

        except Exception as e:
            print(f"Error converting {file_name}: {e}")

    with open(manifest_path, 'w') as f:
        f.write("\n".join(manifest_lines))
    
    print("\n--- CONVERSION COMPLETE ---")

if __name__ == "__main__":
    INPUT_DIR = './aist_plusplus_annotations' 
    OUTPUT_DIR = './converted_jsons'          
    MANIFEST = './animations.txt'             

    convert_aist_to_unity_json(INPUT_DIR, OUTPUT_DIR, MANIFEST)