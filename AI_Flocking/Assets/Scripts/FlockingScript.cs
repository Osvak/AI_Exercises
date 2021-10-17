using System.Collections;
using UnityEngine;

public class FlockingScript : MonoBehaviour // MonoBehaviour adds coroutines, used to 
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Creates the UI inside Unity

    [Header("General Settings")]
    public bool debug;
    public Material debugMaterial;

    [Header("Flock Settings")]
    [Range(1, 150)] public int flockNum = 50;
    [Range(0, 5000)] public int fragmentedFlock = 100;
    [Range(0, 1)] public float fragmentedFlockYLimit = 0.43f;
    [Range(0, 1.0f)] public float migrationFrequency = 0.254f;
    [Range(0, 1.0f)] public float posChangeFrequency = 0.362f;
    [Range(0, 100)] public float smoothChFrequency = 3;

    [Header("Butterfly Settings")]
    public GameObject butterflyPrefab;
    [Range(1, 9999)] public int butterflyNum = 3000;
    [Range(0, 150)] public float butterflySpeed = 5;
    [Range(0, 100)] public int fragmentedButterflies = 50;
    [Range(0, 1)] public float fragmentedButterfliesYLimit = 1;
    [Range(0, 10)] public float turnSharpness = 5f;
    public Vector2 scaleRandom = new Vector2(1.0f, 1.5f);
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Variables

    Vector2 behaviorRand = new Vector2(0.0f, 25.0f);
    Transform thisTransform;
    Transform[] butterfliesTransform, flocksTransform;
    Vector3[] rdTargetPos, flockPos, velFlocks;
    float[] butterfliesSpeed, butterfliesSpeedCur, spVelocity;
    float rotationClampValue = 50;
    int[] curentFlock;
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Called at the beginning
    void Awake()
    {
        thisTransform = transform;
        CreateFlock();
        CreateButterfly();
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Called every loop iteration
    void Update()
    {
        BehavioralChange();
        FlocksMove();
        ButterfliesMove();
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Controls the movement of each flock Sphere
    void FlocksMove()
    {
        for (int f = 0; f < flockNum; f++)
        {
            flocksTransform[f].localPosition = Vector3.SmoothDamp(flocksTransform[f].localPosition, flockPos[f], ref velFlocks[f], smoothChFrequency);
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Controls the movement of each Butterfly
    void ButterfliesMove()
    {
        float dt = Time.deltaTime;
        Vector3 translateCur = Vector3.forward * butterflySpeed * dt;
        float soaringCur = turnSharpness * dt;

        for (int b = 0; b < butterflyNum; b++)
        {
            if (butterfliesSpeedCur[b] != butterfliesSpeed[b]) butterfliesSpeedCur[b] = Mathf.SmoothDamp(butterfliesSpeedCur[b], butterfliesSpeed[b], ref spVelocity[b], 0.5f);
            butterfliesTransform[b].Translate(translateCur * butterfliesSpeed[b]);
            Vector3 tpCh = flocksTransform[curentFlock[b]].position + rdTargetPos[b] + Vector3.up - butterfliesTransform[b].position;
            Quaternion rotationCur = Quaternion.LookRotation(Vector3.RotateTowards(butterfliesTransform[b].forward, tpCh, soaringCur, 0));
            butterfliesTransform[b].localRotation = BirdsRotationClamp(rotationCur, rotationClampValue);
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Controls the change in behavior of the flocks and of the butterflies inside the flocks
    void BehavioralChange()
    {
        float rand = Random.Range(behaviorRand.x, behaviorRand.y);

        if (rand < 0.1f)
        {
            // Flocks
            for (int f = 0; f < flockNum; f++)
            {
                if (Random.value < posChangeFrequency)
                {
                    Vector3 rdvf = Random.insideUnitSphere * fragmentedFlock;
                    flockPos[f] = new Vector3(rdvf.x, Mathf.Abs(rdvf.y * fragmentedFlockYLimit), rdvf.z);
                }
            }

            // Butterflies
            for (int b = 0; b < butterflyNum; b++)
            {
                butterfliesSpeed[b] = Random.Range(3.0f, 7.0f);
                Vector3 lpv = Random.insideUnitSphere * fragmentedButterflies;
                rdTargetPos[b] = new Vector3(lpv.x, lpv.y * fragmentedButterfliesYLimit, lpv.z);
                if (Random.value < migrationFrequency) curentFlock[b] = Random.Range(0, flockNum);
            } 
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Creates an amount of Spheres that act as a marker for the butterflies to follow equal to flockNum
    void CreateFlock()
    {
        flocksTransform = new Transform[flockNum];
        flockPos = new Vector3[flockNum];
        velFlocks = new Vector3[flockNum];
        curentFlock = new int[butterflyNum];

        for (int f = 0; f < flockNum; f++)
        {
            GameObject nobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nobj.GetComponent<Renderer>().material = debugMaterial;
            nobj.SetActive(debug);
            flocksTransform[f] = nobj.transform;
            Vector3 rdvf = Random.onUnitSphere * fragmentedFlock;
            flocksTransform[f].position = thisTransform.position;
            flockPos[f] = new Vector3(rdvf.x, Mathf.Abs(rdvf.y * fragmentedFlockYLimit), rdvf.z);
            flocksTransform[f].parent = thisTransform;
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Creates an amount of butterflies equal to butterflyNum
    void CreateButterfly()
    {
        butterfliesTransform = new Transform[butterflyNum];
        butterfliesSpeed = new float[butterflyNum];
        butterfliesSpeedCur = new float[butterflyNum];
        rdTargetPos = new Vector3[butterflyNum];
        spVelocity = new float[butterflyNum];

        for (int b = 0; b < butterflyNum; b++)
        {
            butterfliesTransform[b] = Instantiate(butterflyPrefab, thisTransform).transform;
            Vector3 lpv = Random.insideUnitSphere * fragmentedButterflies;
            butterfliesTransform[b].localPosition = rdTargetPos[b] = new Vector3(lpv.x, lpv.y * fragmentedButterfliesYLimit, lpv.z);
            butterfliesTransform[b].localScale = Vector3.one * Random.Range(scaleRandom.x, scaleRandom.y);
            butterfliesTransform[b].localRotation = Quaternion.Euler(0, Random.value * 360, 0);
            curentFlock[b] = Random.Range(0, flockNum);
            butterfliesSpeed[b] = Random.Range(3.0f, 7.0f);
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Controls the rotation of the butterfly
    static Quaternion BirdsRotationClamp(Quaternion rotationCur, float rotationClampValue)
    {
        Vector3 angleClamp = rotationCur.eulerAngles;
        rotationCur.eulerAngles = new Vector3(Mathf.Clamp((angleClamp.x > 180) ? angleClamp.x - 360 : angleClamp.x, -rotationClampValue, rotationClampValue), angleClamp.y, 0);
        return rotationCur;
    }
    //-----------------------------------------------------------------------------------------------------------------------------
}