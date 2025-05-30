using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using NUnit.Framework;
using UnityEngine;

public class ArticulationChainComponent : MonoBehaviour
{
    public List<ArticulationBody> bodyParts;
    public Dictionary<ArticulationBody, DriveController> DriveControllers;
    public ArticulationBody root;

    public void Awake()
    {
        bodyParts = new List<ArticulationBody>();
        bodyParts.Add(root);
        bodyParts.AddRange(FindArticulationBodies(root.transform));

        DriveControllers = bodyParts.Select(bp => (bp, new DriveController(bp)))
            .ToDictionary(tuple => tuple.bp, tuple => tuple.Item2);
    }

    public List<ArticulationBody> FindArticulationBodies(Transform item)
    {
        var findArticulationBodies = new List<ArticulationBody>();
        foreach (Transform child in item)
        {
            var articulationBody = child.GetComponent<ArticulationBody>();
            if (articulationBody != null)
            {
                findArticulationBodies.Add(articulationBody);
            }

            findArticulationBodies.AddRange(FindArticulationBodies(child));
        }

        return findArticulationBodies;
    }


    public void Restart(Vector3 position, Quaternion rotation)
    {
        root.TeleportRoot(position, rotation);
        
        foreach (var bodyPart in DriveControllers)
        {
            bodyPart.Key.ResetArticulationBody();
            bodyPart.Value.ResetDrives();
        }
    }

    public class DriveController
    {
        public DriveParameters XParameters;
        public DriveParameters YParameters;
        public DriveParameters ZParameters;

        public readonly ArticulationBody articulationBody;
        private readonly ArticulationDrive xIntial;
        private readonly ArticulationDrive yIntial;
        private readonly ArticulationDrive zIntial;

        public DriveController(ArticulationBody articulationBody)
        {
            this.articulationBody = articulationBody;
            XParameters = DriveParameters.CreateParameters(articulationBody.xDrive);
            YParameters = DriveParameters.CreateParameters(articulationBody.yDrive);
            ZParameters = DriveParameters.CreateParameters(articulationBody.zDrive);
            xIntial = articulationBody.xDrive;
            yIntial = articulationBody.yDrive;
            zIntial = articulationBody.zDrive;
            this.articulationBody.GetJointForces(new List<float>());
        }

        public void ResetDrives()
        {
            articulationBody.xDrive = xIntial;
            articulationBody.yDrive = yIntial;
            articulationBody.zDrive = zIntial;
        }
        
        public void SetDriveTargets(float x, float y, float z)
        {
            articulationBody.SetDriveTarget(ArticulationDriveAxis.X, ComputeNormalizedDriveTarget(XParameters, x));
            articulationBody.SetDriveTarget(ArticulationDriveAxis.Y, ComputeNormalizedDriveTarget(YParameters, y));
            articulationBody.SetDriveTarget(ArticulationDriveAxis.Z, ComputeNormalizedDriveTarget(ZParameters, z));
        }

        public void SetDriveStrength(float x)
        {
            SetDriveStrengths(x, x, x);
        }

        public void SetDriveStrengths(float x, float y, float z)
        {
            articulationBody.SetDriveForceLimit(ArticulationDriveAxis.X, ComputeNormalizedDriveStrength(XParameters, x));
            articulationBody.SetDriveForceLimit(ArticulationDriveAxis.Y, ComputeNormalizedDriveStrength(YParameters, y));
            articulationBody.SetDriveForceLimit(ArticulationDriveAxis.Z, ComputeNormalizedDriveStrength(ZParameters, z));
        }


        public float ComputeNormalizedDriveTarget(DriveParameters drive, float actionValue)
        {
            return drive.lowerLimit + (actionValue + 1) / 2 * (drive.upperLimit - drive.lowerLimit);
        }

        public float ComputeNormalizedDriveStrength(DriveParameters drive, float actionValue)
        {
            return (actionValue + 1f) * 0.5f * drive.forceLimit;
        }
    }

    public struct DriveParameters
    {
        public float upperLimit;
        public float lowerLimit;
        public float stiffness;
        public float damping;
        public float forceLimit;

        public static DriveParameters CreateParameters(ArticulationDrive drive)
        {
            return new DriveParameters
            {
                upperLimit = drive.upperLimit,
                lowerLimit = drive.lowerLimit,
                stiffness = drive.stiffness,
                damping = drive.damping,
                forceLimit = drive.forceLimit,
            };
        }
    }
}