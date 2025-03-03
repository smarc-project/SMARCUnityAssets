using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Force;
using UnityEngine;

namespace Importer
{
    [Serializable]
    public class ColliderModel
    {
        public String transformPath;
        public Vector3 scale;
        public bool isTrigger;
        public bool providesContacts;

        public ColliderType colliderType;

        public Vector3 center;

        // Box
        public Vector3 size;

        // Sphere
        public float radius;

        // Capsule
        public float height;

        public int direction;

        // Mesh
        public bool isConvex;
        public string mesh;
        public string material;

        public enum ColliderType
        {
            Mesh,
            Box,
            Sphere,
            Capsule
        }

        public static ColliderModel WriteModel(Collider collider, GameObject toStore = null)
        {
            var model = new ColliderModel();
            model.transformPath = collider.gameObject.transform.GetPath(toStore?.transform);
            model.scale = collider.transform.localScale;

            model.isTrigger = collider.isTrigger;
            model.providesContacts = collider.providesContacts;
            if (collider is BoxCollider)
            {
                model.colliderType = ColliderType.Box;
                model.center = ((BoxCollider)collider).center;
                model.size = ((BoxCollider)collider).size;
            }

            if (collider is SphereCollider)
            {
                model.colliderType = ColliderType.Sphere;
                model.center = ((SphereCollider)collider).center;
                model.radius = ((SphereCollider)collider).radius;
            }

            if (collider is CapsuleCollider)
            {
                model.colliderType = ColliderType.Capsule;
                model.center = ((CapsuleCollider)collider).center;
                model.radius = ((CapsuleCollider)collider).radius;
                model.height = ((CapsuleCollider)collider).height;
                model.direction = ((CapsuleCollider)collider).direction;
            }

            if (collider is MeshCollider)
            {
                model.colliderType = ColliderType.Mesh;
                model.isConvex = ((MeshCollider)collider).convex;
                model.material = ((MeshCollider)collider).material.name;
                model.mesh = ((MeshCollider)collider).sharedMesh.name;
            }

            return model;
        }

        public void LoadOntoObject(GameObject loadedRobot)
        {
            var find = loadedRobot.transform.Find(transformPath);
            if (find == null) find = loadedRobot.transform.CreatePath(transformPath);
            
            if (colliderType == ColliderType.Sphere)
            {
                var collider = find.gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
                collider.center = center;
                collider.radius = radius;
            }

            if (colliderType == ColliderType.Box)
            {
                var collider = find.gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
                collider.center = center;
                collider.size = size;
            }

            if (colliderType == ColliderType.Capsule)
            {
                var collider = find.gameObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
                collider.center = center;
                collider.radius = radius;
                collider.height = height;
                collider.direction = direction;
            }
        }
    }

    [Serializable]
    public class VehicleModel
    {
        public string urdfFilePath;

        public List<ForcePointModel> forcePoints;
        public List<ColliderModel> colliders;
        public List<ArticulationModel> articulationModels;


        public static VehicleModel WriteModel(String urdfPath, GameObject toStore)
        {
            var vehicleModel = new VehicleModel();
            vehicleModel.urdfFilePath = urdfPath;

            vehicleModel.forcePoints = toStore.transform.FindAllChildrenOfType<ForcePoint>().Select(point => ForcePointModel.WriteModel(point, toStore)).ToList();
            vehicleModel.articulationModels = toStore.transform.FindAllChildrenOfType<ArticulationBody>().Select(body => ArticulationModel.WriteModel(body, toStore)).ToList();
            vehicleModel.colliders = toStore.transform.FindAllChildrenOfType<Collider>().Select(collider => ColliderModel.WriteModel(collider, toStore)).ToList();

            return vehicleModel;
        }
    }

    [Serializable]
    public class ForcePointModel
    {
        public String transformPath;
        public Quaternion localRotation;
        public Vector3 localPosition;
        public Vector3 localScale;
        public string volumeObjectTransformPath;

        public static ForcePointModel WriteModel(ForcePoint forcePoint, GameObject toStore = null)
        {
            var model = new ForcePointModel();
            model.transformPath = forcePoint.gameObject.transform.GetPath(toStore?.transform);
            model.localPosition = forcePoint.transform.localPosition;
            model.localRotation = forcePoint.transform.localRotation;
            model.localScale = forcePoint.transform.localScale;
            model.volumeObjectTransformPath = forcePoint.VolumeObject.transform.GetPath(toStore?.transform);
            return model;
        }

