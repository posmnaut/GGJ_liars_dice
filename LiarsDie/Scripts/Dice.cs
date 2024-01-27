using Godot;
using System;

public partial class Dice : Node3D
{
    [Signal]
    public delegate void PlayerWonBluffEventHandler();
    [Signal]
    public delegate void WardenWonBluffEventHandler();

    private Random ranNumGen = new Random();
    public bool shakenOnce = false;
    int[] playerDiceArray = new int[5];
    Label3D dieLab1;
    Label3D dieLab2;
    Label3D dieLab3;
    Label3D dieLab4;
    Label3D dieLab5;
    AudioStreamPlayer3D cupShakeAudio;
    Label3D freqLabel;
    Label3D faceLabel;

    Warden warden;
    PlayerCamera playerCam;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        dieLab1 = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(0);
        dieLab2 = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(1);
        dieLab3 = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(2);
        dieLab4 = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(3);
        dieLab5 = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(4);

        dieLab1.Visible = false;
        dieLab2.Visible = false;
        dieLab3.Visible = false;
        dieLab4.Visible = false;
        dieLab5.Visible = false;

        cupShakeAudio = GetTree().Root.GetChild(0).GetChild(3).GetChild<AudioStreamPlayer3D>(5);
        freqLabel = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(6);
        faceLabel = GetTree().Root.GetChild(0).GetChild(3).GetChild<Label3D>(7);
        
        warden = GetTree().Root.GetChild(0).GetChild<Warden>(2);
        playerCam = GetTree().Root.GetChild(0).GetChild<PlayerCamera>(1);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        freqLabel.Text = playerCam.playfreqBid.ToString();
        faceLabel.Text = playerCam.playfaceBid.ToString();
    }

    public void _on_PlayerCamera_CupShakenEventHandler(){
        shakenOnce = true;

        cupShakeAudio.Play();

        for(int i = 0; i < playerDiceArray.Length; i++){
            playerDiceArray[i] = ranNumGen.Next(1, 7);

            if(i == 0){
                dieLab1.Text = playerDiceArray[i].ToString();
            }
            else if(i == 1){
                dieLab2.Text = playerDiceArray[i].ToString();
            }
            else if(i == 2){
                dieLab3.Text = playerDiceArray[i].ToString();
            }
            else if(i == 3){
                dieLab4.Text = playerDiceArray[i].ToString();
            }
            else if(i == 4){
                dieLab5.Text = playerDiceArray[i].ToString();
            }
        }

        // dieLab1.Text = ranNumGen.Next(1, 7).ToString();
        // dieLab2.Text = ranNumGen.Next(1, 7).ToString();
        // dieLab3.Text = ranNumGen.Next(1, 7).ToString();
        // dieLab4.Text = ranNumGen.Next(1, 7).ToString();
        // dieLab5.Text = ranNumGen.Next(1, 7).ToString();
    }

    public void _on_PlayerCamera_CupLiftEventHandler(){
        for(int i = 0; i < playerDiceArray.Length; i++){
            if(i == 0){
                dieLab1.Visible = true;
            }
            else if(i == 1){
                dieLab2.Visible = true;
            }
            else if(i == 2){
                dieLab3.Visible = true;
            }
            else if(i == 3){
                dieLab4.Visible = true;
            }
            else if(i == 4){
                dieLab5.Visible = true;
            }
        }
    }

    public void _on_PlayerCamera_CupDownEventHandler(){
        dieLab1.Visible = false;
        dieLab2.Visible = false;
        dieLab3.Visible = false;
        dieLab4.Visible = false;
        dieLab5.Visible = false;
    }

    //IMPORTANT NOTE: I am using the `Dice` Class as the mediator between bluffs because ->
    //-> it can easily acquire and change both the players and "Warden"s dice.
    //Player calls bluff:
    public void _on_PlayerCamera_BluffInputHandler(int wardenFreq, int wardenFace){
        int[] totalFreqArray = new int[6];
        for(int i = 0; i < playerDiceArray.Length; i++){
            int dieFaceP = playerDiceArray[i] - 1;//Subtract `1` to fit indicies better.
            totalFreqArray[dieFaceP] += 1;
        }

        for(int i = 0; i < warden.dieArray.Length; i++){
            int dieFaceW = warden.dieArray[i] - 1;//Subtract `1` to fit indicies better.
            totalFreqArray[dieFaceW] += 1;
        }

        if(totalFreqArray[wardenFace-1] < wardenFreq){
            GD.Print("PLAYER WON THE BLUFF");
            warden.playerFirst = true;
            EmitSignal("PlayerWonBluffEventHandler");
        }
        else{
            GD.Print("WARDEN WON THE BLUFF");
            playerDiceArray = new int[playerDiceArray.Length-1];//Remove one die from the ->
            //-> players cup by making a new `playerDiceArray` that has `Length` one less ->
            //-> than previous `Length`.
            GD.Print("PLAYER DICE AMOUT: " + playerDiceArray.Length);
            warden.playerFirst = false;
            EmitSignal("WardenWonBluffEventHandler");
        }
    }
    
    //"Warden" calls bluff:
    public void _on_Warden_WBluffEventHandler(){
        // warden.wardenLabel.Text = "CALLING [REDACTED]'s BLUFF!";
        // warden.wardenLabel.Visible = true;
        int[] totalFreqArray = new int[6];
        for(int i = 0; i < playerDiceArray.Length; i++){
            int dieFaceP = playerDiceArray[i] - 1;//Subtract `1` to fit indicies better.
            totalFreqArray[dieFaceP] += 1;
        }

        for(int i = 0; i < warden.dieArray.Length; i++){
            int dieFaceW = warden.dieArray[i] - 1;//Subtract `1` to fit indicies better.
            totalFreqArray[dieFaceW] += 1;
        }

        if(totalFreqArray[playerCam.playfaceBid-1] < playerCam.playfreqBid){
            GD.Print("WARDEN WON THE BLUFF");
            playerDiceArray = new int[playerDiceArray.Length-1];//Remove one die from the ->
            //-> players cup by making a new `playerDiceArray` that has `Length` one less ->
            //-> than previous `Length`.
            GD.Print("PLAYER DICE AMOUT: " + playerDiceArray.Length);
            warden.playerFirst = false;
            EmitSignal("WardenWonBluffEventHandler");
        }
        else{
            GD.Print("PLAYER WON THE BLUFF");
            warden.playerFirst = true;
            EmitSignal("PlayerWonBluffEventHandler");
        }
    }
}
