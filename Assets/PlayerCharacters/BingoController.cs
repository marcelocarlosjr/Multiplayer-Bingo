using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using System.Linq;

public class BingoController : NetworkComponent
{
    public Text PlayerName;
    public string PName = "<Default>";

    public int[] bingoNumbers;
    public bool unique;
    public bool[] WinningSpots;

    public override void HandleMessage(string flag, string value)
    {
       if(flag == "START" && IsClient)
        {
            bingoNumbers = value.Split(',').Select(int.Parse).ToArray();
            UpdateBingoUI();
        }

       if(flag == "CORRECT" && IsClient)
        {
            int i = int.Parse(value);
            Debug.Log(i);
            UpdateBingoUI(i);
        }
       if(flag == "WINNER" && IsClient && value == "1")
        {
            this.transform.GetChild(0).GetChild(2).GetComponent<Image>().color = new Color32(0, 255, 0, 75);

        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        foreach (NPM p in GameObject.FindObjectsOfType<NPM>())
        {
            if (p.Owner == this.Owner)
            {
                yield return new WaitUntil(() => p.IsReady);

                PName = p.PName;
                this.PlayerName.text = PName;
            }
        }
        yield return new WaitForSeconds(.1f);

        if (IsServer)
        {
            if (IsDirty)
            {
                SendUpdate("START", IntArrayToString(bingoNumbers));
                IsDirty = false;
            }
        }
    }

    void Start()
    {
        bingoNumbers = new int[25];
        WinningSpots = new bool[25];
        if (IsServer)
        {
            GenerateBingo(0);
        }

        if (this.Owner == MyCore.LocalPlayerId)
        {
            this.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().color = new Color(0, 140, 0);
        }
        else if(this.Owner != MyCore.LocalPlayerId)
        {
            this.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().color = new Color(255, 0, 0);
        }
    }

    void Update()
    {
        
    }

    public string IntArrayToString(int[] ints)
    {
        return string.Join(",", ints.Select(x => x.ToString()).ToArray());
    }

    public void UpdateBingoUI()
    {
        for(int i = 0; i < 25; i++)
        {
            this.transform.GetChild(0).GetChild(1).GetChild(i).GetChild(0).GetComponent<Text>().text = bingoNumbers[i].ToString();
        }
    }

    public void UpdateBingoUI(int index)
    {
        this.transform.GetChild(0).GetChild(1).GetChild(index).GetComponent<Image>().color = new Color(0, 255, 0, 100);
    }

    public void CheckBingo(int num)
    {
        if (IsServer)
        {
            for (int i = 0; i < 25; i++)
            {
                if (bingoNumbers[i] == num)
                {
                    WinningSpots[i] = true;
                    SendUpdate("CORRECT", i.ToString());
                    CheckWinner(0,0);
                }
            }
        }
    }

    public void CheckWinner(int r, int c)
    {
        if(r<=20 || c <= 4)
        {
            if (WinningSpots[0 + r] && WinningSpots[1 + r] && WinningSpots[2 + r] && WinningSpots[3 + r] && WinningSpots[4 + r])
            {
                FindObjectOfType<GameMaster>().WinnerSelected(this.Owner);
            }
            if (WinningSpots[0 + c] && WinningSpots[5 + c] && WinningSpots[10 + c] && WinningSpots[15 + c] && WinningSpots[20 + c])
            {
                FindObjectOfType<GameMaster>().WinnerSelected(this.Owner);
            }
            if (WinningSpots[0] && WinningSpots[6] && WinningSpots[12] && WinningSpots[18] && WinningSpots[24])
            {
                FindObjectOfType<GameMaster>().WinnerSelected(this.Owner);
            }
            if (WinningSpots[4] && WinningSpots[8] && WinningSpots[12] && WinningSpots[16] && WinningSpots[20])
            {
                FindObjectOfType<GameMaster>().WinnerSelected(this.Owner);
            }
            CheckWinner(r + 5, c + 1);
        }
    }

    public void SetWinner()
    {
        SendUpdate("WINNER", "1");
    }

    public void GenerateBingo(int r)
    {
        if(r == 5 && IsServer)
        {
            SendUpdate("START", IntArrayToString(bingoNumbers));
        }

        if (IsServer && r < 5)
        {
            for(int i = r; i<25; i+=5)
            {
                bingoNumbers[i] = Random.Range((1 + (r * 15)), ((r*15) + 15));
                unique = false;
                while (!unique)
                {
                    for(int j = r; j < 25; j += 5)
                    {
                        if (i != j)
                        {
                            if(bingoNumbers[i] == bingoNumbers[j])
                            {
                                unique = false;
                                bingoNumbers[i] = Random.Range((1 + (r * 15)), ((r * 15) + 15));
                                break;
                            }
                        }
                        unique = true;
                    }
                }
            }
            GenerateBingo(r + 1);
        }
    }
}
