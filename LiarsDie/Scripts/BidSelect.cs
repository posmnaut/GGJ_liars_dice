using Godot;
using System;

public partial class BidSelect : Node3D
{
    [Signal]
    public delegate void FaceIncreaseInputHandlerEventHandler();
    [Signal]
    public delegate void FaceDecreaseInputHandlerEventHandler();
    [Signal]
    public delegate void FreqIncreaseInputHandlerEventHandler();
    [Signal]
    public delegate void FreqDecreaseInputHandlerEventHandler();
    [Signal]
    public delegate void SubmitInputHandlerEventHandler();
    [Signal]
    public delegate void BluffInputHandlerEventHandler();

    Random randNumGen = new Random();
    AudioStreamPlayer3D SC1Audio;
    AudioStreamPlayer3D SC2Audio;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SC1Audio = GetTree().Root.GetChild(0).GetChild(4).GetChild<AudioStreamPlayer3D>(8);
        SC2Audio = GetTree().Root.GetChild(0).GetChild(4).GetChild<AudioStreamPlayer3D>(9);

    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    //IMPORTANT NOTE FOR FUTURE REFERENCE: This signal is fired when the `input_event()` ->
    //-> signal is fired from the `Area` Node child. This signal was throwing errors saying ->
    //-> that it did not exist, this is because I did not have matching parameters to the ->
    //-> original `input_event()` signal.
    public void _on_FaceI_input_event(Node camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx){
        // //IMPORTANT NOTE: I decided not to use `Scancodes` for the detecting of key ->
        // //-> presses, like below, because it is much less readable and in a Game Jam ->
        // //-> environment it needs to be readable.
        // GD.Print(@event);
        // if(@event.GetClass() == "InputEventMouseButton"){
        //     InputEventMouseButton mouseButtonEvent = (InputEventMouseButton) @event;

        //     // //NOTE: `68` is the keycode constant for `D`.
        //     // if(keyEvent.Scancode == 68 && keyEvent.Echo == false && keyEvent.Pressed == true){//NOTE: `68` is the keycode constant for `D`.
        //     //     GD.Print("ME! D!");
        //     // }
        //     EmitSignal("FaceIncreaseInputHandler");
        // }

        if(@event.IsActionPressed("mouse_click")){
            // InputEventMouseButton mouseButtonEvent = (InputEventMouseButton) @event;
            EmitSignal("FaceIncreaseInputHandler");
        }
    }

    public void _on_FaceD_input_event(Node camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx){
        if(@event.IsActionPressed("mouse_click")){
            // InputEventMouseButton mouseButtonEvent = (InputEventMouseButton) @event;
            EmitSignal("FaceDecreaseInputHandler");
        }
    }

    public void _on_FreqI_input_event(Node camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx){
        if(@event.IsActionPressed("mouse_click")){
            // InputEventMouseButton mouseButtonEvent = (InputEventMouseButton) @event;
            EmitSignal("FreqIncreaseInputHandler");
        }
    }

    public void _on_FreqD_input_event(Node camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx){
        if(@event.IsActionPressed("mouse_click")){
            // InputEventMouseButton mouseButtonEvent = (InputEventMouseButton) @event;
            EmitSignal("FreqDecreaseInputHandler");
        }
    }

    public void _on_SubmitArea_input_event(Node camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx){
        if(@event.IsActionPressed("mouse_click")){
            if(randNumGen.Next(1,3) % 2 == 0){
                SC1Audio.Play();
            }
            else{
                SC2Audio.Play();
            }
            EmitSignal("SubmitInputHandler");
        }
    }

    public void _on_BluffArea_input_event(Node camera, InputEvent @event, Vector3 click_position, Vector3 click_normal, int shape_idx){
        if(@event.IsActionPressed("mouse_click")){
            if(randNumGen.Next(1,3) % 2 == 0){
                SC1Audio.Play();
            }
            else{
                SC2Audio.Play();
            }
            EmitSignal("BluffInputHandler");
        }
    }
}
