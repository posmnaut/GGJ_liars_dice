using Godot;
using System;
using System.IO.Compression;
using System.Runtime.CompilerServices;
// using System.Threading;

public partial class PlayerCamera : Camera3D
{
    [Signal]
    public delegate void CupLiftEventHandler();
    [Signal]
    public delegate void CupDownEventHandler();
    [Signal]
    public delegate void CupShakenEventHandler();
    [Signal]
    public delegate void NoEyeContactEventHandler();
    [Signal]
    public delegate void SubmitInputHandlerEventHandler();
    [Signal]
    public delegate void BluffInputHandlerEventHandler(int previousHighestFreq, int previousHighestFace);

    Camera3D cameraNode;

    public bool gameStart = true;
    // public string playerName = "";

    // int turningRight = 1;
    // int lookingForward = 0;
    // int lookingLeft = -1;
    int turningDirect = 0;
    bool turnDownFlag = false;
    Transform3D defaultTransform;
    Node3D lookAtDown;
    Node3D lookAtRight;
    Node3D lookAtLeft;
    Timer warderDecideTimer;

    float interpolationDur = 0.0f;
    public bool isInterpolating = false;
    public bool isCupShaking = false;
    bool lockedInRound = false;
    bool bidRound = false;
    int previousHighestFreq = 0;
    int previousHighestFace = 0;
    public int playfreqBid = 1;
    public int playfaceBid = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Input.MouseMode = Input.MouseModeEnum.Captured;
        // cameraNode = GetTree().Root.GetChild(0).GetChild<Camera>(1);

        //The position for the `PlayerCamera` `Camera` instance to `lookAt()` when `W` is ->
        //-> pressed.
        defaultTransform = this.Transform;
        //The position for the `PlayerCamera` `Camera` instance to `lookAt()` when `S` is ->
        //-> pressed.
        lookAtDown = GetTree().Root.GetChild(0).GetChild(1).GetChild<Node3D>(2);
        //The position for the `PlayerCamera` `Camera` instance to `lookAt()` when `D` is ->
        //-> pressed.
        lookAtRight = GetTree().Root.GetChild(0).GetChild(1).GetChild<Node3D>(0);
        //The position for the `PlayerCamera` `Camera` instance to `lookAt()` when `A` is ->
        //-> pressed.
        lookAtLeft = GetTree().Root.GetChild(0).GetChild(1).GetChild<Node3D>(1);

