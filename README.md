# Text to 3D Animation Elaboration through Generative AI

This project aims to create a easy-to-use GUI Based solution to generate 3D animations from text using Generative AI and the AMASS database.

## Description

This project is based on SMPLH animations from the AMASS database, and a text description of such from the HumanML3D Dataset, it using the bmlSUP .npz to .json converter to 
translate the animation in a readable format. Than those are loaded by giving the script a .txt file to bmlSUP indicate which animation to load.

## Getting Started

### Dependencies

* This project uses Unity 2022.3
* To use the JSON converter Python3 is needed

### Installing

* How/where to download your program
* Any modifications needed to be made to files/folders

### Executing program

Inside the Prefab folder, you can find SUPBatchLoader.
Put it in the scene and it will execute based on the options provided in the inspector or when instantiated by script.
Inside Scripts/Settings you will find all options for Models and Settings the same as the [bmlSUP](#bmlSUP)

<!--- 
## Help



## Authors


## Version History
--->

## License

This project is licensed under the [MIT] License - see the LICENSE.md file for details

## Acknowledgments
SMPL Model
[SMPL Website](https://smpl.is.tue.mpg.de/index.html)

<a name="bmlSUP">Biomotion Lab SMPL Unity Player (bmlSUP)</a>
[BioMotionLab/SUP](https://github.com/BioMotionLab/SUP)

HumanML3D: 3D Human Motion-Language Dataset
[ricGuo5513/HumanML3D](https://github.com/EricGuo5513/HumanML3D)
