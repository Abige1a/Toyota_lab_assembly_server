using System.Collections;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ObjectRayActivate : MonoBehaviour
{

    public Transform rayOrigin;
    public RequestSendeManager requestmanager;
    private List<GameObject> balls = new List<GameObject>();
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (requestmanager != null)
        {
            if (requestmanager.spheres.Count != 0)
            {
                balls=requestmanager.spheres;
                CalculateDistances();
            }
        }

        
    }

    private void CalculateDistances()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        foreach (var ball in balls)
        {
            Vector3 ballPosition = ball.transform.position;
            float distance = Vector3.Cross(ray.direction, ballPosition - ray.origin).magnitude;
            string score_text="";
            if (distance <= 0.2)
            {
                text.color = Color.white;
                score_text = "Good job. You did it";
            }
            else if (distance > 0.2 && distance<=0.5)
            {
                text.color = Color.white;
                double score = 0.5 / distance * 100;
                score_text = $"Look at the arrow. Distance: {distance * 100:F1} cm";

            }
            else if (distance > 0.5)
            {
                text.color = Color.red;
                score_text = $"Look at the arrow. Distance: {distance * 100:F1} cm";
            }
            text.text = score_text;
            
            Debug.Log($"Distance from ray to {ball.name}: {distance}");
        }
    }
}
