using Godot;
using System;

public class PlayerCup : StaticBody
{
    Spatial pivotLeft;
    Spatial pivotRight;
    Timer shakeTimer;
    AudioStreamPlayer3D cupLiftAudio1;
    AudioStreamPlayer3D cupLiftAudio2;
    Random randNumGen = new Random();
    PlayerCamera playerCam;
    Dice diceSpatial;
    Vector3 cupOrigTrans;
    bool freedInterp = false;
    bool returnCup = false;
    bool onLeft = false;
    bool onRight = false;
    float interpolationDur = 0.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pivotLeft = GetTree().Root.GetChild(0).GetChild(0).GetChild(3).GetChild<Spatial>(2);
        pivotRight = GetTree().Root.GetChild(0).GetChild(0).GetChild(3).GetChild<Spatial>(3);
        shakeTimer = GetTree().Root.GetChild(0).GetChild(0).GetChild(3).GetChild<Timer>(4);
        cupLiftAudio1 = GetTree().Root.GetChild(0).GetChild(0).GetChild(3).GetChild<AudioStreamPlayer3D>(5);
        cupLiftAudio2 = GetTree().Root.GetChild(0).GetChild(0).GetChild(3).GetChild<AudioStreamPlayer3D>(6);

        diceSpatial = GetTree().Root.GetChild(0).GetChild<Dice>(3);
        playerCam = GetTree().Root.GetChild(0).GetChild<PlayerCamera>(1);
    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(playerCam.isCupShaking == true){
            if(returnCup == true && freedInterp == false){
                interpolationDur += 0.2f + delta;
                Quat qStartRot = new Quat(this.Rotation).Normalized();

                this.Rotation = qStartRot.Slerp(new Quat(cupOrigTrans).Normalized(), 0.2f + delta).GetEuler();

                //More time was needed for interpolation here, so the `interpolationDur` ->
                //-> cut-off was slightly extended. This prevented the cup from not ->
                //-> returning to its starting position after "shaking".
                if(interpolationDur >= 4.0f && interpolationDur <= 5.0f){
                    GD.Print("RETURING " + interpolationDur);
                    playerCam.isCupShaking = false;
                    playerCam.isInterpolating = false;
                    returnCup = false;
                    interpolationDur = 0.0f;
                }
            }
            //Currently on the `PivotRight` so move cup to `PivotLeft`.
            else if(onRight == true){
                // GD.Print("Shaking LEFT");
                interpolationDur += 0.2f + delta;
                Quat qStartRot = new Quat(this.Rotation).Normalized();
                this.LookAt(pivotLeft.Translation, Vector3.Up);
                Quat qEndRot = new Quat(this.Rotation).Normalized();
                
                this.Rotation = qStartRot.Slerp(qEndRot, 0.2f + delta).GetEuler();

                if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                    onRight = false;
                    onLeft = true;
                    interpolationDur = 0.0f;

                    //This `if-statement` allows the current "shake" to finish interpolating ->
                    //-> before starting the `returnCup` interpolation (this prevents the ->
                    //-> interpolations "overlapping").
                    if(returnCup == true){
                        freedInterp = false;
                    }
                }
            }
            //Currently on the `PivotLeft` so move cup to `PivotRight`.
            else if(onLeft == true){
                // GD.Print("Shaking RIGHT");
                interpolationDur += 0.2f + delta;
                Quat qStartRot = new Quat(this.Rotation).Normalized();
                this.LookAt(pivotRight.Translation, Vector3.Up);
                Quat qEndRot = new Quat(this.Rotation).Normalized();
                
                this.Rotation = qStartRot.Slerp(qEndRot, 0.2f + delta).GetEuler();

                if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
                    onLeft = false;
                    onRight = true;
                    interpolationDur = 0.0f;
                    //This `if-statement` allows the current "shake" to finish interpolating ->
                    //-> before starting the `returnCup` interpolation (this prevents the ->
                    //-> interpolations "overlapping").
                    if(returnCup == true){
                        freedInterp = false;
                    }
                }
            }
            //Currently in the middle so begin shaking the cup by moving to `PivotLeft`.
            else{
                onRight = true;
            }
            this.Transform.Orthonormalized();
        }
        // else if(returnCup == true && playerCam.isInterpolating == false){
        //     interpolationDur += 0.2f + delta;
        //     Transform startLocation = this.Transform;
        //     this.LookAt(cupOrigTrans, Vector3.Up);
        //     Transform endLocation = this.Transform;

        //     this.Transform = startLocation.InterpolateWith(endLocation, 0.2f + delta);

        //     if(interpolationDur >= 1.0f && interpolationDur <= 1.084){
        //         playerCam.isInterpolating = false;
        //     }
        // }
    }

    public void _on_PlayerCamera_CupLiftEventHandler(){
        if(diceSpatial.shakenOnce == true){
            int numAud = randNumGen.Next(1,3);
            if(numAud % 2 == 0){
                cupLiftAudio1.Play(0.2f);
            }
            else{
                cupLiftAudio2.Play();
            }
        }
        this.RotateX(-1.0f);
    }

    public void _on_PlayerCamera_CupDownEventHandler(){
        // if(playerCam.turnDownFlag == false){
        // GD.Print("Lowered");

        this.RotateX(1.0f);
        // }
    }

    public void _on_PlayerCamera_CupShakenEventHandler(){
        cupOrigTrans = this.Rotation;
        shakeTimer.Start();
        freedInterp = true;
        playerCam.isCupShaking = true;
    }

    public void _on_ShakeTimer_timeout(){
        returnCup = true;
    }

    public void _on_AudioStreamPlayer3D_finished(){
        cupLiftAudio1.Stop();
    }
}
