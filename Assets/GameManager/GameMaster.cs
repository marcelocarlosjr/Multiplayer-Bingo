using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class GameMaster : NetworkComponent
{
    public bool GameStarted = false;

    public bool Winner = false;

    public bool Unique;

    public List<int> CalledBingos = new List<int>();
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "GAMESTART" && value == "True" && IsClient)
        {
            foreach (NPM p in GameObject.FindObjectsOfType<NPM>())
            {
                p.transform.GetChild(0).gameObject.SetActive(false);
                //GameObject.Find("Cube").SetActive(false);
            }
        }

        if(flag == "BINGONUM" && IsClient)
        {
            this.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = value;
        }


    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            bool tempGameStart;

            NPM[] npm;

            do
            {
                tempGameStart = true;

                npm = GameObject.FindObjectsOfType<NPM>();
                if (npm.Length < 2)
                {
                    tempGameStart = false;
                }
                else
                {
                    foreach(NPM p in npm)
                    {
                        if (!p.IsReady)
                        {
                            tempGameStart = false;
                            break;
                        }
                    }
                    GameStarted = tempGameStart;
                }
                yield return new WaitForSeconds(.1f);
            } while (!tempGameStart);

            SendUpdate("GAMESTART", GameStarted.ToString());


            if(GameStarted == true)
            {
                foreach (NPM p in GameObject.FindObjectsOfType<NPM>())
                {
                    MyCore.NetCreateObject(0, p.Owner, GameObject.Find("P" + (p.Owner+1) + "Start").transform.position , Quaternion.identity);
                }
                if (IsServer)
                {
                    StartCoroutine(BingoTimer());
                }
            }

            if (IsDirty)
            {
                SendUpdate("GAMESTART", GameStarted.ToString());
                IsDirty = false;
            }
            yield return new WaitForSeconds(5);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        GameStarted = false;
    }

    private IEnumerator BingoTimer()
    {
        yield return new WaitForSeconds(1);
        for(int i = 1; i <= 75; i++)
        {
            CalledBingos.Add(i);
        }
        while (!Winner)
        {
            int index = Random.Range(0, CalledBingos.Count);
            foreach(BingoController b in GameObject.FindObjectsOfType<BingoController>())
            {
                SendUpdate("BINGONUM", CalledBingos[index].ToString());
                b.CheckBingo(CalledBingos[index]);
            }
            CalledBingos.RemoveAt(index);
            yield return new WaitForSeconds(1);
        }
    }

    public void WinnerSelected(int id)
    {
        Winner = true;
        foreach (BingoController b in GameObject.FindObjectsOfType<BingoController>())
        {
            if(b.Owner == id)
            {
                b.SetWinner();
            }
        }
        StartCoroutine(DisconnectGame());
    }

    IEnumerator DisconnectGame()
    {
        yield return new WaitForSeconds(14.5f);

        if (IsServer)
        {
            StartCoroutine(MyCore.DisconnectServer());
        }
    }
}
