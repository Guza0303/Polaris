﻿using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TouchManager : MonoBehaviour {
    
    GameObject Scope = null;

    private float skyRadius = 4.6f;
    private float touchBound = 2.5f;
    private float scopeRadius = 1.5f;

    private bool touchOn = false;
    private Touch tempTouchs;
    private int divideCount = 20;

    public static bool moveAble = true;
    
    Dictionary<string, float> Constellation = new Dictionary<string, float>();
    Dictionary<string, string> Character = new Dictionary<string, string>(); // 캐릭터이름, 별자리이름
    public static Dictionary<string, float> charProb = new Dictionary<string, float>();

    // Use this for initialization
    void Start () {
        Scope = GameObject.Find("Scope");

        Constellation.Add("Draco", 0f);
        Constellation.Add("UrsaMinor", 0f);
        Constellation.Add("Lyra", 0f);
        Constellation.Add("Sagittarius", 0f);
        Constellation.Add("Aquarius", 0f);
        Constellation.Add("Eridanus", 0f);
        Constellation.Add("CanisMajor", 0f);

        Character.Add("acher", "Eridanus");
        Character.Add("catseye", "Draco");
        Character.Add("melik", "Aquarius");
        Character.Add("pluto", "Sagittarius");
        Character.Add("polaris", "UrsaMinor");
        Character.Add("sirius", "CanisMajor");
        Character.Add("thuban", "Draco");
        Character.Add("vega", "Lyra");

        Variables.Constels = new Dictionary<string, ConstelData>();

        var raw = Resources.Load<TextAsset>("Data/Characters");
        var charGroup = JsonMapper.ToObject<CharacterDataGroup>(raw.text);
        var constelRaw = Resources.Load<TextAsset>("Data/Constels");
        var constelGroup = JsonMapper.ToObject(constelRaw.text);

        Variables.Characters = new Dictionary<int, CharacterData>();
        foreach (CharacterDataCore data in charGroup.Characters)
        {
            Variables.Characters.Add(data.CharNumber, data);
        }

        Variables.Constels = new Dictionary<string, ConstelData>();
        foreach (JsonData data in constelGroup["constels"])
        {
            var newConstel = new ConstelData((string)data["key"], (string)data["name"]);
            Variables.Constels.Add(newConstel.InternalName, newConstel);
        }

        shotRay();
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if(moveAble)
            ScopeMove();
    }


    public void ScopeMove()
    {
        Vector2 center = new Vector2(0f, 3.78f);
        
        touchOn = false;
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                tempTouchs = Input.GetTouch(i);
                if (tempTouchs.phase == TouchPhase.Began || tempTouchs.phase == TouchPhase.Moved)
                {
                    if(Vector3.Distance(tempTouchs.position, center) < skyRadius)
                    {
                        if (Vector3.Distance(tempTouchs.position, center) >= touchBound)
                        {
                            Scope.transform.position = center + touchBound * ((tempTouchs.position - center / Vector3.Distance(tempTouchs.position, center)));
                        }
                        else
                        {
                            Scope.transform.position = tempTouchs.position;
                        }
                    }

                    touchOn = true;
                    shotRay();
                    break;
                }
            }
        }

        // Click for Test
        Vector3 centerT = new Vector3(0f, 3.78f, -1f);

        if(Input.GetMouseButton(0))
        {
            Vector3 mos = (Input.mousePosition / 100f) + new Vector3(-5.4f, -9.6f, -1f);
            
            if (Vector3.Distance(mos, centerT) < skyRadius)
            {
                if (Vector3.Distance(mos, centerT) >= touchBound)
                {
                    Scope.transform.localPosition = centerT + touchBound * ((mos - centerT) / Vector3.Distance(mos, centerT));
                }
                else
                {
                    Scope.transform.localPosition = mos;
                }
            }
            shotRay();
        }
    }

    public void shotRay()
    {
        charProb.Clear();

        Constellation["Draco"] = 0f;
        Constellation["UrsaMinor"] = 0f;
        Constellation["Lyra"] = 0f;
        Constellation["Sagittarius"] = 0f;
        Constellation["Aquarius"] = 0f;
        Constellation["Eridanus"] = 0f;
        Constellation["CanisMajor"] = 0f;

        Vector3 pos = Scope.transform.position;

        CastRay(pos);
        CastRay(pos);
        CastRay(pos);
        CastRay(pos);

        for (float i = 1; i <= divideCount; i++)
        {
            for (float j = 0; j < i * 18f; j++)
            {
                float r = scopeRadius * 1f / divideCount * i;
                float theta = 2f * Mathf.PI * (j / (18f * i)) + (2f * Mathf.PI / 18 / divideCount * (i - 1));
                pos = Scope.transform.position + new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0);
                CastRay(pos);
            }
        }

        foreach (var key in Character.Keys)
        {
            if(key == "catseye")
                charProb.Add(key, Constellation[Character[key]] * 0.4f);
            else
                charProb.Add(key, Constellation[Character[key]]);
        }

        var Char_desc = charProb.OrderByDescending(p => p.Value);
        GameObject CharSprite = null;
        string nowConstellName = CastRayCenter(Scope.transform.position);

        GameObject ConstellSprite = GameObject.Find("Constell_Sprite");
        ConstellSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Constellations/Observation/" + nowConstellName);
        GameObject nowConstell = GameObject.Find("Now_Constell");
        if(nowConstellName != "null")
            nowConstell.GetComponent<TextMesh>().text = Variables.Constels[nowConstellName].Name;
        else
            nowConstell.GetComponent<TextMesh>().text = "-";

        int charIndex = 0;
        //int cardIndex = 0;

        for (int i = 1; i <= 4; i++)
        {
            KeyValuePair<string, float> charRank = Char_desc.ElementAt(i-1);
            
            charIndex = 0;
            foreach(var value in Variables.Characters.Values)
            {
                if (charRank.Key == value.InternalName)
                    charIndex = value.CharNumber;
            }

            var rankCharacter = Variables.Characters[charIndex];

            int favority = rankCharacter.Cards[0].Favority;
            int cnt = 0, clrdFavority = 0;

            CharSprite = GameObject.Find("Character_" + i.ToString());
            GameObject heartBarUI = GameObject.Find("HeartBarUI_" + i.ToString());
            GameObject heart = heartBarUI.transform.FindChild("Heart_" + i.ToString()).gameObject;
            GameObject heartBar = heartBarUI.transform.FindChild("HeartBar_" + i.ToString()).gameObject;
            GameObject nowFav = heartBar.transform.FindChild("Now_" + i.ToString()).gameObject;
            GameObject totalFav = heartBar.transform.FindChild("Total_" + i.ToString()).gameObject;
            GameObject favLevel = heart.transform.FindChild("Fav_Level_" + i.ToString()).gameObject;
            GameObject charName = GameObject.Find("Name_" + i.ToString());

            if (favority != 0)
            //if (favority == 0)
            {
                heart.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gacha/obs_heart");
                heartBarUI.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gacha/obs_heartbarbackground");
                favLevel.SetActive(true);
                heartBar.SetActive(true);
                for (; cnt < Variables.FavorityThreshold.Length; cnt++)
                {
                    if (favority < Variables.FavorityThreshold[cnt])
                        break;
                    clrdFavority += Variables.FavorityThreshold[cnt];
                }

                CharSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Characters/" + charRank.Key + "/default/image_obs");
                nowFav.GetComponent<TextMesh>().text = (favority - clrdFavority).ToString();
                totalFav.GetComponent<TextMesh>().text = Variables.FavorityThreshold[cnt].ToString();
                favLevel.GetComponent<TextMesh>().text = cnt.ToString();
                charName.GetComponent<TextMesh>().text = rankCharacter.Name;
            }
            else
            {
                heart.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gacha/obs_heartgray");
                heartBarUI.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gacha/obs_heartgraybar");
                favLevel.SetActive(false);
                heartBar.SetActive(false);
                CharSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("obs_character_unknown");
                charName.GetComponent<TextMesh>().text = "???";
            }
        }
    }

    void CastRay(Vector3 pos)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

        if(hit.collider != null)
        {
            if (Constellation.ContainsKey(hit.collider.name))
                Constellation[hit.collider.name]++;
        }
    }

    string CastRayCenter(Vector3 pos)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

        if (hit.collider == null)
            return "null";
        else
            return hit.collider.name;
    }
}
