using Godot;
using System;
using System.Net;

public partial class Warden : Node3D
{
    [Signal]
    public delegate void WBidMadeEventHandler(int highestFreq, int highestFace);
    [Signal]
    public delegate void WBluffEventHandler();

    Random randNumGen = new Random();
    Label3D wardenLabel;
    Timer allTimer;
    Label3D diceLabel;
    Timer decideTimer;
    AudioStreamPlayer3D bidSelectAudio;
    AudioStreamPlayer3D PWB1Audio;
    AudioStreamPlayer3D PWB2Audio;
    PlayerCamera playerCam;
    public bool playerFirst {get; set;} = false;
    bool initialBid = true;
    public int[] dieArray {get; set;} = new int[5];
    //`Array` instance used to keep track of the frequency of each die.
    int[] freqArray = new int[6];
    int quantVal;
    int faceVal;
    int highestFreq = 0;
    int highestFace = 0;
    int bidIncrease = 0;
    int intuitionIncrease = 0;

    int TOTAL_DICE = 10;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        wardenLabel = GetTree().Root.GetChild(0).GetChild(2).GetChild<Label3D>(0);
        wardenLabel.Text = "Hit \"enter\", and we can get this show on the road!";
        wardenLabel.Visible = true;

        allTimer = GetTree().Root.GetChild(0).GetChild(2).GetChild<Timer>(1);
        diceLabel = GetTree().Root.GetChild(0).GetChild(2).GetChild<Label3D>(2);
        diceLabel.Visible = false;

        decideTimer = GetTree().Root.GetChild(0).GetChild(2).GetChild<Timer>(3);
        bidSelectAudio = GetTree().Root.GetChild(0).GetChild(2).GetChild<AudioStreamPlayer3D>(4);
        PWB1Audio = GetTree().Root.GetChild(0).GetChild(2).GetChild<AudioStreamPlayer3D>(5);
        PWB2Audio = GetTree().Root.GetChild(0).GetChild(2).GetChild<AudioStreamPlayer3D>(6);

