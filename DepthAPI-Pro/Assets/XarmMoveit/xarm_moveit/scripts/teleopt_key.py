#!/usr/bin/env python

import rospy
import tf
# import pygame
from xarm_moveit.msg import VehicleControl
from geometry_msgs.msg import Twist, Pose


# def input_from_controller():
    

#     pygame.init()
#     pygame.joystick.init()

#     joystick_count = pygame.joystick.get_count()
#     if joystick_count == 0:
#         print("No joysticks found.")
#         quit()

#     joystick = pygame.joystick.Joystick(0)
#     joystick.init()

#     while True:
#         for event in pygame.event.get():
#             if event.type == pygame.JOYAXISMOTION:
#                 # Handle joystick axis motion
#                 axis = event.axis
#                 value = joystick.get_axis(axis)
#                 # Process the axis value

#             elif event.type == pygame.JOYBUTTONDOWN:
#                 # Handle button press
#                 button = event.button
#                 # Process the button press

#             elif event.type == pygame.JOYBUTTONUP:
#                 # Handle button release
#                 button = event.button
#                 # Process the button release

# def input_from_keyboard():
    
def main():
    # Initialize the ROS node
    rospy.init_node('vehicle_msg_publisher')

    # Create a publisher for the turtle's velocity commands
    velocity_pub = rospy.Publisher('/turtle1/cmd_vel', VehicleControl, queue_size=10)

    # Set the rate at which to publish the velocity commands
    rate = rospy.Rate(1)  # 1 Hz

    

    # Create a Twist message object to store the velocity commands
    twist_msg = Twist()

    # pygame.init()

    rotation=[0.0,0.0,0.0]

    while not rospy.is_shutdown():

        # for event in pygame.event.get():
        #     if event.type ==pygame.QUIT:
                
        #         exit()
        # Get the user input
        cmd = input("Enter a command (w = forward, s = backward, a = left, d = right, q = quit): ")

        

        # Check the user input and set the appropriate velocity command
        if cmd == 'w':
            twist_msg.linear.x = 1.0  # Move forward
        elif cmd == 's':
            twist_msg.linear.x = -1.0  # Move backward
        elif cmd == 'a':
            twist_msg.angular.z = 1.0  # Rotate left
        elif cmd == 'd':
            twist_msg.angular.z = -1.0  # Rotate right

        elif cmd == 'j':
            rotation[2] += -0.1  # gimble yaw left
        elif cmd == 'l':
            rotation[2] += 0.1 # gimble yaw right
        elif cmd == 'i':
            rotation[1] += 0.1  # gimble yaw left
        elif cmd == 'k':
            rotation[1] += -0.1 # gimble yaw right



        elif cmd == 'q':
            break  # Quit the teleoperation
        else:
            print("Invalid command!")

        rotation_msg=Pose()
        
     



        orientation=tf.transformations.quaternion_from_euler(rotation[0],rotation[1],rotation[2])

        rotation_msg.position.x=0
        rotation_msg.position.y=0
        rotation_msg.position.z=0
        rotation_msg.orientation.x = orientation[0]
        rotation_msg.orientation.y = orientation[1]
        rotation_msg.orientation.z = orientation[2]
        rotation_msg.orientation.w = orientation[3]

        


        msg_sed=VehicleControl()
        msg_sed.twist_value=twist_msg
        msg_sed.head_pose=rotation_msg


        # Publish the velocity command
        velocity_pub.publish(msg_sed)

        # Reset the velocity commands
        twist_msg.linear.x = 0.0
        twist_msg.angular.z = 0.0

        # Sleep to maintain the desired publishing rate
        rate.sleep()

if __name__ == '__main__':
    # try:
    #     turtle_teleop()
    # except rospy.ROSInterruptException:
    #     pass
    main()