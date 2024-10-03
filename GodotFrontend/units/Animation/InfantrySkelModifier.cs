using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.units.Animation
{
    [Tool]
    public partial class InfantrySkelModifier : SkeletonModifier3D
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        private Vector3 upperArmOffset;
        private Vector3 lowerArmOffset;
        private Vector3 handOffset;
        private Skeleton3D skel;
        private int upperArmidx;
        private int lowerArmidx;
        private int handidx;

        public InfantrySkelModifier()
        {
            rng.Randomize();
            // TODO: This random requires more constraint so the arms feel more natural, lot of tweeking, MAYBE some kind of IK
            upperArmOffset = RandomizeOffsetsPositive(3);
            lowerArmOffset = RandomizeOffsetsPositive(2.1f);
            handOffset = Randomizeoffsets(1.3f);
        }
        private Vector3 RandomizeOffsetsPositive(float MaxAngle)
        {
            Vector3 vec3 = Randomizeoffsets(MaxAngle);
            return new Vector3(Math.Abs(vec3.X),Math.Abs(vec3.Y),Math.Abs(vec3.Z));
        }
        private Vector3 Randomizeoffsets(float maxAngle)
        {
            return new Vector3(randomAngleNormalized(maxAngle), randomAngleNormalized(maxAngle), randomAngleNormalized(maxAngle));
        }
        private float randomAngleNormalized(float degreesRange)
        {
            float radiansRange = Mathf.DegToRad(degreesRange);
            return (rng.Randfn() * (2 * radiansRange)) - (radiansRange);
        }
        public override void _ProcessModification()
        {
            skel = GetSkeleton();
            if (skel == null) return;
            upperArmidx = skel.FindBone("UpperArm.R");
            lowerArmidx = skel.FindBone("LowerArm.R");
            handidx = skel.FindBone("Hand.R");

            localPose(skel, upperArmidx,upperArmOffset );
            localPose(skel, lowerArmidx, lowerArmOffset);
            localPose(skel, handidx, handOffset);
        }
        private void localPose(Skeleton3D skeleton, int boneidx, Vector3 rotation)
        {
            if (boneidx == -1) return;// TODO: look why we are getting -1, probably wrong skeleton loaded
            var t = skeleton.GetBonePose(boneidx);
            
            t = t.Rotated(new Vector3(1,0,0), rotation.X);
            t = t.Rotated(new Vector3(0, 1, 0), rotation.Y);
            t = t.Rotated(new Vector3(0, 0, 1), rotation.Z);

            skeleton.SetBonePose(boneidx, t);

        }
    }
}
