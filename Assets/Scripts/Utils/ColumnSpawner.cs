using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Dreamteck.Splines;
using UnityEngine;

public class ColumnSpawner : MonoBehaviour
{
    [SerializeField] CommonValues commonValues;
    [SerializeField] private GameObject _columnPrefab;
    [SerializeField] private float _max_distance = 4.6f;
    [SerializeField] private float valore_aggiunto_raycast = 0.0f;
    [Description("Se nello spazio che occuperà la colonna ci sono questi layer, non generarla.")]
    [SerializeField] private LayerMask _detectionColumnLayer;

    private SplineProjector _splineProjector;
    bool canEvokeColumn = true;
    private Color _debugColor = Color.green;
    public Vector3 colliderDimension = new Vector3(2f, 2f, 2f);
    private Vector3 _columnProjection = Vector3.zero;
    private Vector2 rightStickInput;
    private bool canAttack = true;
    private Vector3 personalForward = Vector3.zero;
    // Numero massimo di colonne
    private int _maxColumns = 2;
    // Distanza DAL GIOCATORE alla quale vengono generate le colonne
    private float _max_columns_distance = 3;
    private ColumnController2[] _instances;
    int lastGeneratedColumn = 0;

    private void Awake()
    {
        _splineProjector = GetComponent<SplineProjector>();

        _instances = new ColumnController2[_maxColumns];

        for (int i = 0; i < _maxColumns; i++)
        {
            _instances[i] = Instantiate(_columnPrefab, new Vector3(0, -100, 0), Quaternion.identity).GetComponent<ColumnController2>();
            SetLayerRecursively(_instances[i].transform.gameObject, LayerMask.NameToLayer("Column_" + (i + 1)));
        }

        commonValues.currentSpline = _splineProjector.spline;
    }

