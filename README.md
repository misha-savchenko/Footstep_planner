# Rough Terrain Exploration With a Legged Humanoid Robot

# Introduction

Legged robots are capable of traversing terrains the are simply inaccessible to wheeled type of robots, they have the potential of freely moving around environment designed for humans which is why they are so interesting.Their applications range from being an invaluable tool during catastrophes, such as a nuclear reactor meltdown, where legged robots are capable accessing environment when it is too dangerous for humans to enter, or in more commercial applications, in more human oriented environments for example, which are specifically designed for humans and which other robots may encounter problems navigating.However legged robot come with additional problems that need to be solved in order for them to become efficient and widespread. Bipedal robots, especially must maintain a degree of stability as they move around, even more so when their environment is rough and unstructured. While there is a number of approaches to ensure this stability of a legged robot during its locomotion one way to approach the traversal of the unstructured terrain is to ensure the stability of the footsteps that the robot will take throughout the terrain, thus guaranteeing a level of stability for walking pattern that may be generated with the help of such a model as Zero Point Momentum (ZMP) or 3D-LIMP.

The stability of a footstep can be described by using the characteristics of the terrain itself. As Chestnutt [1] describes in their paper the terrain traversability can be described via the slope, step height, roughness, and curvature of the area. Another problem in traversing unstructured terrain is finding the right sequence of footstep, a number of different methods were proposed that generate the sequence of footstep the most common, amongst the literature I have encountered is to use graph search techniques like A* search and its family after analyzing the terrain in question as a traversability grid where each cell contains the information about the local terrain data and giving each cell a cost based on the traverisiblity estimation, thus converting the terrain into a traversability map and letting the graph search algorithm find a path through out the terrain.

For this project I have chosen a different method that relies on variations of the RRT algorithms, t-RRT[2] and a hierarchical-RRT[3] algorithm and a hierarchical structure for their implementation[4]. The Unity engine served as a platform for visualization of my application of these algorithms.

# Methods

## Unity
The simulation is built with the Unity engine, mostly used for game development unity offers a number of features that are helpful for its application in the field of robotics. Unity engine offers powerful scripting framework that allows for extensive control and manipulations over the objects and models used in this simulation. The main reason for this choice for this problem was the research on the which framework to use, unity seemed to offer many of the features I was looking for especially in term of visualization.

Since Unity is primarily built for making video games, aside from the ability of moving the camera during the simulation and conversion of the heightmap into the terrain mesh all of the code was written from scratch.

## Traversability Map
Before attempting to find a path through our terrain we must characterize the given terrain by the degrees of travesibility at each point. The approach to this problem is inspired by [4] where the data on the terrain was separated into grid cells and these grid cells were analyzed by the data points in them and the nearby grid cells. Since my maps were generated from height maps, grayscale images where white is the highest point and black is the lowest point of the map I treated each pixel as a separate point and used the built in function of Unity to get the pixel’s elevation, interpolated slope and roughness of the surrounding area. The step height value of each pixel was derived by collecting the point in the nearby region and comparing them to the point in being analyzed In a manner this processes is exactly the same as described by [4] but each pixel is a grid cell.

## Transition Based RRT
Transition based RRT or t-RRT is a variant of the RRT that I am using in order to find a global path through which our footstep will be planned. t-RRT combines the exploratory strength of the RRT algorithm together with the stochastic optimization methods in order to generate the least cost path. Since we are working with an unstructured terrain which we convert into a traversability map that the t-RRT is used to find the global path described by a series of nodes that are supposed to guarantee a traversable path through the terrain.

