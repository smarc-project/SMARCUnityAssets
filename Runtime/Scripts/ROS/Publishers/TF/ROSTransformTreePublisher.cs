using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using ROS.Core;

namespace ROS.Publishers
{
    public class ROSTransformTreePublisher : ROSBehaviour
    {
        TransformTreeNode BaseLinkTreeNode;
        string prefix;

        
        [Header("TF Tree")]
        public string BaseLinkName = "base_link";
        GameObject BaseLinkGO;
        GameObject OdomLinkGO;

        public float Frequency = 10f;

        
        float period => 1.0f/Frequency;
        double lastUpdate;

        TFMessageMsg finalMsg;
        bool registered = false;


        void OnValidate()
        {
            if (period < Time.fixedDeltaTime)
            {
                Debug.LogWarning($"TF Publisher update frequency set to {Frequency}Hz but Unity updates physics at {1f / Time.fixedDeltaTime}Hz. Setting to Unity's fixedDeltaTime!");
                Frequency = 1f / Time.fixedDeltaTime;
            }

            if (topic != "/tf")
            {
                Debug.LogWarning($"TF Publisher topic set to {topic} but should be /tf. Setting to /tf!");
                topic = "/tf";
            }

            if (transform.rotation != Quaternion.identity)
            {
                Debug.LogWarning($"[{transform.name}] TF Publisher transform (probably the root robot object: {transform.name}) is not identity, this will cause issues with the TF tree! Resetting to identity.");
                transform.rotation = Quaternion.identity;
            }
            
        }

        protected override void StartROS()
        {
            OdomLinkGO = Utils.FindParentWithTag(gameObject, "robot", true);
            if(OdomLinkGO == null)
            {
                Debug.LogError($"No #robot tagged parent found for {gameObject.name}! Disabling.");
                enabled = false;
            }
            prefix = OdomLinkGO.name;

            // we need map(ENU) -> odom(ENU) -> base_link(ENU) -> children(FLU)

            BaseLinkGO = Utils.FindDeepChildWithName(OdomLinkGO, BaseLinkName);
            if(BaseLinkGO == null)
            {
                Debug.LogError($"No {BaseLinkName} found under {OdomLinkGO.name}! Disabling.");
                enabled = false;
                return;
            }
            BaseLinkTreeNode = new TransformTreeNode(BaseLinkGO);


            if (!registered)
            {
                rosCon.RegisterPublisher<TFMessageMsg>(topic);
                registered = true;
            }
            
        }

        static void PopulateTFList(List<TransformStampedMsg> tfList, TransformTreeNode tfNode)
        {
            // TODO: Some of this could be done once and cached rather than doing from scratch every time
            // Only generate transform messages from the children, because This node will be parented to the global frame
            foreach (var childTf in tfNode.Children)
            {
                tfList.Add(TransformTreeNode.ToTransformStamped(childTf));

                if (!childTf.IsALeafNode)
                {
                    PopulateTFList(tfList, childTf);
                }
            }
        }


        void PopulateGlobalFrames(List<TransformStampedMsg> tfMessageList)
        {
            // we want globally oriented transforms to be the first in the list.
            // map -> odom and odom -> base_link
            // map frame in unity is the unity origin, so we want a 0-transform for that
            // odom frame is cached in StartROS, it is the position of the robot at game start
            // we want the transform from base_link to odom

            var mapToOdomMsg = new TransformMsg
            {
                translation = OdomLinkGO.transform.To<ENU>().translation,
            };
            var mapToOdom = new TransformStampedMsg(
                new HeaderMsg(new TimeStamp(Clock.time), $"map_gt"),
                $"{prefix}/odom",
                mapToOdomMsg);
            tfMessageList.Add(mapToOdom);

            // base_link is the robot's main frame, so we want to publish the transform from odom to base_link
            var rosOdomPos = ENU.ConvertFromRUF(BaseLinkTreeNode.Transform.localPosition);
            var rosOdomOri = ENU.ConvertFromRUF(BaseLinkTreeNode.Transform.localRotation);
            var odomToBaseLinkMsg = new TransformMsg
            {
                translation = new Vector3Msg(
                    rosOdomPos.x,
                    rosOdomPos.y,
                    rosOdomPos.z),
                rotation = new QuaternionMsg(
                    rosOdomOri.x,
                    rosOdomOri.y,
                    rosOdomOri.z,
                    rosOdomOri.w)
            };
            var odomToBaseLink = new TransformStampedMsg(
                new HeaderMsg(new TimeStamp(Clock.time), $"{prefix}/odom"),
                $"{prefix}/{BaseLinkTreeNode.name}",
                odomToBaseLinkMsg);
            tfMessageList.Add(odomToBaseLink);
        }

        void PopulateMessage()
        {
            var tfMessageList = new List<TransformStampedMsg>();
            try
            {
                PopulateTFList(tfMessageList, BaseLinkTreeNode);
            }catch(MissingReferenceException)
            {
                // If the object tree was modified after the TF Tree was built
                // such as deleting a child object, this will throw an exception
                // So we need to re-build the TF tree and skip the publish.
                Debug.Log($"[{transform.name}] TF Tree was modified, re-building.");
                BaseLinkTreeNode = new TransformTreeNode(BaseLinkGO);
                return;
            }
            foreach(TransformStampedMsg msg in tfMessageList)
            {
                msg.header.frame_id = $"{prefix}/{msg.header.frame_id}";
                msg.child_frame_id = $"{prefix}/{msg.child_frame_id}";
            }

            // populate the global frames last, dont wanna prefix those.
            PopulateGlobalFrames(tfMessageList);

            finalMsg = new TFMessageMsg(tfMessageList.ToArray());
        }

        void Update()
        {
            if (Clock.time - lastUpdate < period) return;
            lastUpdate = Clock.time;
            PopulateMessage();
            rosCon.Publish(topic, finalMsg);
        }
    }
}
