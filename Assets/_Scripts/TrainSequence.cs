using UnityEngine;
using System.Collections.Generic;

public class TrainSequence : MonoBehaviour
{
    public Holder holder_A;
    public Holder holder_B;
    public Holder holder_C;

    [Space, Space]

    public RectTransform clone;

    [Space, Space]

    //public List<string> activeHolders = new List<string>();
    public List<Holder> activeHolders = new List<Holder>();

    Holder currentHolder;
    Holder tempHolder;
    int holderCounter;
    
    public void SpawnHolder(APIManager api, APIManager.Data data)
    {
        if(currentHolder == null)
        {
            var newHolder = (Holder)Instantiate(holder_A, this.transform);
            newHolder.SetPhoto(api, data);
            //activeHolders.Add(newHolder.type.ToString());
            activeHolders.Add(newHolder);
            currentHolder = newHolder;
            holderCounter++;
        }
        else if (currentHolder != null)
        {
            switch(currentHolder.isOccupied)
            {
                case true:
                    switch(currentHolder.type)
                    {
                        case HolderType.A:
                            if(holderCounter > 1)
                                //if (activeHolders[holderCounter - 2] == "B")
                                if (activeHolders[holderCounter - 2].type.ToString() == "B")
                                    tempHolder = holder_C;
                                else
                                    tempHolder = holder_B;
                            else
                                tempHolder = holder_B;
                            break;
                        default: tempHolder = holder_A; break;
                    }

                    var newHolder = (Holder)Instantiate(tempHolder, this.transform);
                    newHolder.SetPhoto(api, data);
                    //activeHolders.Add(newHolder.type.ToString());
                    activeHolders.Add(newHolder);
                    currentHolder = newHolder;
                    holderCounter++;
                    break;
                case false:
                    currentHolder.SetPhoto(api, data);
                    break;
            }
        }
    }

    public void SpawnHolder(Picture photo)
    {
        if (currentHolder == null)
        {
            var newHolder = (Holder)Instantiate(holder_A, this.transform);
            newHolder.SetPhoto(photo);
            //activeHolders.Add(newHolder.type.ToString());
            activeHolders.Add(newHolder);
            currentHolder = newHolder;
            holderCounter++;
        }
        else if (currentHolder != null)
        {
            switch (currentHolder.isOccupied)
            {
                case true:
                    switch (currentHolder.type)
                    {
                        case HolderType.A:
                            if (holderCounter > 1)
                                //if (activeHolders[holderCounter - 2] == "B")
                                if (activeHolders[holderCounter - 2].type.ToString() == "B")
                                    tempHolder = holder_C;
                                else
                                    tempHolder = holder_B;
                            else
                                tempHolder = holder_B;
                            break;
                        default: tempHolder = holder_A; break;
                    }

                    var newHolder = (Holder)Instantiate(tempHolder, this.transform);
                    newHolder.SetPhoto(photo);
                    //activeHolders.Add(newHolder.type.ToString());
                    activeHolders.Add(newHolder);
                    currentHolder = newHolder;
                    holderCounter++;
                    break;
                case false:
                    currentHolder.SetPhoto(photo);
                    break;
            }
        }
    }
}
