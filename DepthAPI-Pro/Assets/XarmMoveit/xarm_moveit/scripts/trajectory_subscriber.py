#!/usr/bin/env python
"""
    Subscribes to SourceDestination topic.
    Uses MoveIt to compute a trajectory from the target to the destination.
    Trajectory is then published to PickAndPlaceTrajectory topic.
"""
import rospy

from xarm_moveit.msg import XarmMoveitJoints, XarmTrajectory
from moveit_msgs.msg import RobotTrajectory


def callback(data):
    rospy.loginfo(rospy.get_caller_id() + "I heard:\n%s", data)

def listener():
    rospy.init_node('Trajectory_Subscriber', anonymous=True)
    rospy.Subscriber("/xarm_joints", XarmMoveitJoints, callback)

    # spin() simply keeps python from exiting until this node is stopped
    rospy.spin()


if __name__ == '__main__':
    listener()
