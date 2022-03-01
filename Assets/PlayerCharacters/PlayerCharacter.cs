using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using System.Linq;

public class PlayerCharacter : NetworkComponent
{
    public Text PlayerName;
    public string PName = "<Default>";

    public int[] bingoNumbers;
    public bool unique;

    public override void HandleMessage(string flag, string value)
    {
       if(flag == "START" && IsClient)
        {
            bingoNumbers = value.Split(',').Select(int.Parse).ToArray();
            UpdateBingoUI();
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

    // Start is called before the first frame update
    void Start()
    {
        bingoNumbers = new int[25];
        if (IsServer)
        {
            GenerateBingo(0);
        }
    }

    // Update is called once per frame
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
