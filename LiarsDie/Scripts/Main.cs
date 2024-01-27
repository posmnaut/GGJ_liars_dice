using Godot;
using System;

//IDEA AREA:
//1.) What if it is liars dice, but you want to lose!
//2.) TOMORROW: Set up bluffing system for the "Warden".
//3.) Turn swap for winner going next first.
//4.) What happens when you run out of dice.
//FUTURE: Make a return statement that is a boolean and will stop this signal ->
//-> from firing if the "Warden" calls a bluff.


public class Main : Node
{
    Transform oldPlayerTrans;

    Spatial environment;
    PlayerCamera playerCam;
    Spatial gameStartAngle;
    Warden warden;
    Dice diceSpatial;
    Spatial bidSelect;
    Spatial startGame;
    Timer gameStartTimer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        environment = GetTree().Root.GetChild(0).GetChild<Spatial>(0);
        warden = GetTree().Root.GetChild(0).GetChild<Warden>(2);
        diceSpatial = GetTree().Root.GetChild(0).GetChild<Dice>(3);

        bidSelect = GetTree().Root.GetChild(0).GetChild<Spatial>(4);
        startGame = GetTree().Root.GetChild(0).GetChild<Spatial>(5);
        gameStartTimer = GetTree().Root.GetChild(0).GetChild<Timer>(6);

        // environment.Visible = false;
        // warden.Visible = false;
        // diceSpatial.Visible = false;
        // bidSelect.Visible = false;

        playerCam = GetTree().Root.GetChild(0).GetChild<PlayerCamera>(1);
        oldPlayerTrans = playerCam.Transform;
        gameStartAngle = GetTree().Root.GetChild(0).GetChild<Spatial>(7);
        playerCam.Transform = gameStartAngle.Transform;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if(playerCam.gameStart != true){
            Transform startLocation = playerCam.Transform;
            Transform endLocation = oldPlayerTrans;
            playerCam.Transform = startLocation.InterpolateWith(endLocation, 0.09f + delta);
        }
    }

    public void _on_StartGame_NameEnteredEventHandler(){
        gameStartTimer.Start();
    }

    public void _on_GameStartTimer_timeout(){
        playerCam.gameStart = false;
        environment.Visible = true;
        warden.Visible = true;
        diceSpatial.Visible = true;
        bidSelect.Visible = true;
    }
}
