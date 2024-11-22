using Godot;
using System;
using System.Threading.Tasks;
// Kind of a toast message
// the behaviour will be to come from bottom and exit from top
// need accesory methods to know start and finish
public partial class ChargeResponseUI : Panel
{
    Label responseLabel;
    Vector2 startPos;
    int yScreenSize;
    Tween tween;
    public override void _Ready()
    {
        responseLabel = GetNode<Label>("ChargeResponseLabel");
        startPos = responseLabel.Position;  
        yScreenSize = (int)GetViewportRect().Size.Y;
      
      
        

    }
    public async Task ShowToast(string message)
    {
        tween = CreateTween();
        Vector2 outsideDown = new Vector2(startPos.X, yScreenSize);
        responseLabel.Position = outsideDown;
        float time = 1f;
        float tweenduration = time;
        Vector2 targetPos = new Vector2(startPos.X, startPos.Y);
        tween.TweenProperty(responseLabel, "position", targetPos, 1).SetTrans(Tween.TransitionType.Quad);
        Vector2 targetPosOut = new Vector2(startPos.X, -160);
        tween.TweenInterval(2.0f);

        tween.TweenProperty(responseLabel, "position", targetPosOut, 1).SetTrans(Tween.TransitionType.Quad);
        
        await ToSignal(tween, "finished");
    }
}
