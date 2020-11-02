using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarDATA : MonoBehaviour
{
    public r_BloodDATA.BloodManager bloodManager;
    public RectTransform hpBar;
    public float hp;

    public int currentMpBar;
    public int currentMpBarBack;

    public float maxMp;

    public UnitHost unitHost;

    public bool canAttack;

    public List<ManaClass> mana = new List<ManaClass>();

    [System.Serializable]
    public class ManaClass
    {
        public ManaClass()
        {
            canvas = GameObject.Find("MpBars");
            mpBarsBox = Instantiate(Resources.Load("UI/MpBack"), canvas.transform) as GameObject;
            mpBarsBox.GetComponent<Image>().enabled = false;

            mpBarBackPref = Instantiate(Resources.Load("UI/MpBack"), mpBarsBox.transform) as GameObject;
            mpBarPref = Instantiate(Resources.Load("UI/Mp"), mpBarsBox.transform) as GameObject;

            mpBarBack = mpBarBackPref.GetComponent<RectTransform>();
            mpBar = mpBarPref.GetComponent<RectTransform>();
            mpBack = 0;
            mp = 1;
            mpBarBack.localScale = new Vector2(0, 1);
            canЕxpendMp = false;
        }

        public GameObject canvas;
        public GameObject mpBarsBox;
        public GameObject mpBarPref;
        public GameObject mpBarBackPref;

        public RectTransform mpBar;
        public RectTransform mpBarBack;

        public float mpTimer;
        public float mp;
        public float mpBack;
        public bool canЕxpendMp;
    }

    void Start()
    {

        bloodManager = GameObject.FindObjectOfType<BloodController>().bloodManager;
        unitHost = bloodManager.container.unitHost;

        hpBar = GameObject.Find("Hp").GetComponent<RectTransform>();
        bloodManager.container.barData = GetComponent<BarDATA>();
        mana.Add(new ManaClass());

        maxMp = 1;
        currentMpBar = 0;
        currentMpBarBack = 0;

        bloodManager.TryAttack += (args) =>
        {
            return Can_MP_DOWN;
        };

        bloodManager.OnAttack += (args) =>
        {
            return Mp_Down(1);
        };
    }

    void Update()
    {
        Mp_Up(0.5f * Time.deltaTime);
        UpdateBars();

        if (Input.GetKey(KeyCode.L))
            DownMPBack(0.5f);
    }

    public void UpdateBars()
    {
        if (unitHost.unit != null)
            hpBar.localScale = new Vector2(unitHost.unit.container.nowHP, 1);

        for (int i = 0; i < mana.Count; i++)
        {
            mana[i].mpBar.localScale = new Vector2(mana[i].mp, 1);
        }

        for (int i = 0; i < mana.Count; i++)
        {
            mana[i].mpBarBack.localScale = new Vector2(mana[i].mpBack, 1);
        }
    }

    private int mpLenght;
    public void Mp_Up(float value)
    {
        for (int i = 0; i < mana.Count; i++)
        {
            mana[i].mpTimer -= Time.deltaTime;
            mana[i].mpTimer = (mana[i].mpTimer < 0)
                              ? 0
                              : mana[i].mpTimer;

            if(mana[i].mpTimer > 0)
                continue;

            mana[i].mp += value;
            mana[i].mp = (mana[i].mp > maxMp)
                         ? maxMp
                         : mana[i].mp;
        }
    }

    public bool Can_MP_DOWN
    {
        get
        {
            currentMpBar = -1;
            for (int i = 0; i < mana.Count; i++)
            {
                if (mana[i].mp != 1)
                    continue;


                currentMpBar = i;
                break;
            }
            if (currentMpBar == -1)
                return false;

            if (!mana[currentMpBar].canЕxpendMp)
                return false;
            return true;
        }
    }
    public bool Mp_Down(float value)
    {
        currentMpBar = -1;
        for (int i = 0; i < mana.Count; i++)
        {
            if (mana[i].mp != 1)
                continue;

            currentMpBar = i;
            break;
        }
        if (currentMpBar == -1)
            return false;

        if (!mana[currentMpBar].canЕxpendMp)
            return false;

        mana[currentMpBar].mp -= value;

        mana[currentMpBar].mp = (mana[currentMpBar].mp < 0)
                                ? 0
                                : mana[currentMpBar].mp;

        mana[currentMpBar].mpTimer = 1f;
        return true;
    }

    public bool isGetMPBack;
    public bool currentGetMPBack;

    public void GetMPBack(float value)
    {     
        if (mana[currentMpBarBack].mpBack >= 1 && currentMpBarBack < mana.Count)
            mana[currentMpBarBack].canЕxpendMp = true;

        if (mana[currentMpBarBack].mpBack >= 1 && currentMpBarBack < mana.Count - 1)
            currentMpBarBack++;

        mana[currentMpBarBack].mpBack += value * Time.deltaTime;
        mana[currentMpBarBack].mpBack = mana[currentMpBarBack].mpBack > 1 ? 1 : mana[currentMpBarBack].mpBack;
        //Debug.Log(currentMpBarBack);
    }

    public void DownMPBack(float value)
    {
        if (mana[0].mpBack <= 0)
            return;

        if (mana[currentMpBarBack].mpBack <= 1 && currentMpBarBack > -1)
            mana[currentMpBarBack].canЕxpendMp = false;

        if (mana[currentMpBarBack].mpBack <= 0 && currentMpBarBack > -1)
            currentMpBarBack--;

        mana[currentMpBarBack].mpBack -= value * Time.deltaTime;
        mana[currentMpBarBack].mpBack = mana[currentMpBarBack].mpBack < 0 ? 0 : mana[currentMpBarBack].mpBack;


    }

    public void AddNewBars()
    {
        mana.Add(new ManaClass());
    }

    public void AddNewBars(float mp, float mpBack, bool canExpendMp)
    {
        mana.Add(new ManaClass()
        {
            mp = mp,
            mpBack = mpBack,
            canЕxpendMp = canExpendMp
        });

       // currentMpBar = mana.Count - 1;
        

    }

    public void RemoveBars(int number)
    {
        Destroy(mana[number].mpBarsBox);
        mana.RemoveAt(number);
       // currentMpBar = number - 1;
       // currentMpBarBack = number - 1;
    }

    public void RemoveAllBars()
    {
        for (int i = mana.Count - 1; i > 0; i--)
        {
            Destroy(mana[i].mpBarsBox);
            mana.RemoveAt(i);
            Debug.Log(i);
        }
            currentMpBar = 0;
            currentMpBarBack = 0;
    }
}
