using UnityEngine;
using Unity.Robotics.ROSTCPConnector; // For ROS 2 TCP connector
using pos = RosMessageTypes.Brov.PosMsg;

public class ReferenceUpdater : MonoBehaviour
{
    public bool Controller_mode = true;
    
    [Header("ROS 2 Configuration")]
    public string topicName = "/unity/position"; // Topic name to publish and subscribe to
    public float publishFrequency = 1.0f; // Frequency at which to publish position (Hz)

    [Header("Movement Settings")]
    public float movementSpeed = 5.0f; // Speed of movement
    public float verticalSpeed = 3.0f; // Speed of moving up and down (space and shift)

    private ROSConnection rosConnection;
    private float timeSinceLastPublish = 0f;
    private Vector3 velocity = Vector3.zero;
    
    public void OnTickChange(bool tick)
    {
        Controller_mode = tick;
    }
    void Start()
    {
        // Initialize ROS 2 connection
        rosConnection = ROSConnection.GetOrCreateInstance();

        // Register to publish the position message on the topic
        rosConnection.RegisterPublisher<pos>(topicName);

        // Subscribe to the same topic to receive position updates
        rosConnection.Subscribe<pos>(topicName, HandleReceivedPositionMessage);
    }

    void FixedUpdate()
    {
        if (Controller_mode)
        {
            // Handle movement input
            HandleMovementInput();

            // Move the object based on velocity
            transform.position += velocity * Time.fixedDeltaTime;

            // Handle publishing based on frequency
            timeSinceLastPublish += Time.deltaTime;
        }

        if (timeSinceLastPublish >= 1.0f / publishFrequency)
        {
            // Publish the current position to the topic
            PublishPosition();
            timeSinceLastPublish = 0f;
        }
    }

    void HandleMovementInput()
    {
        // Reset velocity
        velocity = Vector3.zero;

        // WASD controls for movement in the X and Z plane
        if (Input.GetKey(KeyCode.W)) // Forward (increase Z)
            velocity.z = movementSpeed;
        if (Input.GetKey(KeyCode.S)) // Backward (decrease Z)
            velocity.z = -movementSpeed;
        if (Input.GetKey(KeyCode.A)) // Left (decrease X)
            velocity.x = -movementSpeed;
        if (Input.GetKey(KeyCode.D)) // Right (increase X)
            velocity.x = movementSpeed;

        // Space and Shift for moving up and down
        if (Input.GetKey(KeyCode.Space)) // Up
            velocity.y = verticalSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) // Down
            velocity.y = -verticalSpeed;
    }

    void PublishPosition()
    {
        // Create and set up a new Position message
        var positionMsg = new pos
        {
            x = transform.position.z,
            y = transform.position.x,
            z = -transform.position.y
        };

        // Publish the message on the topic
        rosConnection.Publish(topicName, positionMsg);
    }

    void HandleReceivedPositionMessage(pos msg)
    {
        // Update the Unity object's position when a new message is received
        transform.position = new Vector3((float)msg.y, (float)-msg.z, (float)msg.x);
    }
}