        playerCam = GetTree().Root.GetChild(0).GetChild<PlayerCamera>(1);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if(diceLabel.Visible == true && decideTimer.TimeLeft != 0){
            quantVal = randNumGen.Next(1, 7);
            faceVal =  randNumGen.Next(1, 7);

            diceLabel.Text = quantVal + " " + faceVal;
        }
    }

    public void _on_PlayerCamera_NoEyeContactEventHandler(){
        wardenLabel.Visible = true;
        wardenLabel.Text = "Come on, look at me! We want it to be fair, don't we!";
        allTimer.Start();
    }

    public void _on_AllTimer_timeout(){
        wardenLabel.Visible = false;
    }

    public void _on_PlayerCamera_CupShakenEventHandler(){
        if(playerFirst == false){
            wardenLabel.Visible = false;
            diceLabel.Visible = true;

            bidSelectAudio.Play();
            decideTimer.Start();
        }
        else{
            wardenLabel.Visible = false;
            // initialBid = false;
            GD.Print("WAITING ON PLAYER TO TAKE FIRST TURN");
        }
    }

    public void _on_DiceDecideTimer_timeout(){
        //FIRST TURN AND WARDEN FIRST:
        if(initialBid == true && playerFirst == false){
            //New round so all round specific variables are reset to default.
            highestFreq = 1;
            highestFace = 1;
            freqArray = new int[7];

            BeginRoundWarden();
        }
        //FIRST TURN AND PLAYER FIRST:
        else if(initialBid == true && playerFirst == true){
            //TWO LINES BELOW: These two lines of code make sure that the `highestFreq` ->
            //-> and `highestFace` variables are not set to the default of `0`, but instead ->
            //-> set to whatever frequency and face bid the player chose. This is ->
            //-> important because we want to make sure the "Warden" interacts with the ->
            //-> player's first bid and does not make any illegal game actions.
            highestFreq = playerCam.playfreqBid;
            highestFace = playerCam.playfaceBid;

            //New round so all round specific variables are reset to default.
            freqArray = new int[7];
            
            BeginRoundPlayer();
        }
        //MID ROUND TURN:
        else{
            //NOTE: `decisionNUM[0]` is the "frequency" of the bid, and `decisionNUM[1]` is ->
            //-> the "face" of the bid.
            (int, int) decisionNUM = MakeBid(dieArray);
            quantVal = decisionNUM.Item1;
            faceVal = decisionNUM.Item2;

            diceLabel.Text = quantVal + " " + faceVal;
            //FUTURE: Make a return statement that is a boolean and will stop this signal ->
            //-> from firing if the "Warden" calls a bluff.
            EmitSignal("WBidMadeEventHandler", highestFreq, highestFace);
        }
    }

    public void _on_PlayerCamera_SubmitInputHandler(){
        wardenLabel.Visible = false;
        diceLabel.Visible = true;

        bidSelectAudio.Play();
        decideTimer.Start();

        // (int, int) decisionNUM = MakeBid(dieArray);
        // quantVal = decisionNUM.Item1;
        // faceVal = decisionNUM.Item2;

        // diceLabel.Text = quantVal + " " + faceVal;
        // EmitSignal("WBidMadeEventHandler", highestFreq);
    }

    //PLAYER WON BLUFF:
    public void _on_Dice_PlayerWonBluffEventHandler(){
        if(randNumGen.Next(1,3) % 2 == 0){
            PWB1Audio.Play();
        }
        else{
            PWB2Audio.Play();
        }
        dieArray = new int[dieArray.Length-1];
        TOTAL_DICE -= 1;
        GD.Print("WARDENS DIE: " + dieArray.Length);

        wardenLabel.Text = "[SUCCESS PROMPT] While I cant feel pain, I sure can feel your laughter!";
        wardenLabel.Visible = true;
        diceLabel.Visible = false;

        allTimer.Start();

        initialBid = true;
    }

    //WARDEN WON BLUFF:
    public void _on_Dice_WardenWonBluffEventHandler(){
        TOTAL_DICE -= 1;
        wardenLabel.Text = "[FAILURE PROMPT [:] INITIATING LAUGHTER PROTOCOL] HA-HA-HA-HAAA!";
        wardenLabel.Visible = true;
        diceLabel.Visible = false;

        allTimer.Start();

        initialBid = true;
    }

    public void BeginRoundWarden(){
        //Creation of "Warden"s roll.
        for(int i = 0; i < dieArray.Length; i++){
            dieArray[i] = randNumGen.Next(1, 7);
            GD.Print(dieArray[i]);
        }

        for(int i = 0; i < dieArray.Length; i++){
            freqArray[dieArray[i]] += 1;
            //IMPORTANT NOTE: You may be wondering, in the line above, why we do not do ->
            //-> `freqArray[dieArray[i]-1] += 1`, where we would be subtracting `1` from ->
            //-> what index we place in the `freqArray` Array instance. This seems like ->
            //-> it would make sense because it would utilize the entire array. For ->
            //-> example, say at `dieArray[i]` it was `1`, this means we would want to ->
            //-> place it at an index in `freqArray` Array where all other `1` elements ->
            //-> are. And we could either choose to put `1` at and index of `0` or an ->
            //-> index of `1` depending on how we chose to write the line above. It seems ->
            //-> that an index of `0` would be better, since it does not allow for any ->
            //-> empty space within the `freqArray` Array. While that is true, when you ->
            //-> think of it from the perspective of adding additions to the code, and ->
            //-> maybe one day there will be a die of face `0`, and so if we did put it at ->
            //-> index `0-1`, it would be put at an index of `-1` which is out of bounds. ->
            //-> While this would not happen if we just put face `0` at index `0`. So, ->
            //-> to future proof the `freqArray`Array, there will be a small cost of an ->
            //-> empty element at index `0` of `freqArray` Array.

            if(freqArray[dieArray[i]] >= highestFreq){
                highestFreq = freqArray[dieArray[i]];
                highestFace = dieArray[i];
            }
        }

        //NOTE: `decisionNUM[0]` is the "frequency" of the bid, and `decisionNUM[1]` is ->
        //-> the "face" of the bid.
        (int, int) decisionNUM = MakeBid(dieArray);
        quantVal = decisionNUM.Item1;
        faceVal = decisionNUM.Item2;

        diceLabel.Text = quantVal + " " + faceVal;
        EmitSignal("WBidMadeEventHandler", highestFreq, highestFace);
    }

    public void BeginRoundPlayer(){
        //Creation of "Warden"s roll.
        for(int i = 0; i < dieArray.Length; i++){
            dieArray[i] = randNumGen.Next(1, 7);
            GD.Print(dieArray[i]);
        }

        for(int i = 0; i < dieArray.Length; i++){
            freqArray[dieArray[i]] += 1;

            if(freqArray[dieArray[i]] >= highestFreq){
                highestFreq = freqArray[dieArray[i]];
                highestFace = dieArray[i];
            }
        }

        initialBid = false;
        //NOTE: `decisionNUM[0]` is the "frequency" of the bid, and `decisionNUM[1]` is ->
        //-> the "face" of the bid.
        (int, int) decisionNUM = MakeBid(dieArray);
        quantVal = decisionNUM.Item1;
        faceVal = decisionNUM.Item2;

        diceLabel.Text = quantVal + " " + faceVal;
        EmitSignal("WBidMadeEventHandler", highestFreq, highestFace);
    }
    
    public (int, int) MakeBid(int[] dieArray){
        if(initialBid == true){
            initialBid = false;

            //BELOW: The chain of `if-statements` below, dictact the chance and amount the ->
            //-> "Warden" increase its bid frequency (`highestFreq`) by.
            float anteChance = GD.Randf();

            if(anteChance < 0.1f){
                bidIncrease = 0;
            }
            else if(anteChance < 0.15f){
                bidIncrease = 4;
            }
            else if(anteChance < 0.3f){
                bidIncrease = 3;
            }
            else if(anteChance < 0.6f){
                bidIncrease = 2;
            }
            else{
                bidIncrease = 1;
            }

            GD.Print(anteChance);
            highestFreq = highestFreq + bidIncrease;
            return (highestFreq, highestFace);
        }
        else{
            float retainFace = GD.Randf();
            //NOTE: Increase the `highestFreq` bid frequency to be equal to the player's ->
            //-> `playerfreqBid` bid frequency. This is because the "Warden" cannot ->
            //-> bid BELOW the player's bid frequency.
            highestFreq = playerCam.playfreqBid;
            //"Warden" ignores player's "face-value" for bidding, and continues with its ->
            //-> original "face-value" bid.
            //NOTE: If the "Warden"s dice that match the player's face bid, make up less ->
            //-> than `50%` (half) of the "Warden"s dice AND the random chance to match the ->
            //-> face of the player's bid (chance held in the `retainFace` variable) is ->
            //-> not less than `35%` (`0.35f`), then the "Warden" will remain ->
            //-> bidding with it's original face with the `highestFreq`.
            if((float)freqArray[playerCam.playfaceBid]/(dieArray.Length-1) < 0.50f && retainFace >= 0.35f){
                float anteChance = GD.Randf();

                if(anteChance < 0.1f){
                    bidIncrease = 0;
                }
                // else if(anteChance < 0.2f){
                //     bidIncrease = 4;
                // }
                else if(anteChance < 0.2f){
                    bidIncrease = 3;
                }
                else if(anteChance < 0.6f){
                    bidIncrease = 2;
                }
                else{
                    bidIncrease = 1;
                }

                //Determine wether the bid must raise because "face-value" is less than ->
                //-> player's "face-value".
                //NOTE: If the "face-value"s are the same, it will be differed to an ->
                //-> increase in `highestFreq` bid frequency. This is to prevent an ->
                //-> illegal game action if the "Warden" and the player both have a ->
                //-> `highestFace` face bid of `6`.
                if(highestFace <= playerCam.playfaceBid && bidIncrease == 0){
                    highestFreq = highestFreq + bidIncrease + 1;
                }
                else{
                    highestFreq = highestFreq + bidIncrease;
                }

                // if(highestFace > playerCam.playfaceBid){
                //     highestFreq = playerCam.playfreqBid + bidIncrease;
                // }
                // else if(highestFace <= playerCam.playfaceBid && bidIncrease == 0){
                //     highestFreq = playerCam.playfreqBid + bidIncrease + 1;
                // }

                //NOTE: Below `if-statement` is for adding a "inuition" effect to the ->
                //-> "Warden"s choice. This means that the "Warden" will take into account ->
                //-> how many dice of the player's bid face it has when determining to ->
                //-> bluff or not.
                if(freqArray[playerCam.playfaceBid] <= 1){
                    intuitionIncrease = 2;
                }
                else if(freqArray[playerCam.playfaceBid] == 2){
                    intuitionIncrease = 1;
                }
                else if(freqArray[playerCam.playfaceBid] == 4){
                    intuitionIncrease = -1;
                }
                else if(freqArray[playerCam.playfaceBid] >= 5){
                    intuitionIncrease = -2;
                }
                else{
                    intuitionIncrease = 0;
                }

                GD.Print("INTUITION: " + intuitionIncrease);

                //NOTE: Below `if-statement` is for calling bluff on previous bid. First ->
                //-> a certain requirment must be met for there to be the possibility of ->
                //-> a bluff being made by the "Warden".
                if((((float)playerCam.playfreqBid + intuitionIncrease)/TOTAL_DICE) > 0.5f){
                    float bluffChance = GD.Randf();
                    GD.Print("BLUFF CHANCE: " + bluffChance);
                    //NOTE: First `if-statement` is for weeding at the completely ->
                    //-> impossible player bids. For example, the "Warden" has `0` `5`s, ->
                    //-> but the player bids `6` `5`s, that means that even if the player ->
                    //-> had all `5`s there still wouldn't be enough `5`s.
                    //NOTE: The math used to compute this is done by subtracting the ->
                    //-> player's bid frequency by the "Warden"s frequency for that ->
                    //-> bid face. If the resulting output is greater than `5` it means ->
                    //-> that even if the player had all the same face dice, the ->
                    //-> "Warden" does not have enough of that face dice to reach the ->
                    //-> player's bid frequency.
                    if(playerCam.playfreqBid - freqArray[playerCam.playfaceBid] > 5){
                        EmitSignal("WBluffEventHandler");
                    }
                    else if(bluffChance > 0.4f){
                        EmitSignal("WBluffEventHandler");
                    }
                    else{
                        GD.Print("BIDDING MY FACE");
                        return (highestFreq, highestFace);
                    }
                }
                else{
                    GD.Print("BIDDING MY FACE");
                    return (highestFreq, highestFace);
                }
            }
            //"Warden" now goes with the player's "face-value" for bidding.
            else{
                float anteChance = GD.Randf();

                // if(anteChance < 0.2f){
                //     bidIncrease = 4;
                // }
                if(anteChance < 0.2f){
                    bidIncrease = 3;
                }
                else if(anteChance < 0.6f){
                    bidIncrease = 2;
                }
                else{
                    bidIncrease = 1;
                }

                //NOTE: Below `if-statement` is for adding a "inuition" effect to the ->
                //-> "Warden"s choice. This means that the "Warden" will take into account ->
                //-> how many dice of the bid face it has when determining to bluff or not.
                if(freqArray[playerCam.playfaceBid] <= 1){
                    intuitionIncrease = 2;
                }
                else if(freqArray[playerCam.playfaceBid] == 2){
                    intuitionIncrease = 1;
                }
                else if(freqArray[playerCam.playfaceBid] == 4){
                    intuitionIncrease = -1;
                }
                else if(freqArray[playerCam.playfaceBid] >= 5){
                    intuitionIncrease = -2;
                }
                else{
                    intuitionIncrease = 0;
                }

                GD.Print("INTUITION: " + intuitionIncrease);

                //NOTE: Below `if-statement` is for calling bluff on previous bid. First ->
                //-> a certain requirment must be met for there to be the possibility of ->
                //-> a bluff being made by the "Warden".
                if((((float)playerCam.playfreqBid + intuitionIncrease)/TOTAL_DICE) > 0.5f){
                    float bluffChance = GD.Randf();
                    GD.Print("BLUFF CHANCE: " + bluffChance);
                    //NOTE: First `if-statement` is for weeding at the completely ->
                    //-> impossible player bids. For example, the "Warden" has `0` `5`s, ->
                    //-> but the player bids `6` `5`s, that means that even if the player ->
                    //-> had all `5`s there still wouldn't be enough `5`s.
                    //NOTE: The math used to compute this is done by subtracting the ->
                    //-> player's bid frequency by the "Warden"s frequency for that ->
                    //-> bid face. If the resulting output is greater than `5` it means ->
                    //-> that even if the player had all the same face dice, the ->
                    //-> "Warden" does not have enough of that face dice to reach the ->
                    //-> player's bid frequency.
                    if(playerCam.playfreqBid - freqArray[playerCam.playfaceBid] > 5){
                        EmitSignal("WBluffEventHandler");
                    }
                    else if(bluffChance > 0.4f){
                        EmitSignal("WBluffEventHandler");
                    }
                    else{
                        GD.Print("BIDDING PLAYER'S FACE");
                        highestFreq = highestFreq + bidIncrease;
                        return (highestFreq, highestFace);
                    }
                }
                else{
                    GD.Print("BIDDING PLAYER'S FACE");
                    highestFreq = highestFreq + bidIncrease;
                    return (highestFreq, playerCam.playfaceBid);
                }
            }
            //NOTE: Below `return` statement is to catch any errors.
            return (-1,-1);
        }
    }
}
