README

- Write up a document (either a PDF or web page) detailing your project
- including motivation, methods, results, and supporting media (images, videos, datasets).
-  This should be sent to me over email.

# Rough Terrain Exploration With a Legged Humanoid Robot

## Introduction

Legged robots are capable of traversing terrains the are simply inaccessible to the wheeled type of robots, however the accessibility to more difficult terrains come with the problem of traversing unstructured rough terrain. One way to approach the traversal of the unstructured terrain is to ensure the stability of it footsteps that the robot will take throughout the terrain.
	The stability of a footstep can be described by using the characteristics of the terrain itself. As Chestnutt [1] describes in their paper the terrain traversability can be described via the slope, step height, roughness, and curvature of the area. Another problem in traversing unstructured terrain is finding the right sequence of footstep, a number of different methods were proposed that generate the sequence of footstep the most common, amongst the literature I have encountered is to use graph search techniques like A* search and its family after analyzing the terrain in question as a traversability grid where each cell contains the information about the local terrain data and giving each cell a cost based on the traverisiblity estimation, thus converting the terrain into a traversability map and letting the graph search algorithm find a path through out the terrain.
	For this project I have chosen a different method that relies on variations of the RRT algorithms, t-RRT[2] and a hierarchical-RRT[3] algorithm and a hierarchical structure for their implementation[4].

# Methods

# Unity
The simulation is built with the Unity engine, mostly used for game development unity offers a number of features that are helpful for its application in the field of robotics. Unity engine offers powerful scripting framework that allows for extensive control and manipulations over the objects and models used in this simulation.

# Transition Based RRT
Transition based RRT or t-RRT is a variant of the RRT that I am using in order to find a global path through which our footstep will be planned. t-RRT combines the exploratory strength of the RRT algorithm together with the stochastic optimization methods in order to generate the least cost path. Since we are working with an unstructured terrain which we convert into a traversability map t-RRT is to find stable path by c
# Hierarchical RRT
Hierarchical RRT is used to generate a sequence of footsteps between each of the nodes generated by the t-RRT. It works in a similar fashion as the multi-RRT algorithm where each step a number of nodes generated. In this case however the nodes are footstep that the robot can take to ensure a stable ZMP-type of gait.These footsteps must be within the reachable area of the non-swing foot and adhere to the stable transition of the robot model in use. While there is no set number of different nodes that can be generated each turn, there is a trade off between using too few and too many of them. While giving the Hierarchical RRT too few nodes decreases the time between each step it also limits the movement capabilities of the robot. Too many specified footstep grows the number of expanded nodes significantly thus increasing the time between each step, however it give room for a smoother movement.
Another core feature of Hierarchical-RRT algorithm is the use of AlignExtend function. This function is used for adjustment of each generated node that is either in collision or who’s cost is too high. This function works by using a specified range of for the configuration of parameters for each node. Generating a specified amount of random configurations from that range the nodes are tested for cost and collision, the first node to be accepted is the node that added to the tree and the rest of the nodes from this function are discarded.
Hierarchical-RRT is also capable of adjusting the gait of the robot by defining which node is the “closest”. Instead of using euclidean distance the algorithm considered the orientation of each potential and chooses the best combination based on the weights given. A number of different approaches were described in the paper but for this project I chose quickly expanding orientation for the purpose of higher rate of local terrain exploration.

# Overall Structure

The overall structure was inspired by the a similar hierarchical implementation of footstep checking as described in [5]. The hierarchical structures checks the potential region for a footstep starting from a broad region and reducing the size of a region until only the foot itself fits into the specified region. This structure was implemented in attempt to increase the computational efficiency when traversing terrain with large areas of low cost surface. As soon as the traversability cost function identifies the area of 0 cost, we can immediately add our potential nodes to the tree and move on to the next step. If, however our error checking does identify a non zero cost for our area we proceed a layer deeper to check a smaller area, and if needed again we check the exact area of the foot for fitting to validate this potential step.

# Conclusion


## Using and Installing

A step by step series of examples that tell you have to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Conclusion

# References
1.   Chestnutt, Joel E.. “Navigation Planning for Legged Robots.” (2006).
    URL: https://pdfs.semanticscholar.org/8762/98fac00a165ecc90bb4f965f77e79ff74732.pdf

2.   L. Jaillet, J. Cortes and T. Simeon, "Transition-based RRT for path planning in continuous cost spaces," 2008 IEEE/RSJ International Conference on Intelligent Robots and Systems, Nice, 2008, pp. 2145-2150.
    doi: 10.1109/IROS.2008.4650993
    URL: http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=4650993&isnumber=4650570

3.   H. Liu, Q. Sun and T. Zhang, "Hierarchical RRT for humanoid robot footstep planning with multiple constraints in complex environments," 2012 IEEE/RSJ International Conference on Intelligent Robots and Systems, Vilamoura, 2012, pp. 3187-3194.
    doi: 10.1109/IROS.2012.6385836
    URL: http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=6385836&isnumber=6385431

4.   A. Chilian and H. Hirschmüller, "Stereo camera based navigation of mobile robots on rough terrain," 2009 IEEE/RSJ International Conference on Intelligent Robots and Systems, St. Louis, MO, 2009, pp. 4571-4576.
    doi: 10.1109/IROS.2009.5354535
    URL: http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=5354535&isnumber=5353884

5.  M. Wermelinger, P. Fankhauser, R. Diethelm, P. Krüsi, R. Siegwart and M. Hutter, "Navigation planning for legged robots in challenging terrain," 2016 IEEE/RSJ International Conference on Intelligent Robots and Systems (IROS), Daejeon, 2016, pp. 1184-1189.
    doi: 10.1109/IROS.2016.7759199
    URL: http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=7759199&isnumber=7758082
