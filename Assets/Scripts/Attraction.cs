using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attraction : MonoBehaviour
{
    public GameObject _atom, _attractor;
    public Gradient _gradient;
    public Material _material;
    Material[] _sharedMaterial;
    Color[] _sharedColor;
    public int[] _attractPoints;
    public Vector3 _spacingDirection;
    [Range(0, 20)]
    public float _spacingBetweenAttractPoints;
    [Range(0, 20)]
    public float _scaleAttractPoints;
    GameObject[] _attractorArray, _atomArray;
    [Range(1, 64)]
    public int _amountOfAtomsPerPoint;
    public Vector2 _atomScaleMinMax;
    float[] _atomScaleSet;
    public float _strengthOfAttraction, _maxMagnitude, _randomPosDistance;
    public bool useGravity;

    public float _audioScaleMultiplier, _audioEmissionMultiplier;
    [Range(0.0f,1.0f)]
    public float _thresholdEmission;

    float[] _audioBandEmissionThreshold;
    float[] _audioBandEmissionColor;
    float[] _audioBandScale;
    public enum _emissionThreshold { Buffered, NoBuffer};
    public _emissionThreshold emissionThreshold = new _emissionThreshold();
    public enum _emissionColor { Buffered, NoBuffer };
    public _emissionColor emissionColor = new _emissionColor();
    public enum _atomScale { Buffered, NoBuffer };
    public _atomScale atomScale = new _atomScale();
    private void OnDrawGizmos()
    {
        for (int i = 0; i < _attractPoints.Length; i++)
        {
            float evaluateStep = 0.125f ;
            Color color = _gradient.Evaluate(Mathf.Clamp(evaluateStep * _attractPoints[i],0,7));
            Gizmos.color = color;

            Vector3 pos = new Vector3(transform.position.x + (_spacingBetweenAttractPoints * i * _spacingDirection.x), transform.position.y + (_spacingBetweenAttractPoints * i * _spacingDirection.y), transform.position.z + (_spacingBetweenAttractPoints * i * _spacingDirection.z));
            Gizmos.DrawSphere(pos, _scaleAttractPoints*0.5f);
       

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _attractorArray = new GameObject[_attractPoints.Length];
        _atomArray = new GameObject[_attractPoints.Length * _amountOfAtomsPerPoint];
        _atomScaleSet = new float[_attractPoints.Length * _amountOfAtomsPerPoint];
        
        _audioBandEmissionThreshold= new float[8];
        _audioBandEmissionColor = new float[8];
        _audioBandScale = new float[8];
        _sharedMaterial = new Material[8];
        _sharedColor = new Color[8];
        int _countAtom = 0;
        //instantiate attract points
        for(int i = 0; i < _attractPoints.Length; i++)
        {
            GameObject _attractorInstance = (GameObject)Instantiate(_attractor);
            _attractorArray[i] = _attractorInstance;
            _attractorInstance.transform.position=new Vector3(transform.position.x + (_spacingBetweenAttractPoints * i * _spacingDirection.x),
                transform.position.y + (_spacingBetweenAttractPoints * i * _spacingDirection.y), 
                transform.position.z + (_spacingBetweenAttractPoints * i * _spacingDirection.z));
            _attractorInstance.transform.parent = this.transform;
            _attractorInstance.transform.localScale = new Vector3(_scaleAttractPoints, _scaleAttractPoints, _scaleAttractPoints);

            Material _matInstance = new Material(_material);
            _sharedMaterial[i] = _matInstance;
            _sharedColor[i] = _gradient.Evaluate(0.125f * i);
            //instantiate atoms
            for (int j = 0; j < _amountOfAtomsPerPoint; j++)
            {
                GameObject _atomInstance = (GameObject)Instantiate(_atom);
                _atomArray[_countAtom] = _atomInstance;
                _atomInstance.GetComponent<AttractedTo>().attractedTo = _attractorArray[i].transform;
                _atomInstance.GetComponent<AttractedTo>().strengthOfAttraction = _strengthOfAttraction;
                _atomInstance.GetComponent<AttractedTo>().maxMagnitude = _maxMagnitude;
                if (useGravity)
                { _atomInstance.GetComponent<Rigidbody>().useGravity = true; }
                else{ _atomInstance.GetComponent<Rigidbody>().useGravity = false;
            }
                _atomInstance.transform.position = new Vector3(_attractorArray[i].transform.position.x + Random.Range(-_randomPosDistance, _randomPosDistance),
                    _attractorArray[i].transform.position.y + Random.Range(-_randomPosDistance, _randomPosDistance),
                    _attractorArray[i].transform.position.z + Random.Range(-_randomPosDistance, _randomPosDistance));

                float randomScale = Random.Range(_atomScaleMinMax.x, _atomScaleMinMax.y);
                _atomScaleSet[_countAtom] = randomScale;
                _atomInstance.transform.localScale = new Vector3(_atomScaleSet[_countAtom], _atomScaleSet[_countAtom], _atomScaleSet[_countAtom]);
                _atomInstance.transform.parent = _attractorInstance.transform;
                _atomInstance.GetComponent<MeshRenderer>().material = _sharedMaterial[i];
                _countAtom++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        SelectAudioValues();
        AtomBehavior();
    }
    void AtomBehavior()
    {
        int countAtom = 0;
        for(int i = 0; i < _attractPoints.Length; i++)
        {
            if (_audioBandEmissionThreshold[_attractPoints[i]] >= _thresholdEmission)
            {
                Color _audioColor = new Color(_sharedColor[i].r * _audioBandEmissionColor[_attractPoints[i]] * _audioEmissionMultiplier,
                    _sharedColor[i].b * _audioBandEmissionColor[_attractPoints[i]] * _audioEmissionMultiplier,
                    _sharedColor[i].g * _audioBandEmissionColor[_attractPoints[i]] * _audioEmissionMultiplier);
                _sharedMaterial[i].SetColor("_EmissionColor", _audioColor);
            }
            else
            {
                Color _audioColor = new Color(0, 0, 0, 1);
                _sharedMaterial[i].SetColor("_EmissionColor", _audioColor);
            }
            for(int j = 0; j < _amountOfAtomsPerPoint; j++)
            {
                _atomArray[countAtom].transform.localScale = new Vector3(_atomScaleSet[countAtom] + _audioBandScale[_attractPoints[i]] * _audioScaleMultiplier,
                    _atomScaleSet[countAtom] + _audioBandScale[_attractPoints[i]] * _audioScaleMultiplier,
                    _atomScaleSet[countAtom] + _audioBandScale[_attractPoints[i]] * _audioScaleMultiplier);
                countAtom++;
            }
        }
    }
    void SelectAudioValues()
    {
        //threshold
        if (emissionThreshold == _emissionThreshold.Buffered)
        {
            for(int i=0;i<8;i++)
            _audioBandEmissionThreshold[i] = AudioPeer._audioBandBuffer[i];
        }
        if (emissionThreshold == _emissionThreshold.NoBuffer)
        {
            for (int i = 0; i < 8; i++)
                _audioBandEmissionThreshold[i] = AudioPeer._audioBand[i];
        }
        //emission Color
        if (emissionColor == _emissionColor.Buffered)
        {
            for (int i = 0; i < 8; i++)
                _audioBandEmissionColor[i] = AudioPeer._audioBandBuffer[i];
        }
        if (emissionColor == _emissionColor.NoBuffer)
        {
            for (int i = 0; i < 8; i++)
                _audioBandEmissionColor[i] = AudioPeer._audioBand[i];
        }
        //atom Scale
        if (atomScale == _atomScale.Buffered)
        {
            for (int i = 0; i < 8; i++)
                _audioBandScale[i] = AudioPeer._audioBandBuffer[i];
        }
        if (atomScale == _atomScale.NoBuffer)
        {
            for (int i = 0; i < 8; i++)
                _audioBandScale[i] = AudioPeer._audioBand[i];
        }
    }
}
