using UnityEngine;
using CardEnums;
using System.Xml;
using System.Text;
using UnityEditor;

using System.IO;

class CountingFilePath
{
    public static string filePath(int idx)
    {
        /*
        Windows Store Apps: % userprofile %\AppData\Local\Packages\< productname >\LocalState.
        iOS: / var / mobile / Containers / Data / Application /< guid >/ Documents.
        Adroid : /storage/emulated/0/Android/data/<packagename>/files
         */
        return Application.persistentDataPath +
                "/counting" + (idx).ToString() + ".xml";
    }
}

public class Situation_Info
{
    //Hit
    public int total_Hit = 0;
    public float rate_Hit = 0.0f;
    //Stand
    public int total_Stand = 0;
    public float rate_Stand = 0.0f;
    //DoubleDown
    public int total_DoubleDown = 0;
    public float rate_DoubleDown = 0.0f;
    //Split
    public int total_Split = 0;
    public float rate_Split = 0.0f;
    //Insurance
    public int total_Insurance = 0;
    public float rate_Insurance = 0.0f;

    public string bestChoice;

    bool isSplit = false;
    bool isInsurance = false;

    public Situation_Info(){}
    public Situation_Info(
        string hitBundle,
        string stBundle,
        string ddBundle,
        string spBundle,
        string inBundle,
        bool _isSplit,
        bool _isInsurance)
    {
        SetInfo(out total_Hit, out rate_Hit, hitBundle);
        SetInfo(out total_Stand, out rate_Stand, stBundle);
        SetInfo(out total_DoubleDown, out rate_DoubleDown, ddBundle);
        SetInfo(out total_Split, out rate_Split, spBundle);
        SetInfo(out total_Insurance, out rate_Insurance, inBundle);
        isSplit = _isSplit;
        isInsurance = _isInsurance;
    }
    public string GetHitBunch()
    {
        return (total_Hit.ToString() + "/" + rate_Hit.ToString("F3"));
    }
    public string GetStandBunch()
    {
        return (total_Stand.ToString() + "/" + rate_Stand.ToString("F3"));
    }
    public string GetDoubleDownBunch()
    {
        return (total_DoubleDown.ToString() + "/" + rate_DoubleDown.ToString("F3"));
    }
    public string GetSplitBunch()
    {
        return (total_Split.ToString() + "/" + rate_Split.ToString("F3"));
    }
    public string GetInsuranceBunch()
    {
        return (total_Insurance.ToString() + "/" + rate_Insurance.ToString("F3"));
    }
    public string BestHand()
    {
        float[] rates= new float[6]{
                    rate_Stand,
                    rate_Hit,
                    rate_DoubleDown,
                    (isSplit)? rate_Split:-999f,
                    (isInsurance)? rate_Insurance:-999f,
                    -0.5f
        };

        int bestIdx = 0;
        float bestRate = -9999f;
        for (int i = 0; i < rates.Length; ++i)
        {
            if (rates[i] > bestRate)
            {
                bestIdx = i;
                bestRate = rates[i];
            }
        }
        switch (bestIdx)
        {
            case 0: 
                bestChoice = "Stand";
                break;
            case 1:
                bestChoice = "Hit";
                break;
            case 2:
                bestChoice = "DoubleDown";
                break;
            case 3:
                bestChoice = "Split";
                break;
            case 4:
                bestChoice = "Insurance";
                break;
            case 5:
                bestChoice = "Surrender";
                break;
        }

        return bestChoice;
    }

    void SetInfo(out int total, out float rate, string bunch) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    {
        int barIndex;
        string front;
        string back;

        barIndex = bunch.IndexOf('/');
        front = bunch.Remove(barIndex);
        back = bunch.Remove(0, front.Length + 1);
        
        total = int.Parse(front);
        rate = float.Parse(back);
    }
}
public class Burst_Info
{
    public int total;
    public float rate;
    
    public Burst_Info(string bunch)
    {
        int barIndex;
        string front;
        string back;

        barIndex = bunch.IndexOf('/');
        front = bunch.Remove(barIndex);
        back = bunch.Remove(0, front.Length + 1);

        total = int.Parse(front);
        rate = float.Parse(back);
    }
    public string GetBunch()
    {
        return (total.ToString() + "/" + rate.ToString("F4"));
    }
}

public class FirstHandValue_To_SituationKind
{
    const int doubleA= 24;
    const int double2 = 25;
    const int double10 = 33;