![alt text](https://i.imgur.com/xCtXZAL.png "Transition Test")

Transition Test Function

A more detailed look at this algorithm shows that to find the least cost path the algorithm uses the its TransitionTest to only accept point that are only of lower cost current nearest node, however that plunges the search into local minima. To escape the local minimas the function the cost of the new potential node over time. As more and more nodes get rejected the parameter T increases and if sufficient number of rejection occurs a node of higher cost than the current one can be added to the tree.

The cost of each nodes depends on the the cost function provided. This cost function can be changed based on the kind of quality of the path that is desired. For this project I used the most simple the same cost function that graded each point of the traversability map. Additionally I attempted to add a heuristic that will prioritize conformations closer to goal point however that significantly weakened the exploration strength of the algorithm and since that was one of the reason I chose it t-RRT for this project I decided against further use of it.

## Hierarchical RRT

![alt text](https://i.imgur.com/589QpTz.png "Hierarchical-RRT")
Hierarchical-RRT

Hierarchical RRT is used to generate a sequence of footsteps between each of the nodes generated by the t-RRT. It works in a similar fashion as the multi-RRT algorithm where each step a number of nodes generated. In this case however the nodes are footstep that the robot can take to ensure a stable ZMP-type of gait.These footsteps must be within the reachable area of the non-swing foot and adhere to the stable transition of the robot model in use. While there is no set number of different nodes that can be generated each turn, there is a trade off between using too few and too many of them. While giving the Hierarchical RRT too few nodes decreases the time between each step it also limits the movement capabilities of the robot, in order to make a sharp turn the robot will require many more steps or a much wider breath of path. Too many specified footstep grows the number of expanded nodes significantly thus increasing the time between each step, however it give room for a smoother movement.

![alt text](https://i.imgur.com/peAEh1E.png "AlignExtend ")
AlignExtend

Another core feature of Hierarchical-RRT algorithm is the use of AlignExtend function. This function is used for adjustment of each generated node that is either in collision or who’s cost is too high. This function works by using a specified range for the configuration of parameters for each node. Generating a specified amount of random configurations from that range the nodes are tested for cost and collision, the first node to be accepted is the node that added to the tree and the rest of the nodes from this function are discarded.
Hierarchical-RRT is also capable of adjusting the gait of the robot by defining which node is the “closest”. Instead of using simple euclidean distance the algorithm can be changed to consider the orientation of each potential and chooses the best combination based on the weights given. A number of different approaches were described in the paper but for this project I chose quickly expanding orientation for the purpose of higher rate of local terrain exploration, as it showed a much better performance over longer paths and generated a higher quality gait.

## Overall Structure

The overall structure was inspired by the a similar hierarchical implementation of footstep checking as described in [5]. The hierarchical structures checks the potential region for a footstep starting from a broad region and reducing the size of a region until only the foot itself fits into the specified region. This structure was implemented in attempt to increase the computational efficiency when traversing terrain with large areas of low cost surface. As soon as the traversability cost function identifies the area of 0 cost, we can immediately add our potential nodes to the tree and move on to the next step. If, however our error checking does identify a non zero cost for our area we proceed a layer deeper to check a smaller area, and if
needed again we check the exact area of the foot for fitting to validate this potential step.

The following image gives a general idea of hierarchical ordering of the algorithms and their relationship between each other and how they function.

![alt text](https://i.imgur.com/czjcMcC.png "Overall Structure")
Overall Structure

# Results
First we evaluate the ability of the t-RRT algorithm to find a path through rough terrain. I approached testing this algorithm by looking at three variations of the terrain, stair case, a smooth path through rough but passable terrain, and a doorway.

## Traversability Map Visualization
Travesibility map visualization takes in 4 parameters to determine high and low cost areas, 3 weights that determine how we treat the characteristics of the local region and the 4th parameter determines the size of the region. The weights have a significant impact on the way the terrain is treated and what counts for passable and impassable. At the same time the size of a local region acts as a padding designating certain regions as high cost or not based on all the point in the area. We can see the effect of varying these input in the following images.

Balanced: Slope Weight - 0.5, Step Height Weight - 0.25, Roughness Weight - 0.25

![alt text](https://i.imgur.com/VkfaYTq.png "Balanced")

High Slope Weight: Slope Weight - 1 , Step Height Weight - 0, Roughness Weight - 0

![alt text](https://i.imgur.com/4BsCvFG.png "High Slope Weight")

High Step Height Weight: Slope Weight - 0 , Step Height Weight - 1, Roughness Weight - 0

![alt text](https://i.imgur.com/zbFgPMc.png "High Step Height Weight")

High Roughness Weight: Slope Weight - 0 , Step Height Weight - 0, Roughness Weight - 1

![alt text](https://i.imgur.com/6DwkjxA.png "High Roughness Weight")

High Local Area: 15

![alt text](https://i.imgur.com/kOP7f7C.png "High Local Area")

Small Local Area: 3

![alt text](https://i.imgur.com/9qI2KOi.png "Small Local Area")



## t-RRT

![](https://i.imgur.com/BqJeobI.gif)

![](https://i.imgur.com/Jby7gE0.gif)

![](https://i.imgur.com/1IUawSV.gif)


My variation of the t-RRT algorithm includes interpolation for additional checking, which is done by subdividing the distance of between the new node and the closest node into ten point and checking each points. While this does help to avoid setting points that cannot be reached when the maximum distance to a new node (epsilon) is small relative to the terrain and the obstacles. Problems bebegin to occur we increase the size of epsilon relative to the size of the terrain and the obstacles

![](https://i.imgur.com/tc1QJKo.gif)

As we can see in the example points are being considered that are otherwise unreachable by the robot. We can see the beginning of this problem in the second gif with rough but passable terrain and it becomes worse as we increase the epsilon value. Nodes of the global path are generated in places where while it is possible for the robot to transverse are not optimal when considering stability.

The obvious solution for this problem is to decrease the size of epsilon and make the algorithm check over smaller distances however this creates a new problem where some reachable area is ignored because a passable obstacle is in the way. This is most noticeable when trying to find a path through the staircase. Since the epsilon value is too small it identifies the riser as an impassable obstacle and does not proceed forward. I looked at this problem extensively as I was working on other part of the and came to the conclusion that this is in fact the limitation the way I analyzing my height map. Since each pixel of the heightmap only holds in the height the unity engine scratches points between them to account for the difference in height thus creating a slope of nearly 90 degrees,  that slope is rejected by the cost function thus creating an impassable obstacle. To really develop this algorithm forward I would like to develop a different way for analyzing creating terrain.



## Generating Steps

The constraints of this planner depend highly on the model of the robot that which we would like to apply our algorithm too. First there are reachable area constraints, which is the convex area in which the robot can put down a table conformation of its swing foot. Since I am using the values stated in [3] and only scaling them to the appropriate size of the terrain, I am considering the reachability area as the union of all of the regions where the set of predefined footstep can adjust their position. A second level of constraint can be considered the orientations of the foot itself. While the limits of yaw are also predetermined by just like the example of in [3]  the roll and the pitch limits of the foot have to be chosen based on the model. As we derive the interpolated normal to the surface of the terrain we also have to evaluate the roll and pitch that the foot will have to take in order to have a stable position on the plane.

Hierarchical-RRT (HRRT) is able to generate the sequence of steps to reach each midpoints that are by the t-RRT however parameters for this algorithm must be carefully chosen. The distance between each points of the path affects the performance of the HRRT. The longer distance between the points of the path the longer RRT will take to solve and the more deviation from the specified path will occur, which is not so much of a problem on its own, but when there is a sequence of sharp turn that the robot must take time it takes to find the necessary footstep increase significantly. Similar problem occurs epsilon is small compared to the size of the footsteps. This can be seen the two following examples, it is important to note that these simulation were are shown 10 times the original speed, the first taking 5 and a half minutes before finishing and the second one does not finish after 10 minutes.

High epsilon value and small footstep
![](https://i.imgur.com/PRVMtGs.gif)

Low epsilon value and large footstep
![](https://i.imgur.com/CoxMuE3.gif)


The best options is to tune the size of the footsteps and the epsilon used to the t-RRT, these tuned parameters may produce a much better sequence of steps as we can see in the following example.

Balanced epsilon and footstep size
![](https://i.imgur.com/RQj2W0O.gif)





# Conclusion
While it is possible for the described setup to discover a sequence of stable footprints that the our robot can use to traverse the unstructured rough terrain, when applying this algorithm the parameters for both layers of the search must be carefully chosen. The general structure of this approach accounts for the ability to swap the two parts of the layout as only minimal data is passed the two algorithms.

As we have seen the quality of the path and the efficiency in which it is found depends on both algorithms you can replace the t-RRT with a completely different approach as long as the input into the HRRT consists out a set of points that described the global path the our robot can take. HRRT is much more rooted in this model but with some modification it can also be swapped with a different algorithm as well. As mentioned above the two input points actually describe an area of traversability in the algorithm can find the stable footsteps. The implementation of a different algorithm for this layer is possible in this regard. Algorithms from the A* search family, designed for generating footsteps for legged robots, need to only convert the area described by the two nodes into a grid and search through it.

Unfortunately I was not able to build a complete simulation of this algorithm in Unity. The current best version that I can demonstrate shows instability as it encounters major obstacles and needs to check for collisions. I tried various methods of detecting impossible configurations from changing the cost function to using different built in unity methods however I wasn't able to solve this issue. I will continue working on this problem outside the scope of this course until it is fully flushed and can properly demonstrates the benefits and the shortcomings of these algorithms and their application.

The simulations below are only capable of showing how the global path is found and how the steps are generated. When they encounter obstacles the programs begin to crash and rarely result in the complete set of steps that we are looking for. These simulations do however show that the parameters for each layer must be chosen carefully. Picking parameters arbitrarily at best take a long time and yield a low quality sequence of footsteps, in the worst case the parameters cause the algorithms to increase the search time to impractical levels or not generating a result whatsoever.

There are other areas in which this project could be improved. There are a number ways in which the t-RRT algorithm can be extended. Dynamic-Domain RRTs can improve the rate of finding paths from bug-trap type of scenarios. Letting the algorithm run longer and choosing the best path is also another viable option that can be easily implemented, as well as the shortcutting methods that we discussed in the class. For the HRRT identifying the relationship between the constraints of the reachability area, the number of footsteps expended each turn, and the exploration of addition gait heuristics that chosen the best nearby node in the tree will yield better results.

While I did not achieve the results I have expected in this project I because intimately familiar with RRT algorithms. I certainly have a better feeling of their limitations and strong point and how in which ways they can be applied to other motion planning problems outside of legged locomotion.

## Using and Installing

To download this repository you can use the following string in the command line
`git clone git@gitlab.com:Scathach/height_map.git`
Or just download the zip file with the link on the top of the page. To test some of the simulation either launch the the following files directly or if you are familiar with the unity setup you can import the project into you Unity workspace and change the different variables in the comm script on the right to see how the simulation reacts.

The following executable file are the simulation that showcase the some of my discussion:

* high_epsilon_low_footsstep_size
* low_epsilon_high_footsstep_size
* medium_epsilon_medius_footsstep_size
* trrt_doorway
* trrt_rough_path
* trrt_stair_case

## Additional Comments


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
