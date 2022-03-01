using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class GameMaster : NetworkComponent
{
    public bool GameStarted = false;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
