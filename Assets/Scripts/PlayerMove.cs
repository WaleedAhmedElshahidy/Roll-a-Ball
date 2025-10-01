using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour{
    private Rigidbody rb;
    public float speed;
    private int count;
    public Text countText;
    public GameObject countertextobject;
    public Text winText;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winText.text = "";
    }
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(moveHorizontal,0.0f,moveVertical);

        rb.AddForce(movement * speed);

    }
    void OnTriggerEnter(Collider other)
    {
      if(other.gameObject.CompareTag("pickup"))
      {
        other.gameObject.SetActive(false);
        count = count + 1;
        SetCountText();
      }
    }
    void SetCountText()
    {
        Debug.Log(count);
        countText.text = "Counter: " +  count.ToString();
        if(count >= 14)
        {
            countertextobject.SetActive(false);
            winText.text = "You Win!!";
        }
    }
    //Destroy(other.gameObject);
}
