using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracking : MonoBehaviour
{
    // Start is called before the first frame update
    public UDPReceive udpReceive;
    public GameObject[] leftHandPoints;  // Points for left hand
    public GameObject[] rightHandPoints; // Points for right hand
    
    // Cubo da muovere
    public GameObject movingCube;
    
    // Velocità di movimento del cubo
    public float moveSpeed = 5.0f;
    
    // Per tracciare la posizione precedente delle mani
    private Vector3 previousLeftHandPosition;
    private Vector3 previousRightHandPosition;
    private bool isFirstFrame = true;
    
    // Soglia per determinare quando un movimento è significativo
    public float movementThreshold = 0.1f;
    
    void Start()
    {
        // Se il cubo non è stato assegnato nell'inspector, lo creiamo
        if (movingCube == null)
        {
            movingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            movingCube.transform.position = new Vector3(0, 0, 0);
            movingCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Aggiungiamo un colore al cubo
            Renderer renderer = movingCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        string data = udpReceive.data;

        if (data == null)
        {
            Debug.Log("Data is null");
            return;
        }
        
        if(data.Length > 1)
        {
            data = data.Remove(0, 1);
            data = data.Remove(data.Length-1, 1);
            string[] points = data.Split(',');
            
            if (points.Length < 2) {
                Debug.Log("Not enough data");
                return;
            }
            
            // First value tells us how many hands we have
            int handCount = int.Parse(points[0]);
            
            int currentIndex = 1;
            
            Vector3 leftHandCurrentPos = Vector3.zero;
            Vector3 rightHandCurrentPos = Vector3.zero;
            bool leftHandDetected = false;
            bool rightHandDetected = false;
            
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
                    
                    // Tracciamo la posizione del punto centrale della mano (punto 9)
                    if (i == 9)
                    {
                        if (handType == 0) // Mano sinistra
                        {
                            leftHandCurrentPos = new Vector3(x, y, z);
                            leftHandDetected = true;
                        }
                        else // Mano destra
                        {
                            rightHandCurrentPos = new Vector3(x, y, z);
                            rightHandDetected = true;
                        }
                    }
                    
                    currentIndex += 3; // Move to next point (x,y,z)
                }
            }
            
            // Muovi il cubo in base al movimento delle mani
            if (!isFirstFrame)
            {
                Vector3 movement = Vector3.zero;
                
                // Calcola il movimento per ogni mano rilevata e somma i contributi
                if (leftHandDetected && previousLeftHandPosition != Vector3.zero)
                {
                    float leftHandDeltaX = leftHandCurrentPos.x - previousLeftHandPosition.x;
                    if (Mathf.Abs(leftHandDeltaX) > movementThreshold)
                    {
                        movement.x += leftHandDeltaX;
                    }
                }
                
                if (rightHandDetected && previousRightHandPosition != Vector3.zero)
                {
                    float rightHandDeltaX = rightHandCurrentPos.x - previousRightHandPosition.x;
                    if (Mathf.Abs(rightHandDeltaX) > movementThreshold)
                    {
                        movement.x += rightHandDeltaX;
                    }
                }
                
                // Applica il movimento al cubo
                if (movement != Vector3.zero && movingCube != null)
                {
                    movingCube.transform.Translate(movement * moveSpeed * Time.deltaTime);
                }
            }
            
            // Aggiorna le posizioni precedenti per il prossimo frame
            if (leftHandDetected)
                previousLeftHandPosition = leftHandCurrentPos;
            if (rightHandDetected)
                previousRightHandPosition = rightHandCurrentPos;
            
            isFirstFrame = false;
        }
    }
}