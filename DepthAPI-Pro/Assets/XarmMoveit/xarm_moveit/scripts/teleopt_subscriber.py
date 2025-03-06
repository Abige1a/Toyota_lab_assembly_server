import rospy
import tf
from xarm_moveit.msg import VehicleControl
from geometry_msgs.msg import Twist,Pose



def send_twist(twist,frequence):
    vehicle_twist=rospy.Publisher('/managed/joy', Twist, queue_size=10)

    rate = rospy.Rate(frequence)

    for i in range(frequence):
        vehicle_twist.publish(twist)
        rospy.loginfo("publishing twist at time{}.".format(i))
        rate.sleep()
def twist_callback(msg_sed):
    # This function will be called whenever a new Twist message is received
    # Access the linear and angular velocities from the msg object

    rotation_msg=msg_sed.head_pose.orientation
    twist_value=msg_sed.twist_value
    rospy.loginfo("head rotation {}.".format(rotation_msg))
    send_twist(twist_value,10)


    linear_velocity = twist_value.linear
    angular_velocity = twist_value.angular

    rospy.loginfo("linear velocity {}. angular velocity {}.".format(linear_velocity,angular_velocity))
    
    # Process the velocities as needed
    # ...
    
    rospy.loginfo("x {}. y {}. z {}. w{}. ".format(rotation_msg.x,rotation_msg.y,rotation_msg.z,rotation_msg.w))
    
    euler_angle=tf.transformations.euler_from_quaternion([rotation_msg.x,rotation_msg.y,rotation_msg.z,rotation_msg.w])

    print("euler_angle is {}.".format(euler_angle))
    
    


def test():
    # Initialize the ROS node
    rospy.init_node('twist_subscriber')

    # Create a subscriber
    rospy.Subscriber('/turtle1/cmd_vel', VehicleControl, twist_callback)

    # Spin the node to receive incoming messages
    rospy.spin()

if __name__ == '__main__':
    test()