    public static int Convert(HAND_VALUE value, bool isSoftHand)
    {
        if (isSoftHand)
        {
            if (value == HAND_VALUE.VALUE12)
                return doubleA;
            else
                return ((int)value + 2);
        }
        else
        {
            if (value == HAND_VALUE.VALUE20)
            {
                return double10;
            }
            else if(value == HAND_VALUE.VALUE4)
            {
                return double2;
            }
            else
            {
                return ((int)value - 5);
            }
        }
    }
}
public struct ResultInfo
{
    public int countingNumber;
    public int dealerIdx;
    public int playerIdx;
    public ChoiceKind firstChoice;

    public float winningRate;
}

public class DB_Manager : MonoBehaviour
{
    // Singleton---------------
    static DB_Manager sInstance;
    public static DB_Manager Instance
    {
        get
        {
            if (sInstance == null)
            {
                GameObject newObj = new GameObject("_StatisticsManager");
                sInstance = newObj.AddComponent<DB_Manager>();
            }
            return sInstance;
        }
    }
    //-------------------
    public const int MAX_CCN_chart = 20;
    public const int MIN_CCN_chart = -20;
    public const int RANGE_CCN_chart = -MIN_CCN_chart + MAX_CCN_chart + 1;
    public const int MAX_CCN = 30;
    public const int MIN_CCN = -30;
    public const int RANGE_CCN = -MIN_CCN + MAX_CCN + 1;

    Situation_Info[,,] situations = new Situation_Info[RANGE_CCN, 10, 34];
    Burst_Info[,] bursts = new Burst_Info[RANGE_CCN, 9];

    
    //_______________________Initialize & CallBack___________________________________________
    void Awake()
    {
        Load();

        DontDestroyOnLoad(gameObject);
    }
    void OnApplicationQuit()
    {
        Save();
    }
    //_______________________Access_______________________________________________
    public Situation_Info GetSingle(int counting, int dealer, int player)
    {
        return situations[counting + MAX_CCN, dealer, player];
    }
    public void SetSingle(int counting, int dealer, int player, Situation_Info value)
    {
        situations[counting + MAX_CCN, dealer, player] = value;
    }
    public Burst_Info GetBurst(int count, int card)
    {
        return bursts[count + MAX_CCN, card - 12]; 
    }
    public void SetBurst(int count, int card, Burst_Info value)
    {
        bursts[count + MAX_CCN, card - 12] = value;
    }

    public void AddResultInfo(ResultInfo sampleInfo)
    {
        if(sampleInfo.countingNumber < MIN_CCN || sampleInfo.countingNumber > MAX_CCN)
            return;

        Situation_Info situation = GetSingle(sampleInfo.countingNumber, sampleInfo.dealerIdx, sampleInfo.playerIdx);
        
        switch (sampleInfo.firstChoice)
        {
            case ChoiceKind.Hit:
                situation.rate_Hit = (situation.total_Hit * situation.rate_Hit + sampleInfo.winningRate) / (situation.total_Hit + 1);
                ++(situation.total_Hit);
                break;
            case ChoiceKind.Stand:
                situation.rate_Stand = (situation.total_Stand * situation.rate_Stand + sampleInfo.winningRate) / (situation.total_Stand + 1);
                ++(situation.total_Stand);
                break;
            case ChoiceKind.DoubleDown:
                situation.rate_DoubleDown = (situation.total_DoubleDown * situation.rate_DoubleDown + sampleInfo.winningRate) / (situation.total_DoubleDown + 1);
                ++(situation.total_DoubleDown);
                break;
            case ChoiceKind.Split:
                situation.rate_Split = (situation.total_Split * situation.rate_Split + sampleInfo.winningRate) / (situation.total_Split + 1);
                ++(situation.total_Split);
                break;
            case ChoiceKind.Insurance:
                situation.rate_Insurance = (situation.total_Insurance * situation.rate_Insurance + sampleInfo.winningRate) / (situation.total_Insurance + 1);
                ++(situation.total_Insurance);
                break;
            case ChoiceKind.NotInsurance:
                if (sampleInfo.playerIdx == (int)SITUATION_KIND.BLACKJACK)
                {
                    situation.rate_Stand = (situation.total_Stand * situation.rate_Stand + sampleInfo.winningRate) / (situation.total_Stand + 1);
                    ++(situation.total_Stand);
                }
                break;
            default:
                break;
        }
    }
    public void AddBurstInfo(int count, int p_value)
    {
        Burst_Info burstInfo = GetBurst(count, p_value);
        
        burstInfo.rate = (burstInfo.total * burstInfo.rate + 1f) / (burstInfo.total + 1f);
        ++(burstInfo.total);
    }
    public void AddNotBurstInfo(int count, int p_value)
    {
        Burst_Info burstInfo = GetBurst(count, p_value);

        burstInfo.rate = (burstInfo.total * burstInfo.rate) / (burstInfo.total + 1f);
        ++(burstInfo.total);
    }
    