        //The "Warden"s `decideTimer`, this will be used to let us know when the player ->
        //-> can make actions again.
        warderDecideTimer = GetTree().Root.GetChild(0).GetChild(2).GetChild<Timer>(3);
    }

 // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(double delta)
 {
    // GD.Print(this.Translation);
    // GD.Print("FACE: " + playfaceBid);
    // GD.Print("FREQ: " + playfreqBid);
    if(gameStart == true){
        
    }
    else{
        // if(cameraNode.GlobalTransform.basis.z != checkZ){
        //     this.RotateX(0.0001f);
        //     GD.Print(cameraNode.GlobalTransform.basis.z);
        // }
        // GD.Print(cameraNode.GlobalTransform.basis);
        if(Input.IsActionJustPressed("start_round")){
            turningDirect = 0;
            // turnDownFlag = false;
            // isInterpolating = false;
            lockedInRound = true; 
        }

        if(lockedInRound == false){
            if(isInterpolating == false){
                // if(Input.IsActionJustPressed("shake_cup")){
                //     if(turnDownFlag == false && turningDirect == 0){
                //         EmitSignal("CupShakenEventHandler");
                //         isInterpolating = true;
                //     }
                //     else{
                //         GD.Print("I have to make eye-contact to shake my cup");
                //         EmitSignal("NoEyeContactEventHandler");
                //     }
                // }
                if(Input.IsActionJustPressed("move_backward")){
                    if(turnDownFlag == false){
                        turnDownFlag = true;
                        interpolationDur = 0.0f;
                        isInterpolating = true;
                    }
                }
                else if(Input.IsActionJustPressed("move_right") && turningDirect != 1 || Input.IsActionJustPressed("move_right") && turnDownFlag == true){
                    if(turnDownFlag == false){
                        turningDirect += 1;
                    }
                    else{
                        EmitSignal("CupDownEventHandler");
                        turningDirect = 1;
                    }

                    interpolationDur = 0.0f;
                    isInterpolating = true;
                    turnDownFlag = false;
                }
                else if(Input.IsActionJustPressed("move_left") && turningDirect != -1 || Input.IsActionJustPressed("move_left") && turnDownFlag == true){
                    if(turnDownFlag == false){
                        turningDirect -= 1;
                    }
                    else{
                        EmitSignal("CupDownEventHandler");
                        turningDirect = -1;
                    }
                    
                    interpolationDur = 0.0f;
                    isInterpolating = true;
                    turnDownFlag = false;
                }
                else if(Input.IsActionJustPressed("move_forward")){
                    if(turnDownFlag == true){
                        EmitSignal("CupDownEventHandler");
                    }

                    interpolationDur = 0.0f;
                    turnDownFlag = false;
                    turningDirect = 0;
                }
            }
            // GD.Print(turningDirect);
            // GD.Print(turnDownFlag);
            if(isCupShaking == false){
                //Look-Backward:
                if(turnDownFlag == true){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    this.LookAt(lookAtDown.Position, Vector3.Up);
                    Transform3D endLocation = this.Transform;

                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);
                    //This `if-statement` prevents the `Signal` from being emitted too early and ->
                    //-> And prevents it from firing more than once.
                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        EmitSignal("CupLiftEventHandler");
                        isInterpolating = false;
                    }
                }
                //Look-Forward:
                else if(turningDirect == 0){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    Transform3D endLocation = defaultTransform;
                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);

                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        isInterpolating = false;
                    }
                }
                //Look-Right:
                else if(turningDirect == 1){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    this.LookAt(lookAtRight.Position, Vector3.Up);
                    Transform3D endLocation = this.Transform;

                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);
                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        isInterpolating = false;
                    }
                }
                //Look-Left:
                else if(turningDirect == -1){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    this.LookAt(lookAtLeft.Position, Vector3.Up);
                    Transform3D endLocation = this.Transform;

                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);
                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        isInterpolating = false;
                    }
                }
            
            // if(this.Transform == lookAtRight.Transform){
            //     turningRight = false;
            // }
            }
        }
        else{
            if(isInterpolating == false){
                if(Input.IsActionJustPressed("shake_cup") && bidRound == false){
                    if(turnDownFlag == false && turningDirect == 0){
                        EmitSignal("CupShakenEventHandler");
                        isInterpolating = true;
                        bidRound = true;
                    }
                    else{
                        GD.Print("I have to make eye-contact to shake my cup");
                        EmitSignal("NoEyeContactEventHandler");
                    }
                }
                else if(Input.IsActionJustPressed("move_backward")){
                    if(turnDownFlag == false){
                        turnDownFlag = true;
                        interpolationDur = 0.0f;
                        isInterpolating = true;
                    }
                }
                else if(Input.IsActionJustPressed("move_right") && turningDirect != 1 || Input.IsActionJustPressed("move_right") && turnDownFlag == true){
                    if(turnDownFlag == false){
                        turningDirect += 1;
                    }
                    else{
                        EmitSignal("CupDownEventHandler");
                        turningDirect = 1;
                    }

                    interpolationDur = 0.0f;
                    isInterpolating = true;
                    turnDownFlag = false;
                }
                else if(Input.IsActionJustPressed("move_left") && turningDirect != -1 || Input.IsActionJustPressed("move_left") && turnDownFlag == true){
                    if(turnDownFlag == false){
                        turningDirect -= 1;
                    }
                    else{
                        EmitSignal("CupDownEventHandler");
                        turningDirect = -1;
                    }
                    
                    interpolationDur = 0.0f;
                    isInterpolating = true;
                    turnDownFlag = false;
                }
                else if(Input.IsActionJustPressed("move_forward")){
                    if(turnDownFlag == true){
                        EmitSignal("CupDownEventHandler");
                    }

                    interpolationDur = 0.0f;
                    turnDownFlag = false;
                    turningDirect = 0;
                }
            }
            if(isCupShaking == false){
                //Look-Backward:
                if(turnDownFlag == true){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    this.LookAt(lookAtDown.Position, Vector3.Up);
                    Transform3D endLocation = this.Transform;

                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);
                    //This `if-statement` prevents the `Signal` from being emitted too early and ->
                    //-> And prevents it from firing more than once.
                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        EmitSignal("CupLiftEventHandler");
                        isInterpolating = false;
                    }
                }
                //Look-Forward:
                else if(turningDirect == 0){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    Transform3D endLocation = defaultTransform;
                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);

                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        isInterpolating = false;
                    }
                }
                //Look-Right:
                else if(turningDirect == 1){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    this.LookAt(lookAtRight.Position, Vector3.Up);
                    Transform3D endLocation = this.Transform;

                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);
                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        isInterpolating = false;
                    }
                }
                //Look-Left:
                else if(turningDirect == -1){
                    interpolationDur += 0.2f + (float) delta;
                    Transform3D startLocation = this.Transform;
                    this.LookAt(lookAtLeft.Position, Vector3.Up);
                    Transform3D endLocation = this.Transform;

                    this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + (float) delta);
                    if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                        isInterpolating = false;
                    }
                }
            }
        }
    }
}

    public override void _Input(InputEvent @event)
    {   
        // if(notWASD == false){
            //IMPORTANT NOTE: I decided not to use `Scancodes` for the detecting of key ->
            //-> presses, like below, because it is much less readable and in a Game Jam ->
            //-> environment it needs to be readable.
            // if(@event.GetClass() == "InputEventKey"){
            //     InputEventKey keyEvent = (InputEventKey) @event;

            //     //NOTE: `68` is the keycode constant for `D`.
            //     if(keyEvent.Scancode == 68 && keyEvent.Echo == false && keyEvent.Pressed == true){//NOTE: `68` is the keycode constant for `D`.
            //         GD.Print("ME! D!");
            //     }
            // }
        // }
    }

    // public void _on_StartGame_NameEnteredEventHandler(){
    //     // gameStart = false;
    // }

    public void _on_Warden_WBidMadeEventHandler(int highestFreq, int highestFace){
        previousHighestFreq = highestFreq;
        playfreqBid = highestFreq;

        previousHighestFace = highestFace;
        // bidRound = false;
    }

    //FACE INCREASE:
    public void _on_BidSelect_FaceIncreaseInputHandler(){
        if(playfaceBid < 6 && bidRound == true){
            playfaceBid += 1;
        }
    }

    //FACE DECREASE:
    public void _on_BidSelect_FaceDecreaseInputHandler(){
        if(playfaceBid > 1 && bidRound == true){
            playfaceBid -= 1;
        }
    }

    //FREQ. INCREASE:
    public void _on_BidSelect_FreqIncreaseInputHandler(){
        if(bidRound == true){
            playfreqBid += 1;
        }
    }

    //FREQ. DECREASE:
    public void _on_BidSelect_FreqDecreaseInputHandler(){
        if(playfreqBid > previousHighestFreq && bidRound == true){//LESS THAN PREV FREQ.
            playfreqBid -= 1;
        }
    }

    //SUBMIT BID:
    public void _on_BidSelect_SubmitInputHandler(){
        if(playfaceBid == 0 && playfreqBid == 0){
            GD.Print("CANNOT SUBMIT THIS SCORE: [ERROR] REASON: [FREQUENCY AND FACE OF BID ARE STILL INITIAL VALUES OF [0] and [0]]");
        }
        else if(playfaceBid == previousHighestFace && playfreqBid == previousHighestFreq){
            GD.Print("CANNOT SUBMIT THIS SCORE: [ERROR] REASON: [BID IS IDENTICAL TO PREVIOUS BID]");
        }
        else if(playfaceBid <= previousHighestFace && playfreqBid <= previousHighestFreq){
            GD.Print("CANNOT SUBMIT THIS SCORE: [ERROR] REASON: [FREQUENCY OF BID IS TOO LOW FOR CURRENT FACE]");
        }
        //MAY BE REDUNDANT:
        else if(playfaceBid > previousHighestFace && playfreqBid < previousHighestFreq){
            GD.Print("CANNOT SUBMIT THIS SCORE: [ERROR] REASON: [FREQUENCY OF BID IS TOO LOW FOR CURRENT FACE]");
        }
        else if(warderDecideTimer.TimeLeft != 0){
            GD.Print("CANNOT SUBMIT THIS SCORE: [ERROR] REASON: [WARDEN HAS NOT MADE A BID]");
        }
        else if(bidRound == true){
            GD.Print("Submitted");
            EmitSignal("SubmitInputHandler");
        }
    }

    //BLUFF BID:
    public void _on_BidSelect_BluffInputHandler(){
        if(bidRound == true){
            GD.Print("BLUFF");
            EmitSignal("BluffInputHandler", previousHighestFreq, previousHighestFace);
        }
    }

    //PLAYER WON BLUFF:
    public void _on_Dice_PlayerWonBluffEventHandler(){
        //ADD: Player gains turn priority next turn.
        //BELOW FOUR LINES: These are a must for allowing the player's bid frequency and ->
        //-> face to return to `0`.
        previousHighestFace = 0;
        previousHighestFreq = 0;
        playfreqBid = 1;
        playfaceBid = 1;

        bidRound = false;
    }

    //WARDEN WON BLUFF:
    public void _on_Dice_WardenWonBluffEventHandler(){
        //ADD: Revoke player priority next turn.
        bidRound = false;
    }
}
