using Core.GameLoop;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Units.BaseUnit;

namespace GodotFrontend.UIcode
{
    public class DiceThrower
    {
        private static readonly DiceThrower instance = new DiceThrower();
        public static DiceThrower Instance
        {
            get
            {
                return instance;
            }
        }
        private List<RigidBody3D> dicePool = new List<RigidBody3D>();
        PackedScene dicePackedScene = GD.Load<PackedScene>("res://dices_resources/dice_object.tscn");
        Node3D diceTray;
        CenterContainer dicePanel;
        RandomNumberGenerator rnd = new RandomNumberGenerator();
        private TaskCompletionSource<bool> diceThrowFinished;//= new TaskCompletionSource<bool>();
        private int diceThrowedNotFinished = 0;
        private List<int> diceResult;
        
        public DiceThrowerTaskDelegate diceThrowerTaskDel;
        public DiceThrower() {

            diceThrowerTaskDel = ThrowDices;
            // we will use the dicetray to change scale depending on number of dices
            // and move the zoom of the camera 

        }
        public void initDiceThrower(CenterContainer _dicePanel)
        {
            dicePanel = _dicePanel;
            diceTray = dicePanel.GetNode<Node3D>("Panel/diceView/DiceViewport/DiceTray");
            
        }
        private RigidBody3D getDice()
        {
            RigidBody3D? dice = dicePool.Find(x => !x.Visible);
            if (dice == null)
            {
                dice = (RigidBody3D)dicePackedScene.Instantiate();
                diceTray.AddChild(dice);
                dicePool.Add(dice);
                dice.RotationDegrees = new Vector3(rnd.RandfRange(0, Mathf.Tau), rnd.RandfRange(0, Mathf.Tau), rnd.RandfRange(0, Mathf.Tau));
            }
            else
            {
                dice.Visible = true;
                //dice.SetPhysicsProcess(true);
                dice.Position = new Vector3(0, 0, 0);
            }
            return dice;
        }
        
        // We gonna use the physics and random number, I guess should be random enough, TODO; check randommness
        // TODO: pooling the dices so we avoid the instantion
        public async Task<List<int>> ThrowDices(int numberdices, int diceType = 6)
        {
            dicePanel.Visible = true;
            // remove and return to pool used dices
            foreach(var diceIns in dicePool)
            {
                diceIns.QueueFree();     
            }
            dicePool.Clear();

            diceThrowedNotFinished = numberdices;
            diceThrowFinished = new TaskCompletionSource<bool>();
            diceResult = new List<int>();

            if (diceType == 6)
            {
                for (int i = 0; i < numberdices; i++)
                {
                    RigidBody3D dice = getDice();
                    float xRand = 0.5f - rnd.Randf();
                    float yRand = rnd.Randf() + 0.5f;
                    dice.Position = dice.Position + new Vector3(0, 4f + i / 10f, i);


                    Vector3 impulse = new Vector3(yRand * 15f, 0, -15f);
                    //Vector3 impulseOrigin = new Vector3(0, yRand, xRand);
                    dice.ApplyImpulse(impulse);
                    dice.ApplyTorqueImpulse(new Vector3(yRand * 500, yRand * 500, -yRand * 500));
                    dice.SleepingStateChanged += () => { OnDiceSleep(dice); };

                }
            }
            else
            {
                throw new NotImplementedException();
            }
            await diceThrowFinished.Task;
            dicePanel.Visible = false;
            return diceResult;
        }
        public async Task<int> ThrowDicesSum(int numberDices, int diceType = 6)
        {
            List<int> resultDices = await ThrowDices(numberDices, diceType);
            int resultnum = 0;
            foreach (int i in resultDices)
            {
                resultnum += i;
            }
            return resultnum;
        }
        // get the maximum value of various dices, used in charges
        public async Task<int> ThrowDicesMaximum(int numberDices, int diceType = 6)
        {
            List<int> resultDices = await ThrowDices(numberDices, diceType);
            return resultDices.Max(x => x);
        }
        public async Task<int> ThrowDicesCharge()
        {
            return await ThrowDicesMaximum(2);
        }

        private void OnDiceSleep(RigidBody3D dice)
        {
            var basisTransformDice = dice.GlobalTransform.Basis;

            int number = -1;
            float threshold = 0.9f;
            Vector3 upVectorLocal = Vector3.Up * basisTransformDice;
            float dotProduct = upVectorLocal.Dot(Vector3.Up);
            // check for up and down faces
            if (Math.Abs(dotProduct) > threshold)
            {
                if (dotProduct > 0)
                {
                    number = 1;
                }
                else
                {
                    number = 6;
                }
            }
            else
            {
                //ignore Y rotation
                //number2 
                Vector3 number2 = new Vector3(-1, 0, 0);
                Vector3 number3 = new Vector3(0, 0, 1);
                Vector3 number4 = new Vector3(0, 0, -1);
                Vector3 number5 = new Vector3(1, 0, 0);
                List<Vector3> faces = new List<Vector3>
            {
                 number2, number3, number4, number5
            };


                float bestDot = -1;
                int indexBest = -1;
                for (int i = 0; i < faces.Count; i++)
                {
                    float dotproduct = faces[i].Dot(upVectorLocal);
                    if (dotproduct > bestDot)
                    {
                        bestDot = dotproduct;
                        indexBest = i;
                    }
                }
                number = indexBest + 2;
            }
            diceResult.Add(number);
            diceThrowedNotFinished--;
            if (diceThrowedNotFinished == 0)
            {
                diceThrowFinished.SetResult(true);
            }
        }
    }
}
