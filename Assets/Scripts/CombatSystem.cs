using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] CommonValues commonValues;
    [SerializeField] private GameObject _columnPrefab;

    private SplineProjector _splineProjector;

    private Vector2 rightStickInput;
    bool canAttack = true;

    [SerializeField] int _maxColumns = 5;
    ColumnController[] _instances;

    int lastGeneratedColumn = 0;

    private void Awake() {
        _splineProjector = GetComponent<SplineProjector>();

        _instances = new ColumnController[_maxColumns];

        for (int i = 0; i < _maxColumns; i++){
            _instances[i] = Instantiate(_columnPrefab, new Vector3(0,-100,0), Quaternion.identity).GetComponentInChildren<ColumnController>();
            SetLayerRecursively(_instances[i].transform.parent.gameObject, LayerMask.NameToLayer("Column_" + (i + 1)));
        }

        commonValues.currentSpline = _splineProjector.spline;
    }


    private void Start() {
        InputManager.Instance.OnRightStick += (input) => rightStickInput = input;

        StartCoroutine(GenerateColumn());
    }

    void SetLayerRecursively(GameObject obj, int newLayer) {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    IEnumerator AttackTimer(){
        canAttack = false;
        yield return new WaitForSeconds(.5f);
        canAttack = true;
    }

    IEnumerator GenerateColumn(){
        bool generated = false;
        while(true){
            yield return new WaitUntil(() => rightStickInput != Vector2.zero && canAttack);

            generated = false;

            for (int i = 0; i < _maxColumns; i++){
                if(!_instances[i].IsDestroyed) continue;

                GenerationLogic(_instances[i]);

                StartCoroutine(AttackTimer());

                lastGeneratedColumn = i % _maxColumns;
                generated = true;
                break;
            }

            if(!generated) {
                lastGeneratedColumn = (lastGeneratedColumn + 1) % _maxColumns;
                _instances[lastGeneratedColumn].Reset();

                GenerationLogic(_instances[lastGeneratedColumn]);
                StartCoroutine(AttackTimer());
                
                generated = true;
            }
        }
    }

    void GenerationLogic(ColumnController column){
        if(rightStickInput.x == 1){
            StartCoroutine(column.GenerateColumn(transform.position - _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, _splineProjector.result.forward), ColumnDirection.Right));
        } else if(rightStickInput.x == -1){
            StartCoroutine(column.GenerateColumn(transform.position + _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, -_splineProjector.result.forward), ColumnDirection.Left));
        } else if(rightStickInput.y == 1){
            StartCoroutine(column.GenerateColumn(transform.position - _splineProjector.result.up / 2, Quaternion.LookRotation(-_splineProjector.result.forward, _splineProjector.result.up), ColumnDirection.Up));
        } else if(rightStickInput.y == -1){
            StartCoroutine(column.GenerateColumn(transform.position + _splineProjector.result.up * 3f, Quaternion.LookRotation(_splineProjector.result.forward, -_splineProjector.result.up), ColumnDirection.Down));
        }
    }
}
