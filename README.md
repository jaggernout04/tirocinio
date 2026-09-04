# Engineering a Data Pipeline and Unity Architecture for the Visualization of Parametric Virtual Humans (SMPL-H)

This project provides an easy-to-use, GUI-based Unity solution to visualize SMPL-H animations. Built around the **AMASS database**, **AIST++**, it features a data pipeline that converts SMPL-H animation data into a Unity-readable format, allowing seamless playback and control of multiple parametric virtual humans, based around the **BMLSup** plugin.

## Features

* **Complete Data Pipeline:** Fully implemented Python converter to translate `.pkl` animation files into `.json` format for Unity.
* **Custom Animation Control:** Granular control over root position and rotation application directly via text configuration (`{[pos],[rot]}`, `{[pos]}`).
* **GUI & Script Driven:** Execute batch loading easily through the Unity Inspector or via custom scripts.

## Getting Started

### Dependencies

* **Unity:** Version 2022.3 (or newer)
* **Python:** Python 3.8+ (required for running the `.pkl` to `.json` conversion pipeline)
* **BMLSup Plugin:** BiomotionLab SMPL Unity Player dependencies (integrated into the Unity project)

### Installation

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/jaggernout04/SMPLHViewer.git](https://github.com/jaggernout04/SMPLHViewer.git)

2. **Set up the Python environment:**
Ensure Python 3 is installed along with the required libraries for processing SMPL-H annotations and generating .json files.

3. **Open the Unity Project:**
Launch Unity Hub and open the cloned project folder using Unity 2022.3.

## Usage

### 1. Converting Data (.pkl to .json)
Run the Python conversion pipeline to process SMPL-H `.pkl` animation files (derived from AMASS, AIST++, etc.) into Unity-friendly `.json` files. Ensure the output files are stored where the BMLSup loader can access them.
(In the future i will add a guide how to use them)

### 2. Configuring Animations (animation.txt)
Control which animations to load and how root motion (position and rotation) is applied using the `animation.txt` configuration file.
(rotation are in euler angles)

Format options per line:
* `{[pos],[rot]} animation_name, animation_name, ...` - Applies both root position and root rotation.
* `{[pos]} animation_name, animation_name, ...` - Applies root position only.
* `animation_name` - Applies base animation without custom root motion modifiers.

Example `animation.txt`:
```text
{[-4.67,0.0,1.71][0.0,70.3,0.0]} gJB_sBM_cAll_d08_mJB5_ch04.json gKR_sBM_cAll_d28_mKR0_ch02.json gJS_sBM_cAll_d03_mJS3_ch07.json gWA_sBM_cAll_d25_mWA3_ch02.json gHO_sBM_cAll_d19_mHO2_ch01.json
{[-2.52,0.0,4.93][0.0,35.9,0.0]} gPO_sBM_cAll_d10_mPO1_ch07.json gWA_sBM_cAll_d25_mWA2_ch01.json gJB_sBM_cAll_d07_mJB1_ch01.json
```
### 3. Executing in Unity
1. Navigate to the Prefabs folder in Unity.

2. Drag the SUPBatchLoader prefab into your active scene.

3. Select the SUPBatchLoader object in the Hierarchy and configure the inspector settings (pointing to your animation.txt file).

4. Press Play in Unity to load and visualize the virtual human animations.

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.

## Acknowledgments

* **SMPL / SMPL-H Model:** [SMPL Body Model](https://smpl.is.tue.mpg.de/)
* **Biomotion Lab SMPL Unity Player (bmlSUP):** [BioMotionLab/SUP](https://github.com/BioMotionLab/SUP)
* **AMASS Database:** [AMASS Dataset](https://amass.is.tue.mpg.de/)
* **AIST++ Dataset:** [AIST++ Dance Motion Dataset](https://google.github.io/aistplusplus_dataset/)
