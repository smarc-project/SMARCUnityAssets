using UnityEngine;
using Force;

namespace Rope
{
    public class Pulley : RopeSystemBase
    {
        [Header("Load One")]
        public ArticulationBody LoadOneAB;
        public Rigidbody LoadOneRB;
        MixedBody loadOneBody;
        
        [Header("Load Two")]
        public ArticulationBody LoadTwoAB;
        public Rigidbody LoadTwoRB;
        MixedBody loadTwoBody;

        SpringJoint ropeJointOne;
        SpringJoint ropeJointTwo;
        LineRenderer LROne;
        LineRenderer LRTwo;

        [Header("Debug")]
        public float limitOne;
        public float limitTwo;
        public float limitSum;
        public float ropeSpeed;
        

        protected override void SetupEnds()
        {
            loadOneBody = new MixedBody(LoadOneAB, LoadOneRB);
            loadTwoBody = new MixedBody(LoadTwoAB, LoadTwoRB);
            ropeJointOne = AttachBody(loadOneBody);
            ropeJointTwo = AttachBody(loadTwoBody);
            LROne = ropeJointOne.gameObject.GetComponent<LineRenderer>();
            LRTwo = ropeJointTwo.gameObject.GetComponent<LineRenderer>();
            limitOne = Vector3.Distance(loadOneBody.position, transform.position) + 0.05f;
            limitTwo = Vector3.Distance(loadTwoBody.position, transform.position) + 0.05f;
            ropeSpeed = 0;
            ropeJointOne.maxDistance = limitOne;
            ropeJointTwo.maxDistance = limitTwo;
        }


        void FixedUpdate()
        {
            if(!setup) return;
            
            float distOne = Vector3.Distance(loadOneBody.position, transform.position);
            float distTwo = Vector3.Distance(loadTwoBody.position, transform.position);

            bool oneIsSlack = distOne < limitOne;
            bool twoIsSlack = distTwo < limitTwo;

            // Visualize all the time
            LROne.SetPosition(0, transform.position);
            LROne.SetPosition(1, loadOneBody.position);
            LROne.startColor = oneIsSlack ? Color.green : Color.red;
            LROne.endColor = LROne.startColor;
            LRTwo.SetPosition(0, transform.position);
            LRTwo.SetPosition(1, loadTwoBody.position);
            LRTwo.startColor = twoIsSlack ? Color.green : Color.red;
            LRTwo.endColor = LRTwo.startColor;
            
            // both sides are slack, give maximum slack to the one
            // with the most rope already and tighten the other
            // without pulling the side with the less rope
            if(oneIsSlack && twoIsSlack)
            {
                ropeSpeed = 0;
                if(distOne > distTwo)
                {
                    limitOne = RopeLength - distTwo;
                    limitTwo = RopeLength - limitOne;
                }
                else
                {
                    limitTwo = RopeLength - distOne;
                    limitOne = RopeLength - limitTwo;
                }
            }
            // one side is slack, let the other have all the slack rope
            else if(oneIsSlack)
            {
                ropeSpeed = 0;
                limitTwo = RopeLength - distOne;
                limitOne = RopeLength - limitTwo;
            }
            else if(twoIsSlack)
            {
                ropeSpeed = 0;
                limitOne = RopeLength - distTwo;
                limitTwo = RopeLength - limitOne;
            }
            // no side is slack, pull around
            else
            {
                var pullOne = ropeJointOne.currentForce.magnitude;
                var pullTwo = ropeJointTwo.currentForce.magnitude;
                float ropeAccel = (pullOne-pullTwo) / (loadOneBody.mass + loadTwoBody.mass);
                ropeSpeed += ropeAccel * Time.fixedDeltaTime;
                limitOne += ropeSpeed * Time.fixedDeltaTime;
                limitOne = Mathf.Clamp(limitOne, 0, RopeLength);
                limitTwo = RopeLength - limitOne;
            }

            if(limitOne == 0 || limitTwo == 0) ropeSpeed = 0;

            // update the joint limits
            ropeJointOne.maxDistance = limitOne;
            ropeJointTwo.maxDistance = limitTwo;
            limitSum = limitOne + limitTwo;
        }

    }
}