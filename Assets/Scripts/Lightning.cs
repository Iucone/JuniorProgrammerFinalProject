using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class Lightning : MonoBehaviour
{
    public ParticleSystem lightingPrefab;
    public ParticleSystem explosionPrefab;
    public AudioClip[] lightningAudioClips;
    public AudioClip[] rumbleAudioClips;
    public Light envLight;
    public float minTimeBetweenFlashes = 8f;
    public Vector2 flashesDuration = new Vector2(1f, 5f);

    private float curFlashDuration;
    private float extraFlashesDelay = 0f;
    private float lastFlashesTime = 0f;
    private float flashesEventTimeElapsed = 0f;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastFlashesTime = Time.time;
        extraFlashesDelay = Random.Range(0f, 15f);
        curFlashDuration = Random.Range(flashesDuration[0], flashesDuration[1]);

        // Ottiene o aggiunge un AudioSource al GameObject
        audioSource = GetComponent<AudioSource>();
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            audioSource.PlayOneShot(lightningAudioClips[Random.Range(0, lightningAudioClips.Length)]);
            
            //StartCoroutine(nameof(SimulateFlashes));
            StartCoroutine(nameof(SimulateLightingStrike));
        }

        if (Time.time - lastFlashesTime >= minTimeBetweenFlashes + extraFlashesDelay)
        {
            flashesEventTimeElapsed += Time.deltaTime;
            if (flashesEventTimeElapsed > curFlashDuration)
            {
                audioSource.PlayOneShot(rumbleAudioClips[Random.Range(0, rumbleAudioClips.Length)]);
                flashesEventTimeElapsed = 0f;
                lastFlashesTime = Time.time;
                extraFlashesDelay = Random.Range(0f, 15f);
                curFlashDuration = Random.Range(flashesDuration[0], flashesDuration[1]);
            }
            else
            {
                envLight.intensity += (Mathf.PerlinNoise1D(Time.time * 2.5f) - 0.5f) * 0.35f;
                envLight.intensity = Mathf.Clamp(envLight.intensity, 1.24f, 6f);
            }

        }
        else envLight.intensity = Mathf.Lerp(envLight.intensity, 1.24f, 0.5f);

    }


    IEnumerator SimulateLightingStrike()
    {
        int numStrikes = 15;

        Vector3 position = new Vector3(Random.Range(20f, 50f), 16f, Random.Range(20f, 50f));

        if (Physics.Raycast(position, -Vector3.up, out RaycastHit hit, 100f))
        {
            ParticleSystem exp = Instantiate(explosionPrefab, hit.point, Quaternion.identity);
            exp.Play();
        }



        while (numStrikes >= 0)
        {
            uint seed = (uint)(Random.value * 1000000);
            print("seed " + seed);
            List<ParticleSystem> lightnings = new List<ParticleSystem>();
            for (int i = 0; i < 5; i++)
            {
                ParticleSystem lightning = Instantiate(lightingPrefab,
                    position, lightingPrefab.transform.rotation);
                var main = lightning.main;
                lightning.useAutoRandomSeed = false;
                lightning.randomSeed = seed;
                ParticleSystem.EmissionModule emission = lightning.emission;
                //emission.rateOverTime = Random.Range(0.0465f, 0.0535f); 
                emission.rateOverTime = Random.Range(0.048f+0.025f, 0.0520f + 0.025f);
                ParticleSystem.ShapeModule shape = lightning.shape;
                shape.radius = Random.Range(0.1f, 0.65f);
                lightning.Play();
                lightnings.Add(lightning);
                //yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(0.035f);
            //lighting.Stop();
            //Destroy(lighting );
            for (int i = 0; i < lightnings.Count; i++)
                Destroy(lightnings[i].gameObject);
            lightnings.Clear();
            numStrikes--;
        }
    }



    IEnumerator SimulateFlashes()
    {
        int numOfFlashs = 5;

        while (numOfFlashs >= 0)
        {
            envLight.intensity += Time.deltaTime * Mathf.PerlinNoise1D(Time.deltaTime);

            yield return new WaitForSeconds(Random.Range(0.04f, 0.08f));
        }
    }
}
