using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class Functions : MonoBehaviour
{

    //UI
    public GameObject main_buttons;
    public GameObject return_button;
    public ParticleSystem particles;
    public Text text_points;
    public Text text_bidcoins;

    //Adding
    public List<Organ> organs_posession;
    public List<MyObject> objects_posession;
    private Organ organ_active;
    private MyObject object_active;
    public Text text_message_error;
    private bool adding;
    private bool targeted;

    //Points
    private int bidcoins;
    private int points;
    private int extra_bonus;
    private int super_extra_bonus;
    
    //Auction
    private List<Organ> organs_to_auction;
    public InputField auction_price;
    public Text text_aucting;
    public Text text_price;
    private bool aucting;
    private bool organ_choiced;

    // Start is called before the first frame update
    void Start()
    {
        organs_posession = new List<Organ>();
        objects_posession = new List<MyObject>();
        adding = false;
        targeted = false;
        bidcoins = 0;
        points = 0;
        extra_bonus = 5;
        super_extra_bonus = 10;

        organs_to_auction = new List<Organ>();
        aucting = false;
        organ_choiced = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((organ_active|| object_active) && adding && !targeted)
        {
            if(organ_active)
                organ_active.gameObject.SetActive(true);
            else
                object_active.gameObject.SetActive(true);
            targeted = true;
            text_message_error.text = "Touch it to add!";
        }
            
        if (adding && organ_active && organ_active.PickInput())
            AddOrgan(organ_active);
        else if (adding && object_active && object_active.PickInput())
            AddObject(object_active);

        if (aucting && !organ_choiced)
        {
            foreach (var organ in organs_to_auction)
            {
                if (organ.PickInput())
                {
                    foreach (var organ_poss in organs_posession)
                    {
                        if(organ.order == organ_poss.order)
                        {
                            organ_active = organ_poss;
                            organ_choiced = true;
                        }
                    }
                }
            }
        }
        else if(aucting && organ_active && !organ_active.gameObject.activeSelf)
        {
            organ_active.gameObject.SetActive(true);
            foreach (var organ in organs_to_auction)
            {
                Destroy(organ.gameObject);
            }
            organs_to_auction.Clear();
            SelectPrice();
        }
            
    }

    #region menu_buttons
    public void ReturnToMainMenu()
    {
        main_buttons.SetActive(true);
        return_button.SetActive(false);

        //Add
        if(adding)
        {
            if (organ_active)
                organ_active.gameObject.SetActive(false);
            if (object_active)
                object_active.gameObject.SetActive(false);
            
            text_message_error.text = "";
            adding = false;
            targeted = false;
        }
        
        //Auction
        if(aucting)
        {
            aucting = false;
            organ_choiced = false;
            foreach (var organ in organs_to_auction)
            {
                Destroy(organ.gameObject);
            }
            organs_to_auction.Clear();
            text_aucting.gameObject.SetActive(false);
            organ_active = null;
        }
    }
    
    public void ActivateTracker()
    {
        main_buttons.SetActive(false);
        return_button.SetActive(true);
        adding = true;
        text_message_error.text = "No target on camera";
    }

    public void AuctionOrgan()
    {
        main_buttons.SetActive(false);
        return_button.SetActive(true);
        aucting = true;
        SelectOrganToAuction();
    }

    public void RefreshFinalScore()
    {

    }
    #endregion

    #region pick_organ
    public void AddOrgan(Organ _organ)
    {
        ThrowParticles();
        RecalculateScore(_organ);
        ReturnToMainMenu();
        DesactiveOrgan();
    }

    public void ActiveOrgan(Organ _organ)
    {
        if(organ_active)
            organ_active.gameObject.SetActive(false);
        organ_active = _organ;
    }

    public void DesactiveOrgan()
    {
        if (organ_active)
            organ_active.gameObject.SetActive(false);

        if(adding)
            text_message_error.text = "No target on camera";
        targeted = false;
        organ_active = null;
    }
    #endregion

    #region pick_object
    public void AddObject(MyObject _object)
    {
        ThrowParticles();
        RecalculateScore(null, _object);
        ReturnToMainMenu();
        DesactiveObject();
    }

    public void ActiveObject(MyObject _object)
    {
        if (object_active)
            object_active.gameObject.SetActive(false);
        object_active = _object;
    }

    public void DesactiveObject()
    {
        if(object_active)
            object_active.gameObject.SetActive(false);

        if (adding)
            text_message_error.text = "No target on camera";
        targeted = false;
        object_active = null;
    }
    #endregion

    #region auction_organ
    void SelectOrganToAuction()
    {
        int count_x = 0;
        int count_y = 0;
        foreach (var organ in organs_posession)
        {
            Organ organ_copia = Instantiate(Resources.Load<Organ>("Models/organs/" + organ.name), organ.transform.parent);
            organ_copia.gameObject.transform.localScale = new Vector3(organ.gameObject.transform.localScale.x / 2, organ.gameObject.transform.localScale.y / 2, organ.gameObject.transform.localScale.z / 2);
            organ_copia.transform.localPosition = new Vector3(-0.5f + (0.5f * count_x), 0.5f - (0.5f * count_y), 4);
            organs_to_auction.Add(organ_copia);
            count_x++;
            if(count_x > 2)
            {
                count_x = 0;
                count_y++;
            }
        }
    }

    void SelectPrice()
    {
        text_aucting.gameObject.SetActive(true);
        text_aucting.text = "Put the price that you consider convenient for the initial bid.";
        auction_price.gameObject.SetActive(true);
        text_price.gameObject.SetActive(true);
        if (organ_active)
        {
            organ_active.gameObject.SetActive(true);
            organ_active.gameObject.transform.localScale = organ_active.gameObject.transform.localScale/2;
        }
    }

    void StartAuction()
    {
        return_button.SetActive(false);
        text_aucting.text = "Auction opens, " + organ_active.gameObject.name + " bid starts on " + auction_price + " bidcoins. \nWho gives " + auction_price + "?";
    }
    #endregion

    #region points
    void RecalculateScore(Organ _organ = null, MyObject _object = null)
    {
        bool have_organ = false;
        int same_organ_count = 0;
        int max_copies = 4;
        
        if (_organ)
        {
            foreach (var organ in organs_posession)
            {
                if (organ == _organ)
                {
                    have_organ = true;
                    same_organ_count++;
                }
            }

            //Add organ
            if (same_organ_count <= max_copies)
                organs_posession.Add(_organ);
            else
                same_organ_count = max_copies;

            //Order
            organs_posession.Sort((x, y) => x.order.CompareTo(y.order));

            //Print
            PrintOrganImage(_organ, same_organ_count, have_organ);
        }
        else if(_object)
        {
            //Add object
            objects_posession.Add(_object);

            //Print
            PrintObjectImage(_object);
        }
        

        //Points
        if (!have_organ)
            points = CountScale();
        points = points + (objects_posession.Count * 3);
        text_points.text = "Score: " + points + "0000";

        RecalculateBidcoins();
    }

    void RecalculateBidcoins()
    {
        bidcoins = (objects_posession.Count * 3);
        text_bidcoins.text = "Money: " + bidcoins + "0000";
    }

    int CountScale()
    {
        int scale_count = 1;
        int aux_scale_count = 1;
        int aux_back_order = organs_posession[0].order;

        foreach (var organ in organs_posession)
        {
            if (organ.order == aux_back_order + 1)
                aux_scale_count++;
            else if (organ.order == aux_back_order)
                continue;
            else
                aux_scale_count = 1;

            if (scale_count < aux_scale_count)
                scale_count = aux_scale_count;

            aux_back_order = organ.order;
        }

        if(scale_count < 7)
            return scale_count * 4;
        else if(scale_count >= 7 && scale_count < 9)
            return (scale_count * 4) + extra_bonus;
        else
            return (scale_count * 4) + super_extra_bonus;
    }
    #endregion

    void PrintOrganImage(Organ _organ, int _same_organ_count, bool _have_organ)
    {
        if (_have_organ)
        {
            Vector3 position = new Vector3(_organ.image.transform.position.x + (110 * _same_organ_count), _organ.image.transform.position.y, _organ.image.transform.position.z);
            GameObject image_copia = Instantiate(Resources.Load<GameObject>("Prefabs/Organs_Images/" + _organ.image.name), position, _organ.image.transform.rotation, _organ.image.transform);
            image_copia.GetComponent<UnityEngine.UI.Image>().color = new Color(_organ.image.color.r, 0.5f, 0.5f, 1f);
        }
        else
        {
            _organ.image.color = new Color(_organ.image.color.r, _organ.image.color.g, _organ.image.color.b, 1f);
        }
    }

    void PrintObjectImage(MyObject _object)
    {
         _object.image.color = new Color(_object.image.color.r, _object.image.color.g, _object.image.color.b, 1f);
    }

    void ThrowParticles()
    {
        if (particles)
            particles.Play();
    }
    

    
}
