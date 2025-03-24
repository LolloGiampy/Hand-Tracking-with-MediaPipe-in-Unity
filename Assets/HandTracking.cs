using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracking : MonoBehaviour
{
    // Start is called before the first frame update
    public UDPReceive udpReceive;
    public GameObject[] leftHandPoints;  // Points for left hand
    public GameObject[] rightHandPoints; // Points for right hand
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string data = udpReceive.data;

        if (data == null)
        {
            Debug.Log("Data is null");
        }
        
        if(data.Length > 1)
        {
            data = data.Remove(0, 1);
            data = data.Remove(data.Length-1, 1);
            string[] points = data.Split(',');
            
            if (points.Length < 2) {
                Debug.Log("Not enough data");
            }; // Not enough data
            
            // First value tells us how many hands we have
            int handCount = int.Parse(points[0]);
            
            int currentIndex = 1;
            
            // Process each hand
            for (int hand = 0; hand < handCount && hand < 2; hand++)
            {
                // Get hand type (0 for left, 1 for right)
                int handType = int.Parse(points[currentIndex]);
                currentIndex++;
                
                GameObject[] targetHandPoints = (handType == 1) ? rightHandPoints : leftHandPoints;
                
                // Process 21 landmarks for this hand
                for (int i = 0; i < 21; i++)
                {
                    if (currentIndex + 2 >= points.Length) break; // Safety check
                    
                    float x = 7 - float.Parse(points[currentIndex]) / 100;
                    float y = float.Parse(points[currentIndex + 1]) / 100;
                    float z = float.Parse(points[currentIndex + 2]) / 100;
                    
                    targetHandPoints[i].transform.localPosition = new Vector3(x, y, z);
                    
                    currentIndex += 3; // Move to next point (x,y,z)
                }
            }
        }
    }
}