    //_______________________Xml___________________________________________________

    public void Save()
    {
        XmlWriter writer = null;
        XmlWriterSettings setting = new XmlWriterSettings();
        setting.Indent = true;
        setting.Encoding = Encoding.UTF8;

        for (int i = MIN_CCN; i <= MAX_CCN; ++i)
        {
            using (writer = XmlWriter.Create(CountingFilePath.filePath(i), setting))
            {
                writer.WriteStartElement("Counting");

                SaveBurstInfo(writer, i);
                
                SaveInsuranceHand(writer, i);
                
                SaveNotInsuranceHand(writer, i);

                writer.WriteEndElement();

                writer.Close();
            }
        }
    }
    void SaveBurstInfo(XmlWriter writer, int i)
    {
        writer.WriteStartElement("Burst");
        for (int j = 12; j <= 20; ++j)
        {
            writer.WriteStartElement("Card" + j.ToString());
            writer.WriteString(GetBurst(i, j).GetBunch());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }
    void SaveInsuranceHand(XmlWriter writer, int i)
    {
        writer.WriteStartElement("Dealer_Hand");
        writer.WriteAttributeString("_id", "1");

        for (int k = 0; k < 24; ++k)
        {
            writer.WriteStartElement("Player_Hand");
            writer.WriteAttributeString("_id", ((SITUATION_KIND)k).ToString());

            writer.WriteStartElement("Hit");
            writer.WriteString(GetSingle(i, 0, k).GetHitBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("Stand");
            writer.WriteString(GetSingle(i, 0, k).GetStandBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("DoubleDown");
            writer.WriteString(GetSingle(i, 0, k).GetDoubleDownBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("Insurance");
            writer.WriteString(GetSingle(i, 0, k).GetInsuranceBunch());
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        for (int k = 24; k < 34; ++k)//___________Split__________________
        {
            writer.WriteStartElement("Player_Hand");
            writer.WriteAttributeString("_id", ((SITUATION_KIND)k).ToString());

            writer.WriteStartElement("Hit");
            writer.WriteString(GetSingle(i, 0, k).GetHitBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("Stand");
            writer.WriteString(GetSingle(i, 0, k).GetStandBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("DoubleDown");
            writer.WriteString(GetSingle(i, 0, k).GetDoubleDownBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("Split");
            writer.WriteString(GetSingle(i, 0, k).GetSplitBunch());
            writer.WriteEndElement();
            writer.WriteStartElement("Insurance");
            writer.WriteString(GetSingle(i, 0, k).GetInsuranceBunch());
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }
    void SaveNotInsuranceHand(XmlWriter writer, int i)
    {
        for (int j = 1; j < 10; ++j)
        {
            writer.WriteStartElement("Dealer_Hand");
            writer.WriteAttributeString("_id", (j + 1).ToString());
            for (int k = 0; k < 24; ++k)
            {
                writer.WriteStartElement("Player_Hand");
                writer.WriteAttributeString("_id", ((SITUATION_KIND)k).ToString());

                writer.WriteStartElement("Hit");
                writer.WriteString(GetSingle(i, j, k).GetHitBunch());
                writer.WriteEndElement();
                writer.WriteStartElement("Stand");
                writer.WriteString(GetSingle(i, j, k).GetStandBunch());
                writer.WriteEndElement();
                writer.WriteStartElement("DoubleDown");
                writer.WriteString(GetSingle(i, j, k).GetDoubleDownBunch());
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
            for (int k = 24; k < 34; ++k)//______________Split__________________
            {
                writer.WriteStartElement("Player_Hand");
                writer.WriteAttributeString("_id", ((SITUATION_KIND)k).ToString());

                writer.WriteStartElement("Hit");
                writer.WriteString(GetSingle(i, j, k).GetHitBunch());
                writer.WriteEndElement();
                writer.WriteStartElement("Stand");
                writer.WriteString(GetSingle(i, j, k).GetStandBunch());
                writer.WriteEndElement();
                writer.WriteStartElement("DoubleDown");
                writer.WriteString(GetSingle(i, j, k).GetDoubleDownBunch());
                writer.WriteEndElement();
                writer.WriteStartElement("Split");
                writer.WriteString(GetSingle(i, j, k).GetSplitBunch());
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }

    public void Load()
    {
        XmlDocument doc = new XmlDocument();

        // check if data files are created already
        if (!System.IO.File.Exists(CountingFilePath.filePath(0)))
        {
            SetFirstAllFile();
        }

        for (int i = MIN_CCN; i <= MAX_CCN; ++i)
        {
            doc.Load(CountingFilePath.filePath(i));

            XmlNodeList xnList = doc.SelectNodes("/Counting/Burst");
            LoadBurstInfo(i, xnList);

            xnList = doc.SelectNodes("/Counting/Dealer_Hand/Player_Hand");
            int playerHand = 0;

            //________________________________________인슈어런스________________________________
            for (int j = 0; j < 24; ++j)// 
            {
                SetSingle(i, 0, playerHand, 
                    new Situation_Info(
                    xnList[j]["Hit"].InnerText,
                    xnList[j]["Stand"].InnerText,
                    xnList[j]["DoubleDown"].InnerText,
                    "-1/-1",
                    xnList[j]["Insurance"].InnerText,
                    false,
                    true));

                ++playerHand;
            }
            for (int j = 24; j < 34; ++j) // 스플릿
            {
                SetSingle(i, 0, playerHand, 
                    new Situation_Info(
                    xnList[j]["Hit"].InnerText,
                    xnList[j]["Stand"].InnerText,
                    xnList[j]["DoubleDown"].InnerText,
                    xnList[j]["Split"].InnerText,
                    xnList[j]["Insurance"].InnerText,
                    true,
                    true));

                ++playerHand;
            }
            playerHand = 0;
            for (int k = 1; k < 10; ++k)
            {
                for (int j = 0; j < 24; ++j)// 
                {
                    int idx = j + k * 34;

                    SetSingle(i, k, playerHand, 
                        new Situation_Info(
                        xnList[idx]["Hit"].InnerText,
                        xnList[idx]["Stand"].InnerText,
                        xnList[idx]["DoubleDown"].InnerText,
                        "-1/-1",
                        "-1/-1",
                        false,
                        false));

                    ++playerHand;
                }
                for (int j = 24; j < 34; ++j) // 스플릿
                {
                    int idx = j + k * 34;

                    SetSingle(i, k, playerHand, 
                        new Situation_Info(
                        xnList[idx]["Hit"].InnerText,
                        xnList[idx]["Stand"].InnerText,
                        xnList[idx]["DoubleDown"].InnerText,
                        xnList[idx]["Split"].InnerText,
                        "-1/-1",
                        true,
                        false));

                    ++playerHand;
                }// nodeList

                playerHand = 0;
            }

        }// counting
    }

    private void LoadBurstInfo(int i, XmlNodeList xnList)
    {
        for (int j = 12; j <= 20; ++j)// 
        {
            SetBurst(i, j, new Burst_Info(xnList[0]["Card" + j.ToString()].InnerText));
        }
    }

    void SetFirstAllFile()
    {
        XmlWriter writer = null;
        XmlWriterSettings setting = new XmlWriterSettings();
        setting.Indent = true;
        setting.Encoding = Encoding.UTF8;

        for (int i = MIN_CCN; i <= MAX_CCN; ++i)
        {
            using (writer = XmlWriter.Create(CountingFilePath.filePath(i), setting))
            {
                writer.WriteStartElement("Counting"); //루트노드

                writer.WriteStartElement("Burst"); // 버스트 관련
                for (int j = 12; j <= 20; ++j)
                {
                    writer.WriteStartElement("Card" + j.ToString());
                    writer.WriteString("0/0");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                for (int j = 0; j < 10; ++j)
                {
                    writer.WriteStartElement("Dealer_Hand");
                    writer.WriteAttributeString("_id", (j + 1).ToString()); // RemoveAfterComplete
                    for (int k = 0; k < 34; ++k)
                    {
                        writer.WriteStartElement("Player_Hand");
                        writer.WriteAttributeString("_id", ((SITUATION_KIND)k).ToString()); // RemoveAfterComplete

                        writer.WriteStartElement("Hit");
                        writer.WriteString("0/0");
                        writer.WriteEndElement();
                        writer.WriteStartElement("Stand");
                        writer.WriteString("0/0");
                        writer.WriteEndElement();
                        writer.WriteStartElement("DoubleDown");
                        writer.WriteString("0/0");
                        writer.WriteEndElement();
                        writer.WriteStartElement("Split");
                        writer.WriteString("0/0");
                        writer.WriteEndElement();
                        writer.WriteStartElement("Insurance");
                        writer.WriteString("0/0");
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.Close();
            }
        }
    }

}
