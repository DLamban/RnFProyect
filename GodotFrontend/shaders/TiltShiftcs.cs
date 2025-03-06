using Godot;
using System;
[GlobalClass]
[Tool]
public partial class TiltShiftcs : CompositorEffect
{
	RenderingDevice rd;
	Rid shader;
	Rid pipeline;
    // NO USED YET
    public TiltShiftcs() : base()
	{
		EffectCallbackType = EffectCallbackTypeEnum.PostOpaque;

		rd = RenderingServer.GetRenderingDevice();
		RenderingServer.CallOnRenderThread(Callable.From(() => InitializeCompute()));
	}

	public override void _Notification(int what)
	{
		if (what == GodotObject.NotificationPredelete)
		{
			if (shader.IsValid)
			{
				RenderingServer.FreeRid(shader);
			}
		}
	}

	private void InitializeCompute()
	{
		rd = RenderingServer.GetRenderingDevice();
		GD.Print("TiltShiftEffect cargado en el editor.");
	}

	public override void _RenderCallback(int effectCallbackType, RenderData renderData)
	{
		
	}
}