        public void LoadOntoObject(GameObject loadedRobot)
        {
            var transform = loadedRobot.transform.Find(transformPath);
            if (transform == null)
            {
                transform = loadedRobot.transform.CreatePath(transformPath);
                
                transform.localPosition = localPosition;
                transform.localRotation = localRotation;
                transform.localScale = localScale;
            }

            var point = transform.gameObject.AddComponent<ForcePoint>();
            point.VolumeObject = loadedRobot.transform.Find(volumeObjectTransformPath).gameObject;
            point.ConnectedArticulationBody = loadedRobot.transform.Find("base_link").GetComponent<ArticulationBody>();
            point.ConnectedRigidbody = loadedRobot.transform.Find("base_link").GetComponent<Rigidbody>();
        }
    }


    [Serializable]
    public class ArticulationModel
    {
        public String transformPath;
        public List<DriveModel> drives = new();
        public float linearDamping;
        public float angularDamping;
        public Vector3 inertiaTensor;
        public Quaternion inertiaTensorRotation;
        public Vector3 centerOfMass;
        public bool automaticInertiaTensor;
        public bool automaticCenterOfMass;
        public float joinFriction;

        public static ArticulationModel WriteModel(ArticulationBody body, GameObject toStore = null)
        {
            var model = new ArticulationModel();
            model.transformPath = body.gameObject.transform.GetPath(toStore?.transform);
            model.linearDamping = body.linearDamping;
            model.angularDamping = body.angularDamping;
            model.inertiaTensor = body.inertiaTensor;
            model.inertiaTensorRotation = body.inertiaTensorRotation;
            model.automaticInertiaTensor = body.automaticInertiaTensor;
            model.joinFriction = body.jointFriction;
            model.automaticCenterOfMass = body.automaticCenterOfMass;
            model.centerOfMass = body.centerOfMass;
            model.drives.Add(DriveModel.WriteModel(body.xDrive, ArticulationDriveAxis.X));
            model.drives.Add(DriveModel.WriteModel(body.yDrive, ArticulationDriveAxis.Y));
            model.drives.Add(DriveModel.WriteModel(body.zDrive, ArticulationDriveAxis.Z));
            return model;
        }

        public void LoadOntoObject(GameObject loadedObject)
        {
            var find = loadedObject.transform.Find(transformPath);
            var articulationBody = find.GetComponent<ArticulationBody>();
            if (articulationBody != null)
            {
                drives.ForEach(drive =>
                {
                    articulationBody.SetDriveForceLimit(drive.axis, drive.forceLimit);
                    articulationBody.SetDriveLimits(drive.axis, drive.lowerLimit, drive.upperLimit);
                    articulationBody.SetDriveDamping(drive.axis, drive.damping);
                    articulationBody.SetDriveStiffness(drive.axis, drive.stiffness);
                });
               
                articulationBody.inertiaTensorRotation = inertiaTensorRotation;
                if (articulationBody.automaticInertiaTensor && !automaticInertiaTensor)
                {
                    articulationBody.inertiaTensor = inertiaTensor;
                    articulationBody.automaticInertiaTensor = automaticInertiaTensor;
                }
                articulationBody.centerOfMass = centerOfMass;
                articulationBody.automaticCenterOfMass = automaticCenterOfMass;
                articulationBody.linearDamping = linearDamping;
                articulationBody.angularDamping = angularDamping;
                articulationBody.jointFriction = joinFriction;
            }
            else
            {
                Debug.LogWarning("ArticulationBody not found at: " + transformPath);
            }
        }
    }


    [Serializable]
    public class DriveModel
    {
        public float damping;
        public float stiffness;
        public float forceLimit;
        public float upperLimit;
        public float lowerLimit;
        public ArticulationDriveType driveType;
        public ArticulationDriveAxis axis;

        public static DriveModel WriteModel(ArticulationDrive drive, ArticulationDriveAxis axis)
        {
            var writeModel = new DriveModel();
            writeModel.damping = drive.damping;
            writeModel.stiffness = drive.stiffness;
            writeModel.forceLimit = drive.forceLimit;
            writeModel.upperLimit = drive.upperLimit;
            writeModel.lowerLimit = drive.lowerLimit;
            writeModel.driveType = drive.driveType;
            writeModel.axis = axis;
            return writeModel;
        }
    }
}