    private void Start()
    {
        InputManager.Instance.OnRightStick += (input) => rightStickInput = input;
        StartCoroutine(SpawnerColumn());
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // Spawner Column
    IEnumerator SpawnerColumn()
    {
        bool generated = false;

        // Come funziona esattamente? Come si riesce a prendere l'input?
        while (true)
        {
            // Statti ferma, SOSPENDITI fino a quando non viene premuto l'analogico destro
            yield return new WaitUntil(() => rightStickInput != Vector2.zero && canAttack);

            //La possibilità di attaccare è sospesa finché non è finita la coroutine di generazione
            generated = false;

            for (int i = 0; i < _maxColumns; i++)
            {
                if (!_instances[i].IsDestroyed) continue;

                GenerationLogic(_instances[i]);

                yield return new WaitForSeconds(.5f);

                lastGeneratedColumn = i;
                generated = true;
                break;
            }


            // Entro qua dentro quando devo riciclare le colonne.
            // Se però non posso generarle, non ha senso farle sparire. E' quello che accade.

            //Infatti viene chiamato il reset.

            // Si potrebbe vedere cosa fa generationLogic e in caso uscire o fare continue, ma evitare di far chiamare "RESET".
            if (!generated)
            {
                lastGeneratedColumn = (lastGeneratedColumn + 1) % _maxColumns;

                GenerationLogic(_instances[lastGeneratedColumn], lastGeneratedColumn);
                yield return new WaitForSeconds(.5f);

                generated = true;
            }
        }
    }

    private Color ChangeColor(bool value)
    {
        if (value)
            return _debugColor = Color.green;

        return _debugColor = Color.red;
    }

    private bool CanEvokeColumn(Vector3 position, Vector3 personalForward, out Vector3 groundVector)
    {
        RaycastHit hit;

        Vector3 underTerrainPos = transform.position + (personalForward * _max_columns_distance) + new Vector3(0, valore_aggiunto_raycast, 0);

        // I muri non devono mai essere ground, ma "obstacles"
        if (Physics.Raycast(underTerrainPos, Vector3.down, out hit, _max_distance, LayerMask.GetMask("Ground")))
        {
            _columnProjection = hit.point;
            //Dal terreno, casto un box per vedere se è possibile costruirlo
            Collider[] hitColliders = Physics.OverlapBox(hit.point + new Vector3(0, colliderDimension.y / 2, 0) + new Vector3(0, 0.20f, 0), colliderDimension / 2f, Quaternion.identity, _detectionColumnLayer);

            // Il controllo ora funziona.
            //In futuro, quando verranno introdotti altri layer, bisogna aggiungerli qui ed escluderli.

            // Debug per vedere che cosa impedisce la generazione della colonna.
            if (hitColliders.Length > 0)
            {
                List<string> nomiOggetti = new List<string>();
                foreach (var hitCollider in hitColliders)
                {
                    nomiOggetti.Add(hitCollider.gameObject.name);
                }

                string logFinale = "Oggetti trovati nell'area: " + string.Join(", ", nomiOggetti);
                Debug.Log(logFinale);
                groundVector = Vector3.zero;
                return false;
            }


            float groundLevelY = hit.point.y - 4.90f;
            groundVector = new Vector3(underTerrainPos.x, groundLevelY, underTerrainPos.z);
            canEvokeColumn = true;
            return canEvokeColumn;
        }


        groundVector = Vector3.zero;
        canEvokeColumn = false;
        return canEvokeColumn;
    }

    private void OnDrawGizmos()
    {
        //Duplicazione di codice necessaria per il debug
        Vector3 underTerrainPos = transform.position + (personalForward * _max_columns_distance) + new Vector3(0, valore_aggiunto_raycast, 0);
        Gizmos.color = _debugColor;
        Gizmos.DrawWireCube(_columnProjection + new Vector3(0, colliderDimension.y / 2, 0) + new Vector3(0, 0.01f, 0), colliderDimension);  // Mostra la zona come un cubo wireframe
    }


    void Update()
    {
        //Necessario solo per il debug, altrimenti inutile e duplicata
        Vector3 temp;
        ChangeColor(CanEvokeColumn(transform.position, personalForward, out temp));
        personalForward = _splineProjector.direction == Spline.Direction.Forward ? _splineProjector.result.forward : -_splineProjector.result.forward;
        Vector3 startTerrainPos = transform.position + (personalForward * _max_columns_distance);
        Vector3 endTerrainPos = new Vector3(startTerrainPos.x, startTerrainPos.y - _max_distance, startTerrainPos.z);

        Vector3 underTerrainPos = transform.position + (personalForward * _max_columns_distance) + new Vector3(0, valore_aggiunto_raycast, 0);
        Debug.DrawRay(underTerrainPos, Vector3.down * _max_distance, _debugColor);
    }



    void GenerationLogic(ColumnController2 column, int lastGeneratedColumn = -1)
    {

        personalForward = _splineProjector.direction == Spline.Direction.Forward ? _splineProjector.result.forward : -_splineProjector.result.forward;

        //Colonne dal basso verso l'alto
        /* if (rightStickInput.x == 1 || rightStickInput.x == -1)
        {
            StartCoroutine(column.GenerateColumn(transform.position - _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, _splineProjector.result.forward), ColumnDirection.Right));
        } */
        if (rightStickInput.y == 1 || rightStickInput.y == -1)
        {
            Vector3 groundVector;
            if (CanEvokeColumn(transform.position, personalForward, out groundVector) && PlayerMovement.Instance.IsGrounded)
            {
                Debug.Log("L'oggetto vererà creato a " + groundVector);
                if (lastGeneratedColumn != -1)
                    _instances[lastGeneratedColumn].Reset();

                canAttack = false;
                StartCoroutine(column.GenerateColumn(groundVector, Quaternion.LookRotation(personalForward, _splineProjector.result.up), test));
            }
        }
    }

    // Serve per scandire ogni quanto posso generare colonne, DA RINONIMARE
    private void test()
    {
        canAttack = true;
    }
}
